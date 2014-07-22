using UnityEngine;
using UnityEditor;

public class GA_Autorun : AssetPostprocessor
{
	static void OnPostprocessAllAssets ( string[] importedAssets,string[] deletedAssets,string[] movedAssets,string[] movedFromAssetPaths)
	{
		GA_Inspector.CheckForUpdates();
		
		GA_Tracking.Setup();
	}
}