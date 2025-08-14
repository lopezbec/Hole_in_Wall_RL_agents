using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class EnergyExpenditure : MonoBehaviour
{
    //https://journals.plos.org/plosone/article?id=10.1371/journal.pone.0127113#pone.0127113.e007

    [Header("Floor Reference")]
    public Transform floor;


    [Header("Hip/Pelvis Joint")]
    public Transform root_joint;


    [Header("Joints With ConfigurableJoint Components")]
    public GameObject left_up_leg;
    public GameObject right_up_leg;
    public GameObject left_leg;
    public GameObject right_leg;
    public GameObject spine_1;
    public GameObject left_arm;
    public GameObject right_arm;
    public GameObject left_forearm;
    public GameObject right_forearm;


    [Header("Limb to find COM")]
    public GameObject left_hand;
    public GameObject right_hand;
    public GameObject left_foot;
    public GameObject right_foot;

    private readonly float avg_mass = 70000;                                // grams
    private readonly float gravity_accel = Mathf.Abs(Physics.gravity.y);    // meters / second^2
    private readonly float time = 1f;                                       // second       

    private UnityEngine.Vector3 com_offset = new(0f, 0f, 0f);                           // center of mass offset; currently unknown - no access to paper cited            
    private UnityEngine.Vector3 com_pos;                                                // Vector3 com_pos = root_joint.position + com_offset;
    private UnityEngine.Vector3 initial_com_pos;                                       // initial com position
    private float initial_com_energy;

    private Dictionary<string, float> limb_length = new();
    private Dictionary<string, float> limb_mass = new();
    private Dictionary<string, UnityEngine.Vector3> limb_gyration = new();
    private Dictionary<string, UnityEngine.Vector3> initial_limb_com;
    private Dictionary<string, UnityEngine.Quaternion> initial_limb_rotation;
    private float initial_int_energy;

    void Awake()
    {
        //find the initial positions of the avatar
        com_pos = root_joint.position + com_offset;
        initial_com_pos = com_pos;

        //find Ecom(t) when t = 0
        initial_com_energy = Calculate_COM_WB();

        //https://wiki.has-motion.com/doku.php?id=visual3d:documentation:definitions:adjusted_zatsiorsky-seluyanov_s_segment_inertia_parameters
        limb_mass["left_up_leg"] = 0.1416f * avg_mass;
        limb_mass["left_leg"] = 0.0433f * avg_mass;
        limb_mass["left_arm"] = 0.0271f * avg_mass;
        limb_mass["left_forearm"] = 0.0162f * avg_mass;
        limb_mass["spine_1"] = 0.4346f * avg_mass;
        limb_mass["right_up_leg"] = limb_mass["left_up_leg"];
        limb_mass["right_leg"] = limb_mass["left_leg"];
        limb_mass["right_arm"] = limb_mass["left_arm"];
        limb_mass["right_forearm"] = limb_mass["left_forearm"];

        //https://www.researchgate.net/figure/Dimensions-of-average-male-human-being-23_fig1_283532449
        limb_length["left_up_leg"] = .46f;
        limb_length["left_leg"] = .45f;
        limb_length["left_arm"] = 0.302f;
        limb_length["left_forearm"] = .269f;
        limb_length["spine_1"] = 0.488f;
        limb_length["right_up_leg"] = limb_mass["left_up_leg"];
        limb_length["right_leg"] = limb_mass["left_leg"];
        limb_length["right_arm"] = limb_mass["left_arm"];
        limb_length["right_forearm"] = limb_mass["left_forearm"];

        //find the radii of gyration
        limb_gyration["left_up_leg"] = new(0.329f, 0.329f, 0.149f);
        limb_gyration["left_leg"] = new(0.251f, 0.246f, 0.102f);
        limb_gyration["left_arm"] = new(0.285f, 0.269f, 0.158f);
        limb_gyration["left_forearm"] = new(0.276f, 0.265f, 0.121f);
        limb_gyration["spine_1"] = new(0.328f, 0.306f, 0.169f);
        limb_gyration["right_up_leg"] = limb_gyration["left_up_leg"];
        limb_gyration["right_leg"] = limb_gyration["left_leg"];
        limb_gyration["right_arm"] = limb_gyration["left_arm"];
        limb_gyration["right_forearm"] = limb_gyration["left_forearm"];

        //find the initial rotations of the limbs
        initial_limb_rotation = Store_limb_rotation();

        //find the COM of each limb
        initial_limb_com = Store_limb_com();

        //find Eint(t) when t = 0
        initial_int_energy = Calculate_energy_transfer();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public float Calculate_energy()
    {
        //find external work -> Ecom(t+1) - Ecom(t) where t = 0
        float external_work = Calculate_COM_WB() - initial_com_energy;

        //find internal work -> Eint(t+1) - Eint(t) where t = 0
        float internal_work = Calculate_energy_transfer() - initial_int_energy;

        return external_work + internal_work;
    }

    // center of mass for whole body. 
    // E = (mass)(gravity accel)(height at time t) + (1/2)(mass)(linear velocity of COM at time t)^2
    // disregard time, not a factor in calculations here. 
    // units : g(m)^2 / (s)^2
    private float Calculate_COM_WB()
    {
        UnityEngine.Vector3 com_velocity;

        //recompute com position
        com_pos = root_joint.position + com_offset;

        //find the velocity of center of mass
        if (time == 0) com_velocity = new(0f, 0f, 0f);
        else com_velocity = (com_pos - initial_com_pos) / time;

        float l_velocity = com_velocity.magnitude;

        //find height from the floor
        float height = com_pos.y - floor.position.y;

        //calculate center of mass for whole body - not isolating negative and positive work
        return Math.Abs((avg_mass * gravity_accel * height) + (0.5f * avg_mass * l_velocity * l_velocity));
    }

    // energy transfer between the limbs
    // E = (1/2)(limb mass)(relative velocity)^2 + (1/2)(limb mass)(radius of gyration)^2(angular velocity)^2
    // to get gyration inertia: https://wiki.has-motion.com/doku.php?id=visual3d:documentation:definitions:adjusted_zatsiorsky-seluyanov_s_segment_inertia_parameters
    private float Calculate_energy_transfer()
    {
        float combined_int_energy = 0f;

        combined_int_energy += Energy_transfer_helper("left_up_leg");
        combined_int_energy += Energy_transfer_helper("right_up_leg");
        combined_int_energy += Energy_transfer_helper("left_leg");
        combined_int_energy += Energy_transfer_helper("right_leg");
        combined_int_energy += Energy_transfer_helper("left_arm");
        combined_int_energy += Energy_transfer_helper("right_arm");
        combined_int_energy += Energy_transfer_helper("left_forearm");
        combined_int_energy += Energy_transfer_helper("right_forearm");
        combined_int_energy += Energy_transfer_helper("spine1");

        return combined_int_energy;
    }

    // E = (1/2)(limb mass)(relative velocity)^2 + (1/2)(limb mass)(radius of gyration)^2(angular velocity)^2
    private float Energy_transfer_helper(string limb_part)
    {
        //get the new positions
        UnityEngine.Vector3 limb_com_velocity;
        UnityEngine.Quaternion limb_ang_delta;
        UnityEngine.Vector3 new_limb_com = Store_limb_com()[limb_part];
        UnityEngine.Quaternion new_limb_rotation = Store_limb_rotation()[limb_part];

        //get the mass of the limb
        float mass = limb_mass[limb_part];

        //find the velocity of the limb
        if (time == 0) limb_com_velocity = new(0f, 0f, 0f);
        else limb_com_velocity = (new_limb_com - initial_limb_com[limb_part]) / time;
        float r_velocity = limb_com_velocity.magnitude;

        //find the angular velocity -> separate rotation direction and angle from quaternion -> convert to radians -> calculate 
        limb_ang_delta = new_limb_rotation * UnityEngine.Quaternion.Inverse(initial_limb_rotation[limb_part]);
        limb_ang_delta.ToAngleAxis(out float angle_deg, out UnityEngine.Vector3 axis);
        float angle_rad = angle_deg * Mathf.Deg2Rad;
        UnityEngine.Vector3 a_velocity = axis * (angle_rad / time);

        //find the gyration's inertia
        UnityEngine.Vector3 gyration_squared = new()
        {
            x = limb_mass[limb_part] * avg_mass * (float)Math.Pow(limb_length[limb_part] * limb_gyration[limb_part].x, 2),
            y = limb_mass[limb_part] * avg_mass * (float)Math.Pow(limb_length[limb_part] * limb_gyration[limb_part].y, 2),
            z = limb_mass[limb_part] * avg_mass * (float)Math.Pow(limb_length[limb_part] * limb_gyration[limb_part].z, 2),
        };

        //gyration^2 * angular_velocity^2
        float g2_av2 =
                gyration_squared.x * a_velocity.x * a_velocity.x +
                gyration_squared.y * a_velocity.y * a_velocity.y +
                gyration_squared.z * a_velocity.z * a_velocity.z;

        //calculate the interal energy - not isolating negative and positive work
        return Math.Abs((0.5f * mass * r_velocity * r_velocity) + (0.5f * mass * g2_av2));
    }

    private Dictionary<string, UnityEngine.Vector3> Store_limb_com()
    {
        Dictionary<string, UnityEngine.Vector3> limb_com = new()
        {
            ["left_up_leg"] = (left_up_leg.transform.position + left_leg.transform.position) / 2f,
            ["right_up_leg"] = (right_up_leg.transform.position + right_leg.transform.position) / 2f,
            ["left_leg"] = (left_leg.transform.position + left_foot.transform.position) / 2f,
            ["right_leg"] = (right_leg.transform.position + right_foot.transform.position) / 2f,
            ["left_arm"] = (left_arm.transform.position + left_forearm.transform.position) / 2f,
            ["right_arm"] = (right_arm.transform.position + right_forearm.transform.position) / 2f,
            ["left_forearm"] = (left_forearm.transform.position + left_hand.transform.position) / 2f,
            ["right_forearm"] = (right_forearm.transform.position + right_hand.transform.position) / 2f
        };

        return limb_com;
    }

    private Dictionary<string, UnityEngine.Quaternion> Store_limb_rotation()
    {
        Dictionary<string, UnityEngine.Quaternion> limb_rotation = new()
        {
            //find the initial rotations of the limbs
            ["left_up_leg"] = left_up_leg.transform.rotation,
            ["right_up_leg"] = right_up_leg.transform.rotation,
            ["left_leg"] = left_leg.transform.rotation,
            ["right_leg"] = right_leg.transform.rotation,
            ["left_arm"] = left_arm.transform.rotation,
            ["right_arm"] = right_arm.transform.rotation,
            ["left_forearm"] = left_forearm.transform.rotation,
            ["right_forearm"] = right_forearm.transform.rotation,
            ["spine_1"] = spine_1.transform.rotation
        };

        return limb_rotation;
    }
}
