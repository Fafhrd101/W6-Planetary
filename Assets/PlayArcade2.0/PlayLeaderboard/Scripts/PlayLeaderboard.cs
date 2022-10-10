using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class PlayLeaderboard : MonoBehaviour
{
    public enum NavalRanks
    {
        Ensign,
        Lieutenant,
        LtCommander,
        Commander,
        Captain,
        RearAdmiral
    }
    
    // public string mainMenuName = "Start";
    public List<HighScoreUI> scoreUI = new List<HighScoreUI>();
    // public GameObject playersGroup;
    // public Text playerRank;
    // public Text playerName;
    // public Text playerScore;
    // public Image coinImage;
    
    // public void Awake()
    // {
    //     playersGroup.SetActive(false);
    //     coinImage.sprite = PlayArcadeIntegration.Instance.coinSprite;
    // }

    public void Update()
    {
        if (PlayArcadeIntegration.Instance.scoresRetrieved == false)
            return;

        //playersGroup.SetActive(true);
        int i = 0;
        foreach (HighScoreUI ui in scoreUI)
        {
            if (PlayArcadeIntegration.Instance.coinScores.Count > i)
            {
                ui.gameObject.SetActive(true);
                ui.number.text = (i + 1).ToString();
                ui.playerName.text = ToTitleCase(PlayArcadeIntegration.Instance.coinScores[i].user_name);
                ui.score.text = PlayArcadeIntegration.Instance.coinScores[i].score.ToString();
                i++;
            }
            else
            {
                ui.gameObject.SetActive(false);
            }
        }

        // if (PlayArcadeIntegration.Instance.playerRank > 10)
        // {
        //     playerRank.text = PlayArcadeIntegration.Instance.playerRank.ToString();
        //     playerName.text = PlayArcadeIntegration.Instance.playerName;
        //     playerScore.text = PlayArcadeIntegration.Instance.lastScore.ToString();
        // }
        // else
        // {
        //     playerName.text = "";
        //     playerRank.text = "";
        //     playerScore.text = "";
        // }
    }
    //
    // public void buttonMainMenu()
    // {
    //     SceneManager.LoadScene(mainMenuName);
    // }
    
    public string ToTitleCase(string str)
    {
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
    }
}

