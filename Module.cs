using Blish_HUD;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Blish_HUD.Input;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.ObjectModel;
using Manlaan.Mounts.Controls;
using System.Collections.Generic;
using Manlaan.Mounts.Views;
using System.IO;
using Manlaan.Mounts.Things.Mounts;
using Manlaan.Mounts.Things;
using Mounts;

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

        internal static Collection<Thing> _things = new Collection<Thing>();
        internal static List<Thing> _availableOrderedThings => _things.Where(m => m.IsAvailable).OrderBy(m => m.OrderSetting.Value).ToList();
        internal static List<RadialThingSettings> RadialSettings;
        internal static List<RadialThingSettings> OrderedRadialSettings() => RadialSettings.OrderBy(c => c.Order).ToList();
        internal static RadialThingSettings GetApplicableRadialSettings() => OrderedRadialSettings().FirstOrDefault(c => c.IsEnabledSetting.Value && c.IsApplicable());


        public static string mountsDirectory;
        private TabbedWindow2 _settingsWindow;

        public static List<ThingImageFile> _thingImageFiles = new List<ThingImageFile>();

        public static string[] _mountBehaviour = new string[] { "DefaultMount", "Radial" };
        public static string[] _mountOrientation = new string[] { "Horizontal", "Vertical" };
        public static string[] _mountRadialCenterMountBehavior = new string[] { "None", "Default", "LastUsed" };

        public static SettingEntry<string> _settingDefaultMountChoice;
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
        public static SettingEntry<Point> _settingLoc;
        public static SettingEntry<bool> _settingDrag;
        public static SettingEntry<int> _settingImgWidth;
        public static SettingEntry<float> _settingOpacity;


        public static DebugControl _debug;
        private DrawRadial _radial;
        private DrawCornerIcons _drawCornerIcons;
        private DrawManualIcons _drawManualIcons;
        private LoadingSpinner _queueingSpinner;
        private DrawMouseCursor _drawMouseCursor;
        private Helper _helper;
        private TextureCache _textureCache;

        private bool _lastIsMountSwitchable = false;


        [ImportingConstructor]
        public Module([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            _debug = new DebugControl();
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
                "warclaw.png",
                "fishing.png",
                "skiff.png",
                "jadebotwaypoint.png",
                "chair.png",
                "music.png",
                "held.png",
                "toy.png",
                "tonic.png",
                "scanforrift.png",
                "skyscaleleap.png",
                "unmount.png"
            };
            mountsDirectory = DirectoriesManager.GetFullDirectoryPath("mounts");
            mountsFilesInRef.ForEach(f => ExtractFile(f, mountsDirectory));
            _thingImageFiles = Directory.GetFiles(mountsDirectory, ".")
                .Where(file => file.ToLower().Contains(".png"))
                .Select(file => new ThingImageFile() { Name = file.Substring(mountsDirectory.Length + 1) }).ToList();
            _textureCache = new TextureCache(ContentsManager);

            GameService.Gw2Mumble.PlayerCharacter.IsInCombatChanged += async (sender, e) => await HandleCombatChangeAsync(sender, e);

            var mountsIcon = _textureCache.GetImgFile(TextureCache.MountLogoTextureName);

            _settingsWindow = new TabbedWindow2(
                                    _textureCache.GetImgFile(TextureCache.TabBackgroundTextureName),
                                    new Rectangle(35, 36, 1300, 900),
                                    new Rectangle(95, 42, 1183 + 38, 900)
                                   )
            {
                Title = "Mounts",
                Parent = GameService.Graphics.SpriteScreen,
                Location = new Point(100, 100),
                Emblem = mountsIcon,
                Id = $"{this.Namespace}_SettingsWindow",
                SavesPosition = true,
            };
            var x = Strings.Window_AllSettingsTab;
            _settingsWindow.Tabs.Add(new Tab(_textureCache.GetImgFile(TextureCache.SettingsTextureName), () => new SettingsView(_textureCache), x));
            _settingsWindow.Tabs.Add(new Tab(_textureCache.GetImgFile(TextureCache.RadialSettingsTextureName), () => new RadialThingSettingsView(), Strings.Window_RadialSettingsTab));
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
            if (_things.All(m => m.ImageFileNameSetting.Value.Equals("")))
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
                foreach (var mount in _things)
                {
                    mount.ImageFileNameSetting.Value = $"{mount.ImageFileName}{partOfFileName}.png";
                }                
            }
        }


        /*
         * Migrate from defaultwatermount, etc to RadialTHingSettings
         */
        private void MigrateRadialThingSettings(SettingCollection settings)
        {
            if (settings.ContainsSetting("DefaultFlyingMountChoice"))
            {
                var flyingRadialSettings = RadialSettings.Single(c => c.Name == "IsPlayerGlidingOrFalling");
                var settingDefaultFlyingMountChoice = settings["DefaultFlyingMountChoice"] as SettingEntry<string>;
                if (settingDefaultFlyingMountChoice.Value != "Disabled" && _things.Count(t => t.Name == settingDefaultFlyingMountChoice.Value) == 1)
                {
                    flyingRadialSettings.ApplyInstantlyIfSingleSetting.Value = true;
                    flyingRadialSettings.IsEnabledSetting.Value = true;
                    flyingRadialSettings.SetThings(new List<Thing> { _things.Single(t => t.Name == settingDefaultFlyingMountChoice.Value) });
                }
                settings.UndefineSetting("DefaultFlyingMountChoice");
            }

            if (settings.ContainsSetting("DefaultWaterMountChoice"))
            {
                var underwaterRadialSettings = RadialSettings.Single(c => c.Name == "IsPlayerUnderWater");
                var settingDefaultWaterMountChoice = settings["DefaultWaterMountChoice"] as SettingEntry<string>;
                if (settingDefaultWaterMountChoice.Value != "Disabled" && _things.Count(t => t.Name == settingDefaultWaterMountChoice.Value) == 1)
                {
                    underwaterRadialSettings.ApplyInstantlyIfSingleSetting.Value = true;
                    underwaterRadialSettings.IsEnabledSetting.Value = true;
                    underwaterRadialSettings.SetThings(new List<Thing> { _things.Single(t => t.Name == settingDefaultWaterMountChoice.Value) });
                }
                settings.UndefineSetting("DefaultWaterMountChoice");
            }
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            _things = new Collection<Thing>
            {
                new Raptor(settings, _helper),
                new Springer(settings, _helper),
                new Skimmer(settings, _helper),
                new Jackal(settings, _helper),
                new Griffon(settings, _helper),
                new RollerBeetle(settings, _helper),
                new Warclaw(settings, _helper),
                new Skyscale(settings, _helper),
                new SiegeTurtle(settings, _helper),
                new Fishing(settings, _helper),
                new Skiff(settings, _helper),
                new JadeBotWaypoint(settings, _helper),
                new Chair(settings, _helper),
                new Music(settings, _helper),
                new Held(settings, _helper),
                new Toy(settings, _helper),
                new Tonic(settings, _helper),
                new ScanForRift(settings, _helper),
                new SkyscaleLeap(settings, _helper),
                new UnMount(settings, _helper)
            };


            _settingDefaultMountBinding = settings.DefineSetting("DefaultMountBinding", new KeyBinding(Keys.None), () => Strings.Setting_DefaultMountBinding, () => "");
            _settingDefaultMountBinding.Value.Enabled = true;
            _settingDefaultMountBinding.Value.Activated += async delegate { await DoDefaultMountActionAsync(); };
            _settingDefaultMountChoice = settings.DefineSetting("DefaultMountChoice", "Disabled", () => Strings.Setting_DefaultMountChoice, () => "");
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

            RadialSettings = new List<RadialThingSettings>
            {
                new RadialThingSettings(settings, "IsPlayerMounted", 0, _helper.IsPlayerMounted, true, true, _things.Where(t => t is UnMount).ToList()),
                new RadialThingSettings(settings, "IsPlayerInWvwMap", 1, _helper.IsPlayerInWvwMap, true, true, _things.Where(t => t is Warclaw).ToList()),
                new RadialThingSettings(settings, "IsPlayerGlidingOrFalling", 2, _helper.IsPlayerGlidingOrFalling, false, false, _things.Where(t => t is Griffon || t is Skyscale).ToList()),
                new RadialThingSettings(settings, "IsPlayerUnderWater", 3, _helper.IsPlayerUnderWater, false, false, _things.Where(t => t is Skimmer || t is SiegeTurtle).ToList()),
                new RadialThingSettings(settings, "IsPlayerOnWaterSurface", 4, _helper.IsPlayerOnWaterSurface, false, true, _things.Where(t => t is Skiff).ToList()),
                new RadialThingSettings(settings, "Default", 99, () => true, true, false, _things)
            };
            MigrateRadialThingSettings(settings);

            foreach (var t in _things)
            {
                t.KeybindingSetting.SettingChanged += UpdateSettings;
                t.ImageFileNameSetting.SettingChanged += UpdateSettings;
            }
            _settingDefaultMountChoice.SettingChanged += UpdateSettings;
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
            RadialSettings.ForEach(c => _debug.Add(c.Name, () => $"{c.IsApplicable()}"));
            _debug.Add("ApplicableRadialSettings Name", () => $"{GetApplicableRadialSettings()?.Name}");
            _debug.Add("ApplicableRadialSettings Actions", () => $"{string.Join(", ", GetApplicableRadialSettings()?.Things.Select(t => t.DisplayName))}"); 

            DrawUI();

            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        protected override void Update(GameTime gameTime)
        {
            _helper.UpdatePlayerGlidingOrFalling(gameTime);

            var isMountSwitchable = CanThingBeActivated();
            var moduleHidden = _lastIsMountSwitchable && !isMountSwitchable;
            var moduleShown = !_lastIsMountSwitchable && isMountSwitchable;
            var currentCharacterName = GameService.Gw2Mumble.PlayerCharacter.Name;
            var inUseMountsCount = _things.Count(m => m.IsInUse());
            if (moduleHidden && inUseMountsCount == 1 && _settingMountAutomaticallyAfterLoadingScreen.Value && GameService.GameIntegration.Gw2Instance.Gw2HasFocus)
            {
                _helper.StoreThingForLaterActivation(_things.Single(m => m.IsInUse()), currentCharacterName);
            }
            if (moduleShown && inUseMountsCount == 0 && _helper.IsCharacterTheSameAfterMapLoad(currentCharacterName) && GameService.GameIntegration.Gw2Instance.Gw2HasFocus)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                _helper.DoThingActionForLaterActivation();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }

            _lastIsMountSwitchable = isMountSwitchable;

            bool shouldShowModule = ShouldShowModule();
            if (shouldShowModule)
            {
                _drawManualIcons?.Show();
                foreach (var mount in _availableOrderedThings)
                {
                    mount.CornerIcon?.Show();
                }
            }
            else
            {
                _drawManualIcons?.Hide();
                foreach (var mount in _availableOrderedThings)
                {
                    mount.CornerIcon?.Hide();
                }
            }

            if (_settingDisplayMountQueueing.Value && _things.Any(m => m.QueuedTimestamp != null))
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
            return _settingDisplayModuleOnLoadingScreen.Value || CanThingBeActivated();
        }

        public static bool CanThingBeActivated()
        {
            return GameService.GameIntegration.Gw2Instance.IsInGame && !GameService.Gw2Mumble.UI.IsMapOpen;
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            _debug?.Dispose();
            _drawManualIcons?.Dispose();
            _radial?.Dispose();

            foreach (var t in _things)
            {
                t.KeybindingSetting.SettingChanged -= UpdateSettings;
                t.ImageFileNameSetting.SettingChanged += UpdateSettings;
                t.DisposeCornerIcon();
            }

            _settingDefaultMountChoice.SettingChanged -= UpdateSettings;
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

        private void DrawUI()
        {   
            _drawCornerIcons?.Dispose();
            if (_settingDisplayCornerIcons.Value)
                _drawCornerIcons = new DrawCornerIcons(_textureCache);

            _drawManualIcons?.Dispose();
            if (_settingDisplayManualIcons.Value)
                _drawManualIcons = new DrawManualIcons(_helper, _textureCache);

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

            var selectedRadialSettings = GetApplicableRadialSettings();
            var things = selectedRadialSettings.Things;
            if (things.Count() == 1 && things.FirstOrDefault().IsAvailable && selectedRadialSettings.ApplyInstantlyIfSingleSetting.Value)
            {
                await things.FirstOrDefault()?.DoAction();
                Logger.Debug("DoDefaultMountActionAsync instantmount");
                return;
            }

            var defaultThing = _helper.GetDefaultThing();
            if (defaultThing != null && GameService.Input.Mouse.CameraDragging)
            {
                await (defaultThing?.DoAction() ?? Task.CompletedTask);
                Logger.Debug("DoDefaultMountActionAsync CameraDragging defaultmount");
                return;
            }

            switch (_settingDefaultMountBehaviour.Value)
            {
                case "DefaultMount":
                    await (defaultThing?.DoAction() ?? Task.CompletedTask);
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
                await (_things.Where(m => m.QueuedTimestamp != null).OrderByDescending(m => m.QueuedTimestamp).FirstOrDefault()?.DoAction() ?? Task.CompletedTask);
                foreach (var thing in _things)
                {
                    thing.QueuedTimestamp = null;
                }
                _queueingSpinner?.Hide();
            }
        }
    }
}
