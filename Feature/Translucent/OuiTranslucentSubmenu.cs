using Celeste.Mod.StrawberryTool.Module;
using Celeste.Mod.StrawberryTool.UI;

namespace Celeste.Mod.StrawberryTool.Feature.Translucent {
    public class OuiTranslucentSubmenu : AbstractSubmenu {
        public OuiTranslucentSubmenu() : base(DialogIds.TranslucentOptions, DialogIds.TranslucentOptions) { }
        private static StrawberryToolSettings Settings => StrawberryToolModule.Settings;

        protected override void addOptionsToMenu(TextMenu menu, bool inGame) {
            menu.Add(new TextMenu.OnOff(DialogIds.TranslucentEnabled.DialogClean(), Settings.TranslucentEnabled).Change(
                value => Settings.TranslucentEnabled = value));

            TextMenu.Slider item = new TextMenu.Slider(DialogIds.TransparentRadius.DialogClean(),
                value => value.ToString(), 1, 999,
                Settings.TransparentRadius) {
                OnValueChange = value => Settings.TransparentRadius = value
            };
            menu.Add(item);
            item.AddDescription(menu, DialogIds.TransparentRadiusDescription.DialogClean());

            item = new TextMenu.Slider(DialogIds.TranslucentRadius.DialogClean(), value => value.ToString(), 1, 999,
                Settings.TranslucentRadius) {
                OnValueChange = value => Settings.TranslucentRadius = value
            };
            menu.Add(item);
            item.AddDescription(menu, DialogIds.TranslucentRadiusDescription.DialogClean());
        }

        protected override void gotoMenu(Overworld overworld) {
            Overworld.Goto<OuiTranslucentSubmenu>();
        }
    }
}