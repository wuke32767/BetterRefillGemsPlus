﻿using Celeste.Mod.Entities;
using Monocle;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Celeste.Mod.BetterRefillGemsPlus.Internalop;
using static Celeste.Mod.BetterRefillGemsPlus.Interop;

namespace Celeste.Mod.BetterRefillGemsPlus
{
    static class EntityImageHandler
    {
        //public static Dictionary<Type, Func<Entity, bool>> oneuser = [];
        public static DefaultDictionary<Type, List<Action<Entity>>> Registered = new(x => []);
        public static DefaultDictionary<Type, List<Action<Entity>>> RegisteredSafe = new(x => []);
        public static DefaultDictionary<string, List<Action<Entity>>> RegisteredRefl = new(x => []);
        public static HashSet<string> TryRegisterer = [];
        internal static void Load()
        {
            RegisterSprite(typeof(Refill), e => (e as Refill)!.sprite, e => (e as Refill)!.oneUse);
        }
        internal static void LoadContent()
        {
            TryAutoRegister("DJMapHelper/colorfulRefill");
            TryAutoRegister("MaxHelpingHand/CustomizableRefill");

            TryAutoRegister("CommunalHelper/DreamRefill");
            TryAutoRegister("CommunalHelper/SeekerDashRefill");
            TryAutoRegister("CommunalHelper/SJ/ExpiringDashRefill");

            TryAutoRegister("ExtendedVariantMode/ExtraJumpRefill");
            //TryAutoRegister("ExtendedVariantMode/RecoverJumpRefill");

            TryAutoRegister("Anonhelper/CoreRefill");
            TryAutoRegister("Anonhelper/CloudRefill");
            TryAutoRegister("Anonhelper/BoosterRefill");
            TryAutoRegister("Anonhelper/FeatherRefill");
            TryAutoRegister("Anonhelper/JellyRefill");
            TryAutoRegister("Anonhelper/SuperDashRefill");

            TryAutoRegister("ArphimigonHelper/RefillRefill");
            TryAutoRegister("ArphimigonHelper/DifficultRefill");

            TryAutoRegister("CommunalHelper/ShieldedRefill");

            TryAutoRegister("FrostHelper/PlusOneRefill");

            TryAutoRegister("JackalHelper/CryoRefill");
            TryAutoRegister("JackalHelper/TracerRefill");

            TryAutoRegister("ReverseHelper/HoldableRefill");

            TryAutoRegister("MoreDasheline/SpecialRefill");

            TryAutoRegister("VivHelper/RedDashRefill");
            TryAutoRegister("VivHelper/WarpDashRefill");

            TryAutoRegister("BounceHelper/BounceRefill");

            TryAutoRegister("batteries/power_refill");

            TryAutoRegister("CherryHelper/ShadowDashRefill");

            TryAutoRegister("SaladimHelper/FlagRefill");
            TryAutoRegister("SaladimHelper/BitsMomentumRefill");

            TryAutoRegister("XaphanHelper/TimerRefill");

            //no, just because CreateClone() is not deepclone. (at least not deep enough)
            TryAutoRegister("ChronoHelper/ShatterRefill");

            TryAutoRegister("GravityHelper/GravityRefill");

            TryAutoRegister("JackalHelper/StarRefill");
            TryAutoRegister("JackalHelper/GrappleRefill");

            TryAutoRegister("JungleHelper/RemoteKevinRefill");
        }
        public static HashSet<Type> CheckedType = [];

