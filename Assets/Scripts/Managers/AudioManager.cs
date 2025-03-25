using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioSource sfxPrefab;
    [SerializeField] private AudioMixer mixer;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void PlaySFXClip(AudioClip clip, Transform location, float volume)
    {
        AudioSource source = Instantiate(sfxPrefab, location.position, Quaternion.identity);
        source.clip = clip;
        source.volume = volume;
        source.Play();
        Destroy(source.gameObject, source.clip.length);
    }
    public void SetMasterVolume(float volume)
    {
        mixer.SetFloat("masterVolume", volume);
    }
    public void SetSFXVolume(float volume)
    {
        mixer.SetFloat("sfxVolume", volume);
    }
}
