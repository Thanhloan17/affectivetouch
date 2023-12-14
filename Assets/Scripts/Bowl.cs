using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bowl : MonoBehaviour
{
	[Header("Bowl")]
	public GameObject bowlObject;

	public virtual void EnableBowl() {

		bowlObject.SetActive(true);
	}

	public virtual void DisableBowl() {

		bowlObject.SetActive(false);
	}
}
