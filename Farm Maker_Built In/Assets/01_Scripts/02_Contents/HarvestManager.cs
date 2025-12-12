using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

#region 농작물 오브젝트 풀을 만들기 위한 조건
[System.Serializable]
public class CropStages
{
    // 작물의 이름
    public string cropName;
    public GameObject[] stageObjects;
}
#endregion

public class HarvestManager : MonoBehaviour, ITimer
{
    // 1. Public Enum, Veriables
    #region 상태 정보
    // 경작 상태 정보
    public enum HarvestState
    {
        Wasteland,
        None,
        Cultivated,
        Prepared,
        Planted,
        Harvesting
    }

    // 작물 상태 정보
    public enum CropState
    {
        Seed,
        Sprout,
        NeedWater,
        Growing,
        SemiHarvestReady,
        HarvestReady
    }
    #endregion

    #region 경작 관련 변수들
    // 현재 경작 상태
    public HarvestState harvestState;

    // 농사를 지을 수 있는가?
    public bool harvestAble;

    // 물을 줄 양동이는 있는가?
    public GameObject bucket;

    // UI 변수
    public GameObject info;

    // 농경지 오브젝트
    public GameObject farmland;

    // 농경지 존재 여부
    public GameObject cultivated;

    //// 상태마다 뜨는 메서드
    //public GameObject cmsg;
    //public GameObject pmsg;
    //public GameObject hmsg;

    // 씨앗 메뉴
    public GameObject plantMenu;
    public string[] seedNames;

    // 관개 상태가 양호한가
    public bool watering;
    #endregion

    #region 작물 관련 변수들
    // 현재 작물 상태
    public CropState cropState;

    // 성장 시간
    public float allGrowthTime;

    // 지금까지 성장한 시간
    private float growthTime;

    // 자라나고 있는가?
    private bool isGrowing;

    // 작물 오브젝트 풀
    public CropStages[] cropPoolSet;

    // 현재 심긴 작물
    public GameObject[] currentCrops;

    // 이 시점에서는 아직 어떠한 작물도 심기지 않았을 것이므로, 일단 심긴작물은 빈 칸으로 남겨놓는다.
    private int currentCropIndex = -1;

    // Refrigerator 클래스 안, 필드 구역 적당한 위치에 추가
    [Header("인벤토리에 넣을 결과 아이템(이름과 매칭)")]
    public Item[] items;

    #endregion

    #region 기타 변수
    // 플레이어 인터랙터 가져오기
    PlayerInterector interact;

    public float TotalTime => allGrowthTime;
    public float CurrentTime => growthTime;
    public bool IsRunning => isGrowing;

    // 타이머 UI
    private TimeViewer timeView;
    #endregion

    // 2. Start, OnTrigger
    #region 시작
    // 일단 시작 시에는 '심기' UI가 활성화되지 않는다.
    private void Start()
    {
        // 처음부터 씨앗 고르기 메뉴가 뜨면 난감하다. 그래서 일단 관련 UI를 비활성화시킨다.
        InfoActiveDisable();

        // 자식의 타이머 컴포넌트를 가져온다.
        timeView = GetComponentInChildren<TimeViewer>();
        //if (timeView != null )
        //    timeView.SetTarget(this);

        // 처음 경작지는 밭도 안 갈린 상태이므로, 상태는 None이다.
        harvestState = HarvestState.None;

        // 각 cropPoolSet마다 풀 초기화
        for (int i = 0; i < cropPoolSet.Length; i++)
        {
            for (int j = 0; j < cropPoolSet[i].stageObjects.Length; j++)
            {
                // 프리팹 인스턴스 생성 (씬에 존재하도록)
                GameObject obj = Instantiate(
                    cropPoolSet[i].stageObjects[j],
                    transform.position,
                    Quaternion.identity,
                    transform
                );

                cropPoolSet[i].stageObjects[j] = obj;
                obj.SetActive(false);
            }
        }

        // 작물 시간이 제대로 적용되었는지 확인한다. >>> 이후 작물에 따라 시간을 다르게 적용시킬 것이다.
        //print($"총 작물 타임 : {allGrowthTime}초");
    }
    #endregion

