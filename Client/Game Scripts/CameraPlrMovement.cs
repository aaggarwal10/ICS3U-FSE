//this script allows users to move their camera to see different parts of the map. I also allows users to see other character's positions.
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class CameraPlrMovement : MonoBehaviour
{
    public Camera camera;//Camera to get the mouse point to ray from camera
    public GameObject player;//change position
    public NetworkHandler networkMan;
    public SelectionUnitsScript selectMan;
    public GameObject playerFolder;
    public GameObject origPlayer;
    public float last_update = 0;
    public InputField chatfieldINF;
    public float dist;//determine going back or front , by setting to zero nothing moves
    public static List<Color> colour_setup = new List<Color> { new Color(1, 0, 0), new Color(0, 1, 0 ),
        new Color(0, 0, 1), new Color(1, 1, 0), new Color(1, 0, 1), new Color(0, 1, 1)};
    void Start()
    {
        dist = 0;//doesnt move at start
    }
    private void Update()
    {

        Ray ray = camera.ScreenPointToRay(Input.mousePosition);//makes a ray to move along
        Vector3 dir = ray.direction;//gets the direction the ray is going to so that the perpendicular normal ray can bbe taken to get the straffe ray.
        Vector3 left =  Vector3.Cross(dir, Vector3.up).normalized;
        Vector3 right = -left;//going right vector that is added just opposite of left
        Vector3 previousPos = player.transform.position;//get reference for previous position so that if the new position does not fit inside the bounds, the old position cen be used
        Vector3 bottomCornerBounds = new Vector3(550, 940, 740);//min bound
        Vector3 topCornerBounds = new Vector3(1240, 1000, 1365);//max bound
        if (!chatfieldINF.isFocused)
        {
            if (Input.GetKey(KeyCode.W))//W - Forward 1 Unit
            {
                dist = 1;
                player.transform.position = ray.GetPoint(dist);
            }
            else if (Input.GetKey(KeyCode.S))
            {//S - Backward 1 Unit
                dist = -1;
                player.transform.position = ray.GetPoint(dist);
            }
            if (Input.GetKey(KeyCode.D))//D - Right 1 unit
            {
                player.transform.position = player.transform.position + right;
            }
            else if (Input.GetKey(KeyCode.A))//A- Left on unit
            {
                player.transform.position = player.transform.position + left;
            }
        }
        if (!PositionIsInBox(player.transform.localPosition, bottomCornerBounds, topCornerBounds))// check if new position is not in box, if so set position back to previous
        {
            player.transform.position = previousPos;
        }
        //Send cam position
        if (Time.realtimeSinceStartup - last_update > 1)
        {
            List<float> camPos = new List<float> { player.transform.position.x, player.transform.position.y, player.transform.position.z };
            networkMan.Send_message("Camera", networkMan.Encode_XML(camPos, typeof(List<float>)), null, null);
            last_update = Time.realtimeSinceStartup;
        }
    }
    //740, 1020,605
    //1435,1050,1230
    public bool PositionIsInBox(Vector3 positionToCheck, Vector3 bottomCorner, Vector3 topCorner)
    {
        Vector3 center = (bottomCorner + topCorner) / 2;//center of cube
        Vector3 size = topCorner - bottomCorner;// get the extents or sizes of cube to make bounds
        size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
        Bounds bounds = new Bounds(center, size);
        return bounds.Contains(positionToCheck);//return if point in bounds
    }
    public void drawOtherPlayer(List<List<float>> cameraPos)//draw other player circles
    {
        int myPlayer = selectMan.playerIndex;//my index
        foreach (Transform child in playerFolder.transform)//destroy the units seen before
        {
            GameObject.Destroy(child.gameObject);
        }
        for (int i =0; i<cameraPos.Count; i++)//loop through the cameras
        {
            
            if (i != myPlayer)//if player not me make their ball and change its color to represent them also move the ball
            {
                GameObject playerObj = Instantiate(origPlayer, playerFolder.transform);
                playerObj.GetComponent<Renderer>().material.color = colour_setup[i];
                playerObj.transform.GetChild(0).gameObject.GetComponent<ParticleSystemRenderer>().material.color = colour_setup[i];
                playerObj.transform.position = new Vector3(cameraPos[i][0], cameraPos[i][1], cameraPos[i][2]);
                playerObj.SetActive(true);
            }
            else//else just change color
            {
                Debug.Log(i);
                Debug.Log(selectMan.playerIndex);
                player.GetComponent<Renderer>().material.color = colour_setup[i];
            }
        }
    }
    
}
