using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;

public class LevelData
{
    public static int ID = 0;
    public String name;
    public List<Cube> cubes;
    public List<Coin> coins;

    public LevelData(String name)
    {
        this.name = name;
        cubes = new List<Cube>();
        coins = new List<Coin>();
    }

    public LevelData()
    {
        name = "Unnamed " + ID++.ToString();
        cubes = new List<Cube>();
        coins = new List<Coin>();
    }


    public class Cube
    {
        public float x1;
        public float y1;
        public float z1;
        public float x2;
        public float y2;
        public float z2;

        public float xRot;
        public float yRot;
        public float zRot;

        public Cube(float x1, float y1, float z1, float x2, float y2, float z2, float xRot, float yRot, float zRot)
        {
            this.x1 = x1;
            this.y1 = y1;
            this.z1 = z1;
            this.x2 = x2;
            this.y2 = y2;
            this.z2 = z2;
            this.xRot = xRot;
            this.yRot = yRot;
            this.zRot = zRot;
        }

        public Cube(float x1, float y1, float z1, float x2, float y2, float z2) : this(x1, y1, z1, x2, y2, z2, 0, 0, 0)
        {

        }

        public Cube()
        {

        }
    }

    public class Coin
    {
        public float x;
        public float y;
        public float z;
        public int type;

        public Coin(float x, float y, float z, int type)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.type = type;
        }

        public Coin()
        {

        }
    }

    public void addCoin(float x, float y, float z, int type)
    {
        coins.Add(new Coin(x, y, z, type));
    }

    public void addCube(float x1, float y1, float z1, float x2, float y2, float z2, float xRot, float yRot, float zRot)
    {
        cubes.Add(new Cube(x1, y1, z1, x2, y2, z2, xRot, yRot, zRot));
    }

    public void addCube(float x1, float y1, float z1, float x2, float y2, float z2)
    {
        cubes.Add(new Cube(x1, y1, z1, x2, y2, z2));
    }
}
