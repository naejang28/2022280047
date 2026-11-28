using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameMenu : MonoBehaviour
{
    public static bool IsOpen { get; private set; }   // 다른 스크립트가 확인할 수 있는 전역 상태

    public GameObject menu;            // Menu 판
    public Slider bgmSlider;           // 배경음 슬라이더
    public Slider sfxSlider;           // 효과음 슬라이더
    public Slider lookSlider;          // 마우스 감도
    public FirstPersonMover mover;     // Player

    private void Start()
    {
        IsOpen = false;
        Time.timeScale = 1f;
        menu.SetActive(false);

        var sound = SoundSettings.Instance;
        if (sound != null)
        {
            bgmSlider.value = sound.BgmValue;
            sfxSlider.value = sound.SfxValue;
            bgmSlider.onValueChanged.AddListener(v => sound.SetBgm(v));
            sfxSlider.onValueChanged.AddListener(v => sound.SetSfx(v));
        }

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
        mover.lookSpeed = lookSlider.value;   // 볼륨은 슬라이더 리스너가 처리하므로 여기선 감도만
    }

    public void Close()
    { Show(false); }

    private void Show(bool open)
    {
        IsOpen = open;
        menu.SetActive(open);
        mover.canMove = !open;

        Time.timeScale = open ? 0f : 1f;
        AudioListener.pause = open;          // BGM 제외 모든 소리 일시정지 / 재개

        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = open;

        if (!open)
        {
            PlayerPrefs.SetFloat("look", lookSlider.value);
            if (SoundSettings.Instance != null) SoundSettings.Instance.Save();
        }
    }

    private void OnDisable()
    {
        IsOpen = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }
}