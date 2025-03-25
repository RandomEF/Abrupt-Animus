using UnityEngine;
using UnityEngine.UI;

public class Options : MonoBehaviour
{
    [SerializeField] private Slider volume;
    [SerializeField] private Slider fov;
    [SerializeField] private Slider zoomFov;

    public void UpdateVolume()
    {
        AudioManager.Instance.SetMasterVolume(Mathf.Lerp(-80, 0, volume.value)); // Set the master volume to the value of the slider
    }
    public void UpdateNormalFOV()
    {
        PlayerManager.Instance.player.GetComponent<PlayerGunInteraction>().fov = fov.value; // Set the normal FOV
    }
    public void UpdateZoomedFOV()
    {
        PlayerManager.Instance.player.GetComponent<PlayerGunInteraction>().zoomFov = zoomFov.value; // Set the zoomed in FOV
    }
}
