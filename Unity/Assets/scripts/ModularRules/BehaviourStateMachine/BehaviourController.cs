using UnityEngine;
using System.Collections;

public class BehaviourController<T> : MonoBehaviour where T : SwitchableBehaviour
{
	protected T currentBehaviour;

	public void ChangeBehaviour(T newBehaviour)
	{
		if (currentBehaviour != newBehaviour && newBehaviour != null)
		{
			if (currentBehaviour != null) currentBehaviour.Unload();
			currentBehaviour = newBehaviour;
			currentBehaviour.Load();
		}
	}
}
