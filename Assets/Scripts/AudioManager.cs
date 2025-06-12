using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    
    [Header("Music Clips")]
    public AudioClip backgroundMusic;
    public AudioClip gameOverMusic;
    
    [Header("SFX Clips")]
    public AudioClip towerShoot;
    public AudioClip enemyHit;
    public AudioClip enemyDeath;
    public AudioClip towerPlace;
    public AudioClip towerUpgrade;
    public AudioClip waveStart;
    public AudioClip buttonClick;
    
    public static AudioManager Instance;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        PlayBackgroundMusic();
    }
    
    public void PlayBackgroundMusic()
    {
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }
    
    public void PlayGameOverMusic()
    {
        if (musicSource != null && gameOverMusic != null)
        {
            musicSource.clip = gameOverMusic;
            musicSource.loop = false;
            musicSource.Play();
        }
    }
    
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }
    
    public void PlayTowerShoot()
    {
        PlaySFX(towerShoot, 0.7f);
    }
    
    public void PlayEnemyHit()
    {
        PlaySFX(enemyHit, 0.6f);
    }
    
    public void PlayEnemyDeath()
    {
        PlaySFX(enemyDeath, 0.8f);
    }
    
    public void PlayTowerPlace()
    {
        PlaySFX(towerPlace, 0.9f);
    }
    
    public void PlayTowerUpgrade()
    {
        PlaySFX(towerUpgrade, 0.8f);
    }
    
    public void PlayWaveStart()
    {
        PlaySFX(waveStart, 1f);
    }
    
    public void PlayButtonClick()
    {
        PlaySFX(buttonClick, 0.5f);
    }
    
    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = volume;
        }
    }
    
    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = volume;
        }
    }
}