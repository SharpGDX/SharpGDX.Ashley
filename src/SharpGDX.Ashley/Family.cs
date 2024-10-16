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
using System.Text;

/**
 * Represents a group of {@link Component}s. It is used to describe what {@link Entity} objects an {@link EntitySystem} should
 * process. Example: {@code Family.all(PositionComponent.class, VelocityComponent.class).get()} Families can't be instantiated
 * directly but must be accessed via a builder ( start with {@code Family.all()}, {@code Family.one()} or {@code Family.exclude()}
 * ), this is to avoid duplicate families that describe the same components.
 * @author Stefan Bachmann
 */
public class Family {
	private static ObjectMap<String, Family> families = new ObjectMap<String, Family>();
	private static int familyIndex = 0;
	private static readonly Builder builder = new Builder();
	private static readonly Bits zeroBits = new Bits();

	private readonly Bits _all;
	private readonly Bits _one;
	private readonly Bits _exclude;
	private readonly int index;

	/** Private constructor, use static method Family.getFamilyFor() */
	private Family (Bits all, Bits any, Bits exclude) {
		this._all = all;
		this._one = any;
		this._exclude = exclude;
		this.index = familyIndex++;
	}

	/** @return This family's unique index */
	public int getIndex () {
		return this.index;
	}

	/** @return Whether the entity matches the family requirements or not */
	public bool matches (Entity entity) {
		Bits entityComponentBits = entity.getComponentBits();

		if (!entityComponentBits.containsAll(_all)) {
			return false;
		}

		if (!_one.isEmpty() && !_one.intersects(entityComponentBits)) {
			return false;
		}

		if (!_exclude.isEmpty() && _exclude.intersects(entityComponentBits)) {
			return false;
		}

		return true;
	}

	/**
	 * @param componentTypes entities will have to contain all of the specified components.
	 * @return A Builder singleton instance to get a family
	 */
	//@SafeVarargs
	// TODO: Should these all be params where it was Class<T extends Component>...? -RP
	public static Builder all (params Type[] componentTypes) {
		return builder.reset().all(componentTypes);
	}

	/**
	 * @param componentTypes entities will have to contain at least one of the specified components.
	 * @return A Builder singleton instance to get a family
	 */
	//@SafeVarargs
	public static Builder one ( Type[] componentTypes) {
		return builder.reset().one(componentTypes);
	}

	/**
	 * @param componentTypes entities cannot contain any of the specified components.
	 * @return A Builder singleton instance to get a family
	 */
	//@SafeVarargs
	public static Builder exclude (Type[] componentTypes) {
		return builder.reset().exclude(componentTypes);
	}

	public class Builder {
		private Bits _all = zeroBits;
		private Bits _one = zeroBits;
		private Bits _exclude = zeroBits;

		internal Builder() {
			
		}
		
		/**
		 * Resets the builder instance
		 * @return A Builder singleton instance to get a family
		 */
		public Builder reset () {
            _all = zeroBits;
            _one = zeroBits;
            _exclude = zeroBits;
			return this;
		}

		/**
		 * @param componentTypes entities will have to contain all of the specified components.
		 * @return A Builder singleton instance to get a family
		 */
		//@SafeVarargs
		public Builder all ( Type[] componentTypes) {
            _all = ComponentType.getBitsFor(componentTypes);
			return this;
		}

		/**
		 * @param componentTypes entities will have to contain at least one of the specified components.
		 * @return A Builder singleton instance to get a family
		 */
		//@SafeVarargs
		public Builder one (Type[] componentTypes) {
            _one = ComponentType.getBitsFor(componentTypes);
			return this;
		}

		/**
		 * @param componentTypes entities cannot contain any of the specified components.
		 * @return A Builder singleton instance to get a family
		 */
		//@SafeVarargs
		public Builder exclude (Type[] componentTypes) {
            _exclude = ComponentType.getBitsFor(componentTypes);
			return this;
		}

		/** @return A family for the configured component types */
		public Family get () {
			String hash = getFamilyHash(_all, _one, _exclude);
			Family family = families.get(hash, null);
			if (family == null) {
				family = new Family(_all, _one, _exclude);
				families.put(hash, family);
			}
			return family;
		}
	}

	public override int GetHashCode () {
		return index;
	}

	public override bool Equals (Object? obj) {
		return this == obj;
	}

	private static String getFamilyHash (Bits all, Bits one, Bits exclude) {
		StringBuilder stringBuilder = new StringBuilder();
		if (!all.isEmpty()) {
			stringBuilder.Append("{all:").Append(getBitsString(all)).Append("}");
		}
		if (!one.isEmpty()) {
			stringBuilder.Append("{one:").Append(getBitsString(one)).Append("}");
		}
		if (!exclude.isEmpty()) {
			stringBuilder.Append("{exclude:").Append(getBitsString(exclude)).Append("}");
		}
		return stringBuilder.ToString();
	}

	private static String getBitsString (Bits bits) {
		StringBuilder stringBuilder = new StringBuilder();

		int numBits = bits.length();
		for (int i = 0; i < numBits; ++i) {
			stringBuilder.Append(bits.get(i) ? "1" : "0");
		}

		return stringBuilder.ToString();
	}
}
