using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuickUIFix : MonoBehaviour
{
    void Start()
    {
        Invoke("FixUILayout", 0.1f);
    }
    
    void FixUILayout()
    {
        Debug.Log("Fixing UI Layout...");
        
        // Fix Health Text
        FixTextPosition("Health_Text", new Vector2(20, -20), new Vector2(200, 30));
        
        // Fix Money Text
        FixTextPosition("Money_Text", new Vector2(20, -60), new Vector2(200, 30));
        
        // Fix Score Text
        FixTextPosition("Score_Text", new Vector2(20, -100), new Vector2(200, 30));
        
        // Fix Wave Text
        FixTextPosition("Wave_Info_Text", new Vector2(20, -140), new Vector2(200, 30));
        
        // Fix Tower Panel (right side)
        FixPanelPosition("Tower_Panel", new Vector2(-200, 0), new Vector2(200, 600));
        
        // Fix HUD Panel (top)
        FixPanelPosition("HUD_Panel", new Vector2(0, -30), new Vector2(0, 60));
        
        // Fix Tower Buttons (right side layout)
        FixButtonPosition("Basic_Tower_Button", new Vector2(-180, -100), new Vector2(80, 60));
        FixButtonPosition("Cannon_Tower_Button", new Vector2(-90, -100), new Vector2(80, 60));
        FixButtonPosition("Laser_Tower_Button", new Vector2(-180, -180), new Vector2(80, 60));
        
        // Fix Labels
        FixLabelText("Basic_Tower_Label", "Basic\\n$100");
        FixLabelText("Cannon_Tower_Label", "Cannon\\n$150");
        FixLabelText("Laser_Tower_Label", "Laser\\n$200");
        
        Debug.Log("UI Layout Fixed!");
    }
    
    void FixTextPosition(string objName, Vector2 pos, Vector2 size)
    {
        GameObject obj = GameObject.Find(objName);
        if (obj != null)
        {
            RectTransform rect = obj.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.anchoredPosition = pos;
                rect.sizeDelta = size;
            }
            
            TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                text.fontSize = 18;
                text.color = Color.white;
            }
        }
    }
    
    void FixPanelPosition(string objName, Vector2 pos, Vector2 size)
    {
        GameObject obj = GameObject.Find(objName);
        if (obj != null)
        {
            RectTransform rect = obj.GetComponent<RectTransform>();
            if (rect != null)
            {
                if (objName == "HUD_Panel")
                {
                    rect.anchorMin = new Vector2(0, 1);
                    rect.anchorMax = new Vector2(1, 1);
                    rect.anchoredPosition = pos;
                    rect.sizeDelta = size;
                }
                else
                {
                    rect.anchorMin = new Vector2(1, 0.5f);
                    rect.anchorMax = new Vector2(1, 0.5f);
                    rect.anchoredPosition = pos;
                    rect.sizeDelta = size;
                }
            }
            
            Image img = obj.GetComponent<Image>();
            if (img != null)
            {
                img.color = new Color(0, 0, 0, 0.3f);
            }
        }
    }
    
    void FixButtonPosition(string objName, Vector2 pos, Vector2 size)
    {
        GameObject obj = GameObject.Find(objName);
        if (obj != null)
        {
            RectTransform rect = obj.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(1, 0.5f);
                rect.anchorMax = new Vector2(1, 0.5f);
                rect.anchoredPosition = pos;
                rect.sizeDelta = size;
            }
            
            Image img = obj.GetComponent<Image>();
            if (img != null)
            {
                img.color = new Color(0.2f, 0.4f, 0.8f, 0.8f);
            }
        }
    }
    
    void FixLabelText(string objName, string text)
    {
        GameObject obj = GameObject.Find(objName);
        if (obj != null)
        {
            TextMeshProUGUI textComp = obj.GetComponent<TextMeshProUGUI>();
            if (textComp != null)
            {
                textComp.text = text;
                textComp.fontSize = 14;
                textComp.color = Color.white;
            }
        }
    }
}