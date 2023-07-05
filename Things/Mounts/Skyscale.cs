using Blish_HUD.Settings;
using Gw2Sharp.Models;

namespace Manlaan.Mounts.Things.Mounts
{
    public class Skyscale : FlyingMount
    {
        public Skyscale(SettingCollection settingCollection, Helper helper) :
            base(settingCollection, helper, "Skyscale", "Skyscale", "skyscale", 8)
        {
        }

        protected override MountType MountType => MountType.Skyscale;
    }
}
