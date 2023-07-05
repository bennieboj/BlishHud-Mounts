using Blish_HUD.Settings;

namespace Manlaan.Mounts.Things.Mounts
{
    public class Fishing : Thing
    {
        public Fishing(SettingCollection settingCollection, Helper helper) 
            : base(settingCollection, helper, "Fishing", "Fishing", "fishing", 10)
        {
        }

        public override bool IsInUse()
        {
            return false; //TODO based on LastUsedTimestamp maybe put in base and only override in Mount/Skiff?? NOT POSSIBLE, 
        }

        public override bool IsInstactActionApplicable()
        {
            return false;
        }
    }
}
