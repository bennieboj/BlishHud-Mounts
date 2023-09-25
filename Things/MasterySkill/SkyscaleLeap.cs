using Blish_HUD.Settings;
using System.Threading.Tasks;

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
