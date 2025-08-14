using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


public class AvatarController : MonoBehaviour
{
    private readonly string prefab_path = "Controller_AI";
    [SerializeField] private GameObject static_animator;

    [Header("Limb Targets")]
    [SerializeField] private Transform right_hand_target;
    [SerializeField] private Transform left_hand_target;
    [SerializeField] private Transform hip_target;
    [SerializeField] private Transform right_leg_target;
    [SerializeField] private Transform left_leg_target;


    [Header("Movement Limits")]
    [SerializeField] private SphereCollider right_arm_span;
    [SerializeField] private SphereCollider left_arm_span;
    [SerializeField] private MeshCollider right_leg_span;
    [SerializeField] private MeshCollider left_leg_span;
    [SerializeField] private BoxCollider movement_boundary;


    // variables to track if given position or rotation exceeds avatar movement (to reduce sparsity)
    [Header("Limit Violation Check")]
    public bool has_over_moved = false;
    public bool has_over_rotated = false;


    [Header("Energy Script")]
    [SerializeField] private EnergyExpenditure energy_script;

    private Vector3 prefab_position;

    //file to read from
    readonly string controller_path = Application.dataPath + "/Scripts/Reinforcement_Learning";
    string direction_file;

    //file to output towards
    string final_position_file;

    // Start is called before the first frame update    
    void Start()
    {
        prefab_position = transform.position;

        //check for serialized fields
        if (right_hand_target == null || left_hand_target == null || hip_target == null || right_leg_target == null || left_leg_target == null)
            Debug.Log("Please drag the limb targets to AvatarController script");
        if (movement_boundary == null || right_arm_span == null || left_arm_span == null || right_leg_span == null || left_leg_span == null)
            Debug.Log("Please drag the limiting boundary objects to AvatarController script");

        //initialize the file names
        direction_file = controller_path + "/move_direction.csv";
        final_position_file = controller_path + "/final_position.csv";

        //tests
        Read_movement_file(11);
        //Generate_Movement_File(6);
        //StartCoroutine(Generate_Movement(5));
    }


    //when reading file, make sure to reset controller first to have the pose in the correct starting pose
    public void Read_movement_file(int wall_id)
    {
        Dictionary<string, (float, float, float)> position_record = new()
        {
            //populate the dictionary                                                                           // order
            ["l_hand_position"] = (0, 0, 0),                                                                    // 0
            ["r_hand_position"] = (0, 0, 0),                                                                    // 1
            ["hip_rotation"] = (0, 0, 0),                                                                       // 2
            ["l_leg_position"] = (0, 0, 0),                                                                     // 3
            ["r_leg_position"] = (0, 0, 0),                                                                     // 4
            ["body_position"] = (static_animator.transform.position.x, static_animator.transform.position.y, static_animator.transform.position.z)              // 5
        };

        //find the rotation of the hand

        using (StreamReader reader = new(direction_file))
        {
            while (!reader.EndOfStream)
            {
                //read the line
                string line = reader.ReadLine();

                if (string.IsNullOrWhiteSpace(line)) continue;

                //param separated by comma
                string[] parameters = line.Split(',');

                if (parameters.Length != 4) continue;

                // try parsing the first 4 items
                if (!int.TryParse(parameters[0], out int move_type) ||
                    !float.TryParse(parameters[1], out float x) ||
                    !float.TryParse(parameters[2], out float y) ||
                    !float.TryParse(parameters[3], out float z))
                {
                    continue;
                }

                //change based on the movement type
                switch (move_type)
                {
                    case 0:
                        position_record["l_hand_position"] = Move_hand(x, y, z, false);

                        //warn that the parameters are out of bounds, resulting in sparsity issues
                        if (has_over_moved)
                            Debug.LogWarning($"Left hand over-moved at ({x}, {y}, {z}).");
                        break;
                    case 1:
                        position_record["r_hand_position"] = Move_hand(x, y, z, true);

                        //warn that the parameters are out of bounds, resulting in sparsity issues
                        if (has_over_moved)
                            Debug.LogWarning($"Right hand over-moved at ({x}, {y}, {z}).");
                        break;
                    case 2:
                        position_record["hip_rotation"] = Rotate_hip(x, y, z);

                        //warn that the parameters are out of bounds, resulting in sparsity issues
                        if (has_over_rotated)
                            Debug.LogWarning($"Hips over-rotated at ({x}, {y}, {z})");
                        break;
                    case 3:
                        position_record["l_leg_position"] = Move_legs(x, y, z, false);

                        //warn that the parameters are out of bounds, resulting in sparsity issues
                        if (has_over_moved)
                            Debug.LogWarning($"Left leg over-moved at ({x}, {y}, {z}).");
                        break;
                    case 4:
                        position_record["r_leg_position"] = Move_legs(x, y, z, true);

                        //warn that the parameters are out of bounds, resulting in sparsity issues
                        if (has_over_moved)
                            Debug.LogWarning($"Right leg over-moved at ({x}, {y}, {z}).");
                        break;
                    case 5:
                        position_record["body_position"] = Move_body(x, y, z);

                        //warn that the parameters are out of bounds, resulting in sparsity issues
                        if (has_over_moved)
                            Debug.LogWarning($"Body over-moved at ({x}, {y} (rotation), {z}).");
                        break;
                    default:
                        Debug.LogWarning("Incorrect move type from direction file");
                        break;
                }
            }
        }

        float energy_expenditure = energy_script.Calculate_energy();

        //record the hand position
        string data = string.Format("{0},\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\"\n",
                                    wall_id, position_record["l_hand_position"], position_record["r_hand_position"], position_record["hip_rotation"],
                                    position_record["l_leg_position"], position_record["r_leg_position"], position_record["body_position"], energy_expenditure);

        //record the position of everything
        File.AppendAllText(final_position_file, data);
    }

