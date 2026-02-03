using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroUIManager : MonoBehaviour
{
    public static IntroUIManager Instance;

    private Animator titleAnimator;
    public Image title;
    public GameObject subTitle;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (title == null)
        {
            Debug.LogError("Title Image가 연결되지 않았습니다.");
            return;
        }

        titleAnimator = title.GetComponent<Animator>();

        if (titleAnimator == null)
        {
            Debug.LogError("TitleImage 오브젝트에 Animator가 없습니다!");
            return;
        }
    }

    void Start()
    {
        StartIntro();
    }

    // 🔹 인트로 시작 (Animator만 건드림)
    void StartIntro()
    {
        title.gameObject.SetActive(true);
        titleAnimator.SetTrigger("TitleFadeIn");
    }

    // =========================
    // 🎬 Animation Events
    // =========================

    // 📌 낙하 거의 끝났을 때 호출
    // → Animation Event로 연결
    public void AE_StartShake()
    {
        titleAnimator.enabled = false;
        StartCoroutine(ShakeAfterDrop());
    }

    // 📌 흔들기 종료 후 호출
    // → Animation Event 또는 코루틴에서 호출
    public void AE_ShowSubTitle()
    {
        subTitle.SetActive(true);
    }

    // =========================
    // 흔들림 코루틴
    // =========================

    IEnumerator ShakeAfterDrop()
    {
        float duration = 2f;
        float elapsed = 0f;
        float frequency = 3f;
        float initialAmplitude = 20f;

        RectTransform rect = title.rectTransform;
        Vector2 origin = rect.anchoredPosition;

        while (elapsed < duration)
        {
            Debug.Log(title.rectTransform.anchoredPosition.y);
            Debug.Log(title.rectTransform.anchoredPosition);
            elapsed += Time.deltaTime;

            float t = elapsed / duration;
            float damping = 1f - t;
            float amplitude = initialAmplitude * damping;

            float yOffset = Mathf.Sin(elapsed * frequency * Mathf.PI * 2f) * amplitude;
            rect.anchoredPosition = origin + new Vector2(0, yOffset);

            yield return null;
        }

        rect.anchoredPosition = origin;

        // 흔들림 종료 → 부제목 표시
        AE_ShowSubTitle();
    }

    // =========================
    // 버튼
    // =========================

    public void OnClickStart()
    {
        IntroUISoundManager.Instance?.PlayClick(0.4f);
        StartCoroutine(LoadMainScene());
    }

    IEnumerator LoadMainScene()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene("Main");
    }
}
