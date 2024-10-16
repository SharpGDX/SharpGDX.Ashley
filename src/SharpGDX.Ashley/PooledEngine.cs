/*******************************************************************************
 * Copyright 2014 See AUTHORS file.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 ******************************************************************************/

namespace SharpGDX.Ashley;

using SharpGDX.Utils;
using SharpGDX.Shims;
using SharpGDX.Ashley.Utils;

/**
 * Supports {@link Entity} and {@link Component} pooling. This improves performance in environments where creating/deleting
 * entities is frequent as it greatly reduces memory allocation.
 * <ul>
 * <li>Create entities using {@link #createEntity()}</li>
 * <li>Create components using {@link #createComponent(Class)}</li>
 * <li>Components should implement the {@link Poolable} interface when in need to reset its state upon removal</li>
 * </ul>
 * @author David Saltares
 */
//@SuppressWarnings({"rawtypes", "unchecked"})
public class PooledEngine : Engine {

	private EntityPool entityPool;
	private ComponentPools componentPools;

	/**
	 * Creates a new PooledEngine with a maximum of 100 entities and 100 components of each type. Use
	 * {@link #PooledEngine(int, int, int, int)} to configure the entity and component pools.
	 */
	public PooledEngine () 
    : this(10, 100, 10, 100)
    {
		
	}

	/**
	 * Creates new PooledEngine with the specified pools size configurations.
	 * @param entityPoolInitialSize initial number of pre-allocated entities.
	 * @param entityPoolMaxSize maximum number of pooled entities.
	 * @param componentPoolInitialSize initial size for each component type pool.
	 * @param componentPoolMaxSize maximum size for each component type pool.
	 */
	public PooledEngine (int entityPoolInitialSize, int entityPoolMaxSize, int componentPoolInitialSize, int componentPoolMaxSize)
        :base()
    {
		

		entityPool = new EntityPool(this, entityPoolInitialSize, entityPoolMaxSize);
		componentPools = new ComponentPools(componentPoolInitialSize, componentPoolMaxSize);
	}

	/** @return Clean {@link Entity} from the Engine pool. In order to add it to the {@link Engine}, use {@link #addEntity(Entity)}. @{@link Override {@link Engine#createEntity()}} */
	public override Entity createEntity () {
		return entityPool.obtain();
	}

	/**
	 * Retrieves a new {@link Component} from the {@link Engine} pool. It will be placed back in the pool whenever it's removed
	 * from an {@link Entity} or the {@link Entity} itself it's removed.
	 * Overrides the default implementation of Engine (creating a new Object)
	 */
	public override T createComponent<T>(Type componentType)
	{
		return componentPools.obtain<T>(componentType);
	}

	/**
	 * Removes all free entities and components from their pools. Although this will likely result in garbage collection, it will
	 * free up memory.
	 */
	public void clearPools () {
		entityPool.clear();
		componentPools.clear();
	}

	protected override void removeEntityInternal (Entity entity) {
		base.removeEntityInternal(entity);

		if (entity is PooledEntity) {
			entityPool.free((PooledEntity)entity);
		}
	}

	private class PooledEntity : Entity , IPoolable {
        private readonly PooledEngine _engine;

        public PooledEntity(PooledEngine engine)
        {
            _engine = engine;
        }

       internal override Component removeInternal(Type componentClass) {
			Component removed = base.removeInternal(componentClass);
			if (removed != null) {
				_engine.componentPools.free(removed);
			}

			return removed;
		}

		public void reset () {
			removeAll();
			flags = 0;
			componentAdded.removeAllListeners();
			componentRemoved.removeAllListeners();
			scheduledForRemoval = false;
			removing = false;
		}
	}

	private class EntityPool : Pool<PooledEntity> {
        private readonly PooledEngine _engine;

        public EntityPool (PooledEngine engine, int initialSize, int maxSize) 
        : base(initialSize, maxSize)
        {
            _engine = engine;
        }

		protected override PooledEntity newObject () {
			return new PooledEntity(_engine);
		}
	}

	private class ComponentPools {
		private ObjectMap<Type, ReflectionPool<object>> pools;
		private int initialSize;
		private int maxSize;

		public ComponentPools (int initialSize, int maxSize) {
			this.pools = new ObjectMap<Type, ReflectionPool<object>>();
			this.initialSize = initialSize;
			this.maxSize = maxSize;
		}

		public  T obtain<T>(Type type) {
			ReflectionPool<T> pool = pools.get(type) as ReflectionPool<T>;

			if (pool == null) {
				pool = new ReflectionPool<T>(type, initialSize, maxSize);
				pools.put(type, pool as ReflectionPool<object>);
			}

			return (T)pool.obtain();
		}

		public void free (Object obj) {
			if (obj == null) {
				throw new IllegalArgumentException("object cannot be null.");
			}

			ReflectionPool<object> pool = pools.get(obj.GetType());

			if (pool == null) {
				return; // Ignore freeing an object that was never retained.
			}

			pool.free(obj);
		}

		public void freeAll (Array<object> objects) {
			if (objects == null) throw new IllegalArgumentException("objects cannot be null.");

			for (int i = 0, n = objects.size; i < n; i++) {
				Object obj = objects.get(i);
				if (obj == null) continue;
				free(obj);
			}
		}

		public void clear () {
			foreach (Pool<object> pool in pools.values()) {
				pool.clear();
			}
		}
	}
}
