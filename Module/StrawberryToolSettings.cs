using Celeste.Mod.StrawberryTool.Feature.Detector;
using Celeste.Mod.StrawberryTool.Feature.Translucent;
using Celeste.Mod.StrawberryTool.UI;

namespace Celeste.Mod.StrawberryTool.Module {
    [SettingName(DialogIds.StrawberryTool)]
    public class StrawberryToolSettings : EverestModuleSettings {
        [SettingIgnore] public bool Enabled { get; set; } = true;

        public void CreateEnabledEntry(TextMenu menu, bool inGame) {
            TextMenu.OnOff button = new TextMenu.OnOff(DialogIds.Enabled.DialogClean(), Enabled) {
                OnValueChange = value => {
                    Enabled = value;

                    bool mineItem = false;
                    int mindItemsCount = 2;
                    foreach (TextMenu.Item item in menu.Items) {
                        if (item == menu.Current && !mineItem) {
                            mineItem = true;
                            continue;
                        }

                        if (!mineItem || mindItemsCount <= 0) {
                            continue;
                        }

                        mindItemsCount--;
                        item.Visible = value;
                    }
                }
            };

            menu.Add(button);
        }

        private void CreateSubmenu<T>(TextMenu menu, bool inGame) where T : AbstractSubmenu {
            TextMenuButtonExt submenu = AbstractSubmenu.BuildOpenMenuButton<T>(menu, inGame);

            if (submenu != null) {
                submenu.Visible = Enabled;
                menu.Add(submenu);
            }
        }

        #region Translucent

        [SettingIgnore] public bool TranslucentEnabled { get; set; } = true;
        [SettingIgnore] public int TransparentRadius { get; set; } = 10;
        [SettingIgnore] public int TranslucentRadius { get; set; } = 60;

        public string Translucent { get; set; } = string.Empty;

        public void CreateTranslucentEntry(TextMenu menu, bool inGame) {
            CreateSubmenu<OuiTranslucentSubmenu>(menu, inGame);
        }

        #endregion


        #region Detector

        [SettingIgnore] public bool DetectorEnabled { get; set; } = true;
        [SettingIgnore] public int DetectorOpacity { get; set; } = 10;
        [SettingIgnore] public bool OpacityGradient { get; set; } = true;
        [SettingIgnore] public bool ShowPointer { get; set; } = true;
        [SettingIgnore] public bool ShowIcon { get; set; } = true;
        [SettingIgnore] public bool ShowIconAtScreenEdge { get; set; } = true;
        [SettingIgnore] public int DetectorRange { get; set; } = 6;
        [SettingIgnore] public int MaxPointers { get; set; } = 1;
        [SettingIgnore] public bool DetectCurrentRoom { get; set; } = true;
        [SettingIgnore] public bool DetectCollected { get; set; } = true;

        public string Detector { get; set; } = string.Empty;

        public void CreateDetectorEntry(TextMenu menu, bool inGame) {
            CreateSubmenu<OuiDetectorSubmenu>(menu, inGame);
        }

        #endregion
    }
}