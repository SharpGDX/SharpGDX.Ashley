namespace SharpGDX.Ashley;

using SharpGDX.Utils;
using SharpGDX.Ashley.Utils;

class FamilyManager {
	ImmutableArray<Entity> entities;
	private ObjectMap<Family, Array<Entity>> families = new ObjectMap<Family, Array<Entity>>();
	private ObjectMap<Family, ImmutableArray<Entity>> immutableFamilies = new ObjectMap<Family, ImmutableArray<Entity>>();
	private SnapshotArray<EntityListenerData> entityListeners = new SnapshotArray<EntityListenerData>(true, 16);
	private ObjectMap<Family, Bits> entityListenerMasks = new ObjectMap<Family, Bits>();
	private BitsPool bitsPool = new BitsPool();
	private bool _notifying = false;
	
	public FamilyManager(ImmutableArray<Entity> entities) {
		this.entities = entities;
	}
	
	public ImmutableArray<Entity> getEntitiesFor(Family family) {
		return registerFamily(family);
	}
	
	public bool notifying() {
		return _notifying;
	}
	
	public void addEntityListener (Family family, int priority, EntityListener listener) {
		registerFamily(family);

		int insertionIndex = 0;
		while (insertionIndex < entityListeners.size) {
			if (entityListeners.get(insertionIndex).priority <= priority) {
				insertionIndex++;
			} else {
				break;
			}
		}

		// Shift up bitmasks by one step
		foreach (Bits mask in entityListenerMasks.values()) {
			for (int k = mask.length(); k > insertionIndex; k--) {
				if (mask.get(k - 1)) {
					mask.set(k);
				} else {
					mask.clear(k);
				}
			}
			mask.clear(insertionIndex);
		}

		entityListenerMasks.get(family).set(insertionIndex);

		EntityListenerData entityListenerData = new EntityListenerData();
		entityListenerData.listener = listener;
		entityListenerData.priority = priority;
		entityListeners.insert(insertionIndex, entityListenerData);
	}
	
	public void removeEntityListener (EntityListener listener) {
		for (int i = 0; i < entityListeners.size; i++) {
			EntityListenerData entityListenerData = entityListeners.get(i);
			if (entityListenerData.listener == listener) {
				// Shift down bitmasks by one step
				foreach (Bits mask in entityListenerMasks.values()) {
					for (int k = i, n = mask.length(); k < n; k++) {
						if (mask.get(k + 1)) {
							mask.set(k);
						} else {
							mask.clear(k);
						}
					}
				}

				entityListeners.removeIndex(i--);
			}
		}
	}
	
	public void updateFamilyMembership (Entity entity) {
		// Find families that the entity was added to/removed from, and fill
		// the bitmasks with corresponding listener bits.
		Bits addListenerBits = bitsPool.obtain();
		Bits removeListenerBits = bitsPool.obtain();

		foreach (Family family in entityListenerMasks.keys()) {
			 int familyIndex = family.getIndex();
			 Bits entityFamilyBits = entity.getFamilyBits();

			bool belongsToFamily = entityFamilyBits.get(familyIndex);
			bool matches = family.matches(entity) && !entity.removing;

			if (belongsToFamily != matches) {
				Bits listenersMask = entityListenerMasks.get(family);
				Array<Entity> familyEntities = families.get(family);
				if (matches) {
					addListenerBits.or(listenersMask);
					familyEntities.add(entity);
					entityFamilyBits.set(familyIndex);
				} else {
					removeListenerBits.or(listenersMask);
					familyEntities.removeValue(entity, true);
					entityFamilyBits.clear(familyIndex);
				}
			}
		}

		// Notify listeners; set bits match indices of listeners
		_notifying = true;
		Object[] items = entityListeners.begin();

		try {
			for (int i = removeListenerBits.nextSetBit(0); i >= 0; i = removeListenerBits.nextSetBit(i + 1)) {
				((EntityListenerData)items[i]).listener.entityRemoved(entity);
			}
	
			for (int i = addListenerBits.nextSetBit(0); i >= 0; i = addListenerBits.nextSetBit(i + 1)) {
				((EntityListenerData)items[i]).listener.entityAdded(entity);
			}
		}
		finally {
			addListenerBits.clear();
			removeListenerBits.clear();
			bitsPool.free(addListenerBits);
			bitsPool.free(removeListenerBits);
			entityListeners.end();
			_notifying = false;	
		}
	}
	
	private ImmutableArray<Entity> registerFamily(Family family) {
		ImmutableArray<Entity> entitiesInFamily = immutableFamilies.get(family);

		if (entitiesInFamily == null) {
			Array<Entity> familyEntities = new Array<Entity>(false, 16);
			entitiesInFamily = new ImmutableArray<Entity>(familyEntities);
			families.put(family, familyEntities);
			immutableFamilies.put(family, entitiesInFamily);
			entityListenerMasks.put(family, new Bits());

			foreach (Entity entity in entities){
				updateFamilyMembership(entity);
			}
		}

		return entitiesInFamily;
	}
	
	private class EntityListenerData {
		public EntityListener listener;
		public int priority;
	}
	
	private  class BitsPool : Pool<Bits> {
		protected override Bits newObject () {
			return new Bits();
		}
	}
}
