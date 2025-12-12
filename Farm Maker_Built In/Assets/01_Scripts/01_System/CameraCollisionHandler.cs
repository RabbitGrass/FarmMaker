using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCollisionHandler : MonoBehaviour
{
    [Header("References")]
    public Transform cameraPos;    // 카메라 위치용 오브젝트 (PlayerViewerRot의 자식)
    public Transform mainCamera;   // 실제 카메라

    [Header("Settings")]
    public float defaultDistance = 4f;    // 기본 거리 (CameraPos의 초기 -Z 값)
    public float smoothSpeed = 10f;       // 부드럽게 이동 속도
    public float cameraRadius = 0.2f;     // 카메라 충돌 반지름
    public LayerMask collisionMask;       // 충돌 감지할 레이어 (Ground, Wall 등)

    private float currentDistance;

    void Start()
    {
        // CameraPos의 초기 위치를 기준으로 기본 거리 설정
        if (cameraPos != null)
            defaultDistance = Mathf.Abs(cameraPos.localPosition.z);

        currentDistance = defaultDistance;
    }

    void LateUpdate()
    {
        if (cameraPos == null || mainCamera == null) return;

        Vector3 origin = transform.position;      // 회전 피벗 (PlayerViewerRot)
        Vector3 dir = -transform.forward;         // 뒤쪽 방향
        RaycastHit hit;

        // Raycast 또는 SphereCast로 장애물 탐지
        if (Physics.SphereCast(origin, cameraRadius, dir, out hit, defaultDistance, collisionMask))
        {
            // 장애물에 부딪히면 거리 축소 (충돌 지점 앞까지)
            float targetDist = hit.distance - cameraRadius;
            currentDistance = Mathf.Lerp(currentDistance, targetDist, Time.deltaTime * smoothSpeed);
        }
        else
        {
            // 충돌 없으면 원래 거리로 복귀
            currentDistance = Mathf.Lerp(currentDistance, defaultDistance, Time.deltaTime * smoothSpeed);
        }

        // CameraPos의 로컬 Z위치 갱신
        cameraPos.localPosition = new Vector3(0, 0, -currentDistance);
    }
}
