using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Camera cam;
    public Transform camPosition;
    private float zoom;
    public float zoomPower;
    private Vector3 velocity = Vector3.zero;

    public float smoothSpeed;
    public float returnSpeed;

    private Transform lastParent; //
    private bool isReturning = false;

    // "Player" Layer 비트값 얻기
    int playerLayer;

    private void Awake()
    {
        playerLayer = LayerMask.NameToLayer("Player");
    }
    private void Start()
    {
        cam = GetComponent<Camera>();
        zoom = cam.fieldOfView;
        gameObject.transform.parent = camPosition;
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localEulerAngles = Vector3.zero;

        lastParent = transform.parent;
    }
    private void Update()
    {
        float wheel = Input.GetAxis("Mouse ScrollWheel");

        if (wheel != 0) //이 아래로는 마우스 휠이 움직이지 않았을 경우 실행하지 않음
        {
        //Vector3 vector = transform.localPosition;

        zoom += ((-wheel) * zoomPower * Time.deltaTime);

        zoom = Mathf.Clamp(zoom, 25, 100f);
        }


        //vector.z = zoom;
        //cam.fieldOfView = zoom;
        cam.fieldOfView = Mathf.Lerp(
            cam.fieldOfView,                //시작 
            zoom,                           //목표
            Time.deltaTime * smoothSpeed);  //목표에 도달할 스피드

        ParentChangeCheck();
        HandleReturnToOrigin();
    }

    private void ParentChangeCheck()
    {
        // 부모가 바뀐 경우 감지
        if (transform.parent != lastParent)
        {
            lastParent = transform.parent;
            isReturning = true; // 복귀 모드 ON
            if (transform.parent != camPosition)
                cam.cullingMask &= ~(1 << playerLayer);
        }
    }

    public void ReturnCam()
    {
        transform.parent = camPosition;
        cam.cullingMask |= (1 << playerLayer);
    }

    private void HandleReturnToOrigin()
    {
        if (!isReturning) return;

        // 부드럽게 원래 위치로 이동
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            Vector3.zero,
            Time.deltaTime * returnSpeed
        );

        transform.localRotation = Quaternion.Lerp(
            transform.localRotation,
            Quaternion.identity,
            Time.deltaTime * returnSpeed
        );

        // 거의 제자리에 왔으면 정지
        if (transform.localPosition.sqrMagnitude < 0.0001f &&
            Quaternion.Angle(transform.localRotation, Quaternion.identity) < 0.1f)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            isReturning = false; //복귀모드 off
        }
    }
}
