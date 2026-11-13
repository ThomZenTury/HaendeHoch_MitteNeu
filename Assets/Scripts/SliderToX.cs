using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SliderToX : MonoBehaviour
{
    [SerializeField] public Slider slider;        // assign in Inspector
    [SerializeField] private TextMeshProUGUI text;        // assign in Inspector
    [SerializeField] private TMP_InputField xIn;        // assign in Inspector
    [SerializeField] private Transform target;     // assign target transform
    [SerializeField] private Transform planetary;     // assign planetary for left set
    [SerializeField] private Transform triangleLeft;     // assign triangle
    [SerializeField] private Transform triangleRight;     // assign triangle
    [SerializeField] private Toggle fine;     // assign target transform
    [SerializeField] private Toggle isLocked;
    [SerializeField] private float minX = -2.0f;      // slider min mapped X
    [SerializeField] private float maxX = 2.0f;      // slider max mapped X
    [SerializeField] private float initialX = 0.1f;      // slider max mapped X

    [SerializeField]
    private int leftKeyVar = (int)Key.A; // Key to decrease slider value key.A=15, key.J=24
    [SerializeField]
    private int rightKeyVar = (int)Key.D; // Key to increase slider value key.D=18, key.L=26
    [SerializeField]
    private int lockKeyVar = (int)Key.F1; // Key to toggle lock state key.F1=94, key.F2=95


    public float step = 0.001f;

    void Start()
    {
        // Set the initial value of the slider
        slider.value = initialX; // Example starting value
        // slider.onValueChanged.AddListener(OnSliderChanged);

        // Initialize the input field with the slider's value
        xIn.text = slider.value.ToString();

        // Debug.Log("L_ctrl" + (int)Key.LeftCtrl);
        // Debug.Log("L_alt" + (int)Key.LeftAlt);
        // Debug.Log("R_alt" + (int)Key.RightAlt);
        // Debug.Log("R_ctrl" + (int)Key.RightCtrl);
        // Debug.Log("Home" + (int)Key.Home);
        // Debug.Log("PageUp" + (int)Key.PageUp);
        // Debug.Log("PageDown" + (int)Key.PageDown);
        // Debug.Log("A" + (int)Key.A);
        // Debug.Log("S" + (int)Key.S);
        // Debug.Log("W" + (int)Key.W);
        // Debug.Log("D" + (int)Key.D);
        // Debug.Log("I" + (int)Key.I);
        // Debug.Log("J" + (int)Key.J);
        // Debug.Log("K" + (int)Key.K);
        // Debug.Log("L" + (int)Key.L);
        // Debug.Log("F2" + (int)Key.F2);



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

        if (xIn != null) xIn.onEndEdit.AddListener(OnInputChanged);
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

            float x = Mathf.Lerp(minX, maxX, t); // assumes slider.value is 0..1
            Vector3 pos = target.position;
            pos.x = x;
            target.position = pos;

            if (planetary != null)
            {
                Vector3 planetaryPos = planetary.position;
                planetaryPos.x = x;
                planetary.position = planetaryPos;
            }

            if (triangleLeft != null)
            {
                // move the Procedural triangle as well
                Vector3 triangleLeftPos = triangleLeft.position;
                triangleLeftPos.x = x + 0.2265f; //(the measured offset of the triangle)
                triangleLeft.position = triangleLeftPos;
            }
            if (triangleRight != null)
            {
                // move the Procedural triangle as well
                Vector3 triangleRightPos = triangleRight.position;
                triangleRightPos.x = x - 0.2265f; //(the measured offset of the triangle)
                triangleRight.position = triangleRightPos;
            }



            text.text = $"X: {x:F4}"; // Update text with current X value, formatted to 2 decimal places
            xIn.text = $"{x:F4}"; // Update text with current X value, formatted to 2 decimal places
        }
    }

    void Update()
    {

        if (isLocked.isOn)
        {
            slider.interactable = false;

            // Toggle lock state on key press
            if (Keyboard.current[(Key)lockKeyVar].wasPressedThisFrame)
            {
                isLocked.isOn = !isLocked.isOn;
            }


        }
        else
        {
            slider.interactable = true;

            // Toggle lock state on key press
            if (Keyboard.current[(Key)lockKeyVar].wasPressedThisFrame)
            {
                isLocked.isOn = !isLocked.isOn;
            }

            // fine mode changes step size for keyboard input
            if (fine.isOn) step = 0.0001f; else step = 0.001f;

            // move slider with keyboard input
            if (Keyboard.current[(Key)rightKeyVar].isPressed)
            {
                slider.value += step; // Increment slider value by step
            }
            if (Keyboard.current[(Key)leftKeyVar].isPressed)
            {
                slider.value -= step; // Decrement slider value by step
            }
        }
    }
}