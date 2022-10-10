using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class HoverClickLink : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{

    public TextMeshProUGUI tooltipText;

    private TextMeshProUGUI m_tmpText;
    private bool m_isInText = false;

    private void Awake()
    {
        m_tmpText = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (!m_isInText) return;
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(m_tmpText, Input.mousePosition, null);
        tooltipText.gameObject.SetActive(linkIndex != -1);
        if (linkIndex != -1)
        { 
            var linkInfo = m_tmpText.textInfo.linkInfo[linkIndex];
            tooltipText.text = linkInfo.GetLinkID();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        m_isInText = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_isInText = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(m_tmpText, Input.mousePosition, null);
        if (linkIndex == -1) return;
        TMP_LinkInfo linkInfo = m_tmpText.textInfo.linkInfo[linkIndex];
        Debug.Log("Clicked: " + linkInfo.GetLinkID());
    }

}
