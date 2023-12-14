using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UIElements;
using static AutoAttachHapticDevice;

public class AutoAttachHapticDevice : MonoBehaviour
{
    public enum e_HandSide
    { 
        Right, Left
    }

    public enum e_CapsuleRigidbody
    {
        Hand_WristRoot1 = 0, Hand_WristRoot2 = 1, Hand_WristRoot3 = 2, Hand_WristRoot4 = 3,
        Hand_Thumb1, Hand_Thumb2, Hand_Thumb3,
        Hand_Index1, Hand_Index2, Hand_Index3,
        Hand_Middle1, Hand_Middle2, Hand_Middle3,
        Hand_Ring1, Hand_Ring2, Hand_Ring3,
        Hand_Pinky1, Hand_Pinky2, Hand_Pinky3,
    }


    [Tooltip("Link to the Right OVR Hand in which the capsules will be created by OVR")]
    [SerializeField]
    private Transform OVRHand_R;
    private Transform CapsulesParent_R;
    [Tooltip("Link to the Left OVR Hand in which the capsules will be created by OVR")]
    [SerializeField]
    private Transform OVRHand_L;
    private Transform CapsulesParent_L;
    [SerializeField]
    public List<AttachLinkDevice> attachLinkDevices;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(waitForHand());
    }

    IEnumerator waitForHand()
    {
        bool done = false;
        for(var i = 0; i<10 & !done; i++)
        {
            yield return new WaitForSeconds(1);
            CapsulesParent_R = OVRHand_R.Find("Capsules");
            CapsulesParent_L = OVRHand_L.Find("Capsules");
            if (CapsulesParent_R == null || CapsulesParent_L == null)
            {
                Debug.Log("Capsules Parent not found");
            }
            else
            {
                //CapsuleRigidbodyParent = Find parent of CapsuleRigidbody
                foreach (var ald in attachLinkDevices)
                {
                    Transform cr = FindCapsuleRigidbody(ald.targetCollider, ald.handSide);
                    if (cr != null && ald.hapticDevice != null)
                    {
                        ald.hapticDevice.transform.parent = cr;
                    }
                }
                done = true;
            }
        }
    }

 

    Transform FindCapsuleRigidbody(e_CapsuleRigidbody cr, e_HandSide hs)
    {
        Transform capsule = null;
        string rbName = cr.ToString("g") ;
        string capsuleName = "";
        if (isHandWristRoot(cr)) 
        {
            rbName = rbName.Substring(0, rbName.Length-1);
        }
        capsuleName = rbName + "_CapsuleCollider";
        rbName += "_CapsuleRigidbody";
        if (hs == e_HandSide.Right)
        {
            if (isHandWristRoot(cr))
            {
                List<Transform> Hand_WristRootObj = new List<Transform>();
                foreach (Transform c in CapsulesParent_R)
                {
                   if(c.name == rbName)
                    {
                        Hand_WristRootObj.Add(c);
                    }

                }
                if ((int)cr < Hand_WristRootObj.Count)
                    capsule = Hand_WristRootObj[(int)cr].Find(capsuleName);
            }
            else
                capsule = CapsulesParent_R.Find(rbName + "/" + capsuleName);
        }
        else
        {
            if (isHandWristRoot(cr))
            {
                List<Transform> Hand_WristRootObj = new List<Transform>();
                foreach (Transform c in CapsulesParent_L)
                {
                    if (c.name == rbName)
                    {
                        Hand_WristRootObj.Add(c);
                    }

                }
                if ((int)cr < Hand_WristRootObj.Count)
                    capsule = Hand_WristRootObj[(int)cr].Find(capsuleName);
            }
            else
                capsule = CapsulesParent_L.Find(rbName + "/" + capsuleName);
        }
        return capsule;
    }

    static bool isHandWristRoot(e_CapsuleRigidbody cr)
    {
        return cr == e_CapsuleRigidbody.Hand_WristRoot1 || cr == e_CapsuleRigidbody.Hand_WristRoot2 || cr == e_CapsuleRigidbody.Hand_WristRoot3 || cr == e_CapsuleRigidbody.Hand_WristRoot4;
    }

    [System.Serializable]
    public class AttachLinkDevice
    {
        [SerializeField]
        public e_HandSide handSide;
        [Tooltip("Choose where to attach the Haptic device")]
        [SerializeField]
        public e_CapsuleRigidbody targetCollider;
        [Tooltip("Haptic Device that will be move and attach to the capsule collider chosen")]
        [SerializeField]
        public HapticDevice hapticDevice;
    }
}
