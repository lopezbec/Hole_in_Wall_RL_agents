using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileData
{
    public HashSet<string> achievements = new HashSet<string>();
    public int highScore;
    public int topStreak;

    public ProfileData() { }

    public void Reset()
    {
        highScore = 0;
        achievements.Clear();
    }
}
