//this script allows users
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildUnitsScript : MonoBehaviour {
    public List<string> UnitOrder = new List<string> { null, "Command Center", "Light Soldier", "Heavy Soldier", "Power Plant" ,"Plane","House","Head Quarters"};
    public Dictionary<string, UnitInfo> UnitDetails = new Dictionary<string, UnitInfo> {
        {"Command Center", new UnitInfo{ MaxHp = 800, bullet_type = "Turret", can_walk = true,  unit_type = 1, spawn_delay = 10f, spawn_hover = 2, housing_change = 1,
            shoot_debounce = 4, damage = 40, shot_distance = 30, can_spawn = new List<string> { "Light Soldier","Heavy Soldier","Plane" }, space_x = 6, space_z = 6, special_char = 1, cost = 150 }  },
        {"Light Soldier", new UnitInfo{ MaxHp = 60, bullet_type = "Bullet", can_walk = true, damage = 10, shoot_debounce = 2, walk_speed = 2.5f, shot_distance = 15,
            special_char = 2, spawn_delay = 3f, cost = 20, unit_space = 1, spawn_hover = 1} },
        {"Heavy Soldier", new UnitInfo{ MaxHp = 100, bullet_type = "Bullet", can_walk = true, damage = 20, shoot_debounce = 2, walk_speed = 2.5f, shot_distance = 20,
            special_char = 3, spawn_delay = 2.5f, cost = 30, unit_space = 1, spawn_hover = 1 } },
        {"Power Plant", new UnitInfo{ MaxHp = 250, can_walk = true, spawn_delay = 1f, space_x = 5, space_z = 5, unit_type = 1, income_change = 10,
            needs_energy = true, special_char = 4, cost = 100, spawn_hover = 2 } },
        { "Plane", new UnitInfo{ MaxHp = 200, bullet_type = "Bullet", can_walk = true, damage = 50, shoot_debounce = 2, walk_speed = 4.5f, shot_distance = 25,
            special_char = 5, spawn_delay = 5.5f, cost = 400, unit_space = 1, spawn_hover = 1, can_float = true} },
        { "House", new UnitInfo{ MaxHp = 100, can_walk = true,  unit_type = 1, spawn_delay = 10f, spawn_hover = 1, space_x = 3, space_z = 3,
            special_char = 7, cost = 150, housing_change = 3 } },
        { "Head Quarters", new UnitInfo{ MaxHp = 1500, bullet_type = "Turret", can_walk = true,  unit_type = 1, spawn_delay = 10f, spawn_hover = 3,
            shoot_debounce = 4, damage = 75, shot_distance = 45, space_x = 6, space_z = 6, special_char = 6, cost = 650 }  } };


    public GameObject blankButton;
    public GameObject buildPan;
    public CollideUI collideMan;
    public DisplayStats displayInfo;
    public GameObject buildObject = null;
    public Transform ModelUnits;
    public Transform EnergyCrys;
    public Material validMat;
    public NetworkHandler networkMan;
    public Material invalidMat;
    public GameObject spriteHolder;
    public bool isValid;
    public string Building = "";
    // Use this for initialization
    void Start () {//make build menu with each unit so that the building can create the unit it needs
        foreach (KeyValuePair<string, UnitInfo> unitDetail in UnitDetails)//Making Build Menu
        {
            if (unitDetail.Value.unit_type == 1)
            {
                GameObject itemBox = Instantiate(blankButton, buildPan.transform);
                
                Image blankIm = itemBox.transform.GetComponent<Image>();

                Image changeIm = spriteHolder.transform.Find(unitDetail.Key).GetComponent<Image>();
                blankIm.sprite = changeIm.sprite;
            
                
                itemBox.name = unitDetail.Key;
                itemBox.SetActive(true);
            }
        }

    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetMouseButtonDown(0) && collideMan.BuildHovering != "" && buildObject == null)//when user clicks on a building to build attach it to the mouse so user can place building
        {
            Building = collideMan.BuildHovering;
            GameObject build = ModelUnits.Find(collideMan.BuildHovering).gameObject;
            buildObject = Instantiate(build);
            buildObject.transform.name = collideMan.BuildHovering;

        }
        else if(Input.GetMouseButtonDown(0) && collideMan.BuildHovering != "" && buildObject!= null && collideMan.BuildHovering != Building)//user chooses new building
        {
            GameObject.Destroy(buildObject);
            Building = collideMan.BuildHovering;
            GameObject build = ModelUnits.Find(collideMan.BuildHovering).gameObject;
            buildObject = Instantiate(build);

        }
        else if (Input.GetMouseButtonDown(0) && buildObject!= null)//user tries building a building
        {
            if (isValid)//build the building if the placement is valid
            {
                //Send message to make building with certain position
                List<float> positionofBuild = new List<float> { buildObject.transform.position.x, buildObject.transform.position.y, buildObject.transform.position.z };
                networkMan.Send_message("Unit Adjustment", "Build",networkMan.Encode_XML(new List<string> { Building },typeof(List<string>)),networkMan.Encode_XML(positionofBuild,typeof(List<float>)));
                GameObject.Destroy(buildObject);
                buildObject = null;
                Building = "";
                isValid = false;
                
            }
        }
        else if (Input.GetMouseButtonDown(1) && buildObject != null)//if user right clicks destroy the object and deselect the building
        {
            isValid = false;
            Building = "";
            GameObject.Destroy(buildObject);
            buildObject = null;
        }
        else if (collideMan.BuildHovering != "")//display the info of a building if user is hovering over it in build panel
        {
            if (displayInfo.Displaying != collideMan.BuildHovering)
            {
                displayInfo.displayBuildPanel(collideMan.BuildHovering);
            }
        }
        else//user if hovering with building
        {
            displayInfo.hideBuildPanel();//hide display info
            if (Building != "")// if building
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);//get point user is trying to build
                if (Physics.Raycast(ray, out hit)  && buildObject!=null)
                {
                    buildObject.transform.position = hit.point;//move position to the point user is trying to build
                    isValid = false//set valid to false and then make it true if position is valid
                    
                    if (buildObject.transform.GetChild(0).GetComponent<Renderer>().material != invalidMat)//make building have have material that is invalid
                    {
                        foreach (Transform chil in buildObject.transform)
                        {
                                chil.GetComponent<Renderer>().material = invalidMat;
                        }
                    }
                    
                    foreach (Transform child in EnergyCrys)/if building needs energy and building position is near energy crystal make material green
                    {
                        if (UnitDetails[Building].needs_energy)
                        {
                            if (Vector3.Distance(child.position, hit.point) < 4)
                            {
                                isValid = true;
                                if (buildObject.transform.GetChild(0).GetComponent<Renderer>().material != validMat)
                                {
                                    foreach (Transform chil in buildObject.transform)
                                    {
                                        chil.GetComponent<Renderer>().material = validMat;
                                    }
                                }
                            }
                        }
                        else if(hit.collider.name!="Plane")//if building does not need energy but is on Plane, make material green in color
                        {
                            isValid = true;
                            if (buildObject.transform.GetChild(0).GetComponent<Renderer>().material != validMat)
                            {
                                foreach (Transform chil in buildObject.transform)
                                {
                                    chil.GetComponent<Renderer>().material = validMat;
                                }
                            }
                        }
                    }
                    
                }
            }
            
        }
    }
}
public class UnitInfo
{
    //Needed info
    public int MaxHp;
    public int special_char;
    public float spawn_delay;
    public float spawn_hover; // how much above ground they spawn
    public int cost;

    //Needed Info, shortened by having defaults
    public float walk_speed = 0;
    public int shot_distance = -1;
    public Dictionary<string, int> heal_distance = new Dictionary<string, int> { { "Troops", -1 }, { "Planes", -1 }, { "Buildings", -1 } };

    public List<string> can_spawn = new List<string> { };
    public int income_change = 0;
    public int housing_change = 0;

    public bool can_float = false;
    public bool can_walk = false;
    public bool needs_energy = false;

    //Info needed if default has been switched
    public int shoot_debounce;
    public float shot_speed;
    public int damage;
    public int heal_debounce;
    public string bullet_type;
    public int unit_space;

    public int unit_type = 0; // 1 is building, 0 is unit
    public int space_x = 4;
    public int space_z = 4;



    //Extra info
    public List<string> special_effects = new List<string> { };
}
