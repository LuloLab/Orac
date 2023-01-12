using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Global;

public class Main : MonoBehaviour
{
    public RectTransform canvas;
    public TitleUI titleUI;
    public SettingsUI settingsUI;
    public LevelMapUI levelMapUI;
    public GamePlayUI gamePlayUI;


    private void Awake()
    {
        VersionUpdate();
        M = this;

        #region Set Default Language
        if (!PlayerPrefs.HasKey("language"))
        {
            if (Application.systemLanguage == SystemLanguage.Chinese ||
                Application.systemLanguage == SystemLanguage.ChineseSimplified ||
                Application.systemLanguage == SystemLanguage.ChineseTraditional)
            {
                PlayerPrefs.SetInt("language", (int)GameLanguage.Chinese);
                PlayerPrefs.Save();
            }
            else if (Application.systemLanguage == SystemLanguage.Spanish)
            {
                PlayerPrefs.SetInt("language", (int)GameLanguage.Espanol);
                PlayerPrefs.Save();
            }
        }
        LG = (GameLanguage)PlayerPrefs.GetInt("language");
        #endregion

        S = LG switch
        {
            GameLanguage.Chinese => new ChineseStringSource(),
            GameLanguage.Espanol => new EspanolStringSource(),
            _ => new StringSource(),
        };
    }

    private void Start()
    {
        GS = GameState.Title;

        titleUI.Init();
    }

    #region Keys
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
            ActionEsc();
        else if (Input.GetKeyUp(KeyCode.Z))
            ActionZ();
        else if (Input.GetKeyUp(KeyCode.R))
            ActionR();
        else if (Input.GetKeyUp(KeyCode.N))
            ActionN();
#if UNITY_EDITOR
        else if (Input.GetKeyUp(KeyCode.P))
            ActionP();
#endif
    }
    public void ActionEsc()
    {
        switch (GS)
        {
            case GameState.Title: QuitGame(); break;
            case GameState.Settings: Settings2Title(); break;
            case GameState.LevelMap: LevelMap2Title(); break;
            case GameState.GamePlay: GamePlay2LevelMap(); break;
            default: break;
        }
    }
    public void ActionZ()
    {
        if (GS == GameState.GamePlay)
        {
            A.ClickButton();
            G.Undo();
        }
    }
    public void ActionR()
    {
        if (GS == GameState.GamePlay)
        {
            A.ClickButton();
            G.Restart();
        }
    }
    public void ActionN()
    {
        if (GS == GameState.GamePlay)
        {
#if !UNITY_EDITOR
            if (!G.isNextButtonActive)
                return;
#endif
            A.ClickButton();
            G.QuitGame();
            if(G.level == NLevel[G.chapter - 1])
            {
                G.chapter = G.chapter % NLevel.Length + 1;
                G.level = 1;
            }
            else
            {
                ++G.level;
            }
            G.NewGame();
        }
    }
    public void ActionP()
    {
        if (GS == GameState.GamePlay)
        {
            A.ClickButton();
            G.QuitGame();
            if (G.level == 1)
            {
                G.chapter = (G.chapter - 2 + NLevel.Length) % NLevel.Length + 1;
                G.level = NLevel[G.chapter - 1];
            }
            else
            {
                --G.level;
            }
            G.NewGame();
        }
    }
#endregion


#region States
    public void Title2Settings()
    {
        A.ClickButton();
        titleUI.gameObject.SetActive(false);
        settingsUI.Init();
    }
    public void Settings2Title()
    {
        A.ClickButton();
        settingsUI.gameObject.SetActive(false);
        titleUI.Init();
    }
    public void Title2LevelMap()
    {
        A.ClickButton();
        titleUI.gameObject.SetActive(false);
        levelMapUI.Init();
    }
    public void LevelMap2Title()
    {
        A.ClickButton();
        levelMapUI.gameObject.SetActive(false);
        titleUI.Init();
    }
    public void LevelMap2GamePlay(int chapter, int level)
    {
        A.ClickButton();
        levelMapUI.gameObject.SetActive(false);
        gamePlayUI.Init();

        G.chapter = chapter;
        G.level = level;
        G.NewGame();
    }
    public void GamePlay2LevelMap()
    {
        A.ClickButton();
        G.QuitGame();
        gamePlayUI.gameObject.SetActive(false);
        levelMapUI.Init();
        levelMapUI.GetComponent<SlideUI>().GoTo(G.chapter - 1);
    }
#endregion

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    private void VersionUpdate()
    {
        int version = PlayerPrefs.GetInt("version");
        if (version == Version)
            return;

        // TODO: Operartion list

        PlayerPrefs.SetInt("version", version);
        PlayerPrefs.Save();
    }
}
