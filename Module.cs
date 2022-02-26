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
using Gw2Sharp.Models;

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
        internal static List<Mount> _availableOrderedMounts => _mounts.Where(m => m.IsAvailable).OrderBy(m => m.OrderSetting.Value).ToList();

        public static int[] _mountOrder = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        public static string[] _mountDisplay = new string[] { "Transparent", "Solid", "SolidText" };
        public static string[] _mountOrientation = new string[] { "Horizontal", "Vertical" };
        public static string[] _mountRadialCenterMountBehavior = new string[] { "None", "Default", "LastUsed" };

        public static SettingEntry<string> _settingDefaultMountChoice;
        public static SettingEntry<string> _settingDefaultWaterMountChoice;
        public static SettingEntry<KeyBinding> _settingDefaultMountBinding;
        public static SettingEntry<bool> _settingDisplayMountQueueing;
        public static SettingEntry<bool> _settingDefaultMountUseRadial;
        public static SettingEntry<bool> _settingMountRadialSpawnAtMouse;
        public static SettingEntry<float> _settingMountRadialRadiusModifier;
        public static SettingEntry<float> _settingMountRadialIconSizeModifier;
        public static SettingEntry<float> _settingMountRadialIconOpacity;
        public static SettingEntry<string> _settingMountRadialCenterMountBehavior;
        public static SettingEntry<bool> _settingMountRadialRemoveCenterMount;
        public static SettingEntry<KeyBinding> _settingMountRadialToggleActionCameraKeyBinding;

        public static SettingEntry<string> _settingDisplay;
        public static SettingEntry<bool> _settingDisplayCornerIcons;
        public static SettingEntry<bool> _settingDisplayManualIcons;
        public static SettingEntry<string> _settingOrientation;
        private SettingEntry<Point> _settingLoc;
        public static SettingEntry<bool> _settingDrag;
        public static SettingEntry<int> _settingImgWidth;
        public static SettingEntry<float> _settingOpacity;

#pragma warning disable CS0618 // Type or member is obsolete
        private WindowTab windowTab;
