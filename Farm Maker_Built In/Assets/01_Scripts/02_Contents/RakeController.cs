using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RakeController : MonoBehaviour, IToolBag
{
    public void UseTool(PlayerInterector player)
    {
        Debug.Log($"[Rake] 호출됨 / player: {player?.name}, target: {player?.target?.name}");

        // 플레이어든 뭐든 간에 인터렉터 없고 타겟 없으면 나가리입니다.
        if (player == null || player.target == null)
        {
            Debug.Log("[Rake] player나 target이 null이라 조기 return");
            return;
        }

        HarvestManager farmland = player.target.GetComponent<HarvestManager>();
        if (farmland == null)
        {
            Debug.Log("[Rake] target에 HarvestManager 없음");
            return;
        }

        Debug.Log($"[Rake] farmland: {farmland.name}, state: {farmland.harvestState}");

        farmland.harvestAble = true;
        // 레이크를 손에 들고 있다면 바로 밭갈기 로직 호출
        if (farmland.harvestState == HarvestManager.HarvestState.None)
        {
            Debug.Log("[Rake] 밭갈기 시작!");
            farmland.Cultivate();
            StartCoroutine(UseRake(player.gameObject));
        }
        else if (farmland.harvestState == HarvestManager.HarvestState.Cultivated)
        {
            Debug.Log("[Rake] 이미 갈린 밭입니다!");
        }
    }

    public void RestTool(GameObject player)
    {
        
    }

    public void WorkTool(GameObject player)
    {
        
    }

    private IEnumerator UseRake(GameObject player)
    {
        PlayerController controller = player.GetComponent<PlayerController>();

        controller.state = PlayerController.State.useTool;
        yield return new WaitForSeconds(2f);
        controller.state = PlayerController.State.idle;
    }
}
