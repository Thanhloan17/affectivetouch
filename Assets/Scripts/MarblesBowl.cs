using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarblesBowl : Bowl
{
	[Header("Marbles")]
	public Transform marblesParent;
	public GameObject marblePrefab;

	public Vector3 marblesSpawnGridResolution = new Vector3(6, 10, 3);
	public float marbleSpawnGridSize = .05f;
	public Vector3 marblesSpawnOffset;
	public string marbleTag = "HapticCollider";

	private List<Marble> marbles;

	private void Awake() {

		SpawnMarbles();
	}

	void SpawnMarbles() {

		marbles = new List<Marble>();

		for(int x=0; x<marblesSpawnGridResolution.x; x++) {
			for (int y = 0; y < marblesSpawnGridResolution.y; y++) {
				for (int z = 0; z < marblesSpawnGridResolution.z; z++) {

					GameObject newMarbleObject = Instantiate(marblePrefab, marblesParent);
					newMarbleObject.tag = marbleTag;

					Vector3 startingPosition = marblesSpawnOffset
						+ Vector3.right * (x - 0.5f * marblesSpawnGridResolution.x) * marbleSpawnGridSize
						+ Vector3.up * (y - 0.5f * marblesSpawnGridResolution.y) * marbleSpawnGridSize
						+ Vector3.forward * (z - 0.5f * marblesSpawnGridResolution.z) * marbleSpawnGridSize;

					Marble newMarble = newMarbleObject.GetComponent<Marble>();

					newMarble.SetDefaultLocalPosition(startingPosition);
					newMarble.transform.localPosition = startingPosition;

					marbles.Add(newMarble);
				}
			}
		}
	}
}
