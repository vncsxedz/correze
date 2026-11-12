using GauntletRun.Core;
using UnityEngine;

namespace GauntletRun.Game
{
    /// <summary>The knight's moving parts, so the view can animate them.</summary>
    public sealed class KnightRig
    {
        public Transform Root;
        public Transform Body;
        public Transform LeftLeg;
        public Transform RightLeg;
        public Transform LeftArm;
        public Transform RightArm;
        public Transform Cape;
        public Renderer[] Armour;
        public Renderer[] Trim;
        public Renderer[] Cloth;
    }

    /// <summary>Every model in the game, built from Unity's primitives and a few code-made meshes.</summary>
    public static class Models
    {
        static GameArt art;
        static Mesh spike;
        static Mesh gem;
        static Mesh blade;
        static Material skinTone;
        static Material shirtDirt;
        static Material denimWear;
        static Material kittyPink;
        static Material kittyWhite;
        static Material kittyInk;
        static Material kittyNose;

        public static void Init(GameArt gameArt)
        {
            art = gameArt;
            // Retint the generated materials at runtime so the existing GameArt asset adopts the city palette too.
            art.Floor.color = Shapes.Hex("#454a50");
            art.FloorDark.color = Shapes.Hex("#30343a");
            art.Trim.color = Shapes.Hex("#f3c844");
            art.Wall.color = Shapes.Hex("#586875");
            art.WallDark.color = Shapes.Hex("#37434c");
            art.Banner.color = Shapes.Hex("#268b57");
            art.Wood.color = Shapes.Hex("#d9a441");
            art.WoodDark.color = Shapes.Hex("#4b3e34");
            art.Iron.color = Shapes.Hex("#30363b");
            art.Armour.color = Shapes.Hex("#273c32");
            art.Cloth.color = Shapes.Hex("#f0eee7");
            skinTone = Shapes.Recolored(art.Wood, Shapes.Hex("#b77b52"));
            shirtDirt = Shapes.Recolored(art.Cloth, Shapes.Hex("#857b6c"));
            denimWear = Shapes.Recolored(art.Armour, Shapes.Hex("#8190a0"));
            kittyPink = Shapes.Recolored(art.Trim, Shapes.Hex("#f28bb7"));
            kittyWhite = Shapes.Recolored(art.Cloth, Shapes.Hex("#fff8eb"));
            kittyInk = Shapes.Recolored(art.Iron, Shapes.Hex("#29242a"));
            kittyNose = Shapes.Recolored(art.Trim, Shapes.Hex("#ed738f"));
            spike = Shapes.Prism(4, 0.09f, 0f, 0.28f, seed: 1);
            gem = Shapes.Blob(2, 4, 0f, 2);
            blade = Shapes.Prism(3, 0.55f, 0.35f, 0.08f, seed: 3);
        }

        // ---- Zé ----

