//This script is for making the global chat system in the menu.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatBox : MonoBehaviour
{

    public GameObject messageText;
    public GameObject chatPanel;
    public InputField chatBox;
    public NetworkHandler networkman;
    public void MakeChat(List<string> messages)//takes a list of messages and makes a chat from it
    {
        foreach (Transform child in chatPanel.transform)//destroy the previous messages to start making new chat
        {
            GameObject.Destroy(child.gameObject);
        }
        foreach (string item in messages)//loop through messages and make a new message in chat box
        {
            Text mes = messageText.GetComponent<Text>();
            mes.text = mes.text = item;
            if (mes.text.Substring(0,2) == "  ")//if two spaces are in from set color of font to gree
            {
                mes.color = new Color(0f,1f,0f,1f);
                mes.text = mes.text.Substring(2, mes.text.Length-2);
            }
            else if(mes.text[0] == ' ')//one space --> red
            {
                mes.color = new Color(1f, 0f, 0f, 1f);
                mes.text = mes.text.Substring(1, mes.text.Length - 1);
            }
            else
            {
                mes.color = new Color(1f, 1f, 1f, 1f);// otherwise --> black
           
            }
            Debug.Log(item);
            GameObject textObject = Instantiate(messageText, chatPanel.transform);//make the text
            textObject.SetActive(true);//make it visible to vieweers
        }
    }
    public void Update()
    {
        if (chatBox.text != "")
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                networkman.Send_message("Global Chat", chatBox.text, null, null);//if enter is clicked send message to server that a new message has been sent
                chatBox.text = "";
            }
        }

    }
}
