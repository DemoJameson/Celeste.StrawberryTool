using Monocle;

namespace Celeste.Mod.StrawberryTool.Extension {
    public static class EntityExtension {
        private const string EntityIdKey = "StrawberryToolEntityId";
        private const string EntityDataKey = "StrawberryToolEntityDataKey";
        public static EntityID ToEntityID(this EntityData entityData) {
            return new EntityID(entityData.Level.Name, entityData.ID);
        }
        
        public static void SetEntityId(this Entity entity, EntityID entityId) {
            entity.SetExtendedDataValue(EntityIdKey, entityId);
        }

        public static void SetEntityId(this Entity entity, EntityData entityData) {
            entity.SetExtendedDataValue(EntityIdKey, entityData.ToEntityID());
        }

        public static EntityID GetEntityId(this Entity entity) {
            return entity.GetExtendedDataValue<EntityID>(EntityIdKey);
        }
        
        public static EntityData GetEntityData(this Entity entity) {
            return entity.GetExtendedDataValue<EntityData>(EntityDataKey);
        }
        
        public static void SetEntityData(this Entity entity, EntityData entityData) {
            entity.SetExtendedDataValue(EntityDataKey, entityData);
        }
    }
}