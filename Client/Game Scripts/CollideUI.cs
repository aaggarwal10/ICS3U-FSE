//This script tells the other programs where the mouse is
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollideUI : MonoBehaviour {
    public string MouseHovering = "";
    public string BuildHovering = "";
    public string SpawnHovering = "";
    public bool onMenu = false;
	// Use this for initialization
	public void mouseEnterUI(GameObject UIPan)//if mouse is in UI panel
    {
        MouseHovering = UIPan.name;
    }
    public void mouseExitUI()
    {
        MouseHovering = "";
    }
    public void mouseEnterBuildUI(GameObject UIPan)//if user in build menu
    {
        BuildHovering = UIPan.name;
    }
    public void mouseExitBuildUI()
    {
        BuildHovering = "";
    }
    public void mouseEnterBuildDetails(GameObject UnitBuild)//if user hovering over a unit
    {
        UnitBuild.GetComponent<Image>().color = new Color(1, 1, 1);
        SpawnHovering = UnitBuild.name;
    }
    public void mouseExitBuildDetails(GameObject UnitBuild)//if user stoped hovering on unit
    {
        
        UnitBuild.GetComponent<Image>().color = new Color(0.8F, 0.8F, 0.8F);
        SpawnHovering = "";
    }
    public void mouseEnterOnMenu()//if user entered the menu(build/unit)
    {
        onMenu = true;
    }
    public void mouseExitOnMenu()//if user not on menu(build/unit)
    {
        onMenu=false;
    }
}
