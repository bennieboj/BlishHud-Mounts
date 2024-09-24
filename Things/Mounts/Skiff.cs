using Blish_HUD.Settings;
using Gw2Sharp.Models;

namespace Manlaan.Mounts.Things.Mounts
{
    public class Skiff : Mount
    {
        public Skiff(SettingCollection settingCollection, Helper helper) 
            : base(settingCollection, helper, "Skiff", "Skiff", "skiff")
        {
        }
        protected override MountType MountType => MountType.Skiff;

        public override bool IsGroundTargeted()
        {
            return true;
        }
        public override bool ShouldGroundTargetingBeDelayed()
        {
            return !_helper.IsPlayerOnWaterSurface();
        }
    }
}
