using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectInfoView : MonoBehaviour
{
    void LateUpdate()
    {
        //오브젝트에 적용되어있는 캔버스가 항상 플레이어를 바라보게 설정
        transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                         Camera.main.transform.rotation * Vector3.up);
    }
}
