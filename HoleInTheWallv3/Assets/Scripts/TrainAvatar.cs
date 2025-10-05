using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;

public class TrainAvatar : Agent
{
    [Header("Object Scripts")]
    [SerializeField] private AvatarController avatar_script;
    [SerializeField] private WallRemoveTrigger wall_delete_trigger;
    [SerializeField] private ObstacleGenerator wall_script;


    [Header("Materials for Pass/Fail")]
    [SerializeField] private GameObject floor;
    [SerializeField] private Material pass_material;
    [SerializeField] private Material fail_material;

    //decision request per reset
    private Vector3 avatar_start_pos;
    private Vector3 wall_start_pos;
    private readonly string prefab_path = "Controller_AI_APOSE";

    // Start is called before the first frame update
    void Start()
    {
        if (avatar_script == null || wall_delete_trigger == null || wall_script == null) Debug.LogWarning("Need to initialize the scripts");

        wall_start_pos = wall_script.transform.position;
        avatar_start_pos = avatar_script.transform.position;

        RequestDecision();
    }

    // Update is called once per frame
    void Update()
    {
        Check_Result();
    }

    // input of the agent
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(avatar_script.has_collided);
        sensor.AddObservation(avatar_script.has_over_moved);
        sensor.AddObservation(avatar_script.has_over_rotated);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        //move left hand
        avatar_script.Move_hand(actions.ContinuousActions[0], actions.ContinuousActions[1], actions.ContinuousActions[2], false);
        Check_Overextension();

        //move right hand
        avatar_script.Move_hand(actions.ContinuousActions[3], actions.ContinuousActions[4], actions.ContinuousActions[5], true);
        Check_Overextension();

        //rotate hips
        avatar_script.Rotate_hip(actions.ContinuousActions[6], actions.ContinuousActions[7], actions.ContinuousActions[8]);
        Check_Overextension();

        //move left leg
        avatar_script.Move_legs(actions.ContinuousActions[9], actions.ContinuousActions[10], actions.ContinuousActions[11], false);
        Check_Overextension();

        //move right leg
        avatar_script.Move_legs(actions.ContinuousActions[12], actions.ContinuousActions[13], actions.ContinuousActions[14], true);
        Check_Overextension();

        //move body
        avatar_script.Move_body(actions.ContinuousActions[15], actions.ContinuousActions[16], actions.ContinuousActions[17]);
        Check_Overextension();

        //finished moving the body
        avatar_script.completed_pose = true;
    }

    private void Check_Result()
    {
        //change color of floor to visualize pass/loss
        if (avatar_script.has_collided)
        {
            floor.GetComponent<MeshRenderer>().material = fail_material;
        }

        //finalize the rewards when the wall is completely done passing
        if (wall_delete_trigger.wall_complete)
        {
            //reward for not touching the walls
            if (!avatar_script.has_collided)
            {
                SetReward(1f);
                floor.GetComponent<MeshRenderer>().material = pass_material;
            }


            //CHANGE THIS. MAKE IT SO THAT SOME COLLIDERS THAT PASS DO NOT CONTRIBUTE TO LOSS
            else SetReward(-1f);

            EndEpisode();
            OnEpisodeBegin();
        }
    }

    private void Check_Overextension()
    {
        float penalty = -0.2f;

        if (avatar_script.has_over_moved) SetReward(penalty);
        if (avatar_script.has_over_rotated) SetReward(penalty);
    }

    public override void OnEpisodeBegin()
    {
        Destroy(avatar_script.gameObject);
        //create and reset controller environment
        GameObject prefab = Resources.Load(prefab_path) as GameObject;
        GameObject new_instance = Instantiate(prefab, avatar_start_pos, Quaternion.identity);
        new_instance.name = "Controller_AI";
        new_instance.transform.SetParent(this.transform);

        //reset the avatar
        avatar_script = new_instance.GetComponent<AvatarController>();
        avatar_script.completed_pose = false;
        floor = avatar_script.transform.Find("Floor").gameObject;

        //reset the walls
        wall_script.avatar_script = avatar_script;
        wall_script.transform.position = wall_start_pos;
        wall_script.gameObject.SetActive(true);

        //wall delete goes back to false
        wall_delete_trigger.wall_complete = false;

        RequestDecision();
        RequestAction();
    }
}
