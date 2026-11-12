using GauntletRun.Core;
using UnityEngine;
using Screen = GauntletRun.Core.Screen;

namespace GauntletRun.Game
{
    /// <summary>
    /// Wires the core session to the scene: reads input, advances the run, keeps the views in
    /// step with it and turns run events into sound, particles and camera shake. While no run is
    /// being played, the bot runs the gauntlet behind the title menu.
    /// </summary>
    public sealed class GameDirector : MonoBehaviour
    {
        [SerializeField] GameArt art;
        [SerializeField] Hud hud;
        [SerializeField] CameraRig cameraRig;

        GameInput input;
        AudioDirector sound;
        Transform worldRoot;
        Run attract;
        int attractSeed = 1000;
        float attractAccumulator;
        Screen shownScreen = (Screen)(-1);
        Run shownRun;

        public Session Session { get; private set; }
        public Hud Hud => hud;
        public CameraRig CameraRig => cameraRig;
        public WorldView World { get; private set; }
        public KnightView Knight { get; private set; }
        public AudioDirector Sound => sound;

        /// <summary>When true the director ignores the keyboard, gamepad and touch (tests, screenshots).</summary>
        public bool ScriptedInput { get; set; }

        /// <summary>When true the bot plays the current run (screenshots and soak runs).</summary>
        public bool Autopilot { get; set; }

        /// <summary>When true nothing advances (screenshots hold a moment still).</summary>
        public bool Frozen { get; set; }

        /// <summary>Shows the current screen again after the HUD was rebuilt.</summary>
        public void RebindHud() => shownScreen = (Screen)(-1);

        /// <summary>The run on screen: the player's, or the bot's behind the title.</summary>
        public Run Shown => Session.Run ?? attract;

        void Awake()
        {
            Shapes.Init(art);
            Models.Init(art);
            input = new GameInput();
            sound = gameObject.AddComponent<AudioDirector>();
            sound.Init();
            worldRoot = new GameObject("Gauntlet").transform;
            World = new WorldView(worldRoot);
            Knight = new KnightView(worldRoot, Models.Glow(new Color(0.35f, 0.65f, 1f, 0.28f)));
            hud.Init();
            hud.PlayClicked += () => { Click(); Play(); };
            hud.ShopClicked += () => { Click(); OpenShop(); };
            hud.SettingsClicked += () => { Click(); Session.OpenSettings(); };
            hud.PauseClicked += () => { Click(); TogglePause(); };
            hud.ResumeClicked += () => { Click(); TogglePause(); };
            hud.RestartClicked += () => { Click(); Restart(); };
            hud.MenuClicked += () => { Click(); ToTitle(); };
            hud.BackClicked += Back;
            hud.SkinClicked += PickSkin;
            hud.MusicStep += step => { Session.SetMusic(Session.Profile.Music + step); Click(); };
            hud.EffectsStep += step => { Session.SetEffects(Session.Profile.Effects + step); Click(); };
            UseStore(new PlayerPrefsProfileStore());
        }

        /// <summary>Swaps the saved profile for one in memory (tests and staged screenshots).</summary>
        public void UseMemoryStore(Profile start = null) => UseStore(new MemoryProfileStore(start));

        void UseStore(IProfileStore store)
        {
            Session = new Session(store);
            shownScreen = (Screen)(-1);
            NewAttract();
        }

        // ---- Commands (buttons, keys and tests all come through here) ----

        public void Play() => Session.StartRun(Random.Range(1, int.MaxValue / 2));

        /// <summary>Starts a run on a given road, optionally at a fixed speed.</summary>
        public void StartTrial(ITrackSource track, float? speed = null) => Session.StartRun(track, speed);

        public void Restart()
        {
            if (Session.Run == null)
            {
                Play();
            }
            else
            {
                Session.Restart();
            }
        }

        public void TogglePause() => Session.TogglePause();

        public void ToTitle()
        {
            Session.ToTitle();
            NewAttract();
        }

        public void OpenShop() => Session.OpenShop();

        public void Press(RunInput press) => Session.Press(press);

        /// <summary>Plays the current run with the bot, without rendering, until it reaches a distance.</summary>
        public void Simulate(float metres)
        {
            Run run = Session.Run;
            while (run != null && run.Alive && run.Distance < metres)
            {
                run.Step(Core.Autopilot.Decide(run), Session.StepTime);
                run.Events.Clear();
            }
        }

