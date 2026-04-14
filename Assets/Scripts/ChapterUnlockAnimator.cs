using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bölüm geçişinde kilit kırılma animasyonunu yönetir.
/// Tam → Çatlak → Kırık sprite geçişi ile kırılma efekti.
/// </summary>
public class ChapterUnlockAnimator : MonoBehaviour
{
    [Header("UI Elemanları")]
    public GameObject unlockPanel;
    public Image overlayImage;
    public Image lockImage;
    public TMP_Text unlockTitleText;
    public TMP_Text unlockSubtitleText;

    [Header("Kilit Sprite'ları")]
    public Sprite lockIntact;      // Tam kilit
    public Sprite lockCracked;     // Çatlak kilit
    public Sprite lockBroken;      // Kırık kilit

    [Header("Animasyon Ayarları")]
    public float fadeInDuration = 0.4f;
    public float holdIntactTime = 0.5f;   // Tam kilit ne kadar beklesin
    public float shakeBeforeCrack = 0.4f;   // Çatlamadan önce sallama
    public float holdCrackedTime = 0.3f;   // Çatlak kilit ne kadar beklesin
    public float holdBrokenTime = 0.4f;   // Kırık kilit ne kadar beklesin
    public float textAppearDelay = 0.2f;
    public float holdDuration = 1.5f;
    public float fadeOutDuration = 0.5f;
    public float shakeStrength = 12f;

    private System.Action onComplete;

    // ──────────────────────────────────────────
    // Başlangıç
    // ──────────────────────────────────────────

    private void Awake()
    {
        if (unlockPanel != null)
            unlockPanel.SetActive(false);
    }

    // ──────────────────────────────────────────
    // Ana Giriş
    // ──────────────────────────────────────────

    public void PlayUnlockAnimation(int nextChapter, System.Action onComplete)
    {
        Debug.Log("ChapterUnlockAnimator tetiklendi! Bölüm: " + nextChapter);

        this.onComplete = onComplete;

        if (unlockTitleText != null)
            unlockTitleText.text = $"Bölüm {nextChapter} Açıldı!";

        if (unlockSubtitleText != null)
            unlockSubtitleText.text = "Yeni zorluklar seni bekliyor!";

        StartCoroutine(AnimateSequence());
    }

    // ──────────────────────────────────────────
    // Animasyon Sekansı
    // ──────────────────────────────────────────

    private IEnumerator AnimateSequence()
    {
        if (unlockPanel != null)
            unlockPanel.SetActive(true);

        // Başlangıç değerleri
        SetOverlayAlpha(0f);
        SetTextAlpha(unlockTitleText, 0f);
        SetTextAlpha(unlockSubtitleText, 0f);

        if (lockImage != null)
        {
            lockImage.gameObject.SetActive(true);
            lockImage.transform.localScale = Vector3.one;
            lockImage.transform.localRotation = Quaternion.identity;
            SetImageAlpha(lockImage, 0f);

            // Başlangıçta tam kilit
            if (lockIntact != null)
                lockImage.sprite = lockIntact;
        }

        // 1. Overlay fade in
        yield return StartCoroutine(FadeOverlay(0f, 0.85f, fadeInDuration));

        // 2. Kilit belir
        yield return StartCoroutine(LockAppear());

        // 3. Tam kilit bekle
        yield return new WaitForSeconds(holdIntactTime);

        // 4. Sallan (kırılmadan önce)
        yield return StartCoroutine(ShakeLock(shakeBeforeCrack));

        // 5. Çatlak sprite'a geç
        if (lockImage != null && lockCracked != null)
        {
            lockImage.sprite = lockCracked;
            yield return StartCoroutine(PunchScale(1.15f, 0.1f));
        }

        yield return new WaitForSeconds(holdCrackedTime);

        // 6. Tekrar sallan
        yield return StartCoroutine(ShakeLock(0.2f));

        // 7. Kırık sprite'a geç + büyüyüp solar
        if (lockImage != null && lockBroken != null)
        {
            lockImage.sprite = lockBroken;
            yield return StartCoroutine(BreakLock());
        }
        else
        {
            yield return StartCoroutine(BreakLock());
        }

        // 8. Yazı belirir
        yield return new WaitForSeconds(textAppearDelay);
        yield return StartCoroutine(ShowText());

        // 9. Bekle
        yield return new WaitForSeconds(holdDuration);

        // 10. Fade out
        yield return StartCoroutine(FadeOverlay(0.85f, 0f, fadeOutDuration));

        // 11. Panel kapat
        if (unlockPanel != null)
            unlockPanel.SetActive(false);

        // 12. Callback
        onComplete?.Invoke();
    }

