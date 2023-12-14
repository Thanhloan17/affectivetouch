using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpatialCalibration : MonoBehaviour
{

    public SpatialAnchorLoader spatialAnchorLoader;
    public Transform environmentTransform;
    public Transform leftMarker;
    public Transform rightMarker;

    [Header("Calibration")]
    public bool calibrateOnStart = false;
    public float angularPrecision = 1;
    public float calibrationTimeout = 3;

	private Coroutine calibrationCoroutine;
	private bool calibrating = false;

	private void OnEnable() {

		if (calibrateOnStart) {
			OVRManager.TrackingAcquired += Calibrate;
		}
	}

	private void OnDisable() {

		if (calibrateOnStart) {
			OVRManager.TrackingAcquired -= Calibrate;
		}
	}

	public void Calibrate() {

		if (calibrating) {
			StopCoroutine(calibrationCoroutine);
		}

		calibrationCoroutine = StartCoroutine(CalibrationCoroutine());
	}

    IEnumerator CalibrationCoroutine() {

		calibrating = true;

		Debug.Log("Starting calibration...");

		LoadAnchors();

		var anchors = spatialAnchorLoader.GetComponentsInChildren<Anchor>();

		float timoutTimer = 0;

		while (anchors.Length != 2) {

			timoutTimer += Time.deltaTime;

			if (timoutTimer < calibrationTimeout) {
				yield return null;
				anchors = spatialAnchorLoader.GetComponentsInChildren<Anchor>();
			} else {
				Debug.LogWarning("Calibration timeout, could not load anchors.");
				yield break;
			}
		}

		//Translate environment to match left anchor
		environmentTransform.Translate(anchors[0].transform.position - leftMarker.position, Space.World);

		//Rotate environment to match right anchor
		float currentDist = Vector3.Distance(anchors[1].transform.position, rightMarker.position);
		float minDist = currentDist;

		//Try rotating left
		bool done = false;
		while (!done) {
			environmentTransform.RotateAround(leftMarker.position, Vector3.up, angularPrecision);
			currentDist = Vector3.Distance(anchors[1].transform.position, rightMarker.position);
			if (currentDist < minDist) {
				//Rotation is reducing the distance, continue
				minDist = currentDist;
			} else {
				//Cancel last rotation and stop
				environmentTransform.RotateAround(leftMarker.position, Vector3.up, -angularPrecision);
				done = true;
			}
		}

		//Try rotating right
		done = false;
		while (!done) {
			environmentTransform.RotateAround(leftMarker.position, Vector3.up, -angularPrecision);
			currentDist = Vector3.Distance(anchors[1].transform.position, rightMarker.position);
			if (currentDist < minDist) {
				//Rotation is reducing the distance, continue
				minDist = currentDist;
			} else {
				//Cancel last rotation and stop
				environmentTransform.RotateAround(leftMarker.position, Vector3.up, -angularPrecision);
				done = true;
			}
		}

		Debug.Log("Calibration result : leftDistance = " + Vector3.Distance(anchors[0].transform.position, leftMarker.position) + "; rightDistance = " + Vector3.Distance(anchors[1].transform.position, rightMarker.position));

		calibrating = false;
	}

    public void LoadAnchors() {

		spatialAnchorLoader.LoadAnchorsByUuid();
	}

    public void EraseAllAnchors() {

        var anchors = spatialAnchorLoader.GetComponentsInChildren<Anchor>();

        Debug.Log("Erasing " + anchors.Length + " anchor(s).");

		foreach (var a in anchors) {

            a.OnEraseButtonPressed();
        }
    }
}
