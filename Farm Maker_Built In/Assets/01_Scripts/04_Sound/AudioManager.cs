using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("UI Sliders")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    private void Start()
    {
        if (audioMixer == null)
            Debug.LogError("[AudioManager] ❌ AudioMixer가 연결되지 않았습니다!");

        if (bgmSlider == null || sfxSlider == null)
            Debug.LogError("[AudioManager] ❌ 슬라이더가 연결되지 않았습니다!");

        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        Debug.Log("[AudioManager]  AudioManager 초기화 완료");
    }

    public void SetBGMVolume(float value)
    {
        float dB;

        if (value < 1.5f) // 중앙보다 작을 때: -40 → 0
        {
            float t = value / 1.5f;
            dB = Mathf.Lerp(-40f, 0f, t);
        }
        else // 중앙 이상: 0 → +20
        {
            float t = (value - 1.5f) / 1.5f;
            dB = Mathf.Lerp(0f, 20f, t);
        }

        audioMixer.SetFloat("BGM", dB);
        Debug.Log($"[BGM] value={value:F2}, dB={dB:F2}");
    }

    public void SetSFXVolume(float value)
    {
        float dB;

        if (value < 1.5f)
        {
            float t = value / 1.5f;
            dB = Mathf.Lerp(-40f, 0f, t);
        }
        else
        {
            float t = (value - 1.5f) / 1.5f;
            dB = Mathf.Lerp(0f, 20f, t);
        }

        audioMixer.SetFloat("SFX", dB);
    }

    public void ResetToDefault()
    {
        // 기본 볼륨 슬라이더 값
        float defaultValue = 1.5f;

        // 슬라이더 UI를 1.5로 되돌림
        bgmSlider.value = defaultValue;
        sfxSlider.value = defaultValue;

        // 실제 오디오 믹서도 바로 반영
        SetBGMVolume(defaultValue);
        SetSFXVolume(defaultValue);

        Debug.Log("[AudioManager] 🔄 기본 볼륨(1.5)으로 복원 완료");
    }


}