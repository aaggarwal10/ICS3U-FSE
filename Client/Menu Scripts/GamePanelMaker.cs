// this script make the list of rooms that users can join
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePanelMaker : MonoBehaviour {

    public GameObject gameGrid;
    public GameObject original;
    public void makeGameMenu(List<List<string>> open_rooms)
    {
        foreach (Transform child in gameGrid.transform)//destroy the previous rooms
        {
            GameObject.Destroy(child.gameObject);
        }
        foreach (List<string> items in open_rooms)//remake the rooms based of the room list
        {
            GameObject clone;
            clone = Instantiate(original, gameGrid.transform);
            clone.SetActive(true);
            clone.transform.GetChild(0).GetComponent<Text>().text = items[3]+"/6";//number of players
            clone.transform.GetChild(1).GetComponent<Text>().text = items[0];//Room Name
            clone.transform.GetChild(2).GetComponent<Text>().text = items[2];//Creator Name
            clone.transform.GetChild(3).GetComponent<Text>().text = items[4];//Game Type
            if (items[1] == "")
            {
                clone.transform.GetChild(4).gameObject.SetActive(false); //if room has password show lock icon
            } 
        }
    }
}
