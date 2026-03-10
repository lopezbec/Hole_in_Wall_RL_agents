using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class ObstacleGenerator : MonoBehaviour
{
    private float wall_height = 2.25f;                                              // the set size of the wall's height
    private float wall_width = 3f;                                                  // the set size of the wall's width
    private float leftover_height;                                                  // the remaining wall height required to make the set wall
    private float leftover_width;                                                   // the remaining wall width required to make the set wall
    private int wall_col = 10;                                                      // the amount of columns for the matrix
    private int wall_row = 9;                                                       // the amount of rows for the matrix
    public float block_height = 0.25f;                                              // the block height size
    public float block_width = 0.3f;                                                // the block width size
    public float block_depth = 0.2f;                                                // the block thickness size
    public int[,] wall;                                                             // matrix representing the wall with holes. 0 = hole, 1 = wall

    public GameObject boundaryObj;

    //second way to generate the wall
    public int custom_cube_amt = 5;
    private float move_spd = .1f;
    public AvatarController avatar_script;

    private List<int[,]> test_walls = new();
    private int test_id = -1;
    private static int[,] full_wall_matrix = {  {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                                                {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                                                {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                                                {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                                                {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                                                {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                                                {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                                                {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                                                {1, 1, 1, 1, 1, 1, 1, 1, 1, 1}
                                             };
    private readonly string wall_save_file = Application.dataPath + "/Scripts/Reinforcement_Learning/wall_tests.txt";

    // Start is called before the first frame update
    void Start()
    {
        //normal run
        //initialize_wall_generation(false);
        initialize_wall_generation_large(3.5f, false);

        //add walls that should incentivize limb movement, but are impossible to pass
        //generate_limb_walls();

        //reduced wall set
        //initialize_wall_generation(true);

        //debug and creation of walls
        //test_id = 51;
        //initialize_specific_wall(false, test_id);
        //initialize_complete_wall();

        // float[,] block_param = {    {.25f, 0, 0, .25f, .5f},
        //                             {0, .5f, 0, .25f, .5f},
        //                             {0, 0, 0, .25f, .5f},
        //                             {.5f, .5f, 0, .25f, .5f},
        //                             {.5f, .25f, 0, .25f, 1f}
        //                         };

        // custom_build(block_param);
        if(!boundaryObj) Debug.Log("Boundary Object is not initialized");
    }

    // Update is called once per frame
    void Update()
    {
        Move_wall();
    }


    //using matrix wall generation
    public void initialize_wall_generation(bool is_reduced)
    {
        if (!is_reduced) generate_test_walls();
        else generate_test_walls_reduced();

        int[,] obstacle = select_random_test(-1);
        Build_wall(obstacle);
    }

        public void initialize_wall_generation_large(float wall_depth, bool is_reduced)
    {
        block_depth = wall_depth;

        if (!is_reduced) generate_test_walls();
        else generate_test_walls_reduced();

        int[,] obstacle = select_random_test(-1);
        Build_wall(obstacle);
    }
    //matrix wall generation
    public void initialize_specific_wall(bool is_reduced, int test_num)
    {
        if (!is_reduced) generate_test_walls();
        else generate_test_walls_reduced();

        int[,] obstacle = select_random_test(test_num);
        Build_wall(obstacle);
    }


    //full wall using matrix generation
    public void initialize_complete_wall()
    {
        int[,] obstacle = full_wall_matrix;

        Build_wall(obstacle);
    }

    //for ML wall reset
    public void Reset_Wall()
    {
        int[,] test_wall = select_random_test(test_id);
        Build_wall(test_wall);
        this.gameObject.SetActive(true);
        if(boundaryObj) boundaryObj.SetActive(false);
    }

    public void Set_dim()
    {
        //find the amount of rows and columns
        wall_row = (int)Math.Floor(wall_height / block_height);
        wall_col = (int)Math.Floor(wall_width / block_width);

        //find the remaining height and width to have the same set wall size
        leftover_height = wall_height % block_height;
        leftover_width = wall_height % block_height;

        //increment the row and col if there are leftover heights
        if (leftover_height != 0) wall_row++;
        if (leftover_width != 0) wall_col++;
        //check if the given size is within the set size of the total wall
        if ((block_height > wall_height) || (block_width > wall_width))
        {
            //set the dimensions if over the size
            if (block_height > wall_height)
            {
                wall_row = 1;
                block_height = wall_height;
                leftover_height = 0f;
            }

            if (block_width > wall_width)
            {
                wall_col = 1;
                block_width = wall_width;
                leftover_width = 0f;
            }
        }

        wall = new int[wall_row, wall_col];
    }

    //build set dimension wall according to size of cubes
    public void Build_wall(int[,] given_wall)
    {
        //destroy all children first
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        
        //set the dimension
        Set_dim();
        Set_wall_matrix(given_wall);

        //make sure the wall is above the ground
        float ground_offset = block_height / 2;

        GameObject parent_obj = new();
        parent_obj.transform.SetParent(transform);
        parent_obj.transform.position = transform.position;
        parent_obj.SetActive(false);

        //fill from the bottom up
        for (int i = wall_row - 1; i >= 0; i--)
        {
            for (int j = 0; j < wall_col; j++)
            {
                // if not 0, then is wall block
                if (wall[i, j] != 0)
                {
                    float height = block_height;
                    float width = block_width;

                    //calculate centered offset 
                    float x_offset = (j - (wall_col - 1) / 2f) * block_width;
                    float y_offset = (wall_row - 1 - i) * block_height;

                    //create the cube block
                    GameObject rect_obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    rect_obj.name = i + " , " + j;

                    rect_obj.tag = "Walls";
                    rect_obj.GetComponent<BoxCollider>().isTrigger = true;

                    //make sure the last col or row reflects the correct sizing
                    if (i == 0 && leftover_height != 0)
                    {
                        height = leftover_height;
                        //adjust the positioning of the remaining block
                        y_offset -= (block_height - leftover_height) / 2f;
                    }
                    else if (j == wall_col - 1 && leftover_width != 0)
                    {
                        width = leftover_width;
                        //adjust the positioning of the remaining block
                        x_offset -= (block_width - leftover_width) / 2f;
                    }

                    //create the block with right size
                    rect_obj.transform.localScale = new Vector3(width, height, block_depth);
                    // set parent to this GameObject
                    rect_obj.transform.SetParent(parent_obj.transform);

                    // apply local offset relative to this transform
                    Vector3 localOffset = new(x_offset, y_offset + ground_offset, 0f);
                    rect_obj.transform.position = parent_obj.transform.TransformPoint(localOffset);
                    rect_obj.AddComponent<Reenable_Colliders>();

                }
            }
        }
    }

    // public void build_wall()
    // {
    //     // destroy all children first
    //     foreach (Transform child in transform)
    //     {
    //         GameObject.Destroy(child.gameObject);
    //     }

    //     // build the wall based on the vector matrix.
    //     for (int i = 0; i < wall_row; i++)
    //     {
    //         for (int j = 0; j < wall_col; j++)
    //         {
    //             // if not 0, then is a wall block
    //             if (wall[i, j] != 0)
    //             {
    //                 // calculate centered offset 
    //                 float x_offset = (j - (wall_col - 1) / 2f) * block_width;
    //                 float y_offset = (wall_row - 1 - i) * block_height;

    //                 // create the cube block
    //                 GameObject rect_obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //                 rect_obj.name = i + " , " + j;
    //                 rect_obj.transform.localScale = new Vector3(block_width, block_height, block_depth);

    //                 // set parent to this GameObject
    //                 rect_obj.transform.SetParent(transform);

    //                 // apply local offset relative to this transform
    //                 Vector3 localOffset = new Vector3(x_offset, y_offset, 0f);
    //                 rect_obj.transform.position = transform.TransformPoint(localOffset);
    //             }

    //         }
    //     }
    // }

    public void Set_wall(int row, int col)
    {
        if (row < wall_row && col < wall_col) wall[row, col] = 1;
    }

    public void Set_hole(int row, int col)
    {
        if (row < wall_row && col < wall_col) wall[row, col] = 0;
    }

    public void Set_wall_matrix(int[,] given_wall)
    {
        //the limits of the given wall
        int copied_row_limit = given_wall.GetLength(0);
        int copied_col_limit = given_wall.GetLength(1);

        // copy the matrix given as closely as possible (esp if not the same size)
        for (int i = 0; i < wall_row; i++)
        {
            for (int j = 0; j < wall_col; j++)
            {
                //copy the values from the given wall, automatically a wall if not available
                if (i < copied_row_limit && j < copied_col_limit) wall[i, j] = given_wall[i, j];
                else wall[i, j] = 1;
            }
        }
    }

    public int Get_col()
    {
        return wall_col;
    }

    public int Get_row()
    {
        return wall_row;
    }

    //second way to build wall
    public void custom_build(float[,] cube_parameters)
    {
        //check if the array has the correct amount of parameters for each of the cubes
        if (cube_parameters.GetLength(0) != custom_cube_amt || cube_parameters.GetLength(1) != 5) return;

        for (int i = 0; i < custom_cube_amt; i++)
        {
            //create the wall based off the items in the array
            custom_build_wall(cube_parameters[i, 0], cube_parameters[i, 1], cube_parameters[i, 2], cube_parameters[i, 3], cube_parameters[i, 4]);
        }
    }

    public void custom_build_wall(float x_axis, float y_axis, float z_axis, float custom_width, float custom_height)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        cube.tag = "Walls";
        cube.GetComponent<BoxCollider>().isTrigger = true;

        //create the block with right size
        cube.transform.localScale = new Vector3(custom_width, custom_height, block_depth);
        // set parent to this GameObject
        cube.transform.SetParent(transform);

        // apply local offset relative to this transform
        Vector3 localOffset = new(x_axis, y_axis, z_axis);
        cube.transform.position = transform.TransformPoint(localOffset);
    }

    private void Move_wall()
    {   
        if (avatar_script.completed_pose) {
            transform.GetChild(0).gameObject.SetActive(true);
            if(boundaryObj) boundaryObj.gameObject.SetActive(true);
            transform.position = new(transform.position.x, transform.position.y, transform.position.z - move_spd);
        }
    }

    private int[,] select_random_test(int wall_id)
    {
        //select the wall if given valid index
        if (wall_id >= 0)
        {
            if (wall_id < test_walls.Count)
            {
                return test_walls[wall_id];
            }
        }

        System.Random rand = new();

        //when test id is -1 or lower, do random select
        return test_walls[rand.Next(0, test_walls.Count)];
    }

    // int[,] test_wall =  {   {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
    //                         {1, 1, 1, 1, 0, 1, 1, 1, 1, 1},
    //                         {1, 1, 0, 1, 1, 1, 1, 1, 1, 1},
    //                         {0, 0, 0, 1, 1, 1, 1, 1, 1, 1},
    //                         {0, 1, 0, 1, 1, 1, 1, 1, 1, 1},
    //                         {0, 1, 0, 1, 1, 1, 1, 1, 1, 1},
    //                         {0, 0, 0, 1, 1, 1, 1, 1, 1, 1},
    //                         {1, 0, 0, 1, 1, 1, 1, 1, 1, 1},
    //                         {0, 1, 0, 1, 1, 1, 1, 1, 1, 1}
    //                     };

    public void save_wall(string pose_name)
    {
        //save the current wall into the test walls
        int[,] saved_wall = new int[wall_row, wall_col];
        
        Transform level = this.gameObject.transform.GetChild(0);
        
        Transform[] wall_children = level.GetComponentsInChildren<Transform>();

        foreach (Transform child in wall_children)
        {
            if (child != level && child != transform) // check if it's not the parent itself
            {
                string[] parts = child.name.Split(',');

                int child_row = int.Parse(parts[0]);
                int child_col = int.Parse(parts[1]);

                saved_wall[child_row, child_col] = 1;
            }
        }

        //write to file
        using (var file_writer = new StreamWriter(wall_save_file, true))
        {
            file_writer.WriteLine("// Wall for pose: " + pose_name + "\n");

            for (int i = 0; i < wall_row; i++)
            {
                if (i == 0) file_writer.Write("{"); //start of wall

                for (int j = 0; j < wall_col; j++)
                {
                    if (j == 0) file_writer.Write("{"); //start of row
                    file_writer.Write(saved_wall[i, j]);
                    if (j == wall_col - 1) file_writer.Write("}"); //end of row

                    if (j < wall_col - 1) file_writer.Write(","); //comma separation except last
                }
                if (i < wall_row - 1) file_writer.Write(","); //comma separation between rows
                else file_writer.Write("}\n\n"); //new line after last row
            }

        }

        Debug.Log("Saved wall for pose: " + pose_name);
    }



    private void generate_test_walls()
    {
        //assuming the default size

        //default pose
        int[,] wall_0 = {   { 1,1,1,1,1,1,1,1,1,1},
                            { 1,1,1,1,0,1,1,1,1,1},
                            { 1,1,1,1,0,1,1,1,1,1},
                            { 1,1,1,0,0,0,1,1,1,1},
                            { 1,1,1,0,0,0,1,1,1,1},
                            { 1,1,1,0,0,0,1,1,1,1},
                            { 1,1,1,0,0,0,1,1,1,1},
                            { 1,1,1,0,0,0,1,1,1,1},
                            { 1,1,1,0,0,0,1,1,1,1} };

        test_walls.Add(wall_0);

        //default pose but shifted to the left
        int[,] wall_1 = {   { 1,1,1,1,1,1,1,1,1,1},
                            { 1,1,0,1,1,1,1,1,1,1},
                            { 1,1,0,1,1,1,1,1,1,1},
                            { 1,0,0,0,1,1,1,1,1,1},
                            { 1,0,0,0,1,1,1,1,1,1},
                            { 1,0,0,0,1,1,1,1,1,1},
                            { 1,0,0,0,1,1,1,1,1,1},
                            { 1,0,0,0,1,1,1,1,1,1},
                            { 1,0,0,0,1,1,1,1,1,1} };
        test_walls.Add(wall_1);

        //Hand Side Right
        int[,] wall_2 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 0, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 } };
        test_walls.Add(wall_2);

        //Hand Side Left
        int[,] wall_3 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 0, 1, 1, 1 }, { 1, 1, 1, 1, 0, 0, 1, 1, 1 }, { 1, 1, 0, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 1, 0, 0, 1, 1, 1 }, { 1, 1, 1, 1, 0, 0, 1, 1, 1 }, { 1, 1, 1, 1, 0, 0, 1, 1, 1 }, { 1, 1, 1, 1, 0, 0, 1, 1, 1 }, { 1, 1, 1, 1, 0, 0, 1, 1, 1 } };
        test_walls.Add(wall_3);

        //T-Pose
        int[,] wall_4 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 0, 1, 1, 1 }, { 1, 1, 1, 1, 0, 0, 1, 1, 1 }, { 1, 1, 0, 0, 0, 0, 0, 0, 1 }, { 1, 1, 1, 1, 0, 0, 1, 1, 1 }, { 1, 1, 1, 1, 0, 0, 1, 1, 1 }, { 1, 1, 1, 1, 0, 0, 1, 1, 1 }, { 1, 1, 1, 1, 0, 0, 1, 1, 1 }, { 1, 1, 1, 1, 0, 0, 1, 1, 1 } };
        test_walls.Add(wall_4);

        //Right leg lean forward
        int[,] wall_5 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 0, 1, 1, 1 }, { 1, 1, 0, 0, 0, 0, 0, 1, 1 }, { 1, 1, 1, 1, 0, 0, 1, 1, 1 }, { 1, 1, 1, 1, 0, 0, 1, 1, 1 }, { 1, 1, 1, 1, 0, 0, 1, 1, 1 }, { 1, 1, 1, 1, 0, 0, 1, 1, 1 }, { 1, 1, 1, 1, 0, 0, 1, 1, 1 } };
        test_walls.Add(wall_5);

        //Left leg lean forward
        int[,] wall_6 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 0, 0, 0, 0, 0, 0, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 } };
        test_walls.Add(wall_6);

        //Hand Behind Back, Rotated Sideways
        int[,] wall_7 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 1, 1, 1, 1 } };
        test_walls.Add(wall_7);

        //Right Leg Sightly Raised
        int[,] wall_8 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 0, 0, 0, 1, 1, 1, 1 }, { 1, 1, 0, 0, 0, 1, 1, 1, 1 }, { 1, 1, 0, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_8);

        //Right Leg Slightly Raised Mirror
        int[,] wall_9 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 0, 0, 1, 1, 1, 1, 1, 1 }, { 1, 0, 0, 1, 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 1, 1, 1, 1, 1 }, { 0, 0, 0, 0, 1, 1, 1, 1, 1 }, { 1, 0, 0, 1, 1, 1, 1, 1, 1 }, { 1, 0, 0, 1, 1, 1, 1, 1, 1 }, { 1, 1, 0, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_9);

        //Right Leg 90 Raised, Rotated Sideways
        int[,] wall_10 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 0, 1, 1 }, { 1, 1, 1, 1, 1, 0, 0, 1, 1 }, { 1, 1, 1, 1, 1, 0, 0, 1, 1 }, { 1, 1, 1, 1, 1, 0, 0, 1, 1 }, { 1, 1, 1, 1, 1, 0, 0, 0, 1 }, { 1, 1, 1, 1, 1, 1, 0, 0, 1 }, { 1, 1, 1, 1, 1, 1, 0, 0, 1 }, { 1, 1, 1, 1, 1, 1, 0, 1, 1 } };
        test_walls.Add(wall_10);

        //Right Leg 90 Raised, Rotated Sideways Mirror
        int[,] wall_11 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 0, 1, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 0, 0, 0, 1, 1, 1, 1 }, { 1, 1, 0, 0, 1, 1, 1, 1, 1 }, { 1, 1, 0, 0, 1, 1, 1, 1, 1 }, { 1, 1, 1, 0, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_11);

        //Squat
        int[,] wall_12 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 } };
        test_walls.Add(wall_12);

        //Squat V2
        int[,] wall_13 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 0, 1, 1, 1, 1, 1, 1 }, { 1, 1, 0, 1, 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_13);

        //Squat V3
        int[,] wall_14 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 0, 1 }, { 1, 1, 1, 1, 1, 1, 1, 0, 1 }, { 1, 1, 1, 1, 1, 1, 0, 0, 0 }, { 1, 1, 1, 1, 1, 1, 0, 0, 0 }, { 1, 1, 1, 1, 1, 1, 0, 0, 0 }, { 1, 1, 1, 1, 1, 1, 0, 0, 0 } };
        test_walls.Add(wall_14);

        //Wide Stance
        int[,] wall_15 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 0, 0, 1, 0, 0, 1, 1 }, { 1, 1, 0, 0, 1, 0, 0, 1, 1 } };
        test_walls.Add(wall_15);

        //Kneel
        int[,] wall_16 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 0, 1, 1, 1 }, { 1, 1, 1, 1, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 0, 1, 1 }, { 1, 1, 1, 1, 0, 0, 1, 1, 1 } };
        test_walls.Add(wall_16);

        int[,] wall_17 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 0, 0, 1, 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 0, 1, 1, 1, 1 }, { 1, 0, 0, 0, 0, 1, 1, 1, 1 }, { 1, 0, 0, 1, 1, 1, 1, 1, 1 }, { 1, 0, 0, 1, 1, 1, 1, 1, 1 }, { 1, 0, 0, 1, 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_17);

        //Leg Lift V2
        int[,] wall_18 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 0, 1, 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 1, 1, 1, 1, 1 }, { 1, 1, 0, 0, 1, 1, 1, 1, 1 }, { 1, 1, 0, 0, 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 1, 1, 1, 1, 1 }, { 0, 0, 0, 1, 1, 1, 1, 1, 1 }, { 0, 0, 0, 1, 1, 1, 1, 1, 1 }, { 1, 1, 0, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_18);

        //Leg Lift V2 (mirror)
        int[,] wall_19 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 0, 1, 1, 1, 1, 1, 1, 1 }, { 0, 0, 0, 1, 1, 1, 1, 1, 1 }, { 0, 0, 1, 1, 1, 1, 1, 1, 1 }, { 0, 0, 1, 1, 1, 1, 1, 1, 1 }, { 0, 0, 0, 1, 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 1, 1, 1, 1, 1 }, { 1, 0, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_19);

        //Leg Lift V2 (mirror, shifted right)
        int[,] wall_20 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 0, 1, 1, 1, 1, 1 }, { 1, 1, 0, 0, 0, 1, 1, 1, 1 }, { 1, 1, 0, 0, 1, 1, 1, 1, 1 }, { 1, 1, 0, 0, 1, 1, 1, 1, 1 }, { 1, 1, 0, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_20);

        //Leg Lift V2 (shifted right)
        int[,] wall_21 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 0, 1, 1 }, { 1, 1, 1, 1, 1, 0, 0, 0, 1 }, { 1, 1, 1, 1, 1, 1, 0, 0, 1 }, { 1, 1, 1, 1, 1, 1, 0, 0, 1 }, { 1, 1, 1, 1, 1, 0, 0, 0, 1 }, { 1, 1, 1, 1, 0, 0, 0, 1, 1 }, { 1, 1, 1, 1, 0, 0, 0, 1, 1 }, { 1, 1, 1, 1, 1, 1, 0, 1, 1 } };
        test_walls.Add(wall_21);

        //Leg Lift V3
        int[,] wall_22 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 0, 1, 1, 1 }, { 1, 1, 1, 1, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 0, 0, 0 }, { 1, 1, 1, 0, 0, 0, 0, 0, 1 }, { 1, 1, 1, 1, 1, 0, 1, 1, 1 }, { 1, 1, 1, 1, 1, 0, 1, 1, 1 } };
        test_walls.Add(wall_22);

        //Leg Lift V3 (shifted left)
        int[,] wall_23 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 0, 0, 0, 1, 1, 1, 1 }, { 1, 1, 0, 0, 0, 0, 0, 0, 1 }, { 1, 1, 0, 0, 0, 0, 0, 1, 1 }, { 1, 1, 1, 1, 0, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 1, 1, 1, 1 } };
        test_walls.Add(wall_23);

        //Leg Lift V3 (mirror)
        int[,] wall_24 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 0, 0, 0, 0, 0, 0, 1, 1, 1 }, { 1, 0, 0, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 1, 1, 1, 1, 1 }, { 1, 1, 1, 0, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_24);

        //Leg Lift V3 (mirror, shifted right)
        int[,] wall_25 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 0, 0, 1 }, { 1, 1, 1, 1, 1, 1, 0, 0, 1 }, { 1, 1, 1, 1, 1, 1, 0, 0, 0 }, { 1, 1, 1, 0, 0, 0, 0, 0, 0 }, { 1, 1, 1, 1, 0, 0, 0, 0, 0 }, { 1, 1, 1, 1, 1, 1, 0, 1, 1 }, { 1, 1, 1, 1, 1, 1, 0, 1, 1 } };
        test_walls.Add(wall_25);

        //Wide Stance V2
        int[,] wall_26 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 0, 0, 0, 0, 0, 1, 1 }, { 1, 0, 0, 0, 1, 0, 0, 1, 1 } };
        test_walls.Add(wall_26);

        //Wide Stance (shifted left)
        int[,] wall_27 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 0, 1, 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 1, 1, 1, 1, 1 }, { 0, 0, 1, 0, 0, 1, 1, 1, 1 }, { 0, 0, 1, 0, 0, 1, 1, 1, 1 } };
        test_walls.Add(wall_27);

        //Wide Stance (shifted right)
        int[,] wall_28 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 0, 1, 1 }, { 1, 1, 1, 1, 1, 0, 0, 0, 1 }, { 1, 1, 1, 1, 1, 0, 0, 0, 1 }, { 1, 1, 1, 1, 1, 0, 0, 0, 1 }, { 1, 1, 1, 1, 1, 0, 0, 0, 1 }, { 1, 1, 1, 1, 0, 0, 1, 0, 0 }, { 1, 1, 1, 1, 0, 0, 1, 0, 0 } };
        test_walls.Add(wall_28);

        //Wide Stance V2 (shifted right)
        int[,] wall_29 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 0, 1, 1, 1 }, { 1, 1, 1, 1, 0, 0, 0, 1, 1 }, { 1, 1, 1, 1, 0, 0, 0, 1, 1 }, { 1, 1, 1, 1, 0, 0, 0, 1, 1 }, { 1, 1, 1, 0, 0, 0, 0, 0, 1 }, { 1, 1, 0, 0, 0, 1, 0, 0, 1 } };
        test_walls.Add(wall_29);

        //Wide Stance V2 (shifted left)
        int[,] wall_30 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 0, 1, 1, 1, 1, 1 }, { 1, 1, 0, 0, 0, 1, 1, 1, 1 }, { 1, 1, 0, 0, 0, 1, 1, 1, 1 }, { 1, 1, 0, 0, 0, 1, 1, 1, 1 }, { 1, 0, 0, 0, 0, 0, 1, 1, 1 }, { 0, 0, 0, 1, 0, 0, 1, 1, 1 } };
        test_walls.Add(wall_30);

        //Bend Forward
        int[,] wall_31 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 1, 0, 0, 1, 1, 1 }, { 1, 1, 1, 1, 0, 0, 1, 1, 1 } };
        test_walls.Add(wall_31);

        //Kneel V2
        int[,] wall_32 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 0, 1, 1 } };
        test_walls.Add(wall_32);

        //Kneel V2 (shifted left)
        int[,] wall_33 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 0, 1, 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 0, 1, 1, 1, 1 } };
        test_walls.Add(wall_33);

        //Kneel V2 (mirror shifted right)
        int[,] wall_34 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 0, 1, 1 }, { 1, 1, 1, 1, 1, 0, 0, 0, 1 }, { 1, 1, 1, 1, 1, 0, 0, 0, 1 }, { 1, 1, 1, 1, 1, 0, 0, 0, 1 }, { 1, 1, 1, 1, 0, 0, 0, 0, 1 } };
        test_walls.Add(wall_34);
    }

    // Generate test walls that has reduced scope such that the agent do not have to worry about moving body, just needs to understand
    // how to move limbs
    private void generate_test_walls_reduced()
    {
        //default pose
        int[,] wall_0 = {   { 1,1,1,1,1,1,1,1,1,1},
                            { 1,1,1,1,0,1,1,1,1,1},
                            { 1,1,1,1,0,1,1,1,1,1},
                            { 1,1,1,0,0,0,1,1,1,1},
                            { 1,1,1,0,0,0,1,1,1,1},
                            { 1,1,1,0,0,0,1,1,1,1},
                            { 1,1,1,0,0,0,1,1,1,1},
                            { 1,1,1,0,0,0,1,1,1,1},
                            { 1,1,1,0,0,0,1,1,1,1} };

        test_walls.Add(wall_0);

        //Right leg lean forward
        int[,] wall_1 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 1, 1, 1, 1 }, { 1, 0, 0, 0, 0, 0, 0, 0, 1 }, { 1, 1, 0, 0, 0, 0, 0, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 } };
        test_walls.Add(wall_1);

        //Hand Behind Back, Rotated Sideways
        int[,] wall_2 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 1, 1, 1, 1 } };
        test_walls.Add(wall_2);

        //Right Leg Sightly Raised
        int[,] wall_3 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 0, 1, 1 }, { 1, 1, 1, 1, 0, 0, 0, 1, 1 }, { 1, 1, 1, 1, 0, 0, 0, 1, 1 }, { 1, 1, 1, 1, 0, 1, 1, 1, 1 } };
        test_walls.Add(wall_3);

        //Squat
        int[,] wall_4 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 } };
        test_walls.Add(wall_4);

        //Wide Stance
        int[,] wall_5 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 0, 0, 1, 0, 0, 1, 1 }, { 1, 1, 0, 0, 1, 0, 0, 1, 1 } };
        test_walls.Add(wall_5);

        //Kneel
        int[,] wall_6 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 } };
        test_walls.Add(wall_6);

        //slight lean
        int[,] wall_7 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 0, 1, 1 }, { 1, 1, 1, 0, 0, 0, 0, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 } };
        test_walls.Add(wall_7);

        //Leg Lift V2
        int[,] wall_8 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 0, 0, 0, 1, 1, 1, 1 }, { 1, 1, 0, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 1, 1, 1, 1 } };
        test_walls.Add(wall_8);

        //Wide Stance V2
        int[,] wall_9 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 0, 0, 0, 0, 0, 1, 1 }, { 1, 0, 0, 0, 1, 0, 0, 1, 1 } };
        test_walls.Add(wall_9);

        //Bend Forward
        int[,] wall_10 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 1, 0, 0, 1, 1, 1 }, { 1, 1, 1, 1, 0, 0, 1, 1, 1 } };
        test_walls.Add(wall_10);

        //Kneel V2
        int[,] wall_11 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 0, 1, 1 } };
        test_walls.Add(wall_11);

    }

    //not possible walls but walls that incentivize agents move their limbs to pass with as great of a reward as possible
    private void generate_limb_walls()
    {
        //A-Pose Single Right Arm
        int[,] wall_0 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 0, 1, 1, 1 }, { 1, 1, 1, 1, 1, 0, 1, 1, 1 }, { 1, 1, 1, 1, 1, 0, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_0);

        //A-Pose Single Right Arm -> Shifted Right
        int[,] wall_1 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 0, 1 }, { 1, 1, 1, 1, 1, 1, 1, 0, 1 }, { 1, 1, 1, 1, 1, 1, 1, 0, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_1);

        //A-Pose Single Left Arm
        int[,] wall_2 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 0, 1, 1, 1, 1, 1 }, { 1, 1, 1, 0, 1, 1, 1, 1, 1 }, { 1, 1, 1, 0, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_2);

        //A-Pose Single Left Arm -> Shifted Left
        int[,] wall_3 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 0, 1, 1, 1, 1, 1, 1, 1 }, { 1, 0, 1, 1, 1, 1, 1, 1, 1 }, { 1, 0, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_3);

        //A-Pose Single Right Arm Centered
        int[,] wall_4 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_4);

        //Arm Lift Right Side Below Shoulders
        int[,] wall_5 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 0, 1, 1, 1 }, { 1, 1, 1, 1, 1, 0, 0, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_5);

        //Arm Lift Right Side Below Shoulders -> Shifted Right
        int[,] wall_6 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 0, 1 }, { 1, 1, 1, 1, 1, 1, 1, 0, 0 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_6);

        //Arm Lift Left Side Below Shoulders
        int[,] wall_7 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 0, 1, 1, 1, 1, 1 }, { 1, 1, 0, 0, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_7);

        //Arm Lift Left Side Below Shoulders -> Shifted Left
        int[,] wall_8 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 0, 1, 1, 1, 1, 1, 1, 1 }, { 0, 0, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_8);

        //Arm Left Lift Below Shoulders Center
        int[,] wall_9 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_9);

        //Arm Right Lift Below Shoulders Center
        int[,] wall_10 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 0, 1, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_10);

        //T-Pose Single Arm
        int[,] wall_11 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 0, 0, 0, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_11);

        //T-Pose Single Arm -> Shifted Right
        int[,] wall_12 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 0, 0, 0 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_12);

        //T-Pose Single Arm -> Shifted Left
        int[,] wall_13 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 0, 0, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_13);

        //T-Pose Single Arm -> Shifted Left
        int[,] wall_14 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 0, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_14);

        //T-Pose Single Arm -> Shifted Left
        int[,] wall_15 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_15);

        //T-Pose Single Arm -> Shifted Left
        int[,] wall_16 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 0, 0, 0, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_16);

        //Arm Lift Above Shoulders
        int[,] wall_17 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 0, 0, 1, 1 }, { 1, 1, 1, 1, 1, 0, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_17);

        //Arm Lift Above Shoulders -> Shifted Right
        int[,] wall_18 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 0, 0 }, { 1, 1, 1, 1, 1, 1, 1, 0, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_18);

        //Arm Left Lift Above Shoulders 
        int[,] wall_19 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 0, 0, 1, 1, 1, 1, 1, 1 }, { 1, 1, 0, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_19);

        //Arm Left Lift Above Shoulders -> Shifted Left
        int[,] wall_20 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 0, 0, 1, 1, 1, 1, 1, 1, 1 }, { 1, 0, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_20);

        //Arm Left Lift Center
        int[,] wall_21 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_21);

        //Arm Right Lift Center
        int[,] wall_22 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_22);

        //Arm Straight Up or Head
        int[,] wall_23 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 0, 1, 1, 1 }, { 1, 1, 1, 1, 1, 0, 1, 1, 1 }, { 1, 1, 1, 1, 1, 0, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_23);

        //Arm Straight Up or Head -> Shifted Right
        int[,] wall_24 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 0 }, { 1, 1, 1, 1, 1, 1, 1, 1, 0 }, { 1, 1, 1, 1, 1, 1, 1, 1, 0 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_24);

        //Arm Straight Up or Head -> Shifted Left
        int[,] wall_25 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 0, 1, 1, 1, 1, 1, 1 }, { 1, 1, 0, 1, 1, 1, 1, 1, 1 }, { 1, 1, 0, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_25);

        //Arm Straight Up or Head-> Shifted Left
        int[,] wall_26 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 0, 1, 1, 1, 1, 1, 1, 1, 1 }, { 0, 1, 1, 1, 1, 1, 1, 1, 1 }, { 0, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_26);
        
        //Chest Default
        int[,] wall_27 = {{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,0,1,1,1,1},{1,1,1,0,0,0,1,1,1},{1,1,1,1,0,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1}};
        test_walls.Add(wall_27);
        
        //Chest Default Shifted Right
        int[,] wall_28 = {{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,0,1,1},{1,1,1,1,1,0,0,0,1},{1,1,1,1,1,1,0,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1}};
        test_walls.Add(wall_28);

        //Chest Default Shifted Left
        int[,] wall_29 = {{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,0,1,1,1,1,1,1},{1,0,0,0,1,1,1,1,1},{1,1,0,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1}};
        test_walls.Add(wall_29);     

        //Hips Default
        int[,] wall_30 = {{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,0,0,0,1,1,1},{1,1,1,0,0,0,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1}};
        test_walls.Add(wall_30);

        //Hips Default Shifted Right
        int[,] wall_31 = {{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,0,0,0,1},{1,1,1,1,1,0,0,0,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1}};
        test_walls.Add(wall_31);
                
        //Hips Default Shifted Left
        int[,] wall_32 = {{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,0,0,0,1,1,1,1,1},{1,0,0,0,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1}};
        test_walls.Add(wall_32);
        
        //Legs Standing
        int[,] wall_33 = {{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,0,0,1,1,1},{1,1,1,1,0,0,1,1,1},{1,1,1,1,0,0,1,1,1},{1,1,1,1,0,0,1,1,1}};
        test_walls.Add(wall_33);

        //Legs Standing Shifted Right
        int[,] wall_34 = {{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,0,0,1},{1,1,1,1,1,1,0,0,1},{1,1,1,1,1,1,0,0,1},{1,1,1,1,1,1,0,0,1}};
        test_walls.Add(wall_34);
                
        //Legs Standing Shifted Left
        int[,] wall_35 = {{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{0,0,1,1,1,1,1,1,1},{0,0,1,1,1,1,1,1,1},{0,0,1,1,1,1,1,1,1},{0,0,1,1,1,1,1,1,1}};
        test_walls.Add(wall_35);

        //Wide Stance V2 Legs
        int[,] wall_36 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 0, 0, 0, 0, 0, 1, 1 }, { 1, 0, 0, 0, 1, 0, 0, 1, 1 } };
        test_walls.Add(wall_36);

        //Wide Stance V2 Legs Shifted Right
        int[,] wall_37 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 0, 0, 0, 1 }, { 1, 1, 1, 1, 0, 0, 0, 0, 0 }, { 1, 1, 1, 0, 0, 0, 1, 0, 0 } };
        test_walls.Add(wall_37);

        //Wide Stance V2 Legs Shifted Left
        int[,] wall_38 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 0, 0, 0, 1, 1, 1, 1 }, { 1, 0, 0, 0, 0, 0, 1, 1, 1 }, { 0, 0, 0, 1, 0, 0, 1, 1, 1 } };
        test_walls.Add(wall_38);

        //Leg Lift V3
        int[,] wall_39 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 0, 0, 0, 0 }, { 1, 1, 1, 1, 1, 0, 0, 0, 1 }, { 1, 1, 1, 1, 1, 0, 1, 1, 1 }, { 1, 1, 1, 1, 1, 0, 1, 1, 1 } };
        test_walls.Add(wall_39);

        //Squat Legs
        int[,] wall_40 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 } };
        test_walls.Add(wall_40);

        //Squat Legs Shifted Right
        int[,] wall_41 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 0, 0, 0 }, { 1, 1, 1, 1, 1, 1,0, 0, 0 }, { 1, 1, 1, 1, 1, 1, 0, 0, 0 } };
        test_walls.Add(wall_41);

        //Squat Legs Shifted Left
        int[,] wall_42 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 0, 0, 0, 1, 1, 1, 1, 1, 1 }, { 0, 0, 0, 1, 1, 1, 1, 1, 1 }, { 0, 0, 0, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_42);

        //Squat Chest
        int[,] wall_43 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_43);

        //Squat Chest Shifted Left
        int[,] wall_44 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 0, 1, 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_44);

        //Squat Chest Shifted Right
        int[,] wall_45 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 0, 1 }, { 1, 1, 1, 1, 1, 1, 0, 0, 0 }, { 1, 1, 1, 1, 1, 1, 0, 0, 0 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_45);

        //Squat Hips
        int[,] wall_46 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_46);

        //Squat Hips Shifted Right
        int[,] wall_47 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 0, 0, 0 }, { 1, 1, 1, 1, 1, 1, 0, 0, 0 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_47);        
        
        //Squat Hips Shifted Left
        int[,] wall_48 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 0, 0, 0, 1, 1, 1, 1, 1, 1 }, { 0, 0, 0, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        test_walls.Add(wall_48);
        
        //Wide Stance
        int[,] wall_49 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 0, 0, 1, 0, 0, 1, 1 }, { 1, 1, 0, 0, 1, 0, 0, 1, 1 } };
        test_walls.Add(wall_49);

        //Wide Stance Shifted Right
        int[,] wall_50 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 0, 0, 0, 1 }, { 1, 1, 1, 1, 1, 0, 0, 0, 1 }, { 1, 1, 1, 1, 0, 0, 1, 0, 0 }, { 1, 1, 1, 1, 0, 0, 1, 0, 0 } };
        test_walls.Add(wall_50);
        
        //Wide Stance Shifted Left
        int[,] wall_51 = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 1, 1, 1, 1, 1 }, { 0, 0, 1, 0, 0, 1, 1, 1, 1 }, { 0, 0, 1, 0, 0, 1, 1, 1, 1 } };
        test_walls.Add(wall_51);


    }
}
