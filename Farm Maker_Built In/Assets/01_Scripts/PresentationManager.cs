using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;


// 1. Sprite와 AudioClip을 한 쌍으로 묶는 클래스 정의 
[System.Serializable] // 유니티 인스펙터 창에 노출되도록 설정
public class SlideAudioPair
{
    [Tooltip("소리를 재생할 특정 Sprite 이미지 파일을 연결하세요.")]
    public Sprite targetSprite;

    [Tooltip("targetSprite가 표시될 때 재생할 소리 클립입니다.")]
    public AudioClip soundClip;
}


public class PresentationManager : MonoBehaviour
{
    // 1. 기존 UI 및 데이터 변수
    [Header("UI 연결")]
    public UnityEngine.UI.Image displayImage;
    public GameObject presentationPanel;

    [Header("이미지 설정")]
    public Sprite[] slides;

    private int currentSlideIndex = 0;

    // 2. 오디오 관련 변수 수정: 1:1 배열 대신 맵(Map) 역할을 할 새 배열 사용 
    [Header("오디오 설정")]
    [Tooltip("소리를 재생할 AudioSource 컴포넌트를 연결하세요.")]
    public AudioSource audioSource;

    [Tooltip("특정 Sprite와 소리 클립을 연결하는 목록입니다. 이미지 순서와 상관 없습니다.")]
    public SlideAudioPair[] slideAudioMap;


    // 2. 초기화 함수
    void Start()
    {
        // AudioSource 컴포넌트가 연결되어 있지 않다면, 이 오브젝트에서 찾음
        if (audioSource == null)
        {
            // 만약 오브젝트에 AudioSource가 없다면, 콘솔에 경고를 출력
            if (!TryGetComponent<AudioSource>(out audioSource))
            {
                Debug.LogWarning("PresentationManager: AudioSource가 연결되지 않았으며, 오브젝트에서도 찾을 수 없습니다. 소리 재생 기능이 작동하지 않습니다.");
            }
        }

        // 씬이 시작될 때, 첫 번째 이미지(인덱스 0)를 표시하고 오디오를 재생합니다.
        if (slides.Length > 0 && displayImage != null)
        {
            UpdateSlideDisplay();
        }

        if (presentationPanel != null)
        {
            presentationPanel.SetActive(true);
        }
    }

    // 3. 매 프레임 입력 감지 및 업데이트
    void Update()
    {
        // '/' 키를 눌렀는지 확인하고, 이미지 창을 켜거나 끔
        if (Input.GetKeyDown(KeyCode.Slash))
        {
            TogglePresentationPanel();
        }

        // 이미지 창이 켜져 있을 때만 슬라이드 전환을 허용
        if (presentationPanel != null && presentationPanel.activeSelf)
        {
            // '.' 키를 눌러 다음 이미지로 이동
            if (Input.GetKeyDown(KeyCode.Period))
            {
                GoToNextSlide();
            }

            // ',' 키를 눌러 이전 이미지로 이동
            if (Input.GetKeyDown(KeyCode.Comma))
            {
                GoToPreviousSlide();
            }
        }
    }

    // 4. 슬라이드 전환 기능

    void GoToNextSlide()
    {
        // 현재 인덱스를 1 증가
        currentSlideIndex++;

        // 인덱스가 전체 슬라이드 개수를 초과하면, 다시 첫 번째(0)로 돌아감
        if (currentSlideIndex >= slides.Length)
        {
            currentSlideIndex = 0; // 루프 (반복)
        }

        // 새로운 인덱스에 맞는 이미지를 표시
        UpdateSlideDisplay();
    }

    void GoToPreviousSlide()
    {
        // 현재 인덱스를 1 감소
        currentSlideIndex--;

        // 인덱스가 0보다 작아지면, 가장 마지막 슬라이드로 이동
        if (currentSlideIndex < 0)
        {
            currentSlideIndex = slides.Length - 1; // 루프 (반복)
        }

        // 새로운 인덱스에 맞는 이미지를 표시
        UpdateSlideDisplay();
    }


    // 5. 이미지 표시 및 오디오 재생 업데이트 (중복 코드를 줄이는 헬퍼 함수)
    void UpdateSlideDisplay()
    {
        if (slides.Length == 0 || displayImage == null) return;

        // 1. 현재 표시할 Sprite를 가져옵니다.
        Sprite currentSprite = slides[currentSlideIndex];

        // 2. 이미지 업데이트
        displayImage.sprite = currentSprite;

        // 3. 오디오 재생 로직 
        if (audioSource != null && slideAudioMap != null)
        {
            // slideAudioMap 배열을 순회하며 현재 Sprite와 일치하는 소리 클립을 찾습니다.
            foreach (SlideAudioPair pair in slideAudioMap)
            {
                // 현재 Sprite(currentSprite)가 맵에 등록된 targetSprite와 동일하다면
                if (pair.targetSprite == currentSprite && pair.soundClip != null)
                {
                    // 소리를 재생합니다.
                    audioSource.PlayOneShot(pair.soundClip);

                    // 소리를 찾았으므로 함수를 즉시 종료합니다.
                    return;
                }
            }
        }
    }

    // 6. 패널 켜기/끄기 기능
    void TogglePresentationPanel()
    {
        if (presentationPanel != null)
        {
            // 현재 활성화 상태의 반대 상태로 설정(True ↔ False).
            bool isCurrentlyActive = presentationPanel.activeSelf;
            presentationPanel.SetActive(!isCurrentlyActive);
        }
    }
}
