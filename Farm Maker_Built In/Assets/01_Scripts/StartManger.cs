using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;


// 1. Sprite와 AudioClip을 한 쌍으로 묶는 클래스 정의 
[System.Serializable] // 유니티 인스펙터 창에 노출되도록 설정
public class SlideStartAudioPair
{
    [Tooltip("소리를 재생할 특정 Sprite 이미지 파일을 연결하세요.")]
    public Sprite targetSprite;

    [Tooltip("targetSprite가 표시될 때 재생할 소리 클립입니다.")]
    public AudioClip soundClip;
}


public class StartManger : MonoBehaviour
{
    // 1. 기존 UI 및 데이터 변수
    [Header("UI 연결")]
    public UnityEngine.UI.Image displayImage;
    public GameObject presentationPanel;

    [Header("이미지 설정")]
    [Tooltip("프리젠테이션에 사용할 모든 Sprite 목록입니다.")]
    public Sprite[] slides;

    private int currentSlideIndex = 0;

    // 2. 오디오 관련 변수 수정: 1:1 배열 대신 맵(Map) 역할을 할 새 배열 사용 
    [Header("오디오 설정")]
    [Tooltip("소리를 재생할 AudioSource 컴포넌트를 연결하세요.")]
    public AudioSource audioSource;

    // 정의된 SlideStartAudioPair 클래스를 사용하여 Sprite-Audio 쌍을 연결합니다.
    [Tooltip("특정 Sprite와 소리 클립을 연결하는 목록입니다. 이미지 순서와 상관 없습니다.")]
    public SlideStartAudioPair[] slideAudioMap;

    // 3. 새로운 기능 추가 : 마지막 슬라이드 후 활성화할 오브젝트 
    [Header("최종 애니메이션")]
    [Tooltip("프리젠테이션이 끝난 후 활성화할 애니메이션 GameObject를 연결하세요.")]
    public GameObject finalSceneObject;




    // 4. 초기화 함수
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

        // 시작 시 최종 오브젝트 비활성화 확인 (애니메이션이 바로 실행되는 것을 방지) 
        if (finalSceneObject != null && finalSceneObject.activeSelf)
        {
            finalSceneObject.SetActive(false);
        }
    }

    // 5. 매 프레임 입력 감지 및 업데이트

    void Update()
    {
        // 이미지 창이 켜져 있을 때만 슬라이드 전환을 허용
        if (presentationPanel != null && presentationPanel.activeSelf)
        {
            // 'enter' 키를 눌러 다음 이미지로 이동
            if (Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                GoToNextSlide();
            }
        }
    }

    // 6. 슬라이드 전환 기능 (핵심 로직 수정)

    void GoToNextSlide()
    {
        // A. 이미 마지막 슬라이드에 도달했는데 또 엔터 키를 누른 경우 (종료 시점) 
        // 현재 인덱스가 slides.Length - 1, 즉 마지막 슬라이드에 있을 때 다음을 누르면
        if (currentSlideIndex == slides.Length - 1)
        {
            // 1. 최종 애니메이션 오브젝트 활성화
            if (finalSceneObject != null)
            {
                finalSceneObject.SetActive(true);
                Debug.Log("프리젠테이션 종료: 최종 애니메이션 오브젝트가 활성화되었습니다.");
            }

            // 2. 프리젠테이션 패널 비활성화 (슬라이드 전환 정지 및 화면 정리)
            if (presentationPanel != null)
            {
                presentationPanel.SetActive(false);
            }

            // 더 이상 슬라이드가 넘어가지 않도록 함수를 여기서 종료합니다.
            return;
        }

        // B. 일반적인 슬라이드 전환
        currentSlideIndex++; // 현재 인덱스를 1 증가

        // 새로운 인덱스에 맞는 이미지를 표시
        UpdateSlideDisplay();
    }

    // 7. 이미지 표시 및 오디오 재생 업데이트 (중복 코드를 줄이는 헬퍼 함수)

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
            foreach (SlideStartAudioPair pair in slideAudioMap)
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
}
