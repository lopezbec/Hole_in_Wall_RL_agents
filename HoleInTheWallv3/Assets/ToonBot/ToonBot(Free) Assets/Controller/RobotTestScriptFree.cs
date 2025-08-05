using UnityEngine;
using System.Collections;

public class RobotTestScriptFree : MonoBehaviour {

	Animator animator;
    public Transform rightController;
    public Transform leftController;
    public Transform headController;
    private Transform modelHead;

    void Start()
    {
        animator = GetComponent<Animator>();

        modelHead = this.transform.GetChild(0).transform.GetChild(2).transform.GetChild(0).transform.GetChild(2).transform.GetChild(0).transform;
        Debug.Log(modelHead);
    }

    void OnAnimatorIK(int layerIndex)
    {
        float reachR = animator.GetFloat("AvatarRightHand");
        float reachL = animator.GetFloat("AvatarLeftHand");
        float reachH = animator.GetFloat("AvatarHead");
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, reachR);
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, reachL);
        transform.position = new Vector3(headController.position.x, (float)(headController.position.y - .65), headController.position.z);
        animator.SetIKPosition(AvatarIKGoal.RightHand, rightController.position);
        animator.SetIKPosition(AvatarIKGoal.LeftHand, leftController.position);
    }
}
