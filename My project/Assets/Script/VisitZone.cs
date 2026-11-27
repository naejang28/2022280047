using UnityEngine;

public class VisitZone : MonoBehaviour
{
    public VisitCounter counter;   // Progress 글자
    private bool visited;          // 이미 본 작품인지

    private void OnTriggerEnter(Collider other)
    {
        if (visited || !other.CompareTag("Player")) return;
        visited = true;
        counter.Add();
    }
}