        public static KnightRig Knight(Transform parent)
        {
            // Preserve the runner's pivots and body proportions so all existing poses
            // continue to align with the gameplay lane and collision math.
            var rig = new KnightRig { Root = new GameObject("Ze").transform };
            rig.Root.SetParent(parent, false);
            Shapes.Shadow(rig.Root, 0.5f);
            rig.Body = new GameObject("Body").transform;
            rig.Body.SetParent(rig.Root, false);
            Transform body = rig.Body;

            Material shorts = art.Armour;
            Material accent = art.Trim;
            Material shirt = art.Cloth;
            Material skin = skinTone ?? Shapes.Recolored(art.Wood, Shapes.Hex("#b77b52"));
            Material hair = art.WoodDark;
            Material detail = art.Steel;
            Material stains = shirtDirt ?? Shapes.Recolored(art.Cloth, Shapes.Hex("#a99e87"));
            Material wornDenim = denimWear ?? Shapes.Recolored(art.Armour, Shapes.Hex("#8190a0"));

            // Worn denim shorts, visible bare calves and bare feet. Leg pivots are unchanged.
            rig.LeftLeg = Pivot("Left leg", body, new Vector3(-0.17f, 0.82f, 0f));
            rig.RightLeg = Pivot("Right leg", body, new Vector3(0.17f, 0.82f, 0f));
            foreach (Transform leg in new[] { rig.LeftLeg, rig.RightLeg })
            {
                Shapes.Part("Bermuda", leg, PrimitiveType.Capsule, shorts,
                    new Vector3(0f, -0.18f, 0f), Vector3.zero, new Vector3(0.19f, 0.22f, 0.2f));
                Shapes.Part("Barra gasta", leg, PrimitiveType.Cube, wornDenim,
                    new Vector3(0f, -0.32f, 0.015f), new Vector3(0f, 0f, 2f), new Vector3(0.18f, 0.035f, 0.19f), shadows: false);
                Shapes.Part("Perna", leg, PrimitiveType.Capsule, skin,
                    new Vector3(0f, -0.53f, 0.015f), Vector3.zero, new Vector3(0.13f, 0.24f, 0.14f));
                // Foot length points forward (+Z); flattened toes give a readable barefoot silhouette.
                Shapes.Part("Pé descalço", leg, PrimitiveType.Sphere, skin,
                    new Vector3(0f, -0.76f, 0.075f), new Vector3(-5f, 0f, 0f), new Vector3(0.19f, 0.085f, 0.29f));
                for (int toe = 0; toe < 3; toe++)
                {
                    float x = (toe - 1) * 0.052f;
                    Shapes.Part("Dedo", leg, PrimitiveType.Sphere, skin,
                        new Vector3(x, -0.755f, 0.185f - Mathf.Abs(toe - 1) * 0.018f), Vector3.zero,
                        new Vector3(0.052f, 0.045f, 0.085f), shadows: false);
                }
            }

            // Denim waistband and soft, slightly oversized dirty-white T-shirt.
            Shapes.Part("Cintura da bermuda", body, PrimitiveType.Capsule, shorts,
                new Vector3(0f, 0.84f, 0f), Vector3.zero, new Vector3(0.37f, 0.13f, 0.24f));
            Shapes.Part("Camiseta", body, PrimitiveType.Capsule, shirt,
                new Vector3(0f, 1.14f, 0f), Vector3.zero, new Vector3(0.36f, 0.34f, 0.235f));
            Shapes.Part("Gola", body, PrimitiveType.Cylinder, shirt,
                new Vector3(0f, 1.435f, 0f), Vector3.zero, new Vector3(0.13f, 0.025f, 0.13f));
            // Front-facing backpack straps make the pack read from both gameplay and shop cameras.
            Shapes.Part("Alça frontal esquerda", body, PrimitiveType.Capsule, kittyPink,
                new Vector3(-0.22f, 1.22f, 0.19f), new Vector3(0f, 0f, -12f), new Vector3(0.035f, 0.21f, 0.025f), shadows: false);
            Shapes.Part("Alça frontal direita", body, PrimitiveType.Capsule, kittyPink,
                new Vector3(0.22f, 1.22f, 0.19f), new Vector3(0f, 0f, 12f), new Vector3(0.035f, 0.21f, 0.025f), shadows: false);
            // Subtle grime and small torn cloth notches, kept decorative and collider-free.
            Shapes.Part("Mancha da camiseta", body, PrimitiveType.Sphere, stains,
                new Vector3(-0.17f, 1.04f, 0.235f), new Vector3(0f, 0f, -18f), new Vector3(0.09f, 0.05f, 0.012f), shadows: false);
            Shapes.Part("Mancha da camiseta", body, PrimitiveType.Sphere, stains,
                new Vector3(0.13f, 1.21f, 0.24f), new Vector3(0f, 0f, 22f), new Vector3(0.055f, 0.035f, 0.01f), shadows: false);
            Shapes.Part("Rasgo da camiseta", body, PrimitiveType.Cube, stains,
                new Vector3(0.205f, 0.93f, 0.235f), new Vector3(0f, 0f, 28f), new Vector3(0.018f, 0.085f, 0.02f), shadows: false);
            Shapes.Part("Rasgo da camiseta", body, PrimitiveType.Cube, stains,
                new Vector3(-0.12f, 0.85f, 0.21f), new Vector3(0f, 0f, -35f), new Vector3(0.02f, 0.07f, 0.02f), shadows: false);
            // Denim wear marks and a simple pocket/seam add cloth detail without changing the silhouette.
            Shapes.Part("Costura da bermuda", body, PrimitiveType.Cube, accent,
                new Vector3(0.205f, 0.78f, 0.02f), Vector3.zero, new Vector3(0.012f, 0.12f, 0.17f), shadows: false);
            Shapes.Part("Bolso da bermuda", body, PrimitiveType.Cube, wornDenim,
                new Vector3(0.13f, 0.84f, 0.126f), Vector3.zero, new Vector3(0.11f, 0.09f, 0.015f), shadows: false);

            // Keep the backpack on the original Cape pivot because KnightView animates it.
            rig.Cape = Pivot("Mochila", body, new Vector3(0f, 1.18f, -0.22f));
            Shapes.Part("Mochila rosa", rig.Cape, PrimitiveType.Capsule, kittyPink,
                new Vector3(0f, -0.055f, -0.07f), Vector3.zero, new Vector3(0.32f, 0.36f, 0.15f));
            Shapes.Part("Bolso frontal", rig.Cape, PrimitiveType.Sphere, kittyPink,
                new Vector3(0f, -0.2f, -0.174f), Vector3.zero, new Vector3(0.2f, 0.13f, 0.035f));
            // The small white cat face points toward the following camera (-Z).
            Shapes.Part("Rosto da gatinha", rig.Cape, PrimitiveType.Sphere, kittyWhite,
                new Vector3(0f, -0.04f, -0.207f), Vector3.zero, new Vector3(0.2f, 0.19f, 0.025f), shadows: false);
            foreach (float side in new[] { -1f, 1f })
            {
                Shapes.Part("Orelha da gatinha", rig.Cape, PrimitiveType.Sphere, kittyWhite,
                    new Vector3(side * 0.12f, 0.075f, -0.22f), new Vector3(0f, 0f, side * -28f),
                    new Vector3(0.085f, 0.12f, 0.035f), shadows: false);
                Shapes.Part("Olho da gatinha", rig.Cape, PrimitiveType.Sphere, kittyInk,
                    new Vector3(side * 0.065f, -0.035f, -0.235f), Vector3.zero,
                    new Vector3(0.018f, 0.025f, 0.012f), shadows: false);
                Shapes.Part("Bigode da gatinha", rig.Cape, PrimitiveType.Cube, kittyInk,
                    new Vector3(side * 0.12f, -0.07f, -0.235f), new Vector3(0f, 0f, side * 8f),
                    new Vector3(0.06f, 0.008f, 0.008f), shadows: false);
                Shapes.Part("Bigode da gatinha", rig.Cape, PrimitiveType.Cube, kittyInk,
                    new Vector3(side * 0.12f, -0.095f, -0.235f), new Vector3(0f, 0f, side * -8f),
                    new Vector3(0.06f, 0.008f, 0.008f), shadows: false);
            }
            Shapes.Part("Nariz da gatinha", rig.Cape, PrimitiveType.Sphere, kittyNose,
                new Vector3(0f, -0.063f, -0.239f), Vector3.zero, new Vector3(0.025f, 0.017f, 0.012f), shadows: false);
            Shapes.Part("Laço da gatinha", rig.Cape, PrimitiveType.Sphere, kittyPink,
                new Vector3(0.17f, 0.075f, -0.222f), new Vector3(0f, 0f, -20f), new Vector3(0.075f, 0.045f, 0.025f), shadows: false);
            Shapes.Part("Nó do laço", rig.Cape, PrimitiveType.Sphere, kittyNose,
                new Vector3(0.17f, 0.075f, -0.245f), Vector3.zero, new Vector3(0.025f, 0.025f, 0.015f), shadows: false);
            Shapes.Part("Alça traseira esquerda", rig.Cape, PrimitiveType.Capsule, kittyPink,
                new Vector3(-0.13f, -0.045f, 0.015f), new Vector3(0f, 0f, -8f), new Vector3(0.035f, 0.2f, 0.035f), shadows: false);
            Shapes.Part("Alça traseira direita", rig.Cape, PrimitiveType.Capsule, kittyPink,
                new Vector3(0.13f, -0.045f, 0.015f), new Vector3(0f, 0f, 8f), new Vector3(0.035f, 0.2f, 0.035f), shadows: false);

            // Face, ears and deliberately untidy layered hair; forward remains +Z.
            Shapes.Part("Cabeça", body, PrimitiveType.Sphere, skin,
                new Vector3(0f, 1.64f, 0.02f), Vector3.zero, new Vector3(0.32f, 0.35f, 0.31f));
            Shapes.Part("Cabelo", body, PrimitiveType.Sphere, hair,
                new Vector3(0f, 1.81f, -0.015f), Vector3.zero, new Vector3(0.31f, 0.13f, 0.3f));
            Shapes.Part("Mecha bagunçada", body, PrimitiveType.Capsule, hair,
                new Vector3(-0.11f, 1.89f, -0.06f), new Vector3(0f, 0f, -28f), new Vector3(0.08f, 0.09f, 0.08f));
            Shapes.Part("Mecha bagunçada", body, PrimitiveType.Capsule, hair,
                new Vector3(0.07f, 1.93f, -0.04f), new Vector3(0f, 0f, 24f), new Vector3(0.075f, 0.09f, 0.075f));
            Shapes.Part("Franja", body, PrimitiveType.Sphere, hair,
                new Vector3(0.025f, 1.82f, 0.22f), new Vector3(0f, 0f, -12f), new Vector3(0.18f, 0.08f, 0.11f));
            foreach (float side in new[] { -1f, 1f })
            {
                Shapes.Part("Orelha", body, PrimitiveType.Sphere, skin,
                    new Vector3(side * 0.16f, 1.64f, 0.025f), Vector3.zero, new Vector3(0.09f, 0.12f, 0.075f));
                Shapes.Part("Costeleta", body, PrimitiveType.Capsule, hair,
                    new Vector3(side * 0.22f, 1.68f, 0.08f), new Vector3(0f, 0f, side * 10f), new Vector3(0.05f, 0.12f, 0.06f));
                Shapes.Part("Sobrancelha", body, PrimitiveType.Cube, hair,
                    new Vector3(side * 0.085f, 1.735f, 0.305f), new Vector3(0f, 0f, side * 7f), new Vector3(0.085f, 0.025f, 0.02f));
                Shapes.Part("Olho", body, PrimitiveType.Sphere, detail,
                    new Vector3(side * 0.085f, 1.665f, 0.302f), Vector3.zero, new Vector3(0.052f, 0.064f, 0.028f), shadows: false);
                Shapes.Part("Pupila", body, PrimitiveType.Sphere, art.Visor,
                    new Vector3(side * 0.078f, 1.665f, 0.325f), Vector3.zero, new Vector3(0.024f, 0.034f, 0.015f), shadows: false);
            }
            Shapes.Part("Nariz", body, PrimitiveType.Sphere, skin,
                new Vector3(0f, 1.595f, 0.326f), Vector3.zero, new Vector3(0.07f, 0.085f, 0.065f));
            Shapes.Part("Sorriso", body, PrimitiveType.Cube, hair,
                new Vector3(0f, 1.525f, 0.305f), Vector3.zero, new Vector3(0.105f, 0.018f, 0.018f), shadows: false);

            // Shirt sleeves and skin-toned forearms stay attached to the original pivots.
            rig.LeftArm = Pivot("Left arm", body, new Vector3(-0.34f, 1.3f, 0f));
            rig.RightArm = Pivot("Right arm", body, new Vector3(0.34f, 1.3f, 0f));
            foreach (Transform arm in new[] { rig.LeftArm, rig.RightArm })
            {
                float inward = arm == rig.LeftArm ? 0.07f : -0.07f;
                Shapes.Part("Manga", arm, PrimitiveType.Capsule, shirt,
                    new Vector3(inward, -0.08f, 0f), Vector3.zero, new Vector3(0.19f, 0.18f, 0.16f));
                Shapes.Part("Antebraço", arm, PrimitiveType.Capsule, skin,
                    new Vector3(0f, -0.32f, 0f), Vector3.zero, new Vector3(0.105f, 0.24f, 0.11f));
                Shapes.Part("Mão", arm, PrimitiveType.Sphere, skin,
                    new Vector3(0f, -0.51f, 0.025f), Vector3.zero, new Vector3(0.12f, 0.13f, 0.12f));
            }

            var shortsParts = new System.Collections.Generic.List<Renderer>();
            var accentParts = new System.Collections.Generic.List<Renderer>();
            var shirtParts = new System.Collections.Generic.List<Renderer>();
            foreach (Renderer renderer in rig.Root.GetComponentsInChildren<Renderer>())
            {
                if (renderer.sharedMaterial == shorts) shortsParts.Add(renderer);
                else if (renderer.sharedMaterial == accent) accentParts.Add(renderer);
                else if (renderer.sharedMaterial == shirt) shirtParts.Add(renderer);
            }
            rig.Armour = shortsParts.ToArray();
            rig.Trim = accentParts.ToArray();
            rig.Cloth = shirtParts.ToArray();
            return rig;
        }

