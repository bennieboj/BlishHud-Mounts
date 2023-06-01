using Blish_HUD.Settings;
using Gw2Sharp.Models;

namespace Manlaan.Mounts.Things.Mounts
{
    public class Skimmer : Mount
    {
        public Skimmer(SettingCollection settingCollection, Helper helper) : base(settingCollection, helper, "Skimmer", "Skimmer", "skimmer", true, false, false, 3)
        {
        }

        protected override MountType MountType => MountType.Skimmer;
    }
}
