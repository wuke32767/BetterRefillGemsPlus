using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Celeste.Mod.BetterRefillGemsPlus.InteropAndInternalop;

namespace Celeste.Mod.BetterRefillGemsPlus
{
    static class EntityImageHandler
    {
       public static Dictionary<Type, Func<Entity, bool>> oneuser = [];
        public static CacheDictionary<Type, List<Action<Entity>>> Registered = new(x => []);
        public static CacheDictionary<string, List<Action<Entity>>> RegisteredRefl = new(x => []);
        internal static void Load()
        {
            RegisterSprite(typeof(Refill), e => (e as Refill).sprite, e => (e as Refill).oneUse);
        }
        internal static void LoadContent()
        {
            RegisterAs((null, "Celeste.Mod.DJMapHelper.Entities.ColorfulRefill"), (typeof(Refill), null));
            RegisterAs((null, "Celeste.Mod.MaxHelpingHand.Entities.CustomizableRefill"), (typeof(Refill), null));

            RegisterAs((null, "Celeste.Mod.CommunalHelper.DashStates.DreamTunnelRefill"), (typeof(Refill), null));
            RegisterAs((null, "Celeste.Mod.CommunalHelper.DashStates.SeekerDashRefill"), (typeof(Refill), null));
            RegisterAs((null,"Celeste.Mod.CommunalHelper.Entities.StrawberryJam.ExpiringDashRefill" ), (typeof(Refill), null));

            RegisterAs((null,"ExtendedVariants.Entities.ForMappers.JumpRefill"), (typeof(Refill), null));
            
            RegisterSpriteReflectionReflection("Celeste.Mod.Anonhelper.CoreRefill", "sprite", "oneUse");
            RegisterSpriteReflectionReflection("Celeste.Mod.Anonhelper.CloudRefill", "sprite", "oneUse");
            RegisterSpriteReflectionReflection("Celeste.Mod.Anonhelper.BoosterRefill", "sprite", "oneUse");
            RegisterSpriteReflectionReflection("Celeste.Mod.Anonhelper.FeatherRefill", "sprite", "oneUse");
            RegisterSpriteReflectionReflection("Celeste.Mod.Anonhelper.JellyRefill", "sprite", "oneUse");
            RegisterSpriteReflectionReflection("Celeste.Mod.Anonhelper.SuperDashRefill", "sprite", "oneUse");

            RegisterSpriteReflectionReflection("Celeste.Mod.ArphimigonHelper.RefillRefill", "sprite", "oneUse");
            RegisterSpriteReflectionReflection("Celeste.Mod.ArphimigonHelper.DifficultRefill", "sprite", "oneUse");

            RegisterSpriteReflectionReflection("Celeste.Mod.CommunalHelper.Entities.ShieldedRefill", "sprite", "oneUse");
            RegisterSpriteReflectionReflection("Celeste.Mod.CommunalHelper.Entities.ShieldedRefill", "sprite", "oneUse");

            RegisterSpriteReflectionReflection("FrostHelper.PlusOneRefill", "sprite", "oneUse");
            //RegisterSpriteReflectionReflection("FrostHelper.HeldRefill", "Sprite", "oneUse");

            RegisterSpriteReflectionReflection("Celeste.Mod.JackalHelper.Entities.CryoRefill", "sprite", "oneUse");
            RegisterSpriteReflectionReflection("Celeste.Mod.JackalHelper.Entities.StopwatchRefill", "sprite", "oneUse");

            RegisterSpriteReflectionReflection("Celeste.Mod.ReverseHelper.Entities.HoldableRefill", "sprite", "oneUse");
            RegisterSpriteReflectionReflection("Celeste.Mod.ReverseHelper.Entities.LongDashRefill", "sprite", "oneUse");

            RegisterSpriteReflectionReflection("MoreDasheline.SpecialRefill", "sprite", "oneUse");

            //RegisterSpriteReflectionReflection("VivHelper.Entities.RedDashRefill", "sprite", "oneUse");
            RegisterSpriteReflectionReflection("VivHelper.Entities.WarpDashRefill", "sprite", "oneUse");

            RegisterSpriteReflectionReflection("Celeste.Mod.BounceHelper.BounceRefill", "sprite", "oneUse");

            RegisterSpriteReflectionReflection("Celeste.Mod.Batteries.PowerRefill", "sprite", "oneUse");

            RegisterSpriteReflectionReflection("Celeste.Mod.CherryHelper.ShadowDashRefill", "sprite", "oneUse");

            RegisterSpriteReflectionReflection("Celeste.Mod.SaladimHelper.Entities.FlagRefill", "sprite", "oneUse");
            RegisterSpriteReflectionReflection("Celeste.Mod.SaladimHelper.Entities.BitsMomentumRefill", "sprite", "oneUse");

            RegisterSpriteReflectionReflection("Celeste.Mod.XaphanHelper.Entities.TimerRefill", "sprite", "oneUse");


            //they shared sprite
            //RegisterSpriteReflectionReflection("Celeste.Mod.ChronoHelper.Entities.ShatterRefill", "sprite", "oneUse");

            //RegisterSpriteReflectionReflection("Celeste.Mod.GravityHelper.Entities.GravityRefill", "_sprite", "OneUse");

            //RegisterSpriteReflectionReflection("Celeste.Mod.JackalHelper.Entities.StarRefill", "sprite", "oneUse");
            //RegisterSpriteReflectionReflection("Celeste.Mod.JackalHelper.Entities.GrappleRefill", "sprite", "oneUse");

            //RegisterSpriteReflectionReflection("Celeste.Mod.JungleHelper.Entities.RemoteKevinRefill", "sprite", "oneUse");
        }

