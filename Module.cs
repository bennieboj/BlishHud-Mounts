using Blish_HUD;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Blish_HUD.Controls.Extern;
using Blish_HUD.Input;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;


namespace Manlaan.Mounts
{
    [Export(typeof(Blish_HUD.Modules.Module))]
    public class Module : Blish_HUD.Modules.Module
    {
        private static readonly Logger Logger = Logger.GetLogger<Module>();

        #region Service Managers
        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;
        #endregion


        public enum MountOrder { Disabled, _1, _2, _3, _4, _5, _6, _7, _8, _9, _10 }
        public enum MountDisplay { TransparentCorner, OpaqueCorner, TransparentManual, OpaqueManual, OpaqueManualText }
        public enum Orientation { Horizontal, Vertical }
        private SettingEntry<MountOrder> _settingGriffonOrder;
        private SettingEntry<MountOrder> _settingJackelOrder;
        private SettingEntry<MountOrder> _settingRaptorOrder;
        private SettingEntry<MountOrder> _settingRollerOrder;
        private SettingEntry<MountOrder> _settingSkimmerOrder;
        private SettingEntry<MountOrder> _settingSkyscaleOrder;
        private SettingEntry<MountOrder> _settingSpringerOrder;
        private SettingEntry<MountOrder> _settingWarclawOrder;
        private SettingEntry<KeyBinding> _settingGriffonBinding;
        private SettingEntry<KeyBinding> _settingJackelBinding;
        private SettingEntry<KeyBinding> _settingRaptorBinding;
        private SettingEntry<KeyBinding> _settingRollerBinding;
        private SettingEntry<KeyBinding> _settingSkimmerBinding;
        private SettingEntry<KeyBinding> _settingSkyscaleBinding;
        private SettingEntry<KeyBinding> _settingSpringerBinding;
        private SettingEntry<KeyBinding> _settingWarclawBinding;
        private SettingEntry<MountDisplay> _settingDisplay;
        private SettingEntry<Orientation> _settingOrientation;
        private SettingEntry<string> _settingLocX;
        private SettingEntry<string> _settingLocY;
        private SettingEntry<int> _settingImgWidth;
        private SettingEntry<float> _settingOpacity;

        private CornerIcon _cornerGriffon;
        private CornerIcon _cornerJackel;
        private CornerIcon _cornerRaptor;
        private CornerIcon _cornerRoller;
        private CornerIcon _cornerSkimmer;
        private CornerIcon _cornerSkyscale;
        private CornerIcon _cornerSpringer;
        private CornerIcon _cornerWarclaw;

        private Image _btnGriffon;
        private Image _btnJackel;
        private Image _btnRaptor;
        private Image _btnRoller;
        private Image _btnSkimmer;
        private Image _btnSkyscale;
        private Image _btnSpringer;
        private Image _btnWarclaw;

