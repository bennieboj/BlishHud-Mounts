using Blish_HUD.Settings;
using Gw2Sharp.Models;

namespace Manlaan.Mounts.Things.Mounts
{
    public class SiegeTurtle : Mount
    {
        public SiegeTurtle(SettingCollection settingCollection, Helper helper) : base(settingCollection, helper, "Turtle", "Siege Turtle", "turtle", true, false, false, 9)
        {
        }

        protected override MountType MountType => MountType.SiegeTurtle;
    }
}
