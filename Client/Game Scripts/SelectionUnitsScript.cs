//This script controls the unit movement, selection, and selling
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class SelectionUnitsScript : MonoBehaviour
{
    public GameObject originalPan;
    public GameObject selectionPan;
    // Use this for initialization
    public CollideUI collideMan;
    public DisplayStats displayInfo;
    public BuildUnitsScript buildMan;
    public Transform UnitFolder;
    public Vector3 clickedPos;
    public NetworkHandler networkMan;
    public Vector3 draggingPos;
    private Rect buttonRectangle;
    public List<string> selectedUnits = new List<string>();
    public List<string> unitNames = new List<string> { };
    public List<Vector3> unitPositions = new List<Vector3> { };
    private bool isSelecting = false;
    private string displayingInfo = "";
    public int playerIndex;
    public string selectedBuild;
    public Transform BulletFolder;
    public Transform modelFolder;
    //Redefine these in loop according to the unique ID when you're done testing
    public GameObject testingBullet;
    public GameObject spriteHolder;
    public string spawnatBuild;


    //Initiate list at start
    public static List<Color> colour_setup = new List<Color> { new Color(1, 0, 0), new Color(0, 1, 0 ),
        new Color(0, 0, 1), new Color(1, 1, 0), new Color(1, 0, 1), new Color(0, 1, 1)};
    public int current_transition = 0;
    public IEnumerator Lerp_movement(Transform item, Vector3 wanted_result, float time_required, int lerps)//move unit across the game
    {
        float speed = Vector3.Distance(item.position, wanted_result) / lerps;
        for (int a = 0; a < lerps; a++)
        {
            item.position = Vector3.MoveTowards(item.position, wanted_result, speed);
            yield return new WaitForSeconds(time_required / lerps);
        }
    }
    //Function for lerping through positions and moving the eunit towards a position while making it look good
    public IEnumerator Lerp2(Transform item_found, KeyValuePair<string, Vector3> new_item, Transform wanted_parent)
    {
        //int index = int.Parse(new_item.Key.Substring(0, 1));
        LineRenderer beam = item_found.GetComponent<LineRenderer>();
        var speed = Vector3.Distance(beam.GetPosition(0), new_item.Value) / 10;

        Vector3 current_pos = beam.GetPosition(0);
        for (int a = 0; a < 10; a++)//loop though different positions and move the unit along  the correct pat
        {
            current_pos = Vector3.MoveTowards(current_pos, new_item.Value, speed);
            beam.SetPosition(0, current_pos);
            yield return new WaitForSeconds(0.25f / 10);
        }
    }
    //General function for adding a child or updating it's position according to the name, pos, parent, and child.
    public void Update_item(KeyValuePair<string, Vector3> new_item, Transform wanted_parent, GameObject wanted_object)
    {
        Transform item_found = wanted_parent.Find(new_item.Key);
        current_transition++;
        if (!item_found) // item doesn't currently exist
        {
            GameObject unitObject = null;
            int index = int.Parse(new_item.Key.Substring(0, 1));

            //creating the object
            if (wanted_parent.name == "Units")
            {
                unitObject = Instantiate(wanted_object, wanted_parent);
                unitObject.transform.position = new_item.Value;
                foreach (Transform child in unitObject.transform)
                {
                    if (child.gameObject.name == "Recolor")//the parts that need to be recolored are called recolor
                    {
                        child.gameObject.GetComponent<Renderer>().material.color = colour_setup[index]; //set recolor part to wanted colors
                    }
                    child.gameObject.GetComponent<MeshCollider>().enabled = !child.gameObject.GetComponent<MeshCollider>().enabled;// only enable collider if object is placed other wise problems occur with raycasting
                }
                unitObject.name = new_item.Key;
            }
            else
            {
                unitObject = new GameObject(new_item.Key);
                LineRenderer beam = unitObject.AddComponent<LineRenderer>();
                beam.startColor = beam.endColor = colour_setup[index];
                beam.endWidth = beam.startWidth = 0.005f;
                beam.SetPosition(0, new_item.Value);
                unitObject.transform.parent = wanted_parent;
            }



        }
        else if (item_found.position != new_item.Value) // The object is at the wrong position
        {
            //Moving the object updating position list
            if (wanted_parent.name == "Units")
            {
                if (int.Parse(item_found.name.Substring(2, 3)) == 2)
                {
                    item_found.LookAt(new Vector3(new_item.Value.x, item_found.position.y, new_item.Value.z));
                    item_found.rotation *= Quaternion.Euler(0, 90, 0);
                }
                else if (int.Parse(item_found.name.Substring(2, 3)) == 5)
                {
                    item_found.LookAt(new Vector3(new_item.Value.x, item_found.position.y, new_item.Value.z));
                    item_found.rotation *= Quaternion.Euler(0, -90, 0);
                }
                StartCoroutine(Lerp_movement(item_found, new_item.Value, 0.2F, 20));
            }
            else
            {
                StartCoroutine(Lerp2(item_found, new_item, BulletFolder));
            }
        }
    }

    //Called directly from the network handler
    public void MoveMakeUnits(List<string> newunitNames, List<List<float>> newUnitPositions)
    {

        //Spawn or move existent units
        for (int i = 0; i < newunitNames.Count; i++)
        {
            KeyValuePair<string, Vector3> unit_spot = new KeyValuePair<string, Vector3>(newunitNames[i], new Vector3(newUnitPositions[i][0], newUnitPositions[i][1], newUnitPositions[i][2]));
            Debug.Log(newunitNames[i]);
            int index = int.Parse(newunitNames[i].Substring(2, 3));
            Debug.Log(index);
            string unitName = buildMan.UnitOrder[index];

            GameObject model_reference = modelFolder.Find(unitName).gameObject;

            Update_item(unit_spot, UnitFolder, model_reference);

            if (newUnitPositions[i][3] == 1) // Bullet exists
            {
                //update position of bullet
                KeyValuePair<string, Vector3> bullet_spot = new KeyValuePair<string, Vector3>(newunitNames[i], new Vector3(newUnitPositions[i][4], newUnitPositions[i][5], newUnitPositions[i][6]));
                Update_item(bullet_spot, BulletFolder, testingBullet);
            }
        }

        //Remove unit or bullet no longer exist

        for (int i = UnitFolder.childCount - 1; i >= 0; i--)
        {
            Transform current_child = UnitFolder.GetChild(i);
            GameObject unit_obj = current_child.gameObject;
            if (!newunitNames.Contains(unit_obj.name)) // The item isn't in the updated list!
            {
                //Removing the unit from the selection box if it is currently selected
                if (selectedUnits.Contains(unit_obj.name))
                {
                    selectedUnits.Remove(unit_obj.name);
                }

                //Remove the bullet the unit is shooting if it is shooting
                Transform bullet = BulletFolder.Find(unit_obj.name);
                if (bullet) //Bullet exists!
                {
                    Destroy(bullet.gameObject); //Kill the bullet
                }
                Destroy(unit_obj); // kill the unit
            }
            else
            {
                //Check if a bullet exists when it shouldn't
                int unit_ind = newunitNames.IndexOf(unit_obj.name);
                Transform current_bullet = BulletFolder.Find(unit_obj.name);
                if (current_bullet & newUnitPositions[unit_ind][3] == 0)
                {
                    Destroy(current_bullet.gameObject); // destroy the bullet!
                }
            }
        }
    }

    public void Update()
    {
        //Get Ray to start raycast
        Ray ray = Camera.main.ScreenPointToRay(draggingPos);
        RaycastHit hit;

        draggingPos = Input.mousePosition;// position of mouse dragging and in conjunction with is Selecting created a dragging state
        if (Input.GetMouseButtonDown(0) && !collideMan.onMenu)//if user clicks down on game scene is selecting becomes true and the initial position of mouse is set
        {
            isSelecting = true;
            clickedPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonDown(0) && collideMan.SpawnHovering != "")//Spawn a unit if user clicks down on panel to create it
        {
            Debug.Log("Spawn " + collideMan.SpawnHovering);
            Debug.Log(spawnatBuild);
            List<string> messageToSend = new List<string> { collideMan.SpawnHovering, spawnatBuild };
            networkMan.Send_message("Unit Adjustment", "Buy", networkMan.Encode_XML(messageToSend, typeof(List<string>)), "");
        }
        else if (Input.GetKeyDown(KeyCode.X) && displayInfo.Displaying != "" && int.Parse(displayInfo.Displaying.Substring(0, 1)) == playerIndex)//Sell Unit if X is clicked down
        {
            Debug.Log("Sell " + displayInfo.Displaying);
            networkMan.Send_message("Unit Adjustment", "Sell", networkMan.Encode_XML(new List<string> { displayInfo.Displaying }, typeof(List<string>)), "");
        }
        else if (Input.GetMouseButtonDown(0) && collideMan.MouseHovering != "")//Remove the unit from panel if he clicks on a unit that has already been selected in the panel
        {
            Debug.Log(1);
            selectedUnits.Remove(collideMan.MouseHovering);
            collideMan.MouseHovering = "";
        }

        else if (Input.GetMouseButtonUp(0))
        {

            if (draggingPos == clickedPos)// if the user did not move mouse and thus did not drag
            {
                if (Physics.Raycast(ray, out hit))//check if ray hit something
                {
                    Transform AdultObj = hit.collider.transform.parent;//As meshes were used the adult of the mesh component (ie. arm) should be used

                    if (AdultObj.parent == UnitFolder && int.Parse(AdultObj.name.Substring(0, 1)) == playerIndex)//if the parent is in unit folder and the object is yours
                    {

                        string unitName = buildMan.UnitOrder[int.Parse(AdultObj.name.Substring(2, 3))];//get the unit name from characters from the Unique Identifier that allows the Unit Order dictionary to distinguish which unit it is 
                        UnitInfo extraDetails = buildMan.UnitDetails[unitName];
                        if (extraDetails.unit_type == 0) // if item is unit
                        {
                            if (!selectedUnits.Contains(hit.collider.transform.parent.name))//if the user clicked on a unit, if they did not select it already, it becomes selected, otherwise it becomes unselected
                            {

                                selectedUnits.Add(hit.collider.transform.parent.name);
                            }

                            else
                            {
                                Debug.Log("single selection");
                                selectedUnits.Remove(hit.collider.transform.parent.name);
                            }
                        }
                        else if (!collideMan.onMenu)//if user clicks on building. selected units gets cleared and options pop up for what user can do with building.(i.e spawn units)
                        {

                            Debug.Log(2);
                            selectedUnits = new List<string> { };
                            selectedBuild = unitName;
                            spawnatBuild = AdultObj.name;
                            displayInfo.makeBuildDetails(spawnatBuild);
                        }
                    }
                    else if (selectedUnits.Count != 0 && !collideMan.onMenu)// if user clicks down on game scene and has some selected units, the units selected move toward the position that was clicked.
                    {
                        List<float> positionToMove = new List<float> { hit.point.x, hit.point.y, hit.point.z };
                        networkMan.Send_message("Unit Adjustment", "Move", networkMan.Encode_XML(selectedUnits, typeof(List<string>)), networkMan.Encode_XML(positionToMove, typeof(List<float>)));
                    }

                }
            }
            else
            {
                Debug.Log("box selection");
                //Box Selection - if user dragged mouse box selection is done
                selectedUnits = new List<string>();//selected units is cleared and all units that user owns within the square he dragged is selected instead
                foreach (Transform child in UnitFolder)
                {
                    string unitName = buildMan.UnitOrder[int.Parse(child.name.Substring(2, 3))];
                    UnitInfo extraDetails = buildMan.UnitDetails[unitName];
                    if (IsWithinSelectionBounds(child.gameObject) && int.Parse(child.name.Substring(0, 1)) == playerIndex && extraDetails.unit_type == 0)
                    {
                        if (!selectedUnits.Contains(child.name))
                        {
                            selectedUnits.Add(child.name);
                        }

                    }
                }



            }
            foreach (string item in selectedUnits)//make panels in selection panel ui for all units that were selected
            {

                if (selectionPan.transform.Find(item) == null)
                {
                    GameObject itemBox = Instantiate(originalPan, selectionPan.transform);
                    string unitName = buildMan.UnitOrder[int.Parse(item.Substring(2, 3))];
                    itemBox.GetComponent<Image>().sprite = spriteHolder.transform.Find(unitName).GetComponent<Image>().sprite;
                    itemBox.name = item;
                    itemBox.SetActive(true);
                }

            }
            isSelecting = false;//dragging becomes false after user stops clicking down
        }

        else if (collideMan.MouseHovering != "")//if user hover on unit in panel, display info of that unit
        {
            if (displayInfo.Displaying != collideMan.MouseHovering)
            {
                displayInfo.displayUnitPanel(collideMan.MouseHovering);
            }
        }
        else if (Physics.Raycast(ray, out hit))//if user hovers on unit in game scene display info
        {
            if (displayInfo.Displaying != hit.collider.transform.name)
            {

                if (hit.collider.transform.parent.parent == UnitFolder)
                {

                    displayInfo.displayUnitPanel(hit.collider.transform.parent.name);
                }
                else
                {
                    displayInfo.hideUnitPanel();
                }
            }
        }
        else if (collideMan.MouseHovering == "")//if user not hovering, hide info panel
        {
            displayInfo.hideUnitPanel();
        }
        foreach (Transform child in selectionPan.transform)//for all items in selection panel that were removed from the selected units, the selection panel must remove those as well
        {
            if (!selectedUnits.Contains(child.name))
            {
                GameObject.Destroy(child.gameObject);
            }
        }

    }
    void OnGUI()
    {
        if (isSelecting)//if dragging
        {
            // Create a rect from both mouse positions
            var rect = Utils.GetScreenRect(clickedPos, draggingPos);
            Utils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
            Utils.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
        }
    }
    public bool IsWithinSelectionBounds(GameObject gameObject)//given a game object checks if it is within the selection rect
    {
        if (!isSelecting)
            return false;

        var camera = Camera.main;
        var viewportBounds =
            Utils.GetViewportBounds(camera, clickedPos, draggingPos);

        return viewportBounds.Contains(
            camera.WorldToViewportPoint(gameObject.transform.position));
    }
}
public static class Utils
{
    static Texture2D _whiteTexture;
    public static Texture2D WhiteTexture//making white texture and border for rectangle selection box
    {
        get
        {
            if (_whiteTexture == null)
            {
                _whiteTexture = new Texture2D(1, 1);
                _whiteTexture.SetPixel(0, 0, Color.white);
                _whiteTexture.Apply();
            }

            return _whiteTexture;
        }
    }

