using System.Configuration;

namespace Celeste.Mod.StrawberryTool {
    [SettingName(DialogIds.StrawberryTool)]
    public class StrawberryToolSettings : EverestModuleSettings {
        [SettingName(DialogIds.Enabled)] public bool Enabled { get; set; } = true;

        [SettingName(DialogIds.TransparentRadius)]
        [SettingRange(1, 999)]
        public int TransparentRadius { get; set; } = 10;

        [SettingName(DialogIds.TranslucentRadius)]
        [SettingRange(1, 999)]
        public int TranslucentRadius { get; set; } = 60;
    }
}