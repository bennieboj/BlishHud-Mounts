using Blish_HUD.Settings;

namespace Manlaan.Mounts.Things.Mounts
{
    public class JadeBotWaypoint : Thing
    {
        public JadeBotWaypoint(SettingCollection settingCollection, Helper helper) 
            : base(settingCollection, helper, "Jade Bot Waypoint", "Jade Bot Waypoint", "jadebotwaypoint", 12)
        {
        }

        public override bool IsInUse()
        {
            return false;
        }

        public override bool IsInstactActionApplicable()
        {
            return false;
        }
    }
}
