using UnityEngine;
using UnityEngine.UI;
using static Global;

public class TitleUI : MonoBehaviour
{
    public void Init()
    {
        GS = GameState.Title;
        gameObject.SetActive(true);
        
        transform.Find("playButton").GetChild(0).GetComponent<Text>().SetText(S.Play);
        transform.Find("settingsButton").GetChild(0).GetComponent<Text>().SetText(S.Settings);
        transform.Find("quitButton").GetChild(0).GetComponent<Text>().SetText(S.Quit);
    }
}