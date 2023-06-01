using Blish_HUD.Settings;
using Gw2Sharp.Models;

namespace Manlaan.Mounts.Things.Mounts
{
    public class Warclaw : Mount
    {
        public Warclaw(SettingCollection settingCollection, Helper helper) : base(settingCollection, helper, "Warclaw", "Warclaw", "warclaw", false, false, true, 7)
        {
        }

        protected override MountType MountType => MountType.Warclaw;
    }
}
