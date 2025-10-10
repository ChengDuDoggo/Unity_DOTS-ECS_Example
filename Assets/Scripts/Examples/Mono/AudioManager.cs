using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioSource audioSource;
    public AudioClip shootAudioClip;
    public AudioClip hitAudioClip;
    private void Awake()
    {
        Instance = this;
    }
    public void PlayShootAudio()
    {
        audioSource.PlayOneShot(shootAudioClip);
    }
    public void PlayHitAudio()
    {
        audioSource.PlayOneShot(hitAudioClip);
    }
}