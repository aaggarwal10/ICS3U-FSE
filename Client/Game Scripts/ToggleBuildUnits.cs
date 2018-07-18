//this script allows users to toggle between building buildings and controlling/selecting units
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleBuildUnits : MonoBehaviour {
    public bool isBuilding = false;//set building and controlling to false
    public bool isControlling = false;
    //get objects that allow user to toggle inbetween build and unit panels in game
    public Button buildButton, unitButton;
    public GameObject buildPan, unitPan;
    public Text buildText, unitText;
    public DisplayStats displayMan;
	// U--
	public void GoToBuilds()//toggle to build panel
    {
        var colors = buildButton.GetComponent<Button>().colors;//color backgriund to turquoise
        colors.normalColor = new Color (0,1,1);
        buildButton.GetComponent<Button>().colors = colors;
        colors = unitButton.GetComponent<Button>().colors;
        colors.normalColor = new Color(0, 0, 0);//change other button to black
        unitButton.GetComponent<Button>().colors = colors;
        buildText.color = new Color(0, 0, 0);//switch font color to black
        unitText.color = new Color(0, 1, 1);//other -->turquoise
        buildPan.SetActive(true);
        unitPan.SetActive(false);
        displayMan.hidebuildDetails();
    }
    public void GoToUnits()//extac opposite of toggle to build Unit<-->Build
    {
        var colors = unitButton.GetComponent<Button>().colors;
        colors.normalColor = new Color(0, 1, 1);
        unitButton.GetComponent<Button>().colors = colors;
        colors = buildButton.GetComponent<Button>().colors;
        colors.normalColor = new Color(0, 0, 0);
        buildButton.GetComponent<Button>().colors = colors;
        unitText.color = new Color(0, 0, 0);
        buildText.color = new Color(0, 1, 1);
        unitPan.SetActive(true);
        buildPan.SetActive(false);
        displayMan.hidebuildDetails();
    }
    public void LeaveBoth()//leave both panels
    {
        buildPan.SetActive(false);//do not allow users to see either
        unitPan.SetActive(false);
    }
}
