using System;
using Celeste.Mod.StrawberryTool.Feature.Detector;
using Celeste.Mod.StrawberryTool.Feature.Translucent;

namespace Celeste.Mod.StrawberryTool.Module {
    public class StrawberryToolModule : EverestModule {
        public static StrawberryToolModule Instance;

        public StrawberryToolModule() {
            Instance = this;
        }

        public override Type SettingsType => typeof(StrawberryToolSettings);
        public static StrawberryToolSettings Settings => (StrawberryToolSettings) Instance._Settings;

        public override void Load() {
            TranslucentUtils.Load();
            DetectorUtils.Load();
        }

        public override void Unload() {
            TranslucentUtils.Unload();
            DetectorUtils.Unload();
        }
    }
}