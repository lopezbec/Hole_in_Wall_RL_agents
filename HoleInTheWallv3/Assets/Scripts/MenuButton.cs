using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class MenuButton : MonoBehaviour
{
    public MenuDisplay display;
    public string buttonName;
    private int pressed = 0;
    private float coolDown = -1;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider c)
    {
        if (name == "Settings") Debug.Log(pressed);
        if(pressed == 0 && Time.time - coolDown > 1) display.ActivateButton(buttonName);
        pressed++;
    }

    private void OnTriggerExit(Collider other)
    {
        pressed--;
    }

    private void OnEnable()
    {
        pressed = 0;
        coolDown = Time.time;
    }


}
