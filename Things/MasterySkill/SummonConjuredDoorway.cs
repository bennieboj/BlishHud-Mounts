﻿using Blish_HUD.Settings;

namespace Manlaan.Mounts.Things.Mounts
{
    public class SummonConjuredDoorway : MasterySkill
    {
        public SummonConjuredDoorway(SettingCollection settingCollection, Helper helper) 
            : base(settingCollection, helper, "Summon Conjured Doorway", "Summon Conjured Doorway", "summonconjureddoorway")
        {
        }

        public override bool IsGroundTargeted()
        {
            return true;
        }

        public override bool ShouldGroundTargetingBeDelayed()
        {
            return true;
        }
    }
}
