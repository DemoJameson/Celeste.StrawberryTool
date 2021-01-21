using System;
using System.Collections.Generic;
using Celeste.Mod.StrawberryTool.Extension;
using Celeste.Mod.StrawberryTool.Module;
using Monocle;

namespace Celeste.Mod.StrawberryTool.Feature.Detector {
    public class CollectableConfig {
        private static StrawberryToolSettings Settings => StrawberryToolModule.Settings;

        public Func<Level, EntityData, bool> ShouldBeAdded;
        public float Scale;
        public Func<Level, EntityData, Sprite> GetSprite;
        public Func<Level, EntityData, bool> HasCollected;
        public Func<bool> ShouldDetect;

        public static readonly List<CollectableConfig> All = new List<CollectableConfig> {
            new CollectableConfig {
                ShouldBeAdded = (level, data) => {
                    // strawberries, moon berries, custom entities with [RegisterStrawberry(tracked: true)]
                    return StrawberryRegistry.TrackableContains(data.Name);
                },
                Scale = 0.7f,
                GetSprite = (level, data) => {
                    string spriteId;
                    bool moon = data.Bool("moon");
                    if (SaveData.Instance.CheckStrawberry(data.ToEntityID())) {
                        if (moon) {
                            spriteId = "moonghostberry";
                        } else {
                            spriteId = "ghostberry";
                        }
                    } else {
                        if (moon) {
                            spriteId = "moonberry";
                        } else {
                            spriteId = "strawberry";
                        }
                    }

                    Sprite sprite = GFX.SpriteBank.Create(spriteId);
                    if (data.Bool("winged")) {
                        sprite.Play("flap");
                    }

                    return sprite;
                },
                HasCollected = (level, data) => SaveData.Instance.CheckStrawberry(data.ToEntityID()),
                ShouldDetect = () => Settings.DetectStrawberries
            },

            new CollectableConfig {
                ShouldBeAdded = (level, data) => {
                    Session session = level.Session;
                    switch (data.Name) {
                        case "goldenBerry":
                            bool cheatMode = SaveData.Instance.CheatMode;
                            bool initialState = session.FurthestSeenLevel == data.Level.Name || session.Deaths == 0;
                            bool unlockGolden = SaveData.Instance.UnlockedModes >= 3 || SaveData.Instance.DebugMode;
                            bool completed = SaveData.Instance.Areas[session.Area.ID].Modes[(int) session.Area.Mode].Completed;
                            return (cheatMode || completed && unlockGolden) && initialState;
                        case "memorialTextController":
                            return session.Dashes == 0 && session.StartedFromBeginning;
                        default:
                            return false;
                    }
                },
                Scale = 0.7f,
                GetSprite = (level, data) => {
                    string spriteId;
                    if (SaveData.Instance.CheckStrawberry(data.ToEntityID())) {
                        spriteId = "goldghostberry";
                    } else {
                        spriteId = "goldberry";
                    }

                    Sprite sprite = GFX.SpriteBank.Create(spriteId);
                    if (data.Bool("winged") || data.Name == "memorialTextController") {
                        sprite.Play("flap");
                    }

                    return sprite;
                },
                HasCollected = (level, data) => SaveData.Instance.CheckStrawberry(data.ToEntityID()),
                ShouldDetect = () => Settings.DetectGoldenStrawberries
            },

            new CollectableConfig {
                ShouldBeAdded = (level, data) => data.Name == "key",
                Scale = 0.6f,
                GetSprite = (level, entity) => GFX.SpriteBank.Create("key"),
                HasCollected = (level, data) => false,
                ShouldDetect = () => Settings.DetectKeys
            },

            new CollectableConfig {
                ShouldBeAdded = (level, data) => data.Name == "cassette" && !level.Session.Cassette,
                Scale = 0.4f,
                HasCollected = (level, data) => SaveData.Instance.Areas[level.Session.Area.ID].Cassette,
                ShouldDetect = () => Settings.DetectCassettes
            }.With(config =>
                config.GetSprite = (level, data) => {
                    string id = config.HasCollected(level, data) ? "cassetteGhost" : "cassette";
                    return GFX.SpriteBank.Create(id);
                }),

            new CollectableConfig {
                ShouldBeAdded = (level, data) =>
                    data.Name == "blackGem" && (!level.Session.HeartGem || level.Session.Area.Mode != AreaMode.Normal),
                Scale = 0.5f,
                HasCollected = (level, data) => {
                    AreaKey area = level.Session.Area;
                    return !data.Bool("fake") && SaveData.Instance.Areas_Safe[area.ID].Modes[(int) area.Mode].HeartGem;
                },
                ShouldDetect = () => Settings.DetectHeartGems
            }.With(config =>
                config.GetSprite = (level, data) => {
                    AreaKey area = level.Session.Area;
                    string id = data.Bool("fake") ? "heartgem3" :
                        !config.HasCollected(level, data) ? "heartgem" + (int) area.Mode : "heartGemGhost";
                    Sprite sprite = GFX.SpriteBank.Create(id);
                    sprite.Play("spin");
                    return sprite;
                }),

            new CollectableConfig {
                ShouldBeAdded = (level, data) => data.Name == "summitgem",
                Scale = 0.6f,
                GetSprite = (level, data) => {
                    Sprite sprite = new Sprite(GFX.Game, "collectables/summitgems/" + data.Int("gem") + "/gem");
                    sprite.AddLoop("idle", "", 0.08f);
                    sprite.Play("idle");
                    sprite.CenterOrigin();
                    return sprite;
                },
                HasCollected = (level, data) =>
                    SaveData.Instance.SummitGems != null && SaveData.Instance.SummitGems[data.Int("gem")],
                ShouldDetect = () => Settings.DetectSummitGems
            }
        };
    }
}