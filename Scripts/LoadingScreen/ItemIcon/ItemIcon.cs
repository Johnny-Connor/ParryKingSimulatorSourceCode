using UnityEngine;

public class ItemIcon : MonoBehaviour
{
    [SerializeField] private string _name = "Lorem";
    public string Name => _name; 

    [SerializeField] [TextArea(1, 1)] private string _summaryDescription = "Lorem Ipsum.";
    public string SummaryDescription => _summaryDescription;

    [SerializeField] [TextArea] private string _description = "Lorem ipsum dolor sit amet.";
    public string Description => _description;
}
