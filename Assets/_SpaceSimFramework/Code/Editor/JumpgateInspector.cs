using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Jumpgate))]
public class JumpgateInspector : Editor {

	/*public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		Jumpgate jumpgate = target as Jumpgate;

		if(GUILayout.Button("Set jumpgate values"))
			jumpgate.GenerateJumpgate();

		if(GUILayout.Button("Clear jumpgate values"))
			jumpgate.ClearJumpgate();
	}*/
}