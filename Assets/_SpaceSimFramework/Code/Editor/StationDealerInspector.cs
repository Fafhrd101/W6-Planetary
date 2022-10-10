using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StationDealer))]
public class StationDealerInspector : Editor {

	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		StationDealer stationLoadout = target as StationDealer;

		if(GUILayout.Button("Generate dealer wares"))
			stationLoadout.GenerateStationData();

		if(GUILayout.Button("Clear dealer wares"))
			stationLoadout.ClearStationData();
	}
}