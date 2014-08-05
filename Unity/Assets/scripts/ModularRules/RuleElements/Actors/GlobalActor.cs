using UnityEngine;
using System.Collections;

[DontShowInRuleGUI]
public class GlobalActor : Actor
{
	#region Singleton
	public static GlobalActor Instance
	{
		get
		{
			if (applicationIsQuitting)
			{
				Debug.LogWarning("[Singleton] Instance 'GlobalActor" + 
					"' already destroyed on application quit." +
					" Won't create again - returning null.");
				return null;
			}

			if (instance == null)
			{
				instance = new GameObject("GlobalActor").AddComponent<GlobalActor>();
				DontDestroyOnLoad(instance.gameObject);
			}
			return instance;
		}
	}

	static GlobalActor instance = null;


	private static bool applicationIsQuitting = false;
	/// <summary>
	/// When Unity quits, it destroys objects in a random order.
	/// In principle, a Singleton is only destroyed when application quits.
	/// If any script calls Instance after it have been destroyed, 
	///   it will create a buggy ghost object that will stay on the Editor scene
	///   even after stopping playing the Application. Really bad!
	/// So, this was made to be sure we're not creating that buggy ghost object.
	/// </summary>
	/// 
	void OnDestroy()
	{
		applicationIsQuitting = true;
		instance = null;
	}
	#endregion

	public override void Initialize(RuleGenerator generator)
	{
		base.Initialize(generator);
		
		DontDeleteOnLoad = true;
		Label = "GlobalActor";
		Id = 1000;
	}

	public override void ShowGui(BaseRuleElement.RuleData ruleData)
	{
	}
}
