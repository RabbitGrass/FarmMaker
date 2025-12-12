using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : MonoBehaviour
{
    //[SerializeField]
    private NatureHpManager hp; // 바위의 체력

    [SerializeField]
    private float destroyTime; // 파편 제거 시간

    [SerializeField]
    private SphereCollider col; // 구체 콜라이더

    // 필요한 게임 오브젝트.
    [SerializeField]
    private GameObject go_rock; // 일반 바위
    [SerializeField]
    private GameObject go_debris; // 깨진 바위.
    [SerializeField]
    private GameObject go_effect_prefabs; // 채굴 이펙트.


    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip effect_Sound;
    [SerializeField]
    private AudioClip effect_Sound2;

    private void Start()
    {
        hp = GetComponent<NatureHpManager>();
    }

    public void mining() //채굴
    {
        audioSource.clip = effect_Sound;
        audioSource.Play();

        var clone = Instantiate(go_effect_prefabs, col.bounds.center, Quaternion.identity);
        Destroy(clone, destroyTime);

        hp.Damaged(1);

        if (hp.Hp <= 0)
        {
            Destruction();
        }
    }

    private void Destruction()
    {
        audioSource.clip = effect_Sound2;
        audioSource.Play();
        col.enabled = false;
        Destroy(go_rock);

        go_debris.SetActive(true);
        Destroy(go_debris,destroyTime);
    }
}
