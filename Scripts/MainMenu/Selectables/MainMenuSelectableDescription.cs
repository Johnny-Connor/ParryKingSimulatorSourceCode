using UnityEngine;

public class MainMenuSelectableDescription : MonoBehaviour
{
    [Tooltip("Description shown in description displayer when selected.")]
    [SerializeField] [TextArea] private string _description = "Lorem Ipsum.";
    public string Description => _description;
}