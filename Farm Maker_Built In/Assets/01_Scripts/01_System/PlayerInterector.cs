using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
//using TMPro;
using Unity.VisualScripting;
//using UnityEditor.Rendering.PostProcessing;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class PlayerInterector : MonoBehaviour
{
    private PlayerController playerCtl;

    public Transform OverHand; //플레이어가 물체를 머리 위로 번쩍 들 때의 위치
    private GameObject nearbyObject;
    private GameObject CatchObject; //null 허용
    public GameObject Friend;

    //public GameObject itemPickAble; //인벤토리에 넣을 아이템 감지  // private 에서 public으로 수정함

    public GameObject nearbyItem; //아이템 감지용  //private 에서 puvlic으로 변경함 !!

    public Transform holdPoint;   // 손 위치(물건을 잡을 때)
    public GameObject heldObject; // 플레이어가 현재 들고 있는 물건

    public GameObject target; // 근처 물 감지용

    public LayerMask targetLayer; //타겟 레이어

    public LayerMask exceptionTarget; //예외

    public bool isWater;

    public float throwPower; //던지는 힘, 현 시점에서는 동물을 던지는 힘

    bool isHand = false; //손 사용 여부

    bool isPutDown = false; //오브젝트를 내려놓고 있는지 여부

    private Vector3 dropVector; //물건이나 동물 등을 내려놓을 때의 위치벡터


    //public Animator animator; //플레이어 애니메이터

    private ActionController pickup; //아이템 인벤토리로 보내는 용도

    private bool useTool; //도구사용여부 체크, 도구 사용 중 버리는 행위 방지용

    private bool isHome; // 스폰구역, 집 감지, isHome이 true일 때 잠자는 기능 실행 가능
    private SleepController Home;
    public bool isSleepChoice;

    //public TMP_Text guideText; //가이드 텍스트

    public GameObject Guides; //가이드의 부모 오브젝트
    public Image[] guide; //가이드 약 이미지 세개 넣을 예정
    public Sprite[] guideTextImg; // 0. 줍기
                                  // 1. 다가가기
                                  // 2. 들어올리기
                                  // 3. 내려놓기
                                  // 4. 들기
                                  // 5. 던지기
                                  // 6. 채집하기
                                  //private ToolsInventory toolsInventory;

    private FoodContainer foodContainer; // 오로지 모이통 감지용

    private bool isChest;

    private BenchInteractSimple restBench;
    public bool isSit;
    private void Start()
    {
        playerCtl = GetComponent<PlayerController>();
        dropVector = new Vector3(0, 0.25f, 0.5f);

        //toolsInventory = FindObjectOfType<ToolsInventory>();

        pickup = GetComponent<ActionController>();

        if (holdPoint.childCount > 0)
        {
            heldObject = holdPoint.GetChild(0).gameObject;
            Rigidbody rb = heldObject.GetComponent<Rigidbody>();
            rb.isKinematic = true;
            heldObject.GetComponent<ItemManager>().InHandle();
        }
        GuideClear();

    }

    private void Update()
    {
        if (UIManager.instance.MenuTab.activeSelf == true || playerCtl.state == PlayerController.State.KO || 
            playerCtl.state == PlayerController.State.sleep || isSleepChoice)
            return;

        if (CatchObject != null || heldObject != null)
        {
            if(CatchObject != null)
            {
                GuideText(guideTextImg[3]);
                GuideText(guideTextImg[5]);
            }
                isHand = true;
        }
        else
        {
            if (isHand)
            {
                isHand = false;
                GuideClear();
            }
        }



        if (Input.GetKeyDown(KeyCode.F))
        {
            if (nearbyItem != null) //근처에 주울 수 있는 아이템이 있는 경우
            {
                PickUp();
            }
            else if (isHome)    //근처에 집이 있는 경우
            {
                Home.IsChoice();
            }
            else if (foodContainer != null)  // 모이통이 근처에 있는 경우
            {
                foodContainer.FeedCharge();
            }
            else if (isChest && !UIManager.instance.Chest.activeSelf) //창고가 근처에 있고, 창고가 열려있지 않은 경우
            {
                UIManager.instance.HandleShortKey(0);
                UIManager.instance.Chest.SetActive(true);
            }
            else if (restBench != null) //휴식포인트가 근처에 있는 경우
            {
                if (!isSit) //앉아있지 않는 경우 앉기
                {
                    restBench.SitPlayer();
                    isSit = true;
                }
                else //앉아있었다면 일어서기
                {
                    restBench.StandPlayer();
                    isSit = false;
                }
            }
            else if (Friend != null) //동물 친구가 근처에 있다면
            {
                AnimalFriendManager fri = Friend.GetComponent<AnimalFriendManager>();
                fri.TalkAnimal(gameObject);
            }
            else if (target != null) //근처에 타겟이 있는 경우(해당 코드에서는 밭, 솥, 제작대만 대응
            {
                if (target.layer == LayerMask.NameToLayer("Farmland"))
                {
                    HarvestManager field = target.GetComponent<HarvestManager>();
                    if (field != null)
                    {
                        // 밭이 'Prepared' 상태일 때만 F로 메뉴 오픈
                        if (field.harvestState == HarvestManager.HarvestState.Prepared)
                        {
                            field.ViewPlantMenu();
                            Debug.Log("[PlayerInterector] 씨앗 메뉴 오픈");
                        }
                        else if (field.harvestState == HarvestManager.HarvestState.Harvesting)
                        {
                            field.HarvestCrop();
                        }
                    }
                }
                else if (target.layer == LayerMask.NameToLayer("Kitchen"))
                {
                    Kitchen pot = target.GetComponent<Kitchen>();
                    if (pot != null)
                    {
                        // 빈 솥이면?
                        if (pot.cookState == Kitchen.CookingState.Cool || pot.cookState == Kitchen.CookingState.Empty)
                            UIManager.instance.OpenCooker();

                        // 요리가 다 되었다면?
                        else if (pot.cookState == Kitchen.CookingState.Complete)
                            pot.TakeFood(pot.currentCooking);
                    }
                }
                else if (target.layer == LayerMask.NameToLayer("Crafting"))
                {
                    CraftingPlace windmill = target.GetComponent<CraftingPlace>();
                    if (windmill != null)
                    {
                        // 빈 풍차라면?
                        if (windmill.craftState == CraftingPlace.CraftState.Empty)
                            UIManager.instance.OpenCraftingTable();

                        // 제작이 완료되었다면?
                        else if (windmill.craftState == CraftingPlace.CraftState.Complete)
                            windmill.TakeStuff(windmill.currentTable);
                    }

                }

                GuideClear();
            }
        }

        if (UIManager.instance.talkAnimal.activeSelf == true) //음식 인벤토리 활성화시 F를 제외한 모든 상호작용 막기
            return;

        //좌클릭
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (CatchObject) //오브젝트를 잡고 있을 때
            {
                ThrowObject(); //오브젝트 던지기
            }
            else if (heldObject)
            {
                if (heldObject.CompareTag("Tools")) //손에 Tools를 들고있을 때
                {
                    useTool = true;
                    IToolBag tools = heldObject?.GetComponentInChildren<IToolBag>();
                    if (tools != null)
                        tools.WorkTool(gameObject);
                }

            }
        }

        // 좌클릭 종료
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            if (heldObject)
            {
                if (heldObject.CompareTag("Tools")) //도구 사용 종료(나무캐기, 채광 등)
                {
                    useTool = false;
                    IToolBag tools = heldObject?.GetComponentInChildren<IToolBag>();
                    if (tools != null)
                        tools.RestTool(gameObject);
                    //animator.SetTrigger("Stop");
                }
            }
        }
        // 우클릭
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            //Debug.Log("우클릭");
            if (heldObject != null)
            {
                if (heldObject.CompareTag("Food") && nearbyObject != null && nearbyObject.layer == LayerMask.NameToLayer("Animal"))
                {
                    //FoodManager food = heldObject.GetComponent<FoodManager>(); //음식 정보 얻기
                    GiveFood(heldObject); 
                    //heldObject = null; //음식을 먹였으므로 손 비워버리기
                }
                else if (heldObject.CompareTag("Food"))
                {
                    FoodManager food = heldObject.GetComponent<FoodManager>();
                    //playerCtl.EatFood(food.satiety);
                    food.EatMe(gameObject);
                }
                else if (heldObject.CompareTag("Tools"))
                {
                    IToolBag tools = heldObject.GetComponent<IToolBag>();
                    PlayerInterector inter = GetComponent<PlayerInterector>();
                    tools.UseTool(inter);
                }
            }
            else if (nearbyItem != null && !isHand && (nearbyItem.CompareTag("Tools") || nearbyItem.CompareTag("Food")))
            {
                HandleItem(nearbyItem);
                GuideClear();
            }
            else if ((nearbyObject && heldObject == null) || CatchObject) //동물을 잡을 수 있거나 잡고 있을 때
                PutUp(); //동물, 바위 등 오브젝트 들어올리기
        } 

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (heldObject && !useTool)
            {
                DropItem(heldObject);
            }
        }

        // 도구 인벤토리 단축키
        if (Input.GetKeyDown(KeyCode.Alpha1)) HandleToolSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) HandleToolSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) HandleToolSlot(2); 
        if (Input.GetKeyDown(KeyCode.Alpha4)) HandleToolSlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) HandleToolSlot(4);       
    }

    // 외부에서 불러오기용(동물과 대화 중 End로 끝낼 때 사용하기 위한 용도)
    public void EndTalkAnimal()
    {
        if(playerCtl.state == PlayerController.State.talk)
        {
            AnimalFriendManager fri = Friend.GetComponent<AnimalFriendManager>();
            fri.TalkAnimal(gameObject);
        }
    }

    void PickUp()
    {
        GuideClear();

        //Sound 아이템 줍는 효과음
         SFXSoundManager.Instance.Play("ItemPickUp", 0.5f); // 볼륨 조절 가능 (0.0~1.0)

        pickup.TryPickUp(nearbyItem);
        // 씨앗 먹기  // ActionController 에 합칠에정
        if (nearbyItem.CompareTag("Seeds"))
        {
            // GPT쨩이 써준 디버그용 코드 (데헷)
            Debug.Log($"[DEBUG] 아이템 감지됨: {nearbyItem.name}");
            ItemManager item = nearbyItem.GetComponent<ItemManager>();
            if (item != null)
                Debug.Log($"[DEBUG] itemType = {item.itemType}, itemName = {item.itemName}");
            else
                Debug.Log("[DEBUG] ItemManager가 없음!");

            SeedPocket.Seed.PlusItem(nearbyItem);
        }

        nearbyItem = null;
    }

    void PutUp() //오브젝트를 들어올리는 시스템
    {
        if (playerCtl.stamina < 2f)
            return;
        playerCtl.stamina -= 2f;
        //Debug.Log("픽업애니멀");
        //동물 들어올리기 상호작용
        if (CatchObject == null && nearbyObject != null)
        {
            //isHand = true;
            CatchObject = nearbyObject;                                  //AnimalnearbyObject에 있던 동물을 CatchAnimal변수로 옮긴다. == 잡을 수 있는 동물을 잡은 동물로 변경
            //IHandsUp handsUp = OverHand?.GetComponentInChildren<IHandsUp>();
            IHandsUp handsUp = CatchObject.GetComponent<IHandsUp>();
            if (handsUp == null)
                return;

            CatchObject.transform.parent = OverHand;
            handsUp.InHandTransform();
            //CatchObject.transform.localPosition = new Vector3(0, 1.5f, 0);
            //CatchObject.transform.localEulerAngles = new Vector3(0, 0, 0);
            playerCtl.HandUp();
            GuideClear();
            GuideText(guideTextImg[4]);
            GuideText(guideTextImg[5]);
        }
        else if (CatchObject != null && !isPutDown)
        {
            isPutDown = true;
            playerCtl.HandDown();
        }        
    }

    public void PutDownEvent() //player의 underhead애니메이션쪽 이벤트
    {
        isPutDown = false;
        //isHand = false;
        IHandsUp handsUp = CatchObject.GetComponent<IHandsUp>();
        handsUp.HandDownTransform();

        CatchObject = null;
        GuideClear();
    }

    void ThrowObject()
    {   
        IHandsUp throwObject = CatchObject.GetComponent<IHandsUp> ();

        throwObject.Throw(gameObject, throwPower);

        playerCtl.ThrowObject();
        CatchObject = null;
        //isHand = false;
    }

    public void HandleItem(GameObject item)  //
    {
        //if (item == null || item.CompareTag("Item")) return; // Item 태그는 잡지 않음
        //if (item == null && heldObject == null) return; //잠시 수정 

        if (item != nearbyItem && heldObject != null)
        {
            //heldObject아이템을 인벤토리에 넣기
            pickup.TryPickUp(heldObject);
            heldObject = null;
        }

        if (heldObject == null) // 잡기
        {
            // 점검중
            // Sound 아이템을 손에 직접 쥘 때도 효과음 재생
            //SFXSoundManager.Instance.Play("ItemHand", 0.05f, 0f, 0.1f);
            SFXSoundManager.Instance.Play("ItemHand", 0.05f);

            heldObject = item;
            Rigidbody rb = heldObject?.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
            heldObject.transform.SetParent(holdPoint);
            ItemManager handItem = item.GetComponent<ItemManager>();
            handItem.InHandle();
            //isHand = true;

            if (item == nearbyItem)
                nearbyItem = null;
        }
    }

    //아이템 버리기
    public void DropItem(GameObject itemObj)
    {
        Debug.Log("플레이어 인터렉터 드롭 아이템 실행");
        Rigidbody rb = itemObj.GetComponent<Rigidbody>();
        rb.isKinematic = false;

        itemObj.transform.parent = gameObject.transform;

        itemObj.transform.localPosition = dropVector;

        itemObj.transform.SetParent(null);

        // 부모 변경 X — 그냥 월드에 직접 위치 지정
        Vector3 dropPos = transform.position + transform.forward * 1.5f + Vector3.up * 0.5f;
        itemObj.transform.position = dropPos;

        //Debug.Log($"{heldObject.tag} 내려놓음!");

        ItemManager itemManager = itemObj.GetComponent<ItemManager>();
        itemManager.OutHandle();

        if (itemObj == heldObject)
        {
            heldObject = null;
            //isHand = false;
        }

        // sound 아이템 버릴 때 효과음 재생
        SFXSoundManager.Instance.Play("ItemDrop", 0.6f);
    }

    public void AxeEvent() //도끼질 애니메이션 끝날 때 이벤트처리
    {
        if (target == null || !target.CompareTag("Tree"))
            return;

        ToolsController axe = heldObject.GetComponent<ToolsController>();
        axe.AttackTarget(target);
        playerCtl.stamina -= axe.needStamina;
        if (playerCtl.stamina < axe.needStamina)
            axe.RestTool(gameObject);
    }
    
    public void PickAxeEvent() //곡괭이 애니메이션 이벤트
    {
        if (target == null || !target.CompareTag("Rock"))
            return;

        ToolsController pickAxe = heldObject.GetComponent<ToolsController>();
        pickAxe.AttackTarget(target);

        playerCtl.stamina -= pickAxe.needStamina;
        if (playerCtl.stamina < pickAxe.needStamina)
            pickAxe.RestTool(gameObject);
    }

    public void GiveFood(GameObject food)
    {
        FoodManager foodManager = food.GetComponent<FoodManager>();
        foodManager.EatAnimal(nearbyObject, gameObject);
    }

    public void Petted()
    {
        AnimalFriendManager fri = Friend.gameObject.GetComponent<AnimalFriendManager>();
        float rnd = Random.Range(1, 11);
        fri.FriendShip(rnd, gameObject);

        //UIManager.instance.talkAnimalBtns[1].interactable = false;

        //UIManager.instance.talkAnimalBtns[1].interactable = true;
    }

    private void GuideClear()
    {
        foreach(Image guideTxt in guide)
        {
            guideTxt.gameObject.SetActive(false);
        }
    }

    private void GuideText(Sprite text)
    {
        int n = -1;
        for(int i = 0; i < guide.Length; i++)
        {
            if (!guide[i].gameObject.activeSelf) //guide[i]의 activeSelf가 false인 경우 n의 값을 저장 후 브레이크
            {
                n = i;
                break;
            }
            else if (guide[i].sprite == text) //이미 이미지가 적용되어 있는 경우 리턴
                break;
        }


        if(n >= 0)
        {
            guide[n].gameObject.SetActive(true);
            guide[n].sprite = text;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Home"))
        {
            isHome = true;
            Home = other.GetComponent<SleepController>();
        }

        else if (other.CompareTag("FoodContainer"))
        {
            foodContainer = other.GetComponent<FoodContainer>();
        }

        else if (other.CompareTag("Chest"))
        {
            GuideText(guideTextImg[9]);
            isChest = true;
        }
    }

    private void ExitObject(GameObject other)
    {
        if (other.CompareTag("Home"))
        {
            isHome = false;
            Home = null;
        }
        if (nearbyObject == other.gameObject)
        {
            nearbyObject = null;
            if (other.gameObject == Friend)
                Friend = null;
        }
        if (nearbyItem == other.gameObject)
        {
            nearbyItem = null;
        }
        if (target == other.gameObject)
        {
            target = null;
        }
        if (other.gameObject.layer == LayerMask.NameToLayer("Water") && isWater)
        {
            //nearbyWater = null;
            isWater = false;
        }
        if (other.CompareTag("FoodContainer"))
        {
            foodContainer = null;
        }
        if (other.CompareTag("Chest"))
            isChest = false;
        if (other.CompareTag("BenchA") || other.CompareTag("BenchB") || other.CompareTag("BenchC"))
            restBench = null;

        GuideClear();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == gameObject)
            return;

        if (((1 << other.gameObject.layer) & targetLayer.value) != 0 && target != other.gameObject)
        {
            if(target == null)
            {
                target = other.gameObject;
            }
            //else if(heldObject != null && heldObject.GetComponent<WaterBottle>() && other.gameObject.layer == LayerMask.NameToLayer("Water"))
            //{
            //    target = other.gameObject;
            //}
            else
            {
                var distanceOther = Vector3.Distance(transform.position, other.transform.position);
                var distanceTarget = Vector3.Distance(transform.position, target.transform.position);
                if (distanceOther < distanceTarget)
                    target = other.gameObject;
            }
            if (heldObject != null && heldObject.CompareTag("Tools") && ((1 << target.layer) & exceptionTarget.value) == 0)
                GuideText(guideTextImg[6]);
            else if (target.layer == LayerMask.NameToLayer("Farmland") && heldObject != null)
                GuideText(guideTextImg[7]);
            else if (target.layer == LayerMask.NameToLayer("Kitchen") || target.layer == LayerMask.NameToLayer("Crafting"))
                GuideText(guideTextImg[9]);
            
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Item") && nearbyItem != other.gameObject && other.gameObject != heldObject) //아이템 감지
        { 
            // Debug.Log("주워!");
            //guideText.text = "[F] 아이템 줍기";
            GuideText(guideTextImg[0]);
            nearbyItem = other.gameObject;
            //if((nearbyItem.CompareTag("Tools") || nearbyItem.CompareTag("Food")) && !isHand)
            //{
            //    GuideText(guideTextImg[4]);
            //}
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Water") && !isWater) // 물 감지 더미데이터에 가까움
        {
            //nearbyWater = other.gameObject;
            isWater = true;
        }
        else if (nearbyObject == null) //오브젝트 감지
        {
            //Debug.Log("잡아!");
            bool canAble = false;

            if (other.gameObject.layer == LayerMask.NameToLayer("Animal"))
            {
                AnimalController animal = other.GetComponent<AnimalController>();
                if (animal.CatchAble && !isHand)
                    canAble = true;

                AnimalFriendManager animalFriend = other.GetComponent<AnimalFriendManager>();
                if (animalFriend)
                {
                    //Debug.Log("동물친구 실행");
                    Friend = other.gameObject;
                }
                nearbyObject = other.gameObject;
            }
            if (canAble)
            {
                //guideText.text = "[우클릭] 들어올리기";
                GuideText(guideTextImg[2]);
            }
            if (Friend == nearbyObject && Friend != null)
                GuideText(guideTextImg[1]);
                //guideText.text += "\n[F] 같이놀기";
        }
        if((other.CompareTag("BenchA") || other.CompareTag("BenchB") || other.CompareTag("BenchC")) && !isSit)
        {
            restBench = other.GetComponent<BenchInteractSimple>();
            GuideText(guideTextImg[10]);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ExitObject(other.gameObject);
    }
    private void HandleToolSlot(int index)
    {
        //if (toolsInventory == null) return;

        //Slot slot = toolsInventory.GetSlot(index);
        Slot slot = ToolsInventory.toolsInv.GetSlot(index);

        if (slot == null) return; //이 코드가 실행될 가능성 0

        // 손이 비어 있고 슬롯에 아이템이 있을 때 -> 손으로 장착
        if(heldObject == null && slot.item != null)
        {
            Debug.Log($"[슬롯 -> 손] {slot.item.itemName} 장착");

            if (slot.item.ItemPrefab == null)
            {
                print("ItemPrefab이 할당되지 않았습니다!");
                return;
            }

            // ✅ 프리팹을 복제해서 씬에 올림 (이걸 들 수 있음)
            GameObject newItem = Instantiate(slot.item.ItemPrefab);

            // 손에 장착
            HandleItem(newItem);

            // 슬롯 비우기 
            slot.ClearSlot();

            isHand = true;
            return;
        }
        // 손에 아이템이 있고 슬롯이 비어 있을때 손에 있는 아이템 슬롯으로 이동
        else if (heldObject != null && slot.item == null)
        {
            Debug.Log("[손→슬롯] 아이템 이동");

            ItemManager heldManager = heldObject.GetComponent<ItemManager>();
            if (heldManager != null)
            {
                // 장비만 슬롯으로 이동가능
                if (heldManager.item.itemType == Item.ItemType.Equipment)
                {
                    slot.AddItem(heldManager.item, 1);// 스롯에 아이템 추가
                    Destroy(heldObject); // 실제 오브젝트 제거
                    heldObject = null;
                    isHand = false;
                }
                else
                {
                    //Debug.Log($"[손→슬롯] {heldManager.item.itemName} 은(는) 장비가 아니므로 이동 불가");
                    pickup.TryPickUp(heldObject);
                }
            }
        }
        // 손에도, 슬롯에도 아이템이 모두 있을 때 -> 서로 교체
        else if (heldObject != null && slot.item != null)
        {
            slot.ChangeHandItem();

        }

    }
}
