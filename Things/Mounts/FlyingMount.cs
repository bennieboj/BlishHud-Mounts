using Blish_HUD.Settings;
using Gw2Sharp.Models;

namespace Manlaan.Mounts.Things.Mounts
{
    public abstract class FlyingMount : Mount
    {
        public FlyingMount(SettingCollection settingCollection, Helper helper,
            string name, string displayName, string imageFileName, int defaultOrderSetting)
            : base(settingCollection, helper, name, displayName, imageFileName, defaultOrderSetting)
        {            
        }

        public override bool IsInstactActionApplicable()
        {
            return Module.IsPlayerGlidingOrFalling && Name == Module._settingDefaultFlyingMountChoice.Value;
        }
    }
}