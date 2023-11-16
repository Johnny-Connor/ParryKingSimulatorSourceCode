using UnityEngine;
using UnityEngine.UI;

public class ScrollIndicatorArrowsContainer : MonoBehaviour
{
    // Variables.
    [SerializeField] private MainMenuScrollbar _mainMenuScrollBar;
    [SerializeField] private Animator _scrollIndicatorArrowAnimatorStart;
    [SerializeField] private Animator _scrollIndicatorArrowAnimatorEnd;

    // Animator Parameters.
    private int _animIDShouldBeVisible = Animator.StringToHash("ShouldBeVisible");
    private int _animIDScroll = Animator.StringToHash("Scroll");


    // MonoBehaviour.
    private void Awake()
    {
        _mainMenuScrollBar.OnDidScrollTowardsStart += MainMenuScrollBar_OnScrolledTowardsStart;

        UpdateScrollIndicatorArrowsVisibility();
    }


    // Non-MonoBehaviour.
    private void UpdateScrollIndicatorArrowsVisibility()
    {
        Scrollbar scrollbar = _mainMenuScrollBar.GetComponent<Scrollbar>();

        bool shouldStartArrowBeVisible = scrollbar.value < 1;
        bool shouldEndArrowBeVisible = scrollbar.value > 0;

        _scrollIndicatorArrowAnimatorStart.SetBool(_animIDShouldBeVisible, shouldStartArrowBeVisible);
        _scrollIndicatorArrowAnimatorEnd.SetBool(_animIDShouldBeVisible, shouldEndArrowBeVisible);
    }


    // Event Handlers.
    private void MainMenuScrollBar_OnScrolledTowardsStart(bool ScrolledTowardsStart)
    {
        if (ScrolledTowardsStart)
        {
            _scrollIndicatorArrowAnimatorStart.SetTrigger(_animIDScroll);        
        }
        else
        {
            _scrollIndicatorArrowAnimatorEnd.SetTrigger(_animIDScroll);
        }

        UpdateScrollIndicatorArrowsVisibility();
    }
}
