using UnityEngine;
using UnityEngine.Audio;

public class SoundSettings : MonoBehaviour
{
    public static SoundSettings Instance { get; private set; }

    public AudioMixer mixer;

    public float BgmValue { get; private set; } = 1f;
    public float SfxValue { get; private set; } = 1f;

    void Awake()
    {
        Instance = this;
        BgmValue = PlayerPrefs.GetFloat("bgmVol", 0.5f);
        SfxValue = PlayerPrefs.GetFloat("sfxVol", 0.5f);
    }

    void Start()
    {
        // 믹서 파라미터는 Awake보다 Start에서 설정해야 안정적이에요
        Apply();
    }

    public void SetBgm(float v)
    {
        BgmValue = Mathf.Clamp01(v);
        if (mixer != null) mixer.SetFloat("BGMVol", ToDb(BgmValue));
    }

    public void SetSfx(float v)
    {
        SfxValue = Mathf.Clamp01(v);
        if (mixer != null) mixer.SetFloat("SFXVol", ToDb(SfxValue));
    }

    public void Save()
    {
        PlayerPrefs.SetFloat("bgmVol", BgmValue);
        PlayerPrefs.SetFloat("sfxVol", SfxValue);
    }

    void Apply()
    {
        SetBgm(BgmValue);
        SetSfx(SfxValue);
    }

    // 0~1 슬라이더 값을 dB로 변환 (0이면 -80dB = 무음)
    static float ToDb(float v)
    {
        return Mathf.Log10(Mathf.Max(v, 0.0001f)) * 20f;
    }
}