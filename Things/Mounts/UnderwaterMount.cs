using Blish_HUD;
using Blish_HUD.Settings;

namespace Manlaan.Mounts.Things.Mounts
{
    public abstract class UnderwaterMount : Mount
    {
        public UnderwaterMount(SettingCollection settingCollection, Helper helper,
            string name, string displayName, string imageFileName, int defaultOrderSetting)
            : base(settingCollection, helper, name, displayName, imageFileName, defaultOrderSetting)
        {            
        }

        public override bool IsInstactActionApplicable()
        {
            return _helper.IsPlayerUnderWater() && Name == Module._settingDefaultWaterMountChoice.Value;
        }
    }
}