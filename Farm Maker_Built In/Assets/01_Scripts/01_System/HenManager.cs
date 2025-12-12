using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HenManager : MonoBehaviour
{
    public GameObject Egg;
    private GameObject EggPos;
    public Transform EggPosition;
    private AnimalController animalController;

    private HpController henHp;

    private Coroutine eggEx; //코루틴 변수
    private void Start()
    {
        //StartCoroutine(EggEx());

        henHp = GetComponent<HpController>();

        animalController = GetComponent<AnimalController>();
    }

    private IEnumerator EggEx()
    {
        float rnd;
        //float rnd = 5;
        while (true)
        {
            rnd = Random.Range(900, 1200);

            yield return new WaitForSeconds(rnd);

            if (henHp.Hp <= 20 || animalController.animalState == AnimalController.AnimalState.caught)
                continue;

            EggPos = Instantiate(Egg);
            EggPos.transform.position = EggPosition.transform.position;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("StockFarm"))
        {
            eggEx = StartCoroutine(EggEx());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("StockFarm"))
        {
            StopCoroutine(eggEx);
        }
    }
}
