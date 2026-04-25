using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class DemoJSON 
{
    [JsonProperty("wall_id")]
    public int Wall_id { get;set;}

    [JsonProperty("matrix")]
    public int[][] Wall_matrix { get; set;}

    [JsonProperty("solutions")]
    public float[][] Solutions {get;set;}
}
