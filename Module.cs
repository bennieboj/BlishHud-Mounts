﻿using Blish_HUD;
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
using Mounts.Settings;
using Gw2Sharp;

namespace Manlaan.Mounts
{
    [Export(typeof(Blish_HUD.Modules.Module))]
    public class Module : Blish_HUD.Modules.Module
    {
        private static readonly Logger Logger = Logger.GetLogger<Module>();

        #region Service Managers
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;
        #endregion

        internal static SettingCollection settingscollection;
        internal static Collection<Thing> _things = new Collection<Thing>();
        internal static List<RadialThingSettings> RadialSettings;
        internal static List<IconThingSettings> IconThingSettings;
        internal static List<RadialThingSettings> OrderedRadialSettings() => RadialSettings.OrderBy(c => c.Order).ToList();
        internal static RadialThingSettings GetApplicableRadialSettings() => OrderedRadialSettings().FirstOrDefault(c => c.IsEnabled.Value && c.IsApplicable());


        public static string mountsDirectory;
        private TabbedWindow2 _settingsWindow;

        public static List<ThingImageFile> _thingImageFiles = new List<ThingImageFile>();

        public static string[] _mountBehaviour = new string[] { "DefaultMount", "Radial" };


        public static SettingEntry<KeyBinding> _settingDefaultMountBinding;
        public static SettingEntry<bool> _settingDisplayMountQueueing;
        public static SettingEntry<bool> _settingEnableMountQueueing;
        public static SettingEntry<Point> _settingDisplayMountQueueingLocation;
        public static SettingEntry<bool> _settingDragMountQueueing;
        public static SettingEntry<bool> _settingCombatLaunchMasteryUnlocked;
        public static SettingEntry<string> _settingDefaultMountBehaviour;
        public static SettingEntry<bool> _settingMountRadialSpawnAtMouse;
        public static SettingEntry<float> _settingMountRadialRadiusModifier;
        public static SettingEntry<float> _settingMountRadialStartAngle;
        public static SettingEntry<float> _settingMountRadialIconSizeModifier;
        public static SettingEntry<float> _settingMountRadialIconOpacity;
        public static SettingEntry<KeyBinding> _settingMountRadialToggleActionCameraKeyBinding;

        public static SettingEntry<bool> _settingDisplayModuleOnLoadingScreen;
        public static SettingEntry<bool> _settingMountAutomaticallyAfterLoadingScreen;

        public static SettingEntry<List<int>> _settingDrawIconIds;


        public static DebugControl _debug;
        private DrawRadial _radial;
        private ICollection<DrawIcons> _drawIcons = new List<DrawIcons>();
        private DrawOutOfCombat _drawOutOfCombat;
        private DrawMouseCursor _drawMouseCursor;
        private Helper _helper;
        private TextureCache _textureCache;

        private bool _lastIsMountSwitchable = false;


