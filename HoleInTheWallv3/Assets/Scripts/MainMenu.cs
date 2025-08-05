using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : Menu
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    override public void ActivateButton(string buttonName)
    {
        Debug.Log(buttonName);
        switch (buttonName)
        {
            case "Main Display":
                SwitchDisplay(0);
                break;
            case "Start":
                Debug.Log("game start");
                GameController.StartGame();
                break;
            case "Settings":
                SwitchDisplay(1);
                break;
            case "Reset Profile":
                GameController.ResetProfile();
                break;
            case "Profiles":
                SwitchDisplay(2);
                break;
            case "Profile 1":
                GameController.EnableProfile(0);
                break;
            case "Profile 2":
                GameController.EnableProfile(1);
                break;
            case "Profile 3":
                GameController.EnableProfile(2);
                break;
            case "Leaderboard":
                SwitchDisplay(3);
                break;

        }
    }
}
