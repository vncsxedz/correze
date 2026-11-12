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

        public static void Init(GameArt gameArt)
        {
            art = gameArt;
            spike = Shapes.Prism(4, 0.09f, 0f, 0.28f, seed: 1);
            gem = Shapes.Blob(2, 4, 0f, 2);
            blade = Shapes.Prism(3, 0.55f, 0.35f, 0.08f, seed: 3);
        }

        // ---- Zé ----

        public static KnightRig Knight(Transform parent)
        {
            var rig = new KnightRig { Root = new GameObject("Ze").transform };
            rig.Root.SetParent(parent, false);
            Shapes.Shadow(rig.Root, 0.5f);
            rig.Body = new GameObject("Body").transform;
            rig.Body.SetParent(rig.Root, false);
            Transform body = rig.Body;
            Material pants = art.Armour, shoe = art.Trim, shirt = art.Cloth;

            rig.LeftLeg = Pivot("Left leg", body, new Vector3(-0.17f, 0.82f, 0f));
            rig.RightLeg = Pivot("Right leg", body, new Vector3(0.17f, 0.82f, 0f));
            foreach (Transform leg in new[] { rig.LeftLeg, rig.RightLeg })
            {
                Shapes.Part("Calça", leg, PrimitiveType.Cube, pants, new Vector3(0f, -0.22f, 0f), Vector3.zero, new Vector3(0.19f, 0.44f, 0.22f));
                Shapes.Part("Kichute", leg, PrimitiveType.Cube, shoe, new Vector3(0f, -0.66f, 0.08f), Vector3.zero, new Vector3(0.27f, 0.16f, 0.38f));
                Shapes.Part("Sola", leg, PrimitiveType.Cube, art.WoodDark, new Vector3(0f, -0.74f, 0.08f), Vector3.zero, new Vector3(0.28f, 0.045f, 0.39f));
            }
            Shapes.Part("Camiseta", body, PrimitiveType.Capsule, shirt, new Vector3(0f, 1.16f, 0f), Vector3.zero, new Vector3(0.52f, 0.34f, 0.34f));
            Shapes.Part("Gola", body, PrimitiveType.Cylinder, shoe, new Vector3(0f, 1.44f, 0f), Vector3.zero, new Vector3(0.16f, 0.035f, 0.16f));
            Shapes.Part("Cabeça", body, PrimitiveType.Sphere, art.Gold, new Vector3(0f, 1.65f, 0.02f), Vector3.zero, new Vector3(0.34f, 0.38f, 0.34f));
            Shapes.Part("Cabelo", body, PrimitiveType.Sphere, art.WoodDark, new Vector3(0f, 1.79f, -0.04f), new Vector3(0f, 0f, 0f), new Vector3(0.36f, 0.19f, 0.36f));
            Shapes.Part("Olhos", body, PrimitiveType.Cube, art.Visor, new Vector3(0f, 1.68f, 0.185f), Vector3.zero, new Vector3(0.19f, 0.035f, 0.02f));
            Shapes.Part("Nariz", body, PrimitiveType.Sphere, art.Gold, new Vector3(0f, 1.61f, 0.2f), Vector3.zero, new Vector3(0.07f, 0.07f, 0.06f));

            rig.LeftArm = Pivot("Left arm", body, new Vector3(-0.32f, 1.32f, 0f));
            rig.RightArm = Pivot("Right arm", body, new Vector3(0.32f, 1.32f, 0f));
            foreach (Transform arm in new[] { rig.LeftArm, rig.RightArm })
            {
                Shapes.Part("Braço", arm, PrimitiveType.Capsule, shirt, new Vector3(0f, -0.25f, 0f), Vector3.zero, new Vector3(0.14f, 0.25f, 0.14f));
                Shapes.Part("Mão", arm, PrimitiveType.Sphere, art.Gold, new Vector3(0f, -0.51f, 0.02f), Vector3.zero, new Vector3(0.13f, 0.13f, 0.13f));
            }
            rig.Cape = Pivot("Mochila", body, new Vector3(0f, 1.22f, -0.2f));
            Shapes.Part("Mochila", rig.Cape, PrimitiveType.Cube, shoe, new Vector3(0f, -0.12f, -0.06f), Vector3.zero, new Vector3(0.34f, 0.42f, 0.16f));

            var armourParts = new System.Collections.Generic.List<Renderer>();
            var trimParts = new System.Collections.Generic.List<Renderer>();
            var clothParts = new System.Collections.Generic.List<Renderer>();
            foreach (Renderer renderer in rig.Root.GetComponentsInChildren<Renderer>())
            {
                if (renderer.sharedMaterial == pants) armourParts.Add(renderer);
                else if (renderer.sharedMaterial == shoe) trimParts.Add(renderer);
                else if (renderer.sharedMaterial == shirt) clothParts.Add(renderer);
            }
            rig.Armour = armourParts.ToArray();
            rig.Trim = trimParts.ToArray();
            rig.Cloth = clothParts.ToArray();
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
            var root = new GameObject("Segment " + index).transform;
            root.SetParent(parent, false);
            var random = new System.Random(index * 7919 + 17);
            float half = SegmentLength / 2f;
            Shapes.Part("Slab", root, PrimitiveType.Cube, art.FloorDark, new Vector3(0f, -0.3f, half), Vector3.zero, new Vector3(WallX * 2f, 0.6f, SegmentLength));
            for (int lane = 0; lane < Tuning.LaneCount; lane++)
            {
                for (int row = 0; row < 4; row++)
                {
                    Material stone = random.Next(3) == 0 ? art.FloorDark : art.Floor;
                    float lift = (float)random.NextDouble() * 0.02f;
                    Shapes.Part("Flag", root, PrimitiveType.Cube, stone, new Vector3(Tuning.LaneX(lane), -0.04f + lift, row * 3f + 1.5f),
                        new Vector3(0f, (float)(random.NextDouble() * 2 - 1), 0f), new Vector3(Tuning.LaneWidth - 0.16f, 0.1f, 2.86f));
                }
            }
            foreach (float x in new[] { -Tuning.LaneWidth / 2f, Tuning.LaneWidth / 2f })
            {
                Shapes.Part("Lane line", root, PrimitiveType.Cube, art.Trim, new Vector3(x, 0.01f, half), Vector3.zero, new Vector3(0.07f, 0.04f, SegmentLength), shadows: false);
            }
            foreach (float side in new[] { -1f, 1f })
            {
                float x = side * WallX;
                Shapes.Part("Wall", root, PrimitiveType.Cube, art.Wall, new Vector3(x + side * 0.4f, 2.5f, half), Vector3.zero, new Vector3(0.8f, 5f, SegmentLength));
                Shapes.Part("Kerb", root, PrimitiveType.Cube, art.WallDark, new Vector3(x - side * 0.15f, 0.15f, half), Vector3.zero, new Vector3(0.5f, 0.3f, SegmentLength));
                for (int p = 0; p < 2; p++)
                {
                    float z = p * 6f;
                    Shapes.Part("Pillar", root, PrimitiveType.Cube, art.WallDark, new Vector3(x - side * 0.1f, 2.7f, z), Vector3.zero, new Vector3(0.9f, 5.4f, 0.9f));
                    Shapes.Part("Capital", root, PrimitiveType.Cube, art.Wall, new Vector3(x - side * 0.15f, 5.3f, z), Vector3.zero, new Vector3(1.1f, 0.3f, 1.1f));
                }
                if ((index + (side > 0 ? 1 : 0)) % 2 == 0)
                {
                    // A torch on the wall between the pillars.
                    Transform torch = Shapes.Part("Torch", root, PrimitiveType.Cylinder, art.WoodDark, new Vector3(x - side * 0.15f, 2.4f, 3f), new Vector3(0f, 0f, side * 25f), new Vector3(0.1f, 0.3f, 0.1f));
                    Shapes.Part("Flame", root, PrimitiveType.Sphere, art.TorchFlame, new Vector3(x - side * 0.3f, 2.8f, 3f), Vector3.zero, new Vector3(0.22f, 0.32f, 0.22f), shadows: false);
                    Shapes.Part("Glow", root, PrimitiveType.Quad, Glow(new Color(1f, 0.55f, 0.2f, 0.35f)), new Vector3(x - side * 0.35f, 2.8f, 3f), new Vector3(0f, side * 90f, 0f), Vector3.one * 1.6f, shadows: false);
                    torch.name = "Torch";
                }
                else
                {
                    // A hanging banner.
                    Shapes.Part("Banner", root, PrimitiveType.Cube, art.Banner, new Vector3(x - side * 0.05f, 3.2f, 3f), Vector3.zero, new Vector3(0.04f, 2f, 1.1f));
                    Shapes.Part("Banner pole", root, PrimitiveType.Cube, art.Trim, new Vector3(x - side * 0.08f, 4.25f, 3f), Vector3.zero, new Vector3(0.08f, 0.08f, 1.3f));
                    Shapes.Part("Emblem", root, PrimitiveType.Cube, art.Trim, new Vector3(x - side * 0.03f, 3.4f, 3f), new Vector3(45f, 0f, 0f), new Vector3(0.05f, 0.4f, 0.4f));
                }
            }
            // Arches over the corridor every other segment.
            if (index % 2 == 0)
            {
                Shapes.Part("Arch", root, PrimitiveType.Cube, art.WallDark, new Vector3(0f, 5.7f, 0f), Vector3.zero, new Vector3(WallX * 2f + 1.6f, 0.6f, 0.9f));
            }
            return root;
        }
    }
}
