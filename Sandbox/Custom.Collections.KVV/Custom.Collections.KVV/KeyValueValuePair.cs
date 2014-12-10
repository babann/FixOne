using System;
using System.Text;

namespace Custom.Collections.KVV
{
	[Serializable]
	public class KeyValueValuePair<TKey, TValue1, TValue2>
	{
		public TKey Key { get; private set; }
		public TValue1 Value1 { get; set; }
		public TValue2 Value2 { get; set; }

		public KeyValueValuePair (TKey key, TValue1 value1, TValue2 value2)
		{
			if (isCanBeNull<TKey>() && key == null)
				throw new ArgumentNullException ("key");

			Key = key;
			Value1 = value1;
			Value2 = value2;
		}

		public override string ToString ()
		{
			StringBuilder s = new StringBuilder();
			s.Append('[');
			s.Append(Key.ToString());
			s.Append(", ");
			s.Append (this.Value1 == null ? "null" : this.Value1.ToString());
			s.Append(", ");
			s.Append (this.Value2 == null ? "null" : this.Value2.ToString());
			s.Append(']');
			return s.ToString();
		}

		public override bool Equals (object obj)
		{
			var object1 = obj as KeyValueValuePair<TKey, TValue1, TValue2>;
			if (object1 == null)
				return false;

			return equalsImpl<TKey> (this.Key, object1.Key)
			&& equalsImpl<TValue1> (this.Value1, object1.Value1)
			&& equalsImpl<TValue2> (this.Value2, object1.Value2);
		}

		public override int GetHashCode ()
		{
			if (!(typeof(TKey).IsValueType) && Key == null)
				throw new ApplicationException ("Key can't be null.");

			return Key.GetHashCode ();
		}

		private bool equalsImpl<T>(T v1, T v2)
		{
			if(!isCanBeNull<T>())
				return v1.Equals (v2);

			if((v1 == null && v2 != null) || (v1 != null && v2 == null))
				return false;

			return v1.Equals(v2);
		}

		private bool isCanBeNull<T>()
		{
			//return typeof(T).IsValueType; //will not work for nullable types
			return default(T) == null;
		}
	}
}

