using System.Collections;
using Celeste.Mod.StrawberryTool.Module;
using Celeste.Mod.UI;
using Microsoft.Xna.Framework;
using Monocle;

/*
 * Copy from max480 https://github.com/max4805/Everest-ExtendedVariants
 */
namespace Celeste.Mod.StrawberryTool.UI {
    // A generic submenu kind of thing. This displays some options on a screen, and goes back to another menu when pressing Back.
    // Heavily based off the OuiModOptions from Everest: https://github.com/EverestAPI/Everest/blob/master/Celeste.Mod.mm/Mod/UI/OuiModOptions.cs
    public abstract class AbstractSubmenu : Oui, OuiModOptions.ISubmenu {
        private const float onScreenX = 960f;
        private const float offScreenX = 2880f;
        private readonly string buttonName;

        private readonly string menuName;

        private float alpha;

        private TextMenu menu;

        /// <summary>
        ///     Builds a submenu. The names expected here are dialog IDs.
        /// </summary>
        /// <param name="menuName">The title that will be displayed on top of the menu</param>
        /// <param name="buttonName">The name of the button that will open the menu from the parent submenu</param>
        public AbstractSubmenu(string menuName, string buttonName) {
            this.menuName = menuName;
            this.buttonName = buttonName;
        }

        /// <summary>
        ///     Adds all the submenu options to the TextMenu given in parameter.
        /// </summary>
        protected abstract void addOptionsToMenu(TextMenu menu, bool inGame);

        /// <summary>
        ///     Gives the title that will be displayed on top of the menu.
        /// </summary>
        protected virtual string getMenuName() {
            return Dialog.Clean(menuName);
        }

        /// <summary>
        ///     Gives the name of the button that will open the menu from the parent submenu.
        /// </summary>
        protected virtual string getButtonName() {
            return Dialog.Clean(buttonName);
        }

        /// <summary>
        ///     Builds the text menu, that can be either inserted into the pause menu, or added to the dedicated Oui screen.
        /// </summary>
        private TextMenu buildMenu(bool inGame) {
            TextMenu menu = new TextMenu();

            menu.Add(new TextMenu.Header(getMenuName()));
            menu.Add(new TextMenu.SubHeader(string.Empty));
            addOptionsToMenu(menu, inGame);

            return menu;
        }

        // === some Oui plumbing

        public override IEnumerator Enter(Oui from) {
            menu = buildMenu(false);
            Scene.Add(menu);

            menu.Visible = Visible = true;
            menu.Focused = false;

            for (float p = 0f; p < 1f; p += Engine.DeltaTime * 4f) {
                menu.X = offScreenX + -1920f * Ease.CubeOut(p);
                alpha = Ease.CubeOut(p);
                yield return null;
            }

            menu.Focused = true;
        }

        public override IEnumerator Leave(Oui next) {
            Audio.Play(SFX.ui_main_whoosh_large_out);
            menu.Focused = false;

            for (float p = 0f; p < 1f; p += Engine.DeltaTime * 4f) {
                menu.X = onScreenX + 1920f * Ease.CubeIn(p);
                alpha = 1f - Ease.CubeIn(p);
                yield return null;
            }

            menu.Visible = Visible = false;
            menu.RemoveSelf();
            menu = null;
        }

        public override void Update() {
            if (menu != null && menu.Focused && Selected && Input.MenuCancel.Pressed) {
                Audio.Play(SFX.ui_main_button_back);
                OuiModOptions.Instance.Overworld.Goto<OuiModOptions>();
            }

            base.Update();
        }

        public override void Render() {
            if (alpha > 0f) {
                Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * alpha * 0.4f);
            }

            base.Render();
        }

        // === / some Oui plumbing

        /// <summary>
        ///     Supposed to just contain "overworld.Goto<ChildType>()".
        /// </summary>
        protected abstract void gotoMenu(Overworld overworld);

        /// <summary>
        ///     Builds a button that opens the menu with specified type when hit.
        /// </summary>
        /// <param name="parentMenu">The parent's TextMenu</param>
        /// <param name="inGame">true if we are in the pause menu, false if we are in the overworld</param>
        /// <param name="backToParentMenu">Action that will be called to go back to the parent menu</param>
        /// <param name="parameters">some arbitrary parameters that can be used to build the menu</param>
        /// <returns>A button you can insert in another menu</returns>
        public static TextMenuButtonExt BuildOpenMenuButton<T>(TextMenu parentMenu, bool inGame)
            where T : AbstractSubmenu {
            return OuiModOptions.Instance.Overworld.GetUI<T>()?.buildOpenMenuButton(parentMenu, inGame);
        }

        /// <summary>
        ///     Method getting called on the Oui instance when the method just above is called.
        /// </summary>
        private TextMenuButtonExt buildOpenMenuButton(TextMenu parentMenu, bool inGame) {
            if (inGame) {
                Level level = Engine.Scene as Level;

                // this is how it works in-game
                return (TextMenuButtonExt) new TextMenuButtonExt(getButtonName()).Pressed(() => {
                    // close the parent menu
                    parentMenu.RemoveSelf();

                    // create our menu and prepare it
                    TextMenu thisMenu = buildMenu(true);

                    // notify the pause menu that we aren't in the main menu anymore (hides the strawberry tracker)
                    bool comesFromPauseMainMenu = level.PauseMainMenuOpen;
                    level.PauseMainMenuOpen = false;

                    thisMenu.OnESC = thisMenu.OnCancel = () => {
                        // close this menu
                        Audio.Play(SFX.ui_main_button_back);

                        StrawberryToolModule.Instance.SaveSettings();
                        thisMenu.Close();

                        // and open the parent menu back (this should work, right? we only removed it from the scene earlier, but it still exists and is intact)
                        // "what could possibly go wrong?" ~ famous last words
                        level.Add(parentMenu);

                        // restore the pause "main menu" flag to make strawberry tracker appear again if required.
                        level.PauseMainMenuOpen = comesFromPauseMainMenu;
                    };

                    thisMenu.OnPause = () => {
                        // we're unpausing, so close that menu, and save the mod Settings because the Mod Options menu won't do that for us
                        Audio.Play(SFX.ui_main_button_back);

                        StrawberryToolModule.Instance.SaveSettings();
                        thisMenu.Close();

                        level.Paused = false;
                        Engine.FreezeTimer = 0.15f;
                    };

                    // finally, add the menu to the scene
                    level.Add(thisMenu);
                });
            }

            // this is how it works in the main menu: way more simply than the in-game mess.
            return (TextMenuButtonExt) new TextMenuButtonExt(getButtonName()).Pressed(() => {
                gotoMenu(OuiModOptions.Instance.Overworld);
            });
        }
    }
}