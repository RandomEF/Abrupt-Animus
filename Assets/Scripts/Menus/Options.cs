using UnityEngine;
using UnityEngine.UI;

public class Options : MonoBehaviour
{
    [SerializeField] private Slider volume;
    
    void UpdateVolume(){
        AudioManager.Instance.SetMasterVolume(volume.value);
    }
}
