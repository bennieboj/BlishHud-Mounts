using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Controls.Extern;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Manlaan.Mounts
{
    public abstract class Mount
    {
        public Mount(SettingCollection settingCollection, 
            string name, string displayName, string imageFileName,
            bool isUnderwaterMount, bool isWvWMount, int defaultOrderSettig)
        {
            Name = name;
            DisplayName = displayName;
            ImageFileName = imageFileName;
            IsWaterMount = isUnderwaterMount;
            IsWvWMount = isWvWMount;
            OrderSetting = settingCollection.DefineSetting($"Mount{name}Order2", defaultOrderSettig, $"{displayName} Order", "");
            KeybindingSetting = settingCollection.DefineSetting($"Mount{name}Binding", new KeyBinding(Keys.None), $"{displayName} Binding", "");
        }

        public string Name { get; private set; }
        public string DisplayName { get; private set; }
        public string ImageFileName { get; private set; }
        public bool IsDisabled { get; private set; }
        public DateTime? QueuedTimestamp { get; internal set; }
        public DateTime? LastUsedTimestamp { get; internal set; }
        public bool IsWaterMount { get; private set; }
        public bool IsWvWMount { get; private set; }


        public SettingEntry<int> OrderSetting { get; private set; }
        public SettingEntry<KeyBinding> KeybindingSetting { get; private set; }
        public CornerIcon CornerIcon { get; private set; }

        public void DoHotKey()
        {
            if (GameService.Gw2Mumble.PlayerCharacter.IsInCombat)
            {
                QueuedTimestamp = DateTime.UtcNow;
                return;
            }

            LastUsedTimestamp = DateTime.UtcNow;

            if (KeybindingSetting.Value.ModifierKeys != ModifierKeys.None)
            {
                if (KeybindingSetting.Value.ModifierKeys.HasFlag(ModifierKeys.Alt))
                    Blish_HUD.Controls.Intern.Keyboard.Press(VirtualKeyShort.MENU, true);
                if (KeybindingSetting.Value.ModifierKeys.HasFlag(ModifierKeys.Ctrl))
                    Blish_HUD.Controls.Intern.Keyboard.Press(VirtualKeyShort.CONTROL, true);
                if (KeybindingSetting.Value.ModifierKeys.HasFlag(ModifierKeys.Shift))
                    Blish_HUD.Controls.Intern.Keyboard.Press(VirtualKeyShort.SHIFT, true);
            }
            Blish_HUD.Controls.Intern.Keyboard.Press(ToVirtualKey(KeybindingSetting.Value.PrimaryKey), true);
            System.Threading.Thread.Sleep(50);
            Blish_HUD.Controls.Intern.Keyboard.Release(ToVirtualKey(KeybindingSetting.Value.PrimaryKey), true);
            if (KeybindingSetting.Value.ModifierKeys != ModifierKeys.None)
            {
                if (KeybindingSetting.Value.ModifierKeys.HasFlag(ModifierKeys.Shift))
                    Blish_HUD.Controls.Intern.Keyboard.Release(VirtualKeyShort.SHIFT, true);
                if (KeybindingSetting.Value.ModifierKeys.HasFlag(ModifierKeys.Ctrl))
                    Blish_HUD.Controls.Intern.Keyboard.Release(VirtualKeyShort.CONTROL, true);
                if (KeybindingSetting.Value.ModifierKeys.HasFlag(ModifierKeys.Alt))
                    Blish_HUD.Controls.Intern.Keyboard.Release(VirtualKeyShort.MENU, true);
            }
        }

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
            CornerIcon.Click += delegate { DoHotKey(); };
        }

        public void DisposeCornerIcon()
        {
            CornerIcon?.Dispose();
        }


        private VirtualKeyShort ToVirtualKey(Keys key)
        {
            try
            {
                return (VirtualKeyShort)key;
            }
            catch
            {
                return new VirtualKeyShort();
            }
        }
    }

    public class Raptor : Mount
    {
        public Raptor(SettingCollection settingCollection) : base(settingCollection, "Raptor", "Raptor", "raptor", false, false, 1)
        {
        }
    }

    public class Springer : Mount
    {
        public Springer(SettingCollection settingCollection) : base(settingCollection, "Springer", "Springer", "springer", false, false, 2)
        {
        }
    }

    public class Skimmer : Mount
    {
        public Skimmer(SettingCollection settingCollection) : base(settingCollection, "Skimmer", "Skimmer", "skimmer", true, false, 3)
        {
        }
    }

    public class Jackal : Mount
    {
        public Jackal(SettingCollection settingCollection) : base(settingCollection, "Jackal", "Jackal", "jackal", false, false, 4)
        {
        }
    }

    public class Griffon : Mount
    {
        public Griffon(SettingCollection settingCollection) : base(settingCollection, "Griffon", "Griffon", "griffon", false, false, 4)
        {
        }
    }

    public class RollerBeetle : Mount
    {
        public RollerBeetle(SettingCollection settingCollection) : base(settingCollection, "Roller", "Roller Beetle", "roller", false, false, 5)
        {
        }
    }

    public class Warclaw : Mount
    {
        public Warclaw(SettingCollection settingCollection) : base(settingCollection, "Warclaw", "Warclaw", "warclaw", false, true, 6)
        {
        }
    }

    public class Skyscale : Mount
    {
        public Skyscale(SettingCollection settingCollection) : base(settingCollection, "Skyscale", "Skyscale", "skyscale", false, false, 7)
        {
        }
    }

    public class SiegeTurtle : Mount
    {
        public SiegeTurtle(SettingCollection settingCollection) : base(settingCollection, "Turtle", "Siege Turtle", "turtle", true, false, 8)
        {
        }
    }
}
