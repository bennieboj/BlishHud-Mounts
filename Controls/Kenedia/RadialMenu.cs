using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Structs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static Blish_HUD.ContentService;

namespace Kenedia.Modules.Core.Controls
{
    /// <summary>
    /// Represents a customizable radial menu control, commonly used for context menus or tool selection.
    /// </summary>
    /// <remarks>The <see cref="RadialMenu"/> divides its circular area into a specified number of slices,
    /// each of which can be styled and interacted with.  It supports various visual customizations, including
    /// gradients, borders, and fade effects. The menu is rendered as a donut shape,  with an adjustable hole size and
    /// slice-specific highlighting when hovered or clicked.</remarks>
    public class RadialMenu : Control
    {
        private AsyncTexture2D _texture = AsyncTexture2D.FromAssetId(102804); // Set your icon texture here

        public RadialMenu()
        {
            SetGraphicDevice();
        }

        public void SetGraphicDevice()
        {
            using var gdc = GameService.Graphics.LendGraphicsDeviceContext();

            DpiScale = gdc.GraphicsDevice.PresentationParameters.BackBufferWidth
                / (float)GameService.Graphics.SpriteScreen.Size.X;

            Radius = (int)(Math.Min(Width, Height) / 2 * DpiScale);
        }

        public float DpiScale { get; protected set; }

        public Vector2 Center { get; protected set; }

        public Vector2 MousePos { get; protected set; }

        public int Radius { get; protected set; }

        /// <summary>
        /// Defines the number of slices the radial menu is divided into.
        /// </summary>
        public int Slices { get; set; } = 8;

        /// <summary>
        /// Gets or sets the background gradient used for rendering slices.
        /// </summary>
        public ColorGradient SliceBackground { get; set; } = new(Color.Black * 0.5F);

        /// <summary>
        /// Gets or sets the highlight gradient used when a slice is hovered over or selected.
        /// </summary>
        public ColorGradient SliceHighlight { get; set; } = new(Colors.ColonialWhite * 0.5F);

