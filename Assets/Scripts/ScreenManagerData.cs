using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Stores the data for the ScreenManager.
/// </summary>
[CreateAssetMenu(fileName = "ScreenManagerData", menuName = "Screen Data/Screen Manager Data")]
public class ScreenManagerData : ScriptableObject
{
    [Tooltip("Duration (in seconds) of the screen transitions")]
    public float screenTransitionDuration = .5f;

    [Tooltip("Time (in seconds) before returning to the attract screen due to inactivity.")]
    public float inactivityTimeout = 60f;
}
