//This script allows users to join a queue and leave a queue and it displays who is in a que
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QueueManager: MonoBehaviour
{
    public NetworkHandler networkManager;
    public GameMaker gameMan;
    public AccountManager accountMan;
    public GameObject playerGrid;
    public GameObject original;
    // Use this for initialization
    public void playerJoin(List<List<string>> players)//displays the list of players
    {
        foreach (Transform child in playerGrid.transform)//destroy anyother objects that were in parent
        {
            GameObject.Destroy(child.gameObject);
        }
        foreach (List<string> items in players)//loop through player list and make a bar for each player that displays their name
        {
            GameObject clone;
            clone = Instantiate(original, playerGrid.transform);
            clone.SetActive(true);
            clone.transform.GetChild(0).GetComponent<Text>().text = items[0];
            clone.transform.GetChild(1).GetComponent<Image>().sprite = accountMan.profilePicDict[items[1]];
        }
    }
    public void PlayerLeave()//if player leaves room
    {
        networkManager.Send_message("Leave Room", null, null, null);//send message to server
        gameMan.GoToGameMenu();//leave
    }
    public void GoToQueue(List<List<string>> playersInQueue)//if user wants to go to que
    {
        gameMan.GotoQueue();//go to queue
        playerJoin(playersInQueue);//join queue
    }
}
