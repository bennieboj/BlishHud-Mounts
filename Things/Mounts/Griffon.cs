using Blish_HUD.Settings;
using Gw2Sharp.Models;

namespace Manlaan.Mounts.Things.Mounts
{
    public class Griffon : Mount
    {
        public Griffon(SettingCollection settingCollection, Helper helper) :
            base(settingCollection, helper, "Griffon", "Griffon", "griffon")
        {
        }

        protected override MountType MountType => MountType.Griffon;

        public override bool IsUsableInAir()
        {
            return true;
        }
    }
}
