using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Ship))]
public class ShipInspector : Editor {

	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		Ship ship = target as Ship;

		if(GUILayout.Button("Update ship values"))
			ship.GenerateShipValues();
	}
}