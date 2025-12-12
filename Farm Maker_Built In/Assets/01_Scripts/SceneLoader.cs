using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리 기능

public class SceneLoader : MonoBehaviour
{
    [Header("로드할 보조 씬 이름")]
    [Tooltip("추가로 로드할 씬의 정확한 이름 입력)")]
    public string sceneToLoad;


    void Start()
    {
        // 씬 로드 전 빌드 설정에 씬이 등록되어 있는지 확인
        if (IsSceneInBuildSettings(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Additive);
            Debug.Log($"씬 '{sceneToLoad}'을(를) '{SceneManager.GetActiveScene().name}' 씬에 성공적으로 병합했습니다.");
        }
        else
        {
            Debug.LogError($"오류: 씬 '{sceneToLoad}'을(를) 찾을 수 없습니다. File > Build Settings에 추가했는지 확인하세요.");
        }
    }

    // 헬퍼 함수: 씬이 빌드 설정에 등록되어 있는지 확인
    private bool IsSceneInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            // 빌드 경로에서 씬 인덱스 순회, 이름 추출하여 비교
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName)
            {
                return true;
            }
        }
        return false;
    }
}
