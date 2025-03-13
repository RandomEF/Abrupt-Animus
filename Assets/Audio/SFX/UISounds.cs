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
        Addressables.LoadAssetAsync<AudioClip>("Assets/Audio/SFX/UIHover.wav").Completed += (asyncOp) => {
            if (asyncOp.Status == AsyncOperationStatus.Succeeded){
                uiHover = asyncOp.Result;
            } else {
                Debug.LogError("Failed to load UI audio.");
            }
        };
        Addressables.LoadAssetAsync<AudioClip>("Assets/Audio/SFX/UISelect.wav").Completed += (asyncOp) => {
            if (asyncOp.Status == AsyncOperationStatus.Succeeded){
                uiSelect = asyncOp.Result;
            } else {
                Debug.LogError("Failed to load UI audio.");
            }
        };
    }
    void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void OnPointerEnter(PointerEventData eventData){
        AudioManager.Instance.PlaySFXClip(uiHover, transform, 1);
    }
    void OnClick(){
        AudioManager.Instance.PlaySFXClip(uiSelect, transform, 1);
    }
}
