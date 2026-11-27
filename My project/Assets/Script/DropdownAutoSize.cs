using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_Dropdown))]
public class DropdownAutoSize : MonoBehaviour
{
    [Header("여백")]
    public float paddingLeft = 20f;
    public float paddingRight = 50f;   // 화살표 자리
    public float minWidth = 150f;
    public float maxWidth = 600f;

    [Header("펼친 목록")]
    public float itemHeight = 40f;
    public int maxVisibleItems = 6;

    private TMP_Dropdown dd;
    private RectTransform rt;

    void Awake()
    {
        dd = GetComponent<TMP_Dropdown>();
        rt = GetComponent<RectTransform>();
    }

    void Start()
    {
        Resize();
        dd.onValueChanged.AddListener(_ => Resize());
    }

    public void Resize()
    {
        if (dd == null) return;

        TMP_Text label = dd.captionText;
        if (label == null) return;

        // 줄바꿈을 끄고 한 줄 기준으로 측정
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Overflow;

        float widest = 0f;
        foreach (var opt in dd.options)
        {
            Vector2 size = label.GetPreferredValues(opt.text);
            widest = Mathf.Max(widest, size.x);
        }

        // 측정 후 넘치면 ... 처리
        label.overflowMode = TextOverflowModes.Ellipsis;


        RectTransform template = dd.template;
        if (template != null)
        {
            int count = Mathf.Min(dd.options.Count, maxVisibleItems);
            template.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, count * itemHeight + 4f);

            var item = template.GetComponentInChildren<Toggle>(true);
            if (item != null)
            {
                var itemRt = item.GetComponent<RectTransform>();
                itemRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, itemHeight);

                // 항목 글자도 줄바꿈 끄고 넘치면 ... 처리
                var itemLabel = item.GetComponentInChildren<TMP_Text>(true);
                if (itemLabel != null)
                {
                    itemLabel.textWrappingMode = TextWrappingModes.NoWrap;
                    itemLabel.overflowMode = TextOverflowModes.Ellipsis;
                }
            }
        }
    }
}