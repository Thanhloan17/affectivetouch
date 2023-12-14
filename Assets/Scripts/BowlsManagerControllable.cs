using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowlsManagerControllable : Controllable
{
    [OSCMethod] public void ActivateWater() { (TargetScript as BowlsManager).ActivateWater(); }
    [OSCMethod] public void ActivateMarbles() { (TargetScript as BowlsManager).ActivateMarbles(); }
    [OSCMethod] public void ActivateCube() { (TargetScript as BowlsManager).ActivateCube(); }
    [OSCMethod] public void ActivateSpiders() { (TargetScript as BowlsManager).ActivateSpiders(); }
}
