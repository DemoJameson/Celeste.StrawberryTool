using System.Collections.Generic;
using Celeste.Mod.StrawberryTool.Module;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.StrawberryTool.Feature.Translucent {
    public static class TranslucentUtils {
        private static readonly HashSet<EntityID> HasChangedFollowEntities = new HashSet<EntityID>();
        private static StrawberryToolSettings Settings => StrawberryToolModule.Settings;

        public static void Load() {
            On.Celeste.Player.Render += PlayerOnRender;
        }

        public static void Unload() {
            On.Celeste.Player.Render -= PlayerOnRender;
        }

        private static void PlayerOnRender(On.Celeste.Player.orig_Render orig, Player self) {
            orig(self);

            foreach (Follower follower in self.Leader.Followers) {
                Sprite sprite = follower.Entity.Get<Sprite>();
                if (sprite == null) {
                    continue;
                }

                EntityID id = default;
                switch (follower.Entity) {
                    case Strawberry berry:
                        id = berry.ID;
                        break;
                    case Key key:
                        id = key.ID;
                        break;
                }

                if (id.Equals(default(EntityID))) {
                    continue;
                }

                if (!Settings.TranslucentEnabled || !Settings.Enabled) {
                    if (HasChangedFollowEntities.Contains(id)) {
                        HasChangedFollowEntities.Remove(id);
                        sprite.Color = Color.White;
                    }

                    continue;
                }

                float distance = Vector2.Distance(follower.Entity.Position, self.Position) - Settings.TransparentRadius;
                if (distance < 0) {
                    distance = 0;
                }

                Color originalColor = sprite.Color;
                float alpha = distance / Settings.TranslucentRadius;
                float approachAlpha = Calc.Approach(originalColor.A, 255 * alpha, 10f);
                sprite.Color = Color.White * (approachAlpha / 255f);

                HasChangedFollowEntities.Add(id);
            }
        }
    }
}