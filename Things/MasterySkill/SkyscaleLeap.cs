using Blish_HUD.Settings;

namespace Manlaan.Mounts.Things.Mounts
{
    public class SkyscaleLeap : MasterySkill
    {
        public SkyscaleLeap(SettingCollection settingCollection, Helper helper) 
            : base(settingCollection, helper, "Skyscale Leap", "Skyscale Leap", "skyscaleleap")
        {
        }

        public override bool IsUsableInCombat()
        {
            return _helper.IsCombatLaunchUnlocked();
        }
    }
}
