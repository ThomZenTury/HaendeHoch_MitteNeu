using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class SceneTransition : MonoBehaviour
{


    [Header("UI")]
    public Image fadeImage; // full-screen image covering the screen (set to black or desired color)
    public CanvasGroup blocker; // optional: a CanvasGroup that blocks input (alpha 0, interactable false)
    public Action onSceneLoaded; // set from other scripts before calling FadeToScene

    [Header("Settings")]
    public float fadeDuration = 0.75f;
    public Color fadeColor = Color.black;

    [SerializeField] public Material videoMaterial;       // assign BG video material in Inspector
                                                          // declare a static alpha value for BG video that can be accessed by other SCENES
    public GameObject BGVideoPanel;
    public static float bgVideoAlpha = 0f; // declare a static alpha value for BG video that can be accessed by other SCENES
    public static String bgVideoTitle = ""; // declare a static alpha value for BG video that can be accessed by other SCENES
    private VideoPlayer BGvideoPlayer;

    private void Awake()
    {

        // set the BG video alpha to last known value (e.g. from OSC) when the scene starts
        if (videoMaterial)
        {
            videoMaterial.SetFloat("_AlphaRemapMax", bgVideoAlpha); // need to add extOSC. infront because it uses a namespace extOSC {} in the script
        }
        // set video clip title to last known value (e.g. from OSC) when the scene starts
        BGvideoPlayer = BGVideoPanel.GetComponentInChildren<VideoPlayer>();
        VideoClip clip = Resources.Load<VideoClip>("Videos/" + bgVideoTitle); // no extension 

        if (clip != null && BGvideoPlayer != null)
        {
            BGvideoPlayer.source = UnityEngine.Video.VideoSource.VideoClip;
            BGvideoPlayer.clip = clip;
            BGvideoPlayer.Play();
        }
        else Debug.Log("Failed to load or player is null: " + bgVideoTitle);

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
        // used e.g. in UIKeyboard.cs to load PlayerPrefs and set Video Alpha to 0 after the new scene is loaded
        onSceneLoaded?.Invoke();
        Debug.Log("before fade in");
        // Fade in (1 -> 0)
        yield return StartCoroutine(Fade(1f, 0f, fadeDuration * 2f / 2f));
        Debug.Log("after fade in");
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
            // if (endAlpha == 0f) bgVideoAlpha = a; // update the static alpha value for BG video during fade (optional, depending on desired effect)

            yield return null;
        }
        fadeImage.color = new Color(c.r, c.g, c.b, endAlpha);
        // if (endAlpha == 0f) bgVideoAlpha = endAlpha; // ensure static alpha value is set to final value at the end of fade

    }
}
