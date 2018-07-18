//this script allows users to make an account and to sign in using an account
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class Register : MonoBehaviour
{
    public InputField signInuser, signInPassword;//For signing in
    public InputField password, confirmedpass, usernameRequested;//For registering
    public Button accountButton,storeButton,playButton;//Menu Buttons
    public static bool signedIn=false;//has user signed in
    public NetworkHandler networkManager;
    public RegisterClick panelManager;//if user signs in correctly remove panels
    public AccountManager accountMan;
    public MenuClickScript userMesMan;
    public Text chatPanel;
    public InputField chatDial;
    public Button registerButton, signInButton;
    // Use this for initialization
    public void Registered()
    {
        userMesMan.hideUserMessage();//remove message error of user signing in if they signed in already
        //Registration Check   
        if (usernameRequested.text != "" && password.text != "")//if username and password are not blank
        {
            bool works = true;//set bool to true
            foreach (char c in usernameRequested.text)
            {
                bool isLetterOrDigit = char.IsLetterOrDigit(c);
                if (isLetterOrDigit == false || c.ToString()==" ")//if character is not a letter or digit or it is a string send message that symbols/spaces cannot be used and set works = false
                {
                    userMesMan.UserMessage("Spaces and Symbols Cannot be Used in Username!");
                    works = false;
                    break;
                }
                else if(usernameRequested.text.Length < 5)//check length of username
                {
                    userMesMan.UserMessage("Username has to be at least 5 characters.");
                    works = false;
                    break;
                }
                else if (password.text != confirmedpass.text)//check if pass == confirmed pass
                {
                    works = false;
                    userMesMan.UserMessage("The confirmed password must be the same as the password.");
                    break;
                }
                else if (password.text.Length < 5)//check if pass length is less than 5 --> user security
                {
                    works = false;
                    userMesMan.UserMessage("The password must be at least 5 characters.");
                    break;
                }
            
            foreach (char let in password.text)//make sure no symbols/spaces in password
                {
                    bool isLetterOrDigi = char.IsLetterOrDigit(let);
                    if (isLetterOrDigi == false || let.ToString()==" ")
                    {
                        works = false;
                        userMesMan.UserMessage("Spaces and Symbols Cannot be Used in Password!");
                    }
                }
            }
            if (works)//if everything works out in preliminary check send to serer
            {
                string reason = "Register";
                Debug.Log("Sending");
                registerButton.interactable = false;
                networkManager.Send_message(reason, usernameRequested.text, password.text, null);
            }
        }
        else
        {
            userMesMan.UserMessage("Cannot enter blank username or password");
        }
    }
    public void SignedIn()
    {
        userMesMan.hideUserMessage();//remove message error that user tried before if they try signing in again
        if (signInuser.text != "" && signInPassword.text != "")//if both are not empty because they cannot be empty
        {
            bool works = true;
            foreach (char c in signInuser.text)
            {
                bool isLetterOrDigit = char.IsLetterOrDigit(c);//symbol/space chek
                if (isLetterOrDigit == false || signInuser.text.Length < 5 || c.ToString() == " ")
                {
                    Debug.Log("Incorrect Username");
                    userMesMan.UserMessage("Incorrect Username");
                    works = false;
                    break;
                }
            }
            if (works)//if work check pass
            {
                foreach (char c in signInPassword.text)
                {
                    bool isLetterOrDigit = char.IsLetterOrDigit(c);
                    if (isLetterOrDigit == false || signInPassword.text.Length < 5 || c.ToString() == " ")
                    {
                        userMesMan.UserMessage("Spaces and Symbols Cannot be Used!");
                        works = false;
                        break;
                    }
                }
            }
            if (works)//if above is correct send to server that user wants to sign in
            {
                string reason = "Sign In";
                Debug.Log("Signing In");
                signInButton.interactable = false;
                networkManager.Send_message(reason, signInuser.text, signInPassword.text, null);


            }
        }
        else
        {
            userMesMan.UserMessage("Cannot enter blank username or password.");
        }
    }
    public void Update()
    {

        if (signedIn == false)//if sign in -- false stop user from interacting with other buttons
        {
            Color c = chatPanel.color;
            c.a = 0.5f;
            chatDial.interactable = false;
            accountButton.interactable = false;
            storeButton.interactable = false;
            playButton.interactable = false;
            chatPanel.color = c;
        }
        else//otherwise allow them
        {
            
            Color c = chatPanel.color;
            c.a = 1f;
            chatPanel.color = c;
            chatDial.interactable = true;
            accountButton.interactable = true;
            storeButton.interactable = true;
            playButton.interactable = true;
        }
    }
    public void ServListen(string r, string mm, string e1, string e2)//get sent a message from server
    {
        Debug.Log("Listening");
        if (r == "Login response")//if they loged in successfully
        {
            if (mm == "true")//return all the stats that they have
            {
                signedIn = true;
                panelManager.removePanels();
                //decoding
                XmlSerializer serilize_object = new XmlSerializer(typeof(List<string>));
                List<string> server_message = (List<string>)serilize_object.Deserialize(new StringReader(e1));

                serilize_object = new XmlSerializer(typeof(List<string>));
                List<string> account_strings = (List<string>)serilize_object.Deserialize(new StringReader(server_message[0]));
                // [Username, Password, PP, ST]

                serilize_object = new XmlSerializer(typeof(List<int>));
                List<int> account_ints = (List<int>)serilize_object.Deserialize(new StringReader(server_message[1]));
                // [Tokens, Level, XP, Wins, Losses]

                serilize_object = new XmlSerializer(typeof(bool));
                bool IsVIP = (bool)serilize_object.Deserialize(new StringReader(server_message[2]));

                Debug.Log(account_strings[0]);
                updateAccount(account_strings[0], account_strings[2], account_ints[3], account_ints[4], account_ints[1], account_ints[2], IsVIP);
            }
            else if (mm == "false")
            {
                userMesMan.UserMessage("Username or Password is incorrect");
                Debug.Log("Username or Password is incorrect");
                signInButton.interactable = true;
            }
            else if (mm == "DB")
            {
                userMesMan.SendMessage("Connection Issue. Try again");
                signInButton.interactable = true;
            }
        }
        else// if register response
        {
            if (mm == "true")//user made account and update the account to default settings
            {
                Debug.Log("You have successfully made an account");
                signedIn = true;
                panelManager.removePanels();
                updateAccount(e1, "Default", 0, 0, 1, 0, false);
            }
            else if (mm == "false")//if user failed that means someone has that username
            {
                userMesMan.UserMessage("Someone already has that username");
                registerButton.interactable = true;
            }
            else if (mm == "DB")
            {
                userMesMan.SendMessage("Connection Issue. Try again");
                registerButton.interactable = true;
            }
        }
    }
    
    public void updateAccount(string username, string PP, int Wins, int Losses, int Level, int exp, bool isVIP)//update info in account manager
    {
        accountMan.user = username;
        accountMan.profilePic = PP;
        accountMan.wins = Wins;
        accountMan.losses = Losses;
        accountMan.LVL = Level;
        accountMan.EXP = exp;
        accountMan.VIP = isVIP;
    }
}
