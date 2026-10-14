using UnityEngine;

public class ScreenshotTaker : MonoBehaviour
{
    // 원하는 키를 지정 (예: K 키를 누르면 캡처)
    public KeyCode screenshotKey = KeyCode.K;

    void Update()
    {
        if (Input.GetKeyDown(screenshotKey))
        {
            // 저장할 파일 이름 지정 (현재 시간 기준으로 생성)
            string fileName = "Screenshot_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";

            // 스크린샷 저장 실행
            ScreenCapture.CaptureScreenshot(fileName);

            Debug.Log("스크린샷 저장 완료: " + fileName);
        }
    }
}