using Blish_HUD.Settings;

namespace Manlaan.Mounts.Things.Mounts
{
    public abstract class Novelty : Thing
    {
        public Novelty(SettingCollection settingCollection, Helper helper,
            string name, string displayName, string imageFileName) 
            : base(settingCollection, helper, name, displayName, imageFileName)
        {
        }
    }
}