        [ImportingConstructor]
        public Module([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            _debug = new DebugControl();
            _helper = new Helper(Gw2ApiManager);
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
                Title = "Mounts & More",
                Parent = GameService.Graphics.SpriteScreen,
                Location = new Point(100, 100),
                Emblem = mountsIcon,
                Id = $"{Namespace}_SettingsWindow",
                SavesPosition = true,
            };
            _settingsWindow.Tabs.Add(new Tab(_textureCache.GetImgFile(TextureCache.SettingsTextureName), () => new SettingsView(_textureCache), Strings.Window_GeneralSettingsTab));
            _settingsWindow.Tabs.Add(new Tab(_textureCache.GetImgFile(TextureCache.RadialSettingsTextureName), () => new RadialThingSettingsView(), Strings.Window_RadialSettingsTab));
            _settingsWindow.Tabs.Add(new Tab(_textureCache.GetImgFile(TextureCache.IconSettingsTextureName), () => new IconThingSettingsView(), Strings.Window_IconSettingsTab));
            _settingsWindow.Tabs.Add(new Tab(_textureCache.GetImgFile(TextureCache.SupportMeTabTextureName), () => new SupportMeView(_textureCache), Strings.Window_SupportMeTab));
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


        private void MigrateRadialThingSettings(SettingCollection settings)
        {
            if (settings.ContainsSetting("DefaultFlyingMountChoice"))
            {
                var flyingRadialSettings = RadialSettings.Single(c => c.Name == "IsPlayerGlidingOrFalling");
                var settingDefaultFlyingMountChoice = settings["DefaultFlyingMountChoice"] as SettingEntry<string>;
                if (settingDefaultFlyingMountChoice.Value != "Disabled" && _things.Count(t => t.Name == settingDefaultFlyingMountChoice.Value) == 1)
                {
                    flyingRadialSettings.ApplyInstantlyIfSingle.Value = true;
                    flyingRadialSettings.IsEnabled.Value = true;
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
                    underwaterRadialSettings.ApplyInstantlyIfSingle.Value = true;
                    underwaterRadialSettings.IsEnabled.Value = true;
                    underwaterRadialSettings.SetThings(new List<Thing> { _things.Single(t => t.Name == settingDefaultWaterMountChoice.Value) });
                }
                settings.UndefineSetting("DefaultWaterMountChoice");
            }


            var defaultRadialSettings = RadialSettings.Single(c => c.Name == "Default");
            if (settings.ContainsSetting("DefaultMountChoice"))
            {
                var settingDefaultMountChoice = settings["DefaultMountChoice"] as SettingEntry<string>;

                defaultRadialSettings.DefaultThingChoice.Value = settingDefaultMountChoice.Value;

                settings.UndefineSetting("DefaultMountChoice");
            }

            if (settings.ContainsSetting("MountRadialRemoveCenterMount"))
            {
                var settingMountRadialRemoveCenterMount = settings["MountRadialRemoveCenterMount"] as SettingEntry<bool>;

                defaultRadialSettings.RemoveCenterMount.Value = settingMountRadialRemoveCenterMount.Value;

                settings.UndefineSetting("MountRadialRemoveCenterMount");
            }

            if (settings.ContainsSetting("MountRadialCenterMountBehavior"))
            {
                var settingMountRadialCenterMountBehavior = settings["MountRadialCenterMountBehavior"] as SettingEntry<string>;
                if (Enum.TryParse<CenterBehavior>(settingMountRadialCenterMountBehavior.Value, out var result))
                {
                    defaultRadialSettings.CenterThingBehavior.Value = result;
                }
                settings.UndefineSetting("MountRadialCenterMountBehavior");
            }
        }


        private void MigrateIconThingSettings(SettingCollection settings)
        {
            var defaultIconThingSettings = IconThingSettings.Single(c => c.IsDefault);
            if (settings.ContainsSetting("MountDisplayManualIcons"))
            {
                var settingMountDisplayManualIcons = settings["MountDisplayManualIcons"] as SettingEntry<bool>;
                defaultIconThingSettings.IsEnabled.Value = settingMountDisplayManualIcons.Value;
                settings.UndefineSetting("MountDisplayManualIcons");
            }
            
            if (settings.ContainsSetting("MountDisplayCornerIcons"))
            {
                var settingMountDisplayCornerIcons = settings["MountDisplayCornerIcons"] as SettingEntry<bool>;
                defaultIconThingSettings.DisplayCornerIcons.Value = settingMountDisplayCornerIcons.Value;
                settings.UndefineSetting("MountDisplayCornerIcons");
            }

            if (settings.ContainsSetting("Orientation"))
            {
                var settingOrientation = settings["Orientation"] as SettingEntry<string>;
                if (Enum.TryParse<IconOrientation>(settingOrientation.Value, out var result))
                {
                    defaultIconThingSettings.Orientation.Value = result;
                }
                settings.UndefineSetting("Orientation");
            }

            if (settings.ContainsSetting("MountLoc"))
            {
                var _settingLoc = settings["MountLoc"] as SettingEntry<Point>;
                defaultIconThingSettings.Location.Value = _settingLoc.Value;
                settings.UndefineSetting("MountLoc");
            }

            if (settings.ContainsSetting("MountDrag"))
            {
                var _settingDrag = settings["MountDrag"] as SettingEntry<bool>;
                defaultIconThingSettings.IsDraggingEnabled.Value = _settingDrag.Value;
                settings.UndefineSetting("MountDrag");
            }

            if (settings.ContainsSetting("MountImgWidth"))
            {
                var _settingMountImgWidth = settings["MountImgWidth"] as SettingEntry<int>;
                defaultIconThingSettings.Size.Value = _settingMountImgWidth.Value;
                settings.UndefineSetting("MountImgWidth");
            }

            if (settings.ContainsSetting("MountOpacity"))
            {
                var _settingMountOpacity = settings["MountOpacity"] as SettingEntry<float>;
                defaultIconThingSettings.Opacity.Value = _settingMountOpacity.Value;
                settings.UndefineSetting("MountOpacity");
            }
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            settingscollection = settings;
            var orderedThings = new List<Thing> {
                new UnMount(settings, _helper),
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
                new ScanForRift(settings, _helper),
                new SkyscaleLeap(settings, _helper),
                new Chair(settings, _helper),
                new Music(settings, _helper),
                new Held(settings, _helper),
                new Toy(settings, _helper),
                new Tonic(settings, _helper)
            };
            _things = new Collection<Thing>(orderedThings);
            var thingsForMigration = orderedThings.Where(t => t.IsAvailable).ToList();


            _settingDefaultMountBinding = settings.DefineSetting("DefaultMountBinding", new KeyBinding(Keys.None), () => Strings.Setting_DefaultMountBinding, () => "");
            _settingDefaultMountBinding.Value.Enabled = true;
            _settingDefaultMountBinding.Value.Activated += async delegate { await DoDefaultMountActionAsync(); };
            _settingDefaultMountBinding.Value.BindingChanged += UpdateSettings;
            _settingDefaultMountBehaviour = settings.DefineSetting("DefaultMountBehaviour", "Radial", () => Strings.Setting_DefaultMountBehaviour, () => "");
            _settingDisplayMountQueueing = settings.DefineSetting("DisplayMountQueueing", false);
            _settingEnableMountQueueing = settings.DefineSetting("EnableMountQueueing", false);
            _settingDragMountQueueing = settings.DefineSetting("DragMountQueueing", false);
            _settingCombatLaunchMasteryUnlocked = settings.DefineSetting("CombatLaunchMasteryUnlocked", false);
            _settingDisplayMountQueueingLocation = settings.DefineSetting("DisplayMountQueueingLocation", new Point(200,200));
            _settingMountRadialSpawnAtMouse = settings.DefineSetting("MountRadialSpawnAtMouse", false, () => Strings.Setting_MountRadialSpawnAtMouse, () => "");
            _settingMountRadialIconSizeModifier = settings.DefineSetting("MountRadialIconSizeModifier", 0.28f, () => Strings.Setting_MountRadialIconSizeModifier, () => "");
            _settingMountRadialIconSizeModifier.SetRange(0.05f, 1f);
            _settingMountRadialRadiusModifier = settings.DefineSetting("MountRadialRadiusModifier", 0.6f, () => Strings.Setting_MountRadialRadiusModifier, () => "");
            _settingMountRadialRadiusModifier.SetRange(0.2f, 1f);
            _settingMountRadialStartAngle = settings.DefineSetting("MountRadialStartAngle", 0.0f, () => Strings.Setting_MountRadialStartAngle, () => "");
            _settingMountRadialStartAngle.SetRange(0.0f, 1.0f);
            _settingMountRadialIconOpacity = settings.DefineSetting("MountRadialIconOpacity", 0.7f, () => Strings.Setting_MountRadialIconOpacity, () => "");
            _settingMountRadialIconOpacity.SetRange(0.05f, 1f);
            _settingMountRadialToggleActionCameraKeyBinding = settings.DefineSetting("MountRadialToggleActionCameraKeyBinding", new KeyBinding(Keys.F10), () => Strings.Setting_MountRadialToggleActionCameraKeyBinding, () => "");
            _settingDrawIconIds = settings.DefineSetting("DrawIconIds", new List<int> { 0 });

            _settingDisplayModuleOnLoadingScreen = settings.DefineSetting("DisplayModuleOnLoadingScreen", false, () => Strings.Setting_DisplayModuleOnLoadingScreen, () => "");
            _settingMountAutomaticallyAfterLoadingScreen = settings.DefineSetting("MountAutomaticallyAfterLoadingScreen", false, () => Strings.Setting_MountAutomaticallyAfterLoadingScreen, () => "");



            RadialSettings = new List<RadialThingSettings>
            {
                new RadialThingSettings(settings, "IsPlayerMounted", 0, _helper.IsPlayerMounted, true, true, _things.Where(t => t is UnMount).ToList()),
                new RadialThingSettings(settings, "IsPlayerInWvwMap", 1, _helper.IsPlayerInWvwMap, true, true, _things.Where(t => t is Warclaw).ToList()),
                new RadialThingSettings(settings, "IsPlayerGlidingOrFalling", 2, _helper.IsPlayerGlidingOrFalling, false, false, _things.Where(t => t is Griffon || t is Skyscale).ToList()),
                new RadialThingSettings(settings, "IsPlayerUnderWater", 3, _helper.IsPlayerUnderWater, false, false, _things.Where(t => t is Skimmer || t is SiegeTurtle).ToList()),
                new RadialThingSettings(settings, "IsPlayerOnWaterSurface", 4, _helper.IsPlayerOnWaterSurface, false, true, _things.Where(t => t is Skiff).ToList()),
                new RadialThingSettings(settings, "Default", 99, () => true, true, false, thingsForMigration)
            };
            MigrateRadialThingSettings(settings);

            IconThingSettings = new List<IconThingSettings>
            {
                new IconThingSettings(settings, 0, "Default", thingsForMigration)
            };
            IconThingSettings.AddRange(_settingDrawIconIds.Value.Skip(1).Select(id => new IconThingSettings(settings, id)));
            MigrateIconThingSettings(settings);

            foreach (var t in _things)
            {
                t.KeybindingSetting.Value.BindingChanged += UpdateSettings;
                t.ImageFileNameSetting.SettingChanged += UpdateSettings;
            }
            _settingDisplayModuleOnLoadingScreen.SettingChanged += UpdateSettings;
            _settingMountAutomaticallyAfterLoadingScreen.SettingChanged += UpdateSettings;
            _settingDisplayMountQueueing.SettingChanged += UpdateSettings;
            _settingEnableMountQueueing.SettingChanged += UpdateSettings;
            _settingDragMountQueueing.SettingChanged += UpdateSettings;
            _settingDisplayMountQueueingLocation.SettingChanged += UpdateSettings;
            _settingMountRadialSpawnAtMouse.SettingChanged += UpdateSettings;
            _settingMountRadialIconSizeModifier.SettingChanged += UpdateSettings;
            _settingMountRadialRadiusModifier.SettingChanged += UpdateSettings;
            _settingMountRadialStartAngle.SettingChanged += UpdateSettings;
            _settingMountRadialToggleActionCameraKeyBinding.Value.BindingChanged += UpdateSettings;
            _settingMountRadialIconOpacity.SettingChanged += UpdateSettings;
            _settingDrawIconIds.SettingChanged += UpdateSettings;
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
            RadialSettings.ForEach(c => _debug.Add($"RadialSettings {c.Order} {c.Name}", () => $"IsApplicable: {c.IsApplicable()}, Center: {c.GetCenterThing()?.DisplayName}"));
            _debug.Add("Applicable RadialSettings Name", () => $"{GetApplicableRadialSettings()?.Name}");
            _debug.Add("Applicable RadialSettings Actions", () => $"{string.Join(", ", GetApplicableRadialSettings()?.Things.Select(t => t.DisplayName))}");

            Gw2ApiManager.SubtokenUpdated += async delegate
            {
                await _helper.IsCombatLaunchUnlockedAsync();
            };

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
                foreach (var drawIcons in _drawIcons) drawIcons.Show();
            }
            else
            {
                foreach (var drawIcons in _drawIcons) drawIcons.Hide();
            }

            if (_things.Any(m => m.QueuedTimestamp != null))
            {
                _drawOutOfCombat?.ShowSpinner();
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
            foreach (var drawIcons in _drawIcons) drawIcons.Dispose();
            _radial?.Dispose();

            foreach (var t in _things)
            {
                t.KeybindingSetting.SettingChanged -= UpdateSettings;
                t.ImageFileNameSetting.SettingChanged += UpdateSettings;
                t.DisposeCornerIcon();
            }

            _settingDisplayMountQueueing.SettingChanged -= UpdateSettings;
            _settingEnableMountQueueing.SettingChanged -= UpdateSettings;
            _settingDragMountQueueing.SettingChanged -= UpdateSettings;
            _settingDisplayMountQueueingLocation.SettingChanged -= UpdateSettings;
            _settingMountRadialSpawnAtMouse.SettingChanged -= UpdateSettings;
            _settingMountRadialIconSizeModifier.SettingChanged -= UpdateSettings;
            _settingMountRadialRadiusModifier.SettingChanged -= UpdateSettings;
            _settingMountRadialStartAngle.SettingChanged -= UpdateSettings;
            _settingMountRadialToggleActionCameraKeyBinding.SettingChanged -= UpdateSettings;
            _settingMountRadialIconOpacity.SettingChanged -= UpdateSettings;

            _settingDisplayModuleOnLoadingScreen.SettingChanged -= UpdateSettings;
            _settingMountAutomaticallyAfterLoadingScreen.SettingChanged -= UpdateSettings;
            _settingDrawIconIds.SettingChanged -= UpdateSettings;
        }

        private void UpdateSettings(object sender = null, ValueChangedEventArgs<string> e = null) {
            DrawUI();
        }
        private void UpdateSettings(object sender = null, EventArgs e = null) {
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
        private void UpdateSettings(object sender, ValueChangedEventArgs<List<int>> e)
        {
            DrawUI();
        }

        private void DrawUI()
        {
            foreach (var drawIcons in _drawIcons) drawIcons.Dispose();
            _drawIcons = IconThingSettings.Select(iconSetting => new DrawIcons(iconSetting, _helper, _textureCache)).ToList();

            _drawOutOfCombat?.Dispose();
            _drawOutOfCombat = new DrawOutOfCombat();

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
            if (things.Count() == 1 && things.FirstOrDefault().IsAvailable && selectedRadialSettings.ApplyInstantlyIfSingle.Value)
            {
                await things.FirstOrDefault()?.DoAction();
                Logger.Debug("DoDefaultMountActionAsync instantmount");
                return;
            }

            var defaultThing = selectedRadialSettings.GetDefaultThing();
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
                _drawOutOfCombat?.HideSpinner();
            }
        }
    }
}
