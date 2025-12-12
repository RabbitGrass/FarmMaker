using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class NatureHpManager : MonoBehaviour, IHealth
{
    public float MaxHp;
    public float Hp;

    public GameObject healthyNature;
    public GameObject RottenNature;
    public float respawntime;

    [SerializeField]
    private GameObject go_effect_prefabs; // 채굴 이펙트.
    [SerializeField]
    private GameObject[] Item_Prefab; // 아이템 프리펩
    [SerializeField]
    private int Count;


    public float itemPositionY = 5;

    private Collider col;
    private void Start()
    {
        Hp = MaxHp;
        col = GetComponent<Collider>();
    }

    public void Damaged(float dmg)
    {
        if (Hp <= 0) return;
        Hp -= dmg;
        if (go_effect_prefabs != null)
        {
            if (healthyNature != null && healthyNature.activeSelf)
            {
                GameObject effect = Instantiate(go_effect_prefabs, transform.position, Quaternion.identity);

                //sound 관련 스크립트
                string lowerName = effect.name.ToLower();

                if (lowerName.Contains("tree"))
                {
                    SFXSoundManager.Instance.Play("treeChop", 0.4f, 0f, 0.1f);
                }
                else if (lowerName.Contains("rock"))
                {
                    SFXSoundManager.Instance.Play("rockDamage", 0.4f, 0f, 0.1f);
                }
                // --
                Destroy(effect, 0.5f);
            }
        }
        if (Hp <= 0)
        {
            // HP가 0 이하일 때만 아이템 생성
            if (Item_Prefab != null)
            {  //SFXSoundManager.Instance.Play("treeFall");

                Vector3 itemVector = transform.position;
                itemVector.y += itemPositionY;
                for (int i = 0; i < Count; i++)
                {
                    int r = Random.Range(0, Item_Prefab.Length);
                    
                    Instantiate(Item_Prefab[r], itemVector, Quaternion.identity);
                }
            }

            // Sound 오브젝트 이름 기준으로 쓰러짐/파괴 효과음 구분
            if (go_effect_prefabs != null)
            {
                string lowerName = go_effect_prefabs.name.ToLower();
                if (lowerName.Contains("tree"))
                    SFXSoundManager.Instance.Play("treeFall", 0.3f, 0f, 0.1f);
                else if (lowerName.Contains("rock"))
                    SFXSoundManager.Instance.Play("rockBreak", 0.3f, 0f, 0.1f);
            }
            BecomeRotten();
        }
    }

    private void BecomeRotten()
    {

        // rottenNature 안에 Rock_Debris가 있으면 1초 후 비활성화
        Transform debris = RottenNature.transform.Find("Rock_Debris");
        if (debris != null)
        {
            Debug.Log("Rock_Debris 발견! 코루틴 시작");
            StartCoroutine(DisableAfterSeconds(debris.gameObject, 0.5f));
        }
        // 일정 시간 후 회복
        StartCoroutine(Recovery());
        healthyNature.SetActive(false);
        RottenNature.SetActive(true);
        col.enabled = false;
    }

    public void Heal(float heal) //회복
    {
        Hp += heal;

        if (Hp > MaxHp)
            Hp = MaxHp;
    }

    IEnumerator Recovery()
    {
        yield return new WaitForSeconds(respawntime);

        Heal(MaxHp);

        RottenNature.SetActive(false);
        healthyNature.SetActive(true);
        col.enabled = true;
    }
    private IEnumerator DisableAfterSeconds(GameObject obj, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        obj.SetActive(false);
        Debug.Log($"Rock_Debris를 {seconds}초 후 비활성화");
    }
}
