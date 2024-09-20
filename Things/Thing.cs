using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mounts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Manlaan.Mounts.Things
{
    public abstract class Thing : IEquatable<Thing>
    {
        public Thing(SettingCollection settingCollection, Helper helper, string name, string displayName, string imageFileName)
        {
            _helper = helper;
            Name = name;
            DisplayName = displayName;
            ImageFileName = imageFileName;
            KeybindingSetting = settingCollection.DefineSetting($"Mount{name}Binding", new KeyBinding(Keys.None), () => $"{displayName} Binding", () => "");
            ImageFileNameSetting = settingCollection.DefineSetting($"Mount{name}ImageFileName", $"{imageFileName}.png", () => $"{displayName} Image File Name", () => "");
        }

        protected static readonly Logger Logger = Logger.GetLogger<Thing>();
        protected readonly Helper _helper;

        public string Name { get; private set; }
        public string DisplayName { get; private set; }
        public string ImageFileName { get; private set; }
        public SettingEntry<int> OrderSetting { get; private set; }
        public SettingEntry<KeyBinding> KeybindingSetting { get; private set; }
        public SettingEntry<string> ImageFileNameSetting { get; private set; }
        public CornerIcon CornerIcon { get; private set; }

        private DateTime? _queuedTimestamp;
        public DateTime? QueuedTimestamp {
            get { return _queuedTimestamp;  }
            internal set
            {
                var oldvalue = _queuedTimestamp;
                Logger.Debug($"Setting {nameof(QueuedTimestamp)} on {Name} to: {value}");
                _queuedTimestamp = value;
                if (QueuedTimestampUpdated != null && oldvalue != value)
                {
                    QueuedTimestampUpdated(this, new ValueChangedEventArgs("", ""));
                }

            }
        }
        public event EventHandler<ValueChangedEventArgs> QueuedTimestampUpdated;

        public DateTime? LastUsedTimestamp { get; internal set; }
        public bool IsKeybindSet => KeybindingSetting.Value.ModifierKeys != ModifierKeys.None || KeybindingSetting.Value.PrimaryKey != Keys.None;
        public bool IsAvailable => IsKeybindSet;

        public void CreateCornerIcon(Texture2D img)
        {
            CornerIcon?.Dispose();
            CornerIcon = new CornerIcon()
            {
                IconName = DisplayName,
                Icon = img,
                HoverIcon = img,
                Priority = 10
            };
            CornerIcon.Click += async delegate { await DoAction(false, true); };
        }

        public void DisposeCornerIcon()
        {
            CornerIcon?.Dispose();
        }

        public async Task DoAction(bool unconditionallyDoAction, bool isActionComingFromMouseActionOnModuleUI)
        {
            if (unconditionallyDoAction)
            {
                await _helper.TriggerKeybind(KeybindingSetting, WhichKeybindToRun.Both);
                return;
            }

            if (GameService.Gw2Mumble.PlayerCharacter.IsInCombat && Module._settingEnableMountQueueing.Value && !IsUsableInCombat())
            {
                Logger.Debug($"{nameof(DoAction)} Set queued for out of combat: {Name}");
                QueuedTimestamp = DateTime.UtcNow;
                return;
            }

            if (!Module.CanThingBeActivated())
            {
                _helper.StoreThingForLaterActivation(this, "NotAbleToActivate");
                return;
            }

            if (isActionComingFromMouseActionOnModuleUI && IsGroundTargeted())
            {
                switch (Module._settingGroundTargeting.Value)
                {
                    case GroundTargeting.Instant:
                        _helper.StoredRangedThing = this;
                        return;
                    case GroundTargeting.Normal:
                        break;
                    case GroundTargeting.FastWithRangeIndicator:
                        _helper.StoredRangedThing = this;
                        break;
                    default:
                        break;
                }
            }

            var whichKeybindToRun = WhichKeybindToRun.Both;
            LastUsedTimestamp = DateTime.UtcNow;
            switch (Module._settingGroundTargeting.Value)
            {
                case GroundTargeting.Instant:
                    _helper.StoredRangedThing = null; //reset
                    break;
                case GroundTargeting.Normal:
                    break;
                case GroundTargeting.FastWithRangeIndicator:
                    if (IsGroundTargeted())
                    {
                        whichKeybindToRun = isActionComingFromMouseActionOnModuleUI ? WhichKeybindToRun.Press : WhichKeybindToRun.Release;
                    }
                    break;
                default:
                    break;
            }
            await _helper.TriggerKeybind(KeybindingSetting, whichKeybindToRun);
        }

        public virtual bool IsInUse()
        {
            return false;
        }

        public virtual bool IsUsableInCombat()
        {
            return false;
        }

        public virtual bool IsGroundTargeted()
        {
            return false;
        }

        public bool Equals(Thing other)
        {
            return !(other is null) &&
                   Name == other.Name;
        }

        public override int GetHashCode()
        {
            int hashCode = 657878212;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode;
        }
    }

    public enum WhichKeybindToRun
    {
        Both = 0,
        Press = 1,
        Release = 2,
    }
}
