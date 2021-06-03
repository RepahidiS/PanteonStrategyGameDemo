using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class UIInformation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI txtBuildingName;
    public Image imgBuilding;

    public GameObject productionParent;
    public TextMeshProUGUI txtProductionName;
    public Button btnProduction;
    public Image imgProduction;

    public Button btnSetSpawnPoint;

    public TextMeshProUGUI txtPrimaryInfo;
    public Button btnSetPrimary;

    [HideInInspector] public bool isPointerOnThisUI = false;

    public void SetOnClickEvents()
    {
        btnProduction.onClick.AddListener(() => GameManager.instance.producibleManager.ProduceUnit());
        btnSetSpawnPoint.onClick.AddListener(() => GameManager.instance.buildingManager.UpdateSpawnPoint());
        btnSetPrimary.onClick.AddListener(() => GameManager.instance.buildingManager.SetPrimary());
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOnThisUI = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOnThisUI = false;
    }

    public void Show(Building building)
    {
        txtBuildingName.text = building.data.strName;
        imgBuilding.sprite = building.data.image;

        if (building.data.canProduceUnit)
        {
            productionParent.SetActive(true);
            txtProductionName.text = "Produce : " + building.data.producible.strName;
            imgProduction.sprite = building.data.producible.image;

            if (building.isPrimary)
            {
                btnSetPrimary.gameObject.SetActive(false);
                txtPrimaryInfo.text = "This building is primary";
            }
            else
            {
                btnSetPrimary.gameObject.SetActive(true);
                txtPrimaryInfo.text = "Set this building primary";
            }
        }
        else productionParent.SetActive(false);

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        isPointerOnThisUI = false;
    }
}