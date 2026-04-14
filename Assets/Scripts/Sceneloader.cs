using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    [Header("Fade Panel")]
    public Image fadePanel;

    [Header("Ayarlar")]
    public float fadeDuration = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(false);
            fadePanel.color = new Color(0, 0, 0, 0);
        }
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
        yield return StartCoroutine(Fade(0f, 1f));

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        op.allowSceneActivation = true;
        yield return null;

        yield return StartCoroutine(Fade(1f, 0f));
    }

    private IEnumerator LoadSceneAsyncByIndex(int buildIndex)
    {
        yield return StartCoroutine(Fade(0f, 1f));

        AsyncOperation op = SceneManager.LoadSceneAsync(buildIndex);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        op.allowSceneActivation = true;
        yield return null;

        yield return StartCoroutine(Fade(0f, 0f));
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