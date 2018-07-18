//this script controls the messages sent between client and server and is the core of the client code
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
using System;
using System.Xml.Serialization;
using System.IO;
using UnityEngine.SceneManagement;
//using SimpleJSON;


public class Stringlify_Message : MessageBase
{
    public string main_message;
    public string reason;
    public string extra_info;
    public string extra_info2;
}


public class NetworkHandler : MonoBehaviour
{
    // ------------------------- Defining Variables -------------------------------------- //
    public Register signinManager;
    public bool isAtStartup = true; // just using this rn for the rest of code
    public NetworkClient myClient; // This is our client object, we will connect it later
    private const short chatMessage = 131; // this is our secret message port within our port
    public QueueManager queueMan;
    public AccountManager accountMan;
    public GamePanelMaker gameMan;
    public GameMaker gameMan2;
    public ChatBox chatMan;
    public QueChat queMan;
    public GameObject GameScene, MenuScene;
    public SelectionUnitsScript selectMan;
    public DisplayStats displayMan;
    public Dictionary<string, float> healthDictionary;
    public CameraPlrMovement camMan;
    public GameObject winPanel, losePanel;
    public Wait waitMan;
    public GameChat gameChatMan;

    public short LeaveEvent = MsgType.Disconnect;
    public int channel_ID;

    public IEnumerator set_up()
    {
        string nameKeep = "https://script.google.com/macros/s/AKfycbyHApaejkQCI43E6QinTT3Pc1RGHGxh4Cgfm3Da2YuHvApbBlY/exec";

        WWWForm form = new WWWForm();
        form.AddField("Request", "Get");
        form.AddField("IP", "This Can be left empty");


        using (UnityWebRequest request_test = UnityWebRequest.Post(nameKeep, form))
        {
            yield return request_test.SendWebRequest();

            string response_ret = request_test.downloadHandler.text;

            myClient = new NetworkClient();
            ConnectionConfig myConfig = new ConnectionConfig();
            channel_ID = (int)myConfig.AddChannel(QosType.ReliableFragmented);
            myConfig.PacketSize = 1999;
            myConfig.FragmentSize = 1500;
            myConfig.MaxSentMessageQueueSize = 200;
            myClient.Configure(myConfig, 100);

            myClient.Connect(response_ret.Substring(1, response_ret.Length - 2), 8362);
            isAtStartup = false;

            myClient.RegisterHandler(chatMessage, ReceiveMessage); //connect the event of receiving a server message
            myClient.RegisterHandler(LeaveEvent, ConnectionLost);
        }


    }

    private void Start()
    {
        StartCoroutine(set_up());
    }

    void Update() // Runs everytime the real time unity runs to simulate changes (per frame)
    {
        // -------------------------Checking for client activity -------------------------------------- //

    }

    public void Send_message(string r, string mm, string e1, string e2)
    {
        Stringlify_Message to_ser = new Stringlify_Message
        {
            main_message = mm,
            reason = r,
            extra_info = e1,
            extra_info2 = e2
        };

        myClient.SendByChannel(chatMessage, to_ser, channel_ID);
    }

