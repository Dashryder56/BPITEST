using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Slideshow where the next image fades in over the current one.
/// The first image appears instantly without a fade.
/// </summary>
public class AttractScreen : MonoBehaviour, IScreenState, IFadeableScreen
{
    [SerializeField] private AttractScreenData attractScreenData;

    // Private Variables
    private Image activeImage;
    private Image nextImage;
    private CanvasGroup imageCanvasGroup;
    private CanvasGroup screenCanvasGroup;
    private Coroutine slideshowCoroutine;
    private int currentImageIndex = -1;
    private bool inputEnabled = false;

    // External Classes
    private TitleTextManager titleTextManager;
    private ImageLoader imageLoader;
    private ScreenManagerData screenManagerData;

    void Awake()
    {
        // Validate the data asset
        if (attractScreenData == null)
        {
            Debug.LogError("AttractScreenData is missing! Assign it in the Inspector.");
            return;
        }

        // Get `ScreenManagerData` from the `ScreenManager` in the parent
        ScreenManager screenManager = GetComponentInParent<ScreenManager>();

        if (screenManager != null)
        {
            screenManagerData = screenManager.GetScreenManagerData();
        }
        else
        {
            Debug.LogError("ScreenManager not found in parent! Ensure AttractScreen is inside the correct hierarchy.");
        }

        // Create two UI Images from the ImageFactory class, to dynamically swap between for a slideshow
        activeImage = UIImageFactory.CreateImage("ActiveImage");
        activeImage.transform.SetParent(transform, false);

        nextImage   = UIImageFactory.CreateImage("NextImage", out imageCanvasGroup);
        nextImage.transform.SetParent(transform, false);

        // CanvasGroup for the screen itself
        screenCanvasGroup = GetComponent<CanvasGroup>();
        if (screenCanvasGroup == null)
            screenCanvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Initialize the TitleTextManager
        titleTextManager = new TitleTextManager();
        titleTextManager.CreateTitleText("TitleText", attractScreenData.titleText, transform);

        // Initialize the ImageLoader
        imageLoader = new ImageLoader { ImageFileNames = new List<string>(attractScreenData.imageFileNames) };

        activeImage.color = new Color(1, 1, 1, 1);
        imageCanvasGroup.alpha  = 0;
        screenCanvasGroup.alpha = 1;
    }

    void Start()
    {
        inputEnabled = true;

        StartCoroutine(LoadImagesAndStartSlideshow());

        // Set default text if none provided
        if (string.IsNullOrEmpty(attractScreenData.titleText))
        {
            titleTextManager.UpdateTitleText("Tap Screen to Begin");
        }
    }

    void Update()
    {
        // Code to detect clicks and advance to the next page on click, for both mobile and desktop
        if (inputEnabled && (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)))
        {
            Debug.Log("Screen tapped at: " + (Input.touchCount > 0 ? (Vector3)Input.GetTouch(0).position : Input.mousePosition));
            inputEnabled = false;
            OnScreenTap();
        }
    }

    public void EnterScreen()
    {
        gameObject.SetActive(true);
        inputEnabled = true;

        if (slideshowCoroutine != null)
            StopCoroutine(slideshowCoroutine);

        slideshowCoroutine = StartCoroutine(SlideshowLoop());
    }

    public void ExitScreen()
    {
        if (slideshowCoroutine != null)
            StopCoroutine(slideshowCoroutine);

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Handles user tap to transition to the next screen.
    /// </summary>
    public void OnScreenTap()
    {
        Debug.Log("Screen tapped!");
        ScreenManager.RequestScreenChange("ContentScreen");
    }

    /// <summary>
    /// Loads the images from the streaming folder and begins the slideshow
    /// </summary>
    IEnumerator LoadImagesAndStartSlideshow()
    {
        yield return StartCoroutine(imageLoader.LoadImages());

        if (imageLoader.LoadedSprites.Count > 0)
        {
            Debug.Log($"Loaded {imageLoader.LoadedSprites.Count} images. Starting slideshow.");
            activeImage.sprite = imageLoader.LoadedSprites[imageLoader.LoadedSprites.Count - 1];
            activeImage.gameObject.SetActive(true);

            slideshowCoroutine = StartCoroutine(SlideshowLoop());
        }
        else
        {
            Debug.LogError("No images were loaded from AttractScreenData!");
        }
    }

    /// <summary>
    /// Slideshow where next image fades in over the active one, with no fade outs.
    /// The first image appears instantly with no fade-in.
    /// </summary>
    IEnumerator SlideshowLoop()
    {
        while (true)
        {
            if (imageLoader.LoadedSprites.Count == 0)
            {
                Debug.LogWarning("No images available for slideshow.");
                yield return null;
                continue;
            }

            yield return new WaitForSeconds(attractScreenData.slideshowInterval);

            currentImageIndex = (currentImageIndex + 1 < imageLoader.LoadedSprites.Count) ? (currentImageIndex + 1) : 0;

            nextImage.sprite = imageLoader.LoadedSprites[currentImageIndex];
            imageCanvasGroup.alpha = 0; 
            nextImage.gameObject.SetActive(true);

            yield return StartCoroutine(FadeIn(imageCanvasGroup));

            // After fade-in completes, make next image the active one
            SwapImages();
        }
    }

    /// <summary>
    /// Fades in an image while keeping the previous one fully visible.
    /// </summary>
    IEnumerator FadeIn(CanvasGroup canvasGroup)
    {
        float timer = 0;
        while (timer < attractScreenData.fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, timer / attractScreenData.fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1;
    }

    /// <summary>
    /// Swaps active and next images after the transition is complete.
    /// </summary>
    private void SwapImages()
    {
        // Move nextImage into the active position, no fade out needed
        activeImage.sprite = nextImage.sprite;
        imageCanvasGroup.alpha = 0;
    }

    /// <summary>
    /// Fade in code for the overall screen
    /// </summary>
    public IEnumerator FadeIn()
    {
        Debug.Log("Attract Screen Fade In");

        float startAlpha = screenCanvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < screenManagerData.screenTransitionDuration)
        {
            elapsedTime += Time.deltaTime;
            screenCanvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, elapsedTime / screenManagerData.screenTransitionDuration);
            yield return null;
        }

        screenCanvasGroup.alpha = 1f;
        
        EnterScreen();
    }

    /// <summary>
    /// Fade out code for the overall screen
    /// </summary>
    public IEnumerator FadeOut()
    {
        Debug.Log("Attract Screen Fade Out");

        float startAlpha = screenCanvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < screenManagerData.screenTransitionDuration)
        {
            elapsedTime += Time.deltaTime;
            screenCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / screenManagerData.screenTransitionDuration);
            yield return null;
        }

        screenCanvasGroup.alpha = 0f;

        gameObject.SetActive(false);
        
        ExitScreen();
    }
}
  