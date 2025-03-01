using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Displays buttons for the photo gallery and manages the modal.
/// </summary>
public class ContentScreen : MonoBehaviour, IScreenState
{
    [SerializeField] private ContentScreenData contentScreenData; // Data asset
    [SerializeField] private GameObject buttonContainer; // Parent for the main buttons
    [SerializeField] private GameObject modal; // The modal popup
    [SerializeField] private GameObject modalBG; // Image Fade BG
    [SerializeField] private GameObject arrowL; // Left selection arrow
    [SerializeField] private GameObject arrowR; // Right selection arrow
    [SerializeField] private GameObject close; // Close button
    [SerializeField] private GameObject imageHolder; // Image Holder
    [SerializeField] private TextMeshProUGUI textTitle; // Title Text
    [SerializeField] private TextMeshProUGUI textImage; // Image Text

    private Dictionary<string, Sprite> preloadedImages = new Dictionary<string, Sprite>(); // Local image cache
    private List<ContentScreenData.GalleryItem> currentGallery;
    private int currentImageIndex = 0;
    private ImageLoader imageLoader;

    // Auto close code
    private Coroutine autoCloseCoroutine;
    private float lastInteractionTime;

    void Awake()
    {
        CloseModal();

        StartImagePreload();

        // Ensure data is assigned
        if (contentScreenData == null)
        {
            Debug.LogError("ContentScreenData is missing! Assign it in the Inspector.");
            return;
        }

        // Set the names of the buttons
        SetButtonName("Button1", contentScreenData.buttons[0].buttonName);
        SetButtonName("Button2", contentScreenData.buttons[1].buttonName);
        SetButtonName("Button3", contentScreenData.buttons[2].buttonName);
    }

    void Start()
    {
        // Attach event listeners to navigation buttons in the modal
        arrowL.GetComponent<Button>().onClick.AddListener(() => { NavigateGallery(-1); RegisterInteraction(); });
        arrowR.GetComponent<Button>().onClick.AddListener(() => { NavigateGallery(1); RegisterInteraction(); });
        close.GetComponent<Button>().onClick.AddListener(CloseModal);
    }

    private void StartImagePreload()
    {
        // Check if an ImageLoader already exists as a child
        Transform existingLoader = transform.Find("ContentImageLoader");

        if (existingLoader != null)
        {
            imageLoader = existingLoader.GetComponent<ImageLoader>();
        }
        else
        {
            // Create ImageLoader as a child of ContentScreen
            GameObject loaderObject = new GameObject("ContentImageLoader");
            loaderObject.transform.SetParent(transform);
            loaderObject.transform.localPosition = Vector3.zero; 
            loaderObject.transform.localRotation = Quaternion.identity; 
            loaderObject.transform.localScale = Vector3.one; 
            imageLoader = loaderObject.AddComponent<ImageLoader>();
        }

        // Collect all unique images from all galleries
        HashSet<string> uniqueImages = new HashSet<string>();

        foreach (var buttonData in contentScreenData.buttons)
        {
            foreach (var galleryItem in buttonData.gallery)
            {
                uniqueImages.Add(galleryItem.image);
            }
        }

        // Assign images and enable preloading
        imageLoader.ImageFileNames = new List<string>(uniqueImages);
        imageLoader.EnablePreloading();

        Debug.Log($"Collected {imageLoader.ImageFileNames.Count} unique images for preloading.");
    }

    /// <summary>
    /// Set the names of the buttons
    /// </summary>
    private void SetButtonName(string buttonName, string newText)
    {
        Transform buttonTransform = buttonContainer.transform.Find(buttonName);

        if (buttonTransform != null)
        {
            Button button = buttonTransform.GetComponent<Button>();

            if (button != null)
            {
                TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();

                if (buttonText != null)
                {
                    buttonText.text = newText;
                    Debug.Log($"{buttonName} updated to: {newText}");
                }
                else
                {
                    Debug.LogError($"{buttonName} does not have a TMP_Text component!");
                }
            }
            else
            {
                Debug.LogError($"{buttonName} does not have a Button component!");
            }
        }
        else
        {
            Debug.LogError($"Could not find {buttonName} under buttonContainer!");
        }
    }


    /// <summary>
    /// Called on button click to open the modal
    /// </summary>
    public void OpenModal(int value)
    {
        Debug.Log("Modal opened with value: " + value);

        modal.SetActive(true);
        modalBG.SetActive(true);

        SetModalDetails(value);

        // Start the auto-close timer
        RegisterInteraction();
    }

    /// <summary>
    /// Called on button click to close the modal
    /// </summary>
    public void CloseModal()
    {
        Debug.Log("Modal closed");

        modal.SetActive(false);
        modalBG.SetActive(false);

        // Stop auto-close timer if running
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
    }

     /// <summary>
    /// Sets the Specifics for the button and modal clicked and opened
    /// </summary>
    private void SetModalDetails(int value)
    {
        if (value < 0 || value >= contentScreenData.buttons.Count)
        {
            Debug.LogError("Invalid value passed to open Modal.");
            return;
        }

        ContentScreenData.ButtonData buttonData = contentScreenData.buttons[value];
        textTitle.text = buttonData.buttonName;

        currentGallery = buttonData.gallery;
        currentImageIndex = 0;

        if (currentGallery.Count > 0)
        {
            UpdateGalleryDisplay();
        }
        else
        {
            Debug.LogError("No images in gallery for this button.");
        }
    }

    /// <summary>
    /// Navigation controls for the gallery
    /// </summary>
    private void NavigateGallery(int direction)
    {
        if (currentGallery == null || currentGallery.Count == 0)
            return;

        currentImageIndex = (currentImageIndex + direction + currentGallery.Count) % currentGallery.Count;
        UpdateGalleryDisplay();
    }

    /// <summary>
    /// Updates the gallery display with preloaded images
    /// </summary>
    private void UpdateGalleryDisplay()
    {
        if (currentGallery == null || currentGallery.Count == 0)
            return;

        string imageName = currentGallery[currentImageIndex].image;
        Sprite loadedSprite = imageLoader.GetImage(imageName);

        if (loadedSprite != null)
        {
            imageHolder.GetComponent<Image>().sprite = loadedSprite;
            textImage.text = currentGallery[currentImageIndex].description;
        }
        else
        {
            Debug.LogError($"Preloaded image {imageName} not found.");
        }
    }

    /// <summary>
    /// Registers user interaction and resets the auto-close timer.
    /// </summary>
    private void RegisterInteraction()
    {
        lastInteractionTime = Time.time;

        if (autoCloseCoroutine == null)
        {
            autoCloseCoroutine = StartCoroutine(CheckForInactivity());
        }

        Debug.Log("ContentScreen click registered");

        // Also reset the inactivity timer in ScreenManager
        ScreenManager.Instance?.RegisterInteraction();
    }

    /// <summary>
    /// Continuously checks for inactivity and closes modal if no interaction occurs
    /// </summary>
    private IEnumerator CheckForInactivity()
    {
        while (modal.activeSelf)
        {
            if (Time.time - lastInteractionTime >= contentScreenData.autoCloseModalTime)
            {
                CloseModal();
                yield break; 
            }

            yield return null; 
        }
    }

    public void EnterScreen()
    {
        gameObject.SetActive(true);
    }

    public void ExitScreen()
    {
        CloseModal();
    }
}
