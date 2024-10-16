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

using SharpGDX.Utils.Reflect;
using SharpGDX.Shims;
using SharpGDX.Ashley.Utils;
using SharpGDX.Ashley.Signals;
using SharpGDX.Ashley.Systems;

/**
 * The heart of the Entity framework. It is responsible for keeping track of {@link Entity} and
 * managing {@link EntitySystem} objects. The Engine should be updated every tick via the {@link #update(float)} method.
 *
 * With the Engine you can:
 *
 * <ul>
 * <li>Add/Remove {@link Entity} objects</li>
 * <li>Add/Remove {@link EntitySystem}s</li>
 * <li>Obtain a list of entities for a specific {@link Family}</li>
 * <li>Update the main loop</li>
 * <li>Register/unregister {@link EntityListener} objects</li>
 * </ul>
 *
 * @author Stefan Bachmann
 */
public class Engine {
	private static Family empty = Family.all().get();
	
	private readonly Listener<Entity> componentAdded ;
	private readonly Listener<Entity> componentRemoved;
	
	private SystemManager systemManager;
	private EntityManager entityManager ;
	private ComponentOperationHandler componentOperationHandler;
	private FamilyManager familyManager;	
	private bool updating;

    public Engine()
    {
        componentAdded = new ComponentListener(this);
        componentRemoved = new ComponentListener(this);
        systemManager = new SystemManager(new EngineSystemListener(this));
        entityManager = new EntityManager(new EngineEntityListener(this));
        componentOperationHandler = new ComponentOperationHandler(new EngineDelayedInformer(this));
        familyManager = new FamilyManager(entityManager.getEntities());

    }

	/**
	 * Creates a new Entity object.
	 * @return @{@link Entity}
	 */

	public virtual Entity createEntity () {
		return new Entity();
	}

	/**
	 * Creates a new {@link Component}. To use that method your components must have a visible no-arg constructor
	 */
	public virtual T? createComponent<T>(Type componentType)
	where T: Component{
		try {
			return ClassReflection.newInstance<T>(componentType);
		} catch (ReflectionException e) {
			return default(T);
		}
	}

	/**
	 * Adds an entity to this Engine.
	 * This will throw an IllegalArgumentException if the given entity
	 * was already registered with an engine.
	 */
	public void addEntity(Entity entity){
		bool delayed = updating || familyManager.notifying();
		entityManager.addEntity(entity, delayed);
	}

	/**
	 * Removes an entity from this Engine.
	 */
	public void removeEntity(Entity entity){
		bool delayed = updating || familyManager.notifying();
		entityManager.removeEntity(entity, delayed);
	}
	
	/**
	 * Removes all entities of the given {@link Family}.
	 */
	public void removeAllEntities(Family family) {
		bool delayed = updating || familyManager.notifying();
		entityManager.removeAllEntities(getEntitiesFor(family), delayed);
	}

	/**
	 * Removes all entities registered with this Engine.
	 */
	public void removeAllEntities() {
		bool delayed = updating || familyManager.notifying();
		entityManager.removeAllEntities(delayed);
	}

	/**
	 * Returns an {@link ImmutableArray} of {@link Entity} that is managed by the the Engine
	 *  but cannot be used to modify the state of the Engine. This Array is not Immutable in
	 *  the sense that its contents will not be modified, but in the sense that it only reflects
	 *  the state of the engine.
	 *
	 * The Array is Immutable in the sense that you cannot modify its contents through the API of
	 *  the {@link ImmutableArray} class, but is instead "Managed" by the Engine itself. The engine
	 *  may add or remove items from the array and this will be reflected in the returned array.
	 *
	 * This is an important note if you are looping through the returned entities and calling operations
	 *  that may add/remove entities from the engine, as the underlying iterator of the returned array
	 *  will reflect these modifications.
	 *
	 * The returned array will have entities removed from it if they are removed from the engine,
	 *   but there is no way to introduce new Entities through the array's interface, or remove
	 *   entities from the engine through the array interface.
	 *
	 *  Discussion of this can be found at https://github.com/libgdx/ashley/issues/224
	 *
	 * @return An unmodifiable array of entities that will match the state of the entities in the
	 *  engine.
	 */
	public ImmutableArray<Entity> getEntities() {
		return entityManager.getEntities();
	}

	/**
	 * Adds the {@link EntitySystem} to this Engine.
	 * If the Engine already had a system of the same class,
	 * the new one will replace the old one.
	 */
	public void addSystem(EntitySystem system){
		systemManager.addSystem(system);
	}

	/**
	 * Removes the {@link EntitySystem} from this Engine.
	 */
	public void removeSystem(EntitySystem system){
		systemManager.removeSystem(system);
	}

	/**
	 * Removes all systems from this Engine.
	 */
	public void removeAllSystems(){
		systemManager.removeAllSystems();
	}