#pragma warning restore CS0618 // Type or member is obsolete
        private Panel _mountPanel;
        private DrawRadial _radial;
        private LoadingSpinner _queueingSpinner;
        private Helper _helper;

        private bool _dragging;
        private Point _dragStart = Point.Zero;

        [ImportingConstructor]
        public Module([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) {
            _helper = new Helper(ContentsManager);
        }

        protected override void Initialize()
        {
            GameService.Gw2Mumble.PlayerCharacter.IsInCombatChanged += async (sender, e) => await HandleCombatChangeAsync(sender, e);

#pragma warning disable CS0618 // Type or member is obsolete
            windowTab = new WindowTab("Mounts", ContentsManager.GetTexture("514394-grey.png"));
#pragma warning restore CS0618 // Type or member is obsolete
        }


        /*
         * Migrate from seperate settings from MountDisplay
         * MountDisplay => "Transparent", "Solid", "SolidText"
         *
         */
        private void MigrateDisplaySettings()
        {
            if (_settingDisplay.Value.Contains("Corner") || _settingDisplay.Value.Contains("Manual"))
            {
                _settingDisplayCornerIcons.Value = _settingDisplay.Value.Contains("Corner");
                _settingDisplayManualIcons.Value = _settingDisplay.Value.Contains("Solid");

                if (_settingDisplay.Value.Contains("Text"))
                {
                    _settingDisplay.Value = "SolidText";
                }
                else if (_settingDisplay.Value.Contains("Solid"))
                {
                    _settingDisplay.Value = "Solid";
                }
                else if (_settingDisplay.Value.Contains("Transparent"))
                {
                    _settingDisplay.Value = "Transparent";
                }
            }
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            _mounts = new Collection<Mount>
            {
                new Raptor(settings, _helper),
                new Springer(settings, _helper),
                new Skimmer(settings, _helper),
                new Jackal(settings, _helper),
                new Griffon(settings, _helper),
                new RollerBeetle(settings, _helper),
                new Warclaw(settings, _helper),
                new Skyscale(settings, _helper),
                new SiegeTurtle(settings, _helper)
            };

            _settingDefaultMountBinding = settings.DefineSetting("DefaultMountBinding", new KeyBinding(Keys.None), "Default Mount Binding", "");
            _settingDefaultMountBinding.Value.Activated += async delegate { await DoDefaultMountActionAsync(); };
            _settingDefaultMountChoice = settings.DefineSetting("DefaultMountChoice", "Disabled", "Default Mount Choice", "");
            _settingDefaultWaterMountChoice = settings.DefineSetting("DefaultWaterMountChoice", "Disabled", "Default Water Mount Choice", "");
            _settingDisplayMountQueueing = settings.DefineSetting("DisplayMountQueueing", false, "Display Mount queuing", "");
            _settingDefaultMountUseRadial = settings.DefineSetting("DefaultMountUseRadial", false, "Default Mount uses radial", "");
            _settingMountRadialSpawnAtMouse = settings.DefineSetting("MountRadialSpawnAtMouse", false, "Radial spawn at mouse", "");
            _settingMountRadialIconSizeModifier = settings.DefineSetting("MountRadialIconSizeModifier", 0.5f, "Radial Icon Size", "");
            _settingMountRadialIconSizeModifier.SetRange(0.05f, 1f);
            _settingMountRadialRadiusModifier = settings.DefineSetting("MountRadialRadiusModifier", 0.5f, "Radial radius", "");
            _settingMountRadialRadiusModifier.SetRange(0.2f, 1f);
            _settingMountRadialIconOpacity = settings.DefineSetting("MountRadialIconOpacity", 0.5f, "Radial Icon Opacity", "");
            _settingMountRadialIconOpacity.SetRange(0.05f, 1f);
            _settingMountRadialCenterMountBehavior = settings.DefineSetting("MountRadialCenterMountBehavior", "Default", "Determines the mount in the center of the radial.", "");
            _settingMountRadialRemoveCenterMount = settings.DefineSetting("MountRadialRemoveCenterMount", true, "Removes center mount from radial", "");
            _settingMountRadialToggleActionCameraKeyBinding = settings.DefineSetting("MountRadialToggleActionCameraKeyBinding", new KeyBinding(Keys.F10), "Ingame KeyBind to toggle action camera", "");

            _settingDisplay = settings.DefineSetting("MountDisplay", "Transparent", "Display Type", "");
            _settingDisplayCornerIcons = settings.DefineSetting("MountDisplayCornerIcons", false, "Display corner icons", "");
            _settingDisplayManualIcons = settings.DefineSetting("MountDisplayManualIcons", false, "Display manual icons", "");
            _settingOrientation = settings.DefineSetting("Orientation", "Horizontal", "Manual Orientation", "");
            _settingLoc = settings.DefineSetting("MountLoc", new Point(100, 100), "Window Location", "");
            _settingDrag = settings.DefineSetting("MountDrag", false, "Enable Dragging (White Box)", "");
            _settingImgWidth = settings.DefineSetting("MountImgWidth", 50, "Manual Icon Width", "");
            _settingImgWidth.SetRange(0, 200);
            _settingOpacity = settings.DefineSetting("MountOpacity", 1.0f, "Manual Opacity", "");
            _settingOpacity.SetRange(0f, 1f);

            MigrateDisplaySettings();

            foreach (Mount m in _mounts)
            {
                m.OrderSetting.SettingChanged += UpdateSettings;
                m.KeybindingSetting.SettingChanged += UpdateSettings;
            }
            _settingDefaultMountChoice.SettingChanged += UpdateSettings;
            _settingDefaultWaterMountChoice.SettingChanged += UpdateSettings;
            _settingDisplayMountQueueing.SettingChanged += UpdateSettings;
            _settingDefaultMountUseRadial.SettingChanged += UpdateSettings;
            _settingMountRadialSpawnAtMouse.SettingChanged += UpdateSettings;
            _settingMountRadialIconSizeModifier.SettingChanged += UpdateSettings;
            _settingMountRadialRadiusModifier.SettingChanged += UpdateSettings;
            _settingMountRadialCenterMountBehavior.SettingChanged += UpdateSettings;
            _settingMountRadialIconOpacity.SettingChanged += UpdateSettings;
            _settingMountRadialRemoveCenterMount.SettingChanged += UpdateSettings;

            _settingDisplay.SettingChanged += UpdateSettings;
            _settingDisplayCornerIcons.SettingChanged += UpdateSettings;
            _settingDisplayManualIcons.SettingChanged += UpdateSettings;
            _settingOrientation.SettingChanged += UpdateSettings;
            _settingLoc.SettingChanged += UpdateSettings;
            _settingDrag.SettingChanged += UpdateSettings;
            _settingImgWidth.SettingChanged += UpdateSettings;
            _settingOpacity.SettingChanged += UpdateSettings;

        }

        public override IView GetSettingsView()
        {
            return new Views.DummySettingsView(ContentsManager);
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            DrawUI();
            GameService.Overlay.BlishHudWindow.AddTab(windowTab, () => new Views.SettingsView());

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

            if(_settingDisplayMountQueueing.Value && _mounts.Any(m => m.QueuedTimestamp != null))
            {
                _queueingSpinner?.Show();
            }

            if(_radial.Visible && !_settingDefaultMountBinding.Value.IsTriggering)
            {
                _radial.Hide();
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

            _settingDefaultMountChoice.SettingChanged -= UpdateSettings;
            _settingDefaultWaterMountChoice.SettingChanged -= UpdateSettings;
            _settingDisplayMountQueueing.SettingChanged -= UpdateSettings;
            _settingDefaultMountUseRadial.SettingChanged -= UpdateSettings;
            _settingMountRadialSpawnAtMouse.SettingChanged += UpdateSettings;
            _settingMountRadialIconSizeModifier.SettingChanged -= UpdateSettings;
            _settingMountRadialRadiusModifier.SettingChanged -= UpdateSettings;
            _settingMountRadialCenterMountBehavior.SettingChanged -= UpdateSettings;
            _settingMountRadialIconOpacity.SettingChanged -= UpdateSettings;
            _settingMountRadialRemoveCenterMount.SettingChanged -= UpdateSettings;

            _settingDisplay.SettingChanged -= UpdateSettings;
            _settingDisplayCornerIcons.SettingChanged -= UpdateSettings;
            _settingDisplayManualIcons.SettingChanged -= UpdateSettings;
            _settingOrientation.SettingChanged -= UpdateSettings;
            _settingLoc.SettingChanged -= UpdateSettings;
            _settingDrag.SettingChanged -= UpdateSettings;
            _settingImgWidth.SettingChanged -= UpdateSettings;
            _settingOpacity.SettingChanged -= UpdateSettings;
            
            GameService.Overlay.BlishHudWindow.RemoveTab(windowTab);
        }

        private void UpdateSettings(object sender = null, ValueChangedEventArgs<string> e = null) {
            DrawUI();
        }
        private void UpdateSettings(object sender = null, ValueChangedEventArgs<KeyBinding> e = null) {
            DrawUI();
        }
        private void UpdateSettings(object sender = null, ValueChangedEventArgs<Point> e = null) {
            DrawUI();
        }
        private void UpdateSettings(object sender = null, ValueChangedEventArgs<bool> e = null) {
            DrawUI();
        }
        private void UpdateSettings(object sender = null, ValueChangedEventArgs<float> e = null)
        {
            DrawUI();
        }
        private void UpdateSettings(object sender = null, ValueChangedEventArgs<int> e = null)
        {
            DrawUI();
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
                _btnMount.LeftMouseButtonPressed += async delegate { await mount.DoHotKey(); };

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
                mount.CreateCornerIcon(_helper.GetImgFile(mount.ImageFileName));
            }

        }

        private void DrawUI()
        {
            _mountPanel?.Dispose();
            foreach (Mount mount in _mounts)
            {
                mount.DisposeCornerIcon();
            }

            if (_settingDisplayCornerIcons.Value)
                DrawCornerIcons();
            if (_settingDisplayManualIcons.Value)
                DrawManualIcons();

            _queueingSpinner?.Dispose();
            _queueingSpinner = new LoadingSpinner();
            _queueingSpinner.Location = new Point(GameService.Graphics.SpriteScreen.Width / 2 + 400, GameService.Graphics.SpriteScreen.Height - _queueingSpinner.Height - 25);
            _queueingSpinner.Parent = GameService.Graphics.SpriteScreen;
            _queueingSpinner.Hide();

            _radial?.Dispose();
            _radial = new DrawRadial(_helper);
            _radial.Parent = GameService.Graphics.SpriteScreen;
        }

        private async Task DoDefaultMountActionAsync()
        {
            if (GameService.Gw2Mumble.PlayerCharacter.CurrentMount != MountType.None)
            {
                await (_availableOrderedMounts.FirstOrDefault()?.DoHotKey() ?? Task.CompletedTask);
                return;
            }

            var instantMount = _helper.GetInstantMount();
            if (instantMount != null)
            {
                await instantMount.DoHotKey();
                return;
            }

            _radial.Show();
        }
        
        private async Task HandleCombatChangeAsync(object sender, ValueEventArgs<bool> e)
        {
            if (!e.Value)
            {
                await (_mounts.Where(m => m.QueuedTimestamp != null).OrderByDescending(m => m.QueuedTimestamp).FirstOrDefault()?.DoHotKey() ?? Task.CompletedTask);
                foreach (var mount in _mounts)
                {
                    mount.QueuedTimestamp = null;
                }
                _queueingSpinner?.Hide();
            }
        }
    }
}
