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

/**
 * Uniquely identifies a {@link Component} sub-class. It assigns them an index which is used internally for fast comparison and
 * retrieval. See {@link Family} and {@link Entity}. ComponentType is a package protected class. You cannot instantiate a
 * ComponentType. They can only be accessed via {@link #getIndexFor(Class<? extends Component>)}. Each component class will always
 * return the same instance of ComponentType.
 * @author Stefan Bachmann
 */
public sealed class ComponentType {
	private static ObjectMap<Type, ComponentType> assignedComponentTypes = new ObjectMap<Type, ComponentType>();
	private static int typeIndex = 0;

	private readonly int index;

	private ComponentType () {
		index = typeIndex++;
	}

	/** @return This ComponentType's unique index */
	public int getIndex () {
		return index;
	}

	/**
	 * @param componentType The {@link Component} class
	 * @return A ComponentType matching the Component Class
	 */
	public static ComponentType getFor (Type componentType) {
		ComponentType type = assignedComponentTypes.get(componentType);

		if (type == null) {
			type = new ComponentType();
			assignedComponentTypes.put(componentType, type);
		}

		return type;
	}

	/**
	 * Quick helper method. The same could be done via {@link ComponentType.getFor(Class<? extends Component>)}.
	 * @param componentType The {@link Component} class
	 * @return The index for the specified {@link Component} Class
	 */
	public static int getIndexFor ( Type componentType) {
		return getFor(componentType).getIndex();
	}

	/**
	 * @param componentTypes list of {@link Component} classes
	 * @return Bits representing the collection of components for quick comparison and matching. See
	 *         {@link Family#getFor(Bits, Bits, Bits)}.
	 */
	public static Bits getBitsFor (Type[] componentTypes) {
		Bits bits = new Bits();

		int typesLength = componentTypes.Length;
		for (int i = 0; i < typesLength; i++) {
			bits.set(ComponentType.getIndexFor(componentTypes[i]));
		}

		return bits;
	}

	public override int GetHashCode () {
		return index;
	}

	public override bool Equals (Object? obj) {
		if (this == obj) return true;
		if (obj == null) return false;
		if (GetType() != obj.GetType()) return false;
		ComponentType other = (ComponentType)obj;
		return index == other.index;
	}
}
