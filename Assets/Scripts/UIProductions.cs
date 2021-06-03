using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Linq;

public class ProductionButton
{
    public GameObject gameObject;
    public GameObject preview;
    public object data;
    public BuildingData buildToUnlock;

    public ProductionButton(GameObject gameObject, GameObject preview, object data, BuildingData buildToUnlock)
    {
        this.gameObject = gameObject;
        this.preview = preview;
        this.data = data;
        this.buildToUnlock = buildToUnlock;
    }
}

public class UIProductions : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Transform content;
    public GameObject btnPrefab;
    public List<BuildingData> buildings;
    public float scrollSpeed;
    public float topY;
    public float bottomY;
    [HideInInspector] public bool isPointerOnThisUI = false;
    private List<ProductionButton> productionButtons = new List<ProductionButton>();

    // this method creating and positioning ui buttons from buildings list data
    public void CreateElements()
    {
        int x = 70;
        int y = 870;

        for(int i = 0; i < buildings.Count; i++)
        {
            BuildingData buildingData = buildings[i];

            GameObject buildingButton = Instantiate(btnPrefab, content);
            buildingButton.name = buildingData.strName;
            buildingButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
            buildingButton.GetComponent<RectTransform>().sizeDelta = new Vector2(140.0f, 140.0f);
            buildingButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = buildingData.strName;

            GameObject preview = Instantiate(buildingData.prefab, null);
            preview.SetActive(false);
            preview.GetComponent<SpriteRenderer>().sortingOrder = 4;

            GameManager.instance.buildingManager.FillObjectPool(buildingData);
            Button btn = buildingButton.GetComponent<Button>();
            if (buildingData.buildToUnlock == null)
            {
                btn.image.sprite = buildingData.image;
                btn.onClick.AddListener(() => OnClickEvent(buildingData, preview));
            }
            else
            {
                productionButtons.Add(new ProductionButton(buildingButton, preview, buildingData, buildingData.buildToUnlock));
                btn.onClick.AddListener(() => LockedOnClickEvent(buildingData.buildToUnlock.strName));
            }

            if (x == 70)
                x = 220;
            else if (x == 220)
            {
                x = 70;
                y -= 190;
            }

            if (buildingData.canProduceUnit)
            {
                GameManager.instance.producibleManager.FillObjectPool(buildingData.producible);
                GameObject productionButton = Instantiate(btnPrefab, content);
                productionButton.name = buildingData.producible.strName;
                productionButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
                productionButton.GetComponent<RectTransform>().sizeDelta = new Vector2(140.0f, 140.0f);
                productionButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = buildingData.producible.strName;

                btn = productionButton.GetComponent<Button>();
                if (buildingData.producible.buildToUnlock == null)
                {
                    btn.image.sprite = buildingData.producible.image;
                    btn.onClick.AddListener(() => OnClickEvent(buildingData));
                }
                else
                {
                    productionButtons.Add(new ProductionButton(productionButton, null, buildingData.producible, buildingData));
                    btn.onClick.AddListener(() => LockedOnClickEvent(buildingData.strName));
                }

                if (x == 70)
                    x = 220;
                else if (x == 220)
                {
                    x = 70;
                    y -= 190;
                }
            }
        }
    }

    // this method showing a message about how to unlock clicked building
    private void LockedOnClickEvent(string buildingName)
    {
        GameManager.instance.uiManager.ShowMessage("You have to build at least one '" + buildingName + "' to unlock this production.");
    }

    // creating buildings
    private void OnClickEvent(BuildingData buildingData, GameObject preview)
    {
        if (GameManager.instance.playMode == PlayModes.UpdateSpawnPoint)
            return;

        GameManager.instance.buildingManager.SetCurrentInspectingBuilding(null);
        preview.SetActive(true);
        Player.instance.HideCurrentBuilding();
        Player.instance.imgCurrentBuilding = preview;
        Player.instance.currentBuilding = buildingData;
        GameManager.instance.SetMode(PlayModes.Build);
    }

    // creating producibles
    private void OnClickEvent(BuildingData buildingData)
    {
        GameManager.instance.buildingManager.ProduceUnitFromPrimaryBuilding(buildingData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //content.gameObject.GetComponent<GridLayoutGroup>().enabled = false;
        isPointerOnThisUI = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOnThisUI = false;
    }

    // this method scrolling up production buttons by scrollDelta
    // helping us to make "infinite scrollview" work
    public void ScrollUp(float scrollDelta)
    {
        Vector2 pos;
        for(int i = 0; i < content.childCount; i++)
        {
            Transform child = content.GetChild(i);
            pos = child.GetComponent<RectTransform>().anchoredPosition;
            pos.y += scrollDelta * scrollSpeed;
            child.GetComponent<RectTransform>().anchoredPosition = pos;
        }

        pos = content.GetChild(content.childCount - 1).GetComponent<RectTransform>().anchoredPosition;
        if (pos.y < bottomY)
        {
            pos.y = content.GetChild(0).GetComponent<RectTransform>().anchoredPosition.y;
            pos.y += 190;
            content.GetChild(content.childCount - 1).GetComponent<RectTransform>().anchoredPosition = pos;
            content.GetChild(content.childCount - 1).SetSiblingIndex(0);

            // new last child
            pos.x = content.GetChild(content.childCount - 1).GetComponent<RectTransform>().anchoredPosition.x;
            content.GetChild(content.childCount - 1).GetComponent<RectTransform>().anchoredPosition = pos;
            content.GetChild(content.childCount - 1).SetSiblingIndex(0);
        }
    }

    // this method scrolling down production buttons by scrollDelta
    // helping us to make "infinite scrollview" work
    public void ScrollDown(float scrollDelta)
    {
        Vector2 pos;
        for (int i = 0; i < content.childCount; i++)
        {
            Transform child = content.GetChild(i);
            pos = child.GetComponent<RectTransform>().anchoredPosition;
            pos.y += scrollDelta * scrollSpeed;
            child.GetComponent<RectTransform>().anchoredPosition = pos;
        }

        pos = content.GetChild(0).GetComponent<RectTransform>().anchoredPosition;
        if (pos.y > topY)
        {
            pos.y = content.GetChild(content.childCount - 1).GetComponent<RectTransform>().anchoredPosition.y;
            pos.y -= 190;
            content.GetChild(0).GetComponent<RectTransform>().anchoredPosition = pos;
            content.GetChild(0).SetSiblingIndex(content.childCount - 1);

            // new first child
            pos.x = content.GetChild(0).GetComponent<RectTransform>().anchoredPosition.x;
            content.GetChild(0).GetComponent<RectTransform>().anchoredPosition = pos;
            content.GetChild(0).SetSiblingIndex(content.childCount - 1);
        }
    }

    // this method unlocking buildings/producibles by buildingData
    public void UnlockProductByBuilding(BuildingData buildingData)
    {
        List<ProductionButton> unlockProductionButtons = productionButtons.Where(i => i.buildToUnlock == buildingData).ToList();
        if(unlockProductionButtons != null)
        {
            foreach(ProductionButton productionButton in unlockProductionButtons)
            {
                Button btn = productionButton.gameObject.GetComponent<Button>();
                if (productionButton.data is BuildingData)
                {
                    BuildingData data = (BuildingData)productionButton.data;
                    btn.image.sprite = data.image;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => OnClickEvent(data, productionButton.preview));
                }
                else
                {
                    ProducibleData data = (ProducibleData)productionButton.data;
                    btn.image.sprite = data.image;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => OnClickEvent(data.buildToUnlock));
                }

                productionButtons.Remove(productionButton);
            }
        }
    }
}