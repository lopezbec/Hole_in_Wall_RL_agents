using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogWriter : MonoBehaviour
{
    private float logTimer;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - logTimer > 1 & GameController.getGameOn())
        {
            GameController.AddLog("<" + gameObject.name + 
                "> - position: " + gameObject.transform.position.x + ", " + gameObject.transform.position.y + ", " + gameObject.transform.position.z + 
                " rotation: " + gameObject.transform.eulerAngles.x + ", " + gameObject.transform.eulerAngles.y + ", " + gameObject.transform.eulerAngles.z + ", ");
            logTimer = Time.time;
        }
    }
}
