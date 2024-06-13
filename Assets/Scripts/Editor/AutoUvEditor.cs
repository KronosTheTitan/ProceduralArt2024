using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AutoUv))]
public class AutoUvEditor : Editor
{
	public override void OnInspectorGUI() {
		AutoUv targetUv = (AutoUv)target;

		if (GUILayout.Button("Recalculate UVs")) {
			targetUv.UpdateUvs();
			EditorUtility.SetDirty(targetUv); // otherwise the new mesh won't be saved into the scene!
		}
		DrawDefaultInspector();
	}
}
