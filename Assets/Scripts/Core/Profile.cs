using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace GauntletRun.Core
{
    /// <summary>A look for Zé in the shop: shorts, accent and shirt colours (hex) and its price.</summary>
    public sealed class Skin
    {
        public Skin(string name, int price, string armour, string trim, string cape)
        {
            Name = name;
            Price = price;
            Armour = armour;
            Trim = trim;
            Cape = cape;
        }

        public string Name { get; }
        public int Price { get; }
        public string Armour { get; }
        public string Trim { get; }
        public string Cape { get; }
    }

    public static class Skins
    {
        public static readonly IReadOnlyList<Skin> All = new[]
        {
            new Skin("Zé de rua", 0, "#536b82", "#8291a0", "#f0eee7"),
            new Skin("Domingueira", 150, "#76634f", "#d3ba8b", "#d9d0bd"),
            new Skin("Chuva de verão", 300, "#3d7185", "#7caeb5", "#c7dce0"),
            new Skin("Uniforme da firma", 500, "#344f70", "#d0b65b", "#e5e0d2"),
            new Skin("Baile do bairro", 800, "#593d63", "#bd7f9a", "#ead3dc"),
            new Skin("Noite na cidade", 1200, "#303740", "#d6ad59", "#c8c2b8"),
        };
    }

    /// <summary>Everything kept between runs: records, wallet, owned looks and the volume settings.</summary>
    public sealed class Profile
    {
        public const int MaxVolume = 10;

        public int Best { get; set; }
        public int BestDistance { get; set; }
        public int Wallet { get; set; }
        /// <summary>Bit i is set when skin i is owned. The first skin is always owned.</summary>
        public int Owned { get; set; } = 1;
        public int Skin { get; set; }
        public int Music { get; set; } = 7;
        public int Effects { get; set; } = 8;
        public int Runs { get; set; }

        public bool Owns(int skin) => skin == 0 || (Owned & (1 << skin)) != 0;

        public Profile Copy() => Parse(Serialize());

        /// <summary>A short text form, e.g. <c>best=1200;wallet=40;owned=3;skin=1;music=7;effects=8;runs=5</c>.</summary>
        public string Serialize()
        {
            var text = new StringBuilder();
            text.Append("best=").Append(Best.ToString(CultureInfo.InvariantCulture));
            text.Append(";distance=").Append(BestDistance.ToString(CultureInfo.InvariantCulture));
            text.Append(";wallet=").Append(Wallet.ToString(CultureInfo.InvariantCulture));
            text.Append(";owned=").Append(Owned.ToString(CultureInfo.InvariantCulture));
            text.Append(";skin=").Append(Skin.ToString(CultureInfo.InvariantCulture));
            text.Append(";music=").Append(Music.ToString(CultureInfo.InvariantCulture));
            text.Append(";effects=").Append(Effects.ToString(CultureInfo.InvariantCulture));
            text.Append(";runs=").Append(Runs.ToString(CultureInfo.InvariantCulture));
            return text.ToString();
        }

        /// <summary>Reads <see cref="Serialize"/>'s text. Unknown or broken fields keep their defaults.</summary>
        public static Profile Parse(string text)
        {
            var profile = new Profile();
            if (string.IsNullOrEmpty(text))
            {
                return profile;
            }
            foreach (string pair in text.Split(';'))
            {
                int equals = pair.IndexOf('=');
                if (equals <= 0 || !int.TryParse(pair.Substring(equals + 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
                {
                    continue;
                }
                switch (pair.Substring(0, equals).Trim())
                {
                    case "best": profile.Best = Math.Max(0, value); break;
                    case "distance": profile.BestDistance = Math.Max(0, value); break;
                    case "wallet": profile.Wallet = Math.Max(0, value); break;
                    case "owned": profile.Owned = value < 0 ? 1 : (value & ((1 << Skins.All.Count) - 1)) | 1; break;
                    case "skin": profile.Skin = value; break;
                    case "music": profile.Music = Clamp(value, 0, MaxVolume); break;
                    case "effects": profile.Effects = Clamp(value, 0, MaxVolume); break;
                    case "runs": profile.Runs = Math.Max(0, value); break;
                }
            }
            if (profile.Skin < 0 || profile.Skin >= Skins.All.Count || !profile.Owns(profile.Skin))
            {
                profile.Skin = 0;
            }
            return profile;
        }

        static int Clamp(int value, int min, int max) => Math.Max(min, Math.Min(max, value));
    }

    /// <summary>Where the profile lives between sessions.</summary>
    public interface IProfileStore
    {
        Profile Load();
        void Save(Profile profile);
    }

    /// <summary>A store that forgets everything when the game closes (tests and staged scenes).</summary>
    public sealed class MemoryProfileStore : IProfileStore
    {
        string saved;

        public MemoryProfileStore(Profile start = null)
        {
            saved = start?.Serialize();
        }

        public int Saves { get; private set; }

        public Profile Load() => Profile.Parse(saved);

        public void Save(Profile profile)
        {
            saved = profile.Serialize();
            Saves++;
        }
    }
}
