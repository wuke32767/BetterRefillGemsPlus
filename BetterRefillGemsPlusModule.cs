using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.ModInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Celeste.Mod.BetterRefillGemsPlus
{
    public class BetterRefillGemsPlusModule : EverestModule
    {
        public static BetterRefillGemsPlusModule Instance { get; private set; }

        public override Type SettingsType => typeof(BetterRefillGemsPlusModuleSettings);
        public static BetterRefillGemsPlusModuleSettings Settings => (BetterRefillGemsPlusModuleSettings)Instance._Settings;
        public static Atlas all=new();
        public override Type SessionType => typeof(BetterRefillGemsPlusModuleSession);
        public static BetterRefillGemsPlusModuleSession Session => (BetterRefillGemsPlusModuleSession)Instance._Session;
        public override void PrepareMapDataProcessors(MapDataFixup context)
        {
            base.PrepareMapDataProcessors(context);
            context.Add(new PrepareImageMapDataProcessor());
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
        }


        public override void Load()
        {
            // TODO: apply any hooks that should always be active
            On.Monocle.Entity.Awake += Entity_Awake;
            EntityImageHandler.Load();
            typeof(InteropAndInternalop).ModInterop();
        }

        private void Entity_Awake(On.Monocle.Entity.orig_Awake orig, Entity self, Scene scene)
        {
            orig(self, scene);
            EntityImageHandler.CheckAndReplaceSprite(self);
        }

        public override void Unload()
        {
            On.Monocle.Entity.Awake -= Entity_Awake;

            // TODO: unapply any hooks applied in Load()
        }
    }
}