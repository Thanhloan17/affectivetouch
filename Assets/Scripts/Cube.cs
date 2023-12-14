using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Cube : MonoBehaviour
{
    private new Rigidbody rigidbody;

    private Vector3 startingPosition;

    private void Awake() {

        startingPosition = transform.localPosition;
    }

    private void OnEnable() {

        transform.localPosition = startingPosition;
    }
}
