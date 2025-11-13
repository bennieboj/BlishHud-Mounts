using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kenedia.Modules.Core.Extensions
{
    public static class ControlExtensions
    {
        public static bool SetTexture(this Image image, Texture2D texture2D)
        {
            if (image is not null)
            {
                image.Texture = texture2D;
                return true;
            }

            return false;
        }

        public static bool IsParentSetAndVisible(this Control ctrl)
        {
            return ctrl?.Parent?.Visible is true
                && (ctrl.Parent == GameService.Graphics.SpriteScreen || IsParentSetAndVisible(ctrl.Parent));
        }

        public static bool IsVisible(this Control ctrl)
        {
            return ctrl?.Visible is true && IsParentSetAndVisible(ctrl);
        }

        public static bool IsDrawn(this Control c)
        {
            return c.Parent is not null && c.Parent.Visible is true && c.Parent.AbsoluteBounds.Contains(c.AbsoluteBounds.Center) is true && (c.Parent == GameService.Graphics.SpriteScreen || IsDrawn(c.Parent));
        }

        public static bool IsDrawn(this Control c, Rectangle b)
        {
            return c.Parent is not null && c.Parent.Visible is true && c.Parent.AbsoluteBounds.Contains(b.Center) is true && (c.Parent == GameService.Graphics.SpriteScreen || IsDrawn(c.Parent, b));
        }

        public static bool ToggleVisibility(this Control c, bool? visible = null)
        {
            c.Visible = visible ?? !c.Visible;
            return c.Visible;
        }

        public static void SetLocation(this Control c, int? x = null, int? y = null)
        {
            x ??= c.Location.X;
            y ??= c.Location.Y;

            c.Location = new((int)x, (int)y);
        }

        public static void SetLocation(this Control c, Point location)
        {
            c.Location = location;
        }

        public static void SetSize(this Control c, int? width = null, int? height = null)
        {
            width ??= c.Width;
            height ??= c.Height;

            c.Size = new((int)width, (int)height);
        }

        public static void SetSize(this Control c, Point size)
        {
            c.Size = size;
        }

        public static void SetBounds(this Control c, Rectangle bounds)
        {
            c.SetLocation(bounds.Location);
            c.SetSize(bounds.Size);
        }
    }
}
