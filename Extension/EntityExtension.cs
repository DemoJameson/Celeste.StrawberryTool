using Monocle;

namespace Celeste.Mod.StrawberryTool.Extension {
    internal static class EntityExtension {
        private const string EntityIdKey = "StrawberryToolEntityId";
        private const string EntityDataKey = "StrawberryToolEntityDataKey";
        public static EntityID ToEntityID(this EntityData entityData) {
            return new EntityID(entityData.Level.Name, entityData.ID);
        }
        
        public static void SetEntityId(this Entity entity, EntityID entityId) {
            entity.GetDynDataInstance().Set(EntityIdKey, entityId);
        }

        public static void SetEntityId(this Entity entity, EntityData entityData) {
            entity.GetDynDataInstance().Set(EntityIdKey, entityData.ToEntityID());
        }

        public static EntityID GetEntityId(this Entity entity) {
            return entity.GetDynDataInstance().Get<EntityID>(EntityIdKey);
        }

        public static void SetEntityData(this Entity entity, EntityData entityData) {
            entity.GetDynDataInstance().Set(EntityDataKey, entityData);
        }

        public static EntityData GetEntityData(this Entity entity) {
            return entity.GetDynDataInstance().Get<EntityData>(EntityDataKey);
        }
    }
}