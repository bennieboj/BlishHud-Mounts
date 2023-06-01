using Blish_HUD.Settings;
using Gw2Sharp.Models;

namespace Manlaan.Mounts.Things.Mounts
{
    public class Skyscale : Mount
    {
        public Skyscale(SettingCollection settingCollection, Helper helper) : base(settingCollection, helper, "Skyscale", "Skyscale", "skyscale", false, true, false, 8)
        {
        }

        protected override MountType MountType => MountType.Skyscale;
    }
}
