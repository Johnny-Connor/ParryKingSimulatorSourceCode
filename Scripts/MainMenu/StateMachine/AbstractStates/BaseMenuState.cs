using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseMenuState : IMainMenuState
{
    #region Variables
    // Variables.
    protected MainMenuController _menuController;
    protected IMainMenuStateMachineContext _menuStateMachineContext;
    private GameObject _selector;
    private int _selectedSelectableIndex = -1;

    // Index-selectable dictionaries.
    private Dictionary<int, Selectable> _indexToSelectableDictionary;
    private Dictionary<Selectable, int> _selectableToIndexDictionary;

    // Screen dictionaries.
    private Dictionary<Type, GameObject> _mainScreenToGameObjectDictionary;
    private Dictionary<Type, GameObject> _popUpScreenToGameObjectDictionary;

    // Properties.
    private Selectable SelectedSelectable => _indexToSelectableDictionary[_selectedSelectableIndex];
    private Vector3 SelectedSelectablePosition => SelectedSelectable.transform.position;

    public abstract Selectable[] SelectablesList { get; }
    public abstract bool IsNavigationHorizontal { get; }
    public abstract bool IsPopUpScreen { get; }

    // Events.
    public Action<Selectable> OnSelectableChanged;
    public Action<float> OnScrollbarValueChanged;
    #endregion Variables


    #region Constructor
    // Constructor.
    protected BaseMenuState(
        MainMenuController mainMenuController, 
        IMainMenuStateMachineContext menuStateMachineContext)
    {
        void SetupScreenDictionaries()
        {
            _mainScreenToGameObjectDictionary ??= new()
            {
                [typeof(StartState)] = _menuController.StartScreen,
                [typeof(MenuState)] = _menuController.MenuScreen,
                [typeof(OptionsState)] = _menuController.OptionsScreen,
                [typeof(CreditsState)] = _menuController.CreditsScreen
            };

            _popUpScreenToGameObjectDictionary ??= new()
            {
                [typeof(QuitConfirmationState)] = _menuController.QuitConfirmationScreen,
            };
        }

        void SetupSelectableIndexDictionaries()
        {
            _indexToSelectableDictionary ??= new Dictionary<int, Selectable>();
            _selectableToIndexDictionary ??= new Dictionary<Selectable, int>();

            for (int i = 0; i < SelectablesList.Length; i++)
            {
                _indexToSelectableDictionary[i] = SelectablesList[i];
                _selectableToIndexDictionary[SelectablesList[i]] = i;
            }
        }

        _menuController = mainMenuController;
        _menuStateMachineContext = menuStateMachineContext;

        SetupScreenDictionaries();
        SetupSelectableIndexDictionaries();
    }
    #endregion Constructor


    #region State Methods
    // State Methods.
    public virtual void OnStateEnter()
    {
        // Sets the initial selected selectable index.
        if (_selectedSelectableIndex == -1) _selectedSelectableIndex = SelectablesList.Length - 1;

        // Instantiates and sets intended selector prefab.
        UpdateSelectorVisual();
        UpdateSelectorTransform();

        // Switches to state's corresponding screen.
        if (IsPopUpScreen)
        {
            ShowPopUpScreen();
        }
        else
        {
            DisablePopUpScreens();
            ShowMainScreen();
        }

        // Subscribes to state's selectables events.
        foreach (Selectable selectable in SelectablesList)
        {
            MainMenuSelectable menuSelectable = selectable.GetComponent<MainMenuSelectable>();
            menuSelectable.OnPointerEnterEvent += MainMenuSelectable_OnPointerEnterEvent;
        }
    }

    public virtual void OnStateExit()
    {
        // Destroys the selector prefab from this state, preventing it from being visible in a pop up state.
        GameObject.Destroy(_selector);

        // Unsubscribes from state's selectables events.
        foreach (Selectable Selectable in SelectablesList)
        {
            MainMenuSelectable menuSelectable = Selectable.GetComponent<MainMenuSelectable>();
            menuSelectable.OnPointerEnterEvent -= MainMenuSelectable_OnPointerEnterEvent;
        }
    }

    public virtual void OnAnyKeyPerformed() { /* Do nothing. */ }

    public abstract void OnBackPerformed();

    public virtual void OnConfirmPerformed()
    {
        if (SelectedSelectable.TryGetComponent(out ConfirmableSelectable confirmableSelectable))
        {
            confirmableSelectable.DoConfirm();
        }
    }

    public virtual void OnMovePerformed(Vector2 direction)
    {
        NavigateThroughMenu(direction);

        // If is selecting a slider.
        if (SelectedSelectable.GetComponent<Slider>() != null) 
        {
            ControlSlider(direction);
            return;
        }

        // If is selecting a scroll.
        if (SelectedSelectable.GetComponent<Scrollbar>() != null) 
        {
            ControlScrollbar(direction);
            return;
        }
    }

    public void GoToState(IMainMenuState newState) => _menuStateMachineContext.SetState(newState);
    #endregion State Methods


    #region Class Methods
    // Class Methods.
    private void NavigateThroughMenu(Vector2 direction)
    {
        Selectable oldSelectable = SelectedSelectable;

        float navigationDirection = IsNavigationHorizontal ? direction.x : direction.y;

        if (navigationDirection > 0)
        {
            // Move selection index up.
            _selectedSelectableIndex++;

            // Check if index is out of bounds.
            if (_selectedSelectableIndex > _indexToSelectableDictionary.Count - 1)
            {
                _selectedSelectableIndex = 0;
            }
        }
        else if (navigationDirection < 0)
        {
            // Move selection index down.
            _selectedSelectableIndex--;

            // Check if index is out of bounds.
            if (_selectedSelectableIndex < 0)
            {
                _selectedSelectableIndex = _indexToSelectableDictionary.Count - 1;
            }
        }

        TryUpdateSelector(oldSelectable, SelectedSelectable);
    }

    private void ControlScrollbar(Vector2 direction)
    {
        Scrollbar selectedScrollbar = 
            _indexToSelectableDictionary[_selectedSelectableIndex].GetComponent<Scrollbar>();

        bool isScrollbarNavigationHorizontal = 
            selectedScrollbar.direction == Scrollbar.Direction.LeftToRight || 
            selectedScrollbar.direction == Scrollbar.Direction.RightToLeft;

        // Determine the control direction based on the scrollbar's orientation.
        float controlDirection = isScrollbarNavigationHorizontal ? direction.x : direction.y;

        // Calculate the scroll step size based on the number of steps.
        float scrollStepSize = 1f / selectedScrollbar.numberOfSteps;

        // Adjust the scrollbar's value based on the control direction and step size.
        selectedScrollbar.value = 
            Mathf.Clamp01(selectedScrollbar.value + controlDirection * scrollStepSize);

        OnScrollbarValueChanged?.Invoke(selectedScrollbar.value);
    }

    private void ControlSlider(Vector2 direction)
    {
        Slider selectedSlider = 
            _indexToSelectableDictionary[_selectedSelectableIndex].GetComponent<Slider>();

        bool isSliderNavigationHorizontal = 
            selectedSlider.direction == Slider.Direction.LeftToRight || 
            selectedSlider.direction == Slider.Direction.RightToLeft;

        // Determine the control direction based on the slider's orientation.
        float controlDirection = isSliderNavigationHorizontal ? direction.x : direction.y;

        // Adjust the slider's value based on the control direction and step size.
        selectedSlider.value = Mathf.Clamp(
            selectedSlider.value + controlDirection,
            selectedSlider.minValue,
            selectedSlider.maxValue
        );
    }

    private void ShowPopUpScreen()
    {
        _menuController.ModalPopUpScreenBackground.enabled = true;

        // Show the requested screen.
        if (_popUpScreenToGameObjectDictionary.TryGetValue(GetType(), out var screenToActivate))
        {
            screenToActivate.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"No pop up screen found for state type {GetType()}.");
        }
    }

    private void DisablePopUpScreens()
    {
        _menuController.ModalPopUpScreenBackground.enabled = false;

        foreach (var popUpScreen in _popUpScreenToGameObjectDictionary.Values)
        {
            popUpScreen.SetActive(false);
        }
    }

    private void ShowMainScreen()
    {
        // Hide all main screens.
        foreach (var mainScreen in _mainScreenToGameObjectDictionary.Values)
        {
            mainScreen.SetActive(false);
        }

        // Show the requested main screen.
        if (_mainScreenToGameObjectDictionary.TryGetValue(GetType(), out var screenToActivate))
        {
            screenToActivate.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"No main screen found for state type {GetType()}.");
        }
    }

    private void UpdateSelectorVisual()
    {
        // Deletes old selector.
        if (_selector != null) GameObject.Destroy(_selector);

        GameObject intendedSelectorPrefab = 
            SelectedSelectable.GetComponent<MainMenuSelectable>().IntendedSelector;

        Transform intendedSelectorParent = IsPopUpScreen ? 
            _popUpScreenToGameObjectDictionary[GetType()].transform : 
            _mainScreenToGameObjectDictionary[GetType()].transform;

        // Instantiates intended selector.
        _selector = GameObject.Instantiate(
            intendedSelectorPrefab, 
            SelectedSelectablePosition, 
            Quaternion.identity, 
            intendedSelectorParent
        );
    }

    private void UpdateSelectorTransform()
    {
        // Sets the position of the selector prefab to match that of the selected selectable.
        _selector.transform.position = SelectedSelectablePosition;

        // Sets the size of the selector prefab to match that of the selected selectable.
        RectTransform _selectorRectTransform = _selector.GetComponent<RectTransform>();
        RectTransform selectedSelectableRectTransform = SelectedSelectable.GetComponent<RectTransform>();
        _selectorRectTransform.sizeDelta = selectedSelectableRectTransform.sizeDelta;
    }
    
    private void TryUpdateSelector(Selectable oldSelectable, Selectable newSelectable)
    {
        // If a valid navigation move was made, update the selector.
        if (oldSelectable != SelectedSelectable)
        {
            OnSelectableChanged?.Invoke(SelectedSelectable);

            // Prevents mouse from interacting with the now unselected old selectable.
            oldSelectable.interactable = false;
            SelectedSelectable.interactable = true;

            GameObject oldSelectorPrefab = 
                oldSelectable.GetComponent<MainMenuSelectable>().IntendedSelector;
            GameObject intendedSelectorPrefab = 
                SelectedSelectable.GetComponent<MainMenuSelectable>().IntendedSelector;

            if (oldSelectorPrefab != intendedSelectorPrefab) UpdateSelectorVisual();

            UpdateSelectorTransform();
        }
    }
    #endregion Class Methods


    #region Event Handlers
    // Event Handlers.
    private void MainMenuSelectable_OnPointerEnterEvent(Selectable senderSelectable)
    {
        Selectable oldSelectable = SelectedSelectable;

        // Update selected selectable index with the index of senderSelectable.
        _selectedSelectableIndex = _selectableToIndexDictionary[senderSelectable];

        TryUpdateSelector(oldSelectable, SelectedSelectable);
    }
    #endregion Event Handlers
}
