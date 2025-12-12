using System.Collections;
using Ultrabolt.SkyEngine;
using UnityEngine;

public class NaturalBGMSoundManager : MonoBehaviour
{
    public static NaturalBGMSoundManager Instance;

    [Header("Sky Controller 연결")]
    public SkyController skyController; // 시간대 확인용
    private string currentDayState = "";

    [Header("플레이어 연결")]
    public Transform playerTransform;

    //[Header("Sky 상태 입력")]
    //public string simulatedDayState = "Morning";  // 테스트용

    [Header("자연 사운드 소스")]
    public AudioSource treeWindSource;
    public AudioSource birdSource;
    public AudioSource waterSource;

    [Header("사운드 클립")]
    public AudioClip treeWindClip;
    public AudioClip birdMorningClip;
    public AudioClip birdNightClip;
    public AudioClip waterClip;

    [Header("사운드 반응 반경")]
    public float treeDistance = 10f;
    public float waterDistance = 30f;

    [Header("볼륨 기본값")]
    [Range(0f, 1f)] public float treeBaseVolume = 1f;
    [Range(0f, 1f)] public float birdBaseVolume = 0.2f;
    [Range(0f, 1f)] public float waterBaseVolume = 0.3f;

    // 볼륨 최대 상한선 (이 이상은 무조건 차단)
    private const float MaxVolume = 1f;

    private string currentBirdState = "";

    private Coroutine treeFadeCoroutine;
    private Coroutine waterFadeCoroutine;
    private Coroutine birdFadeCoroutine;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (skyController == null)
            skyController = FindObjectOfType<SkyController>();

        if (skyController != null)
        {
            currentDayState = skyController.GetComponent<SkyCore>().dayState;
            HandleBirdSound(currentDayState);
        }

        InitAudioSource(treeWindSource, treeWindClip);
        InitAudioSource(birdSource, null);  // bird는 상태에 따라 Play
        InitAudioSource(waterSource, waterClip);
    }

    private void InitAudioSource(AudioSource source, AudioClip clip)
    {
        if (source == null) return;

        source.loop = true;
        source.volume = 0f;
        if (clip != null)
        {
            source.clip = clip;
            source.Play();
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        HandleTreeSound();
        HandleWaterSound();

        string newDayState = skyController.GetComponent<SkyCore>().dayState;
        if (currentDayState != newDayState)
        {
            currentDayState = newDayState;
            Debug.Log($"[하늘 상태] 변경 감지됨: {currentDayState}");
            HandleBirdSound(currentDayState);
        }
    }

    void HandleTreeSound()
    {
        GameObject[] trees = GameObject.FindGameObjectsWithTag("Tree");
        float nearest = float.MaxValue;

        foreach (var tree in trees)
        {
            float dist = Vector3.Distance(playerTransform.position, tree.transform.position);
            if (dist < nearest) nearest = dist;
        }

        float volume = Mathf.Clamp01(1 - (nearest / treeDistance)) * treeBaseVolume;
        volume = Mathf.Min(volume, MaxVolume);

        if (treeFadeCoroutine != null) StopCoroutine(treeFadeCoroutine);
        treeFadeCoroutine = StartCoroutine(Fade(treeWindSource, volume, 1f));
    }

    void HandleWaterSound()
    {
        GameObject[] waters = GameObject.FindGameObjectsWithTag("Water");
        float nearest = float.MaxValue;

        foreach (var water in waters)
        {
            float dist = Vector3.Distance(playerTransform.position, water.transform.position);
            if (dist < nearest) nearest = dist;
        }

        float volume = Mathf.Clamp01(1 - (nearest / waterDistance)) * waterBaseVolume;
        volume = Mathf.Min(volume, MaxVolume);

        if (volume > 0f)
        {
            if (!waterSource.isPlaying)
            {
                waterSource.volume = 0f;
                waterSource.Play();
            }
            if (waterFadeCoroutine != null) StopCoroutine(waterFadeCoroutine);
            waterFadeCoroutine = StartCoroutine(Fade(waterSource, volume, 1.2f));
        }
        else if (waterSource.isPlaying)
        {
            if (waterFadeCoroutine != null) StopCoroutine(waterFadeCoroutine);
            waterFadeCoroutine = StartCoroutine(FadeOutThenStop(waterSource, 1.5f));
        }
    }

    void HandleBirdSound(string currentDayState)
    {
        string state = currentDayState;

        if (state == "Dawn" || state == "Morning" || state == "Noon")
        {
            if (currentBirdState != "Day")
            {
                currentBirdState = "Day";
                if (birdFadeCoroutine != null) StopCoroutine(birdFadeCoroutine);
                StartCoroutine(SwitchBirdClip(birdMorningClip, birdBaseVolume));
            }
        }
        else if (state == "Evening" || state == "Night")
        {
            if (currentBirdState != "Night")
            {
                currentBirdState = "Night";
                if (birdFadeCoroutine != null) StopCoroutine(birdFadeCoroutine);
                StartCoroutine(SwitchBirdClip(birdNightClip, birdBaseVolume * 0.8f));
            }
        }
    }

    IEnumerator SwitchBirdClip(AudioClip clip, float volume)
    {
        if (clip == null)
        {
            Debug.LogWarning("[새소리] 클립이 null입니다! Inspector에서 birdMorningClip / birdNightClip 할당 확인!");
            yield break;
        }

        if (birdSource.isPlaying)
            yield return FadeOutThenStop(birdSource, 1f);

        birdSource.clip = clip;
        birdSource.volume = 0f;
        birdSource.Play();

        Debug.Log($"[새소리] 클립 변경됨: {clip.name}, 타겟 볼륨: {volume}");

        volume = Mathf.Min(volume, MaxVolume);
        birdFadeCoroutine = StartCoroutine(Fade(birdSource, volume, 1.5f));
    }

    IEnumerator Fade(AudioSource source, float target, float duration)
    {
        target = Mathf.Min(target, MaxVolume);

        float start = source.volume;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            source.volume = Mathf.Lerp(start, target, time / duration);
            yield return null;
        }

        source.volume = target;
    }

    IEnumerator FadeOutThenStop(AudioSource source, float duration)
    {
        float start = source.volume;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            source.volume = Mathf.Lerp(start, 0f, time / duration);
            yield return null;
        }

        source.volume = 0f;
        source.Stop();
    }
}