using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Marble : MonoBehaviour
{
    private new Rigidbody rigidbody;

    private Vector3 startingPosition;

    private void OnEnable() {

        transform.localPosition = startingPosition;
    }

    public void SetDefaultLocalPosition(Vector3 position) {

        startingPosition = position;
    }
}
