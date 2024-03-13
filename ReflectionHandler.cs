using Celeste.Mod.Helpers.LegacyMonoMod;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Celeste.Mod.BetterRefillGemsPlus
{
    internal static class ReflectionHandler
    {
        public const BindingFlags bf = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        static readonly DefaultDictionary<Type, Dictionary<string, Delegate>> Getters = new(x => []);
        internal static Func<Entity, Value>? GetGetter<Value>(Type type, string name)
        {
            static Func<Entity, Value>? _GetGetter(Type type, string name)
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
            if (Getters.TryGetValue(type, out var func_type))
            {
                //should have only 2 (or 1) results so enumerate directly might be faster
                //do not break this.
                //if you need it, copy it to your project.

                //and Image and Sprite should not appear at the same time.
                //if (func_type.Values.FirstOrDefault(x=>x.GetType()==typeof(Func<Entity, Value>)) is Func<Entity, Value> ret)
                if (func_type.Values.OfType<Func<Entity, Value>>().FirstOrDefault() is Func<Entity, Value> ret)
                {
                    return ret;
                }
                //if (func_type.TryGetValue(name, out var func_or)
                //&& func_or is Func<Entity, Value> func)
                //{
                //    return func;
                //}
            }
            if (_GetGetter(type, name) is Func<Entity, Value> func2)
            {
                Getters[type][name] = func2;
                return func2;
            }
            return null;
        }

        //almost as fast as proprty that saved in a lambda.
        public static Func<Entity, Value>? GetGetter<Value>(this PropertyInfo property)
        {
            if (property.GetMethod is null)
            {
                return null;
            }
            DynamicMethod getdyn = new("", typeof(Value), [typeof(Entity)]);
            var get = property.GetMethod!;//.CreateDelegate(typeof(Func<,>).MakeGenericType(property.DeclaringType, typeof(Value)));
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