    public static void DrawScreenRect(Rect rect, Color color)//draw the rect given a rect and a color
    {
        GUI.color = color;
        GUI.DrawTexture(rect, WhiteTexture);
        GUI.color = Color.white;
    }
    public static Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2)//get the rect between the point the user first clicked and then stopped clicking
    {
        // Move origin from bottom left to top left
        screenPosition1.y = Screen.height - screenPosition1.y;
        screenPosition2.y = Screen.height - screenPosition2.y;
        // Calculate corners
        var topLeft = Vector3.Min(screenPosition1, screenPosition2);
        var bottomRight = Vector3.Max(screenPosition1, screenPosition2);
        // Create Rect
        return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
    }
    public static void DrawScreenRectBorder(Rect rect, float thickness, Color color)//draw border using rects.
    {
        // Top
        Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
        // Left
        Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
        // Right
        Utils.DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
        // Bottom
        Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
    }
    public static Bounds GetViewportBounds(Camera camera, Vector3 screenPosition1, Vector3 screenPosition2)//get bounds of the rect to check if object is inside it
    {
        var v1 = Camera.main.ScreenToViewportPoint(screenPosition1);
        var v2 = Camera.main.ScreenToViewportPoint(screenPosition2);
        var min = Vector3.Min(v1, v2);
        var max = Vector3.Max(v1, v2);
        min.z = camera.nearClipPlane;
        max.z = camera.farClipPlane;

        var bounds = new Bounds();
        bounds.SetMinMax(min, max);
        return bounds;
    }

}
