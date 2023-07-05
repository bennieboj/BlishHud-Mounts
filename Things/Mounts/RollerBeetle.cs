using Blish_HUD.Settings;
using Gw2Sharp.Models;

namespace Manlaan.Mounts.Things.Mounts
{
    public class RollerBeetle : Mount
    {
        public RollerBeetle(SettingCollection settingCollection, Helper helper) :
            base(settingCollection, helper, "Roller", "Roller Beetle", "roller", 6)
        {
        }

        protected override MountType MountType => MountType.RollerBeetle;
    }
}
