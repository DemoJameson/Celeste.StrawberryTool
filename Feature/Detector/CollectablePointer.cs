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

        private static readonly float PointerLength = 20;
        private static readonly float IconLength = 40;
        
        public readonly EntityData EntityData;
        private float Alpha = 0f;
        private float lastAlpha = 0f;
        private readonly CollectableConfig collectableConfig;
        private readonly Vector2 followerPosition;
        private readonly List<MTexture> arrowImages;
        private float direction;
        private Sprite sprite;
        private float transitionFade = 1f;
        private float fade = 1f;
        private Level level;
        private VertexLight light;
        private BloomPoint bloom;

        public CollectablePointer(EntityData followerPosition, CollectableConfig collectableConfig) {
            EntityData = followerPosition;
            this.collectableConfig = collectableConfig;
            Position = this.followerPosition = EntityData.Position +
                                               new Vector2(EntityData.Level.Bounds.Left, EntityData.Level.Bounds.Top);

            Tag = Tags.Persistent;
            Depth = Depths.Top;

            arrowImages = GFX.Game.GetAtlasSubtextures("util/strawberry_tool_arrow/strawberry_tool_arrow");

            Add(new TransitionListener {
                OnOut = delegate(float value) {
                    if (!Settings.DetectCurrentRoom && EntityData.Level == level.Session.LevelData) {
                        transitionFade = 1 - value;
                    } else {
                        transitionFade = 1f;
                    }
                },
            });
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            level = scene as Level;

            sprite = collectableConfig.GetSprite(level, EntityData);
            sprite.Scale = Vector2.One * collectableConfig.Scale;
            Add(sprite);
            Add(light = new VertexLight(Color.White, 1f, 12, 12));
            Add(bloom = new BloomPoint(0.5f, 6f));
        }

        public override void Update() {
            base.Update();

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null) {
                return;
            }


            float angle = (followerPosition - player.Position).Angle();
            if (Calc.AbsAngleDiff(angle, direction) >= 1.58079636f) {
                direction = angle;
            } else {
                direction = Calc.AngleApproach(direction, angle, (float) Math.PI * 6f * Engine.RawDeltaTime);
            }

            if (Settings.ShowIconAtScreenEdge) {
                Position = GetCameraEdgePosition(level.Camera.Center());
            } else {
                Position = GetAroundPlayerPosition(player, IconLength);
            }

            if (level.Session.DoNotLoad.Contains(EntityData.ToEntityID())) {
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

            fade = follow || collected ? 0 : 1;
        }

        public override void Render() {
            light.Visible = bloom.Visible = false;

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

            float alpha = Settings.DetectorOpacity / 10f * fade * transitionFade;
            float distance = Vector2.Distance(followerPosition, Settings.ShowIconAtScreenEdge ? level.Camera.Center() : player.Position);
            float detectorRange = Settings.DetectorRange * 100;

            if (!Settings.DetectCurrentRoom) {
                if (level.Session.LevelData == EntityData.Level) {
                    distance = float.MaxValue;
                } else {
                    distance = player.Position.Distance(EntityData.Level.Bounds);
                }
            }

            if (distance < detectorRange) {
                alpha *= 1 - distance / detectorRange;
            } else {
                alpha = 0f;
            }

            Alpha = alpha;

            if (!Settings.OpacityGradient) {
                alpha = 1f;
            }

            var list = level.Entities.FindAll<CollectablePointer>();
            int currentRoomCounts = list.Count(pointer => level.Session.LevelData == pointer.EntityData.Level);
            list.Sort((pointer, collectablePointer) => Math.Sign(collectablePointer.Alpha - pointer.Alpha));
            if (list.IndexOf(this) >= Settings.MaxPointers - (Settings.DetectCurrentRoom ? 0 : currentRoomCounts)) {
                alpha = 0;
            }

            alpha = Calc.Approach(lastAlpha, alpha, Engine.DeltaTime * 3);

            lastAlpha = alpha;

            sprite.Color = Color.White * alpha * (Settings.ShowIcon ? 1 : 0);

            if (alpha > 0 && Settings.ShowIcon && Settings.ShowIconAtScreenEdge) {
                light.Visible = bloom.Visible = true;
            }

            if (!Settings.ShowPointer) {
                return;
            }

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

            mTexture.DrawCentered(GetAroundPlayerPosition(player, PointerLength), Color.White * alpha, 1f, rotation);
        }

        private Vector2 GetAroundPlayerPosition(Player player, float length) {
            return (player.Position + GetOffset(player) + Calc.AngleToVector(direction, length))
                .Round();
        }

        private Vector2 GetCameraEdgePosition(Vector2 position) {
            return level.Camera.GetIntersectionPoint(position, followerPosition, 5f);
        }

        private static Vector2 GetOffset(Player player) {
            return player.CurrentBooster == null && player.StateMachine.PreviousState != Player.StRedDash
                ? new Vector2(0f, -6f)
                : new Vector2(0f, -10f);
        }
    }
}