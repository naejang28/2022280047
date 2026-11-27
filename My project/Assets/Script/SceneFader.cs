using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneFader : MonoBehaviour
{
    public float duration = 0.6f;       // 페이드에 걸리는 시간(초)
    public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    private Image black;
    private float alpha = 1f;           // 1 은 검은 화면, 0 은 투명
    private string nextScene;           // 넘어갈 씬. 비어 있으면 밝아지는 중
    private static SceneFader current;
    private void Awake()
    {
        current = this;
        black = GetComponent<Image>();
        black.raycastTarget = false;    // 버튼 클릭을 막지 않게
        black.color = Color.black;      // 시작은 검은 화면
    }

    private void Update()
    {
        float target = (nextScene == null) ? 0f : 1f;
        alpha = Mathf.MoveTowards(alpha, target, Time.unscaledDeltaTime / duration);
        black.color = new Color(0f, 0f, 0f, curve.Evaluate(alpha));
        if (nextScene != null && alpha >= 1f) SceneManager.LoadScene(nextScene);
    }
    public static void Go(string sceneName)
    {
        if (current == null) SceneManager.LoadScene(sceneName);
        else current.nextScene = sceneName;
    }
}
