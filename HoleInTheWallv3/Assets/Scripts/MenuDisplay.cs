using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuDisplay : MonoBehaviour
{
    public Menu menu;

    public void ActivateButton(string buttonName)
    {
        menu.ActivateButton(buttonName);
    }
}
