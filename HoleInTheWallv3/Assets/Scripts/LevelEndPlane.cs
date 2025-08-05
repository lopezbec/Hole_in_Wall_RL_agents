using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEndPlane : MonoBehaviour
{
    public LevelSpawner spawner;
    private bool hit = false;
    // Start is called before the first frame update


    public LevelEndPlane()
    {

    }
    public LevelEndPlane(LevelSpawner spawner)
    {
        this.spawner = spawner;
    }


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (!hit && collision.gameObject.tag == "Player")
        {
            GameController.LevelEnded(spawner);
            hit = true;
        }
    }
}
