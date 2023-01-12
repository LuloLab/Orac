using System;
using UnityEngine;
using UnityEngine.UI;

public static class Global
{
    public static readonly int[] NLevel = { 20, 9, 16, 10 };
    public const int Version = 100;

    public static Main M;
    public static Game G;
    public static AudioController A;
    public static StringSource S;
    public static GlobalResource GR;
    public static GameState GS;
    public static GameLanguage LG;
    public static bool GLOBAL_INTERACTIVE = true;

    public enum GameLanguage { English, Chinese, Espanol };

    public enum GameState
    {
        Title, Settings, LevelMap, GamePlay
    }


    public static void SetText(this Text text, string s)
    {
        text.text = s;
    }
}