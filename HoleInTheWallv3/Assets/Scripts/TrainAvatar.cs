using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using TMPro;
using UnityEngine.Rendering;

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

    [SerializeField] private TextMeshProUGUI reward_text;

    //decision request per reset
    private Vector3 avatar_start_pos;
    private Vector3 wall_start_pos;
    private readonly string prefab_path = "Controller_AI_APOSE";

    //private bodyCollider[] limb_positions;
    private bool is_waiting_episode_end = false;

    // Start is called before the first frame update
    void Start()
    {
        if (avatar_script == null || wall_delete_trigger == null || wall_script == null) Debug.LogWarning("Need to initialize the scripts");

        wall_start_pos = wall_script.transform.position;
        avatar_start_pos = avatar_script.transform.position;

        //RequestDecision();
    }

    // Update is called once per frame
    void Update()
    {
        Check_Result();
        reward_text.text = GetCumulativeReward().ToString();
    }

    // input of the agent
    public override void CollectObservations(VectorSensor sensor)
    {
        //wall observations, using matrix method in obstacle generator
        int[,] wall_matrix = wall_script.wall;


        for (int i = 0; i < wall_matrix.GetLength(0); i++)
        {
            for (int j = 0; j < wall_matrix.GetLength(1); j++)
            {
                //find the wall piece according to the matrix index
                string name = string.Format("{0}, {1}", i, j);
                Transform wall_piece = wall_script.transform.Find(name);

                sensor.AddObservation(wall_matrix[i, j]);

                if (wall_piece == null)
                {
                    //pad the local scale + position, because this is the hole in wall
                    sensor.AddObservation(new Vector3(0, 0, 0));
                    sensor.AddObservation(new Vector3(0, 0, 0));
                }
                else
                {
                    sensor.AddObservation(wall_piece.localScale);
                    sensor.AddObservation(wall_piece.position);
                }
            }
        }

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        //18 actions

        //move left hand
        avatar_script.Move_hand(actions.ContinuousActions[0], actions.ContinuousActions[1], actions.ContinuousActions[2], false);
        Check_Overextension();

        // //move right hand
        avatar_script.Move_hand(actions.ContinuousActions[3], actions.ContinuousActions[4], actions.ContinuousActions[5], true);
        Check_Overextension();

        //rotate hips. rotation restictions are based off of AvatarController.cs
        float hip_x_rot = actions.ContinuousActions[6];
        if (hip_x_rot < 0) hip_x_rot *= 30;
        else hip_x_rot *= 100;

        float hip_y_rot = actions.ContinuousActions[7] * 30;
        float hip_z_rot = actions.ContinuousActions[8] * 25;
        avatar_script.Rotate_hip(hip_x_rot, hip_y_rot, hip_z_rot);
        Check_Overextension();

        //move left leg
        float left_leg_x = actions.ContinuousActions[9];
        float left_leg_y = actions.ContinuousActions[10];
        float left_leg_z = actions.ContinuousActions[11];
        avatar_script.Move_legs(left_leg_x, left_leg_y, left_leg_z, false);
        Check_Overextension();

        //move right leg
        float right_leg_x = actions.ContinuousActions[12];
        float right_leg_y = actions.ContinuousActions[13];
        float right_leg_z = actions.ContinuousActions[14];
        avatar_script.Move_legs(right_leg_x, right_leg_y, right_leg_z, true);
        Check_Overextension();

        //move body
        float body_x_pos = actions.ContinuousActions[15] * 1.5f;
        float body_y_rot = actions.ContinuousActions[16] * 360;
        float body_z_pos = actions.ContinuousActions[17] * 1.5f;
        avatar_script.Move_body(body_x_pos, body_y_rot, body_z_pos);
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
        if (wall_delete_trigger.wall_complete && !is_waiting_episode_end)
        {

            float energy_calculated = avatar_script.energy_script.Calculate_energy();
            //float reward = Mathf.Exp(-energy_calculated * 0.01f);

            //lower the energy, the more reward. add small number to ensure no 0 division
            float reward = 1000 / (energy_calculated + 0.0001f);


            //reward for not touching the walls
            if (!avatar_script.has_collided)
            {

                //only give reward if not grounded/sitting for this training
                if (!avatar_script.energy_script.root_joint.GetComponent<PelvisCollider>().is_grounded)
                {
                    //pass reward
                    AddReward(reward);
                }

                floor.GetComponent<MeshRenderer>().material = pass_material;
            }


            //CHANGE THIS. MAKE IT SO THAT SOME COLLIDERS THAT PASS DO NOT CONTRIBUTE TO LOSS
            else
            {

                //punish for losing
                AddReward(-1 * energy_calculated / 10);
            }

            is_waiting_episode_end = true;
            StartCoroutine(EndEpisodeAfterDelay(.1f));
        }
    }

    private void Check_Overextension()
    {
        float penalty = -5f;

        if (avatar_script.has_over_moved) AddReward(penalty);
        if (avatar_script.has_over_rotated) AddReward(penalty);
    }

    public override void OnEpisodeBegin()
    {
        ResetEnvironment();
        StartCoroutine(RequestDecisionAfterDelay(0.1f));
    }

    public void Collision_punish()
    {
        //lose per collided limb
        AddReward(-10f);
    }

    private void ResetEnvironment()
    {
        is_waiting_episode_end = false;
        //wall delete goes back to false
        wall_delete_trigger.wall_complete = false;

        Destroy(avatar_script.gameObject);
        //create and reset controller environment
        GameObject prefab = Resources.Load(prefab_path) as GameObject;
        GameObject new_instance = Instantiate(prefab, avatar_start_pos, Quaternion.identity);
        new_instance.name = "Controller_AI";
        new_instance.transform.SetParent(this.transform);

        //reset the avatar
        avatar_script = new_instance.GetComponent<AvatarController>();
        //limb_positions = GetComponentsInChildren<bodyCollider>();
        avatar_script.completed_pose = false;
        avatar_script.has_collided = false;
        floor = avatar_script.transform.Find("Floor").gameObject;

        //reset the walls
        wall_script.avatar_script = avatar_script;
        wall_script.transform.position = wall_start_pos;
        wall_script.Reset_Wall();
    }

    private IEnumerator EndEpisodeAfterDelay(float delay)
    {
        // show reward for debugging
        reward_text.text = GetCumulativeReward().ToString();

        yield return new WaitForSeconds(delay);

        EndEpisode();

    }

    private IEnumerator RequestDecisionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        RequestDecision();
    }
}
