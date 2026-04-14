using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultsAnimator : MonoBehaviour
{
    [Header("Kart Animasyonu")]
    public RectTransform resultsCard;
    public float cardSlideDistance = 300f;
    public float cardSlideDuration = 0.5f;

    [Header("Puan Sayıcı")]
    public TMP_Text finalScoreText;
    public float scoreCountDuration = 1.5f;

    [Header("Yıldızlar")]
    public Image[] starImages;
    public Sprite filledStarSprite;      // 1-2 yıldız için
    public Sprite perfectStarSprite;     // 3 yıldız için özel görsel
    public Sprite emptyStarSprite;
    public float starPopDelay = 0.3f;
    public float starPopDuration = 0.25f;
    public float starPopScale = 1.4f;

    [Header("3 Yıldız Efektleri")]
    public float pulseScale = 1.15f;   // Pulse büyüklüğü
    public float pulseDuration = 0.6f;    // Pulse süresi
    public float rotationSpeed = 90f;     // Derece/saniye

    [Header("Konfeti")]
    public GameObject confettiPrefab;
    public int confettiCountNormal = 20;  // 1-2 yıldız
    public int confettiCountPerfect = 50;  // 3 yıldız

    private int earnedStars = 0;
    private Coroutine[] pulseCoroutines;

    // ──────────────────────────────────────────
    // Ana Giriş
    // ──────────────────────────────────────────

    public void PlayResultsAnimation(int totalScore, int stars)
    {
        earnedStars = stars;
        StopAllCoroutines();

        // Pulse coroutine'leri durdur
        if (pulseCoroutines != null)
            foreach (var c in pulseCoroutines)
                if (c != null) StopCoroutine(c);

        // Yıldızları sıfırla
        if (starImages != null)
        {
            foreach (var img in starImages)
            {
                if (img == null) continue;
                img.sprite = emptyStarSprite;
                img.transform.localScale = Vector3.zero;
                img.transform.rotation = Quaternion.identity;
            }
        }

        StartCoroutine(AnimateSequence(totalScore, stars));
    }

    private IEnumerator AnimateSequence(int totalScore, int stars)
    {
        // 1. Kart yukarı süz
        if (resultsCard != null)
        {
            Vector2 originalPos = resultsCard.anchoredPosition;
            Vector2 startPos = originalPos + Vector2.down * cardSlideDistance;
            resultsCard.anchoredPosition = startPos;
            yield return StartCoroutine(SlideCard(startPos, originalPos, cardSlideDuration));
        }

        yield return new WaitForSeconds(0.2f);

        // 2. Puan sayıcı
        if (finalScoreText != null)
            yield return StartCoroutine(CountScore(totalScore, scoreCountDuration));

        yield return new WaitForSeconds(0.1f);

        // 3. Yıldızlar
        yield return StartCoroutine(AnimateStars(stars));

        // 4. 3 yıldız efektleri
        if (stars >= 3)
        {
            StartCoroutine(PulseAllStars());
            StartCoroutine(RotateAllStars());
            SpawnConfetti(confettiCountPerfect);
        }
        else if (stars > 0)
        {
            SpawnConfetti(confettiCountNormal);
        }
    }

    // ──────────────────────────────────────────
    // Kart Animasyonu
    // ──────────────────────────────────────────

    private IEnumerator SlideCard(Vector2 from, Vector2 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            resultsCard.anchoredPosition = Vector2.Lerp(from, to, EaseOutBack(t));
            yield return null;
        }
        resultsCard.anchoredPosition = to;
    }

    // ──────────────────────────────────────────
    // Puan Sayıcı
    // ──────────────────────────────────────────

    private IEnumerator CountScore(int targetScore, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            int current = Mathf.RoundToInt(Mathf.Lerp(0, targetScore, t));
            finalScoreText.text = "Toplam Puan: " + current;
            yield return null;
        }
        finalScoreText.text = "Toplam Puan: " + targetScore;
    }

    // ──────────────────────────────────────────
    // Yıldız Animasyonu
    // ──────────────────────────────────────────

    private IEnumerator AnimateStars(int stars)
    {
        if (starImages == null) yield break;

        // 3 yıldızda perfectStarSprite, değilse filledStarSprite
        Sprite starToUse = (stars >= 3 && perfectStarSprite != null)
            ? perfectStarSprite
            : filledStarSprite;

        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null) continue;

            bool isFilled = i < stars;
            starImages[i].sprite = isFilled ? starToUse : emptyStarSprite;

            yield return StartCoroutine(PopStar(starImages[i].transform));
            yield return new WaitForSeconds(starPopDelay);
        }
    }

    private IEnumerator PopStar(Transform star)
    {
        float elapsed = 0f;
        while (elapsed < starPopDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / starPopDuration);
            float scale = t < 0.6f
                ? Mathf.Lerp(0f, starPopScale, t / 0.6f)
                : Mathf.Lerp(starPopScale, 1f, (t - 0.6f) / 0.4f);
            star.localScale = Vector3.one * scale;
            yield return null;
        }
        star.localScale = Vector3.one;
    }

    // ──────────────────────────────────────────
    // 3 Yıldız — Pulse Efekti
    // ──────────────────────────────────────────

    private IEnumerator PulseAllStars()
    {
        if (starImages == null) yield break;

        while (true) // Sürekli pulse
        {
            float elapsed = 0f;
            while (elapsed < pulseDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.PingPong(elapsed / pulseDuration, 1f);
                float scale = Mathf.Lerp(1f, pulseScale, t);

                foreach (var img in starImages)
                    if (img != null)
                        img.transform.localScale = Vector3.one * scale;

                yield return null;
            }
        }
    }

    // ──────────────────────────────────────────
    // 3 Yıldız — Döndürme Efekti
    // ──────────────────────────────────────────

    private IEnumerator RotateAllStars()
    {
        if (starImages == null) yield break;

        while (true)
        {
            foreach (var img in starImages)
            {
                if (img != null)
                    img.transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
            }
            yield return null;
        }
    }

    // ──────────────────────────────────────────
    // Konfeti
    // ──────────────────────────────────────────

    private void SpawnConfetti(int count)
    {
        if (confettiPrefab == null) return;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        for (int i = 0; i < count; i++)
        {
            GameObject confetti = Instantiate(confettiPrefab, transform);
            RectTransform rt = confetti.GetComponent<RectTransform>();

            if (rt != null)
            {
                float randomX = Random.Range(-canvasRect.rect.width / 2f, canvasRect.rect.width / 2f);
                rt.anchoredPosition = new Vector2(randomX, canvasRect.rect.height / 2f + 50f);
            }

            Image img = confetti.GetComponent<Image>();
            if (img != null)
                img.color = Random.ColorHSV(0f, 1f, 0.8f, 1f, 0.8f, 1f);

            StartCoroutine(FallConfetti(confetti));
        }
    }

    private IEnumerator FallConfetti(GameObject confetti)
    {
        if (confetti == null) yield break;

        RectTransform rt = confetti.GetComponent<RectTransform>();
        float speed = Random.Range(400f, 900f);
        float rotSpeed = Random.Range(-180f, 180f);
        float horizontalDrift = Random.Range(-50f, 50f);
        float duration = Random.Range(1.5f, 2.5f);
        float elapsed = 0f;

        while (elapsed < duration && confetti != null)
        {
            elapsed += Time.deltaTime;
            rt.anchoredPosition += new Vector2(horizontalDrift * Time.deltaTime, -speed * Time.deltaTime);
            rt.Rotate(0f, 0f, rotSpeed * Time.deltaTime);
            yield return null;
        }

        if (confetti != null)
            Destroy(confetti);
    }

    // ──────────────────────────────────────────
    // Easing
    // ──────────────────────────────────────────

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}