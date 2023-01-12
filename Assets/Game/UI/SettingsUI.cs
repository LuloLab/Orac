using UnityEngine;
using UnityEngine.UI;
using static Global;

public class SettingsUI : MonoBehaviour
{
    public void Init()
    {
        GS = GameState.Settings;
        gameObject.SetActive(true);
        transform.Find("Title").GetComponent<Text>().SetText(
            S.Settings);
        transform.Find("languageButton").GetChild(0).GetComponent<Text>().SetText(
            S.Language);
        transform.Find("backButton").GetChild(0).GetComponent<Text>().SetText(
            S.Back);
        transform.Find("soundButton").GetChild(0).GetComponent<Text>().SetText(
            A.soundOn ? S.SoundOn : S.SoundOff);
    }
    public void ToggleSound()
    {
        A.ToggleSound();
        A.ClickButton();
        Init();
    }
    public void ToggleLanguage()
    {
        A.ClickButton();
        LG = (GameLanguage)(((int)LG + 1) % 3);
        S = LG switch
        {
            GameLanguage.English => new StringSource(),
            GameLanguage.Chinese => new ChineseStringSource(),
            _ => new EspanolStringSource()
        };
        PlayerPrefs.SetInt("language", (int)LG);
        PlayerPrefs.Save();
        Init();
    }
}