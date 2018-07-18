//This is the script that holds all of the account information for an individual player
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AccountManager : MonoBehaviour
{

    // Random Variables for Initialization :)

    public int wins = 0; //wins
    public int losses = 5000; //losses
    public int EXP = 10000; //total exp that user has
    public int LVL = 4;//user's level
    private int totalEXPLVL;//total exp for current level
    public string user = "AnishIsGreat";//username :)
    public bool VIP = false;//VIP status
    public Image profilePicIM, VIPBadge, menuPic;//images
    public Sprite defaultPic, secondaryPic;//sprites for each profile picture
    public Text gameWins, gameLosses, LeVeL, pointPercentage, DisplayUser, menuUser;//text for account menu
    public string profilePic = "Default";//user's current profile picture
    public Slider winlossBar, ExpBar;//sliders for exp and win loss ratios
    public Dictionary<string, Sprite> profilePicDict = new Dictionary<string, Sprite>();//dictionary of strings to sprites for the profile pictures

    public void Start()//add the pictures to the dictionary 
    {
        profilePicDict.Add("Default", defaultPic);
        profilePicDict.Add("secondary", secondaryPic);
    }
    public void Update()
    {
        totalEXPLVL = LVL * LVL * 50;
        gameWins.text = wins.ToString();
        gameLosses.text = losses.ToString();

        if (wins + losses == 0)
        {

            winlossBar.value = 1f;//if user did not win a game and did not lose a game that means they played no game and thus winlossbar is 1
        }
        else//set value of winloss bar
        {
            winlossBar.value = (float)wins / (wins + losses);
        }
        ExpBar.value = (float)EXP / totalEXPLVL;//set value of exp bar
        LeVeL.text = LVL.ToString();
        pointPercentage.text = (EXP * 100 / totalEXPLVL).ToString() + "%";//exp percentage
        DisplayUser.text = user;//set the placeholders for username to be the username of user
        menuUser.text = user;//set the placeholders for username to be the username of user
        //change color of badge to be translucent when user is not VIP
        Color c = VIPBadge.color;
        if (VIP)
        {
            c.a = 1f;
        }
        else
        {
            c.a = 0.21f;
        }
        VIPBadge.color = c;
        //Set User's profile pictures
        profilePicIM.sprite = profilePicDict[profilePic];
        menuPic.sprite = profilePicDict[profilePic];
    }

}