        /// <summary>
        /// Gets or sets a value indicating whether the inner border should be displayed.
        /// </summary>
        public bool ShowInnerBorder { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether a border is displayed around each slice.
        /// </summary>
        public bool ShowSliceBorder { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the outer border should be displayed.
        /// </summary>
        public bool ShowOuterBorder { get; set; } = false;

        /// <summary>
        /// Gets or sets the percentage of the donut that is occupied by the hole.
        /// </summary>
        /// <remarks>The value must be within the range of 0.0 to 1.0, where 0.0 indicates no hole and 1.0
        /// indicates the entire donut is a hole.</remarks>
        public float DonutHolePercent { get; set; } = 0.4f;

        /// <summary>
        /// Gets or sets the percentage of the fade effect to apply to the <see cref="SliceBackground"/> and <see cref="SliceHighlight"/>. 
        /// </summary>
        /// <remarks>The value must be within the range of 0.0 to 1.0, where 0.0 indicates no fading and 1.0
        /// indicates the entire slice background will be faded.</remarks>
        public float FadePercent { get; set; } = 0.20f;

        /// <summary>
        /// Gets or sets the thickness of the outer border, in device-independent units (DIPs).
        /// </summary>
        public float OuterBorderThickness { get; set; } = 5F;

        /// <summary>
        /// Gets or sets the thickness of the inner border, in device-independent units (DIPs).
        /// </summary>
        public float InerBorderThickness { get; set; } = 5F;

        /// <summary>
        /// Gets or sets the thickness of the border for a slice, in device-independent units (DIPs).
        /// </summary>
        public float SliceBorderThickness { get; set; } = 2F;

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            /// Radius for the slices is half the smaller dimension, scaled for DPI
            Radius = (int)(Math.Min(Width, Height) / 2 * DpiScale);
        }

        protected override void OnHidden(EventArgs e)
        {
            base.OnHidden(e);
            RunActionWhenMouseContains(OnSliceOnHoverHidden);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
            RunActionWhenMouseContains(OnSliceClick);
        }

        private void RunActionWhenMouseContains(Action<int> onSliceAction)
        {
            // Angle per slice
            float angleStep = MathHelper.TwoPi / Slices;
            float startOffset = -MathHelper.PiOver2; // top of circle

            for (int i = 0; i < Slices; i++)
            {
                float startAngle = startOffset + i * angleStep;
                float endAngle = startOffset + (i + 1) * angleStep;
                float midAngle = (startAngle + endAngle) / 2f;

                // Mouse position relative to Center
                Vector2 dir = MousePos - Center;
                float distance = dir.Length();

                // Mouse angle (0 = right), we rotate by -startOffset so slice 0 starts at top
                float angle = (float)Math.Atan2(dir.Y, dir.X) - startOffset;
                if (angle < 0) angle += MathHelper.TwoPi;

                // Slice boundaries in [0, TwoPi]
                float sliceStart = i * angleStep;
                float sliceEnd = (i + 1) * angleStep;

                bool contains_mouse = distance <= Radius && angle >= sliceStart && angle <= sliceEnd;

                if (contains_mouse)
                {
                    onSliceAction(i);
                }
            }
        }

        /// <summary>
        /// Handles the event when a slice is hovered over when the radial is hidden.
        /// </summary>
        /// <remarks>This method is called to process a slice hover event when the radial is hidden. Derived classes can override
        /// this method  to provide custom handling for slice hover events when hidden.</remarks>
        /// <param name="i">The index of the clicked slice. Must be a non-negative integer.</param>
        protected virtual void OnSliceOnHoverHidden(int i)
        {
            Debug.WriteLine($"Hovered on slice {i} when radial was hidden");
        }

        /// <summary>
        /// Handles the event when a slice is clicked.
        /// </summary>
        /// <remarks>This method is called to process a slice click event. Derived classes can override
        /// this method  to provide custom handling for slice clicks.</remarks>
        /// <param name="i">The index of the clicked slice. Must be a non-negative integer.</param>
        protected virtual void OnSliceClick(int i)
        {
            Debug.WriteLine($"Clicked on slice {i}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="contains_mouse"></param>
        /// <param name="Center"></param>
        /// <param name="midAngle"></param>
        /// <param name="iconRadius"></param>
        /// <param name="sliceIndex"></param>
        protected virtual void DrawSliceContent(SpriteBatch spriteBatch, bool contains_mouse, Vector2 center, float midAngle, float iconRadius, int sliceIndex)
        {
            // Icon
            float iconSize = 64; // Set your desired icon size here
            float scale = iconSize / (float)_texture.Width;

            Vector2 relativeCenter = center / DpiScale - (_texture.Bounds.Size.ToVector2() * scale / 2);
            spriteBatch.Draw(_texture, relativeCenter, _texture.Bounds, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 1);

            if (contains_mouse)
            {
                //Set tooltips or whatnot
            }
        }

        public void SetCenter(Point position)
        {
            this.SetLocation(position.X - (Width / 2), position.Y - (Height / 2));

            var p = position.ToVector2() * DpiScale;
            var absBounds = this.AbsoluteBounds.Center.ToVector2() * DpiScale;
            Center = new Vector2(absBounds.X, absBounds.Y);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            MousePos = GameService.Input.Mouse.Position.ToVector2() * DpiScale;
            DrawRadialMenu(spriteBatch);
        }

        private void DrawRadialMenu(SpriteBatch spriteBatch)
        {
            if (Slices == 0) return;
            // Angle per slice
            float angleStep = MathHelper.TwoPi / Slices;
            float startOffset = -MathHelper.PiOver2; // top of circle
            bool any_contains_mouse = false;

            for (int i = 0; i < Slices; i++)
            {
                float startAngle = startOffset + i * angleStep;
                float endAngle = startOffset + (i + 1) * angleStep;
                float midAngle = (startAngle + endAngle) / 2f;

                // Mouse position relative to Center
                Vector2 dir = MousePos - Center;
                float distance = dir.Length();

                // Mouse angle (0 = right), we rotate by -startOffset so slice 0 starts at top
                float angle = (float)Math.Atan2(dir.Y, dir.X) - startOffset;
                if (angle < 0) angle += MathHelper.TwoPi;

                // Slice boundaries in [0, TwoPi]
                float sliceStart = i * angleStep;
                float sliceEnd = (i + 1) * angleStep;

                bool contains_mouse = distance <= Radius && angle >= sliceStart && angle <= sliceEnd && !any_contains_mouse;
                any_contains_mouse |= contains_mouse;

                var slice_background = GetSliceColors(i, contains_mouse);

                // Draw the slice
                float innerRadius = Radius * DonutHolePercent; // adjust for donut thickness
                var slice_center = Center + (new Vector2((float)Math.Cos(midAngle), (float)Math.Sin(midAngle)) * (innerRadius + (Radius - innerRadius) / 2f));

                DrawGradientDonutSlice(startAngle, endAngle, slice_background.Start * (slice_background.Start.A / 255F), slice_background.End * (slice_background.End.A / 255F), DonutHolePercent, FadePercent, OuterBorderThickness, InerBorderThickness);
                DrawSliceContent(spriteBatch, contains_mouse, slice_center, midAngle, innerRadius, i);
            }
        }

        protected virtual ColorGradient GetSliceColors(int index, bool contains_mouse)
        {
            return contains_mouse ? SliceHighlight : SliceBackground;
        }

        private void DrawRadialBorder(GraphicsDevice GraphicsDevice, BasicEffect Effect, Vector2 Center, float innerR, float outerR, float angle, float thickness, Color color)
        {
            var verts = new List<VertexPositionColor>();

            Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            Vector2 normal = new Vector2(-dir.Y, dir.X) * thickness;

            Vector2 i1 = Center + dir * innerR - normal;
            Vector2 i2 = Center + dir * innerR + normal;
            Vector2 o1 = Center + dir * outerR - normal;
            Vector2 o2 = Center + dir * outerR + normal;

            verts.Add(new VertexPositionColor(new Vector3(i1, 0), color));
            verts.Add(new VertexPositionColor(new Vector3(o1, 0), Color.Transparent));
            verts.Add(new VertexPositionColor(new Vector3(o2, 0), Color.Transparent));

            verts.Add(new VertexPositionColor(new Vector3(i1, 0), color));
            verts.Add(new VertexPositionColor(new Vector3(o2, 0), Color.Transparent));
            verts.Add(new VertexPositionColor(new Vector3(i2, 0), color));

            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts.ToArray(), 0, verts.Count / 3);
            }
        }

        private void DrawGradientDonutSlice(float startAngle, float endAngle, Color solidColor, Color edgeFadeColor, float clearPercent = 0.20f, float fadePercent = 0.20f, float outerBorderThickness = 5f, float innerBorderThickness = 5f, int segments = 48)
        {
            using var gdc = GameService.Graphics.LendGraphicsDeviceContext();
            var graphicsDevice = gdc.GraphicsDevice;
            var effect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                Projection = Matrix.CreateOrthographicOffCenter
               (
                   0, graphicsDevice.Viewport.Width,
                   graphicsDevice.Viewport.Height, 0,
                   0, 1
               )
            };

            if (graphicsDevice == null || effect == null || segments < 2) return;

            clearPercent = MathHelper.Clamp(clearPercent, 0f, 0.95f);
            fadePercent = MathHelper.Clamp(fadePercent, 0f, 1f);

            float clearRadius = Radius * clearPercent;
            float solidRadius = Radius * (1f - fadePercent);
            float fadeRadius = Radius;

            if (solidRadius < clearRadius)
                solidRadius = clearRadius;

            float angleStep = (endAngle - startAngle) / segments;

            // -----------------------------
            // 1) SOLID RING: clear → solid
            // -----------------------------
            var solidVerts = new VertexPositionColor[segments * 6];
            int idx = 0;

            for (int i = 0; i < segments; i++)
            {
                float a1 = startAngle + i * angleStep;
                float a2 = a1 + angleStep;

                Vector2 i1 = Center + new Vector2((float)Math.Cos(a1), (float)Math.Sin(a1)) * clearRadius;
                Vector2 o1 = Center + new Vector2((float)Math.Cos(a1), (float)Math.Sin(a1)) * solidRadius;
                Vector2 i2 = Center + new Vector2((float)Math.Cos(a2), (float)Math.Sin(a2)) * clearRadius;
                Vector2 o2 = Center + new Vector2((float)Math.Cos(a2), (float)Math.Sin(a2)) * solidRadius;

                solidVerts[idx++] = new VertexPositionColor(new Vector3(i1, 0), solidColor);
                solidVerts[idx++] = new VertexPositionColor(new Vector3(o1, 0), solidColor);
                solidVerts[idx++] = new VertexPositionColor(new Vector3(o2, 0), solidColor);

                solidVerts[idx++] = new VertexPositionColor(new Vector3(i1, 0), solidColor);
                solidVerts[idx++] = new VertexPositionColor(new Vector3(o2, 0), solidColor);
                solidVerts[idx++] = new VertexPositionColor(new Vector3(i2, 0), solidColor);
            }

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, solidVerts, 0, segments * 2);
            }

