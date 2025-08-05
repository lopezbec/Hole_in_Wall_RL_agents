using UnityEngine;
using System.Collections;

public class WallCreator : MonoBehaviour {
    
	// Use this for initialization
	void Start () {
        GameObject wall = new GameObject();
        for (int i = 0; i < 25; i++) {
            for (int j = 0; j < 25; j++) {
                GameObject cube = Instantiate(Resources.Load("WallCube")) as GameObject;
                cube.transform.parent = wall.transform;
                cube.transform.localPosition = new Vector3(i * 0.1f, j * 0.1f, 3);
            }
        }
        wall.transform.lossyScale.Set(0.1f, 0.1f, 0.75f);
        
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
