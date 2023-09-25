using Blish_HUD.Settings;

namespace Manlaan.Mounts.Things.Mounts
{
    public abstract class MasterySkill : Thing
    {
        public MasterySkill(SettingCollection settingCollection, Helper helper,
            string name, string displayName, string imageFileName) 
            : base(settingCollection, helper, name, displayName, imageFileName)
        {
        }

        public override bool IsInUse()
        {
            return false;
        }

        public override bool IsUsableOnMount()
        {
            return true;
        }
    }
}