    // public (float, float, float) Rotate_hand(float x_angle, float y_angle, float z_angle, bool is_right_hand)
    // {
    //     has_over_rotated = false;

    //     //check if within human bounds (dont need to be too realistic)
    //     if (Math.Abs(x_angle) > 90f)
    //     {
    //         has_over_rotated = true;
    //         //set as max rotation
    //         if (x_angle > 0) x_angle = 90f;
    //         else x_angle = -90f;
    //     }
    //     if (Math.Abs(y_angle) > 90f)
    //     {
    //         has_over_rotated = true;
    //         //set as max rotation
    //         if (y_angle > 0) y_angle = 90f;
    //         else y_angle = -90f;
    //     }
    //     if (Math.Abs(z_angle) > 90f)
    //     {
    //         has_over_rotated = true;
    //         //set as max rotation
    //         if (z_angle > 0) z_angle = 90f;
    //         else z_angle = -90f;
    //     }


    //     UnityEngine.Vector3 r_current_rotation = right_hand_target.transform.eulerAngles;
    //     UnityEngine.Vector3 l_current_rotation = left_hand_target.transform.eulerAngles;

    //     //ASSUMING T POSE
    //     if (is_right_hand)
    //     {
    //         //rotate the hand based on current position
    //         r_current_rotation.x += x_angle;
    //         r_current_rotation.y += y_angle;
    //         r_current_rotation.z += z_angle;
    //         right_hand_target.transform.eulerAngles = r_current_rotation;
    //         return (r_current_rotation.x, r_current_rotation.y, r_current_rotation.z);
    //     }
    //     else
    //     {
    //         //rotate hand based on current position
    //         l_current_rotation.x += x_angle;
    //         l_current_rotation.y += y_angle;
    //         l_current_rotation.z += z_angle;
    //         left_hand_target.transform.eulerAngles = l_current_rotation;
    //         return (l_current_rotation.x, l_current_rotation.y, l_current_rotation.z);
    //     }
    // }

    //move hand from T-pose to the local position of the given transform
    public (float, float, float) Move_hand(float x_pos, float y_pos, float z_pos, bool is_right_hand)
    {
        has_over_moved = false;

        //find which one is the arm span limitation
        SphereCollider arm_span = is_right_hand ? right_arm_span : left_arm_span;
        Transform target = is_right_hand ? right_hand_target : left_hand_target;

        //move the hand based on local values; transformation based off the parents
        UnityEngine.Vector3 target_position = new(x_pos, y_pos, z_pos);
        target.localPosition = target_position;

        //check if within radius
        if (!arm_span.bounds.Contains(target.position))
        {
            has_over_moved = true;

            //move to the closest surface point of the sphere collider
            target.position = arm_span.ClosestPoint(target.position);
        }

        return (target.localPosition.x, target.localPosition.y, target.localPosition.z);
    }

