using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip shootAudioClip;
    public AudioClip hitAudioClip;
    public void PlayShootAudio()
    {
        audioSource.PlayOneShot(shootAudioClip);
    }
    public void PlayHitAudio()
    {
        audioSource.PlayOneShot(hitAudioClip);
    }
}