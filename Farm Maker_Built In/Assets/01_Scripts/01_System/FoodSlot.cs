using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FoodSlot : MonoBehaviour, IPointerClickHandler
{
    public Item item;//  획득한 아이템
    public int itemCount; //획득한 아이템의 개수.
    public Image itemImage; // 아이템의 이미지.

    // 필요한 컴포넌트.
    [SerializeField] private Text Text_Count;
    [SerializeField] private GameObject go_CountImage;
    public Slot slot;
    private Rect baseRect;
    //public PlayerInterector playerInterector;

    public bool isToolSlot = false; // true면 도구 인벤토리 슬롯, false면 일반 슬롯

    public GameObject giveButton;
    public Vector3 buttonOffset = new Vector3(80, 30, 0); // 오른쪽으로 80픽셀 y 20

    private void Start()
    {
        baseRect = transform.parent.parent.GetComponent<RectTransform>().rect;
    }
    public void ViewSlot()
    {
        item = slot.item;
        itemCount = slot.itemCount;

        Text_Count.text = itemCount.ToString();
        itemImage.sprite = item.itemImage;

        go_CountImage.SetActive(true);
        SetColor(1);
    }

    private void SetColor(float _alpha)
    {
        Color color = itemImage.color;
        color.a = _alpha;
        itemImage.color = color;
    }

    public void OnPointerClick(PointerEventData eventData)  // IPointerClickHandler 를 사용하기 위해 필요함 !!
    {
        Debug.Log($"[Click] {eventData.button} 눌림");

        // 아이템 우클릭 하면 나오는 버튼
        if (eventData.button == PointerEventData.InputButton.Right && item != null)
        {
            // 아이템 있는 경우 -> 버튼 표시
            if (item != null)
            {
                giveButton.SetActive(true);

                // 버튼 켜질 때는 클릭 가능 상태로 만들어야 함
                var cg1 = giveButton.GetComponent<CanvasGroup>();
                if (cg1 != null) cg1.blocksRaycasts = true;


                giveButton.transform.position = transform.position + buttonOffset;

                giveButton.GetComponent<GiveFoodBtn>().SetSlot(this); // 클릭한 슬롯 정보 전달.           
            }
            else
            {
                // 아이템 없는 슬롯 클릭 시 버튼 비활성화
                giveButton.SetActive(false);
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            // 왼쪽 클릭 시에도 버튼 숨김(버리기 / 장착 ui 닫기)
            giveButton.SetActive(false);
        }
    }

    public void ClearSlot()
    {
        item = null;
        itemCount = 0;
        itemImage.sprite = null;
        SetColor(0);

        Text_Count.text = "0";
        go_CountImage.SetActive(false);
    }

    public void GiveFood()
    {
        GameObject food = Instantiate(item.ItemPrefab, transform);

        slot.playerInterector.GiveFood(food);

        slot.SetSlotCount(itemCount - 1);
    }
}
