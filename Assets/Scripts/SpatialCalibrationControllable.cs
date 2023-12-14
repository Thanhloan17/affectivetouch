using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpatialCalibrationControllable : Controllable
{
    [OSCMethod]
    public void Calibrate() {
        (TargetScript as SpatialCalibration).Calibrate();
    }
}
