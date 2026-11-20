using UnityEngine;

public class ScaleUpDown : MonoBehaviour
{
    public float amount = 0.05f;   // 커지는 정도. 0.05 는 5퍼센트
    public float speed = 3f;       // 빠르기
    private Vector3 baseScale;

    private void Start()
    { baseScale = transform.localScale; }

    private void Update()
    {
        transform.localScale = baseScale * (1f + Mathf.Sin(Time.time * speed) * amount);
    }
}