        /// <summary>Applies a skin's clothing colours (one material per colour, shared by its parts).</summary>
        public static void Paint(KnightRig rig, Skin skin)
        {
            Apply(rig.Armour, Shapes.Recolored(art.Armour, Shapes.Hex(skin.Armour)));
            Apply(rig.Trim, Shapes.Recolored(art.Trim, Shapes.Hex(skin.Trim)));
            Apply(rig.Cloth, Shapes.Recolored(art.Cloth, Shapes.Hex(skin.Cape)));
        }

        static void Apply(Renderer[] renderers, Material material)
        {
            foreach (Renderer renderer in renderers) renderer.sharedMaterial = material;
        }

        static readonly System.Collections.Generic.Dictionary<Color, Material> glows = new System.Collections.Generic.Dictionary<Color, Material>();

        /// <summary>A shared additive glow material per colour.</summary>
        public static Material Glow(Color color)
        {
            if (!glows.TryGetValue(color, out Material material) || material == null)
            {
                material = Shapes.Tinted(color);
                glows[color] = material;
            }
            return material;
        }

        static Transform Pivot(string name, Transform parent, Vector3 position)
        {
            var pivot = new GameObject(name).transform;
            pivot.SetParent(parent, false);
            pivot.localPosition = position;
            return pivot;
        }

