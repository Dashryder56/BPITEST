using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Stores button information and associated images for ContentScreen.
/// </summary>
[CreateAssetMenu(fileName = "ContentScreenData", menuName = "Screen Data/Content Screen Data")]
public class ContentScreenData : ScriptableObject
{
    [Tooltip("Time (in seconds) of inactivity before auto closing the modal.")]
    public float autoCloseModalTime = 30f;

    [System.Serializable]
    public class GalleryItem
    {
        public string image;    
        public string description;
    }

    [System.Serializable]
    public class ButtonData
    {
        public string buttonName;  
        public List<GalleryItem> gallery; 
    }

    public List<ButtonData> buttons;
}
