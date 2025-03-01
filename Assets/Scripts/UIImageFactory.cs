using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;

public class UIImageFactory
{
    /// <summary>
    /// Creates a UI Image dynamically and attaches it to the Canvas.
    /// </summary>
    public static Image CreateImage(string name, out CanvasGroup canvasGroup)
    {
        Image image = CreateImage(name);
        canvasGroup = image.gameObject.AddComponent<CanvasGroup>();
        return image;
    }

    public static Image CreateImage(string name)
    {
        Canvas canvas = GameObject.FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("Your screen prefab is missing required Canvas elements. Please add them to run");

            throw new System.NotImplementedException();
        }

        GameObject imageObject = new GameObject(name);
        imageObject.transform.SetParent(canvas.transform, false);
        Image image = imageObject.AddComponent<Image>();

        RectTransform rectTransform = image.rectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        return image;
    }
}