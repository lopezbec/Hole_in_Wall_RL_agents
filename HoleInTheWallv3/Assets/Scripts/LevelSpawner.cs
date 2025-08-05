using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static LevelData;

public class LevelSpawner : MonoBehaviour
{
    public static List<LevelSpawner> levelSpawners {  get; private set; } = new List<LevelSpawner>();
    private int currLevel = 0;
    public GameObject cubePrefab;
    public GameObject coinPrefab;
    public GameObject redCoinPrefab;
    public GameObject greenCoinPrefab;
    public GameObject StartPlanePrefab;
    public GameObject EndPlanePrefab;
    private List<LevelData> levels;
    private LevelContainer levelContainer;
    bool noMoreLevels = false;
    public LevelContainer LevelContainer { 
        private get
        {
            return levelContainer;
        }
        set
        {
            levelContainer = value;
            levels = value.levels;
            currLevel = 0;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        levelSpawners.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
    public void SpawnLevel(LevelData level)
    {
        GameObject newLevel = new GameObject(level.name);
        newLevel.transform.position = transform.position;
        newLevel.tag = "Level";
        float minZ = 0;
        float maxZ = 0;

        foreach(LevelData.Cube cube in level.cubes)
        {
            if (minZ > System.Math.Min(cube.z1, cube.z2)) minZ = System.Math.Min(cube.z1, cube.z2);
            if (maxZ < System.Math.Max(cube.z1, cube.z2)) maxZ = System.Math.Max(cube.z1, cube.z2);

            Vector3 positionOffset = new Vector3((cube.x1 + cube.x2) / 2, (cube.y1 + cube.y2) / 2, (cube.z1 + cube.z2) / 2);
            GameObject newCube = Instantiate(cubePrefab, newLevel.transform.position + positionOffset, Quaternion.Euler(cube.xRot , cube.yRot, cube.zRot), newLevel.transform);
            newCube.transform.localScale = new Vector3(cube.x2 - cube.x1, cube.y2 - cube.y1, cube.z2 - cube.z1);
        }
        foreach(LevelData.Coin coin in level.coins)
        {
            if(minZ > coin.z) minZ = coin.z;
            if(maxZ < coin.z) maxZ = coin.z;

            Vector3 positionOffset = new Vector3(coin.x, coin.y, coin.z);
            GameObject coinTypePrefab = null;
            switch (coin.type)
            {
                case 0:
                    coinTypePrefab = coinPrefab;
                    break;
                case 1:
                    coinTypePrefab = redCoinPrefab;
                    break;
                case 2:
                    coinTypePrefab = greenCoinPrefab;
                    break;
            }
            GameObject newCoin = Instantiate(coinTypePrefab, newLevel.transform.position + positionOffset, Quaternion.Euler(0, 0, 0), newLevel.transform);
        }

        Instantiate(StartPlanePrefab, newLevel.transform.position + new Vector3(0, 0, minZ-1), Quaternion.Euler(-90, 0, 0), newLevel.transform);
        Instantiate(EndPlanePrefab, newLevel.transform.position + new Vector3(0, 0, maxZ+1), Quaternion.Euler(-90, 0, 0), newLevel.transform);

    }

    public void SpawnNext()
    {
        if(currLevel < levels.Count) SpawnLevel(levels[currLevel++]);
        else noMoreLevels = true;
    }

    internal bool hasLevels()
    {
        return !noMoreLevels;
    }

    internal void Reset()
    {
        noMoreLevels = false;
        currLevel = 0;
    }
}


/// <summary>Custom Editor for our PrefabSwitch script, to allow us to perform actions
/// from the editor.</summary>
[CustomEditor(typeof(LevelSpawner))]
public class LevelSpawnerEditor : Editor
{
    /// <summary>Calls on drawing the GUI for the inspector.</summary>
    public override void OnInspectorGUI()
    {
        // Draw the default inspector.
        DrawDefaultInspector();

        // Grab a reference to the target script, so we can identify it as a 
        // PrefabSwitch, instead of a simple Object.
        LevelSpawner levelSpawner = (LevelSpawner)target;

        // Create a Button for "Swap By Tag",
        if (GUILayout.Button("Spawn Test Level"))
        {
            // if it is clicked, call the SwapAllByTag method from prefabSwitch.
            LevelData testLevel = new LevelData("Test Level");
            testLevel.cubes.Add(new LevelData.Cube(0, 0, 0, 1, 2, 1));
            testLevel.coins.Add(new LevelData.Coin(-1, 0, -1, 0));

            //LevelContainer levelContainer = LevelContainer.Load(Path.Combine(Application.dataPath, "levels.xml"));
            //LevelData testLevel = levelContainer.levels[0];
            //levelSpawner.SpawnLevel(testLevel);
            levelSpawner.SpawnLevel(testLevel);
        }

        if (GUILayout.Button("Save Test Level"))
        {
            // if it is clicked, call the SwapAllByTag method from prefabSwitch.
            LevelData testLevel = new LevelData("Test Level");
            LevelData testLevel2 = new LevelData();

            testLevel.cubes.Add(new LevelData.Cube(0, 0, 0, 1, 2, 1));
            testLevel.coins.Add(new LevelData.Coin(-1, 0, -1, 0));

            testLevel2.cubes.Add(new LevelData.Cube(0, 0, 0, 1, 2, 1));
            testLevel2.coins.Add(new LevelData.Coin(-1, 0, -1, 0));

            LevelContainer levelContainer = new LevelContainer();
            levelContainer.levels.Add(testLevel);
            levelContainer.levels.Add(testLevel2);

            levelContainer.Save(Path.Combine(Application.dataPath, "levels2.xml"));

            Debug.Log("saved to " + Path.Combine(Application.dataPath, "levels2.xml"));
        }

        if(GUILayout.Button("Save Test Profile"))
        {
            ProfileData profileData = new ProfileData();
            profileData.achievements.Add("test ach 1");
            profileData.achievements.Add("test ach 2");
            profileData.achievements.Add("test ach 3");
            profileData.highScore = 100;

            ProfileDataContainer profileContainer = new ProfileDataContainer();
            profileContainer.Profiles.Add(profileData);
            profileContainer.Save(Path.Combine(Application.dataPath, "Profiles/testProfiles.xml"));

            ProfileDataContainer newContainer = ProfileDataContainer.Load(Path.Combine(Application.dataPath, "Profiles/testProfiles.xml"));
            Debug.Log(newContainer.Profiles[0].achievements.Contains("test hash ach 1"));
        }
    }
}
