using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingPlace : MonoBehaviour, ITimer
{
    #region 싱글톤
    public static CraftingPlace craft = null;
    private void Awake()
    {
        if (craft == null) craft = this;
    }
    #endregion

    #region 변수 모음

    // 정보 UI
    public GameObject information;

    // 제작 메뉴
    public GameObject craftingTable;

    // 제작 창 변수
    public GameObject[] craftingMenu;

    // 제작이 가능한가 불가능한가의 여부를 알립니다.
    public bool isReady;

    // 인덱스 번호입니다. >> 일단 지금은 밀가루밖에 없습니다.
    public int currentTable = -1;

    // 시간 동기화
    private TimeViewer timeView;

    #endregion

    #region 제작 상태 모음
    public enum CraftState
    {
        Empty,
        Craft,
        Complete
    }

    public CraftState craftState;
    #endregion

    // Refrigerator 클래스 안, 필드 구역 적당한 위치에 추가
    [Header("인벤토리에 넣을 결과 아이템(이름과 매칭)")]
    public Item[] items;

    public float craftTime = 4f;
    private float progress = 0f;
    private bool crafting = false;

    public float TotalTime => craftTime;
    public float CurrentTime => progress;
    public bool IsRunning => crafting;

    // items 순번 : 0-밀가루, 1-건초더미, 2-케찹, 3-모이

    private void Start()
    {
        HideCraftingTable();

        timeView = GetComponentInChildren<TimeViewer>();
    }

    #region 범위 관리
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) CraftAvailable();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && (craftState != CraftState.Empty)) information.SetActive(true);
        else information.SetActive(false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) CraftUnavailable();
    }
    #endregion

    #region 제작 가능 여부
    private void CraftAvailable()
    {
        isReady = true;
        //print("오늘은 어떤 걸 만들러 오셨나요?");
        //craftingOpenUI.SetActive(true);
    }

    private void CraftUnavailable()
    {
        isReady = false;
        information.SetActive(false);
        //print("풍차: 저한테 오세요! 저한테 오라고요! 야 어디가!!!");
        //craftingOpenUI.SetActive(false);
    }
    #endregion

    #region 풍차 제작

    // 탈곡
    public void MakeGrain()
    {
        // 풍차에서 기다리기
        WaitAtWindmill();

        // 상태 변화
        craftState = CraftState.Craft;
        currentTable = 0;

        // 밀가루 제분 재료: 밀 3개
        if (Refrigerator.Food.UseCrop("Wheat", 1))
        {
            print("탈곡 중...");

            StartTimer();
            timeView?.Show();
            timeView?.UpdateCropName("Grain");

            StartCoroutine(CraftingTime(0));
        }
        else
        {
            print("풍차: 재료가 부족하네요~ 재료 빨리 가져와.");
            craftState = CraftState.Empty;
        }
    }

    // 밀가루 제분
    public void MakeFlour()
    {
        // 풍차에서 기다리기
        WaitAtWindmill();

        // 상태 변화
        craftState = CraftState.Craft;
        currentTable = 1;

        // 밀가루 제분 재료: 밀 3개
        if (Refrigerator.Food.UseIngredient("Grain", 1))
        {
            print("밀가루 제분 중...");

            StartTimer();
            timeView?.Show();
            timeView?.UpdateCropName("Flour");

            StartCoroutine(CraftingTime(1));
        }
        else
        {
            print("풍차: 재료가 부족하네요~ 재료 빨리 가져와.");
            craftState = CraftState.Empty;
        }
    }

    // 모이 제작
    public void MakeStockFeed()
    {
        // 풍차에서 기다리기
        WaitAtWindmill();

        // 상태 변화
        craftState = CraftState.Craft;
        currentTable = 2;

        // 모이 재료: 밀가루 1개 + 건초더미 1개
        if (Refrigerator.Food.UseIngredient("Grain", 1) && ToInventoryBox.ToInventory.UseFeed("Hay", 1))
        {
            print("모이 제작 중...");

            StartTimer();
            timeView?.Show();
            timeView?.UpdateCropName("StockFeed");

            StartCoroutine(CraftingTime(2));
        }
        else
        {
            print("풍차: 재료가 부족하네요~ 재료 빨리 가져와.");
            craftState = CraftState.Empty;
        }
    }

    // 케첩 제작
    public void MakeKetchup()
    {
        // 풍차에서 기다리기
        WaitAtWindmill();

        // 상태 변화
        craftState = CraftState.Craft;
        currentTable = 3;

        // 케첩 재료: 토마토 1개
        if (Refrigerator.Food.UseCrop("Tomato", 1))
        {
            print("케첩 제작 중...");

            StartTimer();
            timeView?.Show();
            timeView?.UpdateCropName("Ketchup");

            StartCoroutine(CraftingTime(3));
        }
        else
        {
            print("풍차: 재료가 부족하네요~ 재료 빨리 가져와.");
            craftState = CraftState.Empty;
        }
    }

    #endregion

    #region 제작대 제작

    // 모이 그릇 제작

    #endregion

    #region 타이머와 수령
    // 제작 시간
    IEnumerator CraftingTime(int n)
    {
        yield return new WaitForSecondsRealtime(5f);
        craftState = CraftState.Complete;

        switch (n)
        {
            case 0: print("풍차: 탈곡 다 끝냈어요!"); break;
            case 1: print("풍차: 밀가루 제분 다 끝냈어요!"); break;
            case 2: print("풍차: 아이들 밥 다 만들었어요~"); break;
            case 3: print("풍차: 케첩 다 만들어졌어요!"); break;
            default: print("풍차: 나가"); break;
        }
    }

    // 제작 수령
    public void TakeStuff(int n)  // 여기서 아이템 인벤토리로 이동하게!! 
    {
        Item itemToAdd = null;

        switch (n)
        {
            case 0:
                Refrigerator.Food.PlusIngredient(0);
                ToInventoryBox.ToInventory.AddFeedToInventory("Hay");
               
                // ✅ 수정된 코드: 두 아이템 모두 인벤토리에 추가
                if (InventoryManager.inventory != null)
                {
                    if (items[0] != null)
                    {
                        InventoryManager.inventory.AcquireItem(items[0], 3);
                        Debug.Log($"[Crafting] '{items[0].itemName}' 인벤토리에 추가 완료!");
                    }
                    if (items[1] != null)
                    {
                        InventoryManager.inventory.AcquireItem(items[1], 1);
                        Debug.Log($"[Crafting] '{items[1].itemName}' 인벤토리에 추가 완료!");
                    }
                }
                else
                {
                    Debug.LogWarning("[Crafting] 인벤토리 추가 실패: InventoryManager가 null");
                }
                break;

            case 1:
                Refrigerator.Food.PlusIngredient(0);
                itemToAdd = items[2];
                break;
            case 2:
                ToInventoryBox.ToInventory.AddFeedToInventory("StockFeed");
                itemToAdd = items[3];
                break;
            case 3:
                Refrigerator.Food.PlusIngredient(1);
                itemToAdd = items[4];
                break;
            default:
                print("아직 이 세계에는 존재하지 않는 아이템입니다.");
                break;
        }

        // ✅ 인벤토리로 추가
        if (itemToAdd != null && InventoryManager.inventory != null)
        {
            InventoryManager.inventory.AcquireItem(itemToAdd, 1);
            Debug.Log($"[Crafting] '{itemToAdd.itemName}' 인벤토리에 추가 완료!");
        }
        else
        {
            Debug.LogWarning("[Crafting] 인벤토리 추가 실패: itemToAdd 또는 InventoryManager가 null");
        }

        print("풍차: 모두 완성되었어요!");
        craftState = CraftState.Empty;
    }
    #endregion

    #region UI 관리

    private void HideCraftingTable()
    {
        isReady = false;
        craftState = CraftState.Empty;

        //craftingOpenUI.SetActive(false);
        information.SetActive(false);
        craftingTable.SetActive(false);
        for (int i = 0; i < craftingMenu.Length; i++)
        {
            craftingMenu[i].SetActive(false);
        }
    }

    public void WaitAtWindmill()
    {
        craftingTable.SetActive(false);
    }

    public void StartTimer()
    {
        if (!crafting)
        {
            crafting = true;
            StartCoroutine(CraftingTimer());
        }
    }

    public void StopTimer()
    {
        crafting = false;
    }

    public void ResetTimer()
    {
        progress = 0f;
    }

    public void Complete()
    {
        crafting = false;
        craftState = CraftState.Complete;
        timeView?.Hide();
        Debug.Log("제작 완료!");
    }

    private IEnumerator CraftingTimer()
    {
        while (crafting && progress < craftTime)
        {
            progress += Time.deltaTime;
            yield return null;
        }

        if (progress >= craftTime)
            Complete();
    }

    #endregion
}
