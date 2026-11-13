#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class BorderlessGameView
{
    private static EditorWindow gameView;

    static BorderlessGameView()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    [MenuItem("Tools/Borderless Game View _F11")]
    private static void Toggle()
    {
        if (gameView != null)
        {
            CloseGameView();
            return;
        }

        if (!EditorApplication.isPlaying)
        {
            Debug.LogWarning("Enter Play mode first, then press F11.");
            return;
        }

        Type gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
        if (gameViewType == null)
        {
            Debug.LogError("Could not find Unity's GameView type.");
            return;
        }

        gameView = (EditorWindow)ScriptableObject.CreateInstance(gameViewType);

        // Hide the Game view toolbar, if this Unity version exposes the property.
        PropertyInfo toolbarProperty = gameViewType.GetProperty(
            "showToolbar",
            BindingFlags.Instance | BindingFlags.NonPublic);

        toolbarProperty?.SetValue(gameView, false);

        Resolution resolution = Screen.currentResolution;
        float scale = EditorGUIUtility.pixelsPerPoint;

        gameView.ShowPopup();
        gameView.position = new Rect(
            0,
            0,
            resolution.width / scale,
            resolution.height / scale);

        gameView.Focus();
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
            CloseGameView();
    }

    private static void CloseGameView()
    {
        if (gameView != null)
            gameView.Close();

        gameView = null;
    }
}
#endif
