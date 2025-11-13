using Blish_HUD;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Controls;
using Manlaan.Mounts.Things;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Mounts;
using Mounts.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;

namespace Manlaan.Mounts.Controls
{
    internal class RadialThing
    {
        public double AngleBegin { get; set; }
        public double AngleEnd { get; set; }
        public Thing Thing { get; set; }
        public Texture2D Texture { get; set; }
        public int ImageX { get; set; }
        public int ImageY { get; set; }
        public bool Selected { get; set; }
        public bool Default { get; internal set; }
    }

    internal class DrawRadial : Container
    {
        private static readonly Logger Logger = Logger.GetLogger<DrawRadial>();
        private readonly Helper _helper;
        private readonly TextureCache _textureCache;

        private SemaphoreSlim handleShownHiddenSemaphore = new SemaphoreSlim(1, 1);

        public EventHandler OnSettingsButtonClicked { get; internal set; }
        private StandardButton _settingsButton;
        private Label _errorLabel;
        private readonly DrawRadialMenu drawRadialMenu;

        private List<RadialThing> RadialThings = new List<RadialThing>();

        private RadialThing SelectedMount => RadialThings.SingleOrDefault(m => m.Selected);
        private RadialThingSettings SelectedSettings = null;

        public override int ZIndex { get => base.ZIndex; set => base.ZIndex = value; }
        public bool IsActionCamToggledOnMount { get; private set; }

        int radius = 0;
        int thingIconSize = 0;
        int _maxRadialDiameter = 0;

        private Point SpawnPoint = default;
        private float debugLineThickness = 2;

        public DrawRadial(Helper helper, TextureCache textureCache)
        {
            Visible = false;
            Padding = Blish_HUD.Controls.Thickness.Zero;
            _helper = helper;
            _textureCache = textureCache;
            Shown += async (sender, e) => await HandleShown(sender, e);
            Hidden += async (sender, e) => await HandleHidden(sender, e);

            Location = new Point(0,0);
            Size = new(GameService.Graphics.SpriteScreen.Width, GameService.Graphics.SpriteScreen.Height);
            Module._debug.Add("GameService.Graphics.SpriteScreen", () => $"{GameService.Graphics.SpriteScreen.Width} {GameService.Graphics.SpriteScreen.Height}");
            //BackgroundColor = Color.Black * 0.2F;

            drawRadialMenu = new DrawRadialMenu(this, helper, textureCache)
            {
                Parent = this,
                Location = new Point(0, 0),
                Visible = true,
                ZIndex = 50,
            };
            drawRadialMenu.SetGraphicDevice();
            drawRadialMenu.Show();

            _errorLabel = new Label {
                Parent = this,
                Location = new Point(GameService.Graphics.SpriteScreen.Width/2, GameService.Graphics.SpriteScreen.Height/3),
                Size = new Point(800,500),
                Font = GameService.Content.DefaultFont32,
                TextColor = Color.Red,
                Visible = true //TODO REMOVE
            };
            _settingsButton = new StandardButton
            {
                Parent = this,
                Location = new Point(GameService.Graphics.SpriteScreen.Width / 2, GameService.Graphics.SpriteScreen.Height / 2),
                Text = Strings.Settings_Button_Label,
                Visible = true //TODO REMOVE
            };
            _settingsButton.Click += (args, sender) => {
                OnSettingsButtonClicked(args, sender);
            };
        }

        protected override CaptureType CapturesInput()
        {
            return CaptureType.None;
        }


        //public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
        //    RadialThings.Clear();

        //    var triggeredRadialSettings = _helper.GetTriggeredRadialSettings();
            
        //    if(triggeredRadialSettings  == null)
        //    {
        //        _errorLabel.Text = $"No triggered radial settings found!!!";
        //        _errorLabel.Show();
        //        return;
        //    }

        //    SelectedSettings = triggeredRadialSettings;