    #region 온 트리거: 경작 영역 관리
    // 이 영역으로 들어온다면 경작을 할 수 있다.
    private void OnTriggerEnter(Collider other)
    {
        // 만약 경작지로 접근한 오브젝트가 플레이어라면?
        if (other.gameObject.CompareTag("Player"))
        {
            // 경작 여부 UI를 활성화 시킨다.
            InfoActiveEnable();

            // 플레이어는 경작지와 상호작용할 수 있어야 한다.
            interact = other.GetComponent<PlayerInterector>();
        }
    }

    // 이 영역에서 레이크만 가지고 있다면, 언제든 농사르 지을 수 있다.
    private void OnTriggerStay(Collider other)
    {
        // 접근한 객체가 플레이어라면, 일단 플레이어와 상호작용을 하고 레이크의 유무를 따진다.
        if (other.gameObject.CompareTag("Player"))
        {
            interact = other.GetComponent<PlayerInterector>();

            try
            {
                RakeController rake = interact.heldObject?.GetComponent<RakeController>();
            }
            catch
            {
                print("레이크 없어예");
                return;
            }

            if (harvestState == HarvestState.Planted || harvestState == HarvestState.Harvesting)
                info.SetActive(true);
            else
                info.SetActive(false);

            // 빈 손이거나 들고 있는 물건이 레이크가 아니라면, 플레이어는 농사를 지을 수 없게 된다.
            // if (interact.heldObject != null && rake != null) isHoldRake = true;
            // else isHoldRake = false;

            // 만약 들고 있는 게 양동이라면, 아마 우클릭하면 식물이 물을 먹을 겁니다.
            //if (cropState == CropState.NeedWater && (interact.heldObject.name == bucket.name || interact.heldObject.name.StartsWith(bucket.name)))
            //{
            //    if (Input.GetKeyDown(KeyCode.F))
            //    {
            //        //watering = true;
            //        print("아학 커허헉 크흑!!(식물이 물을 먹는 소리입니다.)");
            //    }
            //}
        // 2단계. 마우스 우클릭을 할 때마다 경작 여부를 조정할 수 있다.
       
            // 밭을 갈 수 있고, 레이크를 들고 있는 상태라면 경작을 지을 수 있다.
            //if (harvestAble)
            //{
            //    switch (harvestState)
            //    {
            //        case HarvestState.None: Cultivate(); break;
            //        case HarvestState.Cultivated: PlantIdle(); break;
            //        case HarvestState.Prepared: ViewPlantMenu(); break;
            //        case HarvestState.Harvesting: HarvestCrop(); break;
            //        default: print("기다려봐요 좀"); break;
            //    }
            //}
        }

        
    }

    // 이 영역에서 나가면 플레이어는 농사를 지을 수 없다.
    private void OnTriggerExit(Collider other)
    {
        InfoActiveDisable();
    }
    #endregion

    // 3. Cultivate >> Plant >> Growth >> Harvest
    #region 농경 1단계: 밭을 간다!
    // 밭갈기 메서드
    public void Cultivate()
    {
        print("밭 가는 중...");

        //음향
        SFXSoundManager.Instance.Play("rakeWork", 0.3f, 0f, 0.1f);

        Invoke("PlantIdle", 2f);
    }

    // 농경지 메서드
    private void PlantIdle()
    {
        // step 01. 농토 오브젝트가 마련되어 있는가?
        if (farmland == null)
        {
            print("밭이 없습니다!");
            return;
        }

        // step 02. 밭을 처음 가는 건가? 아니면 이미 갈려있나?
        if (cultivated == null) cultivated = Instantiate(farmland, transform.position, Quaternion.Euler(transform.localEulerAngles));
        print("밭을 모두 갈았습니다!");

        //// step 03. '심기' 외에 다른 메시지가 떠있다면?
        //if (hmsg.activeSelf) hmsg.SetActive(false);
        //else if (cmsg.activeSelf) cmsg.SetActive(false);

        StartCoroutine(DelayPrepare());
    }
    #endregion

    #region 농경 2단계: 씨를 뿌린다!
    public void PreparePlant()
    {
        // 만약 밭이 갈린 상태에서 씨앗이 없을 때에는 
        if (SeedPocket.Seed.TotalSeeds() <= 0)
        {
            print("씨앗이 다 떨어졌는뎁쇼...?");
            return;
        }

        harvestState = HarvestState.Prepared;
    }

