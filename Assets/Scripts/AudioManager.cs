using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource backgroundMusicSource;
    public AudioSource soundEffectSource;

    public AudioClip[] backgroundMusicClips;
    public AudioClip[] soundEffectClips;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        //DontDestroyOnLoad(gameObject);
    }

    public void PlayBackgroundMusic(int musicIndex)
    {
        if (musicIndex >= 0 && musicIndex < backgroundMusicClips.Length)
        {
            backgroundMusicSource.clip = backgroundMusicClips[musicIndex];
            backgroundMusicSource.Play();
        }
    }

    public void PlaySoundEffect(int soundIndex)
    {
        if (soundIndex >= 0 && soundIndex < soundEffectClips.Length)
        {
            soundEffectSource.clip = soundEffectClips[soundIndex];
            soundEffectSource.Play();
        }
    }

    public void StopBackgroundMusic()
    {
        backgroundMusicSource.Stop();
    }
}