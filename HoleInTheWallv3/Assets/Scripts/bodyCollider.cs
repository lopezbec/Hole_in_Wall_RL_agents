using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bodyCollider : MonoBehaviour
{
    private bool hasCollided = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnTriggerEnter(Collider collision)
    {
        if (!hasCollided && collision.gameObject.tag == "Walls") //if collider hits wall, deduct score once
        {
            hasCollided = true;
            GameController.PlayerCollided();
            GameController.AddLog("Collision: " + gameObject.name + " with " + collision.gameObject.name + " at " + collision.gameObject.transform.position.x + ", " + collision.gameObject.transform.position.y + ", " + collision.gameObject.transform.position.z);
        }
        if(collision.gameObject.tag == "LevelStart")//reset at the start of levels
        {
            
            hasCollided = false;
        }
    }



}
