using Blish_HUD.Settings;
using System.Threading.Tasks;

namespace Manlaan.Mounts.Things.Mounts
{
    public class UnMount : Thing
    {
        public UnMount(SettingCollection settingCollection, Helper helper) 
            : base(settingCollection, helper, "Unmount", "Unmount", "unmount")
        {
        }

        public override bool IsInUse()
        {
            return false;
        }

        public override bool IsUsableInCombat()
        {
            return true;
        }
    }
}
