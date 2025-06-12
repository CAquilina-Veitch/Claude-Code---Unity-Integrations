using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class TextHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float hoverHeight = 20f;
    public float bounceSpeed = 10f;
    public float bounceDuration = 0.3f;
    
    private RectTransform rectTransform;
    private Vector3 originalPosition;
    private bool isAnimating = false;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isAnimating)
        {
            StartCoroutine(BounceEffect());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
    }

    private IEnumerator BounceEffect()
    {
        isAnimating = true;
        
        float elapsedTime = 0f;
        Vector3 startPos = originalPosition;
        Vector3 peakPos = originalPosition + Vector3.up * hoverHeight;
        
        while (elapsedTime < bounceDuration)
        {
            float t = elapsedTime / bounceDuration;
            float bounceValue = Mathf.Sin(t * Mathf.PI);
            
            rectTransform.anchoredPosition = Vector3.Lerp(startPos, peakPos, bounceValue);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        rectTransform.anchoredPosition = originalPosition;
        isAnimating = false;
    }
}