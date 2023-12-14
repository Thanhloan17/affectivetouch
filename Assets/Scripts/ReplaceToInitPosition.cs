using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplaceToInitPosition : MonoBehaviour
{
    private Vector3 initialPos;
    public float allowedDistance = 1;
    // Start is called before the first frame update
    void Start()
    {
        initialPos = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Vector3.Distance(initialPos, transform.position) > allowedDistance)
        {
            transform.position = initialPos;
        }

    }
}
