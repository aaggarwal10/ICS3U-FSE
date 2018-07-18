//this script controls the chat system that is in the game
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GameChat : MonoBehaviour
{
    public string chatMode = "Global";//Starts off as global
    public Dictionary<string, List<string>> gameChatMes = new Dictionary<string, List<string>> { { "Team", new List<string> { } }, { "Global", new List<string> { } } };//dictionary that holds the messages for both global and team
    public Text global, team; // edit color componennts 
    public GameObject contentPanel;//parent for instantiating messages
    public GameObject originalText;//object to clone for meassges
    public InputField chatBoxIF;
    public NetworkHandler networkMan;//networkMan for sending messages to server and receiving messages
    // Use this for initialization
    public void globalChange()//Changes mode to global makes new game chat and changed color scheme of buttons
    {
        chatMode = "Global";
        global.color = Color.white;
        team.color  = Color.black;
        MakeGameChat(gameChatMes[chatMode]);
    }
    public void teamChange()//Reverse of global change
    {
        chatMode = "Team";
        team.color = Color.white;
        global.color = Color.black;
        MakeGameChat(gameChatMes[chatMode]);
    }
    public void MakeGameChat(List<string> messages)//Get a list of strings
    {
        foreach (Transform child in contentPanel.transform)//destroy all messages
        {
            GameObject.Destroy(child.gameObject);
        }
        if (chatMode == "Global")//if chat is global check for leading space that will allow for color to change to red
        {
            foreach (string item in messages)
            {
                Text mes = originalText.GetComponent<Text>();
                mes.text = item;
                if (mes.text.Substring(0, 1) == " ")
                {
                    mes.color = new Color(1f, 0f, 0f, 1f);
                    mes.text = mes.text.Substring(1, mes.text.Length - 1);
                }
                GameObject textObject = Instantiate(originalText, contentPanel.transform);// make message (automatically spaced with formatter in hierarchy)
                textObject.SetActive(true);//set text to be visible
            }
        }
        else//if team do not worry about making text red
        {
            foreach (string item in messages)
            {
                Text mes = originalText.GetComponent<Text>();
                mes.text = item;
                GameObject textObject = Instantiate(originalText, contentPanel.transform);
                textObject.SetActive(true);
            }
        }
    }
    // Update is called once per frame

    void Update()//if there is text in the input field and user clicks enter send message
    {
        
        if (Input.GetKeyDown(KeyCode.Return) && chatBoxIF.text != "")
        {
            Debug.Log(1);
            networkMan.Send_message("Game Chat", chatBoxIF.text, chatMode, null);
            chatBoxIF.text = "";
            
        }
    }
}
