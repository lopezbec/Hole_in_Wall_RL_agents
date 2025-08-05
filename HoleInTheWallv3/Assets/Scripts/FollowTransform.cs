using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    public Transform follow_obj;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = follow_obj.position;
        //more import for semicircle than the sphere collider for Controller AI prefab
        transform.eulerAngles = new(-90f + follow_obj.eulerAngles.x, 90f + follow_obj.eulerAngles.y, 0);
    }
}
