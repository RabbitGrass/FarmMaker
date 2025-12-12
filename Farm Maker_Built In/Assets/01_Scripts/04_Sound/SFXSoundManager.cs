using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

public class SFXSoundManager : MonoBehaviour
{
    //싱글톤
    public static SFXSoundManager Instance;

    // 실제로 소리를 재생할 AudioSource
    public AudioSource sfxSource;

    // AudioMixer 그룹 (효과음 볼륨 조절용)
    public AudioMixerGroup sfxMixerGroup;

    private AudioSource treeWindSource; // 루프 사운드용

    //효과음
    public AudioClip treeChop; // 나무 도끼질 소리
    public AudioClip treeFall; // 나무 쓰러지는 소리
    public AudioClip rockDamage; // 바위 깨는 소리
    public AudioClip rockBreak; // 바위 깨지는 소리
    public AudioClip footstep_water; // 물위를 걷는 소리
    public AudioClip footstep_run; // 평지 달리는 소리
    public AudioClip footstep_walk; // 평지 걷는 소리
    public AudioClip jump; // 평지 걷는 소리
    public AudioClip MorningChicken; // 아침 닭울음소리
    public AudioClip Nightwolf; // 아침 닭울음소리
    public AudioClip ItemPickUp; // 아이템 획득
    public AudioClip ItemDrop; // 아이템 버리기
    public AudioClip ItemHand; // 아이템 손에 들기
    public AudioClip treeWindClip; // 바람에 흔들리는 나무
    public AudioClip playerEatClip; // 내가 음식을 먹는 소리
    public AudioClip animalEatClip; // 동물이 음식을 먹는 소리
    public AudioClip friendLevelClip; // 동물 호감도 올라가는 소리; 
    public AudioClip friendShipClip; // 동물 호감도 올라가는 소리; 
    public AudioClip hungryClip; // 배고픔 소리
    public AudioClip rakeWorkClip; // 밭가는 소리


    // 문자열 키를 이용해 AudioClip을 빠르게 찾아 쓸 수 있도록 관리하는 딕셔너리
    private Dictionary<string, AudioClip> clipDict = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        //나무에 스치는 바람소리 관련
        treeWindSource = gameObject.AddComponent<AudioSource>();
        treeWindSource.playOnAwake = false;
        treeWindSource.loop = true;
        treeWindSource.spatialBlend = 0f; // 3D로 하고 싶으면 1f
        treeWindSource.outputAudioMixerGroup = sfxMixerGroup;

        //싱글톤 패턴
        if (Instance == null)
        {
            Instance = this; // 첫 번째 생성된 인스턴스를 지정
            DontDestroyOnLoad(gameObject);  // 씬이 바뀌어도 파괴되지 않게 유지

            // AudioSource 없으면 자동 추가
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.spatialBlend = 0f;
            }

