using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
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

    //second way to generate the wall
    public int custom_cube_amt = 5;
    private float move_spd = .1f;
    public AvatarController avatar_script;

    // Start is called before the first frame update
    void Start()
    {
        int[,] test_wall =  {   {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                                {1, 1, 1, 1, 0, 1, 1, 1, 1, 1},
                                {1, 1, 0, 1, 1, 1, 1, 1, 1, 1},
                                {0, 0, 0, 1, 1, 1, 1, 1, 1, 1},
                                {0, 1, 0, 1, 1, 1, 1, 1, 1, 1},
                                {0, 1, 0, 1, 1, 1, 1, 1, 1, 1},
                                {0, 0, 0, 1, 1, 1, 1, 1, 1, 1},
                                {1, 0, 0, 1, 1, 1, 1, 1, 1, 1},
                                {0, 1, 0, 1, 1, 1, 1, 1, 1, 1}
                            };

        Build_wall(test_wall);

        // float[,] block_param = {    {.25f, 0, 0, .25f, .5f},
        //                             {0, .5f, 0, .25f, .5f},
        //                             {0, 0, 0, .25f, .5f},
        //                             {.5f, .5f, 0, .25f, .5f},
        //                             {.5f, .25f, 0, .25f, 1f}
        //                         };

        // custom_build(block_param);

    }

    // Update is called once per frame
    void Update()
    {
        Move_wall();
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
                    rect_obj.transform.SetParent(transform);

                    // apply local offset relative to this transform
                    Vector3 localOffset = new(x_offset, y_offset + ground_offset, 0f);
                    rect_obj.transform.position = transform.TransformPoint(localOffset);

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
        if(avatar_script.completed_pose) transform.position = new(transform.position.x, transform.position.y, transform.position.z - move_spd);
    }
}
