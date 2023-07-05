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

        MapType[] warclawOnlyMaps = {
                MapType.RedBorderlands,
                MapType.BlueBorderlands,
                MapType.GreenBorderlands,
                MapType.EternalBattlegrounds,
                MapType.Center,
                MapType.WvwLounge
            };

        public override bool IsInstactActionApplicable()
        {
            return Array.Exists(warclawOnlyMaps, mapType => mapType == GameService.Gw2Mumble.CurrentMap.Type);
        }
    }
}