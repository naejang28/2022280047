using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractZone : MonoBehaviour
{
    public GameObject prompt;   // PRESS E 글자
    public GameObject panel;    // E 를 누르면 열릴 패널
    public bool lockMove;       // 체크하면 열린 동안 이동 멈춤

    [Header("Sound Delay Settings")]
    public GameObject soundObject;        // 💡 딜레이 후 켤 사운드 오브젝트
    public float soundDelay = 1.5f;       // 💡 사운드가 켜질 때까지의 딜레이 시간 (초)
    public bool playOnlyOnce = true;      // 💡 true면 한 번만 재생, false면 매번 재생

    private bool playerNear;
    private bool soundPlayed = false;     // 💡 이미 재생했는지 체크
    private FirstPersonMover mover;

    private void Start()
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null) mr.enabled = false;   // 플레이 중 큐브 숨김

        // 시작할 때 사운드 오브젝트는 꺼두기
        if (soundObject != null)
        {
            soundObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerNear = true;
        if (prompt != null) prompt.SetActive(true);
        mover = other.GetComponent<FirstPersonMover>();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerNear = false;
        if (prompt != null) prompt.SetActive(false);
        if (panel != null) panel.SetActive(false);

        // 존을 벗어날 때 사운드 오브젝트도 다시 꺼주기
        if (soundObject != null)
        {
            soundObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (GameMenu.IsOpen) return;
        if (!playerNear || Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame) return;

        bool willBeActive = panel != null ? !panel.activeSelf : true;

        if (panel != null) panel.SetActive(willBeActive);
        if (prompt != null) prompt.SetActive(false);

        if (lockMove && mover != null)
            mover.enabled = !willBeActive;

        // 💡 패널이 열릴 때만 + 아직 안 울렸을 때만(playOnlyOnce 기준) 사운드 딜레이 실행
        if (willBeActive && soundObject != null)
        {
            if (!playOnlyOnce || !soundPlayed)
            {
                soundPlayed = true;
                StopAllCoroutines();
                StartCoroutine(ActivateSoundObjectWithDelay());
            }
        }
        else if (!willBeActive && soundObject != null)
        {
            // 패널 닫을 때 혹시 대기 중인 코루틴이 있다면 취소
            StopAllCoroutines();
        }
    }

    IEnumerator ActivateSoundObjectWithDelay()
    {
        soundObject.SetActive(false); // 일단 꺼두고 대기
        yield return new WaitForSeconds(soundDelay);

        if (soundObject != null)
        {
            soundObject.SetActive(true);
            Debug.Log("사운드 오브젝트 켜짐 (1회 재생)!");
        }
    }
}