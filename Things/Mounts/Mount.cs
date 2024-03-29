﻿using Blish_HUD;
using Blish_HUD.Settings;
using Gw2Sharp.Models;

namespace Manlaan.Mounts.Things.Mounts
{
    public abstract class Mount : Thing
    {
        public Mount(SettingCollection settingCollection, Helper helper,
            string name, string displayName, string imageFileName) 
            : base(settingCollection, helper, name, displayName, imageFileName)
        {
        }


        protected abstract MountType MountType { get; }

        public override bool IsInUse()
        {
            return GameService.Gw2Mumble.PlayerCharacter.CurrentMount == MountType;
        }
    }
}
