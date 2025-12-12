// using Autodesk.Fbx;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using TMPro;

public class Slot : MonoBehaviour ,IPointerClickHandler ,IBeginDragHandler,IDragHandler,IEndDragHandler,IDropHandler
{
    private Vector3 originPos;

    public Item item;//  획득한 아이템
    public int itemCount; //획득한 아이템의 개수.
    public Image itemImage; // 아이템의 이미지.

    // 필요한 컴포넌트.
    [SerializeField] private Text Text_Count;
    [SerializeField] private GameObject go_CountImage; 

    private Rect baseRect;
    public PlayerInterector playerInterector;
    private GameObject player;

    public bool isToolSlot = false; // true면 도구 인벤토리 슬롯, false면 일반 슬롯

    public GameObject buttons; //버튼들 부모 오브젝트
    public GameObject dropButton; // 공용 버튼을 slot이 제어
    public GameObject holdButton; // 손에 착용버튼
    public TMP_Text itemNameText;
    public TMP_Text itemTypeText;
    public TMP_Text itemCountText;
    public TMP_Text itemText;
    //public GameObject eatButton; //음식전용
    //public Vector3 buttonOffset = new Vector3(150, 50, 0); // 오른쪽으로 80픽셀 y 20

    private void Start()
    {      
        baseRect = transform.parent.parent.GetComponent<RectTransform>().rect;
        originPos = transform.position;

        player = GameObject.FindGameObjectWithTag("Player");
        if (playerInterector == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerInterector = playerObj.GetComponent<PlayerInterector>();
        }
        if (item != null)
        {
            AddItem(item, itemCount);
        }
    }
    // 이미지의 투명도 조절
    private void SetColor(float _alpha)
    {
        Color color = itemImage.color;
        color.a = _alpha;
        itemImage.color = color;
    }
    // 아이템 획득
    public void AddItem(Item _item, int _count = 1)
    {
        // ✅ null 방어 (에러 방지)
        if (_item == null)
        {
            Debug.LogWarning("[AddItem] _item이 null입니다!");
            return;
        }

        if (itemImage == null)  //ui 쪽 확인
        {
            Debug.LogError("[AddItem] itemImage가 인스펙터에 연결되지 않았습니다!");
            return;
        }

        item = _item;
        itemCount = _count;

        // ✅ itemImage가 null일 경우 대비
        if (item.itemImage != null)  // 아이템 데이터 문제일 경우!!
            itemImage.sprite = item.itemImage;
        else
            Debug.LogWarning($"[AddItem] {item.itemName}의 itemImage가 비어 있습니다.");

        if (item.itemType != Item.ItemType.Equipment)  //장비일 때는 Count틑 비활성화
        {
            go_CountImage.SetActive(true);
            Text_Count.text = itemCount.ToString();
        }
        else
        {
            Text_Count.text = "0";
            go_CountImage.SetActive(false);  // 장비가 아니면 Count 활성화
        }

            SetColor(1);
 

    }
    //아이템 개수 조정.
    public void SetSlotCount(int _count)
    {
        itemCount = _count;
        Text_Count.text = itemCount.ToString();

        // 외부 시스템 동기화 (감소할 때도 같이 갱신)
        if (item != null)
        {
            string name = item.itemName;

            if (ToInventoryBox.ToInventory != null)
                ToInventoryBox.ToInventory.UpdateItemCount(name, itemCount);

            if (Refrigerator.Food != null)
                Refrigerator.Food.UpdateItemCount(name, itemCount);
        }

        if (itemCount <= 0)
        {
            ClearSlot();
        }
        if(buttons != null)
            buttons.SetActive(false);
    }
    // 슬롯 초기화
    public void ClearSlot()
    {
        item = null;
        itemCount = 0;
        itemImage.sprite = null;
        SetColor(0);

        Text_Count.text ="0";
        go_CountImage.SetActive(false);
    }
    // 아이템 개수를 더하는 함수
    public void IncreaseCount(int _count)
    {
        itemCount += _count;
        Text_Count.text = itemCount.ToString();

        //  씨앗(Seed) 동기화는 여기서만!
        if (item != null && item.itemType == Item.ItemType.Seed)
        {
            if (SeedPocket.Seed != null)
            {
                string seedName = item.itemName;

                if (SeedPocket.Seed.seeds.ContainsKey(seedName))
                    SeedPocket.Seed.seeds[seedName] += _count;
                else
                    SeedPocket.Seed.seeds.Add(seedName, _count);

                SeedPocket.Seed.UpdateSeedList(seedName, SeedPocket.Seed.seeds[seedName]);
                // Debug.Log($"[SeedSync] {seedName} +{_count} → 총 {SeedPocket.Seed.seeds[seedName]}");
            }
        }

        // ✅ 자재(Ingredient) & 음식(Food) 동기화 — 자동 반영
        if (item != null)
        {
            string name = item.itemName;

            if (ToInventoryBox.ToInventory != null)
                ToInventoryBox.ToInventory.UpdateItemCount(name, itemCount);

            if (Refrigerator.Food != null)
                Refrigerator.Food.UpdateItemCount(name, itemCount);
        }
        if (itemCount <= 0)
            ClearSlot();
    }

