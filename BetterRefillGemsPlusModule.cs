using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Cil;
using MonoMod.ModInterop;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Celeste.Mod.BetterRefillGemsPlus
{
    public class BetterRefillGemsPlusModule : EverestModule
    {
        public Effect Outline;
        public static BetterRefillGemsPlusModule Instance { get; private set; }
        public static object debug_slot_1;
        public static object debug_slot_2;
        public static object debug_slot_3;
        public static object debug_slot_4;
        public override Type SettingsType => typeof(BetterRefillGemsPlusModuleSettings);
        public static BetterRefillGemsPlusModuleSettings Settings => (BetterRefillGemsPlusModuleSettings)Instance._Settings;
        public static Atlas all = new();
        public override Type SessionType => typeof(BetterRefillGemsPlusModuleSession);
        public static BetterRefillGemsPlusModuleSession Session => (BetterRefillGemsPlusModuleSession)Instance._Session;
        public override void PrepareMapDataProcessors(MapDataFixup context)
        {
            base.PrepareMapDataProcessors(context);
            //context.Add(new PrepareImageMapDataProcessor());
        }
        public BetterRefillGemsPlusModule()
        {
            Instance = this;
#if DEBUG
            // debug builds use verbose logging
            Logger.SetLogLevel(nameof(BetterRefillGemsPlusModule), LogLevel.Verbose);
#else
            // release builds use info logging to reduce spam in log files
            Logger.SetLogLevel(nameof(BetterRefillGemsPlusModule), LogLevel.Info);
#endif
        }
        public override void LoadContent(bool firstLoad)
        {
            base.LoadContent(firstLoad);
            EntityImageHandler.LoadContent();

            Outline = new(Engine.Graphics.GraphicsDevice, Everest.Content.Get($"BetterRefillGemsPlus:/Effects/BetterRefillGemsPlus/shader.fxb").Data);
            Outline.CurrentTechnique = Outline.Techniques[true ? "InnerOutline" : "OuterOutline"];
        }


        public override void Load()
        {
            // TODO: apply any hooks that should always be active
            On.Monocle.Entity.Awake += Entity_Awake;
            //IL.Monocle.VirtualTexture.Load += VirtualTexture_Load;
            var load = typeof(VirtualTexture).GetProperty("LoadImmediately", ReflectionHandler.bf)!.GetMethod;
            if (load != null)
            {
                //VTex_LoadImm = new Hook(load, (Func<VirtualTexture, bool> orig, VirtualTexture self) =>
                //{
                //    var r = orig(self);
                //    if (GetProcess())
                //    {
                //        return true;
                //    }
                //    return r;
                //});
            }
            EntityImageHandler.Load();
            typeof(Interop).ModInterop();
        }
        Hook? VTex_LoadImm;
        //should be safe if it's applied for multiple times.
        private void VirtualTexture_Load(ILContext il)
        { 
            var ic = new ILCursor(il);
            if (ic.TryGotoNext(MoveType.After,
                i => i.MatchLdarg(0),
                i => i.MatchCallOrCallvirt(typeof(VirtualTexture), "get_LoadImmediately"),
                i => i.MatchBrfalse(out _)))
            {
                var label = ic.MarkLabel();
                ic.Index -= 3;
                ic.Emit(Mono.Cecil.Cil.OpCodes.Call, typeof(BetterRefillGemsPlusModule).GetMethod(nameof(GetProcess), BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!);
                //ic.Emit(Mono.Cecil.Cil.OpCodes.Ldsfld, typeof(FontCustomizerModule).GetField(nameof(loadimmediately), BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
                ic.Emit(Mono.Cecil.Cil.OpCodes.Brtrue, label);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool GetProcess()
        {
            //return loadimmediately.ContainsKey(Environment.CurrentManagedThreadId);
            return false;
        }
        //internal static ConcurrentDictionary<int, object?> loadimmediately = [];

        private void Entity_Awake(On.Monocle.Entity.orig_Awake orig, Entity self, Scene scene)
        {
            orig(self, scene);
            EntityImageHandler.CheckAndReplaceSprite(self);
        }

        public override void Unload()
        {
            On.Monocle.Entity.Awake -= Entity_Awake;
            //IL.Monocle.VirtualTexture.Load -= VirtualTexture_Load;
            VTex_LoadImm?.Dispose();
            // TODO: unapply any hooks applied in Load()
        }
    }
}