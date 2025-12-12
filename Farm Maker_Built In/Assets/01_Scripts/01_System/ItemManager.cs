using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    // 싱글톤
    public static ItemManager GetItem = null;
    private void Awake()
    {
        if (GetItem == null) GetItem = this;
    }
    public Item item; 
   
    // 인벤토리에 들어갈시 최대 번들 갯수(이거 여기에 쓸지 안쓸지 모름
    public int maxBundle;

    // 아이템 생명 주기
    private float liveTime;

    // 아이템을 주울 수 있는가?
    public bool getItem;

    // 임시: 씨앗의 이름
    public string itemName;
    public string itemType;

    public Coroutine destroyItem;

    [Header("Hand Offset Settings")]
    public Vector3 handPos;
    public Vector3 handRot;

    public Collider col;

    private void Start()
    {
        // 모든 아이템은 그 범위에 닿을 때까지는 주울 수 없다.
        getItem = false;
        
    }

    private void OnEnable()
    {
        if(gameObject.CompareTag("Tools") || transform.parent)
        {
            return;
        }
        liveTime = 120f;
        // 일정 시간이 지나면 아이템은 사라진다.
        destroyItem = StartCoroutine(DestroyItem());
    }

    private void OnDisable()
    {
        if (destroyItem != null)
        {
            StopCoroutine(destroyItem);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 범위 내에 들어온 오브젝트가 플레이어라면, 아이템을 주울 수 있게 된다.
        if (other.gameObject.CompareTag("Player")) getItem = true;
    }

    public IEnumerator DestroyItem()
    {
        // 생명 주기가 지나가게 되면
        yield return new WaitForSeconds(liveTime);

        // 그 아이템은 사라진다.
        Destroy(gameObject);
    }

    public void InHandle()
    {
        transform.localPosition = handPos;
        transform.localEulerAngles = handRot;
        col.isTrigger = true;
    }

    public void OutHandle()
    {
        transform.localRotation = Quaternion.identity;
        col.isTrigger = false;
    }
}