        [ImportingConstructor]
        public Module([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) { }

        protected override void Initialize()
        {
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            _settingRaptorOrder = settings.DefineSetting("MountRaptorOrder", MountOrder._1, "Raptor Order", "");
            _settingSpringerOrder = settings.DefineSetting("MountSpringerOrder", MountOrder._2, "Springer Order", "");
            _settingSkimmerOrder = settings.DefineSetting("MountSkimmerOrder", MountOrder._3, "Skimmer Order", "");
            _settingJackelOrder = settings.DefineSetting("MountJackelOrder", MountOrder._4, "Jackel Order", "");
            _settingGriffonOrder = settings.DefineSetting("MountGriffonOrder", MountOrder._5, "Griffon Order", "");
            _settingRollerOrder = settings.DefineSetting("MountRollerOrder", MountOrder._6, "Roller Order", "");
            _settingWarclawOrder = settings.DefineSetting("MountWarclawOrder", MountOrder._7, "Warclaw Order", "");
            _settingSkyscaleOrder = settings.DefineSetting("MountSkyscaleOrder", MountOrder._8, "Skyscale Order", "");

            _settingRaptorBinding = settings.DefineSetting("MountRaptorBinding", new KeyBinding(Keys.None), "Raptor Binding", "");
            _settingSpringerBinding = settings.DefineSetting("MountSpringerBinding", new KeyBinding(Keys.None), "Springer Binding", "");
            _settingSkimmerBinding = settings.DefineSetting("MountSkimmerBinding", new KeyBinding(Keys.None), "Skimmer Binding", "");
            _settingJackelBinding = settings.DefineSetting("MountJackelBinding", new KeyBinding(Keys.None), "Jackel Binding", "");
            _settingGriffonBinding = settings.DefineSetting("MountGriffonBinding", new KeyBinding(Keys.None), "Griffon Binding", "");
            _settingRollerBinding = settings.DefineSetting("MountRollerBinding", new KeyBinding(Keys.None), "Roller Binding", "");
            _settingWarclawBinding = settings.DefineSetting("MountWarclawBinding", new KeyBinding(Keys.None), "Warclaw Binding", "");
            _settingSkyscaleBinding = settings.DefineSetting("MountSkyscaleBinding", new KeyBinding(Keys.None), "Skyscale Binding", "");

            _settingDisplay = settings.DefineSetting("MountDisplay", MountDisplay.TransparentCorner, "Display Type", "");
            _settingOrientation = settings.DefineSetting("Orientation", Orientation.Horizontal, "Manual Orientation", "");
            _settingLocX = settings.DefineSetting("MountLocX", "60", "Manual X", "");
            _settingLocY = settings.DefineSetting("MountLocY", "40", "Manual Y", "");
            _settingImgWidth = settings.DefineSetting("MountImgWidth", 30, "Manual Icon Width", "");
            _settingOpacity = settings.DefineSetting("MountOpacity", 1.0f, "Manual Opacity", "");

            _settingImgWidth.SetRange(0, 200);
            _settingOpacity.SetRange(0f, 1f);

            _settingGriffonOrder.SettingChanged += UpdateSettings;
            _settingJackelOrder.SettingChanged += UpdateSettings;
            _settingRaptorOrder.SettingChanged += UpdateSettings;
            _settingRollerOrder.SettingChanged += UpdateSettings;
            _settingSkimmerOrder.SettingChanged += UpdateSettings;
            _settingSkyscaleOrder.SettingChanged += UpdateSettings;
            _settingSpringerOrder.SettingChanged += UpdateSettings;
            _settingWarclawOrder.SettingChanged += UpdateSettings;

            _settingGriffonBinding.SettingChanged += UpdateSettings;
            _settingJackelBinding.SettingChanged += UpdateSettings;
            _settingRaptorBinding.SettingChanged += UpdateSettings;
            _settingRollerBinding.SettingChanged += UpdateSettings;
            _settingSkimmerBinding.SettingChanged += UpdateSettings;
            _settingSkyscaleBinding.SettingChanged += UpdateSettings;
            _settingSpringerBinding.SettingChanged += UpdateSettings;
            _settingWarclawBinding.SettingChanged += UpdateSettings;

            _settingDisplay.SettingChanged += UpdateSettings;
            _settingOrientation.SettingChanged += UpdateSettings;
            _settingLocX.SettingChanged += UpdateSettings;
            _settingLocY.SettingChanged += UpdateSettings;
            _settingImgWidth.SettingChanged += UpdateSettings;
            _settingOpacity.SettingChanged += UpdateSettings;

        }

        protected override async Task LoadAsync()
        {

        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            DrawIcons();

            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        protected override void Update(GameTime gameTime)
        {
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            _cornerGriffon?.Dispose();
            _cornerJackel?.Dispose();
            _cornerRaptor?.Dispose();
            _cornerRoller?.Dispose();
            _cornerSkimmer?.Dispose();
            _cornerSkyscale?.Dispose();
            _cornerSpringer?.Dispose();
            _cornerWarclaw?.Dispose();

            _btnGriffon?.Dispose();
            _btnJackel?.Dispose();
            _btnRaptor?.Dispose();
            _btnRoller?.Dispose();
            _btnSkimmer?.Dispose();
            _btnSkyscale?.Dispose();
            _btnSpringer?.Dispose();
            _btnWarclaw?.Dispose();

            _settingGriffonOrder.SettingChanged -= UpdateSettings;
            _settingJackelOrder.SettingChanged -= UpdateSettings;
            _settingRaptorOrder.SettingChanged -= UpdateSettings;
            _settingRollerOrder.SettingChanged -= UpdateSettings;
            _settingSkimmerOrder.SettingChanged -= UpdateSettings;
            _settingSkyscaleOrder.SettingChanged -= UpdateSettings;
            _settingSpringerOrder.SettingChanged -= UpdateSettings;
            _settingWarclawOrder.SettingChanged -= UpdateSettings;

            _settingGriffonBinding.SettingChanged -= UpdateSettings;
            _settingJackelBinding.SettingChanged -= UpdateSettings;
            _settingRaptorBinding.SettingChanged -= UpdateSettings;
            _settingRollerBinding.SettingChanged -= UpdateSettings;
            _settingSkimmerBinding.SettingChanged -= UpdateSettings;
            _settingSkyscaleBinding.SettingChanged -= UpdateSettings;
            _settingSpringerBinding.SettingChanged -= UpdateSettings;
            _settingWarclawBinding.SettingChanged -= UpdateSettings;

            _settingDisplay.SettingChanged -= UpdateSettings;
            _settingOrientation.SettingChanged -= UpdateSettings;
            _settingLocX.SettingChanged -= UpdateSettings;
            _settingLocY.SettingChanged -= UpdateSettings;
            _settingImgWidth.SettingChanged -= UpdateSettings;
            _settingOpacity.SettingChanged -= UpdateSettings;
        }

        private void UpdateSettings(object sender = null, ValueChangedEventArgs<KeyBinding> e = null)
        {
            DrawIcons();
        }
        private void UpdateSettings(object sender = null, ValueChangedEventArgs<MountDisplay> e = null)
        {
            DrawIcons();
        }
        private void UpdateSettings(object sender = null, ValueChangedEventArgs<MountOrder> e = null)
        {
            DrawIcons();
        }
        private void UpdateSettings(object sender = null, ValueChangedEventArgs<Orientation> e = null)
        {
            DrawIcons();
        }
        private void UpdateSettings(object sender = null, ValueChangedEventArgs<float> e = null)
        {
            DrawIcons();
        }
        private void UpdateSettings(object sender = null, ValueChangedEventArgs<string> e = null)
        {
            if (int.Parse(_settingLocX.Value) < 0)
                _settingLocX.Value = "0";
            if (int.Parse(_settingLocY.Value) < 0)
                _settingLocY.Value = "0";

            DrawIcons();
        }
        private void UpdateSettings(object sender = null, ValueChangedEventArgs<int> e = null)
        {
            DrawIcons();
        }

        protected void DrawIcons()
        {
            _cornerGriffon?.Dispose();
            _cornerJackel?.Dispose();
            _cornerRaptor?.Dispose();
            _cornerRoller?.Dispose();
            _cornerSkimmer?.Dispose();
            _cornerSkyscale?.Dispose();
            _cornerSpringer?.Dispose();
            _cornerWarclaw?.Dispose();

            _btnGriffon?.Dispose();
            _btnJackel?.Dispose();
            _btnRaptor?.Dispose();
            _btnRoller?.Dispose();
            _btnSkimmer?.Dispose();
            _btnSkyscale?.Dispose();
            _btnSpringer?.Dispose();
            _btnWarclaw?.Dispose();

            int curX = int.Parse(_settingLocX.Value);
            int curY = int.Parse(_settingLocY.Value);

            foreach (string name in Enum.GetNames(typeof(MountOrder)))
            {
                if (name.Equals(MountOrder.Disabled.ToString())) continue;
                if (_settingGriffonOrder.Value.ToString() == name)
                {
                    Texture2D img = GetImgFile("griffon");
                    if (_settingDisplay.Value == MountDisplay.OpaqueCorner || _settingDisplay.Value == MountDisplay.TransparentCorner)
                    {
                        _cornerGriffon = new CornerIcon()
                        {
                            IconName = "Griffon",
                            Icon = img,
                            HoverIcon = img,
                            Priority = 10
                        };
                        _cornerGriffon.Click += delegate { DoHotKey(_settingGriffonBinding); };
                    }
                    else
                    {
                        _btnGriffon = new Image
                        {
                            Parent = Blish_HUD.GameService.Graphics.SpriteScreen,
                            Texture = img,
                            Size = new Point(_settingImgWidth.Value, _settingImgWidth.Value),
                            Location = new Point(curX, curY),
                            Opacity = _settingOpacity.Value,
                            BasicTooltipText = "Griffon"
                        };
                        _btnGriffon.LeftMouseButtonPressed += delegate { DoHotKey(_settingGriffonBinding); };

                        if (_settingOrientation.Value == Orientation.Horizontal)
                            curX += _settingImgWidth.Value;
                        else
                            curY += _settingImgWidth.Value;
                    }
                }
                if (_settingJackelOrder.Value.ToString() == name)
                {
                    Texture2D img = GetImgFile("jackel");
                    if (_settingDisplay.Value == MountDisplay.OpaqueCorner || _settingDisplay.Value == MountDisplay.TransparentCorner)
                    {
                        _cornerJackel = new CornerIcon()
                        {
                            IconName = "Jackel",
                            Icon = img,
                            HoverIcon = img,
                            Priority = 10
                        };
                        _cornerJackel.Click += delegate { DoHotKey(_settingJackelBinding); };
                    }
                    else
                    {
                        _btnJackel = new Image
                        {
                            Parent = Blish_HUD.GameService.Graphics.SpriteScreen,
                            Texture = img,
                            Size = new Point(_settingImgWidth.Value, _settingImgWidth.Value),
                            Location = new Point(curX, curY),
                            Opacity = _settingOpacity.Value,
                            BasicTooltipText = "Jackel"
                        };
                        _btnJackel.LeftMouseButtonPressed += delegate { DoHotKey(_settingJackelBinding); };

                        if (_settingOrientation.Value == Orientation.Horizontal)
                            curX += _settingImgWidth.Value;
                        else
                            curY += _settingImgWidth.Value;
                    }
                }
                if (_settingRaptorOrder.Value.ToString() == name)
                {
                    Texture2D img = GetImgFile("raptor");
                    if (_settingDisplay.Value == MountDisplay.OpaqueCorner || _settingDisplay.Value == MountDisplay.TransparentCorner)
                    {
                        _cornerRaptor = new CornerIcon()
                        {
                            IconName = "Raptor",
                            Icon = img,
                            HoverIcon = img,
                            Priority = 10
                        };
                        _cornerRaptor.Click += delegate { DoHotKey(_settingRaptorBinding); };
                    }
                    else
                    {
                        _btnRaptor = new Image
                        {
                            Parent = Blish_HUD.GameService.Graphics.SpriteScreen,
                            Texture = img,
                            Size = new Point(_settingImgWidth.Value, _settingImgWidth.Value),
                            Location = new Point(curX, curY),
                            Opacity = _settingOpacity.Value,
                            BasicTooltipText = "Raptor"
                        };
                        _btnRaptor.LeftMouseButtonPressed += delegate { DoHotKey(_settingRaptorBinding); };

                        if (_settingOrientation.Value == Orientation.Horizontal)
                            curX += _settingImgWidth.Value;
                        else
                            curY += _settingImgWidth.Value;
                    }
                }
                if (_settingRollerOrder.Value.ToString() == name)
                {
                    Texture2D img = GetImgFile("roller");
                    if (_settingDisplay.Value == MountDisplay.OpaqueCorner || _settingDisplay.Value == MountDisplay.TransparentCorner)
                    {
                        _cornerRoller = new CornerIcon()
                        {
                            IconName = "Roller",
                            Icon = img,
                            HoverIcon = img,
                            Priority = 10
                        };
                        _cornerRoller.Click += delegate { DoHotKey(_settingRollerBinding); };
                    }
                    else
                    {
                        _btnRoller = new Image
                        {
                            Parent = Blish_HUD.GameService.Graphics.SpriteScreen,
                            Texture = img,
                            Size = new Point(_settingImgWidth.Value, _settingImgWidth.Value),
                            Location = new Point(curX, curY),
                            Opacity = _settingOpacity.Value,
                            BasicTooltipText = "Roller"
                        };
                        _btnRoller.LeftMouseButtonPressed += delegate { DoHotKey(_settingRollerBinding); };

                        if (_settingOrientation.Value == Orientation.Horizontal)
                            curX += _settingImgWidth.Value;
                        else
                            curY += _settingImgWidth.Value;
                    }
                }
                if (_settingSkimmerOrder.Value.ToString() == name)
                {
                    Texture2D img = GetImgFile("skimmer");
                    if (_settingDisplay.Value == MountDisplay.OpaqueCorner || _settingDisplay.Value == MountDisplay.TransparentCorner)
                    {
                        _cornerSkimmer = new CornerIcon()
                        {
                            IconName = "Skimmer",
                            Icon = img,
                            HoverIcon = img,
                            Priority = 10
                        };
                        _cornerSkimmer.Click += delegate { DoHotKey(_settingSkimmerBinding); };
                    }
                    else
                    {
                        _btnSkimmer = new Image
                        {
                            Parent = Blish_HUD.GameService.Graphics.SpriteScreen,
                            Texture = img,
                            Size = new Point(_settingImgWidth.Value, _settingImgWidth.Value),
                            Location = new Point(curX, curY),
                            Opacity = _settingOpacity.Value,
                            BasicTooltipText = "Skimmer"
                        };
                        _btnSkimmer.LeftMouseButtonPressed += delegate { DoHotKey(_settingSkimmerBinding); };

                        if (_settingOrientation.Value == Orientation.Horizontal)
                            curX += _settingImgWidth.Value;
                        else
                            curY += _settingImgWidth.Value;
                    }
                }
                if (_settingSkyscaleOrder.Value.ToString() == name)
                {
                    Texture2D img = GetImgFile("skyscale");
                    if (_settingDisplay.Value == MountDisplay.OpaqueCorner || _settingDisplay.Value == MountDisplay.TransparentCorner)
                    {
                        _cornerSkyscale = new CornerIcon()
                        {
                            IconName = "Skyscale",
                            Icon = img,
                            HoverIcon = img,
                            Priority = 10
                        };
                        _cornerSkyscale.Click += delegate { DoHotKey(_settingSkyscaleBinding); };
                    }
                    else
                    {
                        _btnSkyscale = new Image
                        {
                            Parent = Blish_HUD.GameService.Graphics.SpriteScreen,
                            Texture = img,
                            Size = new Point(_settingImgWidth.Value, _settingImgWidth.Value),
                            Location = new Point(curX, curY),
                            Opacity = _settingOpacity.Value,
                            BasicTooltipText = "Skyscale"
                        };
                        _btnSkyscale.LeftMouseButtonPressed += delegate { DoHotKey(_settingSkyscaleBinding); };

                        if (_settingOrientation.Value == Orientation.Horizontal)
                            curX += _settingImgWidth.Value;
                        else
                            curY += _settingImgWidth.Value;
                    }
                }
                if (_settingSpringerOrder.Value.ToString() == name)
                {
                    Texture2D img = GetImgFile("springer");
                    if (_settingDisplay.Value == MountDisplay.OpaqueCorner || _settingDisplay.Value == MountDisplay.TransparentCorner)
                    {
                        _cornerSpringer = new CornerIcon()
                        {
                            IconName = "Springer",
                            Icon = img,
                            HoverIcon = img,
                            Priority = 10
                        };
                        _cornerSpringer.Click += delegate { DoHotKey(_settingSpringerBinding); };
                    }
                    else
                    {
                        _btnSpringer = new Image
                        {
                            Parent = Blish_HUD.GameService.Graphics.SpriteScreen,
                            Texture = img,
                            Size = new Point(_settingImgWidth.Value, _settingImgWidth.Value),
                            Location = new Point(curX, curY),
                            Opacity = _settingOpacity.Value,
                            BasicTooltipText = "Springer"
                        };
                        _btnSpringer.LeftMouseButtonPressed += delegate { DoHotKey(_settingSpringerBinding); };

                        if (_settingOrientation.Value == Orientation.Horizontal)
                            curX += _settingImgWidth.Value;
                        else
                            curY += _settingImgWidth.Value;
                    }
                }
                if (_settingWarclawOrder.Value.ToString() == name)
                {
                    Texture2D img = GetImgFile("warclaw");
                    if (_settingDisplay.Value == MountDisplay.OpaqueCorner || _settingDisplay.Value == MountDisplay.TransparentCorner)
                    {
                        _cornerWarclaw = new CornerIcon()
                        {
                            IconName = "Warclaw",
                            Icon = img,
                            HoverIcon = img,
                            Priority = 10
                        };
                        _cornerWarclaw.Click += delegate { DoHotKey(_settingWarclawBinding); };
                    }
                    else
                    {
                        _btnWarclaw = new Image
                        {
                            Parent = Blish_HUD.GameService.Graphics.SpriteScreen,
                            Texture = img,
                            Size = new Point(_settingImgWidth.Value, _settingImgWidth.Value),
                            Location = new Point(curX, curY),
                            Opacity = _settingOpacity.Value,
                            BasicTooltipText = "Warclaw"
                        };
                        _btnWarclaw.LeftMouseButtonPressed += delegate { DoHotKey(_settingWarclawBinding); };

                        if (_settingOrientation.Value == Orientation.Horizontal)
                            curX += _settingImgWidth.Value;
                        else
                            curY += _settingImgWidth.Value;
                    }
                }
            }
        }

        private Texture2D GetImgFile(string filename)
        {
            switch (_settingDisplay.Value)
            {
                case MountDisplay.OpaqueManual:
                    return ContentsManager.GetTexture(filename + ".png");
                case MountDisplay.OpaqueCorner:
                    return ContentsManager.GetTexture(filename + ".png");

                case MountDisplay.TransparentManual:
                    return ContentsManager.GetTexture(filename + "-trans.png");
                case MountDisplay.TransparentCorner:
                    return ContentsManager.GetTexture(filename + "-trans.png");

                case MountDisplay.OpaqueManualText:
                    return ContentsManager.GetTexture(filename + "-text.png");

                default:
                    return ContentsManager.GetTexture(filename + ".png");
            }
        }
        protected void DoHotKey(SettingEntry<KeyBinding> setting)
        {
            if (setting.Value.ModifierKeys != ModifierKeys.None)
            {
                if (setting.Value.ModifierKeys.HasFlag(ModifierKeys.Alt))
                    Blish_HUD.Controls.Intern.Keyboard.Press(VirtualKeyShort.MENU, true);
                if (setting.Value.ModifierKeys.HasFlag(ModifierKeys.Ctrl))
                    Blish_HUD.Controls.Intern.Keyboard.Press(VirtualKeyShort.CONTROL, true);
                if (setting.Value.ModifierKeys.HasFlag(ModifierKeys.Shift))
                    Blish_HUD.Controls.Intern.Keyboard.Press(VirtualKeyShort.SHIFT, true);
            }
            Blish_HUD.Controls.Intern.Keyboard.Press(ToVirtualKey(setting.Value.PrimaryKey), true);
            System.Threading.Thread.Sleep(50);
            Blish_HUD.Controls.Intern.Keyboard.Release(ToVirtualKey(setting.Value.PrimaryKey), true);
            if (setting.Value.ModifierKeys != ModifierKeys.None)
            {
                if (setting.Value.ModifierKeys.HasFlag(ModifierKeys.Shift))
                    Blish_HUD.Controls.Intern.Keyboard.Release(VirtualKeyShort.SHIFT, true);
                if (setting.Value.ModifierKeys.HasFlag(ModifierKeys.Ctrl))
                    Blish_HUD.Controls.Intern.Keyboard.Release(VirtualKeyShort.CONTROL, true);
                if (setting.Value.ModifierKeys.HasFlag(ModifierKeys.Alt))
                    Blish_HUD.Controls.Intern.Keyboard.Release(VirtualKeyShort.MENU, true);
            }
        }
        private VirtualKeyShort ToVirtualKey(Keys key)
        {
            foreach (VirtualKeyShort vkey in Enum.GetValues(typeof(VirtualKeyShort)))
            {
                if ((int)vkey == (int)key)
                    return vkey;
            }
            return new VirtualKeyShort();
            /*
            switch (key)
            {
                case Keys.A:
                    return VirtualKeyShort.KEY_A;
                case Keys.B:
                    return VirtualKeyShort.KEY_B;
                case Keys.C:
                    return VirtualKeyShort.KEY_C;
                case Keys.D:
                    return VirtualKeyShort.KEY_D;
                case Keys.E:
                    return VirtualKeyShort.KEY_E;
                case Keys.F:
                    return VirtualKeyShort.KEY_F;
                case Keys.G:
                    return VirtualKeyShort.KEY_G;
                case Keys.H:
                    return VirtualKeyShort.KEY_H;
                case Keys.I:
                    return VirtualKeyShort.KEY_I;
                case Keys.J:
                    return VirtualKeyShort.KEY_J;
                case Keys.K:
                    return VirtualKeyShort.KEY_K;
                case Keys.L:
                    return VirtualKeyShort.KEY_L;
                case Keys.M:
                    return VirtualKeyShort.KEY_M;
                case Keys.N:
                    return VirtualKeyShort.KEY_N;
                case Keys.O:
                    return VirtualKeyShort.KEY_O;
                case Keys.P:
                    return VirtualKeyShort.KEY_P;
                case Keys.Q:
                    return VirtualKeyShort.KEY_Q;
                case Keys.R:
                    return VirtualKeyShort.KEY_R;
                case Keys.S:
                    return VirtualKeyShort.KEY_S;
                case Keys.T:
                    return VirtualKeyShort.KEY_T;
                case Keys.U:
                    return VirtualKeyShort.KEY_U;
                case Keys.V:
                    return VirtualKeyShort.KEY_V;
                case Keys.W:
                    return VirtualKeyShort.KEY_W;
                case Keys.X:
                    return VirtualKeyShort.KEY_X;
                case Keys.Y:
                    return VirtualKeyShort.KEY_Y;
                case Keys.Z:
                    return VirtualKeyShort.KEY_Z;
                case Keys.D0:
                    return VirtualKeyShort.KEY_0;
                case Keys.D1:
                    return VirtualKeyShort.KEY_1;
                case Keys.D2:
                    return VirtualKeyShort.KEY_2;
                case Keys.D3:
                    return VirtualKeyShort.KEY_3;
                case Keys.D4:
                    return VirtualKeyShort.KEY_4;
                case Keys.D5:
                    return VirtualKeyShort.KEY_5;
                case Keys.D6:
                    return VirtualKeyShort.KEY_6;
                case Keys.D7:
                    return VirtualKeyShort.KEY_7;
                case Keys.D8:
                    return VirtualKeyShort.KEY_8;
                case Keys.D9:
                    return VirtualKeyShort.KEY_9;
                case Keys.F1:
                    return VirtualKeyShort.F1;
                case Keys.F2:
                    return VirtualKeyShort.F2;
                case Keys.F3:
                    return VirtualKeyShort.F3;
                case Keys.F4:
                    return VirtualKeyShort.F4;
                case Keys.F5:
                    return VirtualKeyShort.F5;
                case Keys.F6:
                    return VirtualKeyShort.F6;
                case Keys.F7:
                    return VirtualKeyShort.F7;
                case Keys.F8:
                    return VirtualKeyShort.F8;
                case Keys.F9:
                    return VirtualKeyShort.F9;
                case Keys.F10:
                    return VirtualKeyShort.F10;
                case Keys.F11:
                    return VirtualKeyShort.F11;
                case Keys.F12:
                    return VirtualKeyShort.F12;
                case Keys.Add:
                    return VirtualKeyShort.ADD;
                case Keys.CapsLock:
                    return VirtualKeyShort.CAPITAL;
                case Keys.Decimal:
                    return VirtualKeyShort.DECIMAL;
                case Keys.Delete:
                    return VirtualKeyShort.DELETE;
                case Keys.Divide:
                    return VirtualKeyShort.DIVIDE;
                case Keys.Down:
                    return VirtualKeyShort.DOWN;
                case Keys.End:
                    return VirtualKeyShort.END;
                case Keys.Enter:
                    return VirtualKeyShort.RETURN;
                case Keys.Escape:
                    return VirtualKeyShort.ESCAPE;
                case Keys.Home:
                    return VirtualKeyShort.HOME;
                case Keys.Insert:
                    return VirtualKeyShort.INSERT;
                case Keys.Left:
                    return VirtualKeyShort.LEFT;
                case Keys.LeftShift:
                    return VirtualKeyShort.LSHIFT;
                case Keys.LeftControl:
                    return VirtualKeyShort.LCONTROL;
                case Keys.LeftWindows:
                    return VirtualKeyShort.LWIN;
                case Keys.Multiply:
                    return VirtualKeyShort.MULTIPLY;
                case Keys.NumLock:
                    return VirtualKeyShort.NUMLOCK;
                case Keys.NumPad0:
                    return VirtualKeyShort.NUMPAD0;
                case Keys.NumPad1:
                    return VirtualKeyShort.NUMPAD1;
                case Keys.NumPad2:
                    return VirtualKeyShort.NUMPAD2;
                case Keys.NumPad3:
                    return VirtualKeyShort.NUMPAD3;
                case Keys.NumPad4:
                    return VirtualKeyShort.NUMPAD4;
                case Keys.NumPad5:
                    return VirtualKeyShort.NUMPAD5;
                case Keys.NumPad6:
                    return VirtualKeyShort.NUMPAD6;
                case Keys.NumPad7:
                    return VirtualKeyShort.NUMPAD7;
                case Keys.NumPad8:
                    return VirtualKeyShort.NUMPAD8;
                case Keys.NumPad9:
                    return VirtualKeyShort.NUMPAD9;
                case Keys.OemComma:
                    return VirtualKeyShort.OEM_COMMA;
                case Keys.OemMinus:
                    return VirtualKeyShort.OEM_MINUS;
                case Keys.OemPeriod:
                    return VirtualKeyShort.OEM_PERIOD;
                case Keys.OemPlus:
                    return VirtualKeyShort.OEM_PLUS;
                case Keys.Right:
                    return VirtualKeyShort.RIGHT;
                case Keys.RightControl:
                    return VirtualKeyShort.RCONTROL;
                case Keys.RightShift:
                    return VirtualKeyShort.RSHIFT;
                case Keys.RightWindows:
                    return VirtualKeyShort.RWIN;
                case Keys.Space:
                    return VirtualKeyShort.SPACE;
                case Keys.Subtract:
                    return VirtualKeyShort.SUBTRACT;
                case Keys.Tab:
                    return VirtualKeyShort.TAB;
                case Keys.Up:
                    return VirtualKeyShort.UP;

                case Keys.LeftAlt:
                    return VirtualKeyShort.LMENU;
                case Keys.RightAlt:
                    return VirtualKeyShort.RMENU;
                case Keys.PageDown:
                    return VirtualKeyShort.NEXT;
                case Keys.PageUp:
                    return VirtualKeyShort.PRIOR;
                case Keys.OemQuestion:
                    return VirtualKeyShort.OEM_2;
                case Keys.OemQuotes:
                    return VirtualKeyShort.OEM_7;
                case Keys.OemSemicolon:
                    return VirtualKeyShort.OEM_1;
                case Keys.OemTilde:
                    return VirtualKeyShort.OEM_3;
                case Keys.OemPipe:
                    return VirtualKeyShort.OEM_5;
                case Keys.OemBackslash:
                    return VirtualKeyShort.OEM_5;
                case Keys.OemCloseBrackets:
                    return VirtualKeyShort.OEM_6;
                case Keys.OemOpenBrackets:
                    return VirtualKeyShort.OEM_4;
            }
            return new VirtualKeyShort();
            */
        }

    }

}
