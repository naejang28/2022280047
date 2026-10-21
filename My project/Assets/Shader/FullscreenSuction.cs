
using UnityEngine;

public class FullscreenSuction : MonoBehaviour
{
    public Material fullscreenMaterial;
    public Animator bookAnimator;
    public float duration = 3f;

    private bool isPlaying = false;
    private float timer = 0f;


    private void Start()
    {
        if (bookAnimator != null)
        {
            bookAnimator.ResetTrigger("OpenBook");
            bookAnimator.Play("Idle", 0, 0f);
            bookAnimator.Update(0f);
        }

        if (fullscreenMaterial != null)
        {
            fullscreenMaterial.SetFloat("_Zoom", 1f);
            fullscreenMaterial.SetFloat("_TwirlStrength", 0f);
        }
    }



    private void OnMouseDown()
    {
        Debug.Log("책 클릭 감지!");

        if (bookAnimator == null)
        {
            Debug.LogError("Book Animator가 연결되지 않았어!");
            return;
        }

        bookAnimator.ResetTrigger("OpenBook");
        bookAnimator.SetTrigger("OpenBook");

        Debug.Log("OpenBook 트리거 실행!");
    }


    private void Update()
    {
        if (!isPlaying || fullscreenMaterial == null)
            return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);

        fullscreenMaterial.SetFloat("_Zoom", Mathf.Lerp(1f, 0.5f, t));
        fullscreenMaterial.SetFloat("_TwirlStrength", Mathf.Lerp(0f, 5f, t));

        if (t >= 1f)
            isPlaying = false;
    }
}
