using Blish_HUD;
using Blish_HUD.Settings;
using Gw2Sharp.Models;
using System;

namespace Manlaan.Mounts.Things.Mounts
{
    public abstract class WvWMount : Mount
    {
        public WvWMount(SettingCollection settingCollection, Helper helper,
            string name, string displayName, string imageFileName, int defaultOrderSetting)
            : base(settingCollection, helper, name, displayName, imageFileName, defaultOrderSetting)
        {            
        }



        public override bool IsInstactActionApplicable()
        {
            return _helper.IsPlayerInWvwMap();
        }

        public override int IsInstactActionApplicableOrder()
        {
            return 1;
        }
    }
}