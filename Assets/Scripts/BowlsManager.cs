using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowlsManager : MonoBehaviour
{
    [Header("Bowls")]
    public GameObject waterBowl;
    public GameObject marblesBowl;
    public GameObject cubeBowl;
    public GameObject spidersBowl;

    private void Start() {

        DisableAll();
    }

    public void DisableAll() {

        waterBowl.SetActive(false);
        marblesBowl.SetActive(false);
        cubeBowl.SetActive(false);
        spidersBowl.SetActive(false);
    }

    public void ActivateWater() {

		DisableAll();
		waterBowl.SetActive(true);
    }

    public void ActivateMarbles() {

		DisableAll();
		marblesBowl.SetActive(true);
    }

    public void ActivateCube() {

		DisableAll();
		cubeBowl.SetActive(true);
    }

    public void ActivateSpiders() {

		DisableAll();
		spidersBowl.SetActive(true);
	}
}