    // ──────────────────────────────────────────
    // Kilit Belirme
    // ──────────────────────────────────────────

    private IEnumerator LockAppear()
    {
        if (lockImage == null) yield break;

        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float scale = EaseOutBack(t);
            lockImage.transform.localScale = Vector3.one * scale;
            SetImageAlpha(lockImage, t);
            yield return null;
        }

        lockImage.transform.localScale = Vector3.one;
        SetImageAlpha(lockImage, 1f);
    }

    // ──────────────────────────────────────────
    // Sallama
    // ──────────────────────────────────────────

    private IEnumerator ShakeLock(float duration)
    {
        if (lockImage == null) yield break;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float currentStrength = Mathf.Lerp(shakeStrength, 0f, t);
            float angle = Mathf.Sin(elapsed * 25f) * currentStrength;
            lockImage.transform.localRotation = Quaternion.Euler(0f, 0f, angle);

            yield return null;
        }

        lockImage.transform.localRotation = Quaternion.identity;
    }

    // ──────────────────────────────────────────
    // Punch Scale (çatlama hissi)
    // ──────────────────────────────────────────

    private IEnumerator PunchScale(float targetScale, float duration)
    {
        if (lockImage == null) yield break;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float scale = t < 0.5f
                ? Mathf.Lerp(1f, targetScale, t / 0.5f)
                : Mathf.Lerp(targetScale, 1f, (t - 0.5f) / 0.5f);
            lockImage.transform.localScale = Vector3.one * scale;
            yield return null;
        }

        lockImage.transform.localScale = Vector3.one;
    }

    // ──────────────────────────────────────────
    // Kilit Kırılma (büyüyüp sola)
    // ──────────────────────────────────────────

    private IEnumerator BreakLock()
    {
        if (lockImage == null) yield break;

        float elapsed = 0f;
        float duration = 0.4f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float scale = Mathf.Lerp(1f, 1.8f, t);
            float alpha = Mathf.Lerp(1f, 0f, t);

            lockImage.transform.localScale = Vector3.one * scale;
            SetImageAlpha(lockImage, alpha);

            yield return null;
        }

        lockImage.gameObject.SetActive(false);
    }

    // ──────────────────────────────────────────
    // Yazı Belirme
    // ──────────────────────────────────────────

    private IEnumerator ShowText()
    {
        float elapsed = 0f;
        float duration = 0.4f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetTextAlpha(unlockTitleText, t);
            SetTextAlpha(unlockSubtitleText, t * 0.8f);
            yield return null;
        }

        SetTextAlpha(unlockTitleText, 1f);
        SetTextAlpha(unlockSubtitleText, 0.8f);
    }

    // ──────────────────────────────────────────
    // Overlay Fade
    // ──────────────────────────────────────────

    private IEnumerator FadeOverlay(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetOverlayAlpha(Mathf.Lerp(from, to, t));
            yield return null;
        }
        SetOverlayAlpha(to);
    }

    // ──────────────────────────────────────────
    // Yardımcılar
    // ──────────────────────────────────────────

    private void SetOverlayAlpha(float alpha)
    {
        if (overlayImage == null) return;
        Color c = overlayImage.color;
        c.a = alpha;
        overlayImage.color = c;
    }

    private void SetImageAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    private void SetTextAlpha(TMP_Text text, float alpha)
    {
        if (text == null) return;
        Color c = text.color;
        c.a = alpha;
        text.color = c;
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}