using Celeste.Mod.StrawberryTool.Module;
using Celeste.Mod.StrawberryTool.UI;

namespace Celeste.Mod.StrawberryTool.Feature.Detector {
    public class OuiDetectorSubmenu : AbstractSubmenu {
        public OuiDetectorSubmenu() : base(DialogIds.DetectorOptions, DialogIds.DetectorOptions) { }
        private static StrawberryToolSettings Settings => StrawberryToolModule.Settings;

        protected override void addOptionsToMenu(TextMenu menu, bool inGame) {
            menu.Add(new TextMenu.OnOff(DialogIds.DetectorEnabled.DialogClean(), Settings.DetectorEnabled)
                .Change(value => Settings.DetectorEnabled = value));

            menu.Add(new TextMenu.Slider(DialogIds.PointerOpacity.DialogClean(),
                value => (value / 10f).ToString("0.0"), 1, 10,
                Settings.PointerOpacity) {
                OnValueChange = value => Settings.PointerOpacity = value
            });
            
            menu.Add(new TextMenu.OnOff(DialogIds.ShowIcon.DialogClean(), Settings.ShowIcon)
                .Change( value => Settings.ShowIcon = value));
            
            menu.Add(new TextMenu.Slider(DialogIds.DetectorRange.DialogClean(),
                value => (value * 100).ToString(), 1, 99,
                Settings.DetectorRange) {
                OnValueChange = value => Settings.DetectorRange = value
            });

            menu.Add(new TextMenu.OnOff(DialogIds.DetectCurrentRoom.DialogClean(), Settings.DetectCurrentRoom)
                .Change(value => Settings.DetectCurrentRoom = value));
            
            menu.Add(new TextMenu.OnOff(DialogIds.DetectCollected.DialogClean(), Settings.DetectCollected)
                .Change(value => Settings.DetectCollected = value));
        }

        protected override void gotoMenu(Overworld overworld) {
            Overworld.Goto<OuiDetectorSubmenu>();
        }
    }
}