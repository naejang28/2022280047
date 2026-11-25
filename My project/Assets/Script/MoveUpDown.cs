using UnityEngine;

public class MoveUpDown : MonoBehaviour
{
    public float height = 0.15f;   // 오르내리는 폭
    public float speed = 2f;       // 빠르기
    private Vector3 basePos;

    private void Start()
    { basePos = transform.localPosition; }

    private void Update()
    {
        transform.localPosition = basePos + Vector3.up * (Mathf.Sin(Time.time * speed) * height);
    }
}
