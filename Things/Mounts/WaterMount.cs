using Blish_HUD;
using Blish_HUD.Settings;

namespace Manlaan.Mounts.Things.Mounts
{
    public abstract class WaterMount : Mount
    {
        public WaterMount(SettingCollection settingCollection, Helper helper,
            string name, string displayName, string imageFileName, int defaultOrderSetting)
            : base(settingCollection, helper, name, displayName, imageFileName, defaultOrderSetting)
        {            
        }

        public override bool IsInstactActionApplicable()
        {
            return GameService.Gw2Mumble.PlayerCharacter.Position.Z <= 0 && Name == Module._settingDefaultWaterMountChoice.Value;
        }
    }
}