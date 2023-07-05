using Blish_HUD.Settings;
using Gw2Sharp.Models;

namespace Manlaan.Mounts.Things.Mounts
{
    public class SiegeTurtle : WaterMount
    {
        public SiegeTurtle(SettingCollection settingCollection, Helper helper) : 
            base(settingCollection, helper, "Turtle", "Siege Turtle", "turtle", 9)
        {
        }

        protected override MountType MountType => MountType.SiegeTurtle;
    }
}