    //no y_pos because we assume avatar can't jump/fly. y_rotation needs to be constrained to 0-360
    public (float, float, float) Move_body(float x_pos, float y_rotation, float z_pos)
    {
        has_over_moved = false;

        //store the original transformation
        UnityEngine.Vector3 start_pos = static_animator.transform.position;

        //boundary of the movement in world space rather than local
        UnityEngine.Vector3 boundary_scaled = UnityEngine.Vector3.Scale(movement_boundary.size, movement_boundary.transform.lossyScale);
        UnityEngine.Vector3 half_size = boundary_scaled * .5f;
        UnityEngine.Vector3 center = movement_boundary.transform.TransformPoint(movement_boundary.center);

        UnityEngine.Vector3 min = center - half_size;
        UnityEngine.Vector3 max = center + half_size;

        //track the amount of change
        float x_movement = x_pos;
        float z_movement = z_pos;

        //predict the final destination
        float final_x = static_animator.transform.position.x + x_pos;
        float final_z = static_animator.transform.position.z + z_pos;

        //check if the predicted transformation is within bounds. if not, snap to max or min position
        if (final_x > max.x)
        {
            final_x = max.x;
            x_movement = final_x - start_pos.x;
        }
        else if (final_x < min.x)
        {
            final_x = min.x;
            x_movement = final_x - start_pos.x;
        }

        if (final_z > max.z)
        {
            final_z = max.z;
            z_movement = final_z - start_pos.z;
        }
        else if (final_z < min.z)
        {
            final_z = min.z;
            z_movement = final_z - start_pos.z;
        }

        if (x_movement != x_pos || z_movement != z_pos) has_over_moved = true;

        //move avatar
        static_animator.transform.position = new(final_x, static_animator.transform.position.y, final_z);
        //rotate avatar
        static_animator.transform.eulerAngles = new(static_animator.transform.eulerAngles.x, static_animator.transform.eulerAngles.y + (y_rotation % 360), static_animator.transform.eulerAngles.z);

        return (x_movement, z_movement, static_animator.transform.eulerAngles.y);
    }


    public (float, float, float) Rotate_hip(float x_angle, float y_angle, float z_angle)
    {
        //x rotation bends forward(+) and back(-) Limit: (-30 degrees to 100) from Hip Flexion
        //y rotation twist side(r+) to side(l-) Limit: (-30 degrees to 30) from Thoraco-Lumbar Spine Rotation
        //z rotation bends side(l+) to side(r-) Limit: (-25 degrees to 25) from Thoraco-Lumbar Spine Lateral Flexion
        has_over_rotated = false;

        if (x_angle > 100f || x_angle < -30f)
        {
            has_over_rotated = true;
            //set as max/min rotation
            if (x_angle > 100f) x_angle = 100f;
            else x_angle = -30f;
        }
        if (Math.Abs(y_angle) > 30f)
        {
            has_over_rotated = true;
            //set as max/min rotation
            if (y_angle > 0) y_angle = 30f;
            else y_angle = -30f;
        }
        if (Math.Abs(z_angle) > 25f)
        {
            has_over_rotated = true;
            //set as max rotation
            if (z_angle > 0) z_angle = 25f;
            else z_angle = -25f;
        }

        UnityEngine.Vector3 hip_reposition = hip_target.eulerAngles;

        //rotate the hip based on current position
        hip_reposition.x += x_angle;
        hip_reposition.y += y_angle;
        hip_reposition.z += z_angle;
        hip_target.transform.eulerAngles = hip_reposition;

        return (hip_reposition.x, hip_reposition.y, hip_reposition.z);

    }

    public (float, float, float) Move_legs(float x_pos, float y_pos, float z_pos, bool isRight)
    {
        has_over_moved = false;

        //find which one is the leg span limitation
        MeshCollider leg_span = isRight ? right_leg_span : left_leg_span;
        Transform target = isRight ? right_leg_target : left_leg_target;

        //move the leg based on local values; transformation based off the parents
        UnityEngine.Vector3 target_position = new(x_pos, y_pos, z_pos);
        target.localPosition = target_position;

        //check if within radius
        if (!leg_span.bounds.Contains(target.position))
        {
            has_over_moved = true;

            //move to the closest surface point of the mesh collider
            target.position = leg_span.ClosestPoint(target.position);
        }

        return (target.localPosition.x, target.localPosition.y, target.localPosition.z);
    }

    public GameObject Reset_Controller()
    {
        //create and reset controller environment
        GameObject prefab = Resources.Load(prefab_path) as GameObject;
        GameObject new_instance = Instantiate(prefab, prefab_position, Quaternion.identity);
        new_instance.name = "Controller_AI_Scene";

        //delete itself
        Destroy(this.gameObject);

        return new_instance;
    }

