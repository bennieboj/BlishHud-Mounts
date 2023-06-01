using Blish_HUD.Settings;
using Gw2Sharp.Models;

namespace Manlaan.Mounts.Things.Mounts
{
    public class Raptor : Mount
    {
        public Raptor(SettingCollection settingCollection, Helper helper) : base(settingCollection, helper, "Raptor", "Raptor", "raptor", false, false, false, 1)
        {
        }

        protected override MountType MountType => MountType.Raptor;
    }
}
