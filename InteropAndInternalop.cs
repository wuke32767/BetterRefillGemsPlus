using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.BetterRefillGemsPlus
{
    [MonoMod.ModInterop.ModExportName("BetterRefillGemsPlus.MarkEntity")]
    public static class Interop
    {
        /// <summary>
        /// Register your entity so their sprite will be replaced when awaking.
        /// Do not share entities' sprite. CreateClone or build a new sprite for each entity.
        /// </summary>
        /// <param name="entity">typeof your entity.</param>
        /// <param name="spriteGetter">a getter that returns the sprite of the given entity.</param>
        /// <param name="oneuseGetter">
        /// a getter that returns the oneuse attr of the given entity.
        /// in other words, if this func returns true, the sprite of the entity will be replaced.
        /// </param>
        public static void RegisterSprite(Type entity, Func<Entity, Sprite> spriteGetter, Func<Entity, bool> oneuseGetter)
        {
            EntityImageHandler.Registered[entity].Add(e =>
            {
                if (oneuseGetter(e))
                {
                    EntityImageHandler.ReplaceSprite(spriteGetter(e));
                }
            });
        }
        /// <summary>
        /// Register your entity so their image will be replaced when awaking.
        /// Do not share entities' image. create a new image for each entity.
        /// </summary>
        /// <param name="entity">typeof your entity.</param>
        /// <param name="spriteGetter">a getter that returns the sprite of the given entity.</param>
        /// <param name="oneuseGetter">
        /// a getter that returns the oneuse attr of the given entity.
        /// in other words, if this func returns true, the sprite of the entity will be replaced.
        /// </param>
        public static void RegisterImage(Type entity, Func<Entity, Image> spriteGetter, Func<Entity, bool> oneuseGetter)
        {
            EntityImageHandler.Registered[entity].Add(e =>
            {
                if (oneuseGetter(e))
                {
                    EntityImageHandler.ReplaceImage(spriteGetter(e));
                }
            });
        }
        /// <summary>
        /// better not to use this. I don't think you will need reflection.
        /// </summary>
        /// <param name="entity">holds a value of one of (Type, string). type and fullName are functionally the same, one is Type and the other is Type from reflection.</param>
        /// <param name="as">same as entity</param>
        public static void RegisterAs((Type? type, string? fullName) entity, (Type? type, string? fullName) @as)
        {
            if (entity.type is null && entity.fullName is null)
            {
                throw new ArgumentNullException(nameof(entity), $"One of type and fullName should not be null.");
            }
            if (@as.type is null && @as.fullName is null)
            {
                throw new ArgumentNullException(nameof(@as), $"One of type and fullName should not be null.");
            }
            List<Action<Entity>> to;
            if (@as.type is null)
            {
                if (EntityImageHandler.RegisteredRefl.TryGetValue(@as.fullName!, out var list))
                {
                    to = list;
                }
                else
                {
                    var r = EntityImageHandler.Registered
                        .Concat(EntityImageHandler.RegisteredSafe)
                        .FirstOrDefault(x => x.Key.FullName == @as.fullName);
                    if (r.Key is not null)
                    {
                        to = r.Value;
                    }
                    else
                    {
                        Logger.Log(LogLevel.Error, nameof(BetterRefillGemsPlus), $"No type called {@as.fullName} was registered.");
                        return;
                    }
                }
            }
            else
            {
                if (EntityImageHandler.RegisteredRefl.TryGetValue(@as.type.FullName!, out var list)
                    || EntityImageHandler.Registered.TryGetValue(@as.type, out list)
                    || EntityImageHandler.RegisteredSafe.TryGetValue(@as.type, out list))
                {
                    to = list;
                }
                else
                {
                    Logger.Log(LogLevel.Error, nameof(BetterRefillGemsPlus), $"No type called {@as.type.FullName} was registered.");
                    return;
                }
            }

            // better to create a clone, or it will be annoying.
            if (entity.type is null)
            {
                EntityImageHandler.RegisteredRefl[entity.fullName!] = new(to);
            }
            else
            {
                EntityImageHandler.Registered[entity.type] = new(to);
            }

        }
        /// <summary>
        /// If your entity is derived from Refill and re-used its sprite and oneUse,
        /// you can use this.
        /// for example, to support ColorfulRefill [DJMapHelper]:
        /// <code>RegisterAs(typeof(ColorfulRefill), typeof(Refill));</code>
        /// 
        /// something was changed. you should register target type before register smth as it.
        /// and "as" entity can't be registered by reflection.
        /// </summary>
        /// <param name="entity">holds a value of one of (Type, string). type and fullName are functionally the same, one is Type and the other is Type from reflection.</param>
        /// <param name="as">same as entity</param>
        public static void RegisterAs(Type entity, Type @as)
        {
            List<Action<Entity>> to;
            if (EntityImageHandler.Registered.TryGetValue(@as, out var list)
                || EntityImageHandler.RegisteredSafe.TryGetValue(@as, out list))
            {
                to = list;
            }
            else
            {
                Logger.Log(LogLevel.Error, nameof(BetterRefillGemsPlus), $"No type called {@as.FullName} was registered.");
                return;
            }

            // better to create a clone, or it will be annoying.
            EntityImageHandler.Registered[entity] = new(to);
        }
    }
    internal static class Internalop
    {
        public static void RegisterSpriteReflection(Type entity, string spriteName, string oneuseName)
        {
            var spriteGetter = ReflectionHandler.GetGetter<Sprite>(entity, spriteName);
            var oneuseGetter = ReflectionHandler.GetGetter<bool>(entity, oneuseName);
            if (spriteGetter is null || oneuseGetter is null)
            {
                Logger.Log(nameof(BetterRefillGemsPlus), $"failed when register {entity}");
                return;
            }
            EntityImageHandler.Registered[entity].Add(e =>
            {
                var sp = spriteGetter(e);//try if it is broken
                if (oneuseGetter(e))
                {
                    EntityImageHandler.ReplaceSprite(sp);
                }
            });
        }
        public static void RegisterSpriteReflectionReflection(string entityFullname, string spriteName, string oneuseName)
        {
            EntityImageHandler.RegisteredRefl[entityFullname].Add(e =>
            {
                if (ReflectionHandler.GetGetter<bool>(e.GetType(), oneuseName)!(e))
                {
                    EntityImageHandler.ReplaceSpriteReflection(e, spriteName);
                }
            });
        }
        public static void TryAutoRegister(string entityID)
        {
            EntityImageHandler.TryRegisterer[entityID] =
                ([
#nullable disable //should throw if null
                    e=> (e as Refill).oneUse,
                    e=> (ReflectionHandler.GetGetter<bool>(e.GetType(),"oneUse"))(e),
                    e=> (ReflectionHandler.GetGetter<bool>(e.GetType(),"OneUse"))(e),
                    e=> (ReflectionHandler.GetGetter<bool>(e.GetType(),"oneuse"))(e),
                    e=> (ReflectionHandler.GetGetter<bool>(e.GetType(),"_oneUse"))(e),
                    e=> (ReflectionHandler.GetGetter<bool>(e.GetType(),"_OneUse"))(e),
                    e=> (ReflectionHandler.GetGetter<bool>(e.GetType(),"_oneuse"))(e),
                ],
                [
                    e=> (e as Refill).sprite,
                    e=> e.Components.OfType<Sprite>().Single(s => s.Animations.ContainsKey("idle")), //but what if it is different for same type?
                    e=> ReflectionHandler.GetGetter<Sprite>(e.GetType(),"sprite")(e),
                    e=> ReflectionHandler.GetGetter<Sprite>(e.GetType(),"Sprite")(e),
                    e=> ReflectionHandler.GetGetter<Sprite>(e.GetType(),"_sprite")(e),
                    e=> ReflectionHandler.GetGetter<Sprite>(e.GetType(),"_Sprite")(e),
                ]);
#nullable restore
        }
        public static void RegisterImageReflectionReflection(string entityFullname, string imageName, string oneuseName)
        {
            EntityImageHandler.RegisteredRefl[entityFullname].Add(e =>
            {
                if (ReflectionHandler.GetGetter<bool>(e.GetType(),oneuseName)!(e))
                {
                    EntityImageHandler.ReplaceImageReflection(e, imageName);
                }
            });
        }
        public static void RegisterImageReflection(Type entity, string imageName, string oneuseName)
        {
            var imageGetter = ReflectionHandler.GetGetter<Image>(entity, imageName);
            var oneuseGetter = ReflectionHandler.GetGetter<bool>(entity, oneuseName);
            if (imageGetter is null || oneuseGetter is null)
            {
                Logger.Log(nameof(BetterRefillGemsPlus), $"failed when register {entity}");
                return;
            }

            EntityImageHandler.Registered[entity].Add(e =>
            {
                var im = imageGetter(e);
                if (oneuseGetter(e))
                {
                    EntityImageHandler.ReplaceImage(im);
                }
            });
        }

    }
}
