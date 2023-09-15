﻿using Blish_HUD.Settings;
using Gw2Sharp.Models;

namespace Manlaan.Mounts.Things.Mounts
{
    public class Skiff : Mount
    {
        public Skiff(SettingCollection settingCollection, Helper helper) 
            : base(settingCollection, helper, "Skiff", "Skiff", "skiff", 11)
        {
        }
        protected override MountType MountType => MountType.Skiff;

        public override bool IsInstactActionApplicable() => _helper.IsPlayerOnWaterSurface();

        public override bool IsUsableOnMount()
        {
            return true;
        }
    }
}
