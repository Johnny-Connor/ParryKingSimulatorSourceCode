using TMPro;
using UnityEngine;

public class ItemIconTextsDisplayer : MonoBehaviour
{
    // Variables.
    [SerializeField] private ItemIconToShowSelector _itemIconToShowSelector;

    [Header("Text fields.")]
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _summaryDescriptionText;
    [SerializeField] private TMP_Text _descriptionText;


    // MonoBehaviour.
    private void Awake() => _itemIconToShowSelector.OnItemIconToShowSelected += SetTextFieldsTexts;


    // Non-MonoBehaviour.
    private void SetTextFieldsTexts(Transform selectedPrefab)
    {
        ItemIcon selectedItemIcon = selectedPrefab.GetComponent<ItemIcon>();

        _nameText.text = selectedItemIcon.Name;
        _summaryDescriptionText.text = selectedItemIcon.SummaryDescription;
        _descriptionText.text = selectedItemIcon.Description;
    }
}
