using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NumberTile : MonoBehaviour
{
    public TMP_Text valueText;
    public Image backgroundImage;
    public Button button;

    public int Value { get; private set; }

    private MathRoundManager manager;
    private bool isSelected;

    public void Setup(int value, MathRoundManager managerRef)
    {
        Value = value;
        manager = managerRef;
        isSelected = false;

        valueText.text = value.ToString();
        SetSelected(false);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClicked);
        button.interactable = true;
    }

    private void OnClicked()
    {
        if (manager != null)
        {
            manager.OnNumberSelected(this);
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (selected)
        {
            backgroundImage.color = new Color(0.95f, 0.79f, 0.36f, 1f);
            valueText.color = Color.white;
        }
        else
        {
            backgroundImage.color = Color.white;
            valueText.color = Color.white;
        }
    }

    public bool IsSelected()
    {
        return isSelected;
    }
}