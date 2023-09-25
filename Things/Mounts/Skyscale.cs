using Blish_HUD.Settings;
using Gw2Sharp.Models;
using System.Threading.Tasks;

namespace Manlaan.Mounts.Things.Mounts
{
    public class Skyscale : Mount
    {
        public Skyscale(SettingCollection settingCollection, Helper helper) :
            base(settingCollection, helper, "Skyscale", "Skyscale", "skyscale")
        {
        }

        protected override MountType MountType => MountType.Skyscale;

        public override bool IsUsableInCombat()
        {
            return _helper.IsCombatLaunchUnlocked();
        }
    }
}