    //testing
    private IEnumerator Generate_Movement(int move_type)
    {
        System.Random num_gen = new();

        float min_move = -5;
        float max_move = 5;

        float min_rotate = -360f;
        float max_rotate = 360f;

        float x = (float)(num_gen.NextDouble() * (max_move - min_move) + min_move);
        float y = (float)(num_gen.NextDouble() * (max_move - min_move) + min_move);
        float z = (float)(num_gen.NextDouble() * (max_move - min_move) + min_move);

        //change based on the movement type
        switch (move_type)
        {
            case 0:
                Debug.Log(Move_hand(x, y, z, false));

                //warn that the parameters are out of bounds, resulting in sparsity issues
                if (has_over_moved)
                    Debug.LogWarning($"Left hand over-moved at ({x}, {y}, {z}).");
                break;
            case 1:
                Debug.Log(Move_hand(x, y, z, true));

                //warn that the parameters are out of bounds, resulting in sparsity issues
                if (has_over_moved)
                    Debug.LogWarning($"Right hand over-moved at ({x}, {y}, {z}).");
                break;
            case 2:
                x = (float)(num_gen.NextDouble() * (max_rotate - min_rotate));
                y = (float)(num_gen.NextDouble() * (max_rotate - min_rotate));
                z = (float)(num_gen.NextDouble() * (max_rotate - min_rotate));
                Debug.Log(Rotate_hip(x, y, z));

                //warn that the parameters are out of bounds, resulting in sparsity issues
                if (has_over_rotated)
                    Debug.LogWarning($"Hips over-rotated at ({x}, {y}, {z})");
                break;
            case 3:
                Debug.Log(Move_legs(x, y, z, false));

                //warn that the parameters are out of bounds, resulting in sparsity issues
                if (has_over_moved)
                    Debug.LogWarning($"Left leg over-moved at ({x}, {y}, {z}).");
                break;
            case 4:
                Debug.Log(Move_legs(x, y, z, true));

                //warn that the parameters are out of bounds, resulting in sparsity issues
                if (has_over_moved)
                    Debug.LogWarning($"Right leg over-moved at ({x}, {y}, {z}).");
                break;
            case 5:
                y = (float)(num_gen.NextDouble() * (max_rotate - min_rotate));
                Debug.Log(Move_body(x, y, z));

                //warn that the parameters are out of bounds, resulting in sparsity issues
                if (has_over_moved)
                    Debug.LogWarning($"Body over-moved at ({x}, {y} (rotation), {z}).");
                break;
            default:
                Debug.LogWarning("Incorrect move type");
                break;
        }
        yield return new WaitForSeconds(3.0f);

        Reset_Controller();
    }

    private void Generate_Movement_File(int move_count)
    {
        string data = "";

        System.Random num_gen = new();
        int move_type;
        
        while (move_count > 0)
        {
            move_type = UnityEngine.Random.Range(0, 6);

            float min_move = -5;
            float max_move = 5;

            float min_rotate = -360f;
            float max_rotate = 360f;

            float x = (float)(num_gen.NextDouble() * (max_move - min_move) + min_move);
            float y = (float)(num_gen.NextDouble() * (max_move - min_move) + min_move);
            float z = (float)(num_gen.NextDouble() * (max_move - min_move) + min_move);

            //change based on the movement type
            switch (move_type)
            {
                case 0:
                    data += string.Format("{0},{1},{2},{3}\n", move_type, x, y, z);
                    break;
                case 1:
                    data += string.Format("{0},{1},{2},{3}\n", move_type, x, y, z);
                    break;
                case 2:
                    x = (float)(num_gen.NextDouble() * (max_rotate - min_rotate));
                    y = (float)(num_gen.NextDouble() * (max_rotate - min_rotate));
                    z = (float)(num_gen.NextDouble() * (max_rotate - min_rotate));
                    data += string.Format("{0},{1},{2},{3}\n", move_type, x, y, z);
                    break;
                case 3:
                    data += string.Format("{0},{1},{2},{3}\n", move_type, x, y, z);
                    break;
                case 4:
                    data += string.Format("{0},{1},{2},{3}\n", move_type, x, y, z);
                    break;
                case 5:
                    y = (float)(num_gen.NextDouble() * (max_rotate - min_rotate));
                    data += string.Format("{0},{1},{2},{3}\n", move_type, x, y, z);
                    break;
                default:
                    Debug.LogWarning("Incorrect move type");
                    break;
            }

            move_count--;
        }

        using (var file_writer = new StreamWriter(direction_file, false))
        {
            file_writer.WriteLine(data);
        }

        Read_movement_file(-1);
    }

}
