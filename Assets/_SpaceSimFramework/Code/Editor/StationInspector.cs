using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Station))]
public class StationInspector : Editor {

	/*public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		
		//EditorGUILayout.Foldout()
		Station station = target as Station;
		
		if(GUILayout.Button("Set station values"))
			station.GenerateStation();

		if(GUILayout.Button("Clear station values"))
			station.ClearStation();
	}*/
}