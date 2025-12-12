using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shovel : MonoBehaviour, IToolBag
{
    public int damage; // 1로 생각중!! 나중에 변동 있을수도 있음
    public BoxCollider ShovelBlade;

    public void UseTool(PlayerInterector player)
    {
        //비어있음
    }

    public void WorkTool(GameObject player)  //마우스 좌클릭
    {        
        ShovelBlade.enabled = true;
        Debug.Log("삽날 활성화 (좌클릭 눌렀음)");
    }

    public void RestTool(GameObject player)
    {        
        ShovelBlade.enabled = false;
        Debug.Log("삽날 비활성화");
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!ShovelBlade.enabled) return; // 날이 켜져 있을 때만
        //if (other.CompareTag("Stone"))
        //{
        //    //이건 나중에 농사 지을때 사용!!
        //    StoneComponent stone = other.GetComponent<StoneComponent>();  이름은 나중에 수정 !!
        //    if (tree != null)
        //    {
        //        tree.TakeDamage(damage);
        //        Debug.Log("에 데미지:" + damage);
        //    }
        //}
    }
}
