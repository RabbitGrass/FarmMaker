using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionSoundSlider : MonoBehaviour
{
    public AudioMixer audioMixer;   // MainAudioMixer 참조
    public string volumeParameter;  // "BGMVolume" 또는 "SFXVolume"
    public Slider slider;

    void Start()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        // 슬라이더 초기값 (저장값 불러오기 가능)
        float savedVolume = PlayerPrefs.GetFloat(volumeParameter, 0.75f);
        slider.value = savedVolume;
        SetVolume(savedVolume);

        // 슬라이더 변경 시 이벤트 등록
        slider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float value)
    {
        // 볼륨을 dB로 변환해서 Mixer에 전달
        float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat(volumeParameter, dB);

        // PlayerPrefs에 저장
        PlayerPrefs.SetFloat(volumeParameter, value);
    }
}