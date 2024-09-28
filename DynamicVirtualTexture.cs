using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.BetterRefillGemsPlus
{
    internal class DynamicVirtualTexture : VirtualTexture
    {
        public DynamicVirtualTexture(string name, Texture2D texture)
            : base(name, texture.Width, texture.Height, default)
        {
            Texture_Safe = texture;
        }
        //not supported
        public override void Reload()
        {
            //do not do anything
        }
        public override void Dispose()
        {
            Unload();
            Texture_Safe = null;
        }
    }
}
