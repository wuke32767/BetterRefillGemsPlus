using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
namespace Celeste.Mod.BetterRefillGemsPlus
{
    public class BetterRefillGemsPlusModuleSettings : EverestModuleSettings
    {
        bool _inner = true;
        public Color OutlineColor { get; set; } = new(255, 41, 41, 255);
        //public enum Alpha
        //{
        //    Ignore, Outline, Whole,
        //}
        //[SettingName("BetterRefillGemsPlus_Setting_Alpha_Type")]
        //[SettingSubText("BetterRefillGemsPlus_Setting_Alpha_Type_Desc")]
        //public Alpha AlphaMode {  get; set; } = Alpha.Ignore;

        //[SettingIgnore]
        //[YamlDotNet.Serialization.YamlIgnore]
        //public Color OutlineColorMixed
        //{
        //    get
        //    {
        //        static Color make(Color i)
        //        {
        //            i.A = 255;
        //            return i;
        //        }
        //        return AlphaMode switch
        //        {
        //            Alpha.Ignore => make(OutlineColor),
        //            Alpha.Outline => OutlineColor,
        //            Alpha.Whole => make(OutlineColor),
        //        };
        //    }
        //}

        [SettingIgnore]
        [YamlDotNet.Serialization.YamlIgnore]
        public bool IsInner
        {
            get => _inner;
            set
            {
                _inner = value;
                if (BetterRefillGemsPlusModule.Instance.Outline is { } effect)
                {
                    effect.CurrentTechnique = effect.Techniques[_inner ? "InnerOutline" : "OuterOutline"];
                }
            }
        }
        class Colored(string label, bool enterOnSelect, Func<Color> getter) : TextMenuExt.SubMenu(label, enterOnSelect)
        {
            public override void Render(Vector2 position, bool highlighted)
            {
                Vector2 vector = new(position.X, position.Y - Height() / 2f);
                //bool flag = Container.InnerContent == TextMenu.InnerContentMode.TwoColumn && !AlwaysCenter;

                Vector2 position2 = vector + Vector2.UnitY * TitleHeight / 2f /*+ (flag ? Vector2.Zero : new Vector2(Container.Width * 0.5f, 0f))*/;


                base.Render(position, highlighted);
                var WH = ActiveFont.LineHeight;
                var Right = position.X + Container.Width - WH / 2;
                var Top = position2.Y - WH / 2;
                Draw.Rect(Right - WH, Top, WH, WH, getter());
            }
        }
        class SubColored : TextMenu.Slider
        {
            MTexture Icon = GFX.Gui["downarrow"];

