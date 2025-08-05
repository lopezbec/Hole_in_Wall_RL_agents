using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndMenu : Menu
{
    public GameObject startMenu;
    public GameObject highScoreText;
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
                startMenu.SetActive(true);
                gameObject.SetActive(false);
                GameController.ResetGame();
                break;

        }
    }

    public void UpdateDisplay(bool isHighscore, int gameScore, int collisions, int deductions)
    {
        if (isHighscore) highScoreText.SetActive(true);
        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>();
        int cursor = isHighscore ? 1 : 0;
        texts[cursor++].text = "Final Score: " + gameScore;
        texts[cursor++].text = "Collisions: " + collisions;
        texts[cursor++].text = "Score Deductions " + deductions;
        
    }
}
