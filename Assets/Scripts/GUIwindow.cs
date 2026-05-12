using UnityEngine;

public class GUIwindow : MonoBehaviour
{

    public Transform cameraTransform; // Reference to the camera's transform
    public Rect windowRect = new Rect(20, 20, 250, 150);
    private bool showWindow = true;

    void OnGUI()
    {
        if (showWindow)
        {
            windowRect = GUI.Window(0, windowRect, DoMyWindow, "GUI Window");
        }
    }

    void DoMyWindow(int windowID)
    {
        if (GUI.Button(new Rect(10, 20, 100, 30), "Close"))
        {
            showWindow = false; // Close the window
        }

        // Add more GUI elements here
    }
}