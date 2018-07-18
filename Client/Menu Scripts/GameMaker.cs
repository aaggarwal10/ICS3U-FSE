//This script controls where to send the user in the game and it tells the user to go to a game room or to go to menu.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMaker : MonoBehaviour {
    public NetworkHandler networkManager;
    public InputField GameName, GamePassword;
    public Dropdown GameType;
    public GameObject gameMenu, newGameMenu,queueMenu;
    private string gameType;
    private int gameTypeVal;
	void Start () {
		
	}

    public void CreateGame()//user creates game
    {
        gameTypeVal = GameType.value;
        gameType = GameType.options[gameTypeVal].text;
        networkManager.Send_message("Create Room", GameName.text, GamePassword.text, gameType);
        GoToGameMenu();
    }
	
    public void GotoMakeGame()//go to game and actually start playing
    {
        newGameMenu.SetActive(true);
        gameMenu.SetActive(false);
        queueMenu.SetActive(false);
    }

    public void GoToGameMenu()//Go to menu screen
    {
        gameMenu.SetActive(true);
        newGameMenu.SetActive(false);
        queueMenu.SetActive(false);
    }
    public void GotoQueue()//go to queue to wait for players to join
    {
        gameMenu.SetActive(false);
        newGameMenu.SetActive(false);
        queueMenu.SetActive(true);
    }
    public void RefreshMenu()//refresh the game list in game menu
    {
        networkManager.Send_message("Room List", null, null, null);
    }
}
