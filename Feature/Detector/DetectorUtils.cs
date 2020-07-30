using System.Linq;
using Celeste.Mod.StrawberryTool.Extension;
using Celeste.Mod.StrawberryTool.Module;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.StrawberryTool.Feature.Detector {
    public static class DetectorUtils {
        private static StrawberryToolSettings Settings => StrawberryToolModule.Settings;

        public static void Load() {
            On.Celeste.Level.LoadLevel += LevelOnLoadLevel;
            On.Celeste.HeartGem.ctor_EntityData_Vector2 += HeartGemOnCtor_EntityData_Vector2;
            On.Celeste.Cassette.ctor_EntityData_Vector2 += CassetteOnCtor_EntityData_Vector2;
            On.Monocle.Entity.RemoveSelf += EntityOnRemoveSelf;
        }

        public static void Unload() {
            On.Celeste.HeartGem.ctor_EntityData_Vector2 -= HeartGemOnCtor_EntityData_Vector2;
            On.Celeste.Cassette.ctor_EntityData_Vector2 -= CassetteOnCtor_EntityData_Vector2;
            On.Monocle.Entity.RemoveSelf -= EntityOnRemoveSelf;
        }

        private static void HeartGemOnCtor_EntityData_Vector2(On.Celeste.HeartGem.orig_ctor_EntityData_Vector2 orig,
            HeartGem self, EntityData data, Vector2 offset) {
            orig(self, data, offset);
            self.SetEntityData(data);
        }

        private static void CassetteOnCtor_EntityData_Vector2(On.Celeste.Cassette.orig_ctor_EntityData_Vector2 orig,
            Cassette self, EntityData data, Vector2 offset) {
            orig(self, data, offset);
            self.SetEntityData(data);
        }

        private static void EntityOnRemoveSelf(On.Monocle.Entity.orig_RemoveSelf orig, Entity self) {
            if (self is HeartGem || self is Cassette) {
                Engine.Scene.Entities.FindAll<CollectablePointer>().ForEach(pointer => {
                    if (pointer.EntityData == self.GetEntityData()) {
                        pointer.RemoveSelf();
                    }
                });
            }

            orig(self);
        }

        private static void LevelOnLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self,
            Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);
            if (!isFromLoader && playerIntro != Player.IntroTypes.Respawn) return;
            var entities = self.Session.MapData.Levels.SelectMany(data => data.Entities);
            foreach (EntityData entityData in entities) {
                TryAddPointer(self, entityData);
            }
        }

        private static void TryAddPointer(Level level, EntityData entityData) {
            CollectableConfig collectableConfig =
                CollectableConfig.All.Find(item => item.ShouldBeAdded(level, entityData));
            if (collectableConfig != null) {
                level.Add(new CollectablePointer(entityData, collectableConfig));
            }
        }
    }
}