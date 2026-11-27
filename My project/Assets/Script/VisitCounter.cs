using UnityEngine;
using TMPro;

public class VisitCounter : MonoBehaviour
{
    public int total = 14;          // 작품 Zone 의 개수
    public GameObject reward;      // 다 보면 켜질 것
    private int seen = 0;          // 지금까지 본 개수
    private TMP_Text label;

    private void Start()
    {
        label = GetComponent<TMP_Text>();
        label.text = "관람 0 / " + total;
        reward.SetActive(false);
    }
    public void Add()
    {
        seen = seen + 1;
        label.text = "관람 " + seen + " / " + total;
        if (seen >= total) reward.SetActive(true);
    }
}