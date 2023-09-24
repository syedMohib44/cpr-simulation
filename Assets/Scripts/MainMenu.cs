using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject MainMenuObj;
    [SerializeField]
    private GameObject TrainObj;
    [SerializeField]
    private GameObject ScoreObj;
    [SerializeField]
    private TMP_InputField textField;

    // [SerializeField]
    // private TextMeshProUGUI scoreText;
    // [SerializeField]
    // private TextMeshProUGUI highScore;
    // [SerializeField]
    // private TextMeshProUGUI name;

    [SerializeField]
    public GameObject scoreRow;
    [SerializeField]
    public GameObject contentObj;

    private GameObject CurrentActiveObj;

    private void Awake()
    {
        textField.textComponent.SetText("Guest");
    }
    public void OnClickTest()
    {
        PlayerPrefs.SetString("Name", textField.text);
        SceneManager.LoadScene(1);
    }

    public void OnClickTrain()
    {
        Debug.Log(" ==== ");
        MainMenuObj.SetActive(false);
        TrainObj.SetActive(true);
        CurrentActiveObj = TrainObj;
    }

    public void OnClickScore()
    {
        MainMenuObj.SetActive(false);
        ScoreObj.SetActive(true);
        CurrentActiveObj = ScoreObj;
        DataManager dManager = new DataManager();
        dManager.LoadScores();
        if (dManager.scores.Count > contentObj.transform.childCount)
        {
            for(int i = contentObj.transform.childCount; i < dManager.scores.Count; i++)
            {
                GameObject score = GameObject.Instantiate(scoreRow, contentObj.transform);
                TextMeshProUGUI highScoreText = score.transform.Find("HighScore").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI currentScoreText = score.transform.Find("CurrentScore").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI nameText = score.transform.Find("Name").GetComponent<TextMeshProUGUI>();

                highScoreText.SetText(dManager.scores.ElementAt(i).Value.HighScore.ToString("0.00"));
                currentScoreText.SetText(dManager.scores.ElementAt(i).Value.Score.ToString("0.00"));
                nameText.SetText(dManager.scores.ElementAt(i).Value.Name.ToString());
            }
        }
    }
    public void OnClickBack()
    {
        CurrentActiveObj.SetActive(false);
        MainMenuObj.SetActive(true);
    }

}
