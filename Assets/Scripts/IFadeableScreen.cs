using System.Collections;
using UnityEngine;

public interface IFadeableScreen
{
    IEnumerator FadeIn();
    IEnumerator FadeOut();
}
