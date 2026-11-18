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

    private bool playerNear;
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
        if (!playerNear || Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame) return;

        bool willBeActive = panel != null ? !panel.activeSelf : true;

        if (panel != null) panel.SetActive(willBeActive);
        if (prompt != null) prompt.SetActive(false);

        if (lockMove && mover != null)
            mover.enabled = !willBeActive;

        // 💡 E키를 누르는 순간 사운드 오브젝트 딜레이 코루틴 실행
        if (soundObject != null)
        {
            // 기존에 돌고 있던 코루틴이 있다면 겹치지 않게 중지하고 새로 시작
            StopAllCoroutines();
            StartCoroutine(ActivateSoundObjectWithDelay());
        }
        else
        {
            Debug.LogWarning("SoundObject가 인스펙터에 연결되어 있지 않습니다!");
        }
    }

    IEnumerator ActivateSoundObjectWithDelay()
    {
        soundObject.SetActive(false); // 일단 꺼두고 대기
        yield return new WaitForSeconds(soundDelay);

        if (soundObject != null)
        {
            soundObject.SetActive(true);
            Debug.Log("사운드 오브젝트 켜짐!");
        }
    }
}