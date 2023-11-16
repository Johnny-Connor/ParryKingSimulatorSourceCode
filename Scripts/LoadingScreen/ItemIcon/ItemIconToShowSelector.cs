using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemIconToShowSelector : MonoBehaviour
{
    // Variables.
    [SerializeField] private List<ItemIcon> _itemIconsList;

    // Events.
    public event Action<Transform> OnItemIconToShowSelected;


    // MonoBehaviour.
    private void Start()
    {
        int itemIconsListCount = _itemIconsList.Count;

        ItemIcon itemIconToShow = _itemIconsList[UnityEngine.Random.Range(0, _itemIconsList.Count)];
        OnItemIconToShowSelected?.Invoke(itemIconToShow.transform);
    }
}
