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
using SharpGDX.Ashley.Signals;
using SharpGDX.Ashley.Systems;
using SharpGDX.Ashley.Utils;

/**
 * Simple containers of {@link Component}s that give them "data". The component's data is then processed by {@link EntitySystem}s.
 * @author Stefan Bachmann
 */
public class Entity {
	/** A flag that can be used to bit mask this entity. Up to the user to manage. */
	public int flags;
	/** Will dispatch an event when a component is added. */
	public readonly Signal<Entity> componentAdded;
	/** Will dispatch an event when a component is removed. */
	public readonly Signal<Entity> componentRemoved;

	internal bool scheduledForRemoval;
	internal bool removing;
	internal ComponentOperationHandler componentOperationHandler;

	private Bag<Component> components;
	private Array<Component> componentsArray;
	private ImmutableArray<Component> immutableComponentsArray;
	private Bits componentBits;
	private Bits familyBits;

	/** Creates an empty Entity. */
	public Entity () {
		components = new Bag<Component>();
		componentsArray = new Array<Component>(false, 16);
		immutableComponentsArray = new ImmutableArray<Component>(componentsArray);
		componentBits = new Bits();
		familyBits = new Bits();
		flags = 0;

		componentAdded = new Signal<Entity>();
		componentRemoved = new Signal<Entity>();
	}

	/**
	 * Adds a {@link Component} to this Entity. If a {@link Component} of the same type already exists, it'll be replaced.
	 * @return The Entity for easy chaining
	 */
	public Entity add (Component component) {
		if (addInternal(component)) {
			if (componentOperationHandler != null) {
				componentOperationHandler.add(this);
			}
			else {
				notifyComponentAdded();
			}
		}
		
		return this;
	}

	/**
	 * Adds a {@link Component} to this Entity. If a {@link Component} of the same type already exists, it'll be replaced.
	 * @return The Component for direct component manipulation (e.g. PooledComponent)
	 */
	public T addAndReturn<T>(T component)
	where T: Component{
		add(component);
		return component;
	}

	/**
	 * Removes the {@link Component} of the specified type. Since there is only ever one component of one type, we don't need an
	 * instance reference.
	 * @return The removed {@link Component}, or null if the Entity did not contain such a component.
	 */
	public  T? remove<T>(Type componentClass)
	where T: Component{
		ComponentType componentType = ComponentType.getFor(componentClass);
		int componentTypeIndex = componentType.getIndex();
		
		if(components.isIndexWithinBounds(componentTypeIndex)){
			Component removeComponent = components.get(componentTypeIndex);
	
			if (removeComponent != null && removeInternal(componentClass) != null) {
				if (componentOperationHandler != null) {
					componentOperationHandler.remove(this);
				}
				else {
					notifyComponentRemoved();
				}
			}
	
			return (T) removeComponent;
		}
		
		return default(T);
	}

	/** Removes all the {@link Component}'s from the Entity. */
	public void removeAll () {
		while (componentsArray.size > 0) {
			remove<Component>(componentsArray.get(0).GetType());
		}
	}

	/** @return immutable collection with all the Entity {@link Component}s. */
	public ImmutableArray<Component> getComponents () {
		return immutableComponentsArray;
	}

	/**
	 * Retrieve a component from this {@link Entity} by class. <em>Note:</em> the preferred way of retrieving {@link Component}s is
	 * using {@link ComponentMapper}s. This method is provided for convenience; using a ComponentMapper provides O(1) access to
	 * components while this method provides only O(logn).
	 * @param componentClass the class of the component to be retrieved.
	 * @return the instance of the specified {@link Component} attached to this {@link Entity}, or null if no such
	 *         {@link Component} exists.
	 */
	public T? getComponent<T>(Type componentClass)
	where T: Component{
		return getComponent<T>(ComponentType.getFor(componentClass));
	}

	/**
	 * Internal use.
	 * @return The {@link Component} object for the specified class, null if the Entity does not have any components for that class.
	 */
	//@SuppressWarnings("unchecked")
	// TODO: This was originally private, how was this called from ComponentMapper in Java? -RP
	internal T? getComponent<T> (ComponentType componentType)
	where T: Component{
		int componentTypeIndex = componentType.getIndex();

		if (componentTypeIndex < components.getCapacity()) {
			return (T)components.get(componentType.getIndex());
		} else {
			return default(T);
		}
	}

	/**
	 * @return Whether or not the Entity has a {@link Component} for the specified class.
	 */
	internal bool hasComponent (ComponentType componentType) {
		return componentBits.get(componentType.getIndex());
	}

	/**
	 * @return This Entity's component bits, describing all the {@link Component}s it contains.
	 */
	internal Bits getComponentBits () {
		return componentBits;
	}

	/** @return This Entity's {@link Family} bits, describing all the {@link EntitySystem}s it currently is being processed by. */
	internal Bits getFamilyBits () {
		return familyBits;
	}

	/**
	 * @param component
	 * @return whether or not the component was added.
	 */
	bool addInternal (Component component) {
		Type componentClass = component.GetType();
		Component oldComponent = getComponent<Component>(componentClass);

		if (component == oldComponent) {
			return false;
		}

		if (oldComponent != null) {
			removeInternal(componentClass);
		}

		int componentTypeIndex = ComponentType.getIndexFor(componentClass);
		components.set(componentTypeIndex, component);
		componentsArray.add(component);
		componentBits.set(componentTypeIndex);
		
		return true;
	}

	/**
	 * @param componentClass
	 * @return the component if the specified class was found and removed. Otherwise, null
	 */
	virtual internal Component removeInternal (Type componentClass) {
		ComponentType componentType = ComponentType.getFor(componentClass);
		int componentTypeIndex = componentType.getIndex();
		Component removeComponent = components.get(componentTypeIndex);

		if (removeComponent != null) {
			components.set(componentTypeIndex, null);
			componentsArray.removeValue(removeComponent, true);
			componentBits.clear(componentTypeIndex);
			
			return removeComponent;
		}

		return null;
	}
	
	internal void notifyComponentAdded() {
		componentAdded.dispatch(this);
	}
	
	internal void notifyComponentRemoved() {
		componentRemoved.dispatch(this);
	}

	/** @return true if the entity is scheduled to be removed */
	public bool isScheduledForRemoval () {
		return scheduledForRemoval;
	}

	/** @return true if the entity is being removed */
	public bool isRemoving() {
		return removing;
	}
}