    // ------------------------- Check server messages -------------------------------------- //
    public string Encode_XML(object obj_tohide, Type required_type)
    {
        XmlSerializer serializer = new XmlSerializer(required_type);
        StringWriter sw = new StringWriter();
        XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
        serializer.Serialize(sw, obj_tohide, ns);
        string converted_string = sw.ToString();

        return converted_string;
    }
    public void ConnectionLost(NetworkMessage connection)
    {
        Debug.Log("Connection to the sevrer has been broken");
    }
    public void ReceiveMessage(NetworkMessage network_message)//funtion that gets all messages fromm server
    {
        var message = network_message.ReadMessage<Stringlify_Message>();
        Debug.Log(message.reason);
        Debug.Log(message.main_message);
        if (message.reason == "Login response" || message.reason == "Register response")//if login or register send to sign in manager to decode
        {
            signinManager.ServListen(message.reason, message.main_message, message.extra_info, message.extra_info2);
        }
        else if (message.reason == "Room Response")// if room response move to queman if you can make room
        {
            if (message.main_message == "true")
            {
                queueMan.GoToQueue(new List<List<string>> { new List<string> { accountMan.user, accountMan.profilePic } });
            }
            else//otherwise...
            {
                Debug.Log("Room Already Exists");
            }
        }
        else if (message.reason == "Queue Update")//add people to que
        {
            XmlSerializer serilize_object = new XmlSerializer(typeof(List<List<string>>));
            StringReader open_string = new StringReader(message.main_message);
            List<List<string>> LOS = (List<List<string>>)serilize_object.Deserialize(open_string);
            queueMan.GoToQueue(LOS);
        }
        else if (message.reason == "Room Refresh")//refresg the list of rooms in game menu
        {
            XmlSerializer serilize_object = new XmlSerializer(typeof(List<List<string>>));
            StringReader open_string = new StringReader(message.main_message);
            List<List<string>> LOS = (List<List<string>>)serilize_object.Deserialize(open_string);
            gameMan.makeGameMenu(LOS);
        }
        else if (message.reason == "Room removed")//if room remove go to game menu and clear que chat
        {
            gameMan2.GoToGameMenu();
            queMan.MakeChat(new List<string> { "" });//erase the que chat
            Send_message("Room List", null, null, null);//send message to refresh room list in gamme menu
        }
        else if (message.reason == "World Messages")//if world messages update the global chat in the menu
        {
            XmlSerializer serilize_object = new XmlSerializer(typeof(List<string>));
            StringReader open_string = new StringReader(message.main_message);
            List<string> LOS = (List<string>)serilize_object.Deserialize(open_string);
            chatMan.MakeChat(LOS);//make chat with list of messages
        }
        else if (message.reason == "Room Messages")// if room messages update que chat
        {
            XmlSerializer serilize_object = new XmlSerializer(typeof(List<string>));
            StringReader open_string = new StringReader(message.main_message);
            List<string> LOS = (List<string>)serilize_object.Deserialize(open_string);
            queMan.MakeChat(LOS);//make chat with list of messages
        }

        //Game events
        else if (message.reason == "Game Started")//if game started get current player index and go to game scence
        {
            GoToGame();
            XmlSerializer serilize_object = new XmlSerializer(typeof(List<string>));
            StringReader open_string = new StringReader(message.main_message);
            List<string> player_list = (List<string>)serilize_object.Deserialize(open_string);

            selectMan.playerIndex = player_list.IndexOf(accountMan.user);
        }
        else if (message.reason == "Game State")// if game state draw other cameras, update health, and, update the pos list of units based off unique id
        {
            //posx,posy,posz,bullet,bposx,bposy,bposz,health
            XmlSerializer serilize_object = new XmlSerializer(typeof(List<string>));//decode the message sent from server
            StringReader open_string = new StringReader(message.main_message);
            List<string> ID_List = (List<string>)serilize_object.Deserialize(open_string);

            XmlSerializer serilize_object2 = new XmlSerializer(typeof(List<List<float>>));
            StringReader open_string2 = new StringReader(message.extra_info);
            List<List<float>> Pos_List = (List<List<float>>)serilize_object2.Deserialize(open_string2);

            Dictionary<string, float> healthDictionary2 = new Dictionary<string, float> { };
            for (int i = 0; i < ID_List.Count; i++)
            {
                healthDictionary2[ID_List[i]] = Pos_List[i][Pos_List[i].Count - 1];
            }
            healthDictionary = healthDictionary2;//make health dictionary
            // e2 is cam positions
            selectMan.MoveMakeUnits(ID_List, Pos_List);//make units on screen
            XmlSerializer serilize_object3 = new XmlSerializer(typeof(List<List<float>>));
            StringReader open_string3 = new StringReader(message.extra_info2);
            List<List<float>> cam_List = (List<List<float>>)serilize_object3.Deserialize(open_string3);
            camMan.drawOtherPlayer(cam_List);//draw the spheres of the other players on the screen so user can see where others are
        }
        else if (message.reason == "Game Stats")//update right corner game stats:
        {// cash, income, max_units, max_buildings, unit_count, building_count
            XmlSerializer serilize_object = new XmlSerializer(typeof(List<int>));
            StringReader open_string = new StringReader(message.main_message);
            List<int> Stat_list = (List<int>)serilize_object.Deserialize(open_string);
            displayMan.displayGameStats(Stat_list[0], Stat_list[1], Stat_list[2], Stat_list[3], Stat_list[4], Stat_list[5]);//display game panel stats - cash, units, buildings, income

        }
        else if (message.reason == "Game Ended")// send winners to win message and losers to lose message
        {
            if (message.main_message == "Win")
            {
                StartCoroutine(newScreen(winPanel, 10));
            }
            else
            {
                StartCoroutine(newScreen(losePanel, 10));
            }
        }
        else if (message.reason == "Global Game Messages")// maje global message to entire game room
        {
            XmlSerializer serilize_object = new XmlSerializer(typeof(List<string>));
            StringReader open_string = new StringReader(message.main_message);
            List<string> all_messages = (List<string>)serilize_object.Deserialize(open_string);
            gameChatMan.gameChatMes["Global"] = all_messages;
            if (gameChatMan.chatMode == "Global")//if user is in global chat mode
            {
                gameChatMan.MakeGameChat(all_messages);//make chat on screen for him
            }

        }
        else if (message.reason == "Team Messages")//team messages to teamates
        {
            XmlSerializer serilize_object = new XmlSerializer(typeof(List<string>));
            StringReader open_string = new StringReader(message.main_message);
            List<string> all_messages = (List<string>)serilize_object.Deserialize(open_string);
            gameChatMan.gameChatMes["Team"] = all_messages;
            if (gameChatMan.chatMode == "Team")//if user is on team mode
            {
                gameChatMan.MakeGameChat(all_messages);//make chat with list of messages
            }
        }
    }
    public void GoToGame()//switch scene to game
    {
        GameScene.SetActive(true);
        MenuScene.SetActive(false);
    }
    public void GoToMenu()//switch scene to menu
    {
        GameScene.SetActive(false);
        MenuScene.SetActive(true);
    }
    public IEnumerator newScreen(GameObject panel, float seconds)
    {
        panel.SetActive(true);
        yield return new WaitForSeconds(seconds);
        panel.SetActive(false);
        GoToMenu();
    }
}
