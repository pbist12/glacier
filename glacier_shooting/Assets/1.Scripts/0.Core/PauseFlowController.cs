using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseFlowController : MonoBehaviour
{
    public static PauseFlowController Instance { get; private set; }

    public enum UIState { Gameplay, PauseMenu, Settings }

    [Header("Refs")]
    [SerializeField] private PauseMenuPanel pauseMenu;
    [SerializeField] private SettingsPanel settingsPanel;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference pauseAction;
    [SerializeField] private InputActionReference cancelAction;

    [SerializeField] private UIState _state = UIState.Gameplay;
    private float _blockCancelUntil = 0f; // unscaledTime 기준

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        if (pauseAction) pauseAction.action.performed += OnPausePerformed;
        if (cancelAction) cancelAction.action.performed += OnCancelPerformed;
        EnableGameplayMap(true);
        EnableUIMap(false);
    }

    private void OnDisable()
    {
        if (pauseAction) pauseAction.action.performed -= OnPausePerformed;
        if (cancelAction) cancelAction.action.performed -= OnCancelPerformed;
    }

    void OnPausePerformed(InputAction.CallbackContext _)
    {
        if (_state == UIState.Gameplay) OpenPauseMenu();
        else if (_state == UIState.PauseMenu) ResumeGame();
        else if (_state == UIState.Settings) BackToPauseMenu();
    }

    void OnCancelPerformed(InputAction.CallbackContext _)
    {
        // 디바운스: 방금 연 메뉴는 Cancel 무시
        if (Time.unscaledTime < _blockCancelUntil) return;

        if (_state == UIState.Settings) BackToPauseMenu();
        else if (_state == UIState.PauseMenu) ResumeGame();
    }

    public void OpenPauseMenu()
    {
        _state = UIState.PauseMenu;
        settingsPanel?.Hide();
        pauseMenu?.Show();

        // ESC 눌림 잔상 차단
        _blockCancelUntil = Time.unscaledTime + 0.2f;
    }

    public void BackToPauseMenu()
    {
        _state = UIState.PauseMenu;
        settingsPanel?.Hide();
        pauseMenu?.Show();
    }

    public void OpenSettings()
    {
        _state = UIState.Settings;
        pauseMenu?.Hide();
        settingsPanel?.Show();
    }

    public void ResumeGame()
    {
        _state = UIState.Gameplay;
        pauseMenu?.Hide();
        settingsPanel?.Hide();
    }

    void EnableGameplayMap(bool enable)
    {
        // 예시: PlayerInput을 쓰지 않고 ActionReference 직접 on/off
        if (pauseAction)
        {
            if (enable) pauseAction.action.Enable();
            else pauseAction.action.Disable();
        }
    }

    void EnableUIMap(bool enable)
    {
        if (cancelAction)
        {
            if (enable) cancelAction.action.Enable();
            else cancelAction.action.Disable();
        }
    }

    public void QuitToTitle()
    {
        ResumeGame();
        SceneLoader.Instance.LoadScene("LobbyScene");
    }
}
