using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightingManagerControllable : Controllable
{
	[OSCProperty][Range(0, 2)] public float environmentLightMultiplier;
}
