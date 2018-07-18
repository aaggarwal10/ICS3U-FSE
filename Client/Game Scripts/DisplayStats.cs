//this script displays the stats of units in the game
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DisplayStats : MonoBehaviour {

    public GameObject statPanel;
    public GameObject buildstatPanel;
    public GameObject buildDetailsPanel;
    public BuildUnitsScript buildMan;
    public GameObject spriteHolder;
    public GameObject originalPan;
    public GameObject contentPan;
    public GameObject gameStatPanel;
    public ToggleBuildUnits toggleMenu;
    public SelectionUnitsScript selectMan;
    public NetworkHandler networkman;
    public string Displaying;
    public string DisplayingBuild;
	// Use this for initialization
    public void displayUnitPanel(string unit_id)
    {
        string type;
        Displaying = unit_id;
        string unitName = buildMan.UnitOrder[int.Parse(unit_id.Substring(2, 3))];
        UnitInfo extraDetails = buildMan.UnitDetails[unitName];
        Slider health = statPanel.transform.Find("Slider").GetComponent<Slider>();
        health.value = ((float)networkman.healthDictionary[unit_id])/((float)extraDetails.MaxHp);
        Text item = statPanel.transform.Find("UnitName").GetComponent<Text>();
        statPanel.transform.Find("DPS").GetComponent<Text>().text = "DPS: "+extraDetails.damage.ToString();
        if (extraDetails.can_float)
        {
            type = "Air";
        }
        else
        {
            type = "Ground";
        }
        statPanel.transform.Find("Type").GetComponent<Text>().text = "Type: " + type;
        statPanel.transform.Find("Range").GetComponent<Text>().text = "Range: " + extraDetails.shot_distance.ToString();
        statPanel.transform.Find("Image").GetComponent<Image>().sprite = spriteHolder.transform.Find(unitName).GetComponent<Image>().sprite;
        statPanel.transform.Find("Sell For").GetComponent<Button>().transform.Find("Text").GetComponent<Text>().text = "Sell For $" + ((int)(extraDetails.cost * health.value / 2)).ToString() + " (X)";
        item.text = unitName;
        statPanel.SetActive(true);
    }
    public void hideUnitPanel()
    {

        Displaying = "";
        statPanel.SetActive(false);
    }
    public void displayBuildPanel(string unit_name)// Hover over items in shop
    {
        string type;
        Displaying = unit_name;
        UnitInfo extraDetails = buildMan.UnitDetails[unit_name];
        Text item = buildstatPanel.transform.Find("BuildUnit").GetComponent<Text>();
        buildstatPanel.transform.Find("DPS").GetComponent<Text>().text = "DPS: "+extraDetails.damage.ToString();
        buildstatPanel.transform.Find("Range").GetComponent<Text>().text = "Range: " + extraDetails.shot_distance.ToString();
        if (extraDetails.can_float)
        {
            type = "Air";
        }
        else
        {
            type = "Ground";
        }
        buildstatPanel.transform.Find("Type").GetComponent<Text>().text = "Type: " + type;
        buildstatPanel.transform.Find("Image").GetComponent<Image>().sprite = spriteHolder.transform.Find(unit_name).GetComponent<Image>().sprite;
        item.text = unit_name;
        buildstatPanel.transform.Find("Buy For").GetComponent<Button>().transform.Find("Text").GetComponent<Text>().text = "Buy For $" + extraDetails.cost.ToString();
        buildstatPanel.SetActive(true);
    }
    public void hideBuildPanel()
    {
        DisplayingBuild = "";
        buildstatPanel.SetActive(false);
    }
    public void makeBuildDetails(string buildingName)
    {

        string unitName = buildMan.UnitOrder[int.Parse(buildingName.Substring(2, 3))];
        UnitInfo extraDetails = buildMan.UnitDetails[unitName];
        Text buildNam = buildDetailsPanel.transform.Find("buildName").GetComponent<Text>();
        buildNam.text = unitName;
        List<string> spawnUnits = buildMan.UnitDetails[unitName].can_spawn;
        foreach (Transform child in contentPan.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        foreach(string item in spawnUnits)
        {
            GameObject itemBox = Instantiate(originalPan, contentPan.transform);
            Image bImage = itemBox.transform.Find("UnitImage").GetComponent<Image>();
            Image tImage = spriteHolder.transform.Find(item).GetComponent<Image>();
            bImage.sprite = tImage.sprite;
            itemBox.transform.Find("UnitName").GetComponent<Text>().text = item;
            itemBox.transform.Find("Cost").GetComponent<Text>().text = "$" + buildMan.UnitDetails[item].cost;
            itemBox.name = item;
            itemBox.SetActive(true);
        }
        Slider Health = buildDetailsPanel.transform.Find("Health").GetComponent<Slider>();
        Health.value = (float)networkman.healthDictionary[buildingName] / (float)extraDetails.MaxHp;
        Image buildIm = buildDetailsPanel.transform.Find("Image").GetComponent<Image>();
        buildIm.sprite = spriteHolder.transform.Find(unitName).GetComponent<Image>().sprite;
        buildDetailsPanel.SetActive(true);
        toggleMenu.LeaveBoth();


    }
    public void hidebuildDetails()
    {
        buildDetailsPanel.SetActive(false);
        selectMan.spawnatBuild = "";
    }
    
    public void displayGameStats(int cash, int income,int max_units, int max_buildings, int units,int building)
    {
        gameStatPanel.transform.Find("Cash").GetComponent<Text>().text = "Cash: $" + cash.ToString();
        gameStatPanel.transform.Find("Income").GetComponent<Text>().text = "Income: $" + income.ToString();
        gameStatPanel.transform.Find("Units").GetComponent<Text>().text = "Units: "+units.ToString()+"/" + max_units.ToString();
        gameStatPanel.transform.Find("Buildings").GetComponent<Text>().text = "Builds: " + building.ToString() + "/" + max_buildings.ToString();

    }
}
