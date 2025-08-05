using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System;
using System.Collections.Generic;
using System.Runtime;
using UnityEngine.Windows.Speech;
using TMPro;

public class GameController : MonoBehaviour
{
    private static bool gameOn = false;
    private static float lastLevelTimer = -1;
    private static DateTime startTime;
    private static GameObject[] levels;
    private static double speed = -2;
    private static int gameScore;
    private static int coinCount;
    private static int collisionCount;
    private static int currStreak;
    private static int maxStreak;
    private static TMP_Text scoreText;
    private static int gameScoreDeductions;
    private static string levelsPath = "LevelSets/levels2.xml";
    private static GameObject checkmark;
    private static float checkmarkTime;
    private static GameObject xmark;
    private static float xmarkTime;
    private static bool playerCollided;
    private static int profileID;
    private static EventLogger logger;
    private static float logTimer;
    private static ProfileDataContainer profileContainer;
    private static ProfileData currProfile;
    private static string profileXMLPath = "Profiles/testProfile.xml";
    public static bool isHighscore {get; private set; }  = false;

    public static Dictionary<string, int> achievementToPoints = new Dictionary<string, int>();
    private static int bonusPoints;
    private string achievementsPath = "Data/Achievements.txt";



    // Use this for initialization
    void Start()
    {
        Debug.Log("started");
        
        GameObject scoreTextObj = GameObject.FindWithTag("ScoreText");
        checkmark = GameObject.FindWithTag("CheckMark");
        xmark = GameObject.FindWithTag("XMark");

        scoreText = scoreTextObj.GetComponent<TMP_Text>();

        LoadLevels();
        LoadAchievements();
        Debug.Log(achievementToPoints["First Game"]);

        profileContainer = ProfileDataContainer.Load(Path.Combine(Application.dataPath, profileXMLPath));

        EnableProfile(0);


    }

   

    // Update is called once per frame
    void Update()
    {
        if (gameOn)
        {
            

            levels = GameObject.FindGameObjectsWithTag("Level");

            foreach (GameObject level in levels)
            {
                level.transform.position += new Vector3(0, 0, (float)speed * Time.deltaTime);
            }
            
            gameScore = (int)((Mathf.Abs(levels[0].transform.position.z - 10)*2) - gameScoreDeductions + bonusPoints);
            
            if(lastLevelTimer < 0)
            {
                if (NoMoreLevels()) lastLevelTimer = Time.time;
            } else if (Time.time - lastLevelTimer > 3)
            {
                GameOver();
            }

            if (Time.time - logTimer > 1)
            {
                AddLog("Score: " + gameScore);
                logger.WriteLog();
                logTimer = Time.time;
            }
        }
        if(scoreText != null) scoreText.text = "Score: " + gameScore.ToString();
        
        float time = Time.time;
        if(checkmarkTime > 0 && time - checkmarkTime > 3)
        {
            checkmark.GetComponent<Renderer>().enabled = false;
            checkmarkTime = -1;
        }
        if (xmarkTime > 0 && time - xmarkTime > 3)
        {
            xmark.GetComponent<Renderer>().enabled = false;
            xmarkTime = -1;
        }

        
    }

    private bool NoMoreLevels()
    {
        foreach (LevelSpawner spawner in LevelSpawner.levelSpawners)
        {
            if (spawner.hasLevels()) return false;
        }
        return true;
    }

    public void ChangeEnvironment(int input)
    {
        throw new NotImplementedException();
    }

   

    public static void EnableProfile(int ID)
    {
        Debug.Log(ID);
        profileID = ID;
        currProfile = profileContainer.Profiles[ID];
        if(logger != null) logger.Close();
        logger = new EventLogger(Path.Combine(Application.dataPath, "Logs/Profile" + ID + "Log.txt"));
    }



    public static void StartGame()
    {
        gameOn = true;
        startTime = DateTime.Now;
        GiveAchievement("First Game");
        GameObject.FindWithTag("StartMenu").SetActive(false);
        SpawnNext();
    }

