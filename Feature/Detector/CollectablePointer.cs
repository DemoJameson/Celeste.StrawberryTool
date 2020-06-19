using System;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.StrawberryTool.Extension;
using Celeste.Mod.StrawberryTool.Module;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.StrawberryTool.Feature.Detector {
    [Tracked]
    public class CollectablePointer : Entity {
        private static StrawberryToolSettings Settings => StrawberryToolModule.Settings;

        public readonly EntityData EntityData;
        private readonly CollectableConfig collectableConfig;
        private readonly Vector2 followerPosition;
        private readonly List<MTexture> arrowImages;
        private float direction;
        private Sprite sprite;
        private float fade = 1f;

        public CollectablePointer(EntityData followerPosition, CollectableConfig collectableConfig) {
            EntityData = followerPosition;
            this.collectableConfig = collectableConfig;
            Position = this.followerPosition = EntityData.Position +
                                               new Vector2(EntityData.Level.Bounds.Left, EntityData.Level.Bounds.Top);

            Tag = Tags.Persistent;
            Depth = Depths.Top;

            arrowImages = GFX.Game.GetAtlasSubtextures("util/dasharrow/dasharrow");

            Add(new TransitionListener {
                OnOut = delegate(float f) {
                    if (!Settings.DetectCurrentRoom && EntityData.Level == SceneAs<Level>().Session.LevelData) {
                        fade = 1 - f;
                    }
                    else {
                        fade = 1f;
                    }
                },
            });
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            sprite = collectableConfig.GetSprite(SceneAs<Level>(), EntityData);
            sprite.Scale = Vector2.One * collectableConfig.Scale;
            Add(sprite);
        }

        public override void Update() {
            base.Update();

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null) {
                return;
            }

            Position = (player.Center + GetOffset(player) + Calc.AngleToVector(direction, 40f)).Round();

            float angle = (followerPosition - player.Position).Angle();
            if (Calc.AbsAngleDiff(angle, direction) >= 1.58079636f) {
                direction = angle;
            } else {
                direction = Calc.AngleApproach(direction, angle, (float) Math.PI * 6f * Engine.RawDeltaTime);
            }

            if (SceneAs<Level>().Session.DoNotLoad.Contains(EntityData.ToEntityID())) {
                RemoveSelf();
                return;
            }

            bool follow = player.Leader.Followers.Any(follower => {
                switch (follower.Entity) {
                    case Strawberry berry:
                        return berry.ID.Equals(EntityData.ToEntityID());
                    case Key key:
                        return key.ID.Equals(EntityData.ToEntityID());
                    default:
                        return false;
                }
            });

            bool collected = !Settings.DetectCollected && collectableConfig.HasCollected(EntityData);

            Visible = !follow && !collected;
        }

        public override void Render() {
            if (!Settings.Enabled || !Settings.DetectorEnabled) {
                return;
            }

            if (Engine.DashAssistFreeze) {
                return;
            }

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null) {
                return;
            }

            base.Render();

            float alpha = Settings.PointerOpacity / 10f * fade;
            float distance = Vector2.Distance(followerPosition, player.Position);
            float detectorRange = Settings.DetectorRange * 100;

            if (!Settings.DetectCurrentRoom) {
                distance = player.Position.Distance(EntityData.Level.Bounds);
            }

            if (distance < detectorRange) {
                alpha *= 1 - distance / detectorRange;
            }
            else {
                alpha = 0f;
            }

            sprite.Color = Color.White * alpha * (Settings.ShowIcon ? 1 : 0);

            MTexture mTexture = null;
            float rotation = float.MaxValue;
            for (int i = 0; i < 8; i++) {
                float angleDiff = Calc.AngleDiff((float) Math.PI * 2f * (i / 8f), direction);
                if (Math.Abs(angleDiff) < Math.Abs(rotation)) {
                    rotation = angleDiff;
                    mTexture = arrowImages[i];
                }
            }

            if (mTexture == null) {
                return;
            }

            if (Math.Abs(rotation) < 0.05f) {
                rotation = 0f;
            }

            mTexture.DrawCentered(
                (player.Center + GetOffset(player) + Calc.AngleToVector(direction, 30f)).Round(),
                Color.White * alpha, 1f, rotation);
        }

        private static Vector2 GetOffset(Player player) {
            return player.CurrentBooster == null && player.StateMachine.PreviousState != Player.StRedDash
                ? Vector2.Zero
                : new Vector2(0f, -4f);
        }
    }
}