using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ModularRules
{
	public class EventData
	{
		/// <summary>
		/// A list with differently typed data pieces
		/// </summary>
		private Dictionary<string, DataPiece> dataList = new Dictionary<string, DataPiece>();

		public static EventData Empty
		{
			get
			{
				return new EventData();
			}
		}

		/// <summary>
		/// Adds new data to the dataList - if it doesn't exist yet.
		/// </summary>
		/// <returns>false if data piece already exists</returns>
		public EventData Add(DataPiece newData)
		{
			try
			{
				dataList.Add(newData.id, newData);
				return this;
			}
			catch(System.ArgumentException)
			{
				Debug.LogWarning("Key " + newData.id + " already exists in EventData.");
				return this;
			}
		}

		public DataPiece Get(string id)
		{
			if (dataList.ContainsKey(id))
				return dataList[id];
			else
				return null;
		}

		/// <summary>
		/// Removes the data from the dataList
		/// </summary>
		public bool Remove(string id)
		{
			return dataList.Remove(id);
		}

		/// <summary>
		/// Removes all data pieces from the dataList.
		/// </summary>
		public void Clear()
		{
			dataList.Clear();
		}

		public override string ToString()
		{
			string s = "ED--";

			foreach (KeyValuePair<string, DataPiece> kvp in dataList)
			{
				s += kvp.Value.ToString() + "\n";
			}

			return s;
		}
	}
}