    public static void GameOver()
    {
        gameOn = false;

        CheckHighscores();

        DeleteLevels();

        ShowEndMenu();

        logger.Close();

        SaveProfiles();
        // writer.WriteLine("\n\n");
        //scorePanel.SetActive(false);

        //winLose.text = "Great Game!";
        //summary.text = string.Format("Score:{0}\nCoins:{1}", gameScore, GameController.coinCount);


    }

    private static void ShowEndMenu()
    {
        Debug.Log(GameObject.FindObjectsOfType<EndMenu>(true).Length);
        EndMenu endMenu = GameObject.FindObjectsOfType<EndMenu>(true)[0];
        endMenu.gameObject.SetActive(true);
        endMenu.UpdateDisplay(isHighscore, gameScore, collisionCount, gameScoreDeductions);
    }

    private static void DeleteLevels()
    {
        GameObject[] levels = GameObject.FindGameObjectsWithTag("Level");
        foreach (GameObject level in levels)
        {
            Destroy(level);
        }
    }

    private static void CheckHighscores()
    {
        if (gameScore >= 1000) GiveAchievement("Score 1000");
        if (gameScore > currProfile.highScore)
        {
            currProfile.highScore = gameScore;
            isHighscore = true;
        }
        else isHighscore = false;
        if(maxStreak > currProfile.topStreak) currProfile.topStreak = maxStreak;
    }

    public static void LoadLevels()
    {
        foreach(LevelSpawner spawner in LevelSpawner.levelSpawners)
        {
            spawner.LevelContainer = LevelContainer.Load(Path.Combine(Application.dataPath, levelsPath));
        }

    }

    private void LoadAchievements()
    {
        FileStream f = new FileStream(Path.Combine(Application.dataPath, achievementsPath), FileMode.Open);
        StreamReader reader = new StreamReader(f);

        while (true)
        {
            string line = reader.ReadLine();
            if (line == null) break;
            string[] args = line.Split(',');
            if(args.Length > 1) achievementToPoints[args[0]] = Int32.Parse(args[1]);
        }

    }

    public static void LevelEnded(LevelSpawner spawner)
    {
        SpawnNext();
        if (!playerCollided)
        {
            DisplayCheck();
            currStreak++;
            GiveAchievement("Perfect Pass");
            if (currStreak == 5) GiveAchievement("5 Streak");
            if(currStreak > maxStreak) maxStreak = currStreak;
        } else
        {
            collisionCount++;
        }
        playerCollided = false;
    }

    private static void SpawnNext()
    {
        System.Random rnd = new System.Random();
        LevelSpawner.levelSpawners[rnd.Next(0, LevelSpawner.levelSpawners.Count)].SpawnNext();
    }

    public void AddRandomNames()
    {
        string names = "Pikachu Prof.Lopez AllyB Mario T-Pose Luigi Kermit Kirby D.Va Tom-Hanks Voldemort Harry-Potter Unity NoobMaster69";
        string[] individual = names.Split(' ');
        //foreach (var word in individual)
        //{
        //    randomNames.Add(word);
        //}
    }




    public void addPeople()
    {

    }

    //class of ScoreEntry that stores the players names and scores
    class ScoreEntry
    {
        private string name;
        private int score;

        public ScoreEntry(string name, string score)
        {
            this.name = name;
            this.score = int.Parse(score);
        }

        public int getScore()
        {
            return score;
        }

        public string getName()
        {
            return name;
        }
    }






    //Achievement Checking
    public void addStreak()
    {
        currStreak++;
    }

 

    public static int getStreak()
    {
        return currStreak;
    }

    public void enableAchievementBoard()
    {
        //int imageNumber = 0;
        //achievementBoard.SetActive(true);
        //avatarGoodMenu.SetActive(false);
        //foreach (Achievement achievement in gc.achievementList)
        //{
        //    if (achievement.getCompleted() == true)
        //    {
        //        gc.achievementColor = new Color(0.3254717f, 1f, 0.3261002f, 0.7764706f);
        //        //gc.hoverColor = Color.green;
        //    }
        //    else
        //    {
        //        gc.achievementColor = new Color(0.9568628f, 0.4352942f, 0.4392157f, 1f);
        //        //gc.hoverColor = Color.red;
        //    }
        //    gc.imageList[imageNumber].color = gc.achievementColor;
        //    imageNumber++;
        //}
    }

