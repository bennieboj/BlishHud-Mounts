using Blish_HUD.Controls;
using System;

namespace Manlaan.Mounts.Controls
{
    public class DrawCornerIcons : Container, IDisposable
    {
        private bool _disposed = false;

        public DrawCornerIcons(TextureCache textureCache)
        {
            foreach (var thing in Module._availableOrderedThings)
            {
                thing.CreateCornerIcon(textureCache.GetMountImgFile(thing));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // dispose managed state (managed objects).
                foreach (var thing in Module._availableOrderedThings)
                {
                    thing.DisposeCornerIcon();
                }

            }

            // free unmanaged resources (unmanaged objects) and override a finalizer below.
            // set large fields to null.

            _disposed = true;
        }
    }
}
