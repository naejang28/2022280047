using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_Text))]
public class MarqueeLabel : MonoBehaviour
{
    [Header("동작")]
    public float speed = 40f;          // 초당 이동 픽셀
    public float startDelay = 1f;      // 처음 멈춰 있는 시간
    public float endDelay = 1f;        // 끝에서 멈춰 있는 시간
    public bool onlyOnHover = false;   // true면 마우스 올렸을 때만 흐름 (아래 참고)

    private TMP_Text label;
    private RectTransform rt;
    private RectTransform viewport;    // 부모(보이는 영역)
    private string lastText;
    private float timer;
    private float startX;
    private bool inited;

    void Awake()
    {
        label = GetComponent<TMP_Text>();
        rt = label.rectTransform;
        viewport = rt.parent as RectTransform;

        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Overflow;

        // 부모에 마스크가 없으면 추가 (보이는 영역 밖은 잘라냄)
        if (viewport != null && viewport.GetComponent<RectMask2D>() == null)
            viewport.gameObject.AddComponent<RectMask2D>();
    }

    void OnEnable()
    {
        ResetPosition();
    }

    void LateUpdate()
    {
        if (viewport == null) return;

        // 글자가 바뀌면 처음부터 다시
        if (label.text != lastText)
        {
            lastText = label.text;
            ResetPosition();
        }

        float textWidth = label.GetPreferredValues(label.text).x;
        float viewWidth = viewport.rect.width;
        float overflow = textWidth - viewWidth;

        // 안 넘치면 가만히 (왼쪽 정렬)
        if (overflow <= 1f)
        {
            SetX(0f);
            return;
        }

        // 라벨 폭을 글자 길이에 맞춰 두어야 끝까지 보임
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);

        timer += Time.unscaledDeltaTime;

        float moveTime = overflow / speed;
        float total = startDelay + moveTime + endDelay;

        if (timer >= total) timer = 0f;

        float x;
        if (timer < startDelay) x = 0f;
        else if (timer < startDelay + moveTime) x = -(timer - startDelay) * speed;
        else x = -overflow;

        SetX(x);
    }

    void ResetPosition()
    {
        timer = 0f;
        SetX(0f);
    }

    void SetX(float x)
    {
        Vector2 p = rt.anchoredPosition;
        p.x = startX + x;
        rt.anchoredPosition = p;
    }
}