using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.BetterRefillGemsPlus
{
    public class BetterRefillGemsPlusModule : EverestModule
    {
        public static BetterRefillGemsPlusModule Instance { get; private set; }

        public override Type SettingsType => typeof(BetterRefillGemsPlusModuleSettings);
        public static BetterRefillGemsPlusModuleSettings Settings => (BetterRefillGemsPlusModuleSettings)Instance._Settings;

        public override Type SessionType => typeof(BetterRefillGemsPlusModuleSession);
        public static BetterRefillGemsPlusModuleSession Session => (BetterRefillGemsPlusModuleSession)Instance._Session;

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

        public override void Load()
        {
            // TODO: apply any hooks that should always be active
        }

        public override void Unload()
        {
            // TODO: unapply any hooks applied in Load()
        }
    }
}