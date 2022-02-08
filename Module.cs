using Blish_HUD;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Blish_HUD.Input;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.ObjectModel;
using Manlaan.Mounts.Controls;
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

        internal static Collection<Mount> _mounts;
        internal static List<Mount> _availableOrderedMounts => _mounts.Where(m => m.OrderSetting.Value != 0).OrderBy(m => m.OrderSetting.Value).ToList();

        public static int[] _mountOrder = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        public static string[] _mountDisplay = new string[] { "Transparent Corner", "Solid Corner", "Transparent Manual", "Solid Manual", "Solid Manual Text" };
        public static string[] _mountOrientation = new string[] { "Horizontal", "Vertical" };
        public static SettingEntry<KeyBinding> InputQueuingKeybindSetting = null;

        public static SettingEntry<string> _settingDefaultMountChoice;
        public static SettingEntry<string> _settingDefaultWaterMountChoice;
        public static SettingEntry<KeyBinding> _settingDefaultMountBinding;
        public static SettingEntry<bool> _settingDefaultMountUseRadial;
        public static SettingEntry<string> _settingDisplay;
        public static SettingEntry<string> _settingOrientation;
        private SettingEntry<Point> _settingLoc;
        public static SettingEntry<bool> _settingDrag;
        public static SettingEntry<int> _settingImgWidth;
        public static SettingEntry<float> _settingOpacity;

        private Panel _mountPanel;
        private DrawRadial _radial;
        private Helper _helper;

        private bool _dragging;
        private Point _dragStart = Point.Zero;

        [ImportingConstructor]
        public Module([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) { }

        protected override void Initialize()
        {
            GameService.Gw2Mumble.PlayerCharacter.IsInCombatChanged += (sender, e) => HandleCombatChange(sender, e);
            GameService.Input.Keyboard.KeyStateChanged += (sender, e) => HandleKeyBoardKeyChange(sender, e);

            _helper = new Helper(ContentsManager);
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            _mounts = new Collection<Mount>
            {
                new Raptor(settings),
                new Springer(settings),
                new Skimmer(settings),
                new Jackal(settings),
                new Griffon(settings),
                new RollerBeetle(settings),
                new Warclaw(settings),
                new Skyscale(settings),
                new SiegeTurtle(settings)
            };

            _settingDefaultMountChoice = settings.DefineSetting("DefaultMountChoice", "Disabled", "Default Mount Choice", "");
            _settingDefaultWaterMountChoice = settings.DefineSetting("DefaultWaterMountChoice", "Disabled", "Default Water Mount Choice", "");
            _settingDefaultMountBinding = settings.DefineSetting("DefaultMountBinding", new KeyBinding(Keys.None), "Default Mount Binding", "");
            _settingDefaultMountUseRadial = settings.DefineSetting("DefaultMountUseRadial", false, "Default Mount uses radial", "");

            _settingDisplay = settings.DefineSetting("MountDisplay", "Transparent Corner", "Display Type", "");
            _settingOrientation = settings.DefineSetting("Orientation", "Horizontal", "Manual Orientation", "");
            _settingLoc = settings.DefineSetting("MountLoc", new Point(100, 100), "Window Location", "");
            _settingDrag = settings.DefineSetting("MountDrag", false, "Enable Dragging (White Box)", "");
            _settingImgWidth = settings.DefineSetting("MountImgWidth", 50, "Manual Icon Width", "");
            _settingOpacity = settings.DefineSetting("MountOpacity", 1.0f, "Manual Opacity", "");

            _settingImgWidth.SetRange(0, 200);
            _settingOpacity.SetRange(0f, 1f);

            foreach (Mount m in _mounts)
            {
                m.OrderSetting.SettingChanged += UpdateSettings;
                m.KeybindingSetting.SettingChanged += UpdateSettings;
            }
            _settingDefaultMountBinding.SettingChanged += UpdateSettings;
            _settingDefaultMountBinding.Value.Enabled = true;
            _settingDefaultMountBinding.Value.Activated += delegate { DoDefaultMountAction(); };
            _settingDefaultWaterMountChoice.SettingChanged += UpdateSettings;
            _settingDefaultMountUseRadial.SettingChanged += UpdateSettings;

            _settingDisplay.SettingChanged += UpdateSettings;
            _settingOrientation.SettingChanged += UpdateSettings;
            _settingLoc.SettingChanged += UpdateSettings;
            _settingDrag.SettingChanged += UpdateSettings;
            _settingImgWidth.SettingChanged += UpdateSettings;
            _settingOpacity.SettingChanged += UpdateSettings;

        }
        public override IView GetSettingsView() {
            return new Mounts.Views.SettingsView();
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
            if (_mountPanel != null)
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
            _mountPanel?.Dispose();
            _radial?.Dispose();

            foreach (Mount m in _mounts)
            {
                m.OrderSetting.SettingChanged -= UpdateSettings;
                m.KeybindingSetting.SettingChanged -= UpdateSettings;
                m.DisposeCornerIcon();
            }

            _settingDefaultMountBinding.SettingChanged -= UpdateSettings;
            _settingDefaultMountUseRadial.SettingChanged -= UpdateSettings;

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

        internal void DrawManualIcons() {
            int curX = 0;
            int curY = 0;
            int totalMounts = 0;

            _mountPanel = new Panel() {
                Parent = GameService.Graphics.SpriteScreen,
                Location = _settingLoc.Value,
                Size = new Point(_settingImgWidth.Value * 8, _settingImgWidth.Value * 8),
            };

            foreach (Mount mount in _availableOrderedMounts) {
                Texture2D img = _helper.GetImgFile(mount.ImageFileName);
                Image _btnMount = new Image
                {
                    Parent = _mountPanel,
                    Texture = img,
                    Size = new Point(_settingImgWidth.Value, _settingImgWidth.Value),
                    Location = new Point(curX, curY),
                    Opacity = _settingOpacity.Value,
                    BasicTooltipText = mount.DisplayName
                };
                _btnMount.LeftMouseButtonPressed += delegate { mount.DoHotKey(); };

                if (_settingOrientation.Value.Equals("Horizontal"))
                    curX += _settingImgWidth.Value;
                else
                    curY += _settingImgWidth.Value;

                totalMounts++;
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
            foreach (Mount mount in _availableOrderedMounts)
            {
                if (mount.OrderSetting.Value != 0)
                {
                    mount.CreateCornerIcon(_helper.GetImgFile(mount.ImageFileName));
                }
            }

        }

        private void DrawIcons()
        {
            _mountPanel?.Dispose();
            foreach (Mount mount in _mounts)
            {
                mount.DisposeCornerIcon();
            }

            if (_settingDisplay.Value.Equals("Solid Corner") || _settingDisplay.Value.Equals("Transparent Corner"))
                DrawCornerIcons();
            else
                DrawManualIcons();

            _radial?.Dispose();
            _radial = new DrawRadial(_helper);
            _radial.Parent = GameService.Graphics.SpriteScreen;
        }

        private void HandleKeyBoardKeyChange(object sender, KeyboardEventArgs e)
        {
            var key = _settingDefaultMountBinding.Value.PrimaryKey;
            if (_settingDefaultMountUseRadial.Value)
            {
                if (e.Key == key)
                {
                    if (e.EventType == KeyboardEventType.KeyDown) _radial.Start();
                    else if (e.EventType == KeyboardEventType.KeyUp) _radial.Stop();
                }
            }
        }

        private void DoDefaultMountAction()
        {
            if (_settingDefaultMountUseRadial.Value)
            {
                return;
            }

            _helper.GetDefaultMount()?.DoHotKey();
        }


        private void HandleCombatChange(object sender, ValueEventArgs<bool> e)
        {
            if (!e.Value)
            {
                _mounts.Single(m => m.IsQueuing).DoHotKey();
            }
        }
    }
}
