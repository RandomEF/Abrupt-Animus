using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class UISounds : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] AudioClip uiHover;
    [SerializeField] AudioClip uiSelect;

    void Awake()
    {
        Addressables.LoadAssetAsync<AudioClip>("Assets/Audio/SFX/UIHover.wav").Completed += (asyncOp) =>
        { // Attempt to load the hover effect
            if (asyncOp.Status == AsyncOperationStatus.Succeeded)
            { // If the load succeeds
                uiHover = asyncOp.Result; // Assign the hover sound
            }
            else
            {
                Debug.LogError("Failed to load UI audio.");
            }
        };
        Addressables.LoadAssetAsync<AudioClip>("Assets/Audio/SFX/UISelect.wav").Completed += (asyncOp) =>
        { // Attempt to load the select effect
            if (asyncOp.Status == AsyncOperationStatus.Succeeded)
            { // If the load succeeds
                uiSelect = asyncOp.Result; // Assign the hover sound
            }
            else
            {
                Debug.LogError("Failed to load UI audio.");
            }
        };
    }
    void Start()
    { // Run OnClick when the button is clicked
        gameObject.GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        AudioManager.Instance.PlaySFXClip(uiSelect, transform, 1); // Play uiSelect
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    { // When the mouse hovers over the button
        AudioManager.Instance.PlaySFXClip(uiHover, transform, 1); // Play uiHover
    }
}