    public static void GiveAchievement(string name)
    {
        if (!currProfile.achievements.Contains(name)) 
        {
            currProfile.achievements.Add(name);
            AwardScore(achievementToPoints[name]);
        }
    }


    //Checks whether an achievement has been triggered and tells the Grader
    public void checkAchievement()
    {
        //if (achieve)
        //{
        //    if (gc.streak == 5 && achievementList[0].getCompleted() == false)
        //    {
        //        //5 In A Row! Achievement Active
        //        achievementList[0].setCompleted(true);
        //        achievementName.text = achievementList[0].getName();
        //        achievementPopup.SetActive(true);
        //        Invoke("DisableAchievements", 4f);
        //    }

        //    if (gc.streak == 10 && achievementList[1].getCompleted() == false)
        //    {
        //        //10 In A Row Achievment Active
        //        achievementList[1].setCompleted(true);
        //        achievementName.text = achievementList[1].getName();
        //        achievementPopup.SetActive(true);
        //        Invoke("DisableAchievements", 4f);
        //    }

        //    if (coinCount == 20 && achievementList[2].getCompleted() == false)
        //    {
        //        //Coins! Active
        //        achievementList[2].setCompleted(true);
        //        achievementName.text = achievementList[2].getName();
        //        achievementPopup.SetActive(true);
        //        Invoke("DisableAchievements", 4f);
        //    }

        //    if (collisions == 1 && achievementList[3].getCompleted() == false)
        //    {
        //        //Almost! Achievement Active
        //        achievementList[3].setCompleted(true);
        //        achievementName.text = achievementList[3].getName();
        //        achievementPopup.SetActive(true);
        //        Invoke("DisableAchievements", 4f);
        //    }
        //}

    }


    void DisableAchievements()
    {
        //achievementPopup.SetActive(false);
    }

    

    public class Achievement
    {
        private string name;
        private string achievementText;
        private bool completed;

        public Achievement(string name, string achievementText, bool completed)
        {
            this.name = name;
            this.achievementText = achievementText;
            this.completed = completed;
        }

        public string getText()
        {
            return achievementText;
        }

        public string getName()
        {
            return name;
        }

        public bool getCompleted()
        {
            return completed;
        }

        public void setCompleted(bool change)
        {
            completed = change;
        }
    }


    internal static int gameModeGetter()
    {
        return 0;
    }

    internal static void IncreaseSpeed()
    {
        speed *= 1.2;
    }

    internal static void DecreaseSpeed()
    {
        speed *= 1/1.2;
    }

    internal static void AddCoin()
    {
        coinCount++;
    }

    internal static void DeductScore(int deduction)
    {
        gameScoreDeductions += deduction;
    }

    internal static void AwardScore(int addition)
    {
        bonusPoints += addition;
    }

    internal static void PlayerCollided()
    {
        DeductScore(10);
        DisplayX();
        currStreak = 0;
        playerCollided = true;
    }

    private static void DisplayX()
    {
        xmark.GetComponent<Renderer>().enabled = true;
        xmarkTime = Time.time;
    }

    private static void DisplayCheck()
    {
        checkmark.GetComponent<Renderer>().enabled = true;
        checkmarkTime = Time.time;
    }

    internal static void ResetProfile()
    {
        currProfile.Reset();
        SaveProfiles();
    }

    private static void SaveProfiles()
    {
        profileContainer.Save(Path.Combine(Application.dataPath, profileXMLPath));
    }

    internal static void ResetGame()
    {
        speed = -2;
        LoadLevels();
        gameScore = 0;
        coinCount = 0;
        collisionCount = 0;
        currStreak = 0;
        maxStreak = 0;
        gameScoreDeductions = 0;
        playerCollided = false;
        lastLevelTimer = -1;
        foreach (LevelSpawner spawner in LevelSpawner.levelSpawners)
        {
            spawner.Reset();
        }

    }
    internal static void AddLog(string message)
    {
        logger.AddLog(message);
    }

    internal static bool getGameOn()
    {
        return gameOn;
    }
}
