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
        { // Set the static reference to this
            Instance = this;
        }
    }

    public void PlaySFXClip(AudioClip clip, Transform location, float volume)
    {
        AudioSource source = Instantiate(sfxPrefab, location.position, Quaternion.identity); // Make a new prefab at the location specified with no rotation
        source.clip = clip; // Set the clip to start playing
        source.volume = volume; // Set the volume of the clip
        source.Play(); // Allow the clip to start playing
        Destroy(source.gameObject, source.clip.length); // Destroy the object once the clip ends
    }
    public void SetMasterVolume(float volume)
    { // Set the volume of the Master channel
        mixer.SetFloat("masterVolume", volume);
    }
    public void SetSFXVolume(float volume)
    { // Set the volume of the SFX channel
        mixer.SetFloat("sfxVolume", volume);
    }
}
