using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages screen transitions and handles user inactivity.
/// </summary>
public class ScreenManager : MonoBehaviour
{
    [SerializeField] public ScreenManagerData screenManagerData;

    public static ScreenManager Instance { get; private set; }

    // Getter for Screen Manager Data
    public ScreenManagerData GetScreenManagerData()
    {
        return screenManagerData;
    }

    /// <summary>
    /// Event triggered when a screen change is requested.
    /// </summary>
    public static event Action<string> OnScreenChangeRequested;

    private IScreenState currentScreen;
    private Dictionary<string, IScreenState> screens = new Dictionary<string, IScreenState>();

    private float inactivityTimer;
    private bool isAttractScreenActive;
    private Coroutine autoCloseScreenCoroutine;
    private float lastInteractionTime;

    void Awake()
    {
        OnScreenChangeRequested += SetScreen;
    }

    void Start()
    {
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        inactivityTimer = screenManagerData.inactivityTimeout;

        // Find all screens in the scene
        foreach (var screen in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (screen is IScreenState screenState)
            {
                screens[screen.gameObject.name] = screenState;
                Debug.Log($"Screen {screen.gameObject.name} found!");
            }
        }

        // Start on the Attract Screen if it exists
        isAttractScreenActive = screens.ContainsKey("AttractScreen");
        if (isAttractScreenActive) SetScreen("AttractScreen");
    }

    void Update()
    {
        if (!isAttractScreenActive) // Only run inactivity timer if NOT on the Attract Screen
        {
            inactivityTimer -= Time.deltaTime;

            if (inactivityTimer <= 0)
            {
                // Close up the content screen model on timeout if necessary
                if (screens.TryGetValue("ContentScreen", out IScreenState screen))
                {
                    if (screen is ContentScreen contentScreen)
                    {
                        contentScreen.CloseModal();
                    }
                }

                // Return to the attract screen
                RequestScreenChange("AttractScreen");
            }

            // Reset inactivity timer when user clicks
            if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
            {
                inactivityTimer = screenManagerData.inactivityTimeout;
            }
        }
    }

    void OnDestroy()
    {
        OnScreenChangeRequested -= SetScreen;
    }

    /// <summary>
    /// Sets the active screen by name.
    /// </summary>
    /// <param name="screenName">The name of the screen to activate.</param>
    private void SetScreen(string screenName)
    {
        bool wasAttractScreen = isAttractScreenActive;
        isAttractScreenActive = screenName == "AttractScreen";

        if (isAttractScreenActive)
        {
            inactivityTimer = screenManagerData.inactivityTimeout; 
        }

        if (currentScreen != null)
            StartCoroutine(FadeOutAndSwitch(screenName));
        else
            ActivateScreen(screenName);
    }

    private void ActivateScreen(string screenName)
    {
        if (screens.TryGetValue(screenName, out IScreenState newScreen))
        {
            currentScreen = newScreen;
            currentScreen.EnterScreen();
        }
        else
        {
            Debug.LogWarning($"Screen {screenName} not found!");
        }
    }

    private IEnumerator FadeOutAndSwitch(string newScreen)
    {
        if (currentScreen is IFadeableScreen fadeOutScreen)
            yield return fadeOutScreen.FadeOut();

        ActivateScreen(newScreen);

        if (currentScreen is IFadeableScreen fadeInScreen)
            yield return fadeInScreen.FadeIn();
    }

    /// <summary>
    /// Requests a screen change via event-driven logic.
    /// </summary>
    /// <param name="screenName">The name of the screen to switch to.</param>
    public static void RequestScreenChange(string screenName)
    {
        OnScreenChangeRequested?.Invoke(screenName);
    }

    /// <summary>
    /// Registers user interaction and resets the auto-close timer.
    /// </summary>
    public void RegisterInteraction()
    {
        lastInteractionTime = Time.time;

        if (autoCloseScreenCoroutine == null)
        {
            autoCloseScreenCoroutine = StartCoroutine(CheckForScreenInactivity());
        }

        Debug.Log("ScreenManager click registered");
    }

    /// <summary>
    /// Continuously checks for inactivity and closes screen if no interaction occurs and not on attract screen
    /// </summary>
    private IEnumerator CheckForScreenInactivity()
    {
        while (!isAttractScreenActive)
        {
            if (Time.time - lastInteractionTime >= inactivityTimer)
            {
                SetScreen("AttractScreen");
                yield break; 
            }

            yield return null; 
        }
    }
}
