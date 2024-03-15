using System;
using System.Collections.Generic;

namespace Celeste.Mod.BetterRefillGemsPlus
{
    internal class PrepareImageMapDataProcessor : EverestMapDataProcessor
    {
        public PrepareImageMapDataProcessor()
        {
        }

        public override void End()
        {
        }
        public static Dictionary<string, Action<BinaryPacker.Element>> preprocessor = new()
        {
            {"entity:refill", refill =>
            {
                if(refill.AttrBool("oneUse"))
                {
                    if(refill.AttrBool("twoDash"))
                    {
                        ImageRecolor.MarkResourceSpriteAsync("objects/refillTwo/idle");
                    }
                    else
                    { 
                        ImageRecolor.MarkResourceSpriteAsync("objects/refill/idle");
                    }
                }
            }},
            //{"entity:refill", refill =>
            //{
            //    Recolor.MarkResourceImage("objects/refillTwo/idle");
            //    Recolor.MarkResourceImage("objects/refill/idle");
            //}},
        };

        public override Dictionary<string, Action<BinaryPacker.Element>> Init()
        {
            return preprocessor;
        }

        public override void Reset()
        {
        }
    }
}