        // ---- Obstacles ----

        /// <summary>An obstacle standing on the floor at the origin of its lane. Axes return their swinging pendulum.</summary>
        public static Transform Obstacle(ObstacleKind kind, Transform parent, int seed, out Transform pendulum)
        {
            pendulum = null;
            var root = new GameObject(kind.ToString()).transform;
            root.SetParent(parent, false);
            float half = Tuning.LaneWidth / 2f - 0.15f;
            switch (kind)
            {
                case ObstacleKind.Hurdle:
                    // A wooden barricade with iron spikes along the top.
                    foreach (float x in new[] { -half + 0.1f, half - 0.1f })
                    {
                        Shapes.Part("Post", root, PrimitiveType.Cube, art.WoodDark, new Vector3(x, 0.45f, 0f), Vector3.zero, new Vector3(0.16f, 0.9f, 0.2f));
                    }
                    Shapes.Part("Plank", root, PrimitiveType.Cube, art.Wood, new Vector3(0f, 0.68f, 0f), new Vector3(0f, 0f, seed % 2 == 0 ? 2f : -2f), new Vector3(half * 2f, 0.24f, 0.12f));
                    Shapes.Part("Plank", root, PrimitiveType.Cube, art.Wood, new Vector3(0f, 0.3f, 0f), new Vector3(0f, 0f, seed % 2 == 0 ? -3f : 3f), new Vector3(half * 2f, 0.22f, 0.12f));
                    for (int i = 0; i < 6; i++)
                    {
                        float x = -half + 0.25f + i * (half * 2f - 0.5f) / 5f;
                        Shapes.Part("Spike", root, spike, art.Iron, new Vector3(x, 0.8f, 0f), Vector3.zero, Vector3.one);
                    }
                    break;
                case ObstacleKind.Beam:
                    // A spiked log hung across the lane at head height.
                    foreach (float x in new[] { -half, half })
                    {
                        Shapes.Part("Post", root, PrimitiveType.Cube, art.WoodDark, new Vector3(x, 1.5f, 0f), Vector3.zero, new Vector3(0.2f, 3f, 0.24f));
                    }
                    Shapes.Part("Log", root, PrimitiveType.Cylinder, art.Wood, new Vector3(0f, 1.4f, 0f), new Vector3(0f, 0f, 90f), new Vector3(0.42f, half, 0.42f));
                    Shapes.Part("Band", root, PrimitiveType.Cylinder, art.Iron, new Vector3(-half * 0.5f, 1.4f, 0f), new Vector3(0f, 0f, 90f), new Vector3(0.45f, 0.05f, 0.45f));
                    Shapes.Part("Band", root, PrimitiveType.Cylinder, art.Iron, new Vector3(half * 0.5f, 1.4f, 0f), new Vector3(0f, 0f, 90f), new Vector3(0.45f, 0.05f, 0.45f));
                    for (int i = 0; i < 5; i++)
                    {
                        float x = -half + 0.35f + i * (half * 2f - 0.7f) / 4f;
                        Shapes.Part("Spike", root, spike, art.Iron, new Vector3(x, 1.2f, 0f), new Vector3(180f, 0f, 0f), Vector3.one);
                    }
                    break;
                case ObstacleKind.Wall:
                    // A stack of crates as tall as a knight can never jump.
                    var random = new System.Random(seed);
                    for (int row = 0; row < 2; row++)
                    {
                        float y = 0.72f + row * 1.44f;
                        float turn = (float)(random.NextDouble() * 10 - 5);
                        Transform crate = Shapes.Part("Crate", root, PrimitiveType.Cube, row == 0 ? art.Wood : art.WoodDark, new Vector3(0f, y, 0f), new Vector3(0f, turn, 0f), new Vector3(half * 2f - 0.1f, 1.4f, 1.15f));
                        Shapes.Part("Strap", crate, PrimitiveType.Cube, art.Iron, new Vector3(0f, 0.3f, 0f), Vector3.zero, new Vector3(1.02f, 0.08f, 1.02f));
                        Shapes.Part("Strap", crate, PrimitiveType.Cube, art.Iron, new Vector3(0f, -0.3f, 0f), Vector3.zero, new Vector3(1.02f, 0.08f, 1.02f));
                        Shapes.Part("Cross", crate, PrimitiveType.Cube, art.WoodDark, new Vector3(0f, 0f, -0.51f), new Vector3(0f, 0f, 38f), new Vector3(1.1f, 0.1f, 0.02f));
                    }
                    break;
                case ObstacleKind.Axe:
                    // A gallows frame with a pendulum axe swinging across the lane.
                    foreach (float x in new[] { -half - 0.05f, half + 0.05f })
                    {
                        Shapes.Part("Post", root, PrimitiveType.Cube, art.WoodDark, new Vector3(x, 2f, 0f), Vector3.zero, new Vector3(0.24f, 4f, 0.28f));
                    }
                    Shapes.Part("Crossbar", root, PrimitiveType.Cube, art.WoodDark, new Vector3(0f, Run.AxePivot + 0.1f, 0f), Vector3.zero, new Vector3(half * 2f + 0.4f, 0.26f, 0.3f));
                    pendulum = new GameObject("Pendulum").transform;
                    pendulum.SetParent(root, false);
                    pendulum.localPosition = new Vector3(0f, Run.AxePivot, 0f);
                    Shapes.Part("Haft", pendulum, PrimitiveType.Cylinder, art.Iron, new Vector3(0f, -Run.AxeArm * 0.5f, 0f), Vector3.zero, new Vector3(0.08f, Run.AxeArm * 0.5f, 0.08f));
                    Shapes.Part("Head", pendulum, PrimitiveType.Cube, art.Iron, new Vector3(0f, -Run.AxeArm + 0.1f, 0f), Vector3.zero, new Vector3(0.24f, 0.5f, 0.2f));
                    Shapes.Part("Blade L", pendulum, blade, art.Steel, new Vector3(-0.1f, -Run.AxeArm + 0.1f, -0.04f), new Vector3(-90f, 0f, 90f), Vector3.one);
                    Shapes.Part("Blade R", pendulum, blade, art.Steel, new Vector3(0.1f, -Run.AxeArm + 0.1f, -0.04f), new Vector3(-90f, 0f, -90f), Vector3.one);
                    break;
            }
            return root;
        }

