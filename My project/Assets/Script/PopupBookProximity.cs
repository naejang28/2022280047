using UnityEngine;
using UnityEngine.Audio;

public class PopupBookProximity : MonoBehaviour
{
    [Header("대상")]
    public Animator bookAnimator;
    public Animator popupAnimator;
    public Transform player;
    public Transform distancePoint;
    public float openDistance = 20f;

    [Header("Animator 스테이트 이름")]
    public string bookStateName = "Book_Open";
    public string popupStateName = "Book_Open";

    [Header("클립 길이(초). 0이면 자동으로 읽음")]
    public float bookClipLength = 0f;
    public float popupClipLength = 0f;

    [Header("책(Book) 애니메이션 속도 조절")]
    public float bookOpenSpeed = 1f;
    public float bookCloseSpeed = 1f;

    [Header("팝업(Popup) 애니메이션 속도 조절")]
    public float popupOpenSpeed = 1f;
    public float popupCloseSpeed = 1f;

    [Header("열림 사운드")]
    public AudioClip openSound;
    [Range(0f, 1f)] public float openSoundVolume = 1f;
    [Tooltip("0 = 2D(어디서나 같은 크기), 1 = 3D(책에서 멀어지면 작아짐)")]
    [Range(0f, 1f)] public float openSoundSpatialBlend = 0f;
    [Tooltip("열리는 도중에 나갔다 들어와도 다시 재생할지")]
    public bool replayOnReopen = true;
    public AudioMixerGroup sfxGroup;
    // 0 = 닫힘, 1 = 열림
    private float bookProgress;
    private float popupProgress;
    private bool isOpen;
    private bool wasOpen;
    private AudioSource sfxSource;

    void Start()
    {
        Setup(bookAnimator);
        Setup(popupAnimator);
        Apply(bookAnimator, bookStateName, 0f);
        Apply(popupAnimator, popupStateName, 0f);

        // 효과음 전용 AudioSource 자동 생성
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = openSoundSpatialBlend;
        sfxSource.outputAudioMixerGroup = sfxGroup;
    }

    void Setup(Animator anim)
    {
        if (anim == null) return;
        anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        anim.speed = 0f; // 재생은 스크립트가 직접 제어 (음수 사용 안 함)
    }

    void Update()
    {
        if (player == null) return;

        Vector3 targetPosition = distancePoint != null
            ? distancePoint.position
            : transform.position;

        isOpen = Vector3.Distance(player.position, targetPosition) <= openDistance;

        // 닫힘 -> 열림으로 바뀐 순간에만 사운드 재생 (닫힐 때는 재생 안 함)
        if (isOpen && !wasOpen) PlayOpenSound();
        wasOpen = isOpen;

        bookProgress = Step(bookAnimator, bookStateName, bookProgress,
            ResolveLength(bookAnimator, bookClipLength),
            isOpen ? bookOpenSpeed : bookCloseSpeed);

        popupProgress = Step(popupAnimator, popupStateName, popupProgress,
            ResolveLength(popupAnimator, popupClipLength),
            isOpen ? popupOpenSpeed : popupCloseSpeed);
    }

    void PlayOpenSound()
    {
        if (openSound == null || sfxSource == null) return;

        // 이미 재생 중이면 겹쳐서 또 나지 않게 설정에 따라 처리
        if (sfxSource.isPlaying && !replayOnReopen) return;

        sfxSource.Stop();
        sfxSource.PlayOneShot(openSound, openSoundVolume);
    }

    float Step(Animator anim, string stateName, float progress, float length, float speed)
    {
        if (anim == null) return progress;

        float target = isOpen ? 1f : 0f;
        if (Mathf.Approximately(progress, target)) return progress;

        speed = Mathf.Max(speed, 0.01f);
        float delta = (Time.deltaTime * speed) / length;
        progress = Mathf.MoveTowards(progress, target, delta);

        Apply(anim, stateName, progress);
        return progress;
    }

    void Apply(Animator anim, string stateName, float progress)
    {
        if (anim == null) return;
        anim.speed = 0f;
        anim.Play(stateName, 0, Mathf.Clamp01(progress));
        anim.Update(0f); // 지정한 위치의 포즈를 즉시 반영
    }

    float ResolveLength(Animator anim, float manual)
    {
        if (manual > 0.01f) return manual;

        if (anim != null && anim.runtimeAnimatorController != null)
        {
            var clips = anim.runtimeAnimatorController.animationClips;
            if (clips.Length > 0 && clips[0].length > 0.01f)
                return clips[0].length;
        }
        return 2f; // 읽기 실패 시 기본값
    }
}