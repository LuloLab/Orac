using UnityEngine;
using UnityEngine.UI;
using static Global;

public class LevelMapUI : MonoBehaviour
{
    public Color openColor, winColor;
    public GameObject pfEntry;
    private Transform scroll;

    // Use this for initialization
    void BeforeInit()
    {
        scroll = transform.Find("scroll");

        for (int chapterIdx = 0; chapterIdx < NLevel.Length; chapterIdx++)
        {
            for (int levelIdx = 0; levelIdx < NLevel[chapterIdx]; levelIdx++)
            {
                GameObject entry = Instantiate(pfEntry, scroll);
                entry.transform.localPosition = new Vector3(
                    -400 + 200 * (levelIdx % 5) + Screen.width * chapterIdx,
                    300 - 200 * (levelIdx / 5));
                entry.transform.GetChild(0).GetComponent<Text>().text 
                    = $"{(char)('A' + chapterIdx)}{levelIdx + 1}";
                
                int chapter = chapterIdx + 1, level = levelIdx + 1;
                entry.GetComponent<Button>().onClick.AddListener(() =>
                {
                    M.LevelMap2GamePlay(chapter, level);
                });
            }
        }
    }

    public void Init()
    {
        if (scroll == null)
            BeforeInit();

        GS = GameState.LevelMap;
        gameObject.SetActive(true);

        for (int i = 0, cum = -1; i < NLevel.Length; i++)
        {
            int code = PlayerPrefs.GetInt($"data{i + 1}");
            for (int j = 0; j < NLevel[i]; j++)
            {
                scroll.GetChild(++cum).GetComponent<Image>().color =
                    ((code >> j) & 1) == 1 ? winColor : openColor;
            }
        }
    }
}