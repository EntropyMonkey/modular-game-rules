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

		/// <summary>
		/// Adds new data to the dataList - if it doesn't exist yet.
		/// </summary>
		/// <returns>false if data piece already exists</returns>
		public bool Add(DataPiece newData)
		{
			try
			{
				dataList.Add(newData.id, newData);
				return true;
			}
			catch(System.ArgumentException)
			{
				return false;
			}
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
	}
}
