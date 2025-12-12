using UnityEngine;

public class IntroUISoundManager : MonoBehaviour
{
    public static IntroUISoundManager Instance;

    [Header("BGM 관련")]
    public AudioSource bgmSource;
    public AudioClip mainBGM;

    [Header("SFX 관련")]
    public AudioSource sfxSource;
    public AudioClip clickSFX;
    public AudioClip hoverSFX;

    void Awake()
    {
        // 싱글톤: 한 개만 존재
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 넘어가도 유지
        }
        else
        {
            Destroy(gameObject); // 중복 생성 방지
        }
    }

    void Start()
    {
        PlayBGM(mainBGM);
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip); // 겹쳐서 재생 가능
    }

    public void PlayClick(float volume = 0.5f)
    {
        if (clickSFX == null) return;
        sfxSource.PlayOneShot(clickSFX, volume); // 볼륨 인자 추가
    }
    public void PlayHover() => PlaySFX(hoverSFX);
}