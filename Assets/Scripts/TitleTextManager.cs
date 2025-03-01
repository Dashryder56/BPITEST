using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the creation and updating of a centered title text field with an outline.
/// </summary>
public class TitleTextManager
{
    private Text titleTextField;
    private string titleText;

    /// <summary>
    /// Creates a centered title text field.
    /// </summary>
    public Text CreateTitleText(string name, string initialText, Transform transform)
    {
        titleText = initialText;

        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(GameObject.FindFirstObjectByType<Canvas>().transform, false);
        textObject.transform.SetParent(transform, false);

        // Create the text and assign any user-defined changes in the inspector
        Text text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 80;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.black;
        text.text = titleText;

        // Size and center the text
        RectTransform rectTransform = text.rectTransform;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(1200, 200);

        // Add outline effect to make the text pop
        Outline outline = textObject.AddComponent<Outline>();
        outline.effectColor = Color.white;
        outline.effectDistance = new Vector2(2, -2);

        titleTextField = text;
        return text;
    }

    /// <summary>
    /// Updates the title text dynamically.
    /// </summary>
    public void UpdateTitleText(string newText)
    {
        titleText = newText;

        if (titleTextField != null)
        {
            titleTextField.text = newText;
        }
    }
}
