//this script shows the rotating object in the background as the camera rotates around the object
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCamRotate : MonoBehaviour
{
    public Transform CAM;

    void Update()
    {
        transform.Translate(new Vector3(50, 0, 0) * Time.deltaTime);//move camera right
        transform.LookAt(CAM);//make camera look at object

    }

}
