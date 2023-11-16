using UnityEngine;
using UnityEngine.SceneManagement;

public class DevCommands : MonoBehaviour
{
    [SerializeField] private GameObject _playerUI;
    private bool _playerUiSwitch;

    private void Start()
    {
        _playerUI.SetActive(_playerUiSwitch);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) ToggleUI();

        if (Input.GetKeyDown(KeyCode.L)) ReloadScene();
    }

    private void ToggleUI()
    {
        _playerUiSwitch = !_playerUiSwitch;
        _playerUI.SetActive(_playerUiSwitch);
    }

    private void ReloadScene() => SceneManager.LoadScene("GameScene");
}
