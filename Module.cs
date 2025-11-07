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
using Mounts.Settings;
using Newtonsoft.Json;
using Taimi.UndaDaSea_BlishHUD;

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
        //things
        internal static Collection<Thing> _things = new Collection<Thing>();
        //radial settings, contextual and user-defined
        internal static List<ContextualRadialThingSettings> ContextualRadialSettings;
        internal static List<UserDefinedRadialThingSettings> UserDefinedRadialSettings;
        public static SettingEntry<List<int>> _settingUserDefinedRadialIds;
        //icons
        internal static List<IconThingSettings> IconThingSettings;
        public static SettingEntry<List<int>> _settingDrawIconIds;

        public static List<ThingImageFile> _thingImageFiles = new List<ThingImageFile>();
        public static string thingsDirectory;

        public static string[] _keybindBehaviours = new string[] { "Default", "Radial" };

        private bool previousTriggeringState = false;
        private DateTime? lastTriggered = null;
        private TappedModuleKeybindState tappedModuleKeybind = TappedModuleKeybindState.Unknown;

        public static SettingEntry<int> _settingsLastRunMigrationVersion;

        public static SettingEntry<KeyBinding> _settingDefaultMountBinding;
        public static SettingEntry<bool> _settingBlockSequenceFromGw2;
        public static SettingEntry<bool> _settingDisplayMountQueueing;
        public static SettingEntry<bool> _settingDisplayLaterActivation;
        public static SettingEntry<bool> _settingDisplayGroundTargetingAction;
        public static SettingEntry<GroundTargeting> _settingGroundTargeting;
        public static SettingEntry<bool> _settingEnableMountQueueing;
        public static SettingEntry<Point> _settingInfoPanelLocation;
        public static SettingEntry<bool> _settingDragInfoPanel;
        public static SettingEntry<bool> _settingCombatLaunchMasteryUnlocked;
        public static SettingEntry<string> _settingDefaultMountBehaviour;
        public static SettingEntry<string> _settingKeybindBehaviour;
        public static SettingEntry<bool> _settingMountRadialSpawnAtMouse;
        public static SettingEntry<float> _settingMountRadialRadiusModifier;
        public static SettingEntry<float> _settingMountRadialStartAngle;
        public static SettingEntry<float> _settingMountRadialIconSizeModifier;
        public static SettingEntry<float> _settingMountRadialIconOpacity;
        public static SettingEntry<KeyBinding> _settingMountRadialToggleActionCameraKeyBinding;
        public static SettingEntry<bool> _settingDisplayModuleOnLoadingScreen;
        public static SettingEntry<bool> _settingMountAutomaticallyAfterLoadingScreen;
        public static SettingEntry<KeyBinding> _settingJumpBinding;
        public static SettingEntry<float> _settingFallingOrGlidingUpdateFrequency;
        public static SettingEntry<int> _settingTapThresholdInMilliseconds;



        private TabbedWindow2 _settingsWindow;
        public static DebugControl _debug;
        private DrawRadial _radial;
        private ICollection<DrawIcons> _drawIcons = new List<DrawIcons>();
        private InfoPanel _drawInfoPanel;
        private DrawMouseCursor _drawMouseCursor;
        private Helper _helper;
        private TextureCache _textureCache;
        public static List<SkyLake> _skyLakes = new List<SkyLake>();
        private bool _lastIsThingSwitchable = false;
        private int _lastInUseThingsCount = 0;


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
                "unmount.png",
                "unmount-trans.png",
                "fishing-trans.png",
                "fishing-trans-color.png",
                "jadebotwaypoint-trans.png",
                "jadebotwaypoint-trans-color.png",
                "scanforrift-trans.png",
                "scanforrift-trans-color.png",
                "skiff-trans.png",
                "skiff-trans-color.png",
                "skyscaleleap-trans.png",
                "skyscaleleap-trans-color.png",
                "tonic-paint.png",
                "tonic-white.png",
                "toy-paint.png",
                "toy-white.png",
                "chair-paint.png",
                "chair-whiite.png",
                "held-paint.png",
                "held-white.png",
                "music-paint.png",
                "music-white.png",
                "skimmer-remix.png",
                "skyscaleleap-remix.png",
                "skyscale-remix.png",
                "springer-remix.png",
                "tonic-remix.png",
                "toy-remix.png",
                "turtle-remix.png",
                "unmount-remix.png",
                "warclaw-remix.png",
                "chair-remix.png",
                "fishing-remix.png",
                "griffon-remix.png",
                "held-remix.png",
                "jackal-remix.png",
                "jadebotwaypoint-remix.png",
                "music-remix.png",
                "raptor-remix.png",
                "roller-remix.png",
                "scanforrift-remix.png",
                "skiff-remix.png",
                "summonconjureddoorway.png",
                "summonconjureddoorway-trans.png",
                "summonconjureddoorway-trans-color.png",
                "griffon_natural.png",
                "jackal_natural.png",
                "raptor_natural.png",
                "roller_natural.png",
                "skimmer_natural.png",
                "skyscale_natural.png",
                "springer_natural.png",
                "turtle_natural.png",
                "warclaw_natural.png"
            };
            thingsDirectory = DirectoriesManager.GetFullDirectoryPath("mounts");
            mountsFilesInRef.ForEach(f => ExtractFile(f, thingsDirectory));
            _thingImageFiles = Directory.GetFiles(thingsDirectory, ".")
                .Where(file => file.ToLower().Contains(".png"))
                .Select(file => new ThingImageFile() { Name = file.Substring(thingsDirectory.Length + 1) }).ToList();
            _textureCache = new TextureCache(ContentsManager);

            _skyLakes = LoadSkyLakesFromJson();

            GameService.Gw2Mumble.PlayerCharacter.IsInCombatChanged += async (sender, e) => await HandleCombatChangeAsync(sender, e);

            var mountsIcon = _textureCache.GetImgFile(TextureCache.ModuleLogoTextureName);

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
            _settingsWindow.Tabs.Add(new Tab(_textureCache.GetImgFile(TextureCache.RadialSettingsTextureName), () => new RadialThingSettingsView(DoKeybindActionAsync, _helper), Strings.Window_RadialSettingsTab));
            _settingsWindow.Tabs.Add(new Tab(_textureCache.GetImgFile(TextureCache.IconSettingsTextureName), () => new IconThingSettingsView(), Strings.Window_IconSettingsTab));
            _settingsWindow.Tabs.Add(new Tab(_textureCache.GetImgFile(TextureCache.SupportMeTabTextureName), () => new SupportMeView(_textureCache), Strings.Window_SupportMeTab));
        }

        public List<SkyLake> LoadSkyLakesFromJson()
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new Vector3Converter());
            using (StreamReader stream = new StreamReader(ContentsManager.GetFileStream("SkyLakes.json")))
            using (JsonReader reader = new JsonTextReader(stream))
            {
                return serializer.Deserialize<List<SkyLake>>(reader);
            }
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

        /***
         * version 1.5.0 = migration version 2
         * 
         * ***/

        private void MigrateToInfoPanelLocation(SettingCollection settings)
        {
            if (settings.ContainsSetting("DisplayMountQueueingLocation"))
            {
                var settingDisplayMountQueueingLocation = settings["DisplayMountQueueingLocation"] as SettingEntry<Point>;
                _settingInfoPanelLocation.Value = new Point(settingDisplayMountQueueingLocation.Value.X, settingDisplayMountQueueingLocation.Value.Y);
            }
            if (settings.ContainsSetting("DragMountQueueing"))
            {
                var settingDragMountQueueing = settings["DragMountQueueing"] as SettingEntry<bool>;
                _settingDragInfoPanel.Value = settingDragMountQueueing.Value;
            }
        }

        /***
         * version 1.4.0 = migration version 1
         * 
         ***/

        private void MigrateAwayFromMount(SettingCollection settings)
        {
            if (_settingDefaultMountBehaviour.Value == "DefaultMount")
            {
                _settingKeybindBehaviour.Value = "Default";
            }
            if (_settingDefaultMountBehaviour.Value == "Radial")
            {
                _settingKeybindBehaviour.Value = "Radial";
            }
        }


        private void MigrateRadialThingSettings(SettingCollection settings)
        {
            if (settings.ContainsSetting("DefaultFlyingMountChoice"))
            {
                var flyingRadialSettings = ContextualRadialSettings.Single(c => c.Name == "IsPlayerGlidingOrFalling");
                var settingDefaultFlyingMountChoice = settings["DefaultFlyingMountChoice"] as SettingEntry<string>;
                if (settingDefaultFlyingMountChoice.Value != "Disabled" && _things.Count(t => t.Name == settingDefaultFlyingMountChoice.Value) == 1)
                {
                    flyingRadialSettings.ApplyInstantlyIfSingle.Value = true;
                    flyingRadialSettings.IsEnabled.Value = true;
                    flyingRadialSettings.SetThings(new List<Thing> { _things.Single(t => t.Name == settingDefaultFlyingMountChoice.Value) });
                }
            }

            if (settings.ContainsSetting("DefaultWaterMountChoice"))
            {
                var underwaterRadialSettings = ContextualRadialSettings.Single(c => c.Name == "IsPlayerUnderWater");
                var settingDefaultWaterMountChoice = settings["DefaultWaterMountChoice"] as SettingEntry<string>;
                if (settingDefaultWaterMountChoice.Value != "Disabled" && _things.Count(t => t.Name == settingDefaultWaterMountChoice.Value) == 1)
                {
                    underwaterRadialSettings.ApplyInstantlyIfSingle.Value = true;
                    underwaterRadialSettings.IsEnabled.Value = true;
                    underwaterRadialSettings.SetThings(new List<Thing> { _things.Single(t => t.Name == settingDefaultWaterMountChoice.Value) });
                }
            }


            var defaultRadialSettings = ContextualRadialSettings.Single(c => c.Name == "Default");
            if (settings.ContainsSetting("DefaultMountChoice"))
            {
                var settingDefaultMountChoice = settings["DefaultMountChoice"] as SettingEntry<string>;

                defaultRadialSettings.DefaultThingChoice.Value = settingDefaultMountChoice.Value;

            }

            if (settings.ContainsSetting("MountRadialRemoveCenterMount"))
            {
                var settingMountRadialRemoveCenterMount = settings["MountRadialRemoveCenterMount"] as SettingEntry<bool>;

                defaultRadialSettings.RemoveCenterThing.Value = settingMountRadialRemoveCenterMount.Value;
            }

            if (settings.ContainsSetting("MountRadialCenterMountBehavior"))
            {
                var settingMountRadialCenterMountBehavior = settings["MountRadialCenterMountBehavior"] as SettingEntry<string>;
                if (Enum.TryParse<CenterBehavior>(settingMountRadialCenterMountBehavior.Value, out var result))
                {
                    defaultRadialSettings.CenterThingBehavior.Value = result;
                }
            }
        }


        private void MigrateIconThingSettings(SettingCollection settings)
        {
            var defaultIconThingSettings = IconThingSettings.Single(c => c.IsDefault);
            if (settings.ContainsSetting("MountDisplayManualIcons"))
            {
                var settingMountDisplayManualIcons = settings["MountDisplayManualIcons"] as SettingEntry<bool>;
                defaultIconThingSettings.IsEnabled.Value = settingMountDisplayManualIcons.Value;
            }
            
            if (settings.ContainsSetting("MountDisplayCornerIcons"))
            {
                var settingMountDisplayCornerIcons = settings["MountDisplayCornerIcons"] as SettingEntry<bool>;
                defaultIconThingSettings.DisplayCornerIcons.Value = settingMountDisplayCornerIcons.Value;
            }

            if (settings.ContainsSetting("Orientation"))
            {
                var settingOrientation = settings["Orientation"] as SettingEntry<string>;
                if (Enum.TryParse<IconOrientation>(settingOrientation.Value, out var result))
                {
                    defaultIconThingSettings.Orientation.Value = result;
                }
            }

            if (settings.ContainsSetting("MountLoc"))
            {
                var _settingLoc = settings["MountLoc"] as SettingEntry<Point>;
                defaultIconThingSettings.Location.Value = _settingLoc.Value;
            }

            if (settings.ContainsSetting("MountDrag"))
            {
                var _settingDrag = settings["MountDrag"] as SettingEntry<bool>;
                defaultIconThingSettings.IsDraggingEnabled.Value = _settingDrag.Value;
            }

            if (settings.ContainsSetting("MountImgWidth"))
            {
                var _settingMountImgWidth = settings["MountImgWidth"] as SettingEntry<int>;
                defaultIconThingSettings.Size.Value = _settingMountImgWidth.Value;
            }

            if (settings.ContainsSetting("MountOpacity"))
            {
                var _settingMountOpacity = settings["MountOpacity"] as SettingEntry<float>;
                defaultIconThingSettings.Opacity.Value = _settingMountOpacity.Value;
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
                new SummonConjuredDoorway(settings, _helper),
                new Chair(settings, _helper),
                new Music(settings, _helper),
                new Held(settings, _helper),
                new Toy(settings, _helper),
                new Tonic(settings, _helper)
            };
            _things = new Collection<Thing>(orderedThings);
            var thingsForMigration = orderedThings.ToList();

            _settingsLastRunMigrationVersion = settings.DefineSetting("LastRunMigrationVersion", 0);
            _settingBlockSequenceFromGw2 = settings.DefineSetting("BlockSequenceFromGw2", true);
            _settingDefaultMountBinding = settings.DefineSetting("DefaultMountBinding", new KeyBinding(Keys.None), () => Strings.Setting_DefaultMountBinding, () => "");
            _settingDefaultMountBinding.Value.Enabled = true;
            _settingDefaultMountBinding.Value.BlockSequenceFromGw2 = _settingBlockSequenceFromGw2.Value;
            _settingDefaultMountBinding.Value.Activated += async delegate { await DoKeybindActionAsync(KeybindTriggerType.Module); };
            _settingDefaultMountBinding.Value.BindingChanged += UpdateSettings;
            _settingDefaultMountBehaviour = settings.DefineSetting("DefaultMountBehaviour", "Radial");
            _settingKeybindBehaviour = settings.DefineSetting("KeybindBehaviour", "Radial");
            _settingDisplayMountQueueing = settings.DefineSetting("DisplayMountQueueing", false);
            _settingEnableMountQueueing = settings.DefineSetting("EnableMountQueueing", false);
            _settingDisplayLaterActivation = settings.DefineSetting("DisplayLaterActivation", false);
            _settingDisplayGroundTargetingAction = settings.DefineSetting("DisplayGroundTargetingAction", false);
            _settingGroundTargeting = settings.DefineSetting("GroundTargeting", GroundTargeting.Normal);
            _settingCombatLaunchMasteryUnlocked = settings.DefineSetting("CombatLaunchMasteryUnlocked", false);
            _settingInfoPanelLocation = settings.DefineSetting("InfoPanelLocation", new Point(200, 200));
            _settingDragInfoPanel = settings.DefineSetting("DragInfoPanel", false);
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
            _settingUserDefinedRadialIds = settings.DefineSetting("UserDefinedRadialIds", new List<int> {});
            _settingDisplayModuleOnLoadingScreen = settings.DefineSetting("DisplayModuleOnLoadingScreen", false, () => Strings.Setting_DisplayModuleOnLoadingScreen, () => "");
            _settingMountAutomaticallyAfterLoadingScreen = settings.DefineSetting("MountAutomaticallyAfterLoadingScreen", false, () => Strings.Setting_MountAutomaticallyAfterLoadingScreen, () => "");
            _settingJumpBinding = settings.DefineSetting("JumpKeybinding", new KeyBinding(Keys.Space));
            _settingJumpBinding.Value.Enabled = true;
            _settingJumpBinding.Value.Activated += delegate { _helper.UpdateLastJumped(); };
            _settingFallingOrGlidingUpdateFrequency = settings.DefineSetting("FallingOrGlidingUpdateFrequency", 0.1f);
            _settingTapThresholdInMilliseconds = settings.DefineSetting("TapThresholdInMilliseconds", 500);


            ContextualRadialSettings = new List<ContextualRadialThingSettings>
            {
                new ContextualRadialThingSettings(settings, "IsPlayerMounted", 0, _helper.IsPlayerMounted, true, true, true, _things.Where(t => t is Raptor).ToList()),
                new ContextualRadialThingSettings(settings, "IsPlayerInWvwMap", 1, _helper.IsPlayerInWvwMap, true, true, false, _things.Where(t => t is Warclaw).ToList()),
                new ContextualRadialThingSettings(settings, "IsPlayerInCombat", 2, _helper.IsPlayerInCombat, false, true, false, _things.Where(t => t is Skyscale).ToList()),                
                new ContextualRadialThingSettings(settings, "IsPlayerUnderWater", 3, _helper.IsPlayerUnderWater, false, false, false, _things.Where(t => t is Skimmer || t is SiegeTurtle).ToList()),
                new ContextualRadialThingSettings(settings, "IsPlayerOnWaterSurface", 4, _helper.IsPlayerOnWaterSurface, false, true, false, _things.Where(t => t is Skiff).ToList()),
                new ContextualRadialThingSettings(settings, "IsPlayerGlidingOrFalling", 5, _helper.IsPlayerGlidingOrFalling, false, false, false, _things.Where(t => t is Griffon || t is Skyscale).ToList()),
                new ContextualRadialThingSettings(settings, "Default", 99, () => true, true, false, false, thingsForMigration)
            };


            IconThingSettings = new List<IconThingSettings>
            {
                new IconThingSettings(settings, 0, "Default", thingsForMigration)
            };
            IconThingSettings.AddRange(_settingDrawIconIds.Value.Skip(1).Select(id => new IconThingSettings(settings, id)));
            UserDefinedRadialSettings = _settingUserDefinedRadialIds.Value.Select(id => new UserDefinedRadialThingSettings(settings, id, DoKeybindActionAsync)).ToList();

            if (_settingsLastRunMigrationVersion.Value < 1)
            {
                MigrateRadialThingSettings(settings);
                MigrateIconThingSettings(settings);
                MigrateAwayFromMount(settings);
                _settingsLastRunMigrationVersion.Value = 1;
            }
            if (_settingsLastRunMigrationVersion.Value < 2)
            {
                MigrateToInfoPanelLocation(settings);
                _settingsLastRunMigrationVersion.Value = 2;
            }


            foreach (var t in _things)
            {
                t.KeybindingSetting.Value.BindingChanged += UpdateSettings;
                t.ImageFileNameSetting.SettingChanged += UpdateSettings;
            }
            _settingDisplayModuleOnLoadingScreen.SettingChanged += UpdateSettings;
            _settingMountAutomaticallyAfterLoadingScreen.SettingChanged += UpdateSettings;
            _settingDisplayMountQueueing.SettingChanged += UpdateSettings;
            _settingDisplayLaterActivation.SettingChanged += UpdateSettings;
            _settingEnableMountQueueing.SettingChanged += UpdateSettings;
            _settingDragInfoPanel.SettingChanged += UpdateSettings;
            _settingInfoPanelLocation.SettingChanged += UpdateSettings;
            _settingMountRadialSpawnAtMouse.SettingChanged += UpdateSettings;
            _settingMountRadialIconSizeModifier.SettingChanged += UpdateSettings;
            _settingMountRadialRadiusModifier.SettingChanged += UpdateSettings;
            _settingMountRadialStartAngle.SettingChanged += UpdateSettings;
            _settingMountRadialToggleActionCameraKeyBinding.Value.BindingChanged += UpdateSettings;
            _settingMountRadialIconOpacity.SettingChanged += UpdateSettings;
            _settingDrawIconIds.SettingChanged += UpdateSettings;
            _settingUserDefinedRadialIds.SettingChanged += UpdateSettings;
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
            _debug.Add("GetTriggeredRadialSettings", () => {
                var triggered = _helper.GetTriggeredRadialSettings();
                return triggered == null ? "" : $"Name: {triggered.Name}, Number of things: {triggered.AvailableThings.Count}";
                });
            ContextualRadialSettings.ForEach(c => _debug.Add($"Contextual RadialSettings {c.Order} {c.Name} A", () => $"IsApplicable: {c.IsApplicable()}, ApplyInstantlyIfSingle: {c.ApplyInstantlyIfSingle.Value}, ApplyInstantlyOnTap: {c.ApplyInstantlyOnTap.Value}"));
            ContextualRadialSettings.ForEach(c => _debug.Add($"Contextual RadialSettings {c.Order} {c.Name} B", () => $"Center: {c.GetCenterThing()?.Name}, CenterBehavior: {c.CenterThingBehavior.Value}, LastUsed: {c.GetLastUsedThing()?.Name}"));
            _debug.Add("Applicable Contextual RadialSettings", () => $"Name: {_helper.GetApplicableContextualRadialThingSettings()?.Name}, Things count: {_helper.GetApplicableContextualRadialThingSettings()?.AvailableThings.Count}");
            _debug.Add("Queued for out of combat", () => $"{_helper.GetQueuedThing()?.Name}");
            _debug.Add("TappedModuleKeybind", () => $"{DateTime.Now} {tappedModuleKeybind} {lastTriggered} {(lastTriggered != null ? (int)(DateTime.Now-lastTriggered.Value).TotalMilliseconds : "")}");

            if (Gw2ApiManager.HasSubtoken)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                _helper.IsCombatLaunchUnlockedAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
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

            if(Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                _helper.DoRangedThing();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }

            var currentTriggeringState = _settingDefaultMountBinding.Value.IsTriggering;
            double howLongIsModuleKeybindHeldHown = 0;
            if (lastTriggered.HasValue)
            {
                howLongIsModuleKeybindHeldHown = (DateTime.Now - lastTriggered.Value).TotalMilliseconds;
            }
            var isWithinThreshold = howLongIsModuleKeybindHeldHown <= _settingTapThresholdInMilliseconds.Value;
            if ((previousTriggeringState && !currentTriggeringState && isWithinThreshold) || howLongIsModuleKeybindHeldHown > _settingTapThresholdInMilliseconds.Value)
            {
                if (lastTriggered.HasValue)
                {
                    tappedModuleKeybind = isWithinThreshold ? TappedModuleKeybindState.Tap : TappedModuleKeybindState.Hold;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    DoKeybindActionAsync(KeybindTriggerType.Module);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    lastTriggered = null;
                }
            }
            previousTriggeringState = currentTriggeringState;



            var isThingSwitchable = CanThingBeActivated();
            var moduleHidden = _lastIsThingSwitchable && !isThingSwitchable;
            var moduleShown = !_lastIsThingSwitchable && isThingSwitchable;
            var inUseThingsCount = _things.Count(m => m.IsInUse());

            if (inUseThingsCount == 0 && _lastInUseThingsCount > 0 && moduleHidden == false && moduleShown == false)
            {
                _helper.ClearSomethingStoredForLaterActivation();
            }

            if (moduleHidden && inUseThingsCount == 1 && _settingMountAutomaticallyAfterLoadingScreen.Value && GameService.GameIntegration.Gw2Instance.Gw2HasFocus)
            {
                _helper.StoreThingForLaterActivation(_things.Single(m => m.IsInUse()), "ModuleHidden");
            }
            if (moduleShown && inUseThingsCount == 0 && _helper.IsSomethingStoredForLaterActivation() != null && GameService.GameIntegration.Gw2Instance.Gw2HasFocus)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                _helper.DoThingActionForLaterActivation();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }

            _lastInUseThingsCount = inUseThingsCount;
            _lastIsThingSwitchable = isThingSwitchable;

            bool shouldShowModule = ShouldShowModule();
            if (shouldShowModule)
            {
                foreach (var drawIcons in _drawIcons) drawIcons.Show();
                _drawInfoPanel?.Show();
            }
            else
            {
                foreach (var drawIcons in _drawIcons) drawIcons.Hide();
                _drawInfoPanel?.Hide();
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

            if (_radial.Visible && !(_settingDefaultMountBinding.Value.IsTriggering || UserDefinedRadialSettings.Any(s => s.Keybind.Value.IsTriggering) ) || !shouldShowModule)
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
            return GameService.GameIntegration.Gw2Instance.IsInGame;
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            _skyLakes?.Clear();
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
            _settingDisplayLaterActivation.SettingChanged -= UpdateSettings;
            _settingEnableMountQueueing.SettingChanged -= UpdateSettings;
            _settingDragInfoPanel.SettingChanged -= UpdateSettings;
            _settingInfoPanelLocation.SettingChanged -= UpdateSettings;
            _settingMountRadialSpawnAtMouse.SettingChanged -= UpdateSettings;
            _settingMountRadialIconSizeModifier.SettingChanged -= UpdateSettings;
            _settingMountRadialRadiusModifier.SettingChanged -= UpdateSettings;
            _settingMountRadialStartAngle.SettingChanged -= UpdateSettings;
            _settingMountRadialToggleActionCameraKeyBinding.SettingChanged -= UpdateSettings;
            _settingMountRadialIconOpacity.SettingChanged -= UpdateSettings;

            _settingDisplayModuleOnLoadingScreen.SettingChanged -= UpdateSettings;
            _settingMountAutomaticallyAfterLoadingScreen.SettingChanged -= UpdateSettings;
            _settingDrawIconIds.SettingChanged -= UpdateSettings;
            _settingUserDefinedRadialIds.SettingChanged -= UpdateSettings;
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

            _drawInfoPanel?.Dispose();
            _drawInfoPanel = new InfoPanel(_textureCache, _helper);

            _drawMouseCursor?.Dispose();
            _drawMouseCursor = new DrawMouseCursor(_textureCache);
            _drawMouseCursor.Parent = GameService.Graphics.SpriteScreen;
            _drawMouseCursor.Hide();

            _radial?.Dispose();
            _radial = new DrawRadial(_helper, _textureCache);
            _radial.Parent = GameService.Graphics.SpriteScreen;
            _radial.OnSettingsButtonClicked += (args, sender) =>
            {
                _settingsWindow.SelectedTab = _settingsWindow.Tabs.Skip(1).First();
                _settingsWindow.Show();
            };
        }

        private async Task DoKeybindActionAsync(KeybindTriggerType caller)
        {
            Logger.Debug($"{nameof(DoKeybindActionAsync)} entered");

            if(caller == KeybindTriggerType.UserDefined)
            {
                ShowRadial(caller);
                return;
            }

            var selectedRadialSettings = _helper.GetApplicableContextualRadialThingSettings();
            Logger.Debug($"{nameof(DoKeybindActionAsync)} radial applicable settings: {selectedRadialSettings.Name}");
            var things = selectedRadialSettings.AvailableThings;
            if (lastTriggered == null && selectedRadialSettings.IsTapApplicable())
            {
                Logger.Debug($"{nameof(DoKeybindActionAsync)} radial tap is applicable.");
                lastTriggered = DateTime.Now;
                return;
            }

            Thing tappedThing = things.FirstOrDefault(t => t.Name == selectedRadialSettings.ApplyInstantlyOnTap.Value);
            if (selectedRadialSettings.IsTapApplicable())
            {
                if (tappedModuleKeybind == TappedModuleKeybindState.Tap)
                {
                    await tappedThing?.DoAction(selectedRadialSettings.UnconditionallyDoAction.Value, false);
                    Logger.Debug($"{nameof(DoKeybindActionAsync)} not showing radial selected thing (tappedModuleKeybind): {tappedThing?.Name}");
                    tappedModuleKeybind = TappedModuleKeybindState.Unknown;
                    return;
                }
                else
                {
                    if (things.Count() > 1 && tappedThing != null)
                    {
                        things.Remove(tappedThing);
                    }
                    tappedModuleKeybind = TappedModuleKeybindState.Unknown;
                }
            }


            if (things.Count() == 1 && selectedRadialSettings.ApplyInstantlyIfSingle.Value)
            {
                await things.FirstOrDefault()?.DoAction(selectedRadialSettings.UnconditionallyDoAction.Value, false);
                Logger.Debug($"{nameof(DoKeybindActionAsync)} not showing radial selected thing (ApplyInstantlyIfSingle): {things.First().Name}");
                return;
            }

            var defaultThing = selectedRadialSettings.GetDefaultThing();
            if (defaultThing != null && GameService.Input.Mouse.CameraDragging)
            {
                await (defaultThing?.DoAction(false, false) ?? Task.CompletedTask);
                Logger.Debug($"{nameof(DoKeybindActionAsync)} CameraDragging default");
                return;
            }

            switch (_settingKeybindBehaviour.Value)
            {
                case "Default":
                    await (defaultThing?.DoAction(false, false) ?? Task.CompletedTask);
                    Logger.Debug($"{nameof(DoKeybindActionAsync)} KeybindBehaviour default");
                    break;
                case "Radial":
                    ShowRadial(caller);
                    break;
            }
            return;
        }

        private void ShowRadial(KeybindTriggerType caller)
        {

            if (ShouldShowModule())
            {
                _radial?.Show();
                Logger.Debug($"{nameof(DoKeybindActionAsync)} KeybindBehaviour radial, caller {caller}");
            }
        }
        
        private async Task HandleCombatChangeAsync(object sender, ValueEventArgs<bool> e)
        {
            if (!e.Value)
            {
                Logger.Debug($"{nameof(HandleCombatChangeAsync)} Trying queued for out of combat");
                if (!_helper.IsPlayerMounted())
                {
                    var thingInCombat = _helper.GetQueuedThing();
                    Logger.Debug($"{nameof(HandleCombatChangeAsync)} Applied queued for out of combat: {thingInCombat?.Name}");
                    await (thingInCombat?.DoAction(false, false) ?? Task.CompletedTask);
                }
                else
                {
                    Logger.Debug($"{nameof(HandleCombatChangeAsync)} Not applying queued for out of combat: player mounted");
                }
                foreach (var thing in _things)
                {
                    thing.QueuedTimestamp = null;
                }
            }
        }
    }
}
