using UnityEngine;
using UnityEngine.UI;
using static Global;

public class GamePlayUI : MonoBehaviour
{
    private Text titleText, moveText;
    public void Init()
    {
        GS = GameState.GamePlay;
        gameObject.SetActive(true);
        moveText = transform.Find("moveText").GetComponent<Text>();
        titleText = transform.Find("titleText").GetComponent<Text>();
        titleText.SetText(string.Format(S.TheLevel, 
            $"{(char)('@'+G.chapter)}{G.level}"));
    }
    public void SetMove(int move)
    {
        moveText.text = string.Format(S.TheMove, move);
    }
    public void GameOver()
    {
        var nextButton = transform.Find("next").gameObject;
        nextButton.SetActive(true);
        var img = nextButton.GetComponent<Image>();
        var imgSubText = nextButton.GetComponentInChildren<Text>();
        var color = img.color;
        var timer = nextButton.AddComponent<Timer>();
        timer.period = 3;
        timer.periodical = true;
        timer.timeMap = Timer.Wave(0.5f, 1);
        timer.onUpdate = x =>
        {
            Color newColor = new(color.r, color.g, color.b, x);
            img.color = newColor;
            imgSubText.color = newColor;
        };
    }
}