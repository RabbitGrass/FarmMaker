using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroUIManager : MonoBehaviour
{
    public static IntroUIManager Instance;

    private Animator titleAnimator;  // TitleImage에 붙은 Animator
    public Image title; // TitleImage UI
    public GameObject subTitle; // start, option, exit image 오브젝트의 부모(빈오브젝트)

    void Awake()
    {
        // 싱글톤: 한 개만 존재
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // 씬 넘어가도 유지
        }
        else
        {
            Destroy(gameObject); // 중복 생성 방지
        }

        // Title 연결 여부 확인
        if (title == null)
        {
            Debug.LogError(" Title Image가 연결되지 않았습니다. Inspector에서 지정해주세요!");
            return;
        }

        titleAnimator = title.GetComponent<Animator>();

        if (titleAnimator == null)
        {
            Debug.LogError(" TitleImage 오브젝트에 Animator가 없습니다!");
            return;
        }

    }

    void Start()
    {
        StartCoroutine(PlayAndStop());
    }

    IEnumerator PlayAndStop()
    {
        //게임 시작후 x초 동안 대기 후 타이틀 표시
        yield return new WaitForSeconds(0.2f);
        title.gameObject.SetActive(true);        

        //  Animator 트리거 발동
        titleAnimator.SetTrigger("TitleFadeIn");

        //  애니메이션이 실제 길이보다 살짝 여유있게 대기
        yield return new WaitForSeconds(1f);

        // 정지
        titleAnimator.enabled = false;
        //Debug.Log("Title 애니메이션 재생 완료 후 정지됨 (하드코딩)");

        StartCoroutine(ShakeAfterDrop());

        yield return new WaitForSeconds(2.5f); // 흔들리는 시간

        

        subTitle.SetActive(true);
    }

    IEnumerator ShakeAfterDrop()
    {
        float duration = 2f;                 // 총 흔들리는 시간
        float elapsed = 0f;
        float frequency = 3f;               // 진동 속도
        float initialAmplitude = 20f;       // 시작 진폭

        Vector2 origin = title.rectTransform.anchoredPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float normalizedTime = elapsed / duration;        // 0 → 1
            float damping = 1f - normalizedTime;              // 감쇠율 (1 → 0)
            float amplitude = initialAmplitude * damping;     // 점점 줄어드는 진폭

            float yOffset = Mathf.Sin(elapsed * frequency * Mathf.PI * 2f) * amplitude;

            title.rectTransform.anchoredPosition = origin + new Vector2(0, yOffset);

            yield return null;
        }

        title.rectTransform.anchoredPosition = origin;
    }

    public void OnClickStart()
    {
        Debug.Log("게임 시작 버튼 클릭됨!");
        IntroUISoundManager.Instance?.PlayClick(0.4f);

        // 로드씬 효과 넣고 싶다면 여기에 코루틴으로 추가 가능
        StartCoroutine(LoadMainScene());
    }

    IEnumerator LoadMainScene()
    {
        yield return new WaitForSeconds(0.5f); // 클릭음 조금 재생되게 약간 대기
        SceneManager.LoadScene("Main"); // 씬 이름 정확히 일치시켜야 함
    }

}
