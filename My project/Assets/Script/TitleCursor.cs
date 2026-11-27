using UnityEngine;

public class TitleCursor : MonoBehaviour
{
    void Start()
    {
        Time.timeScale = 1f;                    // 메뉴에서 멈춘 채 넘어오는 경우 방지
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        AudioListener.pause = false;
    }
}