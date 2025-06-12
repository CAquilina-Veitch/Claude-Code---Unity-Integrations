using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChangeColorButton : MonoBehaviour
{
    private TextMeshProUGUI textComponent;
    private Button buttonComponent;

    void Start()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        buttonComponent = GetComponent<Button>();
        
        if (buttonComponent != null)
        {
            buttonComponent.onClick.AddListener(ChangeToRandomColor);
        }
    }

    void ChangeToRandomColor()
    {
        if (textComponent != null)
        {
            Color randomColor = new Color(
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                Random.Range(0f, 1f)
            );
            
            textComponent.color = randomColor;
        }
    }

    void OnDestroy()
    {
        if (buttonComponent != null)
        {
            buttonComponent.onClick.RemoveListener(ChangeToRandomColor);
        }
    }
}