using UnityEngine;
using UnityEngine.UI;

public class SliderUIOpacity : MonoBehaviour
{
    public Slider opacitySlider;
    public CanvasGroup canvasGroup;

    void Start()
    {
        opacitySlider.onValueChanged.AddListener(UpdateCanvasGroupOpacity);
    }

    void UpdateCanvasGroupOpacity(float value)
    {
        canvasGroup.alpha = value; // value should be between 0 and 1
    }
}