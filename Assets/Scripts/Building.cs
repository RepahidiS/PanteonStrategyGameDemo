using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Building : MonoBehaviour
{
    public BuildingData data;
    public ProducibleData producible;
    public Node spawnPoint;
    public bool isPrimary = false;

    private void OnMouseDown()
    {
        // pointer is not over at any ui and buildingData has been set
        if(!GameManager.instance.uiManager.IsPointerOnUIs() && data != null)
            GameManager.instance.buildingManager.SetCurrentInspectingBuilding(this, true);
    }
}