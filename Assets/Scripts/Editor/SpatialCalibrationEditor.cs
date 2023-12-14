using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpatialCalibration))]
public class SpatialCalibrationEditor : Editor
{
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        EditorGUILayout.Space();

        if (Application.isPlaying) {
            if (GUILayout.Button("Load Anchors")) {
                (target as SpatialCalibration).LoadAnchors();
            }

			EditorGUILayout.Space();

			if (GUILayout.Button("Calibrate")) {
                (target as SpatialCalibration).Calibrate();
            }

			EditorGUILayout.Space();

			EditorGUILayout.HelpBox("Erasing anchors is definitive !", MessageType.Warning);
			if (GUILayout.Button("Erase Anchors")) {
				(target as SpatialCalibration).EraseAllAnchors();
			}
		} else {

            EditorGUILayout.HelpBox("Calibration is done in play mode.", MessageType.Info);
        }
    }
}