            // AudioMixer 그룹이 지정되어 있으면 AudioSource에 연결
            if (sfxMixerGroup != null)
            {
                sfxSource.outputAudioMixerGroup = sfxMixerGroup;

                // 효과음들을 딕셔너리에 등록 (문자열 키로 호출할 수 있도록)
                clipDict["treeChop"] = treeChop; // 나무 베기
                clipDict["treeFall"] = treeFall; // 나무 쓰러짐
                clipDict["rockDamage"] = rockDamage; // 돌 깨기
                clipDict["rockBreak"] = rockBreak; // 돌 부서짐
                clipDict["footstep_water"] = footstep_water; // 돌 부서짐
                clipDict["footstep_run"] = footstep_run; // 평지 달리는 소리
                clipDict["footstep_walk"] = footstep_walk; // 평지 걷는 소리
                clipDict["jump"] = jump; // 평지 걷는 소리
                clipDict["MorningChicken"] = MorningChicken; // 아침 닭울음소리
                clipDict["Nightwolf"] = Nightwolf; // 저녁 늑대 울음소리
                clipDict["ItemPickUp"] = ItemPickUp; // 아이템 획득
                clipDict["ItemDrop"] = ItemDrop; // 아이템 획득
                clipDict["ItemHand"] = ItemHand; // 아이템 손에들기
                clipDict["treeWindClip"] = treeWindClip; // 바람에 흔들리는 나무
                clipDict["playerEatClip"] = playerEatClip; //  내가 음식을 먹는 소리
                clipDict["animalEatClip"] = animalEatClip; // 동물이 음식을 먹는 소리
                clipDict["friendLevel"] = friendLevelClip; // 동물 호감도 올라가는 소리
                clipDict["friendShip"] = friendShipClip; // 동물과 친구가 된 소리
                clipDict["hungry"] = hungryClip; //배고픔소리
                clipDict["rakeWork"] = rakeWorkClip; //밭가는소리
            }
            else
            {
                //  만약 이미 존재하는 매니저가 있다면 새로 만들어진 건 삭제
                Destroy(gameObject);
            }
        }
    }

    //나무에 스치는 바람소리 관련
    public void PlayTreeWind()
    {
        if (treeWindClip == null) return;

        if (!treeWindSource.isPlaying)
        {
            treeWindSource.clip = treeWindClip;
            treeWindSource.Play();
        }
    }
    //나무에 스치는 바람소리 관련
    public void StopTreeWind()
    {
        if (treeWindSource.isPlaying)
        {
            treeWindSource.Stop();
        }
    }

    /// <summary>
    /// 문자열 키로 지정된 사운드를 재생한다 (같은 소스에서 OneShot)
    /// </summary>
    /// <param name="key">사운드 이름 (예: "TreeChop")</param>
    /// <param name="pitchVariation">피치(톤) 랜덤 가변폭 (기본 ±0.1)</param>
    public void Play(string key, float pitchVariation = 0.1f)
    {
        // 키가 등록되어 있는 경우에만 재생
        if (clipDict.ContainsKey(key))
        {
            // 소리의 피치를 약간 랜덤하게 바꿔서 반복음이 자연스럽게 들리게 함
            sfxSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);

            // 지정된 효과음을 OneShot(겹쳐서 재생 가능)으로 재생
            sfxSource.PlayOneShot(clipDict[key]);
        }
        else
        {
            // 등록되지 않은 키로 접근할 경우 경고 메시지 출력
            Debug.LogWarning($"[SFXSoundManager] '{key}' 클립이 등록되지 않았습니다!");
        }
    }

    //음원파일 볼륨조절해서 출력 하기 위해 메서드 오버로드
    public void Play(string name, float volume, float delay = 0f, float pitchVariation = 0.1f)
    {
        if (!clipDict.ContainsKey(name))
        {
            Debug.LogWarning($"'{name}' 클립이 등록되지 않았습니다!");
            return;
        }

        StartCoroutine(PlayWithDelay(name, volume, delay, pitchVariation));
    }

    private IEnumerator PlayWithDelay(string name, float volume, float delay, float pitchVariation)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        sfxSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        sfxSource.PlayOneShot(clipDict[name], volume);
    }


    /// <summary>
    /// 특정 위치에서 소리를 재생한다 (3D 사운드용)
    /// </summary>
    /// <param name="key">사운드 이름</param>
    /// <param name="pos">재생 위치</param>
    public AudioSource PlayAtPoint(string key, Vector3 pos)
    {
        AudioClip clip = GetClip(key);
        if (clip == null) return null;

        GameObject tempObj = new GameObject("SFX_" + key);
        tempObj.transform.position = pos;

        AudioSource source = tempObj.AddComponent<AudioSource>();
        source.clip = clip;
        source.spatialBlend = 1f;
        source.volume = 0.8f;
        source.outputAudioMixerGroup = sfxMixerGroup; //  소문자
        source.Play();

        Destroy(tempObj, clip.length);
        return source; //  AudioSource를 반환해야 PlayerController에서 추적 가능
    }

    // Aniaml Controller에서 사용하기 위해 오버로드
    public void PlayAtPoint(AudioClip clip, Vector3 pos, float volume = 1f)
    {
        if (clip == null) return;

        GameObject temp = new GameObject("SFX_" + clip.name);
        temp.transform.position = pos;

        AudioSource source = temp.AddComponent<AudioSource>();
        source.clip = clip;
        source.outputAudioMixerGroup = sfxMixerGroup; // 믹서 연결
        source.volume = volume;
        source.spatialBlend = 0.5f; // 3D 사운드   
        source.Play();

        //Destroy(temp, clip.length + 0.1f); // 자동 삭제
        StartCoroutine(DestroyAfterPlay(source, temp));
    }

    //발소리 페이드 아웃 문제를 해결을 위한 코드
    private IEnumerator DestroyAfterPlay(AudioSource source, GameObject obj)
    {
        // clip이 완전히 끝날 때까지 기다림
        yield return new WaitUntil(() => !source.isPlaying);
        Destroy(obj);
    }

    public AudioClip GetClip(string key)
    {
        if (clipDict.TryGetValue(key, out AudioClip clip))
            return clip;
        else
        {
            Debug.LogWarning($"[SFXSoundManager] '{key}' 클립을 찾을 수 없습니다.");
            return null;
        }
    }
}
