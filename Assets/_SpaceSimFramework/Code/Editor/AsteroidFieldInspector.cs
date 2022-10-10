using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AsteroidField))]
public class AsteroidFieldInspector : Editor {

	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		AsteroidField asteroidField = target as AsteroidField;

		if(GUILayout.Button("Generate test asteroid field"))
			asteroidField.GenerateAsteroidField();

		if(GUILayout.Button("Clear asteroid field"))
			asteroidField.ClearAsteroidField();
	}
}