    public void OnPointerClick(PointerEventData eventData)  // IPointerClickHandler 를 사용하기 위해 필요함 !!
    {
        Debug.Log($"[Click] {eventData.button} 눌림");

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if(item != null)
            {
                if(item.itemType == Item.ItemType.Equipment)
                {
                    
                    //장착
                    // 누를경우 인벤토리에 있는 아이템이 손으로 이동 !! 그리고
                    // 손에 있는 아이템이 인벤토리로 교체 !!
                }
                //else
                //{
                //    //소모
                //    Debug.Log(item.itemName + "을 사용했습니다.");
                //    SetSlotCount(-1);
                //}
            }
            if(buttons != null)
                buttons.SetActive(false);
        } 
        // 아이템 우클릭 하면 나오는 버튼
        if(eventData.button == PointerEventData.InputButton.Right && item != null)
        {
            if (buttons == null)
                return;
            // 아이템 있는 경우 -> 버튼 표시
            if (item != null)
            {
                //dropButton.SetActive(true);
                //holdButton.SetActive(true);
                buttons.SetActive(true);
                ItemInfoText();
                if (item.itemType == Item.ItemType.Food || item.itemType == Item.ItemType.Equipment) //아이템이 음식 혹은 도구인 경우에만 사용하기 버튼 보이기
                    holdButton.SetActive(true);
                else
                    holdButton.SetActive(false);

                // 버튼 켜질 때는 클릭 가능 상태로 만들어야 함
                var cg1 = dropButton.GetComponent<CanvasGroup>();
                if (cg1 != null) cg1.blocksRaycasts = true;

                var cg2 = holdButton.GetComponent<CanvasGroup>();
                if (cg2 != null) cg2.blocksRaycasts = true;

                //buttons.transform.position = transform.position + buttonOffset;
                //dropButton.transform.position = transform.position + buttonOffset;
                //holdButton.transform.position = transform.position + new Vector3(80, -20, 0);

                dropButton.GetComponent<DropButton>().SetSlot(this); // 클릭한 슬롯 정보 전달.          
                holdButton.GetComponent<HoldButton>().SetSlot(this); // 클릭한 슬롯 정보 전달.          
            }
            else
            {
                
                // 아이템 없는 슬롯 클릭 시 버튼 비활성화
                buttons.SetActive(false);
                
            }
        }
        //else if(eventData.button == PointerEventData.InputButton.Left)
        //{
        //    // 왼쪽 클릭 시에도 버튼 숨김(버리기 / 장착 ui 닫기)
        //    buttons.SetActive(false);
        //}
    }

    public void ItemInfoText()
    {
        string nameText = "";
        switch (item.itemName)
        {
            case "Bread":
                nameText = "빵";
                break;
            case "Omlet":
                nameText = "오믈렛";
                break;
            case "PumpkinPie":
                nameText = "호박파이";
                break;
            case "Egg":
                nameText = "달걀";
                break;
            case "Feed":
                nameText = "사료";
                break;
            case "WheatGrain":
                nameText = "밀알";
                break;
            case "RottenSeed":
                nameText = "썩은씨앗";
                break;
            case "PumpkinSeed":
                nameText = "호박씨앗";
                break;
            case "WheatSeed":
                nameText = "밀씨앗";
                break;
            case "WoodberrySeed":
                nameText = "토마토씨앗";
                break;
            case "Axe":
                nameText = "도끼";
                break;
            case "EmptyWoodenBucket":
                nameText = "빈물통";             
                break;
            case "FoodContainer":
                nameText = "사료그릇";
                break;
            case "Pickaxe":
                nameText = "곡괭이";
                break;
            case "Rake":
                nameText = "레이크";
                break;
            case "WoodenBucket":
                nameText = "물통";
                break;
            case "Carrot":
                nameText = "당근";
                break;
            case "Pumpkin":
                nameText = "호박";
                break;
            case "Tomato":
                nameText = "토마토";
                break;
            case "Wheat":
                nameText = "밀";
                break;
            case "Flint":
                nameText = "부싯돌";
                break;
            case "Flour":
                nameText = "밀가루";
                break;
            case "HayBale":
                nameText = "건초더미";
                break;
            case "Ketchup":
                nameText = "케찹";
                break;
            case "Moneybag":
                nameText = "돈가방";
                break;
            case "Rice":
                nameText = "쌀";
                break;
            case "Stone":
                nameText = "돌맹이";
                break;
            case "Wood":
                nameText = "나무장작";
                break;
            default:
                nameText = item.itemName;
                break;
        }

        itemNameText.text = nameText;

        string typeText = "";
        switch (item.itemType)
        {
            case Item.ItemType.Equipment:
                typeText = "장비";
                break;
            case Item.ItemType.Used:
                typeText = "소모품";
                break;
            case Item.ItemType.Ingredient:
                typeText = "재료";
                break;
            case Item.ItemType.Seed:
                typeText = "씨앗";
                break;
            case Item.ItemType.Food:
                typeText = "음식";
                break;
            case Item.ItemType.Crop:
                typeText = "농산물";
                break;
            case Item.ItemType.ETC:
                typeText = "기타";
                break;
            default:
                typeText = "사료";
                break;
        }
        itemTypeText.text = typeText;

        //아이템 설명
        string itemDescrip = item.itemDescrip;
        if (itemDescrip == null || itemDescrip.Length == 0)
            itemDescrip = "아직 연구 중이다.";

        itemText.text = itemDescrip;
    }

    public void UsedItemCount(int num)
    {
        int count = int.Parse(itemCountText.text);

        count += num;

        if (count > itemCount)
            count = itemCount;
        else if (count < 1)
            count = 1;

        itemCountText.text = count.ToString();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(item != null)
        {
            DragSlot.instance.dragSlot = this;
            DragSlot.instance.DragSetImage(itemImage); 

            DragSlot.instance.transform.position = eventData.position;

            //var cg = dropButton.GetComponent<CanvasGroup>();
            //if (cg != null) cg.blocksRaycasts = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {                
        if(DragSlot.instance.dragSlot != null)       
            ChangeSlot();       
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 인스턴스 아이템 null 일때때
        if (DragSlot.instance == null) return;

        if (DragSlot.instance.transform.localPosition.x < baseRect.xMin || DragSlot.instance.transform.localPosition.x > baseRect.xMax
            || DragSlot.instance.transform.localPosition.y < baseRect.yMin || DragSlot.instance.transform.localPosition.y > baseRect.yMax)
        {
            //Instantiate(DragSlot.instance.dragSlot.item.ItemPrefab,
            //    player.transform.position + player.transform.forward * 8f, Quaternion.identity);

            //GameObject itemObj = DragSlot.instance.dragSlot.item.ItemPrefab;
            //playerInterector.DropItem(itemObj);

            // 드래그 중인 슬롯 / 아이템 방어
            var srcSlot = DragSlot.instance.dragSlot;
            // Debug.Log($"[OnEndDrag] srcSlot={(srcSlot != null)}, item={(srcSlot?.item != null)}");
            // //if (srcSlot == null || srcSlot.item == null) return;

            GameObject itemPrefab = null;
            if (srcSlot.item != null) 
                itemPrefab = srcSlot.item.ItemPrefab;
            // GameObject itemPrefab = DragSlot.instance.dragSlot.item.ItemPrefab;

            if (itemPrefab != null && playerInterector != null)
            {
                int dropCount = itemCount;

                for (int i = itemCount; i > 0; i--)
                {
                    // 1️ Prefab을 씬에 생성
                    GameObject itemObj = Instantiate(itemPrefab,
                        playerInterector.transform.position + playerInterector.transform.forward * 2f,
                        Quaternion.identity);

                    // 2️ DropItem 호출 (씬에 생성된 인스턴스 전달)
                    playerInterector.DropItem(itemObj);
                }
                SetSlotCount(itemCount - dropCount);
                //SetSlotCount(-1);
            }
        }
        // ✅ 도구 인벤토리 슬롯에서도 잔상 남지 않도록 강제 초기화
        DragSlot.instance.SetColor(0);
        DragSlot.instance.dragSlot = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if(DragSlot.instance.dragSlot != null)
            ChangeSlot();
    }
    private void ChangeSlot()
    {
        // 추가 : DragSlot.instance나 dragSlot이 비어있으면 바로 리턴
        if (DragSlot.instance == null || DragSlot.instance.dragSlot == null)
            return;

        Slot from = DragSlot.instance.dragSlot;

        // 방어 코드 추가
        if (from == null || from.item == null)
        {
            Debug.LogWarning("[ChangeSlot] 드래그 시작 슬롯이 비어 있음.");
            return;
        }
        // 도구 슬롯인데, Equiment가 아닌 경우는 무시
        if (isToolSlot && from.item.itemType != Item.ItemType.Equipment)
        {
            Debug.Log("[ChangeSlot] 도구 슬롯에는 장비(Equipment)만 넣을 수 있습니다!");
            return;
        }

        Item _tempItem = item;           // 현재 슬롯(this)에 들어있는 아이템을 임시 저장
        int _tempItemCount = itemCount;  // 현재 슬롯의 아이템 개수도 임시로 저장

        if (from.item != null)
        {   
            // 지금 드래그하고 있는 슬롯(from)의 아이템을 현재 슬롯(this)에 덮어씌움
            // 즉, 드롭된 슬롯(this)에 드래그된 아이템을 넣는 동작
            AddItem(DragSlot.instance.dragSlot.item, DragSlot.instance.dragSlot.itemCount);
        }
        if(_tempItem != null)
        {
            // 만약 원래 이 슬롯(this)에 있던 아이템이 있었다면
            // 그걸 드래그 시작한 슬롯(from)으로 다시 넣어준다 (자리 교환)
            DragSlot.instance.dragSlot.AddItem(_tempItem,_tempItemCount);       
        }
        else
            // 만약 원래 이 슬롯(this)이 비어 있었다면,
            // 드래그 시작한 슬롯(from)은 비워줌 (이동만 일어난 상태)
            DragSlot.instance.dragSlot.ClearSlot();
        
    }
    // 아이템 우클릭 했을때 버튼 나오는 관련 !!
    public void ConfirmDrop(int dropCount)
    {     
        // 1️ 슬롯이 비었거나 아이템 개수가 0 이하일 경우 즉시 종료
        if (item == null || itemCount <= 0)
            return;
        // 2️ 버릴 개수 보정 (최소 1개, 최대 현재 개수)
        int count = Mathf.Clamp(dropCount, 1, itemCount);
       
        if(item.itemType == Item.ItemType.Seed && SeedPocket.Seed != null)
        {
            string seedName = item.itemName;

            if (SeedPocket.Seed.seeds.ContainsKey(seedName))
            {
                // 씨앗 개수 차감
                SeedPocket.Seed.seeds[seedName] -= count;
                if (SeedPocket.Seed.seeds[seedName] < 0)
                    SeedPocket.Seed.seeds[seedName] = 0;

                // 리스트도 갱신 (UI / 저장용)
                SeedPocket.Seed.UpdateSeedList(seedName, SeedPocket.Seed.seeds[seedName]);

                Debug.Log($"[SeedSync] {seedName} -{count}개 → 남은 {SeedPocket.Seed.seeds[seedName]}개");
            }
        }
        // 재료(Ingredient) 감소
        if (item.itemType == Item.ItemType.Ingredient && ToInventoryBox.ToInventory != null)
        {
            string materialName = item.itemName;
            if (ToInventoryBox.ToInventory.materials.ContainsKey(materialName))
            {
                ToInventoryBox.ToInventory.materials[materialName] -= count;
                if (ToInventoryBox.ToInventory.materials[materialName] < 0)
                    ToInventoryBox.ToInventory.materials[materialName] = 0;

                // 리스트 반영
                var box = ToInventoryBox.ToInventory;
                foreach (var mat in box.materialList)
                {
                    if (mat.materialName == materialName)
                    {
                        mat.count = box.materials[materialName];
                        break;
                    }
                }

                Debug.Log($"[MaterialSync] {materialName} -{count}개 → 남은 {ToInventoryBox.ToInventory.materials[materialName]}개");
            }
        }

        if (item.ItemPrefab != null && playerInterector != null)
        {
            Debug.Log("1. 아이템 생성");
            for (int i = 0; i < count; i++)
            {                        
                Debug.Log("아이템 생성");
                GameObject itemObj = Instantiate(
                    item.ItemPrefab,
                    playerInterector.transform.position +playerInterector.transform.forward * 1.5f + Vector3.up * 0.5f,
                    Quaternion.identity);              
           
                playerInterector.DropItem(itemObj);
            }
        }
        else
            Debug.LogWarning($"ConfirmDrop 실패: Prefab ={item.ItemPrefab}, playerInterector = {playerInterector}");

        // 인벤토리 수량 줄이기
        SetSlotCount(itemCount - count);

        if(buttons != null)
            buttons.SetActive(false);
    }
    
    public void ChangeHandItem()
    {
        GameObject itemObj = Instantiate(item.ItemPrefab);
        ClearSlot();
        Debug.Log(itemObj);
        playerInterector.HandleItem(itemObj);
        Debug.Log(itemObj);
    }

    public void HoldItem()
    {
        // 🔸 손에 이미 아이템이 있다면 → 빈 슬롯으로 옮기기
        //if (playerInterector.heldObject != null)
        //{
        //    ItemManager heldManager = playerInterector.heldObject.GetComponent<ItemManager>();
        //    if (heldManager != null)
        //    {
        //        // 빈 슬롯 찾아서 손 아이템 이동
        //        for (int i = 0; i < ToolsInventory.toolsInv.toolsSlots.Length; i++)
        //        {
        //            Slot emptySlot = ToolsInventory.toolsInv.GetSlot(i);
        //            if (emptySlot != null && emptySlot.item == null)
        //            {
        //                emptySlot.AddItem(heldManager.item, 1);
        //                Destroy(playerInterector.heldObject);
        //                playerInterector.heldObject = null;
        //                break;
        //            }
        //        }

        //    }
        //}
        // 🔸 현재 슬롯의 아이템에서 1개만 손으로 옮기기
        GameObject itemObj = Instantiate(item.ItemPrefab);
        
        // 슬롯 개수 감소 (1개만 사용)
        SetSlotCount(itemCount - 1);
        Debug.Log("여기도 실행 되었음");
        // 손에 장착
        playerInterector.HandleItem(itemObj);
    }

}
