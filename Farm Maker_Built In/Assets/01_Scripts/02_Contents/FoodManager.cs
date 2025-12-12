using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodManager : MonoBehaviour
{
    public float satiety; //포만감
    private ItemManager item;

    private void Start()
    {
        item = GetComponent<ItemManager>();
    }
    public void EatAnimal(GameObject animal, GameObject player)
    {
        AnimalHunger hungry = animal.GetComponent<AnimalHunger>();

        //GameObject player = transform.root.gameObject;
        hungry.EatFood(satiety, player);

        if (player != null)
        {
            PlayerInterector interector = player.GetComponent<PlayerInterector>();

            if (interector.heldObject == gameObject) //손에서 먹인거라면
            {
                interector.heldObject = null;
                gameObject.transform.parent = null;
                InventoryManager.inventory.NextItem(item);
            }
        }

        Destroy(gameObject);
    }

    public void EatMe(GameObject player)
    {
        PlayerController controller = player.GetComponent<PlayerController>();

        controller.EatFood(satiety);

        //플레이어 먹기 음향효과 추가
        SFXSoundManager.Instance.Play("playerEatClip", 0.5f, 0f, 0.1f);

        PlayerInterector interector = player.GetComponent<PlayerInterector>();

        if(interector.heldObject == gameObject)
        {
            interector.heldObject = null;
            gameObject.transform.parent = null;
            InventoryManager.inventory.NextItem(item);
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        //Debug.Log("음식 destroy됨");
    }
}
