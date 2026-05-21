using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    [Header("UI")]
    public Image fadeImage; // full-screen image covering the screen (set to black or desired color)
    public CanvasGroup blocker; // optional: a CanvasGroup that blocks input (alpha 0, interactable false)
    public Action onSceneLoaded; // set from other scripts before calling FadeToScene

    [Header("Settings")]
    public float fadeDuration = 0.75f;
    public Color fadeColor = Color.black;

    private void Awake()
    {
        if (fadeImage)
        {
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
            fadeImage.gameObject.SetActive(true);
        }
        if (blocker)
        {
            blocker.alpha = 0f;
            blocker.blocksRaycasts = false;
            blocker.interactable = false;
        }
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(CrossFadeToScene(sceneName));
    }

    private IEnumerator CrossFadeToScene(string sceneName)
    {
        // block input
        if (blocker) { blocker.blocksRaycasts = true; blocker.interactable = true; }

        // Fade out (0 -> 1)
        yield return StartCoroutine(Fade(0f, 1f, fadeDuration / 2f));

        // Load scene asynchronously and wait until it's ready
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        // optional: wait until scene is loaded to 90% (Unity's async load semantics)
        while (op.progress < 0.9f)
            yield return null;

        // give a frame to let new scene initialize (optional)
        yield return null;

        // activate the scene
        op.allowSceneActivation = true;

        // wait until the scene activation finishes
        while (!op.isDone)
            yield return null;

        // Scene is activated and ready here — invoke callback
        onSceneLoaded?.Invoke();

        // Fade in (1 -> 0)
        yield return StartCoroutine(Fade(1f, 0f, fadeDuration / 2f));

        // unblock input
        if (blocker) { blocker.blocksRaycasts = false; blocker.interactable = false; }
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        if (fadeImage == null)
            yield break;

        float elapsed = 0f;
        Color c = fadeColor;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float a = Mathf.Lerp(startAlpha, endAlpha, t);
            fadeImage.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }
        fadeImage.color = new Color(c.r, c.g, c.b, endAlpha);
    }
}
