using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using TMPro;
using Newtonsoft.Json;
using System.Runtime.ExceptionServices;
using Unity.VisualScripting;
using System.Security.Permissions;
using System.Security.Cryptography;
using System.IO;
using System.Collections.Generic;
using Unity.MLAgents.Policies;

[RequireComponent(typeof(BehaviorParameters))]
public class TrainAvatar : Agent
{
    [Header("Object Scripts")]
    [SerializeField] private AvatarController avatar_script;
    [SerializeField] private WallRemoveTrigger wall_delete_trigger;
    [SerializeField] private ObstacleGenerator wall_script;
    [SerializeField] private StopPlaytime playtime_script;

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
    public bool is_waiting_episode_end = false;
    private bool is_first_run = true;
    private string demo_json_path = Application.dataPath + "/Scripts/Reinforcement_Learning/gail_demos/demo_solutions.json";
    public float current_reward = 0;

    private BehaviorParameters behaviorParameters;

    void Awake()
    {
        behaviorParameters = GetComponent<BehaviorParameters>();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        if (avatar_script == null || wall_delete_trigger == null || wall_script == null) Debug.LogWarning("Need to initialize the scripts");
        if(playtime_script == null) Debug.Log("WARNING, there is no automatic stopping during ML training if policy collapses. Initialize stop playtime script if needed.");
        
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
                Transform wall_piece = wall_script.transform.GetChild(0).Find(name);

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

        GameObject agent_ragdoll = transform.Find("Controller_AI_APOSE").Find("ragdoll").gameObject;
        bodyCollider[] limbs_with_colliders = agent_ragdoll.GetComponentsInChildren<bodyCollider>();

        //add the limb positions to the observation
        for (int i = 0; i < limbs_with_colliders.Length; i++)
        {
            Transform limb_transform = limbs_with_colliders[i].gameObject.transform;
            
            sensor.AddObservation(limb_transform.position);
        }

         sensor.AddObservation(avatar_script.energy_script.is_sitting);
    }


    public override void OnActionReceived(ActionBuffers actions)
    {

        //17 actions

        //Heuristic mode → DO NOT mutate anything

        

        //move left hand
        avatar_script.Move_hand(actions.ContinuousActions[0], actions.ContinuousActions[1], actions.ContinuousActions[2], false);
        Check_Overextension();

        // //move right hand
        avatar_script.Move_hand(actions.ContinuousActions[3], actions.ContinuousActions[4], actions.ContinuousActions[5], true);
        Check_Overextension();

        //rotate hips. rotation restictions are based off of AvatarController.cs
        float hip_x_rot = actions.ContinuousActions[6];
        if(behaviorParameters.BehaviorType != BehaviorType.HeuristicOnly){
            if (hip_x_rot < 0) hip_x_rot *= 30;
            else hip_x_rot *= 100;
        }

        float hip_y_rot = actions.ContinuousActions[7];
        float hip_z_rot = actions.ContinuousActions[8];

        if(behaviorParameters.BehaviorType != BehaviorType.HeuristicOnly){
            hip_y_rot *= 30;
            hip_z_rot *= 25;
        }

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
        //float body_x_pos = actions.ContinuousActions[15] * 1.5f;
        //float body_y_rot = actions.ContinuousActions[16] * 360;
        //float body_z_pos = actions.ContinuousActions[17] * 1.5f;
        //avatar_script.Move_body(body_x_pos, body_y_rot, body_z_pos);

        //move body 
        float body_x_pos = actions.ContinuousActions[15];
        float body_y_rot = actions.ContinuousActions[16];

        if(behaviorParameters.BehaviorType != BehaviorType.HeuristicOnly){
            body_x_pos *= 1.5f;
            body_y_rot *= 360;
        }

        avatar_script.Move_body(body_x_pos, body_y_rot, 0f);
        Check_Overextension();


        //finished moving the body
        StartCoroutine(WallMoveAfterDelay(1.5f));
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;

        //read JSON content from file
        string json_content = File.ReadAllText(demo_json_path);

        //deserialize JSON to object
        List<DemoJSON> demo_walls = JsonConvert.DeserializeObject<List<DemoJSON>>(json_content);
        
        //find the current test wall
        int current_wall_id = wall_script.test_id;
        DemoJSON current_wall = null;

        foreach(var wall in demo_walls)
        {   
            if (current_wall_id == wall.Wall_id) {
                current_wall = wall;
                break;
            }
        }
        
        if (current_wall == null)
        {           
            Debug.LogError($"No wall found for id {current_wall_id}");
            return;
        }
        
        //randomly select a solution 
        System.Random rand = new();
        int sol_id = rand.Next(0, current_wall.Solutions.Length);
        float [] selected_solution = current_wall.Solutions[sol_id];


        //map to the continuous actions
        for(int i = 0; i < selected_solution.Length; i++)
        {
            continuousActions[i] = selected_solution[i];
        }

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
            float reward = Mathf.Exp(-energy_calculated * 0.01f);

            if(avatar_script.energy_script.is_sitting) AddRwd(-5f); // penalty for completing pose while grounded, to prevent "gaming"

            //reward for not touching the walls
            if (!avatar_script.has_collided)
            {
                //pass reward
                if(!avatar_script.energy_script.is_sitting) AddRwd(reward);

                floor.GetComponent<MeshRenderer>().material = pass_material;
            }


            else
            {
                //punish for losing
                AddRwd(-1f * (1f - reward));
            }

            is_waiting_episode_end = true;
            StartCoroutine(EndEpisodeAfterDelay(.1f));
        }
    }

    private void Check_Overextension()
    {
        float penalty = -0.2f;

        if (avatar_script.has_over_moved) AddRwd(penalty);
        if (avatar_script.has_over_rotated) AddRwd(penalty);
    }

    public override void OnEpisodeBegin()
    {   
        //the intiial pose position gets altered if reset occurs on the first run, subsequent runs doesn't get altered
        if(!is_first_run) ResetEnvironment();
        else is_first_run = false;

        StartCoroutine(RequestDecisionAfterDelay(0.1f));
    }

    public void Collision_punish()
    {
        //lose per collided limb. There are 14 bodyColliders per agent that can trigger this function
        AddRwd(-0.5f);
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
        new_instance.name = "Controller_AI_APOSE";
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

        current_reward = 0;
    }

    private IEnumerator EndEpisodeAfterDelay(float delay)
    {
        // show reward for debugging
        reward_text.text = GetCumulativeReward().ToString();

        //check for ml environment collapse
        if(playtime_script != null) playtime_script.AddIntoAVG(current_reward);

        yield return new WaitForSeconds(delay);

        EndEpisode();

    }

    private IEnumerator RequestDecisionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        RequestDecision();
    }

    private IEnumerator WallMoveAfterDelay(float delay)
    {   

        //must give it some time for the ragdoll to catch up to the changes of the static animator. speed up.
        //float project_time = Time.timeScale;
        //Time.timeScale = 30f;

        yield return new WaitForSeconds(delay);

        //put back tp normal time scale
        //Time.timeScale = project_time;
        avatar_script.completed_pose = true;

    }

    private void AddRwd(float reward_amt)
    {   
        //add into the current reward for debugging
        current_reward += reward_amt;
        AddReward(reward_amt);
    }

    
}
