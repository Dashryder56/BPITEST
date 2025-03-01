using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class ImageLoader : MonoBehaviour
{
    public List<string> ImageFileNames { get; set; } = new List<string>();
    public List<Sprite> LoadedSprites { get; private set; } = new List<Sprite>();

    private Dictionary<string, Sprite> preloadedImages = new Dictionary<string, Sprite>();
    private bool isPreloadingComplete = false;
    private bool shouldPreload = false;

    void Start()
    {
        if (shouldPreload)
        {
            StartCoroutine(PreloadImages());
        }
    }

    /// <summary>
    /// Enables image preloading before Start() runs.
    /// </summary>
    public void EnablePreloading()
    {
        shouldPreload = true;
    }

    /// <summary>
    /// Preloads all images into memory for fast access.
    /// </summary>
    private IEnumerator PreloadImages()
    {
        Debug.Log("Starting image preloading...");

        foreach (string fileName in ImageFileNames)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
            Debug.Log($"Preloading: {filePath}");

            if (filePath.Contains("://"))
            {
                using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(filePath))
                {
                    yield return request.SendWebRequest();

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"Failed to preload {fileName}: {request.error}");
                        continue;
                    }

                    Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                    preloadedImages[fileName] = TextureToSprite(texture);
                }
            }
            else
            {
                if (File.Exists(filePath))
                {
                    byte[] imageBytes = File.ReadAllBytes(filePath);
                    Texture2D texture = new Texture2D(2, 2);
                    texture.LoadImage(imageBytes);
                    preloadedImages[fileName] = TextureToSprite(texture);
                }
                else
                {
                    Debug.LogError($"File not found: {filePath}");
                }
            }
        }

        isPreloadingComplete = true;
        Debug.Log("Image preloading complete.");
    }

    /// <summary>
    /// Returns a preloaded sprite if available, otherwise falls back to normal loading.
    /// </summary>
    public Sprite GetImage(string fileName)
    {
        if (preloadedImages.ContainsKey(fileName))
        {
            return preloadedImages[fileName];
        }
        else
        {
            Debug.LogWarning($"Image {fileName} not found in preloaded cache. Falling back to standard loading.");
            return null;
        }
    }

    /// <summary>
    /// Loads images normally (original functionality).
    /// </summary>
    public IEnumerator LoadImages()
    {
        LoadedSprites.Clear();

        foreach (string fileName in ImageFileNames)
        {
            if (preloadedImages.ContainsKey(fileName))
            {
                LoadedSprites.Add(preloadedImages[fileName]);
                continue;
            }

            string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
            Debug.Log($"Loading: {filePath}");

            if (filePath.Contains("://"))
            {
                using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(filePath))
                {
                    yield return request.SendWebRequest();

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"Failed to load {fileName}: {request.error}");
                        continue;
                    }

                    Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                    LoadedSprites.Add(TextureToSprite(texture));
                }
            }
            else
            {
                if (File.Exists(filePath))
                {
                    byte[] imageBytes = File.ReadAllBytes(filePath);
                    Texture2D texture = new Texture2D(2, 2);
                    texture.LoadImage(imageBytes);
                    LoadedSprites.Add(TextureToSprite(texture));
                }
                else
                {
                    Debug.LogError($"File not found: {filePath}");
                }
            }
        }
    }

    /// <summary>
    /// Converts a Texture2D to a Sprite.
    /// </summary>
    private Sprite TextureToSprite(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    public bool IsPreloadingComplete()
    {
        return isPreloadingComplete;
    }
}
