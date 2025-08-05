using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKDriver : MonoBehaviour
{
    Animator animator;
    public Transform rightController;
    public Transform leftController;
    public Transform headController;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void OnAnimatorIK(int layerIndex)
    {
        float reachR = animator.GetFloat("AvatarRightHand");
        float reachL = animator.GetFloat("AvatarLeftHand");
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, reachR);
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, reachL);
        transform.position = new Vector3(headController.position.x, (float)(headController.position.y - .65), headController.position.z);
        animator.SetIKPosition(AvatarIKGoal.RightHand, rightController.position);
        animator.SetIKPosition(AvatarIKGoal.LeftHand, leftController.position);
    }
}
