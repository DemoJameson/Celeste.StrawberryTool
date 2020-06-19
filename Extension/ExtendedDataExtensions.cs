﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.StrawberryTool.Extension {
    // from https://stackoverflow.com/a/17264480
    public static class ExtendedDataExtensions {
        private static readonly ConditionalWeakTable<object, object> ExtendedData =
            new ConditionalWeakTable<object, object>();

        internal static IDictionary<string, object> CreateDictionary(object o) {
            return new Dictionary<string, object>();
        }

        public static void SetExtendedDataValue(this object o, string name, object value) {
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Invalid name");
            }

            name = name.Trim();

            IDictionary<string, object> values =
                (IDictionary<string, object>) ExtendedData.GetValue(o, CreateDictionary);

            if (value != null) {
                values[name] = value;
            }
            else {
                values.Remove(name);
            }
        }

        public static T GetExtendedDataValue<T>(this object o, string name) {
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Invalid name");
            }

            name = name.Trim();

            IDictionary<string, object> values =
                (IDictionary<string, object>) ExtendedData.GetValue(o, CreateDictionary);

            if (values.ContainsKey(name)) {
                return (T) values[name];
            }

            return default(T);
        }

        public static bool GetExtendedBoolean(this object o, string name) {
            return GetExtendedDataValue<bool>(o, name);
        }

        public static void SetExtendedBoolean(this object o, string name, bool value) {
            SetExtendedDataValue(o, name, value);
        }
    }
}