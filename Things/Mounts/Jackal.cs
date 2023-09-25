using Blish_HUD.Settings;
using Gw2Sharp.Models;

namespace Manlaan.Mounts.Things.Mounts
{
    public class Jackal : Mount
    {
        public Jackal(SettingCollection settingCollection, Helper helper) : 
            base(settingCollection, helper, "Jackal", "Jackal", "jackal")
        {
        }

        protected override MountType MountType => MountType.Jackal;
    }
}
