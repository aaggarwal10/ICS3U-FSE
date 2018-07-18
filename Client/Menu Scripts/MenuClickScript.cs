#This script allows users to change between menus (Account, Store, and Play)
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
public class MenuClickScript : MonoBehaviour {
    public GameObject accountMenu;
    public GameObject playMenu;
    public GameObject storeMenu;
    public GameObject userMessagePanel;
    private static bool signedInVar;
    
    void Update()//if user is signed in remove the message panel
    {
        signedInVar = Register.signedIn;
        if (signedInVar)
        {
            userMessagePanel.SetActive(false);
        }
    }
    public void AccountMenuSwitch()//move to account menu
    {
        if (signedInVar)
        {
            accountMenu.SetActive(true);
            playMenu.SetActive(false);
            storeMenu.SetActive(false);
        }
    }
    public void PlayMenuSwitch()//move to play/game menu
    {
        if (signedInVar)
        {
            accountMenu.SetActive(false);
            playMenu.SetActive(true);
            storeMenu.SetActive(false);
        }
    }
    public void StoreMenuSwitch()//move to store
    {
        if (signedInVar)
        {
            accountMenu.SetActive(false);
            playMenu.SetActive(false);
            storeMenu.SetActive(true);
        }
    }
    public void UserMessage(string message)//send message to user about sign in / login problems
    {
        userMessagePanel.SetActive(true);
        userMessagePanel.transform.GetChild(0).GetComponent<Text>().text = message;

    }
    public void hideUserMessage()//hide user message
    {
        userMessagePanel.SetActive(false);
    }
}

