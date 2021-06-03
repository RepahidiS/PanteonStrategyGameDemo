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
    private GameObject _selectionHighlighter;

    public void SetSelectionHighLighter()
    {
        if (transform.childCount == 0)
        {
            Debug.LogError(name + " this building need to have a selection highlighter.");
            Debug.Break();
        }

        _selectionHighlighter = transform.GetChild(0).gameObject;
        Deselect();
    }

    private void OnMouseDown()
    {
        // pointer is not over at any ui and buildingData has been set
        if(!GameManager.instance.uiManager.IsPointerOnUIs() && data != null)
            GameManager.instance.buildingManager.SetCurrentInspectingBuilding(this, true);
    }

    public void Select()
    {
        _selectionHighlighter.SetActive(true);
    }

    public void Deselect()
    {
        _selectionHighlighter.SetActive(false);
    }
}