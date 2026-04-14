using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestCard : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text titleText;
    public TMP_Text progressText;
    public Slider progressBar;
    public Button claimButton;
    public TMP_Text claimButtonText;
    public GameObject completedIcon;

    private QuestProgress currentQuest;
    private QuestUI questUI;

    public void Setup(QuestProgress quest, QuestUI ui)
    {
        currentQuest = quest;
        questUI = ui;

        // Başlık
        if (titleText != null)
            titleText.text = quest.Definition.Title;

        // İlerleme
        if (progressText != null)
            progressText.text = $"{quest.Current} / {quest.Definition.Target}";

        if (progressBar != null)
        {
            progressBar.minValue = 0;
            progressBar.maxValue = quest.Definition.Target;
            progressBar.value = quest.Current;
        }

        // Buton durumu
        if (claimButton != null)
        {
            claimButton.interactable = quest.IsCompleted && !quest.IsRewarded;
            claimButton.onClick.RemoveAllListeners();
            claimButton.onClick.AddListener(() => questUI.ClaimReward(currentQuest));
        }

        // Buton yazısı
        if (claimButtonText != null)
        {
            if (quest.IsRewarded)
                claimButtonText.text = "Alindi";
            else if (quest.IsCompleted)
                claimButtonText.text = $"+{quest.Definition.HintReward} Ipucu Al!";
            else
                claimButtonText.text = $"+{quest.Definition.HintReward} Ipucu";
        }

        // Tamamlandı ikonu
        if (completedIcon != null)
            completedIcon.SetActive(quest.IsRewarded);
    }
}