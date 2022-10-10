using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Planet))]
public class PlanetInspector : Editor {

	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		Planet planet = target as Planet;

		// if(GUILayout.Button("Generate planetary values"))
		// 	planet.GeneratePlanet();
		//
		// if(GUILayout.Button("Clear planetary values"))
		// 	planet.ClearPlanet();
	}
}