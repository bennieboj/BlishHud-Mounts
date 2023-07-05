using Blish_HUD.Settings;
using Gw2Sharp.Models;

namespace Manlaan.Mounts.Things.Mounts
{
    public class Springer : Mount
    {
        public Springer(SettingCollection settingCollection, Helper helper) :
            base(settingCollection, helper, "Springer", "Springer", "springer", 2)
        {
        }

        protected override MountType MountType => MountType.Springer;
    }
}
