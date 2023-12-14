using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightingManager : MonoBehaviour
{
    [Header("Environment Lights")]
    public List<Light> environmentLights;

    public float environmentLightMultiplier = 1;

    private List<float> environmentLightsDefaultIntensities;

    private void Awake() {

        environmentLightsDefaultIntensities = new List<float>();

        for(int i=0; i<environmentLights.Count; i++) {
            environmentLightsDefaultIntensities.Add(environmentLights[i].intensity);
        }
    }

    private void Update() {

        UpdateEnvironmentLights();
    }

    void UpdateEnvironmentLights() {

        for(int i=0; i<environmentLights.Count; i++) {
            environmentLights[i].intensity = environmentLightsDefaultIntensities[i] * environmentLightMultiplier;
        }
    }
}
