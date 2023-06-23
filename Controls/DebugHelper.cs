using Blish_HUD;

namespace Manlaan.Mounts.Controls
{
    public static class DebugHelper { 
        public static bool IsDebugEnabled()
        {
            var isDebug = false;
#if DEBUG
            isDebug = true;
#endif
            return isDebug || GameService.Debug.EnableAdditionalDebugDisplay.Value;
        }
    }
}