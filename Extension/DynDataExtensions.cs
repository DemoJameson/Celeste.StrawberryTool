using System.Runtime.CompilerServices;
using MonoMod.Utils;

namespace Celeste.Mod.StrawberryTool.Extension {
    internal static class DynDataExtensions {
        private static readonly ConditionalWeakTable<object, object> CachedDynDataInstances =
            new ConditionalWeakTable<object, object>();

        public static DynData<T> GetDynDataInstance<T>(this T target) where T : class {
            object targetOrType = target;
            if (target == null) {
                targetOrType = typeof(T);
            }

            if (CachedDynDataInstances.TryGetValue(targetOrType, out object dynData)) {
                return (DynData<T>) dynData;
            } else {
                dynData = new DynData<T>(target);
                CachedDynDataInstances.Add(targetOrType, dynData);
                return (DynData<T>) dynData;
            }
        }
    }
}