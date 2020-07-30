using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.StrawberryTool.Extension;
using Celeste.Mod.StrawberryTool.Module;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.StrawberryTool.Feature.Detector {
    public static class DetectorUtils {
        private static StrawberryToolSettings Settings => StrawberryToolModule.Settings;

        public static void Load() {
            On.Celeste.Player.Added += PlayerOnAdded;
            On.Celeste.HeartGem.ctor_EntityData_Vector2 += HeartGemOnCtor_EntityData_Vector2;
            On.Celeste.Cassette.ctor_EntityData_Vector2 += CassetteOnCtor_EntityData_Vector2;
            On.Monocle.Entity.RemoveSelf += EntityOnRemoveSelf;
        }

        public static void Unload() {
            On.Celeste.Player.Added -= PlayerOnAdded;
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

        private static void PlayerOnAdded(On.Celeste.Player.orig_Added orig, Player self, Scene scene) {
            orig(self, scene);

            var entities = self.SceneAs<Level>().Session.MapData.Levels.SelectMany(data => data.Entities);
            foreach (EntityData entityData in entities) {
                TryAddPointer(self, entityData);
            }
        }

        private static void TryAddPointer(Player player, EntityData entityData) {
            CollectableConfig collectableConfig =
                CollectableConfig.All.Find(item => item.ShouldBeAdded(player.SceneAs<Level>(), entityData));
            if (collectableConfig == null) {
                return;
            }

            player.Add(new Coroutine(AddCollectablePointer(player, entityData, collectableConfig)));
        }

        private static IEnumerator AddCollectablePointer(Player player, EntityData entityData,
            CollectableConfig collectableConfig) {
            player.Scene.Add(new CollectablePointer(entityData, collectableConfig));
            yield break;
        }
    }
}