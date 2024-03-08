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
        /// <summary>
        /// do not pre-process asynchronously temporarily.
        /// I have get celeste freezed in another mod.
        /// </summary>
        /// <returns></returns>
        //
        public override Dictionary<string, Action<BinaryPacker.Element>> Init()
        {
            return new()
            {
                //{"entity:refill", refill =>
                //{
                //    ImageRecolor.MarkResourceImageAsync("objects/refillTwo/idle");
                //    ImageRecolor.MarkResourceImageAsync("objects/refill/idle");
                //}},
                //{"entity:refill", refill =>
                //{
                //    Recolor.MarkResourceImage("objects/refillTwo/idle");
                //    Recolor.MarkResourceImage("objects/refill/idle");
                //}},
            };
        }

        public override void Reset()
        {
        }
    }
}