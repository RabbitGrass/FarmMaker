using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickerFireLight : MonoBehaviour
{
    public Light targetLight;       // 깜빡일 Point Light 컴포넌트
    public float minIntensity = 1f; // 최소 밝기
    public float maxIntensity = 3f; // 최대 밝기
    public float flickerSpeed = 0.5f; // 깜빡임 속도 (낮을수록 천천히, 높을수록 빠르게)

    void Update()
    {
        // Random.Range를 사용하여 최소/최대 밝기 사이에서 무작위로 밝기를 설정합니다.
        // Time.time을 곱하여 매 프레임 다른 값을 얻지만, Mathf.PerlinNoise로 부드러움을 유지합니다.

        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);

        // PerlinNoise 값(0~1)을 사용하여 minIntensity와 maxIntensity 사이의 값으로 보간합니다.
        targetLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
    }
}
