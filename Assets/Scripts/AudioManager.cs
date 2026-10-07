using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource audioSource;

    public AudioClip buttonClick;
    public AudioClip towerAttack;
    public AudioClip enemyHit;
    public AudioClip waveStart;
    public AudioClip towerPlace;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayButtonClick()
    {
        audioSource.PlayOneShot(buttonClick);
    }

    public void PlayTowerAttack()
    {
        audioSource.PlayOneShot(towerAttack);
    }

    public void PlayEnemyHit()
    {
        audioSource.PlayOneShot(enemyHit);
    }

    public void PlayWaveStart()
    {
        audioSource.PlayOneShot(waveStart);
    }

    public void PlayTowerPlace()
    {
        audioSource.PlayOneShot(towerPlace);
    }
}