            // -------------------------------------
            // 2) FADE RING: solidRadius → radius
            // -------------------------------------
            if (fadePercent > 0f)
            {
                var fadeVerts = new VertexPositionColor[segments * 6];
                idx = 0;

                for (int i = 0; i < segments; i++)
                {
                    float a1 = startAngle + i * angleStep;
                    float a2 = a1 + angleStep;

                    Vector2 i1 = Center + new Vector2((float)Math.Cos(a1), (float)Math.Sin(a1)) * solidRadius;
                    Vector2 o1 = Center + new Vector2((float)Math.Cos(a1), (float)Math.Sin(a1)) * fadeRadius;
                    Vector2 i2 = Center + new Vector2((float)Math.Cos(a2), (float)Math.Sin(a2)) * solidRadius;
                    Vector2 o2 = Center + new Vector2((float)Math.Cos(a2), (float)Math.Sin(a2)) * fadeRadius;

                    fadeVerts[idx++] = new VertexPositionColor(new Vector3(i1, 0), solidColor);
                    fadeVerts[idx++] = new VertexPositionColor(new Vector3(o1, 0), edgeFadeColor);
                    fadeVerts[idx++] = new VertexPositionColor(new Vector3(o2, 0), edgeFadeColor);

                    fadeVerts[idx++] = new VertexPositionColor(new Vector3(i1, 0), solidColor);
                    fadeVerts[idx++] = new VertexPositionColor(new Vector3(o2, 0), edgeFadeColor);
                    fadeVerts[idx++] = new VertexPositionColor(new Vector3(i2, 0), solidColor);
                }

                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, fadeVerts, 0, segments * 2);
                }
            }

            // -------------------------------------
            // 3) OUTER BORDER: fade outward
            // -------------------------------------
            if (ShowOuterBorder && outerBorderThickness > 0f)
            {
                var borderVerts = new VertexPositionColor[segments * 6];
                idx = 0;

                float innerBorderR = Radius - outerBorderThickness;
                float outerBorderR = Radius;

                for (int i = 0; i < segments; i++)
                {
                    float a1 = startAngle + i * angleStep;
                    float a2 = a1 + angleStep;

                    Vector2 i1 = Center + new Vector2((float)Math.Cos(a1), (float)Math.Sin(a1)) * innerBorderR;
                    Vector2 o1 = Center + new Vector2((float)Math.Cos(a1), (float)Math.Sin(a1)) * outerBorderR;
                    Vector2 i2 = Center + new Vector2((float)Math.Cos(a2), (float)Math.Sin(a2)) * innerBorderR;
                    Vector2 o2 = Center + new Vector2((float)Math.Cos(a2), (float)Math.Sin(a2)) * outerBorderR;

                    borderVerts[idx++] = new VertexPositionColor(new Vector3(i1, 0), solidColor);
                    borderVerts[idx++] = new VertexPositionColor(new Vector3(o1, 0), edgeFadeColor);
                    borderVerts[idx++] = new VertexPositionColor(new Vector3(o2, 0), edgeFadeColor);

                    borderVerts[idx++] = new VertexPositionColor(new Vector3(i1, 0), solidColor);
                    borderVerts[idx++] = new VertexPositionColor(new Vector3(o2, 0), edgeFadeColor);
                    borderVerts[idx++] = new VertexPositionColor(new Vector3(i2, 0), solidColor);
                }

                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, borderVerts, 0, segments * 2);
                }
            }

            // -------------------------------------
            // 4) INNER BORDER: around clear hole
            // -------------------------------------
            if (ShowInnerBorder && innerBorderThickness > 0f)
            {
                var borderVerts = new VertexPositionColor[segments * 6];
                idx = 0;

                float innerBorderR = clearRadius;
                float outerBorderR = clearRadius + innerBorderThickness;

                for (int i = 0; i < segments; i++)
                {
                    float a1 = startAngle + i * angleStep;
                    float a2 = a1 + angleStep;

                    Vector2 i1 = Center + new Vector2((float)Math.Cos(a1), (float)Math.Sin(a1)) * innerBorderR;
                    Vector2 o1 = Center + new Vector2((float)Math.Cos(a1), (float)Math.Sin(a1)) * outerBorderR;
                    Vector2 i2 = Center + new Vector2((float)Math.Cos(a2), (float)Math.Sin(a2)) * innerBorderR;
                    Vector2 o2 = Center + new Vector2((float)Math.Cos(a2), (float)Math.Sin(a2)) * outerBorderR;

                    borderVerts[idx++] = new VertexPositionColor(new Vector3(i1, 0), solidColor);  // fades inward
                    borderVerts[idx++] = new VertexPositionColor(new Vector3(o1, 0), solidColor);
                    borderVerts[idx++] = new VertexPositionColor(new Vector3(o2, 0), solidColor);

                    borderVerts[idx++] = new VertexPositionColor(new Vector3(i1, 0), solidColor);
                    borderVerts[idx++] = new VertexPositionColor(new Vector3(o2, 0), solidColor);
                    borderVerts[idx++] = new VertexPositionColor(new Vector3(i2, 0), solidColor);
                }

                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, borderVerts, 0, segments * 2);
                }
            }

            if (ShowSliceBorder)
            {
                DrawRadialBorder(graphicsDevice, effect, Center, clearRadius + (ShowInnerBorder ? innerBorderThickness : 0F), fadeRadius, startAngle, SliceBorderThickness, solidColor);
                DrawRadialBorder(graphicsDevice, effect, Center, clearRadius + (ShowInnerBorder ? innerBorderThickness : 0F), fadeRadius, endAngle, SliceBorderThickness, solidColor);
            }
        }
    }
}
