using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Popup")]
    public GameObject tutorialPopup;

    [Header("Slaytlar (5 adet)")]
    public GameObject[] slides;

    [Header("Navigasyon")]
    public Button prevButton;
    public Button nextButton;
    public Button startButton;
    public Image[] dots;

    [Header("Renkler")]
    public Color dotActiveColor = new Color(1f, 0.85f, 0f, 1f);
    public Color dotInactiveColor = new Color(0.27f, 0.27f, 0.27f, 1f);

    private int currentSlide = 0;

    private const string TutorialShownKey = "TutorialShown";

    private void Start()
    {
        if (tutorialPopup != null)
            tutorialPopup.SetActive(false);

        if (prevButton != null) prevButton.onClick.AddListener(GoPrev);
        if (nextButton != null) nextButton.onClick.AddListener(GoNext);
        if (startButton != null) startButton.onClick.AddListener(CloseTutorial);

        if (PlayerPrefs.GetInt(TutorialShownKey, 0) == 0)
            ShowTutorial();
    }

    public void ShowTutorial()
    {
        currentSlide = 0;
        if (tutorialPopup != null) tutorialPopup.SetActive(true);
        UpdateSlide();
    }

    public void CloseTutorial()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        if (tutorialPopup != null) tutorialPopup.SetActive(false);
        PlayerPrefs.SetInt(TutorialShownKey, 1);
        PlayerPrefs.Save();
    }

    public void GoNext()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        if (currentSlide < slides.Length - 1) { currentSlide++; UpdateSlide(); }
    }

    public void GoPrev()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        if (currentSlide > 0) { currentSlide--; UpdateSlide(); }
    }

    private void UpdateSlide()

        {
            Debug.Log("Slide sayısı: " + slides.Length);
            for (int i = 0; i < slides.Length; i++)
            {
                Debug.Log($"Slide {i}: {(slides[i] != null ? slides[i].name : "NULL")}");
                if (slides[i] != null) slides[i].SetActive(i == currentSlide);
            }

            {
                for (int i = 0; i < slides.Length; i++)
                    if (slides[i] != null) slides[i].SetActive(i == currentSlide);

                for (int i = 0; i < dots.Length; i++)
                    if (dots[i] != null) dots[i].color = (i == currentSlide) ? dotActiveColor : dotInactiveColor;

                if (prevButton != null) prevButton.interactable = currentSlide > 0;
                if (nextButton != null) nextButton.gameObject.SetActive(currentSlide < slides.Length - 1);
                if (startButton != null) startButton.gameObject.SetActive(currentSlide == slides.Length - 1);
        }
        }
}