	/**
	 * Quick {@link EntitySystem} retrieval.
	 */
	//@SuppressWarnings("unchecked")
	public  T? getSystem<T>(T systemType)
	where T: EntitySystem{
		return systemManager.getSystem<T>(systemType.GetType());
	}

	/**
	 * @return immutable array of all entity systems managed by the {@link Engine}.
	 */
	public ImmutableArray<EntitySystem> getSystems() {
		return systemManager.getSystems();
	}

	/** Returns immutable collection of entities for the specified {@link Family}. 
	 * Returns the same instance every time for the same Family.
	 */
	public ImmutableArray<Entity> getEntitiesFor(Family family){
		return familyManager.getEntitiesFor(family);
	}

	/**
	 * Adds an {@link EntityListener}.
	 *
	 * The listener will be notified every time an entity is added/removed to/from the engine.
	 */
	public void addEntityListener (EntityListener listener) {
		addEntityListener(empty, 0, listener);
	}

	/**
	 * Adds an {@link EntityListener}. The listener will be notified every time an entity is added/removed
	 * to/from the engine. The priority determines in which order the entity listeners will be called. Lower
	 * value means it will get executed first.
	 */
	public void addEntityListener (int priority, EntityListener listener) {
		addEntityListener(empty, priority, listener);
	}

	/**
	 * Adds an {@link EntityListener} for a specific {@link Family}.
	 *
	 * The listener will be notified every time an entity is added/removed to/from the given family.
	 */
	public void addEntityListener(Family family, EntityListener listener) {
		addEntityListener(family, 0, listener);
	}

	/**
	 * Adds an {@link EntityListener} for a specific {@link Family}. The listener will be notified every time an entity is
	 * added/removed to/from the given family. The priority determines in which order the entity listeners will be called. Lower
	 * value means it will get executed first.
	 */
	public void addEntityListener (Family family, int priority, EntityListener listener) {
		familyManager.addEntityListener(family, priority, listener);
	}

	/**
	 * Removes an {@link EntityListener}
	 */
	public void removeEntityListener (EntityListener listener) {
		familyManager.removeEntityListener(listener);
	}

	/**
	 * Updates all the systems in this Engine.
	 * @param deltaTime The time passed since the last frame.
	 */
	public void update(float deltaTime){
		if (updating) {
			throw new IllegalStateException("Cannot call update() on an Engine that is already updating.");
		}
		
		updating = true;
		ImmutableArray<EntitySystem> systems = systemManager.getSystems();
		try {
			for (int i = 0; i < systems.size(); ++i) {
				EntitySystem system = systems.get(i);
				
				if (system.checkProcessing()) {
					system.update(deltaTime);
				}
	
				while(componentOperationHandler.hasOperationsToProcess() || entityManager.hasPendingOperations()) {
					componentOperationHandler.processOperations();
					entityManager.processPendingOperations();
				}
			}
		}
		finally {
			updating = false;
		}	
	}
	
	protected void addEntityInternal(Entity entity) {
		entity.componentAdded.add(componentAdded);
		entity.componentRemoved.add(componentRemoved);
		entity.componentOperationHandler = componentOperationHandler;
		
		familyManager.updateFamilyMembership(entity);
	}
	
	protected virtual void removeEntityInternal(Entity entity) {
		familyManager.updateFamilyMembership(entity);

		entity.componentAdded.remove(componentAdded);
		entity.componentRemoved.remove(componentRemoved);
		entity.componentOperationHandler = null;
	}
	
	private class ComponentListener : Listener<Entity> {
        private readonly Engine _engine;

        public ComponentListener(Engine engine)
        {
            _engine = engine;
        }

		public void receive(Signal<Entity> signal, Entity obj) {
			_engine.familyManager.updateFamilyMembership(obj);
		}
	}
	
	private class EngineSystemListener : SystemManager.SystemListener {
        private readonly Engine _engine;

        public EngineSystemListener(Engine engine)
        {
            _engine = engine;
        }

		public void systemAdded (EntitySystem system) {
			system.addedToEngineInternal(_engine);
		}

		public void systemRemoved (EntitySystem system) {
			system.removedFromEngineInternal(_engine);
		}
	}
	
	private class EngineEntityListener : EntityListener {
        private readonly Engine _engine;

        public EngineEntityListener(Engine engine)
        {
            _engine = engine;
        }

		public void entityAdded (Entity entity) {
			_engine.addEntityInternal(entity);
		}

		public void entityRemoved (Entity entity) {
            _engine.removeEntityInternal(entity);
		}
	}
	
	private class EngineDelayedInformer : ComponentOperationHandler.BooleanInformer {
        private readonly Engine _engine;

        public EngineDelayedInformer(Engine engine)
        {
            _engine = engine;
        }

        public bool value () {
			return _engine.updating;
		}
	}
}
