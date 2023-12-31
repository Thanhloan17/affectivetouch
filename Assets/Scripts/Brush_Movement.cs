using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brush_Movement : MonoBehaviour
{
    public Transform startPoint; 
    public Transform endPoint;
    float duration_of_wave = 3.0f;

    private float timer = 0.0f;
    private bool movingForward = false;

    private void Update()
    {
        // if the button on the Unity scrin is clicked then move toward the finish position
        if (movingForward)
        {
            timer += Time.deltaTime;
            
            float t = timer / duration_of_wave; //calculate the speed of the movement from startPoint to finishPoint based on the total duration of the movement is seconds 
           
            transform.position = Vector3.Lerp(startPoint.position, endPoint.position, t);
        }

        //when the brush is in the correct place turned of the movement and reset the timer 
        if (timer >= duration_of_wave)
        {
            timer = 0.0f;
            movingForward = false;
        }
    }

    // method that is played by the button on the Unity scene. It starts the brush movement
    public void StartForwardMovement()
    {
        movingForward = true;
    }

    // the information about the speed of the brush movement is read from the .csv file during the experiment
    public void UpdateSpeed(float visualSpeed)
    {
        duration_of_wave = visualSpeed/6;

    }

}

/* Version 2 - the brush is going from startPoint to finishPoint and back

public class Brush_Movement : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    private bool isWaiting = false;
    private bool isMoving = false;
    public float duration_of_wave;
   
    private float waitDuration = 3.0f;
   

    private void Update()
    {
        if (isWaiting)
        {
            Wait();
        }
        else
        {
            Move();
        }
    }

    private void Move()
    {
        if (isMoving)
        {
            
            Vector3 targetPosition = endPoint.position;
            float distance = Vector3.Distance(startPoint.position, endPoint.position);

            float currentSpeed = distance  / (duration_of_wave );

            float step = currentSpeed * Time.deltaTime ;

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                isWaiting = true;
                isMoving = false;
            }
        }
    }

    private void Wait()
    {
        waitDuration -= Time.deltaTime;

        if (waitDuration <= 0.0f)
        {
            waitDuration = 3.0f;
            isWaiting = false;
        }
    }

    public void StartForwardMovement()
    {
        transform.position = startPoint.position;
     
        isWaiting = false;
        isMoving = true;
    }


}
*/