        // ---- Pickups ----

        public static Transform Pickup(PickupKind kind, Transform parent)
        {
            var root = new GameObject(kind.ToString()).transform;
            root.SetParent(parent, false);
            switch (kind)
            {
                case PickupKind.Coin:
                    Shapes.Part("Coin", root, PrimitiveType.Cylinder, art.Gold, Vector3.zero, new Vector3(90f, 0f, 0f), new Vector3(0.5f, 0.05f, 0.5f));
                    Shapes.Part("Mark", root, PrimitiveType.Cube, art.Gold, new Vector3(0f, 0f, 0f), Vector3.zero, new Vector3(0.12f, 0.26f, 0.12f));
                    break;
                case PickupKind.Gem:
                    Shapes.Part("Gem", root, gem, art.Gem, Vector3.zero, Vector3.zero, new Vector3(0.32f, 0.45f, 0.32f));
                    Shapes.Part("Glow", root, PrimitiveType.Quad, Glow(new Color(0.2f, 0.8f, 1f, 0.5f)), Vector3.zero, Vector3.zero, Vector3.one * 1.4f, shadows: false);
                    break;
                case PickupKind.Shield:
                    Shapes.Part("Disc", root, PrimitiveType.Cylinder, art.ShieldBlue, Vector3.zero, new Vector3(90f, 0f, 0f), new Vector3(0.7f, 0.06f, 0.7f));
                    Shapes.Part("Boss", root, PrimitiveType.Sphere, art.Steel, new Vector3(0f, 0f, 0.05f), Vector3.zero, new Vector3(0.22f, 0.22f, 0.12f));
                    Shapes.Part("Glow", root, PrimitiveType.Quad, Glow(new Color(0.3f, 0.6f, 1f, 0.5f)), Vector3.zero, Vector3.zero, Vector3.one * 1.8f, shadows: false);
                    break;
                case PickupKind.Magnet:
                    Shapes.Part("Bend", root, PrimitiveType.Cube, art.MagnetRed, new Vector3(0f, -0.22f, 0f), Vector3.zero, new Vector3(0.56f, 0.16f, 0.16f));
                    foreach (float x in new[] { -0.2f, 0.2f })
                    {
                        Shapes.Part("Arm", root, PrimitiveType.Cube, art.MagnetRed, new Vector3(x, 0.02f, 0f), Vector3.zero, new Vector3(0.16f, 0.4f, 0.16f));
                        Shapes.Part("Tip", root, PrimitiveType.Cube, art.Steel, new Vector3(x, 0.28f, 0f), Vector3.zero, new Vector3(0.17f, 0.14f, 0.17f));
                    }
                    Shapes.Part("Glow", root, PrimitiveType.Quad, Glow(new Color(1f, 0.3f, 0.3f, 0.5f)), Vector3.zero, Vector3.zero, Vector3.one * 1.8f, shadows: false);
                    break;
                default:
                    // Double points: a pair of overlapping purple coins.
                    Shapes.Part("Coin A", root, PrimitiveType.Cylinder, art.DoublePurple, new Vector3(-0.12f, 0.06f, 0f), new Vector3(90f, 0f, 0f), new Vector3(0.48f, 0.05f, 0.48f));
                    Shapes.Part("Coin B", root, PrimitiveType.Cylinder, art.DoublePurple, new Vector3(0.12f, -0.06f, -0.06f), new Vector3(90f, 0f, 0f), new Vector3(0.48f, 0.05f, 0.48f));
                    Shapes.Part("Glow", root, PrimitiveType.Quad, Glow(new Color(0.8f, 0.4f, 1f, 0.5f)), Vector3.zero, Vector3.zero, Vector3.one * 1.8f, shadows: false);
                    break;
            }
            return root;
        }

