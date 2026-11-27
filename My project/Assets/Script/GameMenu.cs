using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameMenu : MonoBehaviour
{
    public static bool IsOpen { get; private set; }   // 다른 스크립트가 확인할 수 있는 전역 상태

    public GameObject menu;            // Menu 판
    public Slider volumeSlider;        // 소리 크기
    public Slider lookSlider;          // 마우스 감도
    public FirstPersonMover mover;     // Player

    private void Start()
    {
        IsOpen = false;
        Time.timeScale = 1f;
        menu.SetActive(false);
        volumeSlider.value = AudioListener.volume;
        lookSlider.value = PlayerPrefs.GetFloat("look", mover.lookSpeed);
        mover.lookSpeed = lookSlider.value;
    }

    private void Update()
    {
        Keyboard key = Keyboard.current;
        if (key == null) return;

        if (key.tabKey.wasPressedThisFrame || key.escapeKey.wasPressedThisFrame)
            Show(!menu.activeSelf);

        if (!menu.activeSelf) return;
        AudioListener.volume = volumeSlider.value;
        mover.lookSpeed = lookSlider.value;
    }

    public void Close()
    { Show(false); }

    private void Show(bool open)
    {
        IsOpen = open;
        menu.SetActive(open);
        mover.canMove = !open;

        // 게임 시간 정지: Update 안의 시간 기반 동작, 애니메이션, 물리가 멈춤
        Time.timeScale = open ? 0f : 1f;
        AudioListener.pause = open;          // BGM 제외 모든 소리 일시정지 / 재개

        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = open;

        if (!open) PlayerPrefs.SetFloat("look", lookSlider.value);
    }

    private void OnDisable()
    {
        // 씬 전환 등으로 꺼질 때 멈춘 채 남지 않게
        IsOpen = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }
}