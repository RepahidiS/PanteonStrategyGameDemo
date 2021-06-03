using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager instance;

    public GameManager gameManager;
    public GameObject spawnFlag;
    public GameObject newSpawnFlag;
    public List<Building> buildings;
    public bool buildingSelectedWithLastClick = false;

    private Building _inspectingBuilding;
    private List<Building> _buildingObjectPool = new List<Building>();

    private void Start()
    {
        instance = this;
        gameManager = GameManager.instance;
    }

    // this method is filling the object pool with given data
    public void FillObjectPool(BuildingData buildingData)
    {
        for(int i = 0; i < buildingData.objectPoolSize; i++)
        {
            GameObject newBuilding = Instantiate(buildingData.prefab, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity, transform);
            Building building = newBuilding.GetComponent<Building>();

            if(building == null)
            {
                Debug.LogError(buildingData.prefab.name + " this building prefab need to have 'Building' script inside of it.");
                Debug.Break();
            }

            building.data = buildingData;
            building.producible = buildingData.producible;
            building.SetSelectionHighLighter();
            newBuilding.SetActive(false);
            _buildingObjectPool.Add(building);
        }
    }

    // this method returns first object at pool and remove it from pool
    public Building GetObjectFromPool(BuildingData buildingData)
    {
        Building building = _buildingObjectPool.Where(i => i.data == buildingData).FirstOrDefault();
        if (building == null)
        {
            // if this building's pool is empty then re-fill it
            FillObjectPool(buildingData);
            building = GetObjectFromPool(buildingData);
        }else _buildingObjectPool.Remove(building); // remove this building from pool
         
        return building;
    }

    // this method create building with given data
    public Building Build(BuildingData buildingData, Vector2 gridPos, Vector3 worldPos, Node flagNode)
    {
        Building building = GetObjectFromPool(buildingData);
        building.transform.position = worldPos;
        building.spawnPoint = flagNode;
        // if this is first building with this type then make it primary
        building.isPrimary = buildings.Where(i => i.data == buildingData).Count() == 0;
        building.gameObject.SetActive(true);
        buildings.Add(building);

        // keep safe our spawn point
        if(flagNode != null)
        {
            // set spawn point to non-buildable because we don't want to build on spawn point
            gameManager.gridManager.SetNodeBuildable(flagNode.gridY, flagNode.gridX, false, true);
            // and mark this node to spawn point because we don't want to soldiers mark this node to buildable
            building.spawnPoint.isSpawnPoint = true;
            gameManager.gridManager.buildingNodeCount++;
        }

        // set building area to non-buildable and non-walkable
        for (int i = (int)gridPos.y; i > gridPos.y - buildingData.height; i--)
        {
            for (int j = (int)gridPos.x; j < gridPos.x + buildingData.width; j++)
            {
                gameManager.gridManager.SetNodeBuildable(i, j, false);
                gameManager.gridManager.SetNodeWalkable(i, j, false);
            }
        }

        // unlock linked locked buildings and producibles
        gameManager.uiManager.uiProductions.UnlockProductByBuilding(buildingData);
        gameManager.gridManager.buildingNodeCount += buildingData.width * buildingData.height;

        return building;
    }

    // this method is updating building info and producible info from given data
    public void SetCurrentInspectingBuilding(Building building, bool byClick = false)
    {
        if(_inspectingBuilding != null)
            _inspectingBuilding.Deselect();

        _inspectingBuilding = building;

        if(_inspectingBuilding == null)
        {
            gameManager.uiManager.uiInformation.Hide();
            HideSpawnFlag();
        }
        else
        {
            _inspectingBuilding.Select();
            buildingSelectedWithLastClick = byClick;
            gameManager.uiManager.uiInformation.Show(_inspectingBuilding);
            gameManager.producibleManager.DeselectPreviousSelectedProducibles();
            gameManager.uiManager.UpdateSoldierCounter();

            if (_inspectingBuilding.data.canProduceUnit)
                ShowSpawnFlag(_inspectingBuilding.spawnPoint.transform.position);
            else HideSpawnFlag();
        }
    }

    public Building GetCurrentInspectingBuilding()
    {
        return _inspectingBuilding;
    }

    public void ShowSpawnFlag(Vector3 worldPos)
    {
        spawnFlag.SetActive(true);
        spawnFlag.transform.position = worldPos;
    }

    public void HideSpawnFlag()
    {
        spawnFlag.SetActive(false);
    }

    // this method updating game mode to UpdateSpawnPoint and showing spawn flag
    public void UpdateSpawnPoint()
    {
        if(gameManager.playMode == PlayModes.Play && _inspectingBuilding != null)
        {
            newSpawnFlag.SetActive(true);
            gameManager.SetMode(PlayModes.UpdateSpawnPoint);
        }
    }

    public void UpdateNewSpawnPonintPos(Vector3 pos)
    {
        newSpawnFlag.transform.position = pos;
    }

    public void UpdateNewSpawnPointColor(Color color)
    {
        newSpawnFlag.GetComponent<SpriteRenderer>().color = color;
    }

    public void HideNewSpawnPointFlag()
    {
        newSpawnFlag.SetActive(false);
    }

    public void SetSpawnPoint(Node newSpawnPoint)
    {
        // restore to buildable our old spawn point
        _inspectingBuilding.spawnPoint.isSpawnPoint = false;
        gameManager.gridManager.SetNodeBuildable(_inspectingBuilding.spawnPoint.gridY, _inspectingBuilding.spawnPoint.gridX, true, true);

        // set new one
        _inspectingBuilding.spawnPoint = newSpawnPoint;

        // set this node to non-buildable
        _inspectingBuilding.spawnPoint.isSpawnPoint = true;
        gameManager.gridManager.SetNodeBuildable(_inspectingBuilding.spawnPoint.gridY, _inspectingBuilding.spawnPoint.gridX, false, true);

        // update ui and hide flag
        SetCurrentInspectingBuilding(_inspectingBuilding);
        newSpawnFlag.SetActive(false);

        // keep safe our spawn point
        gameManager.gridManager.SetNodeBuildable(_inspectingBuilding.spawnPoint.gridY, _inspectingBuilding.spawnPoint.gridX, false);
    }

    // this method marking inspecting building to primary
    public void SetPrimary()
    {
        Building building = buildings.Where(i => i.data == _inspectingBuilding.data && i.isPrimary).FirstOrDefault();
        if(building != null)
        {
            // restore previous primary building to non-primary
            building.isPrimary = false;
            _inspectingBuilding.isPrimary = true;
            // update ui
            gameManager.uiManager.uiInformation.Show(_inspectingBuilding);
        }
    }

    // this method producing selected producible from primary building
    public void ProduceUnitFromPrimaryBuilding(BuildingData buildingData)
    {
        Building building = buildings.Where(i => i.data == buildingData && i.isPrimary).FirstOrDefault();
        if (building != null)
            gameManager.producibleManager.ProduceUnit(building);
    }
}