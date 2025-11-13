using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Structs;
using Manlaan.Mounts.Things;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Manlaan.Mounts.Controls
{
    internal class DrawRadialMenu : RadialMenu
    {
        //private readonly Data _data;
        private readonly AsyncTexture2D _dummy = AsyncTexture2D.FromAssetId(1128572);
        //private readonly CharacterTooltip _tooltip;

        //private readonly Settings _settings;
        //private readonly ObservableCollection<Character_Model> _characters;
        //private readonly Func<Character_Model> _currentCharacter;
        private List<Thing> _displayedThings;
        private int _iconSize;

        private readonly List<RadialThing> _sections = new();
        private readonly Helper _helper;
        private readonly TextureCache _textureCache;
        private RadialThing? _selected;

        private Vector2 _center;
        private GraphicsDevice _graphicsDevice;
        private BasicEffect _effect;

        public DrawRadialMenu(Container parent, Helper helper, TextureCache textureCache) : base()
        {
            //_settings = settings;

            //_settings.Radial_SliceHighlight.PropertyChanged += Radial_Colors_PropertyChanged;
            //_settings.Radial_SliceBackground.PropertyChanged += Radial_Colors_PropertyChanged;

            //_characters = characters;
            //_currentCharacter = currentCharacter;
            //_data = data;
            Parent = parent;

            //_tooltip = new(currentCharacter, textureManager, data, settings)
            //{
            //    Parent = GameService.Graphics.SpriteScreen,
            //    ZIndex = int.MaxValue / 2 + 1,
            //    Size = new Point(300, 50),
            //    Visible = false,
            //};

            BackgroundColor = Color.Red * 0.2F;

            //foreach (Character_Model c in _characters)
            //{
            //    c.Updated += Character_Updated;
            //}

            Parent.Resized += Parent_Resized;
            _helper = helper;
            _textureCache = textureCache;

            //SliceBackground = _settings.Radial_SliceBackground.Value;
            //SliceHighlight = _settings.Radial_SliceHighlight.Value;
        }

        //private void Radial_Colors_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    SliceBackground = _settings.Radial_SliceBackground.Value;
        //    SliceHighlight = _settings.Radial_SliceHighlight.Value;
        //}

        private void Parent_Resized(object sender, ResizedEventArgs e)
        {
            RecalculateLayout();
        }

        private void Character_Updated(object sender, EventArgs e)
        {
            RecalculateLayout();
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (_graphicsDevice is null)
            {
                return;
            }
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);

            //_selected = null;
            //_tooltip.Character = null;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            var triggeredRadialSettings = _helper.GetTriggeredRadialSettings();

            if (triggeredRadialSettings == null)
            {
                //_errorLabel.Text = $"No triggered radial settings found!!!";
                //_errorLabel.Show();
                return;
            }

            //SelectedSettings = triggeredRadialSettings;

            _displayedThings = triggeredRadialSettings.AvailableThings.ToList();
            Slices = _displayedThings.Count;


            //_tooltip?.Hide();
            base.Paint(spriteBatch, bounds);
        }

        protected override ColorGradient GetSliceColors(int index, bool contains_mouse)
        {
            //if (_settings.Radial_UseProfessionColor.Value && _displayedThings.Count > index)
            //{
            //    var character = _displayedCharacters[index];
            //    return new ColorGradient(character.Profession.GetProfessionColor() * (contains_mouse ? 0.8F : 0.5F));
            //}

            return base.GetSliceColors(index, contains_mouse);
        }

        protected override void DrawSliceContent(SpriteBatch spriteBatch, bool contains_mouse, Vector2 center, float midAngle, float iconRadius, int sliceIndex)
        {
            if (_displayedThings.Count <= sliceIndex) return;

            var thing = _displayedThings[sliceIndex];
            var texture = _textureCache.GetThingImgFile(thing);


            // Slice geometry
            int totalSlices = _displayedThings.Count;
            float sliceAngle = MathHelper.TwoPi / totalSlices;

            // Available chord width at iconRadius
            float sliceWidthAtCenter = 2f * iconRadius * (float)Math.Sin(sliceAngle / 2f);

            // Icon size based on that width
            float iconSize = sliceWidthAtCenter * 0.75f;
            float scale = iconSize / (float)texture.Width;

            // Place icon at the given midAngle and radius
            Vector2 iconOffset = new Vector2(
                (float)Math.Cos(midAngle),
                (float)Math.Sin(midAngle)
            ) * iconRadius;

            Vector2 relativeCenter = center / DpiScale - (texture.Bounds.Size.ToVector2() * scale / 2);
            Module._debug.Add("center", () => $"{center} {Center} {relativeCenter} {Input.Mouse.Position}");

            var color = /*_settings.Radial_UseProfessionIconsColor.Value ? thing.Profession.GetProfessionColor() : */ Color.White;
            spriteBatch.Draw(texture, relativeCenter, texture.Bounds, color, 0, Vector2.Zero, scale, SpriteEffects.None, 1);

            // Tooltip handling
            if (contains_mouse)
            {
                BasicTooltipText = thing.Name;
                //if (_settings.Radial_ShowAdvancedTooltip.Value)
                //{
                //    BasicTooltipText = string.Empty;
                //    _selected = thing;
                //    _tooltip.Character = thing;
                //    _tooltip.Show();
                //}
                //else
                //{
                //}
            }
        }

        protected override async void OnSliceClick(int i)
        {
            if (_selected is not null)
            {
                Hide();

                if (_displayedThings.Count > i)
                {
                    var character = _displayedThings[i];

                    //if (await ExtendedInputService.WaitForNoKeyPressed())
                    //{
                    //    character.Swap();
                    //}
                }
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            RecalculateLayout();
        }

        protected override void OnHidden(EventArgs e)
        {
            base.OnHidden(e);
            //_tooltip.Character = null;
            //_tooltip.Hide();
        }

        protected override void DisposeControl()
        {
            if (Parent is not null) Parent.Resized -= Parent_Resized;

            //foreach (Character_Model c in _characters)
            //{
            //    c.Updated -= Character_Updated;
            //}

            //_tooltip?.Dispose();
            base.DisposeControl();
        }



    }
}