        static Action<Entity>? GetCorrectModifier(Type t, Entity sample)
        {
            [DebuggerNonUserCode]
            [DebuggerHidden]
            Func<Entity, T>? test<T>(Func<Entity, T>? f)
            {
                try
                {
                    if (f is not null)
                    {
                        var a = f.Invoke(sample);
                        if (a is not null)
                        {
                            return f;
                        }
                    }
                }
                catch (Exception) { }
                return null;
            }
            Func<Entity, Func<Entity, bool>?>[] oneuses =
                [
                    static e => (e is Refill) ? e => (e as Refill)!.oneUse : null,
                    static e => ReflectionHandler.GetGetter<bool>(e.GetType(), "oneUse"),
                    static e => ReflectionHandler.GetGetter<bool>(e.GetType(), "OneUse"),
                    static e => ReflectionHandler.GetGetter<bool>(e.GetType(), "oneuse"),
                    static e => ReflectionHandler.GetGetter<bool>(e.GetType(), "_oneUse"),
                    static e => ReflectionHandler.GetGetter<bool>(e.GetType(), "_OneUse"),
                    static e => ReflectionHandler.GetGetter<bool>(e.GetType(), "_oneuse"),
                ];
            Func<Entity, bool>? first = null;
            foreach (var v in oneuses)
            {
                first = test(v(sample));
                if (first is not null)
                {
                    break;
                }
            }
            Func<Entity, Func<Entity, Sprite>?>[] sprites =
                [
                    static e => (e is Refill)?e => (e as Refill)!.sprite:null,
                    static e => e.OfType<Sprite>().Count(s => s.Animations.ContainsKey("idle")) switch
                        {
                            1 => e => e.OfType<Sprite>().First(s => s.Animations.ContainsKey("idle")), //but what if it is different for same type?
                            _ => null,
                        },
                    static e => ReflectionHandler.GetGetter<Sprite>(e.GetType(), "sprite"),
                    static e => ReflectionHandler.GetGetter<Sprite>(e.GetType(), "Sprite"),
                    static e => ReflectionHandler.GetGetter<Sprite>(e.GetType(), "_sprite"),
                    static e => ReflectionHandler.GetGetter<Sprite>(e.GetType(), "_Sprite"),
                ];
            Func<Entity, Sprite>? second = null;
            foreach (var v in sprites)
            {
                second = test(v(sample));
                if (second is not null)
                {
                    break;
                }
            }
            if (second is null || first is null)
            {
                return null;
            }
            return e =>
            {
                if (first(e))
                {
                    ReplaceSprite(second(e));
                }
            };
        }
        public static void CheckAndReplaceSprite(Entity e)
        {
            Type ety = e.GetType();
            if (RegisteredSafe.TryGetValue(ety, out var func))
            {
                foreach (var f in func)
                {
                    try
                    {
                        f(e);
                        return;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, nameof(BetterRefillGemsPlus), $"Exception was thrown when trying to replace {ety.FullName} 's sprite. Detail message: \n{ex.Message}\n{ex.StackTrace}");
                    }
                }
            }
            if (!CheckedType.Add(ety))
            {
                return;
            }
            bool flag = Registered.Remove(ety, out func);
            //still try to clear other matched result
            flag = RegisteredRefl.Remove(ety.FullName!, out var func2) || flag;

            if (flag)
            {
                func ??= func2;
                RegisteredSafe[ety] = func!.Where(f =>
                {
                    try
                    {
                        f(e);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, nameof(BetterRefillGemsPlus), $"Exception was thrown when trying to replace {ety.FullName} 's sprite. Detail message: \n{ex.Message}\n{ex.StackTrace}");
                        return false;
                    }
                }).ToList();
            }
            if (TryRegisterer.Any())
            {
                //it looks scary so must be slow
                //although TryRegisterer.Any() is impossible to be false
                var EntityID =
                    (Attribute.GetCustomAttribute(ety, typeof(CustomEntityAttribute)) as CustomEntityAttribute)?
                    .IDs
                    .Select(x =>
                        x.Split('=')
                        .Select(x => x.Trim())
                        .FirstOrDefault());
                if (EntityID is not null)
                {
                    foreach (var v in EntityID)
                    {
                        if (TryRegisterer.Remove(v))
                        {
                            if (!flag)
                            {
                                try
                                {
                                    var mod = GetCorrectModifier(e.GetType(), e);
                                    if (mod is not null)
                                    {
                                        RegisteredSafe[ety].Add(mod);
                                        mod(e);
                                        break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logger.Log(LogLevel.Error, nameof(BetterRefillGemsPlus), $"Exception was thrown when trying to replace {ety.FullName} 's sprite. Detail message: \n{ex.Message}\n{ex.StackTrace}");
                                }
                            }
                        }
                    }
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
        //internal static CacheDictionary<(Type, string), Func<Entity, Sprite>> refler = new(x =>
        //{
        //    return ReflectionHandler.GetGetter<Sprite>(x.Item1, x.Item2);
        //});

        public static void ReplaceSpriteReflection(Entity e, params string[] s)
        {
            foreach (var u in s)
            {
                ReplaceSprite(ReflectionHandler.GetGetter<Sprite>(e.GetType(), u)!(e));
                //should throw nullptr exception
            }
        }
        public static void ReplaceSprite(Sprite sprite)
        {
            //this would not be cloned by SpriteBank.Create
            sprite.animations =
            sprite.animations.Select(x => (x.Key, Value: new Sprite.Animation()
            {
                Delay = x.Value.Delay,
                Frames = [.. x.Value.Frames],
                Goto = x.Value.Goto,
            })).ToDictionary(x => x.Key, x => x.Value);
            sprite.currentAnimation = sprite.animations[sprite.CurrentAnimationID];
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
            foreach (var c in e.Components.OfType<Image>())//sprite included however
            {
                ReplaceImage(c);
            }
        }
        //internal static DefaultDictionary<(Type, string), Func<Entity, Image>> refleri = new(x =>
        //{
        //    return ReflectionHandler.GetGetter<Image>(x.Item1, x.Item2);
        //});

        public static void ReplaceImageReflection(Entity e, params string[] s)
        {
            foreach (var u in s)
            {
                ReplaceImage(ReflectionHandler.GetGetter<Image>(e.GetType(), u)!(e));
            }
        }
        public static void ReplaceImage(Image Image)
        {
            ref var frame = ref Image.Texture;
            frame = ImageRecolor.GetImage(frame);
        }

    }
}
