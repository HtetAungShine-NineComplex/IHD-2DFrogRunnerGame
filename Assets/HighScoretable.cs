using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RankingTable : MonoBehaviour
{
    [SerializeField] Transform entryContainer;
    [SerializeField] Transform entryTemplate;
    [SerializeField] Sprite[] profileSprites;
    private List<HighscoreEntry> highscoreEntryList;
    private List<Transform> highscoreEntryTransformList;

    private void Awake()
    {
        entryTemplate.gameObject.SetActive(false);

        //AddHighscoreEntry(100000000, "Monkey", 0);

        string jsonString = PlayerPrefs.GetString("highScoreTable");
        Highscores highscores = JsonUtility.FromJson<Highscores>(jsonString);

        if (highscores == null || highscores.highscoreEntryList == null)
        {
            highscores = new Highscores { highscoreEntryList = new List<HighscoreEntry>() };
        }
        //highscoreEntryList = highscores.highscoreEntryList;

        for (int i = 0; i < highscores.highscoreEntryList.Count; i++)
        {
            for (int j = i + 1; j < highscores.highscoreEntryList.Count; j++)
            {
                if (highscores.highscoreEntryList[j].score > highscores.highscoreEntryList[i].score)
                {
                    HighscoreEntry tmp = highscores.highscoreEntryList[i];
                    highscores.highscoreEntryList[i] = highscores.highscoreEntryList[j];
                    highscores.highscoreEntryList[j] = tmp;
                }
            }
        }

        highscoreEntryTransformList = new List<Transform>();
        foreach (HighscoreEntry highscoreEntry in highscores.highscoreEntryList)
        {
            CreateHighscoreEntryTransform(highscoreEntry, entryContainer, highscoreEntryTransformList);
        }

    }

    public static void AddHighscoreEntry(int score, string name, int profileImageIndex)
    {
        HighscoreEntry highscoreEntry = new HighscoreEntry { score = score, name = name, profileImageIndex = profileImageIndex };

        string jsonString = PlayerPrefs.GetString("highScoreTable");
        Highscores highscores = JsonUtility.FromJson<Highscores>(jsonString);

        if (highscores == null || highscores.highscoreEntryList == null)
        {
            highscores = new Highscores { highscoreEntryList = new List<HighscoreEntry>() };
        }
        highscores.highscoreEntryList.Add(highscoreEntry);

        string json = JsonUtility.ToJson(highscores);
        PlayerPrefs.SetString("highScoreTable", json);
        PlayerPrefs.Save();
    }

    private void CreateHighscoreEntryTransform(HighscoreEntry highscoreEntry, Transform container, List<Transform> transformList)
    {
        Transform entrytransform = Instantiate(entryTemplate, container);
        entrytransform.gameObject.SetActive(true);

        int rank = transformList.Count + 1;
        string rankString;
        switch (rank)
        {
            default:
                rankString = rank + "位";
                break;

            case 1:
                rankString = "1位";
                break;

            case 2:
                rankString = "2位";
                break;

            case 3:
                rankString = "3位";
                break;
        }

        int score = highscoreEntry.score;
        string name = highscoreEntry.name;
        int playerProfileIndex = highscoreEntry.profileImageIndex;

        entrytransform.Find("Text_Score_Value").GetComponent<TextMeshProUGUI>().text = score.ToString();
        entrytransform.Find("Text_UserName").GetComponent<TextMeshProUGUI>().text = name;
        entrytransform.Find("Text_Ranking").GetComponent<TextMeshProUGUI>().text = rankString;
        entrytransform.Find("CharacterFrame").GetChild(0).GetComponent<Image>().sprite = profileSprites[playerProfileIndex];
        transformList.Add(entrytransform);
    }

    private class Highscores
    {
        public List<HighscoreEntry> highscoreEntryList;
    }

    [System.Serializable]
    private class HighscoreEntry
    {
        public int score;
        public string name;
        public int profileImageIndex;
    }
}