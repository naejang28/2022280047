using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class BGMTrack
{
    public string title;      // UI에 표시할 곡 이름
    public AudioClip clip;
}

[RequireComponent(typeof(AudioSource))]
public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance { get; private set; }

    [Header("곡 목록")]
    public BGMTrack[] tracks;
    public int startIndex = 0;

    [Header("재생 설정")]
    [Range(0f, 1f)] public float volume = 0.5f;
    public float fadeTime = 1f;
    public bool autoNext = true;          // 곡이 끝나면 다음 곡 자동 재생
    public bool keepAcrossScenes = true;  // 씬이 바뀌어도 유지

    [Header("키 설정 (필요 없으면 None)")]
    public Key nextKey = Key.N;
    public Key prevKey = Key.B;
    public Key muteKey = Key.M;

    public int CurrentIndex { get; private set; } = -1;
    public string CurrentTitle =>
        (CurrentIndex >= 0 && CurrentIndex < tracks.Length) ? tracks[CurrentIndex].title : "";

    private AudioSource source;
    private Coroutine fadeRoutine;
    private bool muted;

    private void Awake()
    {
        // 씬 이동 시 중복 생성 방지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (keepAcrossScenes) DontDestroyOnLoad(gameObject);

        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f; // 2D 사운드
        source.ignoreListenerPause = true;
    }

    private void Start()
    {
        if (tracks != null && tracks.Length > 0)
            Play(Mathf.Clamp(startIndex, 0, tracks.Length - 1), false);
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (nextKey != Key.None && kb[nextKey].wasPressedThisFrame) Next();
            if (prevKey != Key.None && kb[prevKey].wasPressedThisFrame) Previous();
            if (muteKey != Key.None && kb[muteKey].wasPressedThisFrame) ToggleMute();
        }

        // 곡이 끝나면 다음 곡으로
        if (autoNext && fadeRoutine == null && CurrentIndex >= 0 &&
            source.clip != null && !source.isPlaying && !IsPausedByBrowser())
        {
            Next();
        }
    }

    // 웹빌드에서 브라우저가 소리를 막아 재생이 안 되는 상태와 곡 종료를 구분하기 위한 보조
    private bool IsPausedByBrowser()
    {
        return source.time <= 0.01f && source.timeSamples == 0;
    }

    // ---------- 외부에서 호출 가능한 함수 (UI 버튼 연결용) ----------
    public void Play(int index) { Play(index, true); }

    public void Next()
    {
        if (tracks.Length == 0) return;
        Play((CurrentIndex + 1) % tracks.Length, true);
    }

    public void Previous()
    {
        if (tracks.Length == 0) return;
        Play((CurrentIndex - 1 + tracks.Length) % tracks.Length, true);
    }

    public void ToggleMute()
    {
        muted = !muted;
        source.volume = muted ? 0f : volume;
    }

    public void SetVolume(float v)
    {
        volume = Mathf.Clamp01(v);
        if (!muted && fadeRoutine == null) source.volume = volume;
    }

    // ---------- 내부 로직 ----------
    private void Play(int index, bool fade)
    {
        if (tracks == null || index < 0 || index >= tracks.Length) return;
        if (tracks[index].clip == null) return;
        if (index == CurrentIndex && source.isPlaying) return;

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(ChangeTrack(index, fade));
    }

    private IEnumerator ChangeTrack(int index, bool fade)
    {
        float half = Mathf.Max(fadeTime * 0.5f, 0.01f);
        float targetVol = muted ? 0f : volume;

        // 페이드 아웃
        if (fade && source.isPlaying)
        {
            float startVol = source.volume;
            for (float t = 0f; t < half; t += Time.unscaledDeltaTime)
            {
                source.volume = Mathf.Lerp(startVol, 0f, t / half);
                yield return null;
            }
        }

        source.Stop();
        source.clip = tracks[index].clip;
        CurrentIndex = index;
        source.volume = fade ? 0f : targetVol;
        source.Play();

        // 페이드 인
        if (fade)
        {
            for (float t = 0f; t < half; t += Time.unscaledDeltaTime)
            {
                targetVol = muted ? 0f : volume;
                source.volume = Mathf.Lerp(0f, targetVol, t / half);
                yield return null;
            }
        }

        source.volume = muted ? 0f : volume;
        fadeRoutine = null;
    }
}