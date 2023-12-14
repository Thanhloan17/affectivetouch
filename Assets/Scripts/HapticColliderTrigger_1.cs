using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HapticColliderTrigger_1 : MonoBehaviour
{
    public enum e_detectElement
    {
        ByTag, ByHapticDevice
    }

    [Tooltip("Set the source that will be connect to trigged HapticDevice")]
    public HapticSource hapticSource;
    [Tooltip("Test detection only on Object with the tag or look for all HapticDevice element on it")]
    public e_detectElement detectElement = e_detectElement.ByTag;
    [Tooltip("Tag of the HapticDevice GameObject")]
    public string hapticDeviceTag = "HapticDevice";
    [Tooltip("Weight amplitude with relative Velocity")]
    //public bool weightAmplitudeWithVelocity = false;
    [Range(0, 100)]
    public float velocityWeight= 10f;
    public HapticDevice[] HapticDevices; //predefine the haptic device 


    [ContextMenu("Get Source on GameObject of child")]
    void GetSourceFromCurrentGameObject()
    {
        hapticSource = GetComponent<HapticSource>();
        if (hapticSource == null)
        {
            hapticSource = GetComponentInChildren<HapticSource>();
            if (hapticSource == null)
                Debug.LogWarning("No Haptic Source found on the Gameobject or Child");
        }
    }



    //play audio form audio source that on audio device
    public void Detection()
    {
        if(detectElement == e_detectElement.ByHapticDevice)
        {
            if (HapticDevices.Length > 0)
            {

                foreach (HapticDevice h in HapticDevices)
                {
                    print(h.name);
                    h.setSource(hapticSource);
                    hapticSource.start();
                }
            }
        } 

       
    }
}
