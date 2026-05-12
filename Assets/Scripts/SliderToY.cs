using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SliderToY : MonoBehaviour
{
    [SerializeField] private Slider slider;        // assign in Inspector
    [SerializeField] private TextMeshProUGUI text;        // assign in Inspector
    [SerializeField] private TMP_InputField yIn;        // assign in Inspector
    [SerializeField] private Transform target;     // assign target transform
    [SerializeField] private Toggle fine;
    [SerializeField] private Toggle isLocked;
    [SerializeField] private float minY = 1.0f;      // slider min mapped Y
    [SerializeField] private float maxY = 2.0f;      // slider max mapped Y
    [SerializeField] private float initialY = 1.5f;      // slider max mapped Y
    [SerializeField]
    private int upKeyVar = (int)Key.W; // Key to increase slider value , key.W=37, key.I=23
    [SerializeField]
    private int downKeyVar = (int)Key.S; // Key to decrease slider value , key.S=33, key.K=25

    private float step = 0.001f;

    void Start()
    {
        // Set the initial value of the slider
        slider.value = initialY; // Example starting value
        // slider.onValueChanged.AddListener(OnSliderChanged);

        // Initialize the input field with the slider's value
        yIn.text = slider.value.ToString();
    }

    private void Reset()
    {
        slider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        if (slider != null) slider.onValueChanged.AddListener(OnSliderChanged);
        // initialize position
        OnSliderChanged(slider != null ? slider.value : 0f);

        if (yIn != null) yIn.onEndEdit.AddListener(OnInputChanged);
    }

    private void OnDisable()
    {
        if (slider != null) slider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void OnInputChanged(string text)
    {
        if (float.TryParse(text, out float value))
        {
            slider.value = value;
        }
    }

    private void OnSliderChanged(float value)
    {
        if (isLocked.isOn)
        {
            slider.interactable = false;
            return;
        }
        else
        {
            slider.interactable = true;

            if (target == null || slider == null) return;

            float range = slider.maxValue - slider.minValue;
            float t = range == 0f ? 0f : (value - slider.minValue) / range;
            t = Mathf.Clamp01(t);

            float y = Mathf.Lerp(minY, maxY, t); // assumes slider.value is 0..1
            Vector3 pos = target.position;
            pos.y = y;
            target.position = pos;

            text.text = $"Y: {y:F4}"; // Update text with current Y value, formatted to 2 decimal places
            yIn.text = $"{y:F4}"; // Update text with current Y value, formatted to 2 decimal places
        }
    }

    void Update()
    {
        if (isLocked.isOn)
        {
            slider.interactable = false;

        }
        else
        {
            slider.interactable = true;

            if (fine.isOn) step = 0.0001f; else step = 0.001f;

            if (Keyboard.current[(Key)upKeyVar].isPressed)
            {
                slider.value += step; // Increment slider value by step
            }
            if (Keyboard.current[(Key)downKeyVar].isPressed)
            {
                slider.value -= step; // Decrement slider value by step
            }
        }
    }
}