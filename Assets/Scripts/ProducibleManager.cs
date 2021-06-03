using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ProducibleManager : MonoBehaviour
{
    public static ProducibleManager instance;

    public GameManager gameManager;

    public List<Producible> spawnedProducibles = new List<Producible>();
    public bool unitSelectedWithLastClick = false;

    private List<Producible> _selectedProducibles = new List<Producible>();
    private List<Producible> _producibleObjectPool = new List<Producible>();

    private void Start()
    {
        instance = this;
        gameManager = GameManager.instance;
    }

    // this method updating selected producible by given data
    public void SetSelectedProducible(Producible producible)
    {
        if(!gameManager.uiManager.IsPointerOnUIs())
        {
            unitSelectedWithLastClick = true;

            if(!gameManager.player.IsShiftPressed)
                DeselectPreviousSelectedProducibles();

            if (_selectedProducibles.Where(i => i == producible).Count() > 0)
            {
                _selectedProducibles.Remove(producible);
                producible.Deselect();
            }
            else
            {
                _selectedProducibles.Add(producible);
                producible.Select();
            }

            gameManager.uiManager.UpdateSoldierCounter();
        }
    }

    // this method updating selected producibles by given data
    // helping us to select/deselect all soldiers from same type
    public void SetSelectedProducibleAll(Producible producible)
    {
        if (!gameManager.player.IsShiftPressed)
            DeselectPreviousSelectedProducibles();

        // is producible data added before
        foreach (Producible p in spawnedProducibles)
        {
            if (producible.data == p.data)
            {
                if (_selectedProducibles.Where(i => i == p).Count() == 0)
                {
                    _selectedProducibles.Add(p);
                    p.Select();
                }
            }
        }

        unitSelectedWithLastClick = true;
        gameManager.buildingManager.SetCurrentInspectingBuilding(null);
        gameManager.uiManager.UpdateSoldierCounter();
    }

    // this method updating selected producibles by given list
    // helping us to select/deselect soldiers by list and previous selected data
    public void SetSelectedProducibles(List<Node> selectedNodes)
    {
        if (!gameManager.player.IsShiftPressed)
            DeselectPreviousSelectedProducibles();

        if (selectedNodes != null)
        {
            foreach(Node node in selectedNodes)
            {
                foreach(Producible producible in spawnedProducibles)
                {
                    if (node == producible.currentNode)
                    {
                        if (_selectedProducibles.Where(i => i == producible).Count() > 0)
                        {
                            _selectedProducibles.Remove(producible);
                            producible.Deselect();
                        }
                        else
                        {
                            _selectedProducibles.Add(producible);
                            producible.Select();
                        }
                    }
                }
            }

            gameManager.buildingManager.SetCurrentInspectingBuilding(null);
        }

        gameManager.uiManager.UpdateSoldierCounter();
    }

    public void DeselectPreviousSelectedProducibles()
    {
        foreach(Producible producible in _selectedProducibles)
            producible.Deselect();

        _selectedProducibles.Clear();
    }

    // this method is filling the object pool with given data
    public void FillObjectPool(ProducibleData producibleData)
    {
        for (int i = 0; i < producibleData.objectPoolSize; i++)
        {
            GameObject newProducible = Instantiate(producibleData.prefab, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity, transform);
            Producible producible = newProducible.GetComponent<Producible>();

            if (producible == null)
            {
                Debug.LogError(producibleData.prefab.name + " this producible prefab need to have 'Producible' script inside of it.");
                Debug.Break();
            }

            producible.data = producibleData;
            newProducible.SetActive(false);

            _producibleObjectPool.Add(producible);
        }
    }

    // this method returns first object at pool and remove it from pool
    public Producible GetObjectFromPool(ProducibleData producibleData)
    {
        Producible producible = _producibleObjectPool.Where(i => i.data == producibleData).FirstOrDefault();
        if (producible == null)
        {
            // if this producible's pool is empty then re-fill it
            FillObjectPool(producibleData);
            producible = GetObjectFromPool(producibleData);
        }else _producibleObjectPool.Remove(producible); // remove this producible from pool

        return producible;
    }

    // this method create producible with given data
    public void ProduceUnit(Building building = null)
    {
        if (gameManager.playMode != PlayModes.Play)
            return;

        if(building == null)
            building = gameManager.buildingManager.GetCurrentInspectingBuilding();

        Node spawnableNode = gameManager.gridManager.GetClosestNotReservedNode(building.spawnPoint);
        if (spawnableNode == null)
        {
            gameManager.uiManager.ShowMessage("There is no empty space to create new units.");
            return;
        }

        Producible newProducible = GetObjectFromPool(building.producible);
        newProducible.transform.position = spawnableNode.transform.position;
        newProducible.data = building.producible;
        newProducible.currentNode = spawnableNode;
        newProducible.gameObject.SetActive(true);
        spawnedProducibles.Add(newProducible);

        // make this node to not buildable because we dont want to build anything above them ^^
        gameManager.gridManager.SetNodeBuildable(spawnableNode.gridY, spawnableNode.gridX, false);
        // make this node to reserved because we dont want to other units can run to this node
        gameManager.gridManager.SetNodeReserved(spawnableNode.gridY, spawnableNode.gridX, true, newProducible);

        gameManager.uiManager.UpdateSoldierCounter();
    }

    public bool IsThereAnySelectedProducible()
    {
        return _selectedProducibles.Count > 0;
    }

    // this method updating selected producible's path by destination node
    public void SetSelectedProduciblePath(Node destination)
    {
        foreach (Producible producible in _selectedProducibles)
        {
            if(!destination.isReserved || destination.reservedProducible == producible)
                SetProduciblePath(producible, destination);
            else // if destination is reserved to some other producible
            {
                Node walkableNode = gameManager.gridManager.GetClosestNotReservedNode(destination);
                if (walkableNode == null)
                {
                    if (gameManager.gridManager.IsEveryNodeIsFilled())
                        gameManager.uiManager.ShowMessage("There is no empty space to move selected units.");
                    else gameManager.uiManager.ShowMessage("There is not enough empty space to move selected units together.");
                    return;
                }

                SetProduciblePath(producible, walkableNode);
            }
        }
    }

    // this method setting producible's path by destination node
    public void SetProduciblePath(Producible producible, Node destination)
    {
        List<Node> path = gameManager.gridManager.FindPath(producible.currentNode, destination);
        if (path != null)
            producible.SetPath(path);
        else Debug.Log("path is null");
    }

    public string GetSoldierCounter()
    {
        return _selectedProducibles.Count.ToString() + " / " + spawnedProducibles.Count.ToString() + " Soldier selected";
    }
}