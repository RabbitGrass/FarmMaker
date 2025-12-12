using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolsController : MonoBehaviour, IToolBag
{
    public int damage ; // 1로 생각중!! 나중에 변동 있을수도 있음
    public string animTrigger;
    public string tagName;

    public float needStamina;

    [SerializeField]
    private GameObject go_effect_prefabs; // 채굴 이펙트. 
    public Transform effectPos;

    private ItemManager itemMgr;

    //WorkTool사용시 위치값
    [Header("Use Offset Settings")]
    public Vector3 WorkToolPos;
    public Vector3 WorkToolRot;

    //private GameObject target;


    private void Start()
    {
        //axeBlade.SetActive(false); // 처음 비활성화
        itemMgr = GetComponent<ItemManager>();
    }

    public void UseTool(PlayerInterector player)  // 마우스 우클릭
    {
        //비어있음
    }

    public void WorkTool(GameObject player)  //마우스 좌클릭
    {    
        PlayerController ctl = player.GetComponent<PlayerController>();
        if (ctl.stamina < needStamina)
            return;

        ctl.anim.SetBool(animTrigger, true);
        ctl.anim.SetTrigger("UseTool");

        ctl.state = PlayerController.State.useTool;


        //위치값 및 회전값 조정
        transform.localPosition = WorkToolPos;
        transform.localEulerAngles = WorkToolRot;
    }

    public void RestTool(GameObject player)  
    {
        //axeBlade.SetActive(false);
        PlayerController ctl = player.GetComponent<PlayerController>();
        ctl.anim.SetBool(animTrigger, false);
        ctl.state = PlayerController.State.idle;
        transform.localPosition = itemMgr.handPos;
        transform.localEulerAngles = itemMgr.handRot;
        //target = null;
    }

    public void AttackTarget(GameObject target)
    {
        NatureHpManager targetHp = target.GetComponent<NatureHpManager>();
        if (targetHp.Hp <= 0)
            return;

        targetHp.Damaged(damage);
        GameObject effect = Instantiate(go_effect_prefabs, effectPos.position, Quaternion.identity);
        Destroy(effect, 1f);
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (!Blade.enabled || target != null) return; // 날이 켜져 있을 때만
    //    if (other.CompareTag(tagName))
    //    {
    //        IHealth Nature = other.GetComponent<IHealth>();
    //        NatureHpManager natureManager = other.GetComponent<NatureHpManager>();
    //        if (Nature != null)
    //        {
    //            target = other.gameObject;
    //            // 맞은 위치 계산
    //            Vector3 hitPos = other.ClosestPoint(transform.position);

    //            Nature.Damaged(damage);

    //            // healthyNature가 활성화일 때만 이펙트 생성
    //            if (natureManager != null && natureManager.healthyNature != null && natureManager.healthyNature.activeSelf && go_effect_prefabs != null)
    //            {
    //                GameObject effect = Instantiate(go_effect_prefabs, hitPos + new Vector3(0, 1, 0), Quaternion.identity);
    //                Destroy(effect, 1f);
    //            }

    //            Debug.Log("데미지:" + damage);
    //        }
    //    }
    //}   
}
