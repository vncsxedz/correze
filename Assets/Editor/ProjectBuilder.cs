using System;
using System.IO;
using GauntletRun.Game;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace GauntletRun.EditorTools
{
    /// <summary>
    /// Generates the materials, art asset, icon, scene and player settings from code, and builds
    /// the players, so every binary asset can be recreated from text. Batch mode:
    /// <code>
    /// Unity -batchmode -quit -projectPath . -executeMethod GauntletRun.EditorTools.ProjectBuilder.SetUpProject
    /// Unity -batchmode -quit -projectPath . -executeMethod GauntletRun.EditorTools.ProjectBuilder.BuildWindows -buildOutput &lt;folder&gt;
    /// Unity -batchmode -quit -projectPath . -executeMethod GauntletRun.EditorTools.ProjectBuilder.BuildWebGL -buildOutput &lt;folder&gt;
    /// </code>
    /// </summary>
    public static class ProjectBuilder
    {
        const string ScenePath = "Assets/Scenes/Gauntlet.unity";
        const string MaterialFolder = "Assets/Art/Materials";
        const string ArtPath = "Assets/Art/GameArt.asset";
        const string SoftDotPath = "Assets/Art/SoftDot.png";
        const string IconPath = "Assets/Art/AppIcon.png";
        const string AudioFolder = "Assets/Audio";
        const string ThemePath = "Assets/UI/HudTheme.tss";
        const string PanelSettingsPath = "Assets/UI/HudPanelSettings.asset";

        static readonly Color Dusk = Hex("#1b1426");

        [MenuItem("Gauntlet Run/Regenerate Art, Scene and Player Settings")]
        public static void SetUpProject()
        {
            GameArt art = CreateArt();
            PanelSettings panel = CreatePanelSettings();
            CreateScene(art, panel);
            ConfigurePlayer(CreateIcon());
            AssetDatabase.SaveAssets();
            Debug.Log("Gauntlet Run project set up.");
        }

        /// <summary>
        /// Enters play mode and lets ScreenshotScenarios capture the README screenshots, then exits.
        /// Run in batch mode without -quit: <c>-executeMethod ...CaptureScreenshots -screenshot &lt;folder&gt;</c>.
        /// </summary>
        public static void CaptureScreenshots()
        {
            EditorSceneManager.OpenScene(ScenePath);
            EditorApplication.isPlaying = true;
        }

        public static void BuildWindows() => Build(BuildTarget.StandaloneWindows64, "GauntletRun.exe");

        public static void BuildWebGL() => Build(BuildTarget.WebGL, string.Empty);

        // ---- Art ----

        static GameArt CreateArt()
        {
            Directory.CreateDirectory(MaterialFolder);
            var art = AssetDatabase.LoadAssetAtPath<GameArt>(ArtPath);
            if (art == null)
            {
                art = ScriptableObject.CreateInstance<GameArt>();
                AssetDatabase.CreateAsset(art, ArtPath);
            }

            art.Floor = Lit("Floor", "#454a50", 0.12f);
            art.FloorDark = Lit("FloorDark", "#30343a", 0.1f);
            art.Trim = Lit("Trim", "#f3c844", 0.6f, metallic: 0.7f);
            art.Wall = Lit("Wall", "#586875", 0.08f);
            art.WallDark = Lit("WallDark", "#37434c", 0.08f);
            art.Banner = Lit("Banner", "#268b57", 0.15f);
            art.TorchFlame = Glowing("TorchFlame", "#f3c844", "#e68124", 1.5f);
            art.Wood = Lit("Wood", "#d9a441", 0.12f);
            art.WoodDark = Lit("WoodDark", "#4b3e34", 0.1f);
            art.Iron = Lit("Iron", "#30363b", 0.5f, metallic: 0.7f);
            art.Steel = Lit("Steel", "#d6dde6", 0.85f, metallic: 0.9f);

            art.Armour = Lit("Armour", "#273c32", 0.7f, metallic: 0.75f);
            art.Cloth = Lit("Cloth", "#268b57", 0.2f);
            art.Visor = Unlit("Visor", "#0b0a10");

            art.Gold = Glowing("Gold", "#f2c14e", "#b98318", 0.9f, smoothness: 0.8f, metallic: 0.7f);
            art.Gem = Glowing("Gem", "#6fe3ff", "#2fb6ff", 1.6f, smoothness: 0.9f);
            art.ShieldBlue = Glowing("ShieldBlue", "#3d7fe0", "#1f5fd0", 0.8f, smoothness: 0.6f, metallic: 0.3f);
            art.MagnetRed = Glowing("MagnetRed", "#d8382f", "#b01a12", 0.8f, smoothness: 0.6f);
            art.DoublePurple = Glowing("DoublePurple", "#a45cff", "#7a2fe0", 1.2f, smoothness: 0.7f, metallic: 0.4f);

            Texture2D dot = CreateSoftDot();
            art.ParticleAdditive = Particle("ParticleAdditive", dot, additive: true, tint: new Color(0.5f, 0.5f, 0.5f, 0.5f));
            art.ParticleAlpha = Particle("ParticleAlpha", dot, additive: false, tint: new Color(0.5f, 0.5f, 0.5f, 0.5f));
            art.BlobShadow = Particle("BlobShadow", dot, additive: false, tint: new Color(0f, 0f, 0f, 0.45f));

            art.CubeMesh = PrimitiveMesh(PrimitiveType.Cube);
            art.SphereMesh = PrimitiveMesh(PrimitiveType.Sphere);
            art.CylinderMesh = PrimitiveMesh(PrimitiveType.Cylinder);
            art.CapsuleMesh = PrimitiveMesh(PrimitiveType.Capsule);
            art.QuadMesh = PrimitiveMesh(PrimitiveType.Quad);

            art.RunMusic = Clip("run_theme", music: true);
            art.TitleMusic = Clip("title_theme", music: true);
            art.Coin = Clip("coin");
            art.GemSound = Clip("gem");
            art.Jump = Clip("jump");
            art.Land = Clip("land");
            art.Slide = Clip("slide");
            art.Lane = Clip("lane");
            art.PowerUp = Clip("power_up");
            art.ShieldBreak = Clip("shield_break");
            art.Crash = Clip("crash");
            art.Milestone = Clip("milestone");
            art.GameOver = Clip("game_over");
            art.NewBest = Clip("new_best");
            art.Buy = Clip("buy");
            art.Click = Clip("click");
            art.Denied = Clip("denied");

            EditorUtility.SetDirty(art);
            return art;
        }

        /// <summary>The exact mesh GameObject.CreatePrimitive uses for a shape (the probe's collider is discarded).</summary>
        static Mesh PrimitiveMesh(PrimitiveType type)
        {
            GameObject probe = GameObject.CreatePrimitive(type);
            Mesh mesh = probe.GetComponent<MeshFilter>().sharedMesh;
            UnityEngine.Object.DestroyImmediate(probe);
            return mesh;
        }

        /// <summary>A Standard material; emission is enabled (black) on characters so hits can flash them.</summary>
        static Material Lit(string name, string color, float smoothness, float metallic = 0f, string emission = null)
        {
            Material material = MaterialAt(name, Shader.Find("Standard"));
            material.color = Hex(color);
            material.SetFloat("_Glossiness", smoothness);
            material.SetFloat("_Metallic", metallic);
            if (emission != null)
            {
                material.EnableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
                material.SetColor("_EmissionColor", Hex(emission));
            }
            else
            {
                material.DisableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Color.black);
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>A Standard material that glows in the dark (eyes, lanterns, bile, curses).</summary>
        static Material Glowing(string name, string color, string glow, float intensity, float smoothness = 0.4f, float metallic = 0f)
        {
            Material material = Lit(name, color, smoothness, metallic, "#000000");
            material.SetColor("_EmissionColor", Hex(glow) * intensity);
            EditorUtility.SetDirty(material);
            return material;
        }

        static Material Unlit(string name, string color)
        {
            Material material = MaterialAt(name, Shader.Find("Unlit/Color"));
            material.color = Hex(color);
            EditorUtility.SetDirty(material);
            return material;
        }

        static Material Particle(string name, Texture2D texture, bool additive, Color tint)
        {
            Material material = MaterialAt(name, Shader.Find("Legacy Shaders/Particles/" + (additive ? "Additive" : "Alpha Blended")));
            material.mainTexture = texture;
            material.SetColor("_TintColor", tint);
            EditorUtility.SetDirty(material);
            return material;
        }

        static Material MaterialAt(string name, Shader shader)
        {
            string path = $"{MaterialFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            return material;
        }

        /// <summary>A small white disc with a soft edge for every particle, glow and shadow.</summary>
        static Texture2D CreateSoftDot()
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f) / size * 2f - 1f;
                    float dy = (y + 0.5f) / size * 2f - 1f;
                    float alpha = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha * alpha * (3f - 2f * alpha)));
                }
            }
            texture.Apply();
            File.WriteAllBytes(SoftDotPath, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(SoftDotPath, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(SoftDotPath);
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(SoftDotPath);
        }

        static AudioClip Clip(string name, bool music = false)
        {
            string path = $"{AudioFolder}/{name}.ogg";
            var importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer == null)
            {
                Debug.LogWarning($"Missing {path}: run python tools/audio/build.py first.");
                return null;
            }
            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            settings.loadType = music ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
            importer.defaultSampleSettings = settings;
            importer.forceToMono = true;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }

        // ---- UI ----

        static PanelSettings CreatePanelSettings()
        {
            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            if (panel == null)
            {
                panel = ScriptableObject.CreateInstance<PanelSettings>();
                AssetDatabase.CreateAsset(panel, PanelSettingsPath);
            }
            panel.themeStyleSheet = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(ThemePath);
            panel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panel.referenceResolution = new Vector2Int(1280, 720);
            panel.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            panel.match = 0.5f;
            panel.clearColor = false;
            EditorUtility.SetDirty(panel);
            return panel;
        }

        // ---- Scene ----

        static void CreateScene(GameArt art, PanelSettings panel)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Dusk;
            camera.fieldOfView = 58f;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 140f;
            cameraObject.AddComponent<AudioListener>();
            CameraRig rig = cameraObject.AddComponent<CameraRig>();

            var sunObject = new GameObject("Evening light");
            var sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = Hex("#ffd2a1");
            sun.intensity = 1.15f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.7f;
            sunObject.transform.rotation = Quaternion.Euler(48f, -28f, 0f);

            var fillObject = new GameObject("Torch fill");
            var fill = fillObject.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = Hex("#ff9a52");
            fill.intensity = 0.3f;
            fill.shadows = LightShadows.None;
            fillObject.transform.rotation = Quaternion.Euler(20f, 160f, 0f);

            RenderSettings.skybox = null;
            RenderSettings.sun = sun;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = Hex("#76649a");
            RenderSettings.ambientEquatorColor = Hex("#5e4a50");
            RenderSettings.ambientGroundColor = Hex("#201a1c");
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = Dusk;
            RenderSettings.fogStartDistance = 45f;
            RenderSettings.fogEndDistance = 120f;

            var ui = new GameObject("HUD");
            var document = ui.AddComponent<UIDocument>();
            document.panelSettings = panel;
            Hud hud = ui.AddComponent<Hud>();

            var game = new GameObject("Game");
            var director = game.AddComponent<GameDirector>();
            var serialized = new SerializedObject(director);
            serialized.FindProperty("art").objectReferenceValue = art;
            serialized.FindProperty("hud").objectReferenceValue = hud;
            serialized.FindProperty("cameraRig").objectReferenceValue = rig;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        static void ConfigurePlayer(Texture2D icon)
        {
            PlayerSettings.companyName = "Trysac";
            PlayerSettings.productName = "Corre, Zé!";
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);
            int[] sizes = PlayerSettings.GetIconSizes(NamedBuildTarget.Standalone, IconKind.Any);
            var standaloneIcons = new Texture2D[sizes.Length];
            Array.Fill(standaloneIcons, icon);
            PlayerSettings.SetIcons(NamedBuildTarget.Standalone, standaloneIcons, IconKind.Any);
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = false;
            PlayerSettings.defaultWebScreenWidth = 1280;
            PlayerSettings.defaultWebScreenHeight = 720;
            // Static hosts such as GitHub Pages do not send Content-Encoding for .br/.gz files.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = false;
        }

        // ---- Icon: a knight's helmet in front of a gold coin ----

        static Texture2D CreateIcon()
        {
            const int size = 1024;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    pixels[y * size + x] = IconPixel(x + 0.5f - size / 2f, y + 0.5f - size / 2f);
                }
            }
            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(IconPath, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(IconPath, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(IconPath);
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
        }

        static Color IconPixel(float x, float y)
        {
            const float aa = 1.5f;
            float tile = RoundedBox(x, y, 470f, 470f, 180f);
            Color color = Color.Lerp(Hex("#120c1c"), Hex("#3a2552"), Mathf.Clamp01((y + 470f) / 940f));
            color.a = Coverage(-tile, aa);

            // A gold coin with a darker rim.
            float coin = Length(x, y - 20f) - 340f;
            color = Over(color, Hex("#8a5a12"), Coverage(-(coin - 22f), aa));
            color = Over(color, Hex("#f2c14e"), Coverage(-coin, aa));
            color = Over(color, Hex("#ffe08a"), Coverage(-(Length(x + 120f, y - 160f) - 90f), aa) * 0.45f);

            // A great helm: rounded top, flat sides, a visor slit and a crest.
            float helm = Mathf.Min(RoundedBox(x, y + 40f, 190f, 210f, 60f), Length(x, y - 90f) - 190f);
            helm = Mathf.Max(helm, -(y + 260f));
            color = Over(color, Hex("#2a2733"), Coverage(-(helm - 16f), aa));
            color = Over(color, Hex("#c3cad6"), Coverage(-helm, aa));
            color = Over(color, Hex("#8d96a5"), Coverage(-helm, aa) * Mathf.Clamp01((x + 40f) / 300f) * 0.6f);
            float slit = RoundedBox(x, y - 20f, 150f, 22f, 10f);
            color = Over(color, Hex("#0d0b12"), Coverage(-slit, aa));
            float breaths = RoundedBox(x, y + 120f, 10f, 60f, 5f);
            color = Over(color, Hex("#2a2733"), Coverage(-breaths, aa));
            float crest = RoundedBox(x, y - 300f, 34f, 70f, 20f);
            color = Over(color, Hex("#a32632"), Coverage(-crest, aa));
            return color;
        }

        static float Length(float x, float y) => Mathf.Sqrt(x * x + y * y);

        static float RoundedBox(float x, float y, float halfWidth, float halfHeight, float radius)
        {
            float qx = Mathf.Abs(x) - (halfWidth - radius);
            float qy = Mathf.Abs(y) - (halfHeight - radius);
            float outside = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude;
            return outside + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
        }

        static float Coverage(float inside, float width) => Mathf.Clamp01(inside / width + 0.5f);

        static Color Over(Color below, Color above, float amount)
        {
            Color mixed = Color.Lerp(below, above, amount);
            mixed.a = below.a;
            return mixed;
        }

        static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out Color color) ? color : Color.magenta;

        // ---- Builds ----

        static void Build(BuildTarget target, string fileName)
        {
            string folder = Argument("-buildOutput");
            if (string.IsNullOrEmpty(folder))
            {
                throw new ArgumentException("Pass -buildOutput <folder> outside the repository.");
            }
            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                target = target,
                locationPathName = string.IsNullOrEmpty(fileName) ? folder : Path.Combine(folder, fileName),
                options = BuildOptions.None,
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            Debug.Log($"Build {target}: {summary.result}, {summary.totalSize / (1024 * 1024f):0.0} MB, {summary.totalErrors} errors.");
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
            }
        }

        static string Argument(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, name);
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
        }
    }
}
