using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Mounts.Controls;
using System.Collections.Generic;

namespace Manlaan.Mounts.Views
{
    class TestingView : View
    {
        internal class MyReorderableItem : IReorderableListItemContent
        {
            private Label? _label;
            private readonly string _text = string.Empty;

            public MyReorderableItem(string text)
            {
                _text = text;
            }

            Control IReorderableListItemContent.CreateContent()
            {
                _label = new Label()
                {
                    Text = "TEXT " + _text // replaced below
                };

                // configure label after construction
                _label.Text = "TEXT " + _text;
                _label.AutoSizeHeight = true;
                _label.Width = 300;
                _label.Left = 8;
                _label.Top = 8;

                // return the control; the base will set Parent = ContentPanel
                return _label;
            }
        }

        private readonly TextureCache textureCache;

        public TestingView(TextureCache textureCache)
        {
            this.textureCache = textureCache;
        }

        protected override void Build(Container buildPanel)
        {

        //    Label l = new Label
        //    {
        //        Text = "I don't expect anything in return, but if you want you can:\n- send some gold/items ingame: Bennieboj.2607\n- donate via Ko-fi:",
        //        Location = new Point(300, 300),
        //        Width = 800,
        //        AutoSizeHeight = true,
        //        WrapText = true,
        //        Font = GameService.Content.DefaultFont18,
        //        HorizontalAlignment = HorizontalAlignment.Left,
        //        Parent = buildPanel
        //    };

        //    StandardButton kofiSupport = new StandardButton
        //    {
        //        Left = 370,
        //        Top = 400,
        //        Icon = textureCache.GetImgFile(TextureCache.KofiTextureName),
        //        Height = 60,
        //        Width = 130,
        //        Parent = buildPanel,
        //        Text = "Ko-fi"
        //};
        //    kofiSupport.Click += delegate
        //    {
        //        Process.Start("https://ko-fi.com/bennieboj");
        //    };


            var mylist = new List<string> { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l" };
            var reorderable = new ReorderableList<MyReorderableItem>()
            {
                Parent = buildPanel,
                Width = 400,
                Height = 1000
            };
            foreach (var item in mylist)
            {
                reorderable.AddItem(new MyReorderableItem(item));
            }
        }
    }
}