    // 메뉴 보기 메서드
    public void ViewPlantMenu()
    {
        plantMenu.SetActive(true);
        PlantStartButton plant = plantMenu.GetComponentInChildren<PlantStartButton>();
        plant.farmland = this;
    }

    // 씨앗 심기
    public void PlantSeed(int n)
    {
        plantMenu.SetActive(false);
        StartCoroutine(Planting(n));
    }

    public IEnumerator Planting(int n)
    {
        string seedName = seedNames[n];

        // 만약 재산 목록에 씨앗이 없으면 농작물을 심을 수 없다. >> 일단 임시방편으로 earn으로 해뒀습니다. 수정 예정.
        if (!SeedPocket.Seed.HasSeed(seedName))
        {
            print("씨앗이 없습니다!");
            yield break;
        }

        UIManager.instance.plantMenu.SetActive(false);

        print("농작물 심는 중");
        
        //Sound

        yield return new WaitForSeconds(2f);

        PlantCrop(n, seedName);
    }
    #endregion

    #region 농경 3단계: 작물이 자라날 때까지 기다린다!
    // 농작물 심기
    public void PlantCrop(int n, string seedName)
    {
        Debug.Log($"[HarvestManager] PlantCrop 호출됨 - 인덱스: {n}, 씨앗 이름: {seedName}");

        // 씨앗을 심으면 밭의 상태는 '파종' 상태로 변경, 마우스는 비활성화된다.
        harvestState = HarvestState.Planted;

        // 해당 작물이 심기는 순간, 배열이 살아난다.
        currentCropIndex = n;
        currentCrops = cropPoolSet[n].stageObjects;

        // 일단 모든 오브젝트는 비활성화된다.
        // foreach(var crop in currentCrops) crop.SetActive(false);

        // 작물 첫번째 단계 활성화
        currentCrops[0].SetActive(true);

        // 그 외에는 비활성화
        for (int i = 1; i < currentCrops.Length; i++) currentCrops[i].SetActive(false);

        // 이름
        if (timeView != null)
        {
            timeView.UpdateCropName(seedName);
        }

        // 확인용 메시지를 남기고, 씨앗 주머니 스크립트에 씨앗 하나를 쓴다고 알려준다.
        print($"{seedName} 작물을 심었습니다!");
        SeedPocket.Seed.UseSeed(seedName);

        // 성장 코루틴
        StartCoroutine(GrowCrop());
        StartTimer();
    }

    // 작물이 자라나는 과정
    IEnumerator GrowCrop()
    {
        // 작물이 자라나는 기간
        float durationToHarvest = allGrowthTime / (currentCrops.Length - 1);

        // 시간마다 돌아가면서 그에 맞는 작물의 생명주기를 표현한다.
        for (int i = 1; i < currentCrops.Length; i++)
        {
            /*
            // 작물이 Growing 상태로 가려면 일단 물이 필요합니다.
            if(i == 2)
            {
                cropState = CropState.NeedWater;
                watering = false;
                print("목말라... 물... 물을... 커헉");
            }

            // 어서 작물에게 물을 주세요!
            if (cropState == CropState.NeedWater)
            {
                yield return new WaitUntil(() => watering == true);
                cropState = CropState.Growing;
                print("물을 줘서 고마워~");
            }
            */
            yield return new WaitForSeconds(durationToHarvest);

            // 시간이 지나며 작물은 점점 자라나는데, 여기서는 전단계가 비활성화되고 다음단계가 활성화되고를 수확할 수 있을 때까지 반복한다.
            currentCrops[i - 1].SetActive(false);
            currentCrops[i].SetActive(true);
        }

        // 작물이 자라나면 경작지 상태는 '수확 가능' 상태로 바뀌고, 확인용 메시지를 출력시킨다.
        harvestState = HarvestState.Harvesting;
        print("이제 수확할 수 있습니다!");
    }
    #endregion

    #region 농경 4단계: 작물을 수확한다!
    public void GoHarvest()
    {
        //cmsg.SetActive(false);
        //pmsg.SetActive(false);
        //hmsg.SetActive(true);
        harvestState = HarvestState.Harvesting;

        StartCoroutine(ReturnToPrepared());
    }

