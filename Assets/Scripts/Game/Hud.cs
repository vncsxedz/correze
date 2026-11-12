using System;
using GauntletRun.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Screen = GauntletRun.Core.Screen;

namespace GauntletRun.Game
{
    /// <summary>
    /// The UI Toolkit HUD and menus, built in code and styled by Hud.uss. Buttons never take
    /// keyboard focus: the director reads keys itself.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class Hud : MonoBehaviour
    {
        VisualElement root;
        VisualElement play;
        Label score;
        Label distance;
        Label coins;
        Label gems;
        Label multiplier;
        readonly VisualElement[] powerChips = new VisualElement[3];
        readonly VisualElement[] powerFills = new VisualElement[3];
        Label banner;
        Label hint;
        float bannerTime;
        float hintTime;

        VisualElement title;
        Label titleRecords;
        VisualElement pause;
        VisualElement settings;
        Label musicValue;
        Label effectsValue;
        VisualElement musicBar;
        VisualElement effectsBar;
        VisualElement shop;
        Label shopWallet;
        readonly Button[] skinCards = new Button[6];
        readonly Label[] skinStates = new Label[6];
        VisualElement over;
        Label overCause;
        Label overScore;
        Label overBest;
        Label overDetails;
        Label overNewBest;

        public event Action PlayClicked;
        public event Action ShopClicked;
        public event Action SettingsClicked;
        public event Action PauseClicked;
        public event Action ResumeClicked;
        public event Action RestartClicked;
        public event Action MenuClicked;
        public event Action BackClicked;
        public event Action<int> SkinClicked;
        public event Action<int> MusicStep;
        public event Action<int> EffectsStep;

        /// <summary>Which full-screen layer is showing, for tests.</summary>
        public string Showing { get; private set; } = "";

        public void Init()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            root.Clear();
            root.AddToClassList("hud");
            root.pickingMode = PickingMode.Ignore;
            BuildPlay();
            BuildTitle();
            BuildPause();
            BuildSettings();
            BuildShop();
            BuildOver();
        }

        // ---- Building ----

        static VisualElement Box(string classes, VisualElement parent, bool picks = false)
        {
            var element = new VisualElement { pickingMode = picks ? PickingMode.Position : PickingMode.Ignore };
            foreach (string name in classes.Split(' ')) if (name.Length > 0) element.AddToClassList(name);
            parent?.Add(element);
            return element;
        }

        static Label Text(string text, string classes, VisualElement parent)
        {
            var label = new Label(text) { pickingMode = PickingMode.Ignore };
            foreach (string name in classes.Split(' ')) if (name.Length > 0) label.AddToClassList(name);
            parent.Add(label);
            return label;
        }

        static Button Click(string text, string classes, VisualElement parent, Action onClick)
        {
            var button = new Button(() => onClick?.Invoke()) { text = text, focusable = false };
            button.AddToClassList("button");
            foreach (string name in classes.Split(' ')) if (name.Length > 0) button.AddToClassList(name);
            parent.Add(button);
            return button;
        }

        VisualElement Layer(string classes) => Box("layer " + classes, root);

        void BuildPlay()
        {
            play = Layer("play");
            VisualElement left = Box("corner corner--left", play);
            score = Text("0", "score", left);
            distance = Text("0 m", "distance", left);
            multiplier = Text("x2 PONTOS", "multiplier hidden", left);

            VisualElement right = Box("corner corner--right", play);
            VisualElement purse = Box("row", right);
            Box("dot dot--coin", purse);
            coins = Text("0", "count", purse);
            Box("dot dot--gem", purse);
            gems = Text("0", "count", purse);
            Click("II", "button--square", purse, () => PauseClicked?.Invoke());
            string[] names = { "ESCUDO", "ÍMÃ", "x2" };
            string[] kinds = { "shield", "magnet", "double" };
            for (int i = 0; i < 3; i++)
            {
                VisualElement chip = Box("chip chip--" + kinds[i] + " hidden", right);
                Text(names[i], "chip__name", chip);
                VisualElement track = Box("chip__track", chip);
                powerFills[i] = Box("chip__fill", track);
                powerChips[i] = chip;
            }

            banner = Text("", "banner hidden", play);
            hint = Text("Deslize para os lados: trocar de faixa  ·  para cima: pular  ·  para baixo: deslizar", "hint hidden", play);
        }