            public override void LeftPressed()
            {
                if (quick && Index != 0)
                {
                    Index -= 9;
                    if (Index < 1)
                    {
                        Index = 1;
                    }
                }
                base.LeftPressed();
            }
            public override void RightPressed()
            {
                if (quick && Index < Values.Count - 1)
                {
                    Index += 9;
                    if (Index > Values.Count - 2)
                    {
                        Index = Values.Count - 2;
                    }
                }
                base.RightPressed();
            }
            public override void Render(Vector2 position, bool highlighted)
            {
                float alpha = Container.Alpha;
                Color strokeColor = Color.Black * (alpha * alpha * alpha);
                Color color = Container.HighlightColor;

                Color ifColor = (highlighted ? Container.HighlightColor : Color.White) * alpha;
                Vector2 current = ActiveFont.Measure("255");
                Vector2 xcurrent = ActiveFont.Measure("R");
                float padding = ActiveFont.LineHeight;

                var val = Values[Index].Item2;

                Color basic = getter();
                float left = position.X + padding + xcurrent.X;
                float top = position.Y - padding / 4;
                float right = position.X + Container.Width - padding - current.X - padding;
                float bottom = position.Y + padding / 4;
                //Engine.Instance.GraphicsDevice.DrawUserPrimitives();
                GFX.DrawIndexedVertices<VertexPositionColor>(
                    Matrix.Identity,
                    [
                        new(new Vector3(left,top,0),masker(basic,0)),
                        new(new Vector3(right,top,0),masker(basic,255)),
                        new(new Vector3(left,bottom,0),masker(basic,0)),
                        new(new Vector3(right,bottom,0),masker(basic,255)),
                    ], 4, Triangle, 2);

                var pos = MathHelper.Lerp(left, right, val / 255f);

                Icon.DrawJustified(new(pos, position.Y), new(0.5f, 1), ifColor);
                //ActiveFont.DrawOutline(Label, , new Vector2(0.5f, 0.5f), Vector2.One, ifColor, 2, strokeColor);
                ActiveFont.DrawOutline(Label, new(position.X, position.Y), new Vector2(0, 0.5f), Vector2.One, ifColor, 2, strokeColor);
                float num = Container.Width;
                ActiveFont.DrawOutline(Values[Index].Item1, position + new Vector2(Container.Width - padding + (float)lastDir * ValueWiggler.Value * 8f, 0f), new Vector2(1, 0.5f), Vector2.One * 0.8f, ifColor, 2f, strokeColor);
                if (!highlighted)
                {
                    return;
                }
                Vector2 vector = Vector2.UnitX * (highlighted ? ((float)Math.Sin(sine * 4f) * 4f) : 0f);
                bool flag = Index > 0;
                Color color2 = (flag ? color : (Color.DarkSlateGray * alpha));
                Vector2 position2 = position + new Vector2(Container.Width + xcurrent.X - num + 40f + ((lastDir < 0) ? ((0f - ValueWiggler.Value) * 8f) : 0f), 0f) - (flag ? vector : Vector2.Zero);
                ActiveFont.DrawOutline("<", position2, new Vector2(0.5f, 0.5f), Vector2.One, color2, 2f, strokeColor);
                bool flag2 = Index < Values.Count - 1;
                Color color3 = (flag2 ? color : (Color.DarkSlateGray * alpha));
                Vector2 position3 = position + new Vector2(Container.Width - 40f - current.X - padding + ((lastDir > 0) ? (ValueWiggler.Value * 8f) : 0f), 0f) + (flag2 ? vector : Vector2.Zero);
                ActiveFont.DrawOutline(">", position3, new Vector2(0.5f, 0.5f), Vector2.One, color3, 2f, strokeColor);


            }
            private static readonly int[] Triangle = [0, 1, 2, 1, 3, 2];
            private readonly Func<Color, byte, Color> masker;
            private readonly Func<Color> getter;
            bool quick = false;
            public SubColored(string label, Func<Color, byte, Color> masker, Func<Color> getter, Action<Color> setter, int value = -1, Func<int, string>? values = null, int min = 0, int max = 255) : base(label, values ?? (x => x.ToString()), min, max, value)
            {
                this.masker = masker;
                this.getter = getter;
                OnAltPressed += () => quick = !quick;
                OnLeave += () => quick = false;
                Change(v =>
                {
                    var i = getter();
                    i = masker(i, (byte)v);
                    setter(i);
                    EntityImageHandler.Clear();
                    ImageRecolor.Clear();
                });
            }
        }
        public void CreateOutlineColorEntry(TextMenu textMenu, bool inGame)
        {
            if(inGame)
            {
                return;
            }
            var sub = new Colored(Dialog.Clean("BetterRefillGemsPlus_Setting_Color"), false, () => OutlineColor);
            sub.Add(new TextMenu.SubHeader(Dialog.Clean("BetterRefillGemsPlus_Setting_Color_Speed"), false));
            textMenu.Add(sub);
            sub.Add(new SubColored("R", (a, v) =>
            {
                a.R = v;
                return a;
            }, () => OutlineColor, v => OutlineColor = v,OutlineColor.R));
            sub.Add(new SubColored("G", (a, v) =>
            {
                a.G = v;
                return a;
            }, () => OutlineColor, v => OutlineColor = v,OutlineColor.G));
            sub.Add(new SubColored("B", (a, v) =>
            {
                a.B = v;
                return a;
            }, () => OutlineColor, v => OutlineColor = v,OutlineColor.B));
            //sub = new Colored(Dialog.Clean("BetterRefillGemsPlus_Setting_Color_A"), false, () => OutlineColor);
            //textMenu.Add(sub);
            //sub.Add(new SubColored("A", (a, v) =>
            //{
            //    a.A = v;
            //    return a;
            //}, () => OutlineColor, v => OutlineColor = v));
        }
    }
}
