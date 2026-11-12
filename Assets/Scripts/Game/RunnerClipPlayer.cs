using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GauntletRun.Game
{
    /// <summary>
    /// Plays imported humanoid clips without depending on the MINIMO demo controller.
    /// The package's sample controller is intentionally not assumed to contain states.
    /// </summary>
    public sealed class RunnerClipPlayer
    {
        const float CrossFadeSeconds = 0.12f;

        readonly Animator animator;
        readonly PlayableGraph graph;
        readonly AnimationMixerPlayable mixer;
        readonly AnimationClipPlayable[] clips = new AnimationClipPlayable[2];

        int currentPort;
        int fromPort;
        int toPort;
        AnimationClip activeClip;
        float fadeElapsed;
        bool hasCurrent;
        bool fading;

        public RunnerClipPlayer(Animator target)
        {
            animator = target;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            graph = PlayableGraph.Create("Corre, Zé! Runner");
            graph.SetTimeUpdateMode(DirectorUpdateMode.UnscaledGameTime);
            mixer = AnimationMixerPlayable.Create(graph, 2, true);
            AnimationPlayableOutput output = AnimationPlayableOutput.Create(graph, "Runner", animator);
            output.SetSourcePlayable(mixer);
            graph.Play();
        }

        public void Play(AnimationClip clip, bool loop)
        {
            if (clip == null || !graph.IsValid() || activeClip == clip)
            {
                return;
            }

            int port = hasCurrent ? 1 - currentPort : 0;
            if (clips[port].IsValid())
            {
                graph.Disconnect(mixer, port);
                graph.DestroyPlayable(clips[port]);
            }

            AnimationClipPlayable playable = AnimationClipPlayable.Create(graph, clip);
            playable.SetApplyFootIK(true);
            playable.SetApplyPlayableIK(true);
            playable.SetOverrideLoopTime(true);
            playable.SetLoopTime(loop);
            clips[port] = playable;
            graph.Connect(playable, 0, mixer, port);
            mixer.SetInputWeight(port, hasCurrent ? 0f : 1f);

            activeClip = clip;
            if (!hasCurrent)
            {
                currentPort = port;
                hasCurrent = true;
                return;
            }

            fromPort = currentPort;
            toPort = port;
            fadeElapsed = 0f;
            fading = true;
        }

        public void Tick(float dt)
        {
            if (!fading || !graph.IsValid())
            {
                return;
            }

            fadeElapsed += Mathf.Max(0f, dt);
            float weight = Mathf.Clamp01(fadeElapsed / CrossFadeSeconds);
            mixer.SetInputWeight(fromPort, 1f - weight);
            mixer.SetInputWeight(toPort, weight);

            if (weight < 1f)
            {
                return;
            }

            graph.Disconnect(mixer, fromPort);
            if (clips[fromPort].IsValid())
            {
                graph.DestroyPlayable(clips[fromPort]);
            }
            mixer.SetInputWeight(fromPort, 0f);
            currentPort = toPort;
            fading = false;
        }

        public void Dispose()
        {
            if (graph.IsValid())
            {
                graph.Destroy();
            }
        }
    }
}
