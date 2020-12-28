using System;
using System.Collections;
using System.Linq;
using Celeste.Mod.StrawberryTool.Extension;
using Microsoft.Xna.Framework;
using Mono.Cecil;
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

            // update pointers while level is transitioning
            // not using Tags.TransitionUpdate in CollectablePointer is because entity.Update() is called before
            // the camera moves, thus what we get in level.Camera.Position is actually the value at last frame,
            // and it will cause some visual glitches
            On.Celeste.Level.TransitionRoutine += LevelOnTransitionRoutine;

            // render CollectablePointer after level lighting, so pointers will always be visible even room lighting is 0%
            IL.Celeste.Level.Render += LevelOnRender;
            On.Celeste.Tags.Initialize += TagsOnInitialize;
            On.Celeste.LevelLoader.LoadingThread += LevelLoaderOnLoadingThread;
            IL.Celeste.GameplayRenderer.Render += GameplayRendererOnRender;
        }

        public static void Unload() {
            On.Celeste.Level.LoadLevel -= LevelOnLoadLevel;
            On.Celeste.HeartGem.ctor_EntityData_Vector2 -= HeartGemOnCtor_EntityData_Vector2;
            On.Celeste.Cassette.ctor_EntityData_Vector2 -= CassetteOnCtor_EntityData_Vector2;
            On.Monocle.Entity.RemoveSelf -= EntityOnRemoveSelf;
            On.Celeste.Level.TransitionRoutine -= LevelOnTransitionRoutine;
            IL.Celeste.Level.Render -= LevelOnRender;
            On.Celeste.Tags.Initialize -= TagsOnInitialize;
            On.Celeste.LevelLoader.LoadingThread -= LevelLoaderOnLoadingThread;
            IL.Celeste.GameplayRenderer.Render -= GameplayRendererOnRender;
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
            if (!isFromLoader && playerIntro != Player.IntroTypes.Respawn)
                return;
            // entities with Tags.Global will not be removed after level reloading, so we need to remove them manually
            self.Entities.Remove(self.Entities.FindAll<CollectablePointer>());
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

        private static IEnumerator LevelOnTransitionRoutine(On.Celeste.Level.orig_TransitionRoutine orig, Level self,
            LevelData next, Vector2 direction) {
            IEnumerator enumerator = orig(self, next, direction);
            while (enumerator.MoveNext()) {
                self.Tracker.GetEntities<CollectablePointer>().ForEach(i => i.Update());
                yield return enumerator.Current;
            }
        }

        private static void TagsOnInitialize(On.Celeste.Tags.orig_Initialize orig) {
            orig();
            TagsExtension.InitCollectablePointer();
        }

        private static void LevelOnRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // move after this.Lighting.Render(this);
            if (cursor.TryGotoNext(
                MoveType.After,
                instr => instr.MatchLdarg(0),
                instr => instr.MatchLdfld(typeof(Level), nameof(Level.Lighting)),
                instr => instr.MatchLdarg(0),
                instr => instr.MatchCallvirt(typeof(Renderer), nameof(Renderer.Render)))
            ) {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Action<Level>>(level => {
                    CollectablePointerRenderer.Instance?.Render(level);
                });
            }
        }

        private static void LevelLoaderOnLoadingThread(On.Celeste.LevelLoader.orig_LoadingThread orig, LevelLoader self) {
            orig(self);
            self.Level.Add(new CollectablePointerRenderer());
        }

        private static void GameplayRendererOnRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(
                MoveType.After,
                instr => instr.MatchLdsfld(typeof(TagsExt), nameof(TagsExt.SubHUD)),
                instr => instr.OpCode == OpCodes.Call &&
                    (instr.Operand as MethodReference)?.FullName == "System.Int32 Monocle.BitTag::op_Implicit(Monocle.BitTag)",
                instr => instr.MatchOr())
            ) {
                // change scene.Entities.RenderExcept(Tags.HUD | TagsExt.SubHUD);
                // to     scene.Entities.RenderExcept(Tags.HUD | TagsExt.SubHUD | TagsExtension.CollectablePointer);
                cursor.EmitDelegate<Func<int>>(() => TagsExtension.CollectablePointer);
                cursor.Emit(OpCodes.Or);
            }
        }
    }
}