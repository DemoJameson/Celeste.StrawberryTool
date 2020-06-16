 using System;
 using System.Collections.Generic;
 using System.Reflection;
 using Microsoft.Xna.Framework;
 using Monocle;

 namespace Celeste.Mod.StrawberryTool {
    public class StrawberryToolModule : EverestModule {
        public StrawberryToolModule() {
            Instance = this;
        }
        public override Type SettingsType => typeof(StrawberryToolSettings);
        
        public static StrawberryToolModule Instance;
        public static StrawberryToolSettings Settings => (StrawberryToolSettings) Instance._Settings;

        private readonly HashSet<EntityID> hasChangedBerries= new HashSet<EntityID>();

        public override void Load() {
            On.Celeste.Player.Render += PlayerOnRender;
        }

        public override void Unload() {
            On.Celeste.Player.Render -= PlayerOnRender;
        }

        private void PlayerOnRender(On.Celeste.Player.orig_Render orig, Player self) {
            orig(self);

            foreach (Follower follower in self.Leader.Followers) {
                Sprite sprite = follower.Entity.Get<Sprite>();
                if (sprite == null) {
                    continue;
                }
                
                EntityID id = default;
                if (follower.Entity is Strawberry berry) {
                    id = berry.ID;
                } else if (follower.Entity is Key key) {
                    id = key.ID;
                }
                
                if (id.Equals(default(EntityID))) {
                    continue;
                }
                
                if (!Settings.Enabled) {
                    if (hasChangedBerries.Contains(id)) {
                        hasChangedBerries.Remove(id);
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
                float targetAlpha = Calc.Approach(originalColor.A, 255 * alpha, 10f);
                sprite.Color = Color.White * (targetAlpha / 255f);
                
                hasChangedBerries.Add(id); 
            }
        }
    }
}