        public static void CheckAndReplaceSprite(Entity e)
        {
            Type ety = e.GetType();
            if (Registered.TryGetValue(e.GetType(), out var func))
            {
                foreach(var f in func)
                {
                    f(e);
                }
            }
            if(RegisteredRefl.TryGetValue(e.GetType().FullName,out func))
            {
                foreach(var f in func)
                {
                    f(e);
                }
            }
        }
        public static void ReplaceSprite(Entity e)
        {
            foreach (var c in e.Components.OfType<Sprite>())
            {
                ReplaceSprite(c);
            }
        }
        internal static CacheDictionary<(Type, string), Func<Entity, Sprite>> refler = new(x =>
        {
            return ReflectionHandler.GetGetter<Sprite>(x.Item1, x.Item2);
        });

        public static void ReplaceSpriteReflection(Entity e, params string[] s)
        {
            foreach (var u in s)
            {
                ReplaceSprite(refler[(e.GetType(), u)](e));
            }
        }
        public static void ReplaceSprite(Sprite sprite)
        {
            foreach (var anim in sprite.Animations.Values)
            {
                for (var i = 0; i < anim.Frames.Length; i++)
                {
                    ref var frame = ref anim.Frames[i];
                    frame = ImageRecolor.GetImage(frame);
                }
            }
            ReplaceImage(sprite);
        }
        public static void ReplaceImage(Entity e)
        {
            foreach (var c in e.Components.OfType<Image>())
            {
                ReplaceImage(c);
            }
        }
        internal static CacheDictionary<(Type, string), Func<Entity, Image>> refleri = new(x =>
        {
            return ReflectionHandler.GetGetter<Image>(x.Item1, x.Item2);
        });

        public static void ReplaceImageReflection(Entity e, params string[] s)
        {
            foreach (var u in s)
            {
                ReplaceImage(refleri[(e.GetType(), u)](e));
            }
        }
        public static void ReplaceImage(Image Image)
        {
            ref var frame = ref Image.Texture;
            frame = ImageRecolor.GetImage(frame);
        }

    }
}