        void Back()
        {
            Click();
            if (Session.Screen == Screen.Settings) Session.CloseSettings();
            else if (Session.Screen == Screen.Shop) Session.CloseShop();
            else if (Session.Screen == Screen.Paused) TogglePause();
            else if (Session.Screen == Screen.GameOver) ToTitle();
        }

        void PickSkin(int index)
        {
            if (Session.Profile.Owns(index))
            {
                Session.Equip(index);
                Click();
            }
            else
            {
                sound.Play(Session.Buy(index) ? art.Buy : art.Denied, 1f, 0f);
            }
            hud.RefreshShop(Session.Profile);
        }

        void Click() => sound.Play(art.Click, 0.7f, 0f);

        void NewAttract()
        {
            attract = new Run(new TrackGenerator(attractSeed++));
            attractAccumulator = 0f;
        }

        // ---- Frame ----

        void Update()
        {
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.1f);
            if (!ScriptedInput)
            {
                ReadInput();
            }
            if (Autopilot && Session.Run != null && Session.Screen == Screen.Playing)
            {
                Session.Press(Core.Autopilot.Decide(Session.Run));
            }
            if (!Frozen)
            {
                Session.Advance(dt);
                StepAttract(dt);
            }

            Run run = Shown;
            if (run != shownRun)
            {
                shownRun = run;
                cameraRig.Snap();
            }
            if (run == Session.Run)
            {
                React(run);
            }
            run.Events.Clear();

            float time = Time.unscaledTime;
            World.Sync(run, time);
            Knight.Wear(Session.Profile.Skin);
            if (Session.Screen == Screen.Shop)
            {
                Vector3 spot = new Vector3(Tuning.LaneX(1), 0f, run.Distance);
                Knight.Idle(spot, time);
                cameraRig.Showcase(spot, dt);
            }
            else
            {
                Knight.Sync(run, Frozen ? 0f : dt, time);
                cameraRig.Chase(Knight.Root.position, run.Alive ? run.Speed : 0f, Frozen ? 0f : dt);
            }

            if (Session.Screen != shownScreen)
            {
                EnterScreen(Session.Screen);
            }
            if (Session.Screen == Screen.Settings)
            {
                hud.RefreshSettings(Session.Profile);
            }
            sound.SetVolumes(Session.Profile.Music, Session.Profile.Effects);
            hud.Tick(Session.Run, dt);
        }

        void StepAttract(float dt)
        {
            bool behindMenu = Session.Run == null && Session.Screen != Screen.Shop;
            if (!behindMenu)
            {
                return;
            }
            attractAccumulator = Mathf.Min(attractAccumulator + dt, Session.StepTime * 8);
            while (attractAccumulator >= Session.StepTime)
            {
                attractAccumulator -= Session.StepTime;
                attract.Step(Core.Autopilot.Decide(attract), Session.StepTime);
            }
            // Keep the bot near the start of the castle, where the road is gentle and the speed modest.
            if (!attract.Alive || attract.Distance > 900f)
            {
                NewAttract();
            }
        }

        void ReadInput()
        {
            switch (Session.Screen)
            {
                case Screen.Title:
                    if (input.Confirm) { Click(); Play(); }
                    else if (input.Shop) { Click(); OpenShop(); }
                    break;
                case Screen.Playing:
                    if (input.Pause) { Click(); TogglePause(); }
                    else Session.Press(input.Read());
                    break;
                case Screen.Paused:
                    if (input.Confirm || input.Pause) { Click(); TogglePause(); }
                    else if (input.Restart) { Click(); Restart(); }
                    break;
                case Screen.Settings:
                    if (input.Back || input.Confirm) Back();
                    else
                    {
                        int step = input.Navigate();
                        if (step != 0) { Session.SetMusic(Session.Profile.Music + step); Click(); }
                    }
                    break;
                case Screen.Shop:
                    if (input.Back || input.Shop) Back();
                    break;
                case Screen.GameOver:
                    if (input.Confirm || input.Restart) { Click(); Restart(); }
                    else if (input.Shop) { Click(); OpenShop(); }
                    else if (input.Back) Back();
                    break;
            }
        }