        void BuildTitle()
        {
            title = Layer("menu menu--title");
            VisualElement card = Box("title-card", title, true);
            Text("CORRE, ZÉ!", "logo", card);
            Text("Zé disparou pela cidade! Desvie dos obstáculos, pule, deslize e pegue as moedas.", "tagline", card);
            Click("Correr", "button--big", card, () => PlayClicked?.Invoke());
            VisualElement row = Box("row row--center", card);
            Click("Visuais", "button--wide-half", row, () => ShopClicked?.Invoke());
            Click("Configurações", "button--wide-half", row, () => SettingsClicked?.Invoke());
            titleRecords = Text("", "records", card);
            Text("Enter: correr  ·  Tab: visuais  ·  Esc: pausar", "note", card);
        }

        void BuildPause()
        {
            pause = Layer("menu menu--dim");
            VisualElement card = Box("panel panel--center", pause, true);
            Text("Pausado", "panel__title", card);
            Click("Continuar", "button--wide button--primary", card, () => ResumeClicked?.Invoke());
            Click("Settings", "button--wide", card, () => SettingsClicked?.Invoke());
            Click("Recomeçar", "button--wide", card, () => RestartClicked?.Invoke());
            Click("Voltar ao início", "button--wide button--danger", card, () => MenuClicked?.Invoke());
        }

        void BuildSettings()
        {
            settings = Layer("menu menu--dim");
            VisualElement card = Box("panel panel--center", settings, true);
            Text("Settings", "panel__title", card);
            Volume(card, "Música", step => MusicStep?.Invoke(step), out musicBar, out musicValue);
            Volume(card, "Efeitos", step => EffectsStep?.Invoke(step), out effectsBar, out effectsValue);
            Click("Voltar", "button--wide button--primary", card, () => BackClicked?.Invoke());
        }

        static void Volume(VisualElement parent, string name, Action<int> step, out VisualElement bar, out Label value)
        {
            VisualElement row = Box("row volume", parent);
            Text(name, "volume__name", row);
            Click("-", "button--square", row, () => step(-1));
            VisualElement track = Box("volume__track", row);
            bar = Box("volume__fill", track);
            Click("+", "button--square", row, () => step(1));
            value = Text("", "volume__value", row);
        }

        void BuildShop()
        {
            shop = Layer("menu menu--shop");
            VisualElement card = Box("panel panel--shop", shop, true);
            Text("Loja de visuais", "panel__title", card);
            shopWallet = Text("", "wallet", card);
            VisualElement grid = Box("grid", card);
            for (int i = 0; i < Skins.All.Count; i++)
            {
                int index = i;
                Skin skin = Skins.All[i];
                Button button = Click("", "skin", grid, () => SkinClicked?.Invoke(index));
                VisualElement swatches = Box("row", button);
                foreach (string color in new[] { skin.Armour, skin.Trim, skin.Cape })
                {
                    VisualElement swatch = Box("swatch", swatches);
                    swatch.style.backgroundColor = Shapes.Hex(color);
                }
                Text(skin.Name, "skin__name", button);
                skinStates[i] = Text("", "skin__state", button);
                skinCards[i] = button;
            }
            Click("Back", "button--wide button--primary", card, () => BackClicked?.Invoke());
        }

        void BuildOver()
        {
            over = Layer("menu menu--dim");
            VisualElement card = Box("panel panel--center panel--over", over, true);
            Text("Fim de corrida", "panel__title", card);
            overCause = Text("", "note", card);
            overScore = Text("", "result-score", card);
            overNewBest = Text("NOVO RECORDE!", "new-best", card);
            overBest = Text("", "records", card);
            overDetails = Text("", "details", card);
            Click("Correr de novo", "button--wide button--primary", card, () => RestartClicked?.Invoke());
            VisualElement row = Box("row row--center", card);
            Click("Shop", "button--wide-half", row, () => ShopClicked?.Invoke());
            Click("Início", "button--wide-half", row, () => MenuClicked?.Invoke());
            Text("Enter: correr de novo  ·  Tab: visuais  ·  Esc: início", "note", card);
        }

