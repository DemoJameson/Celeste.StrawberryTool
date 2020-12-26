using Celeste.Mod.StrawberryTool.Extension;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.StrawberryTool.Feature.Detector {
    public class CollectablePointerRenderer : Renderer {
        public CollectablePointerRenderer() {
            Instance = this;
            gameplayRenderer = new DynData<GameplayRenderer>(null)["instance"] as GameplayRenderer;
        }

        public static void Begin() {
            Draw.SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointWrap,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Instance.gameplayRenderer.Camera.Matrix
            );
        }

        public override void Render(Scene scene) {
            Begin();
            scene.Entities.RenderOnly(TagsExtension.CollectablePointer);
            End();
        }

        public static void End() {
            Draw.SpriteBatch.End();
        }

        private GameplayRenderer gameplayRenderer;

        public static CollectablePointerRenderer Instance;
    }
}