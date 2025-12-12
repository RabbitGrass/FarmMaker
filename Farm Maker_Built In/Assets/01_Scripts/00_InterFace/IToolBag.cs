using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IToolBag
{
    void UseTool(PlayerInterector player); //우클릭

    void WorkTool(GameObject player);   //좌클릭

    void RestTool(GameObject player);   //좌클릭이 끝났을 경우
}
