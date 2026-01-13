#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Analytics;

public class StopPlaytime : MonoBehaviour
{
    public int mlscript_amt = 32;
    public float previous_reward = -9999;
    public float current_reward = 0;
    public int agent_amt = 0;
    public float max_seen = -9999;
    public int flag = 0;

    void Start()
    {
        if (mlscript_amt == 0) Debug.Log("Initialize the amount of ML agents on the scene");
    }

    public void CheckFailure()
    {

        float avg_reward = current_reward / agent_amt;

        //if first run, don't check
        if (previous_reward == -9999)
        {
            previous_reward = avg_reward;
            max_seen = previous_reward;
            agent_amt = 0;
            return;
        }

        //collapse patterns: previous reward is positive, now becomes negative with a huge jump. OR previous reward is now very similar to current reward multiple times while negative, indicating the agent is now stuck in the same action
        if (HasCollapsed())
        {
            flag++;
        }
        else flag = 0;


        //trigger the alarm if the pattern persists
        if (flag >= 3)
        {
// #if UNITY_EDITOR
//             Debug.LogError("Stopping Play Mode due to collapsing reward");
//             EditorApplication.isPlaying = false;
// #endif
        }

        agent_amt = 0;
        previous_reward = avg_reward;
        current_reward = 0;
        if (previous_reward > max_seen) max_seen = previous_reward;
    }

    public bool HasCollapsed()
    {
        float difference = previous_reward - current_reward;

        float difference_tolerance = -0.5f;
        float drop_tolerance = 2f;
        float similar_tolerance = 0.001f;

        //collapse patterns: previous reward is positive, now becomes negative with a huge jump. 
        if ((previous_reward >= 0 && current_reward <= 0 && difference >= difference_tolerance)) return true;
        //OR previous reward is now very similar to current reward multiple times while negative, indicating the agent is now stuck in the same action
        if ((current_reward <= 0 && previous_reward <= 0 && Math.Abs(previous_reward - current_reward) <= similar_tolerance)) return true;

        //if there is a sharp drop when it was positive, instant cancel run
        if (max_seen - current_reward >= drop_tolerance && max_seen >= 0)
        {
            flag = 3;
            Debug.Log("Collapsed due to positive to negative large drop");
            return true;
        }

        return false;
    }
    public void AddIntoAVG(float added_reward)
    {
        current_reward += added_reward;
        agent_amt++;

        if (agent_amt == mlscript_amt) CheckFailure();
    }
}