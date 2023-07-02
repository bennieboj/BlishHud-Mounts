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
using Manlaan.Mounts.Views;
using Blish_HUD.Content;
using System.IO;
using SharpDX.DirectWrite;

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

        public static string mountsDirectory;
        private TabbedWindow2 _settingsWindow;

        public static List<MountImageFile> _mountImageFiles = new List<MountImageFile>();

        public static int[] _mountOrder = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        public static string[] _mountBehaviour = new string[] { "DefaultMount", "Radial" };
        public static string[] _mountOrientation = new string[] { "Horizontal", "Vertical" };
        public static string[] _mountRadialCenterMountBehavior = new string[] { "None", "Default", "LastUsed" };

        public static SettingEntry<string> _settingDefaultMountChoice;
        public static SettingEntry<string> _settingDefaultWaterMountChoice;
        public static SettingEntry<string> _settingDefaultFlyingMountChoice;
        public static SettingEntry<KeyBinding> _settingDefaultMountBinding;
        public static SettingEntry<bool> _settingDisplayMountQueueing;
        public static SettingEntry<string> _settingDefaultMountBehaviour;
        public static SettingEntry<bool> _settingMountRadialSpawnAtMouse;
        public static SettingEntry<float> _settingMountRadialRadiusModifier;
        public static SettingEntry<float> _settingMountRadialStartAngle;
        public static SettingEntry<float> _settingMountRadialIconSizeModifier;
        public static SettingEntry<float> _settingMountRadialIconOpacity;
        public static SettingEntry<string> _settingMountRadialCenterMountBehavior;
        public static SettingEntry<bool> _settingMountRadialRemoveCenterMount;
        public static SettingEntry<KeyBinding> _settingMountRadialToggleActionCameraKeyBinding;

        public static SettingEntry<bool> _settingDisplayModuleOnLoadingScreen;
        public static SettingEntry<bool> _settingMountAutomaticallyAfterLoadingScreen;
        public static SettingEntry<string> _settingDisplay;
        public static SettingEntry<bool> _settingDisplayCornerIcons;
        public static SettingEntry<bool> _settingDisplayManualIcons;
        public static SettingEntry<string> _settingOrientation;
        private SettingEntry<Point> _settingLoc;
        public static SettingEntry<bool> _settingDrag;
        public static SettingEntry<int> _settingImgWidth;
        public static SettingEntry<float> _settingOpacity;

        private Panel _mountPanel;
        public static DebugControl _debug;
        private DrawRadial _radial;
        private LoadingSpinner _queueingSpinner;
        private DrawMouseCursor _drawMouseCursor;
        private Helper _helper;
        private TextureCache _textureCache;

        private float _lastZPosition = 0.0f;
        private double _lastUpdateSeconds = 0.0f;
        public static bool IsPlayerGlidingOrFalling;

        private bool _lastIsMountSwitchable = false;
        
        private bool _dragging;
        private Point _dragStart = Point.Zero;

        [ImportingConstructor]
        public Module([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            _helper = new Helper();
        }

        protected override void Initialize()
        {
            List<string> mountsFilesInRef = new List<string> {
                "griffon-text.png",
                "griffon-trans.png",
                "griffon.png",
                "jackal-text.png",
                "jackal-trans.png",
                "jackal.png",
                "raptor-text.png",
                "raptor-trans.png",
                "raptor.png",
                "roller-text.png",
                "roller-trans.png",
                "roller.png",
                "skimmer-text.png",
                "skimmer-trans.png",
                "skimmer.png",
                "skyscale-text.png",
                "skyscale-trans.png",
                "skyscale.png",
                "springer-text.png",
                "springer-trans.png",
                "springer.png",
                "turtle-text.png",
                "turtle-trans.png",
                "turtle.png",
                "warclaw-text.png",
                "warclaw-trans.png",
                "warclaw.png"
            };
            mountsDirectory = DirectoriesManager.GetFullDirectoryPath("mounts");
            mountsFilesInRef.ForEach(f => ExtractFile(f, mountsDirectory));
            _mountImageFiles = Directory.GetFiles(mountsDirectory, ".")
                .Where(file => file.ToLower().Contains(".png"))
                .Select(file => new MountImageFile() { Name = file.Substring(mountsDirectory.Length + 1) }).ToList();
            _textureCache = new TextureCache(ContentsManager);

            GameService.Gw2Mumble.PlayerCharacter.IsInCombatChanged += async (sender, e) => await HandleCombatChangeAsync(sender, e);

            var mountsIcon = _textureCache.GetImgFile(TextureCache.MountLogoTextureName);

            _settingsWindow = new TabbedWindow2(
                                    _textureCache.GetImgFile(TextureCache.TabBackgroundTextureName),
                                    new Rectangle(35, 36, 1300, 900),
                                    new Rectangle(95, 42, 1183 + 38, 792)
                                   )
            {
                Title = "Mounts",
                Parent = GameService.Graphics.SpriteScreen,
                Location = new Point(100, 100),
                Emblem = mountsIcon,
                Id = $"{this.Namespace}_SettingsWindow",
                SavesPosition = true,
            };
            _settingsWindow.Tabs.Add(new Tab(_textureCache.GetImgFile(TextureCache.SettingsIconTextureName), () => new SettingsView(_textureCache), Strings.Window_AllSettingsTab));
        }

        private void ExtractFile(string filePath, string directoryToExtractTo)
        {
            var fullPath = Path.Combine(directoryToExtractTo, filePath);
            using (var fs = ContentsManager.GetFileStream(filePath))
            {
                fs.Position = 0;
                byte[] buffer = new byte[fs.Length];
                var content = fs.Read(buffer, 0, (int)fs.Length);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                File.WriteAllBytes(fullPath, buffer);
            }
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

        /*
         * Migrate from seperate settings from MountDisplay
         * MountDisplay => "Transparent", "Solid", "SolidText"
         *
         */
        private void MigrateMountFileNameSettings()
        {
            if (_mounts.All(m => m.ImageFileNameSetting.Value.Equals("")))
            {
                var partOfFileName = "";
                if (_settingDisplay.Value.Equals("Transparent"))
                {
                    partOfFileName = "-trans";
                }
                else if (_settingDisplay.Value.Equals("SolidText"))
                {
                    partOfFileName = "-text";
                }
                else if (_settingDisplay.Value.Equals("Solid"))
                {
                    partOfFileName = "";
                }
                foreach (var mount in _mounts)
                {
                    mount.ImageFileNameSetting.Value = $"{mount.ImageFileName}{partOfFileName}.png";
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

            _settingDefaultMountBinding = settings.DefineSetting("DefaultMountBinding", new KeyBinding(Keys.None), () => Strings.Setting_DefaultMountBinding, () => "");
            _settingDefaultMountBinding.Value.Enabled = true;
            _settingDefaultMountBinding.Value.Activated += async delegate { await DoDefaultMountActionAsync(); };
            _settingDefaultMountChoice = settings.DefineSetting("DefaultMountChoice", "Disabled", () => Strings.Setting_DefaultMountChoice, () => "");
            _settingDefaultWaterMountChoice = settings.DefineSetting("DefaultWaterMountChoice", "Disabled", () => Strings.Setting_DefaultWaterMountChoice, () => "");
            _settingDefaultFlyingMountChoice = settings.DefineSetting("DefaultFlyingMountChoice", "Disabled", () => Strings.Setting_DefaultFlyingMountChoice, () => "");
            _settingDefaultMountBehaviour = settings.DefineSetting("DefaultMountBehaviour", "Radial", () => Strings.Setting_DefaultMountBehaviour, () => "");
            _settingDisplayMountQueueing = settings.DefineSetting("DisplayMountQueueing", false, () => Strings.Setting_DisplayMountQueueing, () => "");
            _settingMountRadialSpawnAtMouse = settings.DefineSetting("MountRadialSpawnAtMouse", false, () => Strings.Setting_MountRadialSpawnAtMouse, () => "");
            _settingMountRadialIconSizeModifier = settings.DefineSetting("MountRadialIconSizeModifier", 0.5f, () => Strings.Setting_MountRadialIconSizeModifier, () => "");
            _settingMountRadialIconSizeModifier.SetRange(0.05f, 1f);
            _settingMountRadialRadiusModifier = settings.DefineSetting("MountRadialRadiusModifier", 0.5f, () => Strings.Setting_MountRadialRadiusModifier, () => "");
            _settingMountRadialRadiusModifier.SetRange(0.2f, 1f);
            _settingMountRadialStartAngle = settings.DefineSetting("MountRadialStartAngle", 0.0f, () => Strings.Setting_MountRadialStartAngle, () => "");
            _settingMountRadialStartAngle.SetRange(0.0f, 1.0f);
            _settingMountRadialIconOpacity = settings.DefineSetting("MountRadialIconOpacity", 0.5f, () => Strings.Setting_MountRadialIconOpacity, () => "");
            _settingMountRadialIconOpacity.SetRange(0.05f, 1f);
            _settingMountRadialCenterMountBehavior = settings.DefineSetting("MountRadialCenterMountBehavior", "Default", () => Strings.Setting_MountRadialCenterMountBehavior, () => "");
            _settingMountRadialRemoveCenterMount = settings.DefineSetting("MountRadialRemoveCenterMount", true, () => Strings.Setting_MountRadialRemoveCenterMount, () => "");
            _settingMountRadialToggleActionCameraKeyBinding = settings.DefineSetting("MountRadialToggleActionCameraKeyBinding", new KeyBinding(Keys.F10), () => Strings.Setting_MountRadialToggleActionCameraKeyBinding, () => "");


            _settingDisplayModuleOnLoadingScreen = settings.DefineSetting("DisplayModuleOnLoadingScreen", false, () => Strings.Setting_DisplayModuleOnLoadingScreen, () => "");
            _settingMountAutomaticallyAfterLoadingScreen = settings.DefineSetting("MountAutomaticallyAfterLoadingScreen", false, () => Strings.Setting_MountAutomaticallyAfterLoadingScreen, () => "");
            _settingDisplay = settings.DefineSetting("MountDisplay", "Transparent", () => Strings.Setting_MountDisplay, () => "");
            _settingDisplayCornerIcons = settings.DefineSetting("MountDisplayCornerIcons", false, () => Strings.Setting_MountDisplayCornerIcons, () => "");
            _settingDisplayManualIcons = settings.DefineSetting("MountDisplayManualIcons", false, () => Strings.Setting_MountDisplayManualIcons, () => "");
            _settingOrientation = settings.DefineSetting("Orientation", "Horizontal", () => Strings.Setting_Orientation, () => "");
            _settingLoc = settings.DefineSetting("MountLoc", new Point(100, 100), () => Strings.Setting_MountLoc, () => "");
            _settingDrag = settings.DefineSetting("MountDrag", false, () => Strings.Setting_MountDrag, () => "");
            _settingImgWidth = settings.DefineSetting("MountImgWidth", 50, () => Strings.Setting_MountImgWidth, () => "");
            _settingImgWidth.SetRange(0, 200);
            _settingOpacity = settings.DefineSetting("MountOpacity", 1.0f, () => Strings.Setting_MountOpacity, () => "");
            _settingOpacity.SetRange(0f, 1f);

            MigrateDisplaySettings();
            MigrateMountFileNameSettings();

            foreach (Mount m in _mounts)
            {
                m.OrderSetting.SettingChanged += UpdateSettings;
                m.KeybindingSetting.SettingChanged += UpdateSettings;
                m.ImageFileNameSetting.SettingChanged += UpdateSettings;
            }
            _settingDefaultMountChoice.SettingChanged += UpdateSettings;
            _settingDefaultWaterMountChoice.SettingChanged += UpdateSettings;
            _settingDefaultFlyingMountChoice.SettingChanged += UpdateSettings;
            _settingDisplayModuleOnLoadingScreen.SettingChanged += UpdateSettings;
            _settingMountAutomaticallyAfterLoadingScreen.SettingChanged += UpdateSettings;
            _settingDisplayMountQueueing.SettingChanged += UpdateSettings;
            _settingMountRadialSpawnAtMouse.SettingChanged += UpdateSettings;
            _settingMountRadialIconSizeModifier.SettingChanged += UpdateSettings;
            _settingMountRadialRadiusModifier.SettingChanged += UpdateSettings;
            _settingMountRadialStartAngle.SettingChanged += UpdateSettings;
            _settingMountRadialCenterMountBehavior.SettingChanged += UpdateSettings;
            _settingMountRadialToggleActionCameraKeyBinding.SettingChanged += UpdateSettings;
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
            var dummySettingWindow = new DummySettingsView();
            dummySettingWindow.OnSettingsButtonClicked += (args, sender) =>
            {                
                _settingsWindow.SelectedTab = _settingsWindow.Tabs.First();
                _settingsWindow.Show();
            };
            return dummySettingWindow;
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            DrawUI();

            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        protected override void Update(GameTime gameTime)
        {
            var currentZPosition = GameService.Gw2Mumble.PlayerCharacter.Position.Z;
            var currentUpdateSeconds = gameTime.TotalGameTime.TotalSeconds;
            var secondsDiff = currentUpdateSeconds - _lastUpdateSeconds;
            var zPositionDiff = currentZPosition - _lastZPosition;
            
            if (zPositionDiff < -0.0001 && secondsDiff != 0)
            {
                var velocity = zPositionDiff / secondsDiff;
                IsPlayerGlidingOrFalling = velocity < -2.5;
            }
            else
            {
                IsPlayerGlidingOrFalling = false;
            }

            _lastZPosition = currentZPosition;
            _lastUpdateSeconds = currentUpdateSeconds;

            var isMountSwitchable = IsMountSwitchable();
            var moduleHidden = _lastIsMountSwitchable && !isMountSwitchable;
            var moduleShown = !_lastIsMountSwitchable && isMountSwitchable;
            var currentMount = GameService.Gw2Mumble.PlayerCharacter.CurrentMount;
            var currentCharacterName = GameService.Gw2Mumble.PlayerCharacter.Name;
            if (moduleHidden && currentMount != MountType.None && _settingMountAutomaticallyAfterLoadingScreen.Value)
            {
                _helper.StoreMountForLaterUse(_mounts.Single(m => m.MountType == currentMount), currentCharacterName);
            }
            if (moduleShown && currentMount == MountType.None && _helper.IsCharacterTheSameAfterMapLoad(currentCharacterName))
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                _helper.DoMountActionForLaterUse();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                _helper.ClearMountForLaterUse();
            }

            _lastIsMountSwitchable = isMountSwitchable;

            bool shouldShowModule = ShouldShowModule();
            if (shouldShowModule)
            {
                _mountPanel?.Show();
                foreach (var mount in _availableOrderedMounts)
                {
                    mount.CornerIcon?.Show();
                }
            }
            else
            {
                _mountPanel?.Hide();
                foreach (var mount in _availableOrderedMounts)
                {
                    mount.CornerIcon?.Hide();
                }
            }
            
            if (_dragging)
            {
                var nOffset = InputService.Input.Mouse.Position - _dragStart;
                _mountPanel.Location += nOffset;

                _dragStart = InputService.Input.Mouse.Position;
            }

            if (_settingDisplayMountQueueing.Value && _mounts.Any(m => m.QueuedTimestamp != null))
            {
                _queueingSpinner?.Show();
            }

            //if (GameService.Input.Mouse.CameraDragging && _radial.Visible && !GameService.Input.Mouse.CursorIsVisible)
            //{
            //    _drawMouseCursor.Location = new Point(GameService.Input.Mouse.PositionRaw.X, GameService.Input.Mouse.PositionRaw.Y);
            //    _drawMouseCursor.Show();
            //}
            //else
            //{
            //    _drawMouseCursor.Hide();
            //}

            if (_radial.Visible && !_settingDefaultMountBinding.Value.IsTriggering || !shouldShowModule)
            {
                _radial.Hide();
            }
        }

        public static bool ShouldShowModule()
        {
            return _settingDisplayModuleOnLoadingScreen.Value || IsMountSwitchable();
        }

        public static bool IsMountSwitchable()
        {
            return GameService.GameIntegration.Gw2Instance.IsInGame && !GameService.Gw2Mumble.UI.IsMapOpen;
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            _debug?.Dispose();
            _mountPanel?.Dispose();
            _radial?.Dispose();

            foreach (Mount m in _mounts)
            {
                m.OrderSetting.SettingChanged -= UpdateSettings;
                m.KeybindingSetting.SettingChanged -= UpdateSettings;
                m.ImageFileNameSetting.SettingChanged += UpdateSettings;
                m.DisposeCornerIcon();
            }

            _settingDefaultMountChoice.SettingChanged -= UpdateSettings;
            _settingDefaultWaterMountChoice.SettingChanged -= UpdateSettings;
            _settingDefaultFlyingMountChoice.SettingChanged -= UpdateSettings;
            _settingDisplayMountQueueing.SettingChanged -= UpdateSettings;
            _settingMountRadialSpawnAtMouse.SettingChanged += UpdateSettings;
            _settingMountRadialIconSizeModifier.SettingChanged -= UpdateSettings;
            _settingMountRadialRadiusModifier.SettingChanged -= UpdateSettings;
            _settingMountRadialStartAngle.SettingChanged -= UpdateSettings;
            _settingMountRadialCenterMountBehavior.SettingChanged -= UpdateSettings;
            _settingMountRadialToggleActionCameraKeyBinding.SettingChanged -= UpdateSettings;
            _settingMountRadialIconOpacity.SettingChanged -= UpdateSettings;
            _settingMountRadialRemoveCenterMount.SettingChanged -= UpdateSettings;

            _settingDisplayModuleOnLoadingScreen.SettingChanged -= UpdateSettings;
            _settingMountAutomaticallyAfterLoadingScreen.SettingChanged -= UpdateSettings;
            _settingDisplay.SettingChanged -= UpdateSettings;
            _settingDisplayCornerIcons.SettingChanged -= UpdateSettings;
            _settingDisplayManualIcons.SettingChanged -= UpdateSettings;
            _settingOrientation.SettingChanged -= UpdateSettings;
            _settingLoc.SettingChanged -= UpdateSettings;
            _settingDrag.SettingChanged -= UpdateSettings;
            _settingImgWidth.SettingChanged -= UpdateSettings;
            _settingOpacity.SettingChanged -= UpdateSettings;
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
                Texture2D img = _textureCache.GetMountImgFile(mount);
                Image _btnMount = new Image
                {
                    Parent = _mountPanel,
                    Texture = img,
                    Size = new Point(_settingImgWidth.Value, _settingImgWidth.Value),
                    Location = new Point(curX, curY),
                    Opacity = _settingOpacity.Value,
                    BasicTooltipText = mount.DisplayName
                };
                _btnMount.LeftMouseButtonPressed += async delegate { await mount.DoMountAction(); };

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
                mount.CreateCornerIcon(_textureCache.GetMountImgFile(mount));
            }

        }

        private void DrawUI()
        {
            _mountPanel?.Dispose();
            foreach (Mount mount in _mounts)
            {
                mount.DisposeCornerIcon();
            }

            _debug?.Dispose();
            _debug = new DebugControl()
            {
                Parent = GameService.Graphics.SpriteScreen,
                Location = new Point(0, 0),
                Size = new Point(500, 500)
            };
            _debug.Add("position", () => $"{GameService.Input.Mouse.Position.X}, {GameService.Input.Mouse.Position.Y}");
            _debug.Add("positionRaw", () => $"{GameService.Input.Mouse.PositionRaw.X}, {GameService.Input.Mouse.PositionRaw.Y}");
            _debug.Add("spritescreen", () => $"{GameService.Graphics.SpriteScreen.Height}, {GameService.Graphics.SpriteScreen.Width}");
            _debug.Add("window", () => $"{GameService.Graphics.WindowHeight}, {GameService.Graphics.WindowWidth}");
            _debug.Add("calculation", () => {
                var x = 1.0f * GameService.Input.Mouse.PositionRaw.X / GameService.Graphics.WindowHeight * GameService.Graphics.SpriteScreen.Height;
                var y = 1.0f * GameService.Input.Mouse.PositionRaw.Y / GameService.Graphics.WindowWidth * GameService.Graphics.SpriteScreen.Width;
                x = (float)Math.Floor(x);
                y = (float)Math.Floor(y);
                x = Math.Max(x, 0);
                y = Math.Max(y, 0);
                return $"{x}, {y}";
            });

            if (_settingDisplayCornerIcons.Value)
                DrawCornerIcons();
            if (_settingDisplayManualIcons.Value)
                DrawManualIcons();

            _queueingSpinner?.Dispose();
            _queueingSpinner = new LoadingSpinner();
            _queueingSpinner.Location = new Point(GameService.Graphics.SpriteScreen.Width / 2 + 400, GameService.Graphics.SpriteScreen.Height - _queueingSpinner.Height - 25);
            _queueingSpinner.Parent = GameService.Graphics.SpriteScreen;
            _queueingSpinner.Hide();

            _drawMouseCursor?.Dispose();
            _drawMouseCursor = new DrawMouseCursor(_textureCache);
            _drawMouseCursor.Parent = GameService.Graphics.SpriteScreen;
            _drawMouseCursor.Hide();

            _radial?.Dispose();
            _radial = new DrawRadial(_helper, _textureCache);
            _radial.Parent = GameService.Graphics.SpriteScreen;
            _radial.OnSettingsButtonClicked += (args, sender) =>
            {
                _settingsWindow.SelectedTab = _settingsWindow.Tabs.First();
                _settingsWindow.Show();
            };
        }

        private async Task DoDefaultMountActionAsync()
        {
            Logger.Debug("DoDefaultMountActionAsync entered");
            if (GameService.Gw2Mumble.PlayerCharacter.CurrentMount != MountType.None && IsMountSwitchable())
            {
                await (_helper.GetCurrentlyActiveMount()?.DoUnmountAction() ?? Task.CompletedTask);
                Logger.Debug("DoDefaultMountActionAsync dismounted");
                return;
            }

            var instantMount = _helper.GetInstantMount();
            if (instantMount != null)
            {
                await instantMount.DoMountAction();
                Logger.Debug("DoDefaultMountActionAsync instantmount");
                return;
            }

            var defaultMount = _helper.GetDefaultMount();
            if (defaultMount != null && GameService.Input.Mouse.CameraDragging)
            {
                await (defaultMount?.DoMountAction() ?? Task.CompletedTask);
                Logger.Debug("DoDefaultMountActionAsync CameraDragging defaultmount");
                return;
            }

            switch (_settingDefaultMountBehaviour.Value)
            {
                case "DefaultMount":
                    await (defaultMount?.DoMountAction() ?? Task.CompletedTask);
                    Logger.Debug("DoDefaultMountActionAsync DefaultMountBehaviour defaultmount");
                    break;
                case "Radial":
                    if (ShouldShowModule())
                    {
                        _radial?.Show();
                        Logger.Debug("DoDefaultMountActionAsync DefaultMountBehaviour radial");
                    }
                    break;
            }
            return;
        }
        
        private async Task HandleCombatChangeAsync(object sender, ValueEventArgs<bool> e)
        {
            if (!e.Value)
            {
                await (_mounts.Where(m => m.QueuedTimestamp != null).OrderByDescending(m => m.QueuedTimestamp).FirstOrDefault()?.DoMountAction() ?? Task.CompletedTask);
                foreach (var mount in _mounts)
                {
                    mount.QueuedTimestamp = null;
                }
                _queueingSpinner?.Hide();
            }
        }
    }
}
