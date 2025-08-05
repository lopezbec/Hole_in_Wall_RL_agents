using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenCoin : CoinCollision
{
    // Start is called before the first frame update
    void Start()
    {
        
    }



    void OnTriggerEnter(Collider c)
    {
        GameController.DecreaseSpeed();
        CollectCoin(c);
    }
}
