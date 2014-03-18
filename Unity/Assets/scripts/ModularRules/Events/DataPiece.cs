using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ModularRules
{
	/// <summary>
	/// A datapiece.
	/// TODO: Rework into generic class for type safety.
	/// </summary>
	public class DataPiece : IEqualityComparer<DataPiece>
	{
		/// <summary>
		/// This data piece's id. Used for identifying data in EventData.DataList.
		/// </summary>
		public string id { get; private set; }

		public object data { get; set; }

		/// <summary>
		/// returns instance type
		/// </summary>
		public System.Type dataType
		{
			get { return data.GetType(); }
		}

		// constructor
		public DataPiece(string _id)
		{
			id = _id;
		}

		#region IEqualityComparer implementation
		public bool Equals(DataPiece current, DataPiece other)
		{
			if (other == null || current == null)
				return false;

			return current.id == other.id && current.dataType == other.dataType && current.data == other.data;
		}

		public int GetHashCode(DataPiece other)
		{
			return other.id.GetHashCode() ^ other.data.GetHashCode();
		}
		#endregion

		#region Equals / GetHashCode overrides
		public override bool Equals(object obj)
		{
			DataPiece p = obj as DataPiece;

			if (p == null) return false;

			return base.Equals(obj) && Equals(p);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode() ^ id.GetHashCode() ^ data.GetHashCode();
		}
		#endregion

		public override string ToString()
		{
			return data.ToString();
		}
	}
}
