using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bodyCollider : MonoBehaviour
{
    [SerializeField] private AvatarController avatar_script;
    private bool hasCollided = false;
    public bool isTestObject = false;


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
        if (!isTestObject)
        {
            if (!hasCollided && collision.gameObject.tag == "Walls") //if collider hits wall, deduct score once
            {
                hasCollided = true;
                GameController.PlayerCollided();
                GameController.AddLog("Collision: " + gameObject.name + " with " + collision.gameObject.name + " at " + collision.gameObject.transform.position.x + ", " + collision.gameObject.transform.position.y + ", " + collision.gameObject.transform.position.z);
            }
            if (collision.gameObject.tag == "LevelStart")//reset at the start of levels
            {

                hasCollided = false;
            }
        }
        else
        {
            //this is for the machine learning environment to stop seeing the errors
            if (!hasCollided && collision.gameObject.CompareTag("Walls"))
            {
                hasCollided = true;
                avatar_script.has_collided = true;
            }
        }

    }
}
