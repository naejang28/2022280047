
using UnityEngine;

public class StrengthAnimator : MonoBehaviour
{
    public float Strength = 0f;

    private Renderer targetRenderer;
    private MaterialPropertyBlock block;

    private static readonly int StrengthID =
        Shader.PropertyToID("_Strength");

    void Awake()
    {
        targetRenderer = GetComponent<Renderer>();
        block = new MaterialPropertyBlock();
    }

    void LateUpdate()
    {
        if (targetRenderer == null) return;

        targetRenderer.GetPropertyBlock(block);
        block.SetFloat(StrengthID, Strength);
        targetRenderer.SetPropertyBlock(block);
    }
}
