using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// /// Stores the data for the AttractScreen.
/// </summary>
[CreateAssetMenu(fileName = "AttractScreenData", menuName = "Screen Data/Attract Screen Data")]
public class AttractScreenData : ScriptableObject
{
    [Tooltip("List of image file names to load for the slideshow.")]
    public List<string> imageFileNames;

    [Tooltip("Title text displayed on the AttractScreen.")]
    public string titleText = "Tap Screen to Begin";

    [Tooltip("Time (in seconds) before switching to the next image.")]
    public float slideshowInterval = 3f;

    [Tooltip("Duration (in seconds) of the fade transition between images.")]
    public float fadeDuration = 1f;
}
