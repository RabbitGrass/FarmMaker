using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeComponent : MonoBehaviour
{
    public GameObject healthyTree; // 풍성한 나무
    public GameObject rottenTree; // 썩은 나무
    private NatureHpManager hp; //hp 관리 스크립트
    public float respawntime; // 20분 생각함 !! 1200

    private bool isRotten = false;

    private void Start()
    {
        healthyTree.SetActive(true);
        rottenTree.SetActive(false);
        hp = GetComponent<NatureHpManager>();
    }

    public void TakeDamage(int damage)
    {
        if (isRotten) return; // 이미 썩은 나무면 무시

        hp.Damaged(damage);

        if (hp.Hp <= 0)
        {            
            BecomeRotten();
        }
    }
    private void BecomeRotten()
    {
        isRotten = true;
        healthyTree.SetActive(false);
        rottenTree.SetActive(true);

        // 일정 시간 후 회복
        StartCoroutine(RecoverTree());
    }
    IEnumerator RecoverTree()
    {
        yield return new WaitForSeconds(respawntime);

        hp.Heal(hp.MaxHp);

        rottenTree.SetActive(false);
        healthyTree.SetActive(true);
        isRotten = false;
    }
}
