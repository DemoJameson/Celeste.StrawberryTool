using System;

namespace Celeste.Mod.StrawberryTool.Extension {
    public static class SimpleExtension {
        public static T With<T>(this T item, Action<T> action) {
            action(item);
            return item;
        }
    }
}