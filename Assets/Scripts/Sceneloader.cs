using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    [Header("Ayarlar")]
    public float fadeDuration = 0.3f;

    private Image fadePanel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreateFadePanel();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void CreateFadePanel()
    {
        var canvasGO = new GameObject("FadeCanvas");
        canvasGO.transform.SetParent(transform);

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        var panelGO = new GameObject("FadePanel");
        panelGO.transform.SetParent(canvasGO.transform, false);

        fadePanel = panelGO.AddComponent<Image>();
        fadePanel.color = new Color(0, 0, 0, 0);
        fadePanel.raycastTarget = false;

        var rect = panelGO.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        panelGO.SetActive(false);
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    public void LoadScene(int buildIndex)
    {
        StartCoroutine(LoadSceneAsyncByIndex(buildIndex));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        yield return StartCoroutine(Fade(0f, 1f));
        op.allowSceneActivation = true;
        yield return null;
        yield return StartCoroutine(Fade(1f, 0f));
    }

    private IEnumerator LoadSceneAsyncByIndex(int buildIndex)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(buildIndex);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        yield return StartCoroutine(Fade(0f, 1f));
        op.allowSceneActivation = true;
        yield return null;
        yield return StartCoroutine(Fade(1f, 0f));
    }

    private IEnumerator Fade(float from, float to)
    {
        if (fadePanel == null) yield break;

        float elapsed = 0f;
        fadePanel.gameObject.SetActive(true);
        fadePanel.color = new Color(0, 0, 0, from);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            fadePanel.color = new Color(0, 0, 0, Mathf.Lerp(from, to, t));
            yield return null;
        }

        fadePanel.color = new Color(0, 0, 0, to);

        if (to <= 0f)
            fadePanel.gameObject.SetActive(false);
    }
}