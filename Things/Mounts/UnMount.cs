using Blish_HUD.Settings;

namespace Manlaan.Mounts.Things.Mounts
{
    public class UnMount : Thing
    {
        public UnMount(SettingCollection settingCollection, Helper helper) 
            : base(settingCollection, helper, "Unmount", "Unmount", "unmount")
        {
        }

        public override bool IsUsableInCombat()
        {
            return true;
        }
    }
}
