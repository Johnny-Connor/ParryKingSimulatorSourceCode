using UnityEngine;

public class ItemIconSpawner : MonoBehaviour
{
    // Variables.
    [SerializeField] private ItemIconToShowSelector _itemIconToShowSelector;


    // MonoBehaviour.
    private void Awake() => _itemIconToShowSelector.OnItemIconToShowSelected += InstantiateItemIcon;


    // Non-MonoBehaviour.
    private void InstantiateItemIcon(Transform selectedPrefab)
    {
        Instantiate(selectedPrefab, transform.position, transform.rotation, transform);
    }
}