        //    var things = triggeredRadialSettings.AvailableThings.ToList();
        //    if (!things.Any())
        //    {
        //        _errorLabel.Text = $"No actions configured in the {triggeredRadialSettings.Name} context \n(or no keybinds specified for these actions)\nClick button to go to the relevant settings: ";
        //        _errorLabel.Show();
        //        _settingsButton.Show();
        //        return;
        //    }
        //    else
        //    {
        //        _errorLabel.Hide();
        //        _settingsButton.Hide();
        //    }

        //    if(triggeredRadialSettings is ContextualRadialThingSettings)
        //    {
        //        var triggeredContextual = (ContextualRadialThingSettings)triggeredRadialSettings;
        //        var tapThing = triggeredContextual.GetApplyInstantlyOnTapThing();
        //        if (tapThing != null && triggeredContextual.IsTapApplicable())
        //        {
        //            things.Remove(tapThing);
        //        }
        //    }

        //    var thingToPutInCenter = triggeredRadialSettings.GetCenterThing();
        //    if (thingToPutInCenter != null && thingToPutInCenter.IsAvailable)
        //    {
        //        if (triggeredRadialSettings.RemoveCenterThing.Value)
        //        {
        //            things.Remove(thingToPutInCenter);
        //        }
        //        var texture = _textureCache.GetThingImgFile(thingToPutInCenter);
        //        int loc = radius;
        //        RadialThings.Add(new RadialThing { Texture = texture, Thing = thingToPutInCenter, ImageX = loc, ImageY = loc, Default = true });
        //    }

        //    double startAngle = Math.PI * Math.Floor(Module._settingMountRadialStartAngle.Value * 360) / 180.0;
        //    if (DebugHelper.IsDebugEnabled())
        //    {
        //        var spawnPointVec = SpawnPoint.ToVector2();
        //        var rectpos = spawnPointVec - new Vector2(thingIconSize / 2, thingIconSize / 2);
        //        spriteBatch.DrawRectangle(rectpos, new Size2(thingIconSize, thingIconSize), Color.Red, debugLineThickness);
        //        spriteBatch.DrawCircle(spawnPointVec, 1, 50, Color.Red, debugLineThickness);
        //        spriteBatch.DrawCircle(spawnPointVec, GetRadiusAroundCenterThing(), 50, Color.Red, debugLineThickness);
        //    }
        //    double currentAngle = startAngle;
        //    var partAngleStep = Math.PI * 2 / things.Count();
        //    foreach (var thing in things)
        //    {
        //        var angleMid = currentAngle + partAngleStep / 2;
        //        var angleEnd = currentAngle + partAngleStep;
        //        var texture = _textureCache.GetThingImgFile(thing);

        //        int x = (int)Math.Round(radius + radius * Math.Cos(angleMid));
        //        int y = (int)Math.Round(radius + radius * Math.Sin(angleMid));

        //        if (DebugHelper.IsDebugEnabled())
        //        {
        //            float xDebugInner = (float)Math.Round(GetRadiusAroundCenterThing() * Math.Cos(currentAngle)) + SpawnPoint.X;
        //            float yDebugInner = (float)Math.Round(GetRadiusAroundCenterThing() * Math.Sin(currentAngle)) + SpawnPoint.Y;
        //            var debugRadiusOuter = 250;
        //            float xDebugOuter = (float)Math.Round(2*debugRadiusOuter * Math.Cos(currentAngle)) + SpawnPoint.X;
        //            float yDebugOuter = (float)Math.Round(2*debugRadiusOuter * Math.Sin(currentAngle)) + SpawnPoint.Y;
        //            spriteBatch.DrawLine(new Vector2(xDebugInner, yDebugInner), new Vector2(xDebugOuter, yDebugOuter), Color.Red, debugLineThickness);
        //        }


