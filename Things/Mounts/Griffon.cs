using Blish_HUD.Settings;
using Gw2Sharp.Models;

namespace Manlaan.Mounts.Things.Mounts
{
    public class Griffon : Mount
    {
        public Griffon(SettingCollection settingCollection, Helper helper) :
            base(settingCollection, helper, "Griffon", "Griffon", "griffon", 5)
        {
        }

        protected override MountType MountType => MountType.Griffon;
    }
}
