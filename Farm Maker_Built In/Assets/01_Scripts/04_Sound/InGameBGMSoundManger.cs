using System.Collections;
using Ultrabolt.SkyEngine;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class InGameBGMSoundManager : MonoBehaviour
{
    public static InGameBGMSoundManager Instance;

    [Header("Sky Controller 연결")]
    public SkyController skyController; // 시간대 확인용

    [Header("BGM 설정")]
    public AudioSource bgmSource;
    public AudioMixerGroup bgmMixerGroup;
    [Range(0f, 1f)] public float targetVolume = 0.2f;
    public float fadeDuration = 2f;

    [Header("벤치별 BGM 클립")]
    public AudioClip benchAClip;
    public AudioClip benchBClip;
    public AudioClip benchCClip;

    [Header("시간대별 BGM 리스트")]
    public AudioClip[] dayClips;   // Dawn / Morning / Noon
    public AudioClip[] nightClips; // Evening / Night

    private string currentDayState = "";
    private Coroutine fadeCoroutine;
    private AudioClip originalBGM; // 기본 BGM 저장용
    private bool isBenchBGM = false;

    private Coroutine volumeFadeCoroutine;// 벤치 앉았을때 볼륨 다운 / 복구
    private float previousVolume = -1f;  // -1이면 아직 저장 안 됨 의미
    public BenchInteractSimple benchInteract; // 벤치 스크립트 연결

    private string currentBenchTag = null;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // ** IntroScene 에서 Main 씬 넘어갈때 BGM 통일하기 기능
            //SceneManager.sceneLoaded += OnSceneLoaded; //  이벤트 연결

            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.loop = true;
                bgmSource.playOnAwake = false;
                bgmSource.volume = targetVolume;
                bgmSource.outputAudioMixerGroup = bgmMixerGroup;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (skyController == null)
            skyController = FindObjectOfType<SkyController>();

        if (skyController != null)
        {
            currentDayState = skyController.GetComponent<SkyCore>().dayState;
            PlayRandomByState(currentDayState);
        }

        // ** IntroScene 에서 Main 씬 넘어갈때 BGM 통일하기 기능 ↓
        //else
        //{
        //    //  SkyController 없으면 Dawn으로 간주하고 BGM 재생
        //    currentDayState = "Dawn"; // or "Morning"
        //    PlayRandomByState(currentDayState);
        //}
    }
    //private void OnDestroy()
    //{
    //    // 씬 전환 시 중복 제거 또는 종료 시 이벤트 해제
    //    if (Instance == this)
    //    {
    //        SceneManager.sceneLoaded -= OnSceneLoaded;
    //    }
    //}

    //private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    //{
    //    Debug.Log($"[BGM] 씬 전환 감지됨: {scene.name}");
    //    skyController = FindObjectOfType<SkyController>();

    //    if (skyController != null)
    //    {
    //        currentDayState = skyController.GetComponent<SkyCore>().dayState;
    //        PlayRandomByState(currentDayState);
    //    }
    //}
    // ** IntroScene 에서 Main 씬 넘어갈때 BGM 통일하기 기능 ↑

    //private void Update()
    //{
    //    if (skyController == null || isBenchBGM) return; // 벤치 중에는 무시

    //    string newState = skyController.GetComponent<SkyCore>().dayState;

    //    if (newState != currentDayState)
    //    {
    //        Debug.Log($"[BGM] 상태 변경 감지됨: {currentDayState} → {newState}");
    //        currentDayState = newState;
    //        PlayRandomByState(currentDayState);
    //    }

    //}

    private void Update()
    {
        if (skyController == null) return;

        string newState = skyController.GetComponent<SkyCore>().dayState;

        // 상태 변화 감지
        if (newState != currentDayState)
        {
            Debug.Log($"[BGM] 상태 변경 감지됨: {currentDayState} → {newState}");

            // 앉아 있든 말든 자연 BGM은 항상 실행!
            PlayRandomByState(newState);

            if (benchInteract != null && benchInteract.isSitting)
            {
                HandleDayNightChangeWhileSitting(newState);
            }

            currentDayState = newState;
        }
    }

    private void HandleDayNightChangeWhileSitting(string newState)
    {
        // 밤이 되었다면 → 볼륨 복구 후 벤치 BGM 재생
        if (newState == "Evening" || newState == "Night")
        {
            Debug.Log("[BGM] 밤으로 넘어가서 벤치 BGM 재생함");

            // 낮 상태에서 앉아 있었던 경우 → 복원된 볼륨으로 되돌리기
            SetVolumeForBench(false);

            if (!string.IsNullOrEmpty(currentBenchTag))
                PlayBenchBGM(currentBenchTag); // 현재 앉은 벤치 태그로 재생
        }
        // 낮이 되었다면 → 벤치 BGM 종료, 메인 BGM 유지, 볼륨만 낮춤
        else if (newState == "Dawn" || newState == "Morning" || newState == "MidNoon")
        {
            Debug.Log("[BGM] 낮으로 넘어가서 벤치 BGM 끄고 메인 BGM 볼륨 다운 유지");

            RestoreOriginalBGM();         // 메인 BGM 복구
            SetVolumeForBench(true);      // 낮 → 볼륨 낮추기
        }
    }

    // 벤치에 앉았을때 [낮인 볼륨 다운 / 업]

    public void SetVolumeForBench(bool isSitting)
    {
        string current = skyController?.GetComponent<SkyCore>()?.dayState;

        if (isSitting)
        {
            // 낮에만 볼륨 낮춤
            if (current == "Dawn" || current == "Morning" || current == "MidNoon")
            {
                if (previousVolume < 0f)
                    previousVolume = bgmSource.volume;

                float loweredVolume = previousVolume * 0.3f;

                if (volumeFadeCoroutine != null)
                    StopCoroutine(volumeFadeCoroutine);

                volumeFadeCoroutine = StartCoroutine(FadeVolumeTo(loweredVolume));
            }
        }
        else
        {
            // 일어날 때는 시간대 무관하게 복구
            if (previousVolume >= 0f)
            {
                if (volumeFadeCoroutine != null)
                    StopCoroutine(volumeFadeCoroutine);

                volumeFadeCoroutine = StartCoroutine(FadeVolumeTo(previousVolume));
                previousVolume = -1f;
            }
        }
    }

    private IEnumerator FadeVolumeTo(float target)
    {
        float startVol = bgmSource.volume;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVol, target, t / fadeDuration);
            yield return null;
        }

        bgmSource.volume = target;
    }

    /// <summary>
    /// 시간대에 맞는 랜덤 BGM 재생
    /// </summary>
    private void PlayRandomByState(string state)
    {
        AudioClip[] clips = null;

        //  낮 그룹 (Dawn, Morning, Noon)
        if (state == "Dawn" || state == "Morning" || state == "MidNoon")
        {
            clips = dayClips;

            //  아침 효과음 (한 번만)
            if (state == "Morning" && SFXSoundManager.Instance != null)
                SFXSoundManager.Instance.Play("MorningChicken", 0.05f, 5f);
        }
        //  밤 그룹 (Evening, Night)
        else if (state == "Evening" || state == "Night")
        {
            clips = nightClips;

            //  저녁 효과음 (한 번만)
            if (state == "Evening" && SFXSoundManager.Instance != null)
                SFXSoundManager.Instance.Play("Nightwolf", 0.05f, 5f);
        }
        else
        {
            return; // 위 두 그룹이 아닐 때만 빠져나감
        }

        if (clips == null || clips.Length == 0)
            return;

        AudioClip nextClip = clips[Random.Range(0, clips.Length)];

        // 같은 곡이면 스킵
        if (bgmSource.clip == nextClip)
            return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeToClip(nextClip));
        originalBGM = nextClip;

    }

    /// <summary>
    /// 페이드아웃 후 새 곡으로 전환, 다시 페이드인
    /// </summary>
    private IEnumerator FadeToClip(AudioClip newClip)
    {
        float startVol = bgmSource.volume;

        // 페이드아웃
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVol, 0f, t / fadeDuration);
            yield return null;
        }
        bgmSource.volume = 0f;

        // Clip이 null이면 그냥 정지 후 종료
        if (newClip == null)
        {
            bgmSource.Stop();
            yield break;
        }

        // 새 곡 재생
        bgmSource.clip = newClip;
        bgmSource.Play();

        // 페이드인
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(0f, targetVolume, t / fadeDuration);
            yield return null;
        }
        bgmSource.volume = targetVolume;
    }

    /// <summary>
    /// 벤치 태그에 따라 BGM 재생 (BenchA / BenchB / BenchC)
    /// </summary>
    // 벤치 전용 BGM 재생
    // 벤치 전용 BGM 재생

    public void PlayBenchBGM(string benchTag)
    {
        if (currentDayState == "Dawn" || currentDayState == "Morning" || currentDayState == "MidNoon")
        {
            Debug.Log($"[BGM] {benchTag} 벤치 BGM은 낮 시간대에는 재생되지 않습니다. 현재 상태: {currentDayState}");
            return;
        }

        AudioClip clip = benchTag switch
        {
            "BenchA" => benchAClip,
            "BenchB" => benchBClip,
            "BenchC" => benchCClip,
            _ => null
        };

        if (clip == null)
        {
            Debug.LogWarning($"[BGM] {benchTag}에 대한 벤치 BGM이 없습니다.");
            return;
        }

        // 현재 재생 중인 메인 BGM 저장
        if (!isBenchBGM && bgmSource.clip != null)
            originalBGM = bgmSource.clip;

        currentBenchTag = benchTag;
        isBenchBGM = true;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeToClip(clip));
    }

    //public void PlayBenchBGM(string benchTag)
    //{
    //    // 현재 재생 중인 곡을 originalBGM으로 저장 (복귀용)
    //    if (!isBenchBGM && bgmSource.clip != null)
    //        originalBGM = bgmSource.clip;

    //    AudioClip clip = benchTag switch
    //    {
    //        "BenchA" => benchAClip,
    //        "BenchB" => benchBClip,
    //        "BenchC" => benchCClip,
    //        _ => null
    //    };

    //    if (currentDayState == "Dawn" || currentDayState == "Morning" || currentDayState == "Noon")
    //    {
    //        Debug.Log($"[BGM] {benchTag} 벤치 BGM은 낮 시간대에는 재생되지 않습니다. 현재 상태: {currentDayState}");
    //        return;
    //    }

    //    if (clip == null)
    //    {
    //        Debug.LogWarning($"[BGM] {benchTag}에 대한 벤치 BGM이 없습니다.");
    //        return;
    //    }

    //    isBenchBGM = true;

    //    if (fadeCoroutine != null)
    //        StopCoroutine(fadeCoroutine);

    //    fadeCoroutine = StartCoroutine(FadeToClip(clip));
    //}

    // 벤치에서 일어날 때 기본 BGM 복귀

    public void RestoreOriginalBGM()
    {
        if (originalBGM == null || bgmSource.clip == originalBGM)
            return;

        isBenchBGM = false;
        currentBenchTag = null;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeToClip(originalBGM));
    }


    //public void RestoreOriginalBGM()
    //{
    //    currentBenchTag = null;

    //    if (originalBGM == null || bgmSource.clip == originalBGM)
    //        return;

    //    isBenchBGM = false;

    //    if (fadeCoroutine != null)
    //        StopCoroutine(fadeCoroutine);

    //    fadeCoroutine = StartCoroutine(FadeToClip(originalBGM));
    //}



}