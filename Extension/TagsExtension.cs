using System.Collections.Generic;
using Monocle;

namespace Celeste.Mod.StrawberryTool.Extension {
    public static class TagsExtension {
        private const string TagName = "collectablePointer";
        private static BitTag _CollectablePointer;

        public static BitTag CollectablePointer {
            get {
                // fixed game crashes after hotreload
                if (_CollectablePointer == null) {
                    _CollectablePointer = BitTag.Get(TagName);
                }

                return _CollectablePointer;
            }
            set => _CollectablePointer = value;
        }
    }
}