        void EnterScreen(Screen screen)
        {
            shownScreen = screen;
            hud.Show(screen, Session);
            bool inRun = screen == Screen.Playing || screen == Screen.Paused || (screen == Screen.Settings && Session.Run != null);
            sound.PlayMusic(inRun ? art.RunMusic : art.TitleMusic);
            sound.SetDucked(screen == Screen.Paused || screen == Screen.Settings);
            if (screen == Screen.GameOver && Session.LastResult != null)
            {
                sound.Play(Session.LastResult.NewBest ? art.NewBest : art.GameOver, 0.9f, 0f);
            }
        }

        void React(Run run)
        {
            foreach (RunEvent e in run.Events)
            {
                var at = new Vector3(e.X, e.Y, e.Z);
                switch (e.Kind)
                {
                    case RunEventKind.LaneChange:
                        sound.Play(art.Lane, 0.5f);
                        break;
                    case RunEventKind.Jump:
                        sound.Play(art.Jump, 0.7f);
                        break;
                    case RunEventKind.Land:
                        sound.Play(art.Land, 0.6f);
                        Effects.Burst(at + Vector3.up * 0.05f, art.ParticleAlpha, new Color(0.6f, 0.55f, 0.5f, 0.6f), new Color(0.4f, 0.36f, 0.32f, 0.4f), 10, 2.2f, 0.35f, 0.4f);
                        break;
                    case RunEventKind.Slide:
                        sound.Play(art.Slide, 0.7f);
                        Effects.Burst(at + Vector3.up * 0.05f, art.ParticleAlpha, new Color(0.6f, 0.55f, 0.5f, 0.6f), new Color(0.4f, 0.36f, 0.32f, 0.4f), 14, 2.8f, 0.4f, 0.5f);
                        break;
                    case RunEventKind.Coin:
                        sound.Play(art.Coin, 0.6f, 0.08f);
                        Effects.Burst(at, art.ParticleAdditive, new Color(1f, 0.85f, 0.3f), new Color(1f, 0.6f, 0.1f), 8, 2.5f, 0.18f, 0.35f);
                        break;
                    case RunEventKind.Gem:
                        sound.Play(art.GemSound, 0.9f, 0f);
                        Effects.Burst(at, art.ParticleAdditive, new Color(0.4f, 0.9f, 1f), new Color(0.6f, 0.5f, 1f), 24, 4f, 0.25f, 0.6f);
                        break;
                    case RunEventKind.PowerUp:
                        sound.Play(art.PowerUp, 0.9f, 0f);
                        Effects.Burst(at, art.ParticleAdditive, PowerColor(e.Pickup), Color.white, 30, 5f, 0.3f, 0.6f);
                        hud.ShowBanner(e.Pickup == PickupKind.Shield ? "ESCUDO!" : e.Pickup == PickupKind.Magnet ? "ÍMÃ!" : "PONTOS EM DOBRO!", 1.2f);
                        break;
                    case RunEventKind.ShieldBreak:
                        sound.Play(art.ShieldBreak, 1f, 0f);
                        cameraRig.Shake(0.6f);
                        Effects.Burst(at + Vector3.up, art.ParticleAlpha, new Color(0.55f, 0.36f, 0.2f), new Color(0.35f, 0.22f, 0.12f), 30, 6f, 0.3f, 0.9f, gravity: 1.5f);
                        Effects.Burst(Knight.Root.position + Vector3.up, art.ParticleAdditive, new Color(0.4f, 0.7f, 1f), Color.white, 30, 5f, 0.3f, 0.5f);
                        break;
                    case RunEventKind.Milestone:
                        sound.Play(art.Milestone, 0.9f, 0f);
                        hud.ShowBanner($"{e.Value:N0} m", 1.4f);
                        break;
                    case RunEventKind.Death:
                        sound.Play(art.Crash, 1f, 0f);
                        cameraRig.Shake(1f);
                        Effects.Burst(Knight.Root.position + Vector3.up * 1.1f, art.ParticleAdditive, new Color(1f, 0.85f, 0.5f), new Color(1f, 0.5f, 0.2f), 26, 6f, 0.2f, 0.5f, gravity: 1f);
                        break;
                }
            }
        }

        static Color PowerColor(PickupKind kind) =>
            kind == PickupKind.Shield ? new Color(0.35f, 0.65f, 1f) : kind == PickupKind.Magnet ? new Color(1f, 0.35f, 0.3f) : new Color(0.75f, 0.45f, 1f);
    }
}
