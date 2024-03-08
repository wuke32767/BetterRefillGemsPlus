using Celeste.Mod.Helpers.LegacyMonoMod;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Celeste.Mod.BetterRefillGemsPlus
{
    public static class ReflectionHandler
    {
        public const BindingFlags bf = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        public static Func<Entity, Value>? GetGetter<Value>(Type type, string name)
        {
            var f = type.GetField(name, bf);
            if (f is not null)
            {
                return f.GetGetter<Value>();
            }
            var p = type.GetProperty(name, bf);
            if (p is not null)
            {
                return p.GetGetter<Value>();
            }
            return null;
        }

        //almost as fast as proprty that saved in a lambda.
        public static Func<Entity, Value> GetGetter<Value>(this PropertyInfo property)
        {
            DynamicMethod getdyn = new("", typeof(Value), [typeof(Entity)]);
            var get = property.GetMethod;//.CreateDelegate(typeof(Func<,>).MakeGenericType(property.DeclaringType, typeof(Value)));
            var ic = getdyn.GetILGenerator();
            ic.Emit(OpCodes.Ldarg_0);
            ic.Emit(OpCodes.Call, get);
            ic.Emit(OpCodes.Ret);
            return getdyn.CreateDelegate<Func<Entity, Value>>();
        }
        //as fast as lambda.
        public static Func<Entity, Value> GetGetter<Value>(this FieldInfo field)
        {
            DynamicMethod get = new("", typeof(Value), [typeof(Entity)]);
            var ic = get.GetILGenerator();
            ic.Emit(OpCodes.Ldarg_0);
            ic.Emit(OpCodes.Ldfld, field);
            ic.Emit(OpCodes.Ret);

            return get.CreateDelegate<Func<Entity, Value>>();
        }
    }
}
