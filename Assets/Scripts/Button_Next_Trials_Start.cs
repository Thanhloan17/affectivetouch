using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button_Next_Trials_Start : MonoBehaviour
{
    public void Change_Trial() 
        {

        //connect to all objects in the scene that have an AudioSamplesGenerator script
        HapticColliderTrigger_1[] actuators = FindObjectsOfType<HapticColliderTrigger_1>();

            foreach (HapticColliderTrigger_1 actuator in actuators)
            {
            //print(actuator.name);
            //generator.trial_numb += 1; // Increment the trial_numb variable by 1
            actuator.Detection();
            }

        //connect to all objects in the scene that have an AudioSamplesGenerator script
        AudioSamplesGenerator[] generators = FindObjectsOfType<AudioSamplesGenerator>();

        foreach (AudioSamplesGenerator generator in generators)
        {
            //generator.trial_numb += 1; // Increment the trial_numb variable by 1
            generator.NextTrial();
        }
    }

}
