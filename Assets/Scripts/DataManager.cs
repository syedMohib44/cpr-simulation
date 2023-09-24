using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataManager
{
    // Define a dictionary to store score data (playerName as the key, score as the value)
    private Dictionary<string, PlayerScore> ScoresDict = new Dictionary<string, PlayerScore>();
    public Dictionary<string, PlayerScore> scores { get { return ScoresDict; } }

    public DataManager()
    {
        // Load scores from a JSON file
        LoadScores();

        // // Print the scores to the console
        // foreach (var kvp in ScoresDict)
        // {
        //     Debug.Log("Player: " + kvp.Key + " | Score: " + kvp.Value);
        // }
    }

    public void LoadScores()
    {
        // Specify the file path to load JSON data from
        string filePath = Application.dataPath + "/scores.json";

        // Check if the file exists before trying to read it
        if (File.Exists(filePath))
        {
            // Read the JSON data from the file
            string json = File.ReadAllText(filePath);

            // Deserialize the JSON data into an array of key-value pairs
            // KeyValuePair<string, PlayerScore>[] keyValueArray = Newtonsoft.Json.JsonConvert.DeserializeObject<KeyValuePair<string, PlayerScore>[]>(json);

            // // Populate the dictionary from the array
            // // ScoresDict = new Dictionary<string, int>();
            // if (keyValueArray != null)
            // {
            //     foreach (var kvp in keyValueArray)
            //     {
            //         if (!string.IsNullOrWhiteSpace(kvp.Key))
            //             ScoresDict[kvp.Key] = kvp.Value;
            //     }
            // }
            ScoresDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, PlayerScore>>(json);
            if (ScoresDict == null)
                ScoresDict = new Dictionary<string, PlayerScore>();

        }
        else
        {
            Debug.LogWarning("File does not exist: " + filePath);
        }
    }

    public void SaveScores(PlayerScore score)
    {
        if (!ScoresDict.ContainsKey(score.Name))
        {
            ScoresDict.Add(score.Name, score);
        }
        else
        {
            ScoresDict[score.Name].HighScore += score.Score;
            ScoresDict[score.Name].Score = score.Score;
        }

        Debug.Log(score.Name + " ==== " + score);
        // Convert the dictionary to an array of key-value pairs
        // KeyValuePair<string, PlayerScore>[] keyValueArray = new KeyValuePair<string, PlayerScore>[ScoresDict.Count];
        // int i = 0;
        // foreach (var kvp in ScoresDict)
        // {
        //     keyValueArray[i] = new KeyValuePair<string, PlayerScore>(kvp.Key, kvp.Value);
        //     i++;
        // }

        // // Serialize the array to JSON
        // string json = Newtonsoft.Json.JsonConvert.SerializeObject(keyValueArray);
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(ScoresDict);

        // Specify the file path to save JSON data to
        string filePath = Application.dataPath + "/scores.json";

        // Write the JSON data to the file
        File.WriteAllText(filePath, json);

        Debug.Log("Scores saved to " + filePath);
    }
}