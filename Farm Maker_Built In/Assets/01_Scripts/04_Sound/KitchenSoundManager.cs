using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class KitchenSoundManager : MonoBehaviour
{
    public static KitchenSoundManager Instance;

    [Header("기본 오디오 소스")]
    public AudioSource kitchenSfxSource;

    [Header("루프용 사운드 (예: 장작 불소리)")]
    private AudioSource fireLoopSource;

    [Header("Audio Mixer 그룹 (효과음 볼륨 조절용)")]
    public AudioMixerGroup sfxMixerGroup;

    [Header("요리 관련 사운드 클립")]
    public AudioClip fireLoopClip;   // 장작 타는 루프
    public AudioClip bakeBreadClip;  // 빵 굽기
    public AudioClip omeletteClip;   // 오믈렛 만들기
    public AudioClip pieClip;        // 파이 만들기
    public AudioClip successClip;        // 파이 만들기

    // 사운드 클립을 관리하는 딕셔너리
    private Dictionary<string, AudioClip> clipDict = new Dictionary<string, AudioClip>();

    // 현재 재생 중인 AudioSource (요리 사운드 중단용)
    private AudioSource currentCookingSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (kitchenSfxSource == null)
            {
                kitchenSfxSource = gameObject.AddComponent<AudioSource>();
                kitchenSfxSource.playOnAwake = false;
                kitchenSfxSource.spatialBlend = 0f;
            }

            if (sfxMixerGroup != null)
                kitchenSfxSource.outputAudioMixerGroup = sfxMixerGroup;

            // 장작 불소리용 루프 AudioSource
            fireLoopSource = gameObject.AddComponent<AudioSource>();
            fireLoopSource.playOnAwake = false;
            fireLoopSource.loop = true;
            fireLoopSource.spatialBlend = 0f;
            fireLoopSource.outputAudioMixerGroup = sfxMixerGroup;

            // 사운드 등록
            clipDict["fireLoop"] = fireLoopClip;
            clipDict["bakeBread"] = bakeBreadClip;
            clipDict["omelette"] = omeletteClip;
            clipDict["pie"] = pieClip;
            clipDict["successClip"] = successClip;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 장작 소리
    public void PlayFireLoop()
    {
        if (fireLoopClip == null) return;
        if (!fireLoopSource.isPlaying)
        {
            fireLoopSource.clip = fireLoopClip;
            fireLoopSource.volume = 0.4f;
            fireLoopSource.Play();
        }
    }

    public void StopFireLoop()
    {
        if (fireLoopSource.isPlaying)
            fireLoopSource.Stop();
    }

    // 요리 사운드 (OneShot 또는 루프 가능)
    public void PlayCookingSound(string type, bool loop = false, float volume = 0.8f)
    {
        if (!clipDict.ContainsKey(type))
        {
            Debug.LogWarning($"[KitchenSoundManager] '{type}' 사운드가 등록되지 않았습니다!");
            return;
        }

        AudioClip clip = clipDict[type];

        // 기존에 재생 중인 요리 사운드 정지
        StopCookingSound();

        // 새로운 소스 생성
        GameObject tempObj = new GameObject($"CookingSound_{type}");
        currentCookingSource = tempObj.AddComponent<AudioSource>();
        currentCookingSource.clip = clip;
        currentCookingSource.volume = volume;
        currentCookingSource.loop = loop;
        currentCookingSource.spatialBlend = 0f;
        currentCookingSource.outputAudioMixerGroup = sfxMixerGroup;
        currentCookingSource.Play();

        if (!loop)
            Destroy(tempObj, clip.length + 0.1f);
    }

    public void StopCookingSound()
    {
        if (currentCookingSource != null)
        {
            currentCookingSource.Stop();
            Destroy(currentCookingSource.gameObject);
            currentCookingSource = null;
        }
    }

    // 위치 기반 3D 요리 사운드 (예: 주방 근처)
    public void PlayCookingAtPoint(string type, Vector3 pos, float volume = 0.8f)
    {
        if (!clipDict.ContainsKey(type)) return;

        GameObject temp = new GameObject($"KitchenSFX_{type}");
        temp.transform.position = pos;

        AudioSource source = temp.AddComponent<AudioSource>();
        source.clip = clipDict[type];
        source.volume = volume;
        source.spatialBlend = 1f;
        source.outputAudioMixerGroup = sfxMixerGroup;
        source.Play();

        Destroy(temp, clipDict[type].length + 0.1f);
    }
}