        // ---- Updating ----

        public void Show(Screen screen, Session session)
        {
            Showing = screen.ToString();
            Set(play, screen == Screen.Playing || screen == Screen.Paused || (screen == Screen.Settings && session.Run != null));
            Set(title, screen == Screen.Title);
            Set(pause, screen == Screen.Paused);
            Set(settings, screen == Screen.Settings);
            Set(shop, screen == Screen.Shop);
            Set(over, screen == Screen.GameOver);
            Profile profile = session.Profile;
            titleRecords.text = $"Best {profile.Best:N0}   ·   Furthest {profile.BestDistance:N0} m   ·   Purse {profile.Wallet:N0} coins";
            if (screen == Screen.Playing)
            {
                hintTime = profile.Runs < 3 ? 6f : 0f;
            }
            if (screen == Screen.GameOver && session.LastResult != null)
            {
                RunResult result = session.LastResult;
                overCause.text = result.KilledBy switch
                {
                    ObstacleKind.Hurdle => "Zé tropeçou na barreira",
                    ObstacleKind.Beam => "Zé bateu na placa baixa",
                    ObstacleKind.Wall => "Zé bateu no bloqueio",
                    ObstacleKind.Axe => "Zé foi atingido pelo obstáculo",
                    _ => "",
                };
                overScore.text = result.Score.ToString("N0");
                Set(overNewBest, result.NewBest);
                overBest.text = $"Best {profile.Best:N0}";
                overDetails.text = $"{result.Distance:N0} m   ·   {result.Coins} coins   ·   {result.Gems} gems   ·   +{result.Earned} to the purse ({profile.Wallet:N0})";
            }
            RefreshSettings(profile);
            RefreshShop(profile);
        }

        public void RefreshSettings(Profile profile)
        {
            musicBar.style.width = Length.Percent(profile.Music * 10f);
            effectsBar.style.width = Length.Percent(profile.Effects * 10f);
            musicValue.text = (profile.Music * 10) + "%";
            effectsValue.text = (profile.Effects * 10) + "%";
        }

        public void RefreshShop(Profile profile)
        {
            shopWallet.text = $"Purse: {profile.Wallet:N0} coins";
            for (int i = 0; i < Skins.All.Count; i++)
            {
                bool owned = profile.Owns(i);
                bool worn = profile.Skin == i;
                skinStates[i].text = worn ? "Em uso" : owned ? "Adquirido: usar" : $"{Skins.All[i].Price:N0} moedas";
                skinCards[i].EnableInClassList("skin--worn", worn);
                skinCards[i].EnableInClassList("skin--locked", !owned && profile.Wallet < Skins.All[i].Price);
            }
        }

        public void ShowBanner(string text, float seconds = 1.6f)
        {
            banner.text = text;
            bannerTime = seconds;
            Set(banner, true);
        }

        /// <summary>Updates the in-run readouts.</summary>
        public void Tick(Run run, float dt)
        {
            if (run == null)
            {
                return;
            }
            score.text = run.Score.ToString("N0");
            distance.text = $"{run.Distance:N0} m";
            coins.text = run.Coins.ToString();
            gems.text = run.Gems.ToString();
            Set(multiplier, run.Multiplier > 1);
            Power(0, run.ShieldLeft, Tuning.ShieldTime);
            Power(1, run.MagnetLeft, Tuning.MagnetTime);
            Power(2, run.DoubleLeft, Tuning.DoubleTime);
            if (bannerTime > 0f)
            {
                bannerTime -= dt;
                if (bannerTime <= 0f) Set(banner, false);
            }
            hintTime -= dt;
            Set(hint, hintTime > 0f);
        }

        void Power(int index, float left, float total)
        {
            Set(powerChips[index], left > 0f);
            powerFills[index].style.width = Length.Percent(Mathf.Clamp01(left / total) * 100f);
        }

        static void Set(VisualElement element, bool shown) => element.EnableInClassList("hidden", !shown);
    }
}
