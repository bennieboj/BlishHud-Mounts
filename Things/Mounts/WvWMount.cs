using Blish_HUD.Settings;

namespace Manlaan.Mounts.Things.Mounts
{
    public abstract class WvWMount : Mount
    {
        public WvWMount(SettingCollection settingCollection, Helper helper,
            string name, string displayName, string imageFileName, int defaultOrderSetting)
            : base(settingCollection, helper, name, displayName, imageFileName, defaultOrderSetting)
        {            
        }
    }
}