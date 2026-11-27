using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DropdownTooltip : MonoBehaviour
{
    public static DropdownTooltip Instance { get; private set; }

    public RectTransform panel;     // Tooltip 오브젝트
    public TMP_Text text;           // TooltipText
    public Vector2 offset = new Vector2(16f, -20f);

    private Canvas canvas;
    private bool visible;

    void Awake()
    {
        Instance = this;
        canvas = GetComponentInParent<Canvas>();
        Hide();
    }

    void Update()
    {
        if (!visible || Mouse.current == null) return;

        Vector2 pos = Mouse.current.position.ReadValue() + offset;

        // 화면 밖으로 안 나가게 보정
        Vector2 size = panel.sizeDelta * canvas.scaleFactor;
        pos.x = Mathf.Min(pos.x, Screen.width - size.x);
        pos.y = Mathf.Max(pos.y, size.y);
        panel.position = pos;
    }

    public void Show(string message)
    {
        text.text = message;
        panel.gameObject.SetActive(true);
        visible = true;
        panel.SetAsLastSibling();
    }

    public void Hide()
    {
        visible = false;
        panel.gameObject.SetActive(false);
    }
}