//this is the script that runs when a user clicks join game
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JoinGame : MonoBehaviour {
    public GameMaker gameMan;
    public NetworkHandler networkMan;
    public GameObject enterPass;
    public InputField passwordField;
    private bool enteringPassword = false;
    public string Game;
    public void OnClicked(Button button)//if clicked
    {
        if (!enteringPassword)//if user is not entering password
        {
            Game = button.transform.parent.GetChild(1).GetComponent<Text>().text;//get game name
            bool hasPassword = button.transform.parent.GetChild(4).gameObject.activeSelf;//does game have password
            if (hasPassword)//if game has a password allow user to enter password
            {
                EnterPass(Game);
            }
            else//other wise send them to queue
            {
                //send to server

                enteringPassword = false;
                gameMan.GoToGameMenu();
                networkMan.Send_message("Join Room", Game, "", null);
                Debug.Log(Game);
                Debug.Log("Joining Room - No Lock");

            }
        }
    }
    
    public void EnterPass(string gameName)//set enter password panel to be active
    {
        enterPass.SetActive(true);
        enteringPassword = true;
    }

    public void EnterPassword()//user entered correct password so send user to game
    {
        Debug.Log("Joining Room - Locked");
        //send to server
        
        enterPass.SetActive(false);
        enteringPassword = false;
        gameMan.GoToGameMenu();
        Debug.Log(passwordField.text);
        networkMan.Send_message("Join Room", Game, passwordField.text, null);
        passwordField.text = "";

    }
    public void ExitPass()//user clicked cancel or entered wrong password so make enter password panel inactive
    {
        enterPass.SetActive(false);
        passwordField.text = "";
        enteringPassword = false;
    }
}