        // ---- Corridor ----

        public const float SegmentLength = 12f;
        public const float WallX = 4.6f;

        /// <summary>Twelve metres of castle corridor: flagstones, gold lane lines, walls, pillars, torches and banners.</summary>
        public static Transform Segment(Transform parent, int index)
        {
            var root = new GameObject("Rua " + index).transform;
            root.SetParent(parent, false);
            var random = new System.Random(index * 7919 + 17);
            float half = SegmentLength / 2f;
            // Keep the original segment, lane and side boundaries so camera and gameplay stay aligned.
            Shapes.Part("Asfalto", root, PrimitiveType.Cube, art.FloorDark, new Vector3(0f, -0.3f, half), Vector3.zero, new Vector3(WallX * 2f, 0.6f, SegmentLength));
            for (int lane = 0; lane < Tuning.LaneCount; lane++)
            {
                for (int row = 0; row < 4; row++)
                {
                    Material asphalt = random.Next(3) == 0 ? art.FloorDark : art.Floor;
                    Shapes.Part("Remendo", root, PrimitiveType.Cube, asphalt, new Vector3(Tuning.LaneX(lane), -0.04f, row * 3f + 1.5f),
                        Vector3.zero, new Vector3(Tuning.LaneWidth - 0.16f, 0.1f, 2.86f));
                }
            }
            foreach (float x in new[] { -Tuning.LaneWidth / 2f, Tuning.LaneWidth / 2f })
            {
                Shapes.Part("Faixa amarela", root, PrimitiveType.Cube, art.Trim, new Vector3(x, 0.01f, half), Vector3.zero, new Vector3(0.07f, 0.04f, SegmentLength), shadows: false);
            }
            foreach (float side in new[] { -1f, 1f })
            {
                float x = side * WallX;
                Shapes.Part("Calçada", root, PrimitiveType.Cube, art.WallDark, new Vector3(x - side * 0.15f, 0.15f, half), Vector3.zero, new Vector3(0.5f, 0.3f, SegmentLength));
                for (int p = 0; p < 2; p++)
                {
                    float z = p * 6f;
                    float height = 4.3f + (index + p) % 3 * 0.55f;
                    Material facade = (index + p) % 2 == 0 ? art.Wall : art.Banner;
                    Shapes.Part("Predio", root, PrimitiveType.Cube, facade, new Vector3(x + side * 1.2f, height * 0.5f, z + 2.8f), Vector3.zero, new Vector3(1.8f, height, 5.4f));
                    for (int window = 0; window < 2; window++)
                    {
                        Shapes.Part("Janela", root, PrimitiveType.Cube, art.WallDark,
                            new Vector3(x + side * 0.25f, 2.1f + window * 1.25f, z + 1.5f), Vector3.zero, new Vector3(0.035f, 0.55f, 0.7f), shadows: false);
                    }
                    if ((index + p + (side > 0 ? 1 : 0)) % 2 == 0)
                    {
                        Shapes.Part("Poste", root, PrimitiveType.Cylinder, art.Iron, new Vector3(x - side * 0.3f, 2.2f, z + 3f), Vector3.zero, new Vector3(0.08f, 2.2f, 0.08f));
                        Shapes.Part("Luminaria", root, PrimitiveType.Cube, art.TorchFlame, new Vector3(x - side * 0.3f, 4.35f, z + 3f), Vector3.zero, new Vector3(0.5f, 0.12f, 0.18f), shadows: false);
                    }
                    else
                    {
                        Shapes.Part("Placa", root, PrimitiveType.Cube, art.Trim, new Vector3(x - side * 0.24f, 3.1f, z + 3f), Vector3.zero, new Vector3(0.06f, 0.55f, 1.05f));
                    }
                }
            }
            return root;
        }
    }
}
