using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_Text))]
public class TruncatedLabelTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TMP_Text label;
    private bool showing;

    void Awake()
    {
        label = GetComponent<TMP_Text>();
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Ellipsis;
        label.raycastTarget = true;
    }

    public void OnPointerEnter(PointerEventData e)
    {
        if (DropdownTooltip.Instance == null) return;

        float full = label.GetPreferredValues(label.text).x;
        float avail = label.rectTransform.rect.width
                      - label.margin.x - label.margin.z;

        if (full > avail + 0.5f)
        {
            DropdownTooltip.Instance.Show(label.text);
            showing = true;
        }
    }

    public void OnPointerExit(PointerEventData e)
    {
        HideIfShowing();
    }

    // 항목이 꺼지거나 파괴될 때(드롭다운이 닫힐 때) 툴팁 정리
    void OnDisable() { HideIfShowing(); }
    void OnDestroy() { HideIfShowing(); }

    void HideIfShowing()
    {
        if (!showing) return;
        showing = false;
        if (DropdownTooltip.Instance != null)
            DropdownTooltip.Instance.Hide();
    }
}