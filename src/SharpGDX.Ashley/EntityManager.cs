namespace SharpGDX.Ashley;

using SharpGDX.Utils;
using SharpGDX.Ashley.Utils;
using SharpGDX.Shims;

class EntityManager {
	private EntityListener listener;
	private Array<Entity> entities = new Array<Entity>(false, 16);
	private ObjectSet<Entity> entitySet = new ObjectSet<Entity>();
	private ImmutableArray<Entity> immutableEntities;
	private Array<EntityOperation> pendingOperations = new Array<EntityOperation>(false, 16);
	private EntityOperationPool entityOperationPool = new EntityOperationPool();
	
	public EntityManager(EntityListener listener) {
		this.listener = listener;
        immutableEntities = new ImmutableArray<Entity>(entities);

    }
	
	public void addEntity(Entity entity){
		addEntity(entity, false);
	}
	
	public void addEntity(Entity entity, bool delayed){
		if (delayed) {
			EntityOperation operation = entityOperationPool.obtain();
			operation.entity = entity;
			operation.type = EntityOperation.Type.Add;
			pendingOperations.add(operation);
		}
		else {
			addEntityInternal(entity);
		}
	}
	
	public void removeEntity(Entity entity){
		removeEntity(entity, false);
	}
	
	public void removeEntity(Entity entity, bool delayed){
		if (delayed) {
			if(entity.scheduledForRemoval) {
				return;
			}
			entity.scheduledForRemoval = true;
			EntityOperation operation = entityOperationPool.obtain();
			operation.entity = entity;
			operation.type = EntityOperation.Type.Remove;
			pendingOperations.add(operation);
		}
		else {
			removeEntityInternal(entity);
		}
	}
	
	public void removeAllEntities() {
		removeAllEntities(immutableEntities);
	}
	
	public void removeAllEntities(bool delayed) {
		removeAllEntities(immutableEntities, delayed);
	}
	
	public void removeAllEntities(ImmutableArray<Entity> entities) {
		removeAllEntities(entities, false);
	}

	public void removeAllEntities(ImmutableArray<Entity> entities, bool delayed) {
		if (delayed) {
			foreach(Entity entity in entities) {
				entity.scheduledForRemoval = true;
			}
			EntityOperation operation = entityOperationPool.obtain();
			operation.type = EntityOperation.Type.RemoveAll;
			operation.entities = entities;
			pendingOperations.add(operation);
		}
		else {
			while(entities.size() > 0) {
				removeEntity(entities.first(), false);
			}
		}
	}
	
	public ImmutableArray<Entity> getEntities() {
		return immutableEntities;
	}
	
	public bool hasPendingOperations() {
		return pendingOperations.size > 0;
	}
	
	public void processPendingOperations() {
		for (int i = 0; i < pendingOperations.size; ++i) {
			EntityOperation operation = pendingOperations.get(i); 

			switch(operation.type) {
				case EntityOperation.Type.Add: addEntityInternal(operation.entity); break;
				case EntityOperation.Type.Remove: removeEntityInternal(operation.entity); break;
				case EntityOperation.Type.RemoveAll:
					while(operation.entities.size() > 0) {
						removeEntityInternal(operation.entities.first());
					}
					break;
				default:
					throw new ArgumentOutOfRangeException("Unexpected EntityOperation type");
			}

			entityOperationPool.free(operation);
		}
		
		pendingOperations.clear();
	}
	
	protected void removeEntityInternal(Entity entity) {
		bool removed = entitySet.remove(entity);

		if (removed) {
			entity.scheduledForRemoval = false;
			entity.removing = true;
			entities.removeValue(entity, true);
			listener.entityRemoved(entity);
			entity.removing = false;
		}
	}

	protected void addEntityInternal(Entity entity) {
		if (entitySet.contains(entity)) {
			throw new IllegalArgumentException("Entity is already registered " + entity);
		}

		entities.add(entity);
		entitySet.add(entity);

		listener.entityAdded(entity);
	}

	private class EntityOperation : IPoolable {
		public enum Type {
			Add,
			Remove,
			RemoveAll
		}

		public Type type;
		public Entity entity;
		public ImmutableArray<Entity> entities;

		public void reset() {
			entity = null;
		}
	}

	private class EntityOperationPool : Pool<EntityOperation> {
		protected override EntityOperation newObject() {
			return new EntityOperation();
		}
	}
}