        //        RadialThings.Add(new RadialThing
        //        {
        //            Texture = texture,
        //            Thing = thing,
        //            ImageX = x,
        //            ImageY = y,
        //            AngleBegin = currentAngle,
        //            AngleEnd = angleEnd
        //        });
        //        currentAngle = angleEnd;
        //    }


        //    //Module._debug.Add("SpawnPoint", () => $"{SpawnPoint.X}, {SpawnPoint.Y}");
        //    //Module._debug.Add("position", () => $"{GameService.Input.Mouse.Position.X}, {GameService.Input.Mouse.Position.Y}");
        //    //Module._debug.Add("positionRaw", () => $"{GameService.Input.Mouse.PositionRaw.X}, {GameService.Input.Mouse.PositionRaw.Y}");
        //    //Module._debug.Add("spritescreen", () => $"{GameService.Graphics.SpriteScreen.Height}, {GameService.Graphics.SpriteScreen.Width}");
        //    //Module._debug.Add("window", () => $"{GameService.Graphics.WindowHeight}, {GameService.Graphics.WindowWidth}");
        //    //Module._debug.Add("calculation", () =>
        //    //{
        //    //    var x = 1.0f * GameService.Input.Mouse.PositionRaw.X / GameService.Graphics.WindowHeight * GameService.Graphics.SpriteScreen.Height;
        //    //    var y = 1.0f * GameService.Input.Mouse.PositionRaw.Y / GameService.Graphics.WindowWidth * GameService.Graphics.SpriteScreen.Width;
        //    //    x = (float)Math.Floor(x);
        //    //    y = (float)Math.Floor(y);
        //    //    x = Math.Max(x, 0);
        //    //    y = Math.Max(y, 0);
        //    //    return $"{x}, {y}";
        //    //});
        //    //Module._debug.Add("calculation2", () =>
        //    //{
        //    //    var mouseX2 = (int)(1.0f * GameService.Input.Mouse.PositionRaw.X / GameService.Graphics.WindowHeight * GameService.Graphics.SpriteScreen.Height);
        //    //    var mouseY2 = (int)(1.0f * GameService.Input.Mouse.PositionRaw.Y / GameService.Graphics.WindowWidth * GameService.Graphics.SpriteScreen.Width);
        //    //    mouseX2 = Math.Max(mouseX2, 0);
        //    //    mouseY2 = Math.Max(mouseY2, 0);
        //    //    return $"{mouseX2}, {mouseY2}";
        //    //});

        //    //var mouseX = (int)(1.0f * GameService.Input.Mouse.PositionRaw.X / GameService.Graphics.WindowHeight * GameService.Graphics.SpriteScreen.Height);
        //    //var mouseY = (int)(1.0f * GameService.Input.Mouse.PositionRaw.Y / GameService.Graphics.WindowWidth * GameService.Graphics.SpriteScreen.Width);
        //    //mouseX = Math.Max(mouseX, 0);
        //    //mouseY = Math.Max(mouseY, 0);
        //    //var mousePos = new Point(mouseX, mouseY);
        //    var mousePos = Input.Mouse.Position;
        //    var diff = mousePos - SpawnPoint;
        //    var angle = Math.Atan2(diff.Y, diff.X);
        //    while (angle < startAngle)
        //    {
        //        angle += Math.PI * 2; 
        //    }


        //    var length = new Vector2(diff.Y, diff.X).Length();
            
        //    foreach (var radialMount in RadialThings)
        //    {
        //        if (length < GetRadiusAroundCenterThing())
        //        {
        //            radialMount.Selected = radialMount.Default;
        //        } 
        //        else 
        //        {
        //            radialMount.Selected = radialMount.AngleBegin <= angle && radialMount.AngleEnd > angle;
        //        }

        //        spriteBatch.DrawOnCtrl(this, radialMount.Texture, new Rectangle(radialMount.ImageX, radialMount.ImageY, thingIconSize, thingIconSize), null, Color.White * (radialMount.Selected ? 1f : Module._settingMountRadialIconOpacity.Value));
        //    }

