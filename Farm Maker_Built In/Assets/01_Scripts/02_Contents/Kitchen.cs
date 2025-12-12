using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Kitchen : MonoBehaviour, ITimer
{
    #region 싱글톤(임시, 이후 경우에 따라 삭제 예정)
    public static Kitchen cook = null;
    private void Awake()
    {
        if (cook == null) cook = this;
    }
    #endregion

    #region 변수 모음
    // 정보 UI
    public GameObject information;

    // 요리 시작 메뉴
    public GameObject cooker;

    // 모드
    public GameObject[] cookingMode;

    // 제작 창 변수
    public GameObject[] cookingMenu;

    // 요리 제작이 가능한가 불가능한가의 여부를 알립니다.
    public bool isReadyCooking;

    // 요리의 인덱스 번호입니다.
    public int currentCooking = -1;

    // 요리용 불
    public GameObject fire;
    public GameObject firePoint;

    private TimeViewer timeView;

    #endregion

    #region 요리 상태 모음
    public enum CookingState
    {
        Cool,
        Empty,
        Cook,
        Complete
    }

    public CookingState cookState;

    public float cookTime = 5f;
    private float currentCook = 0f;
    private bool isCooking = false;

    public float TotalTime => cookTime;
    public float CurrentTime => currentCook;
    public bool IsRunning => isCooking;
    #endregion

    private void Start()
    {
        HideCookers();

        timeView = GetComponentInChildren<TimeViewer>();
        if (timeView != null) print("연결 완료!");
    }

    #region 범위 관리
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) CookerAvailable();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            switch(cookState)
            {
                case CookingState.Cool:
                case CookingState.Empty:
                    information.SetActive(false);
                    break;
                case CookingState.Cook:
                case CookingState.Complete:
                    information.SetActive(true);
                    break;

            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) CookerUnavailable();
    }
    #endregion

    #region 요리 가능 여부
    private void CookerAvailable()
    {
        isReadyCooking = true;
        // print("상우의 가마솥: 요리할 거면 재료 가져오셈!");
        //cookerOpenUI.SetActive(true);
    }

    private void CookerUnavailable()
    {
        isReadyCooking = false;
        information.SetActive(false);
        // print("상우의 가마솥: 바이바이~");
        //cookerOpenUI.SetActive(false);
    }
    #endregion

    #region 불 피우기

    // 불 피우기
    public void Fire()
    {
        // 임시 코드: 일단은 Cool 상태이고 F를 누르기만 하면 되는데, 이거 수정할 겁니다. 몇시에? 2025년 10월 29일 20시 30분 경에!!
        if (cookState == CookingState.Cool)
        {
            if(FlintAndFire()) StartCoroutine(StartBurn());
        }
    }

    // 부싯돌과 나무의 데이터가 있을 때 >> 내일 인벤토리 쪽과 한번 연동
    public bool FlintAndFire()
    {
        if (ToInventoryBox.ToInventory.UseItemAndDelete("Flint", 1) && ToInventoryBox.ToInventory.UseItemAndDelete("Wood", 3)) return true;
        else return false;
    }

    // 불의 생명주기
    IEnumerator StartBurn()
    {
        // 1초 대기
        yield return new WaitForSecondsRealtime(1f);

        // 불 이펙트 소환하고 상태 Empty로 바꾸기
        fire.SetActive(true);
        firePoint.SetActive(true);
        cookState = CookingState.Empty;

        // ** 장작 소리 ON**
        KitchenSoundManager.Instance.PlayFireLoop();

        // 5분 대기(조)
        yield return new WaitForSecondsRealtime(500f);

        // 불 없애고 상태 바꾸기
        fire.SetActive(false);
        firePoint.SetActive(false);
        cookState = CookingState.Cool;

        // ** 장작 소리 OFF**
        KitchenSoundManager.Instance.StopFireLoop();
    }

    #endregion

    #region 요리 만들기

    // 빵
    public void MakeBread()
    {
        // 뚜껑 닫기
        ClosingPotHead(0);

        // 상태 변화
        cookState = CookingState.Cook;
        currentCooking = 0;

        // 빵 재료: 밀 3개 >>> 없으면 못해먹음 ㅅㄱ
        if (Refrigerator.Food.UseIngredient("Flour", 3))
        {
            print("빵 굽는 중...");

            StartTimer();

            if (timeView != null)
            {
                timeView.UpdateCropName("Bread");
                timeView.Show();
            }

            StartCoroutine(BakeBread());
            
            // 소리 추가
            KitchenSoundManager.Instance.PlayCookingSound("bakeBread", false, 1f);
        }
        else
        {
            print("상우의 가마솥: 밀가루 3개 가져오셈!");
            cookState = CookingState.Empty;
        }
    }

    // 오믈렛
    public void MakeOmelet()
    {
        // 뚜껑 닫기
        ClosingPotHead(1);

        // 상태 변화
        cookState = CookingState.Cook;
        currentCooking = 1;

        // 오믈렛 재료: 토마토 1개 + 계란 2개 >>> 없으면 못해먹음 ㅅㄱ
        if (Refrigerator.Food.UseIngredient("Ketchup", 1) && Refrigerator.Food.UseLivestock("Egg", 2))
        {

            print("오믈렛 부치는 중...");

            StartTimer();

            if (timeView != null)
            {
                timeView.UpdateCropName("Omelet");
                timeView.Show();
            }

            StartCoroutine(CookingTime(1));

            // 소리 추가
            KitchenSoundManager.Instance.PlayCookingSound("omelette", false, 1f);
        }
        else
        {
            print("상우의 가마솥: 케첩 1개랑 계란 2개 가져오셈!");
            cookState = CookingState.Empty;
        }
    }

    // 호박파이
    public void MakePumpkinPie()
    {
        // 뚜껑 닫기
        ClosingPotHead(2);

        // 상태 변화
        cookState = CookingState.Cook;
        currentCooking = 2;

        // 호박파이 재료: 밀 2개 + 호박 1개 + 계란 1개 >>> 없으면 못해먹음 ㅅㄱ
        if (Refrigerator.Food.UseCrop("Wheat", 2) && Refrigerator.Food.UseCrop("Pumpkin", 1) && Refrigerator.Food.UseLivestock("Egg", 1))
        {

            print("호박파이 만드는 중...");

            StartTimer();

            if (timeView != null)
            {
                timeView.UpdateCropName("PumpkinPie");
                timeView.Show();
            }

            StartCoroutine(CookingTime(2));

            // 소리 추가
            KitchenSoundManager.Instance.PlayCookingSound("pie", false, 1f);
        }
        else
        {
            print("상우의 가마솥: 밀가루 2개랑 호박 1개랑 계란 1개 가져오셈!");
            cookState = CookingState.Empty;
        }
    }

    #endregion

    #region 타이머 및 수령 여부 설정
    // 빵굽는 시간
    IEnumerator BakeBread()
    {
        yield return new WaitUntil(() => cookState == CookingState.Complete);
        cookState = CookingState.Complete;

        // 소리 추가
        KitchenSoundManager.Instance.StopCookingSound();
        KitchenSoundManager.Instance.PlayCookingSound("successClip");

        print("상우의 가마솥: 빵 다 만들었음!");
    }

    // 그 외 요리 시간
    IEnumerator CookingTime(int n)
    {
        yield return new WaitForSecondsRealtime(5f);
        cookState = CookingState.Complete;

        // 소리 추가
        KitchenSoundManager.Instance.StopCookingSound();
        KitchenSoundManager.Instance.PlayCookingSound("successClip");

        switch (n)
        {
            case 1: print("상우의 가마솥: 오믈렛 다 부쳤음!"); break;
            case 2: print("상우의 가마솥: 호박파이 다 구웠음!"); break;
            default: print("상우의 가마솥: 요리다운 요리를 만드셈!"); break;
        }
    }

    // 요리 수령
    public void TakeFood(int n)  //요리 생성 !!
    {
        switch (n)
        {
            case 0: Refrigerator.Food.PlusFood(0); break; // 빵
            case 1: Refrigerator.Food.PlusFood(1); break; // 오믈렛
            case 2: Refrigerator.Food.PlusFood(2); break; // 파이
        }

        print("상우의 가마솥: 맛있게 먹으셈!");
        cookState = CookingState.Empty;
    }
    #endregion

    #region UI 관리

    private void HideCookers()
    {
        isReadyCooking = false;
        cookState = CookingState.Cool;
        fire.SetActive(false);
        firePoint.SetActive(false);

        //cookerOpenUI.SetActive(false);
        information.SetActive(false);
        cooker.SetActive(false);
        for (int i = 0; i <cookingMode.Length; i++) cookingMode[i].SetActive(false);
        for (int i = 0; i < cookingMenu.Length; i++) cookingMenu[i].SetActive(false);
    }

    public void ClosingPotHead(int n)
    {
        cookingMenu[n].SetActive(false);
    }

    public void StartTimer()
    {
        if (!isCooking)
        {
            isCooking = true;
            StartCoroutine(CookingTimer());
            timeView?.Show();
        }
    }

    public void StopTimer()
    {
        isCooking = false;
    }

    public void ResetTimer()
    {
        currentCook = 0f;
    }

    public void Complete()
    {
        isCooking = false;
        cookState = CookingState.Complete;
        timeView?.Hide();
        Debug.Log("요리가 완성되었습니다!");
    }

    private IEnumerator CookingTimer()
    {
        while (isCooking && currentCook < cookTime)
        {
            currentCook += Time.deltaTime;
            // UIManager.instance.UpdateProgressBar(this);
            yield return null;
        }

        if (currentCook >= cookTime)
            Complete();
    }

    #endregion
}
