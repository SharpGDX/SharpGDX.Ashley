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

using System.Collections;

namespace SharpGDX.Ashley.Utils;

using SharpGDX.Utils;

/**
 * Wrapper class to treat {@link Array} objects as if they were immutable. However, note that the values could be modified if they
 * are mutable.
 * @author David Saltares
 */
public class ImmutableArray<T> : IEnumerable<T> {
	private readonly Array<T> array;
	private Array<T>.ArrayIterable iterable;

	public ImmutableArray (Array<T> array) {
		this.array = array;
	}

	public int size () {
		return array.size;
	}

	public T get (int index) {
		return array.get(index);
	}

	public bool contains (T value, bool identity) {
		return array.contains(value, identity);
	}

	public int indexOf (T value, bool identity) {
		return array.indexOf(value, identity);
	}

	public int lastIndexOf (T value, bool identity) {
		return array.lastIndexOf(value, identity);
	}

	public T peek () {
		return array.peek();
	}

	public T first () {
		return array.first();
	}

	public T random () {
		return array.random();
	}

	public T[] toArray () {
		return array.toArray();
	}

	public  V[] toArray<V>(Type type) {
		return array.toArray<V>(type);
	}
	
	public override int GetHashCode() {
		return array.GetHashCode();
	}

	public override bool Equals (Object? obj) {
		return array.equals(obj);
	}

	public override String ToString () {
		return array.ToString();
	}

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public String toString (String separator) {
		return array.toString(separator);
	}

	public IEnumerator<T> GetEnumerator () {
		if (iterable == null) {
			iterable = new Array<T>.ArrayIterable(array, false);
		}

		return iterable.GetEnumerator();
	}
}
