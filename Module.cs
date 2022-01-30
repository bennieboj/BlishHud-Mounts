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
using Blish_HUD.Graphics.UI;
using System.Collections.Generic;

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


        public static int[] _mountOrder = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        public static string[] _mountDisplay = new string[] { "Transparent Corner", "Solid Corner", "Transparent Manual", "Solid Manual", "Solid Manual Text" };
        public static string[] _mountOrientation = new string[] { "Horizontal", "Vertical" };
        private SettingEntry<KeyBinding> InputQueuingKeybindSetting = null;
        public static string[] _defaultMountChoices = new string[] { "Disabled", "Raptor", "Springer", "Skimmer", "Jackal", "Griffon", "Roller Beetle", "Skyscale", "Warclaw" };
        Dictionary<string, Action> defaultMountChoicesToActions;
        Gw2Sharp.Models.MapType[] warclawOnlyMaps = {
                Gw2Sharp.Models.MapType.RedBorderlands,
                Gw2Sharp.Models.MapType.RedHome,
                Gw2Sharp.Models.MapType.BlueBorderlands,
                Gw2Sharp.Models.MapType.BlueHome,
                Gw2Sharp.Models.MapType.GreenBorderlands,
                Gw2Sharp.Models.MapType.GreenHome,
                Gw2Sharp.Models.MapType.EternalBattlegrounds,
                Gw2Sharp.Models.MapType.Center,
            };

        public static SettingEntry<int> _settingGriffonOrder;
        public static SettingEntry<int> _settingJackalOrder;
        public static SettingEntry<int> _settingRaptorOrder;
        public static SettingEntry<int> _settingRollerOrder;
        public static SettingEntry<int> _settingSkimmerOrder;
        public static SettingEntry<int> _settingSkyscaleOrder;
        public static SettingEntry<int> _settingSpringerOrder;
        public static SettingEntry<int> _settingWarclawOrder;
        public static SettingEntry<string> _settingDefaultMountChoice;
        public static SettingEntry<KeyBinding> _settingGriffonBinding;
        public static SettingEntry<KeyBinding> _settingJackalBinding;
        public static SettingEntry<KeyBinding> _settingRaptorBinding;
        public static SettingEntry<KeyBinding> _settingRollerBinding;
        public static SettingEntry<KeyBinding> _settingSkimmerBinding;
        public static SettingEntry<KeyBinding> _settingSkyscaleBinding;
        public static SettingEntry<KeyBinding> _settingSpringerBinding;
        public static SettingEntry<KeyBinding> _settingWarclawBinding;
        public static SettingEntry<KeyBinding> _settingDefaultMountBinding;
        public static SettingEntry<string> _settingDisplay;
        public static SettingEntry<string> _settingOrientation;
        private SettingEntry<Point> _settingLoc;
        public static SettingEntry<bool> _settingDrag;
        public static SettingEntry<int> _settingImgWidth;
        public static SettingEntry<float> _settingOpacity;

        private Panel _mountPanel;
        private CornerIcon _cornerGriffon;
        private CornerIcon _cornerJackal;
        private CornerIcon _cornerRaptor;
        private CornerIcon _cornerRoller;
        private CornerIcon _cornerSkimmer;
        private CornerIcon _cornerSkyscale;
        private CornerIcon _cornerSpringer;
        private CornerIcon _cornerWarclaw;

        private bool _dragging;
        private Point _dragStart = Point.Zero;

        [ImportingConstructor]
        public Module([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) { }

        protected override void Initialize()
        {
            InitializeDefaultMountActions();
            GameService.Gw2Mumble.PlayerCharacter.IsInCombatChanged += (sender, e) => HandleCombatChange(sender, e);
        }

        private void InitializeDefaultMountActions()
        {
            defaultMountChoicesToActions = new Dictionary<string, Action>();
            var defaultMountActions = new Action[]
            {
                () =>{ },
                () =>{ DoHotKey(_settingRaptorBinding); },
                () =>{ DoHotKey(_settingSpringerBinding); },
                () =>{ DoHotKey(_settingSkimmerBinding); },
                () =>{ DoHotKey(_settingJackalBinding); },
                () =>{ DoHotKey(_settingGriffonBinding); },
                () =>{ DoHotKey(_settingRollerBinding); },
                () =>{ DoHotKey(_settingSkyscaleBinding); },
                () =>{ DoHotKey(_settingWarclawBinding); }
            };
            for (int index = 0; index < defaultMountActions.Length; index++)
            {
                defaultMountChoicesToActions.Add(_defaultMountChoices[index], defaultMountActions[index]);
            }
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            _settingRaptorOrder = settings.DefineSetting("MountRaptorOrder2", 1, "Raptor Order", "");
            _settingSpringerOrder = settings.DefineSetting("MountSpringerOrder2", 2, "Springer Order", "");
            _settingSkimmerOrder = settings.DefineSetting("MountSkimmerOrder2", 3, "Skimmer Order", "");
            _settingJackalOrder = settings.DefineSetting("MountJackalOrder2", 4, "Jackal Order", "");
            _settingGriffonOrder = settings.DefineSetting("MountGriffonOrder2", 5, "Griffon Order", "");
            _settingRollerOrder = settings.DefineSetting("MountRollerOrder2", 6, "Roller Order", "");
            _settingWarclawOrder = settings.DefineSetting("MountWarclawOrder2", 7, "Warclaw Order", "");
            _settingSkyscaleOrder = settings.DefineSetting("MountSkyscaleOrder2", 8, "Skyscale Order", "");
            _settingDefaultMountChoice = settings.DefineSetting("DefaultMountChoice", "Disabled", "Default Mount Choice", "");

            _settingRaptorBinding = settings.DefineSetting("MountRaptorBinding", new KeyBinding(Keys.None), "Raptor Binding", "");
            _settingSpringerBinding = settings.DefineSetting("MountSpringerBinding", new KeyBinding(Keys.None), "Springer Binding", "");
            _settingSkimmerBinding = settings.DefineSetting("MountSkimmerBinding", new KeyBinding(Keys.None), "Skimmer Binding", "");
            _settingJackalBinding = settings.DefineSetting("MountJackalBinding", new KeyBinding(Keys.None), "Jackal Binding", "");
            _settingGriffonBinding = settings.DefineSetting("MountGriffonBinding", new KeyBinding(Keys.None), "Griffon Binding", "");
            _settingRollerBinding = settings.DefineSetting("MountRollerBinding", new KeyBinding(Keys.None), "Roller Binding", "");
            _settingWarclawBinding = settings.DefineSetting("MountWarclawBinding", new KeyBinding(Keys.None), "Warclaw Binding", "");
            _settingSkyscaleBinding = settings.DefineSetting("MountSkyscaleBinding", new KeyBinding(Keys.None), "Skyscale Binding", "");
            _settingDefaultMountBinding = settings.DefineSetting("DefaultMountBinding", new KeyBinding(Keys.None), "Default Mount Binding", "");

            _settingDisplay = settings.DefineSetting("MountDisplay", "Transparent Corner", "Display Type", "");
            _settingOrientation = settings.DefineSetting("Orientation", "Horizontal", "Manual Orientation", "");
            _settingLoc = settings.DefineSetting("MountLoc", new Point(100,100), "Window Location", "");
            _settingDrag = settings.DefineSetting("MountDrag", false, "Enable Dragging (White Box)", "");
            _settingImgWidth = settings.DefineSetting("MountImgWidth", 50, "Manual Icon Width", "");
            _settingOpacity = settings.DefineSetting("MountOpacity", 1.0f, "Manual Opacity", "");

            _settingImgWidth.SetRange(0, 200);
            _settingOpacity.SetRange(0f, 1f);

            _settingGriffonOrder.SettingChanged += UpdateSettings;
            _settingJackalOrder.SettingChanged += UpdateSettings;
            _settingRaptorOrder.SettingChanged += UpdateSettings;
            _settingRollerOrder.SettingChanged += UpdateSettings;
            _settingSkimmerOrder.SettingChanged += UpdateSettings;
            _settingSkyscaleOrder.SettingChanged += UpdateSettings;
            _settingSpringerOrder.SettingChanged += UpdateSettings;
            _settingWarclawOrder.SettingChanged += UpdateSettings;

            _settingGriffonBinding.SettingChanged += UpdateSettings;
            _settingJackalBinding.SettingChanged += UpdateSettings;
            _settingRaptorBinding.SettingChanged += UpdateSettings;
            _settingRollerBinding.SettingChanged += UpdateSettings;
            _settingSkimmerBinding.SettingChanged += UpdateSettings;
            _settingSkyscaleBinding.SettingChanged += UpdateSettings;
            _settingSpringerBinding.SettingChanged += UpdateSettings;
            _settingWarclawBinding.SettingChanged += UpdateSettings;
            _settingDefaultMountBinding.SettingChanged += UpdateSettings;
            _settingDefaultMountBinding.Value.Enabled = true;
            _settingDefaultMountBinding.Value.Activated += delegate { DoDefaultMountAction(); };

            _settingDisplay.SettingChanged += UpdateSettings;
            _settingOrientation.SettingChanged += UpdateSettings;
            _settingLoc.SettingChanged += UpdateSettings;
            _settingDrag.SettingChanged += UpdateSettings;
            _settingImgWidth.SettingChanged += UpdateSettings;
            _settingOpacity.SettingChanged += UpdateSettings;

        }
        public override IView GetSettingsView() {
            return new Mounts.Views.SettingsView();
            //return new SettingsView( (this.ModuleParameters.SettingsManager.ModuleSettings);
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
            if(_mountPanel != null)
            {
                if (GameService.GameIntegration.Gw2Instance.IsInGame && !GameService.Gw2Mumble.UI.IsMapOpen)
                {
                    _mountPanel.Show();
                }
                else
                {
                    _mountPanel.Hide();
                }
                if (_dragging)
                {
                    var nOffset = InputService.Input.Mouse.Position - _dragStart;
                    _mountPanel.Location += nOffset;

                    _dragStart = InputService.Input.Mouse.Position;
                }
            }
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            _cornerGriffon?.Dispose();
            _cornerJackal?.Dispose();
            _cornerRaptor?.Dispose();
            _cornerRoller?.Dispose();
            _cornerSkimmer?.Dispose();
            _cornerSkyscale?.Dispose();
            _cornerSpringer?.Dispose();
            _cornerWarclaw?.Dispose();

            _mountPanel?.Dispose();

            _settingGriffonOrder.SettingChanged -= UpdateSettings;
            _settingJackalOrder.SettingChanged -= UpdateSettings;
            _settingRaptorOrder.SettingChanged -= UpdateSettings;
            _settingRollerOrder.SettingChanged -= UpdateSettings;
            _settingSkimmerOrder.SettingChanged -= UpdateSettings;
            _settingSkyscaleOrder.SettingChanged -= UpdateSettings;
            _settingSpringerOrder.SettingChanged -= UpdateSettings;
            _settingWarclawOrder.SettingChanged -= UpdateSettings;

            _settingGriffonBinding.SettingChanged -= UpdateSettings;
            _settingJackalBinding.SettingChanged -= UpdateSettings;
            _settingRaptorBinding.SettingChanged -= UpdateSettings;
            _settingRollerBinding.SettingChanged -= UpdateSettings;
            _settingSkimmerBinding.SettingChanged -= UpdateSettings;
            _settingSkyscaleBinding.SettingChanged -= UpdateSettings;
            _settingSpringerBinding.SettingChanged -= UpdateSettings;
            _settingWarclawBinding.SettingChanged -= UpdateSettings;
            _settingDefaultMountBinding.SettingChanged -= UpdateSettings;

            _settingDisplay.SettingChanged -= UpdateSettings;
            _settingOrientation.SettingChanged -= UpdateSettings;
            _settingLoc.SettingChanged -= UpdateSettings;
            _settingDrag.SettingChanged -= UpdateSettings;
            _settingImgWidth.SettingChanged -= UpdateSettings;
            _settingOpacity.SettingChanged -= UpdateSettings;
        }

        private void UpdateSettings(object sender = null, ValueChangedEventArgs<string> e = null) {
            DrawIcons();
        }
        private void UpdateSettings(object sender = null, ValueChangedEventArgs<KeyBinding> e = null) {
            DrawIcons();
        }
        private void UpdateSettings(object sender = null, ValueChangedEventArgs<Point> e = null) {
            DrawIcons();
        }
        private void UpdateSettings(object sender = null, ValueChangedEventArgs<bool> e = null) {
            DrawIcons();
        }
        private void UpdateSettings(object sender = null, ValueChangedEventArgs<float> e = null)
        {
            DrawIcons();
        }
        private void UpdateSettings(object sender = null, ValueChangedEventArgs<int> e = null)
        {
            DrawIcons();
        }

        public void DrawManualIcons() {
            int curX = 0;
            int curY = 0;
            int totalMounts = 0;

            _mountPanel = new Panel() {
                Parent = Blish_HUD.GameService.Graphics.SpriteScreen,
                Location = _settingLoc.Value,
                Size = new Point(_settingImgWidth.Value * 8, _settingImgWidth.Value * 8),
            };

            foreach (int i in _mountOrder) {
                if (i == 0) continue;
                if (_settingGriffonOrder.Value == i) {
                    Texture2D img = GetImgFile("griffon");
                    Image _btnGriffon = new Image {
                        Parent = _mountPanel,
                        Texture = img,
                        Size = new Point(_settingImgWidth.Value, _settingImgWidth.Value),
                        Location = new Point(curX, curY),
                        Opacity = _settingOpacity.Value,
                        BasicTooltipText = "Griffon"
                    };
                    _btnGriffon.LeftMouseButtonPressed += delegate { DoHotKey(_settingGriffonBinding); };

                    if (_settingOrientation.Value.Equals("Horizontal"))
                        curX += _settingImgWidth.Value;
                    else
                        curY += _settingImgWidth.Value;

                    totalMounts++;
                }
                if (_settingJackalOrder.Value == i) {
                    Texture2D img = GetImgFile("jackal");
                    Image _btnJackal = new Image {
                        Parent = _mountPanel,
                        Texture = img,
                        Size = new Point(_settingImgWidth.Value, _settingImgWidth.Value),
                        Location = new Point(curX, curY),
                        Opacity = _settingOpacity.Value,
                        BasicTooltipText = "Jackal"
                    };
                    _btnJackal.LeftMouseButtonPressed += delegate { DoHotKey(_settingJackalBinding); };

                    if (_settingOrientation.Value.Equals("Horizontal"))
                        curX += _settingImgWidth.Value;
                    else
                        curY += _settingImgWidth.Value;

                    totalMounts++;
                }
                if (_settingRaptorOrder.Value == i) {
                    Texture2D img = GetImgFile("raptor");
                    Image _btnRaptor = new Image {
                        Parent = _mountPanel,
                        Texture = img,
                        Size = new Point(_settingImgWidth.Value, _settingImgWidth.Value),
                        Location = new Point(curX, curY),
                        Opacity = _settingOpacity.Value,
                        BasicTooltipText = "Raptor"
                    };
                    _btnRaptor.LeftMouseButtonPressed += delegate { DoHotKey(_settingRaptorBinding); };

                    if (_settingOrientation.Value.Equals("Horizontal"))
                        curX += _settingImgWidth.Value;
                    else
                        curY += _settingImgWidth.Value;

                    totalMounts++;
                }
                if (_settingRollerOrder.Value == i) {
                    Texture2D img = GetImgFile("roller");
                    Image _btnRoller = new Image {
                        Parent = _mountPanel,
                        Texture = img,
                        Size = new Point(_settingImgWidth.Value, _settingImgWidth.Value),
                        Location = new Point(curX, curY),
                        Opacity = _settingOpacity.Value,
                        BasicTooltipText = "Roller"
                    };
                    _btnRoller.LeftMouseButtonPressed += delegate { DoHotKey(_settingRollerBinding); };

                    if (_settingOrientation.Value.Equals("Horizontal"))
                        curX += _settingImgWidth.Value;
                    else
                        curY += _settingImgWidth.Value;

                    totalMounts++;
                }
                if (_settingSkimmerOrder.Value == i) {
                    Texture2D img = GetImgFile("skimmer");
                    Image _btnSkimmer = new Image {
                        Parent = _mountPanel,
                        Texture = img,
                        Size = new Point(_settingImgWidth.Value, _settingImgWidth.Value),
                        Location = new Point(curX, curY),
                        Opacity = _settingOpacity.Value,
                        BasicTooltipText = "Skimmer"
                    };
                    _btnSkimmer.LeftMouseButtonPressed += delegate { DoHotKey(_settingSkimmerBinding); };

                    if (_settingOrientation.Value.Equals("Horizontal"))
                        curX += _settingImgWidth.Value;
                    else
                        curY += _settingImgWidth.Value;

                    totalMounts++;
                }
                if (_settingSkyscaleOrder.Value == i) {
                    Texture2D img = GetImgFile("skyscale");
                    Image _btnSkyscale = new Image {
                        Parent = _mountPanel,
                        Texture = img,
                        Size = new Point(_settingImgWidth.Value, _settingImgWidth.Value),
                        Location = new Point(curX, curY),
                        Opacity = _settingOpacity.Value,
                        BasicTooltipText = "Skyscale"
                    };
                    _btnSkyscale.LeftMouseButtonPressed += delegate { DoHotKey(_settingSkyscaleBinding); };

                    if (_settingOrientation.Value.Equals("Horizontal"))
                        curX += _settingImgWidth.Value;
                    else
                        curY += _settingImgWidth.Value;

                    totalMounts++;
                }
                if (_settingSpringerOrder.Value == i) {
                    Texture2D img = GetImgFile("springer");
                    Image _btnSpringer = new Image {
                        Parent = _mountPanel,
                        Texture = img,
                        Size = new Point(_settingImgWidth.Value, _settingImgWidth.Value),
                        Location = new Point(curX, curY),
                        Opacity = _settingOpacity.Value,
                        BasicTooltipText = "Springer"
                    };
                    _btnSpringer.LeftMouseButtonPressed += delegate { DoHotKey(_settingSpringerBinding); };

                    if (_settingOrientation.Value.Equals("Horizontal"))
                        curX += _settingImgWidth.Value;
                    else
                        curY += _settingImgWidth.Value;

                    totalMounts++;
                }
                if (_settingWarclawOrder.Value == i) {
                    Texture2D img = GetImgFile("warclaw");
                    Image _btnWarclaw = new Image {
                        Parent = _mountPanel,
                        Texture = img,
                        Size = new Point(_settingImgWidth.Value, _settingImgWidth.Value),
                        Location = new Point(curX, curY),
                        Opacity = _settingOpacity.Value,
                        BasicTooltipText = "Warclaw"
                    };
                    _btnWarclaw.LeftMouseButtonPressed += delegate { DoHotKey(_settingWarclawBinding); };

                    if (_settingOrientation.Value.Equals("Horizontal"))
                        curX += _settingImgWidth.Value;
                    else
                        curY += _settingImgWidth.Value;

                    totalMounts++;
                }
            }

            if (_settingDrag.Value) {
                Panel dragBox = new Panel() {
                    Parent = _mountPanel,
                    Location = new Point(0, 0),
                    Size = new Point(_settingImgWidth.Value / 2, _settingImgWidth.Value / 2),
                    BackgroundColor = Color.White,
                    ZIndex = 10,
                };
                dragBox.LeftMouseButtonPressed += delegate {
                    _dragging = true;
                    _dragStart = InputService.Input.Mouse.Position;
                };
                dragBox.LeftMouseButtonReleased += delegate {
                    _dragging = false;
                   _settingLoc.Value = _mountPanel.Location;
                };
            }

            if (_settingOrientation.Value.Equals("Horizontal")) {
                _mountPanel.Size = new Point(_settingImgWidth.Value * totalMounts, _settingImgWidth.Value);
            }
            else {
                _mountPanel.Size = new Point(_settingImgWidth.Value, _settingImgWidth.Value * totalMounts);
            }

        }
        private void DrawCornerIcons() {
            foreach (int i in _mountOrder) {
                if (i == 0) continue;
                if (_settingGriffonOrder.Value == i) {
                    Texture2D img = GetImgFile("griffon");
                    _cornerGriffon = new CornerIcon() {
                        IconName = "Griffon",
                        Icon = img,
                        HoverIcon = img,
                        Priority = 10
                    };
                    _cornerGriffon.Click += delegate { DoHotKey(_settingGriffonBinding); };
                }
                if (_settingJackalOrder.Value == i) {
                    Texture2D img = GetImgFile("jackal");
                    _cornerJackal = new CornerIcon() {
                        IconName = "Jackal",
                        Icon = img,
                        HoverIcon = img,
                        Priority = 10
                    };
                    _cornerJackal.Click += delegate { DoHotKey(_settingJackalBinding); };
                }
                if (_settingRaptorOrder.Value == i) {
                    Texture2D img = GetImgFile("raptor");
                    _cornerRaptor = new CornerIcon() {
                        IconName = "Raptor",
                        Icon = img,
                        HoverIcon = img,
                        Priority = 10
                    };
                    _cornerRaptor.Click += delegate { DoHotKey(_settingRaptorBinding); };
                }
                if (_settingRollerOrder.Value == i) {
                    Texture2D img = GetImgFile("roller");
                    _cornerRoller = new CornerIcon() {
                        IconName = "Roller",
                        Icon = img,
                        HoverIcon = img,
                        Priority = 10
                    };
                    _cornerRoller.Click += delegate { DoHotKey(_settingRollerBinding); };
                }
                if (_settingSkimmerOrder.Value == i) {
                    Texture2D img = GetImgFile("skimmer");
                    _cornerSkimmer = new CornerIcon() {
                        IconName = "Skimmer",
                        Icon = img,
                        HoverIcon = img,
                        Priority = 10
                    };
                    _cornerSkimmer.Click += delegate { DoHotKey(_settingSkimmerBinding); };
                }
                if (_settingSkyscaleOrder.Value == i) {
                    Texture2D img = GetImgFile("skyscale");
                    _cornerSkyscale = new CornerIcon() {
                        IconName = "Skyscale",
                        Icon = img,
                        HoverIcon = img,
                        Priority = 10
                    };
                    _cornerSkyscale.Click += delegate { DoHotKey(_settingSkyscaleBinding); };
                }
                if (_settingSpringerOrder.Value == i) {
                    Texture2D img = GetImgFile("springer");
                    _cornerSpringer = new CornerIcon() {
                        IconName = "Springer",
                        Icon = img,
                        HoverIcon = img,
                        Priority = 10
                    };
                    _cornerSpringer.Click += delegate { DoHotKey(_settingSpringerBinding); };
                }
                if (_settingWarclawOrder.Value == i) {
                    Texture2D img = GetImgFile("warclaw");
                    _cornerWarclaw = new CornerIcon() {
                        IconName = "Warclaw",
                        Icon = img,
                        HoverIcon = img,
                        Priority = 10
                    };
                    _cornerWarclaw.Click += delegate { DoHotKey(_settingWarclawBinding); };
                }
            }

        }

        private void DrawIcons()
        {
            _mountPanel?.Dispose();

            _cornerGriffon?.Dispose();
            _cornerJackal?.Dispose();
            _cornerRaptor?.Dispose();
            _cornerRoller?.Dispose();
            _cornerSkimmer?.Dispose();
            _cornerSkyscale?.Dispose();
            _cornerSpringer?.Dispose();
            _cornerWarclaw?.Dispose();

            if (_settingDisplay.Value.Equals("Solid Corner") || _settingDisplay.Value.Equals("Transparent Corner"))
                DrawCornerIcons();
            else
                DrawManualIcons();
        }

        private Texture2D GetImgFile(string filename)
        {
            switch (_settingDisplay.Value)
            {
                default:
                case "Solid Manual":
                case "Solid Corner":
                    return ContentsManager.GetTexture(filename + ".png");

                case "Transparent Manual":
                case "Transparent Corner":
                    return ContentsManager.GetTexture(filename + "-trans.png");

                case "Solid Manual Text":
                    return ContentsManager.GetTexture(filename + "-text.png");
            }
        }


        private void DoDefaultMountAction()
        {
            if (Array.Exists(warclawOnlyMaps, mapType => mapType == GameService.Gw2Mumble.CurrentMap.Type))
            {
                DoHotKey(_settingWarclawBinding);
                return;
            }

            if (GameService.Gw2Mumble.PlayerCharacter.Position.Z <= 0)
            {
                DoHotKey(_settingSkimmerBinding);
                return;
            }

            defaultMountChoicesToActions[_settingDefaultMountChoice.Value]();
        }

        private void HandleCombatChange(object sender, ValueEventArgs<bool> e)
        {
            if(!e.Value)
            {
                DoHotKey(InputQueuingKeybindSetting);
                InputQueuingKeybindSetting = null;
            }
        }

        protected void DoHotKey(SettingEntry<KeyBinding> setting)
        {
            if(setting == null)
            {
                return;
            }

            if (GameService.Gw2Mumble.PlayerCharacter.IsInCombat)
            {
                InputQueuingKeybindSetting = setting;
                return;
            }

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
            try
            {
                return (VirtualKeyShort)key;
            } catch
            {
                return new VirtualKeyShort();
            }
        }

    }

}
