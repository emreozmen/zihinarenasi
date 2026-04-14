using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BootManager : MonoBehaviour
{
    [Header("UI")]
    public Image loadingBarFill;
    public TMP_Text loadingText;
    public CanvasGroup canvasGroup;

    [Header("Ayarlar")]
    public string nextScene = "MainMenu";
    public float fakeLoadDuration = 3f;
    public float fadeDuration = 0.6f;

    // ──────────────────────────────────────────
    // Başlangıç
    // ──────────────────────────────────────────

    private void Start()
    {
        if (loadingBarFill != null)
            loadingBarFill.fillAmount = 0f;

        if (loadingText != null)
            loadingText.text = "Yükleniyor";

        StartCoroutine(LoadSequence());
    }

    // ──────────────────────────────────────────
    // Ana Sekans
    // ──────────────────────────────────────────

    private IEnumerator LoadSequence()
    {
        // 1. Fake yükleme: 0 → %85
        yield return StartCoroutine(FakeLoad());

        // 2. Gerçek async yükleme: %85 → %100
        yield return StartCoroutine(RealLoad());

        // 3. Fade out
        yield return StartCoroutine(FadeOut());

        // 4. Sahneye geç
        SceneManager.LoadScene(nextScene);
    }

    // ──────────────────────────────────────────
    // Fake Yükleme (0 → 0.85)
    // ──────────────────────────────────────────

    private IEnumerator FakeLoad()
    {
        float elapsed = 0f;
        float targetFill = 0.85f;

        string[] dots = { "Yükleniyor", "Yükleniyor.", "Yükleniyor..", "Yükleniyor..." };
        int dotIndex = 0;
        float dotTimer = 0f;
        float dotInterval = 0.4f;

        while (elapsed < fakeLoadDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fakeLoadDuration);

            // Bar
            if (loadingBarFill != null)
                loadingBarFill.fillAmount = Mathf.SmoothStep(0f, targetFill, t);

            // Hareketli nokta animasyonu
            dotTimer += Time.deltaTime;
            if (dotTimer >= dotInterval)
            {
                dotTimer = 0f;
                dotIndex = (dotIndex + 1) % dots.Length;
                if (loadingText != null)
                    loadingText.text = dots[dotIndex];
            }

            yield return null;
        }

        if (loadingBarFill != null)
            loadingBarFill.fillAmount = targetFill;
    }

    // ──────────────────────────────────────────
    // Gerçek Yükleme (0.85 → 1.0)
    // ──────────────────────────────────────────

    private IEnumerator RealLoad()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        string[] dots = { "Yükleniyor", "Yükleniyor.", "Yükleniyor..", "Yükleniyor..." };
        int dotIndex = 0;
        float dotTimer = 0f;
        float dotInterval = 0.4f;

        while (op.progress < 0.9f)
        {
            float mappedProgress = Mathf.Lerp(0.85f, 1f, op.progress / 0.9f);

            if (loadingBarFill != null)
                loadingBarFill.fillAmount = mappedProgress;

            dotTimer += Time.deltaTime;
            if (dotTimer >= dotInterval)
            {
                dotTimer = 0f;
                dotIndex = (dotIndex + 1) % dots.Length;
                if (loadingText != null)
                    loadingText.text = dots[dotIndex];
            }

            yield return null;
        }

        // %100
        if (loadingBarFill != null)
            loadingBarFill.fillAmount = 1f;

        if (loadingText != null)
            loadingText.text = "Hazır!";

        yield return new WaitForSeconds(0.3f);
    }

    // ──────────────────────────────────────────
    // Fade Out
    // ──────────────────────────────────────────

    private IEnumerator FadeOut()
    {
        if (canvasGroup == null) yield break;

        float elapsed = 0f;
        canvasGroup.alpha = 1f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }
}