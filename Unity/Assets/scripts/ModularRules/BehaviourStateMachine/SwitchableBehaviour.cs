using UnityEngine;
using System.Collections;

public abstract class SwitchableBehaviour : MonoBehaviour
{

	/// <summary>
	/// called when switching movement types
	/// </summary>
	public virtual void Load()
	{
	}

	/// <summary>
	/// Called when unloading this movement type, before calling the load of the next movement type
	/// </summary>
	public virtual void Unload()
	{
	}
}
