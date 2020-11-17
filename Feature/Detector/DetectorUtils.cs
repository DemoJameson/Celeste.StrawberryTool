using System;
using System.Linq;
using Celeste.Mod.StrawberryTool.Extension;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.StrawberryTool.Feature.Detector {
    public static class DetectorUtils {
        public static void Load() {
            On.Celeste.Level.LoadLevel += LevelOnLoadLevel;
            On.Celeste.HeartGem.ctor_EntityData_Vector2 += HeartGemOnCtor_EntityData_Vector2;
            On.Celeste.Cassette.ctor_EntityData_Vector2 += CassetteOnCtor_EntityData_Vector2;
            On.Monocle.Entity.RemoveSelf += EntityOnRemoveSelf;
            IL.Celeste.LightingRenderer.BeforeRender += LightingRendererOnBeforeRender;
        }

        public static void Unload() {
            On.Celeste.Level.LoadLevel -= LevelOnLoadLevel;
            On.Celeste.HeartGem.ctor_EntityData_Vector2 -= HeartGemOnCtor_EntityData_Vector2;
            On.Celeste.Cassette.ctor_EntityData_Vector2 -= CassetteOnCtor_EntityData_Vector2;
            On.Monocle.Entity.RemoveSelf -= EntityOnRemoveSelf;
            IL.Celeste.LightingRenderer.BeforeRender -= LightingRendererOnBeforeRender;
        }

        // replace if (item.DisableLightsInside)
        // to      if (item.DisableLightsInside && component.Entity.GetType() != typeof(CollectablePointer)
        // make CollectablePointer light inside foreground tiles.
        private static void LightingRendererOnBeforeRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(MoveType.After, instruction => instruction.MatchLdfld<Solid>("DisableLightsInside"))) {
                // find a VertexLight local variable (which is cast from Component)
                if (cursor.TryFindNext(out var cursors,
                    instruction => instruction.OpCode.Name.ToLowerInvariant().StartsWith("ldloc") &&
                        ((VariableReference) instruction.Operand).VariableType.Name == typeof(VertexLight).Name)) {
                    cursor.Emit(cursors[0].Next.OpCode, cursors[0].Next.Operand);
                    cursor.EmitDelegate<Func<bool, VertexLight, bool>>((disableLightsInside, vertexLight) =>
                        disableLightsInside && vertexLight.Entity.GetType() != typeof(CollectablePointer));
                }
            }
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