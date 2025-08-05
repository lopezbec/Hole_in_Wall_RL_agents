using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedCoin : CoinCollision
{
    
    // Start is called before the first frame update
    void Start()
    {
        
    }




    void OnTriggerEnter(Collider c)
    {
        GameController.IncreaseSpeed();
        CollectCoin(c);
    }
}
