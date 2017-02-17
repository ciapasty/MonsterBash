using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Tuple class of 2 generic elements
/// </summary>
/// <typeparam name="T1">First element type</typeparam>
/// <typeparam name="T2">Second element type</typeparam> 
[System.Serializable]
public class Tuple<T1, T2>
{
	#region Variables

	public T1 first;
	public T2 second;

	private static readonly IEqualityComparer<T1> Item1Comparer = EqualityComparer<T1>.Default;
	private static readonly IEqualityComparer<T2> Item2Comparer = EqualityComparer<T2>.Default;

	#endregion

	#region Constructors

	public Tuple(T1 _first, T2 _second) //originally was _internal_
	{
		first = _first;
		second = _second;
	}

	#endregion

	#region Public Functions

	public override string ToString()
	{
		return string.Format("<{0}, {1}>", first, second);
	}

	public override int GetHashCode()
	{
		int hash = 17;
		hash = hash * 23 + first.GetHashCode();
		hash = hash * 23 + second.GetHashCode();
		return hash;
	}


	public override bool Equals(object obj)
	{
		var other = obj as Tuple<T1, T2>;
		if (object.ReferenceEquals(other, null))
			return false;
		else
			return Item1Comparer.Equals(first, other.first) && Item2Comparer.Equals(second, other.second);
	}

	#endregion

	#region Private Functions

	private static bool IsNull(object obj)
	{
		return object.ReferenceEquals(obj, null);
	}

	#endregion

	#region Operators

	public static bool operator ==(Tuple<T1, T2> a, Tuple<T1, T2> b)
	{
		if (Tuple<T1, T2>.IsNull(a) && !Tuple<T1, T2>.IsNull(b))
			return false;

		if (!Tuple<T1, T2>.IsNull(a) && Tuple<T1, T2>.IsNull(b))
			return false;

		if (Tuple<T1, T2>.IsNull(a) && Tuple<T1, T2>.IsNull(b))
			return true;

		return
			a.first.Equals(b.first) &&
			a.second.Equals(b.second);
	}

	public static bool operator !=(Tuple<T1, T2> a, Tuple<T1, T2> b)
	{
		return !(a == b);
	}

	#endregion
}