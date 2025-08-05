using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public List<GameObject> displays = new List<GameObject>();
    private int currDisplay = 0;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchDisplay(int displayID)
    {
        displays[currDisplay].SetActive(false);
        currDisplay = displayID;
        displays[currDisplay].SetActive(true);

    }

    virtual public void ActivateButton(string buttonName)
    {
        Debug.Log("no");
    }
}
