using UnityEngine;
using UnityEngine.InputSystem;

public class SceneZone : MonoBehaviour
{
    public GameObject prompt;       // PRESS E 글자
    public string sceneName;        // 넘어갈 씬 이름
    public Transform door;          // 문의 경첩. 비워 두면 문 없이 넘어감
    public float openAngle = 90f;   // 열리는 각도. 음수면 반대로 열림
    public AudioSource openSound;   // 문 여는 소리. 비워 두면 소리 없음
    private bool playerNear;
    private bool opening;
    private float angle;

    private void Start()
    { GetComponent<MeshRenderer>().enabled = false; }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerNear = true;
        prompt.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerNear = false;
        prompt.SetActive(false);
    }
    private void Update()
    {
        if (playerNear && !opening && Keyboard.current.eKey.wasPressedThisFrame)
        {
            opening = true;
            if (openSound != null) openSound.Play();
        }
        if (!opening) return;
        prompt.SetActive(false);
        if (door != null && Mathf.Abs(angle) < Mathf.Abs(openAngle))
        {
            float step = openAngle * Time.deltaTime;   // 1초에 다 열림
            door.Rotate(0f, step, 0f);
            angle += step;
            return;
        }
        SceneFader.Go(sceneName);
    }
}