    // 농작물 수확하기
    public void HarvestCrop()
    {
        if (currentCrops != null && currentCropIndex >= 0)
        {
            // 모든 단계 비활성화 (풀로 복귀)
            foreach (var obj in cropPoolSet[currentCropIndex].stageObjects)
            {
                if (obj != null)
                    obj.SetActive(false);
            }

            Refrigerator.Food.PlusCropItem(currentCropIndex);

            print($"작물({cropPoolSet[currentCropIndex].cropName}) 수확을 완료했습니다.");

            // ✅ [추가된 부분 시작] ----------------------------------------
            // 작물 이름 가져오기
            if (InventoryManager.inventory != null)
            {

                // 이름과 Inspector에 연결한 harvestItems 매칭
                string cropName = cropPoolSet[currentCropIndex].cropName;

                // ✅ 랜덤 수량 한 번만 계산
                int count = Random.Range(2, 6);

                if (cropName == "Tomato")
                {
                    // Woodberry 작물 수확 시 2개 아이템 추가
                    if (items.Length > 0 && items[0] != null)
                        InventoryManager.inventory.AcquireItem(items[0], count);  // 랜덤 2~5개
                    if (items.Length > 1 && items[1] != null)
                        InventoryManager.inventory.AcquireItem(items[1], count);

                    Debug.Log("[Harvest] Woodberry 아이템 2종 추가 완료!");
                }

                else if (cropName == "Wheat")
                {
                    // Wheat 작물 수확 시 2개 아이템 추가
                    if (items.Length > 2 && items[2] != null)
                        InventoryManager.inventory.AcquireItem(items[2], count);
                    if (items.Length > 3 && items[3] != null)
                        InventoryManager.inventory.AcquireItem(items[3], count);

                    Debug.Log("[Harvest] Wheat 아이템 2종 추가 완료!");
                }

                else if (cropName == "Pumpkin")
                {
                    // Pumpkin 작물 수확 시 2개 아이템 추가
                    if (items.Length > 4 && items[4] != null)
                        InventoryManager.inventory.AcquireItem(items[4], count);
                    if (items.Length > 5 && items[5] != null)
                        InventoryManager.inventory.AcquireItem(items[5], count);

                    Debug.Log("[Harvest] Pumpkin 아이템 2종 추가 완료!");
                }

                else
                {
                    Debug.LogWarning($"[Harvest] '{cropName}' 대응 Item이 HarvestManager에 연결되지 않음.");
                }
            }
            else
            {
                Debug.LogWarning("[Harvest] InventoryManager.inventory 가 null 입니다!");
            }
            // ✅ [추가된 부분 끝] ----------------------------------------
        }
        else
        {
            print("뿌리까지 다 뽑아갔는데 뭘 기대해요ㅋㅋㅋㅋ");
        }

        // 상태를 갱신
        harvestState = HarvestState.Cultivated;
        PlantIdle();
        print("작물 수확을 완료했습니다!");
    }
    #endregion

    // 4. ETC
    #region UI 관리
    // UI 활성화 전용 메서드
    void InfoActiveEnable()
    {
        harvestAble = true;
        //info.SetActive(true);
        //if (cultivated == null)
        //    cmsg.SetActive(true);
        //else
        //{
        //    cmsg.SetActive(false);
        //    pmsg.SetActive(true);
        //}
    }

    // UI 비활성화 전용 메서드
    void InfoActiveDisable()
    {
        harvestAble = false;
        info.SetActive(false);
        //info.SetActive(false);
    }
    #endregion

    #region 타이머들
    IEnumerator DelayPrepare()
    {
        yield return new WaitForSeconds(2f);
        PreparePlant();
    }

    IEnumerator ReturnToPrepared()
    {
        yield return new WaitForSeconds(0.5f);
        harvestState = HarvestState.Prepared;
        print("새로운 농작물을 심어보세요!");
    }

    public void StartTimer()
    {
        isGrowing = true;
        growthTime = 0f;
        StartCoroutine(GrowthTimer());
        StartCoroutine(GrowCrop());
        timeView?.Show();
    }

    public void StopTimer()
    {
        isGrowing = false;
    }

    public void ResetTimer()
    {
        growthTime = 0f;
    }

    public void Complete()
    {
        isGrowing = false;
        harvestState = HarvestState.Harvesting;
        timeView?.Hide();
    }

    IEnumerator GrowthTimer()
    {
        while (isGrowing && growthTime < allGrowthTime)
        {
            growthTime += Time.deltaTime;
            yield return null;
        }

        if (growthTime >= allGrowthTime)
            Complete();
    }
    #endregion
}
