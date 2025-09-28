using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PelvisCollider : MonoBehaviour
{
    public bool is_grounded = false;
    public GameObject floor;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionStay(Collision collision)
    {
        // check if pelvis collider is touching the floor
        if (collision.gameObject == floor)
        {
            is_grounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject == floor)
        {
            is_grounded = false;
        }
    }
}
