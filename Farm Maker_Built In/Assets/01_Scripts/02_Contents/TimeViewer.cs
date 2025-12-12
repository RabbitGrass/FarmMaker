using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using static HarvestManager;
using static Kitchen;

public class TimeViewer : MonoBehaviour
{
    private ITimer target;

    [Header("UI 구성 요소")]
    public Slider progressBar;
    public Text timerText;
    public Canvas canvas;
    public Text cropName;

    private void Awake()
    {
        if (target == null)
        {
            target = GetComponentInParent<ITimer>();
            if (target != null)
                Debug.Log($"[TimeViewer] 부모 ITimer 연결 성공 → {target}");
            else
                Debug.LogWarning("[TimeViewer] 부모 ITimer를 찾지 못했습니다.");
        }
    }

    public void SetTarget(ITimer newTarget)
    {
        target = newTarget;
    }

    public void Show()
    {
        if (canvas != null)
            canvas.enabled = true;
    }

    public void Hide()
    {
        if (canvas != null)
            canvas.enabled = false;
    }

    private void Update()
    {
        if (target == null || !target.IsRunning) return;

        float ratio = target.CurrentTime / target.TotalTime;
        progressBar.value = ratio;

        if (timerText != null)
        {
            float remain = Mathf.Max(0, target.TotalTime - target.CurrentTime);
            string message = GetTimerMessage(ratio, remain);
            timerText.text = message;
        }
    }

    private string GetTimerMessage(float ratio, float remain)
    {
        switch(target)
        {
            case HarvestManager:
                if (ratio < 0.25f) return "씨앗이 싹트는 중...";
                else if (ratio < 0.5f) return "잘 자라는 중...";
                else if (ratio < 0.75f) return "거의 다 자라남";
                else return "수확 가능!";

            case Kitchen:
                return "";

            case CraftingPlace:
                if (ratio < 0.25f) return "제작을 시작했습니다...";
                else if (ratio < 0.75f) return "제작이 진행 중입니다...";
                else return "제품이 제작됩니다!";

            default:
                return "응 시간 안 알려줘ㅋㅋㅋ";
        }
    }

    public void UpdateCropName(string name)
    {
        if (cropName != null)
        {
            switch(name)
            {
                case "WheatSeed": cropName.text = "밀"; break;
                case "WoodberrySeed": cropName.text = "토마토"; break;
                case "PumpkinSeed": cropName.text = "호박"; break;
                default: cropName.text = "제작 중..."; break;
            }
        }
    }
}
