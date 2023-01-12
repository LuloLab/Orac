using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Global;

public class AudioController : MonoBehaviour
{
    public AudioClip levelComplete, clickButton, quadIce, 
        quadQuad, quadStuck, quadGround, quadWall, quadHome,
        quadIron;
    public bool soundOn;

    private AudioSource audioSource;
    private void Awake()
    {
        A = this;
    }
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        soundOn = PlayerPrefs.GetInt("soundOn") == 1;
        audioSource.mute = !soundOn;
    }
    public void ToggleSound()
    {
        soundOn = !soundOn;
        PlayerPrefs.SetInt("soundOn", soundOn ? 1 : 0);
        PlayerPrefs.Save();
        audioSource.mute = !soundOn;
    }

    public void LevelComplete()
    {
        audioSource.PlayOneShot(levelComplete, 0.15f);
    }
    public void ClickButton()
    {
        audioSource.PlayOneShot(clickButton, 0.5f);
    }
    public void QuadQuad()
    {
        audioSource.PlayOneShot(quadQuad, 0.5f);
    }
    public void QuadIron()
    {
        audioSource.PlayOneShot(quadIron, 0.25f);
    }
    public void IronIron()
    {
        audioSource.PlayOneShot(quadIron, 0.5f);
    }
    public void QuadStuck()
    {
        audioSource.PlayOneShot(quadStuck, 0.12f);
    }
    public void QuadWall()
    {
        audioSource.PlayOneShot(quadWall, 0.2f);
    }
    public void QuadHome()
    {
        audioSource.PlayOneShot(quadHome, 0.2f);
    }
    public void QuadIce()
    {
        audioSource.PlayOneShot(quadIce, 0.1f);
    }
    public void QuadGround(float scale)
    {
        audioSource.PlayOneShot(quadGround, scale);
    }
}
