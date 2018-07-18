//this script controls camera rotation allows users to rotate their view
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[AddComponentMenu("Camera-Control/Mouse Look")]
public class MouseLook : MonoBehaviour
{
    
    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;//get the axes for rotation
    //setting mouse sensitivity
    public float sensitivityX = 15F;
    public float sensitivityY = 15F;

    //set bounds so that the user cannot roll over and allows for better gameplay
    public float minimumX = -360F;
    public float maximumX = 360F;

    public float minimumY = -60F;
    public float maximumY = 60F;
    //--------------------------------------------------------------------------
    float rotationY = 0F;//starts off at zero and rotates

    void Update()
    {
        if (Input.GetMouseButton(1))//if button is clicked
        {
            if (axes == RotationAxes.MouseXAndY)//if mouse movement moves both in x and y get the rotation of x and rotation of y 
            {
                float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;

                rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
                rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

                transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
            }
            else if (axes == RotationAxes.MouseX)//if only rotate x, rotate around the x axixs
            {
                transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityX, 0);
            }
            else//rotater around y
            {
                rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
                rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

                transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
            }
        }
    }

}