        //    //Module._dbg.Add("AngleBegin", () => $"{RadialMounts[8].AngleBegin}");
        //    //Module._dbg.Add("AngleEnd", () => $"{RadialMounts[8].AngleEnd}");
        //    //Module._dbg.Add("startangle", () => $"{startAngle}");
        //    //Module._dbg.Add("angle", () => $"{angle}");

        //    base.PaintBeforeChildren(spriteBatch, bounds);
        //}

        private float GetRadiusAroundCenterThing()
        {
            return (float)(thingIconSize * Math.Sqrt(2) / 2);
        }

        public async Task TriggerSelectedMountAsync()
        {
            var unconditionallyDoAction = false;
            if (SelectedSettings!= null && SelectedSettings is ContextualRadialThingSettings)
            {
                unconditionallyDoAction = ((ContextualRadialThingSettings)SelectedSettings).UnconditionallyDoAction.Value;
            }

            await (SelectedMount?.Thing.DoAction(unconditionallyDoAction, true) ?? Task.CompletedTask);
            SelectedSettings = null;
        }


        private async Task HandleShown(object sender, EventArgs e)
        {
            await handleShownHiddenSemaphore.WaitAsync();
            try
            {
                Logger.Debug("HandleShown entered");
                if (!GameService.Input.Mouse.CursorIsVisible && !Module._settingMountRadialToggleActionCameraKeyBinding.IsNull)
                {
                    IsActionCamToggledOnMount = true;
                    await _helper.TriggerKeybind(Module._settingMountRadialToggleActionCameraKeyBinding, WhichKeybindToRun.Both);
                    Logger.Debug("HandleShown turned off action cam");
                }

                _maxRadialDiameter = Math.Min(GameService.Graphics.SpriteScreen.Width, GameService.Graphics.SpriteScreen.Height);
                thingIconSize = (int)(_maxRadialDiameter / 4 * Module._settingMountRadialIconSizeModifier.Value);
                //radius = (int)((_maxRadialDiameter / 2 - thingIconSize / 2) * Module._settingMountRadialRadiusModifier.Value);
                //Size = new Point(_maxRadialDiameter, _maxRadialDiameter);


                if (Module._settingMountRadialSpawnAtMouse.Value)
                {
                    SpawnPoint = Input.Mouse.Position;
                }
                else
                {
                    Module._debug.Add("GameService.Graphics", () => $"{GameService.Graphics.SpriteScreen.Width} {GameService.Graphics.SpriteScreen.Height}");
                    Microsoft.Xna.Framework.Input.Mouse.SetPosition(GameService.Graphics.WindowWidth / 2, GameService.Graphics.WindowHeight / 2);
                    SpawnPoint = new Point(GameService.Graphics.SpriteScreen.Width / 2, GameService.Graphics.SpriteScreen.Height / 2);
                }

                if(drawRadialMenu != null)
                {
                    drawRadialMenu.Size = new Point(_maxRadialDiameter, _maxRadialDiameter);
                    drawRadialMenu.SetCenter(new Point(SpawnPoint.X, SpawnPoint.Y));
                }
            }
            finally
            {
                handleShownHiddenSemaphore.Release();
            }
        }

        private async Task HandleHidden(object sender, EventArgs e)
        {
            await handleShownHiddenSemaphore.WaitAsync();
            try
            {
                Logger.Debug("HandleHidden entered");
                if (IsActionCamToggledOnMount)
                {
                    await _helper.TriggerKeybind(Module._settingMountRadialToggleActionCameraKeyBinding, WhichKeybindToRun.Both);
                    IsActionCamToggledOnMount = false;
                    Logger.Debug("HandleHidden turned back on action cam");
                }
                await TriggerSelectedMountAsync();
            }
            finally
            {
                handleShownHiddenSemaphore.Release();
            }
        }
    }
}