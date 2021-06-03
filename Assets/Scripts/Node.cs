using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour, IHeapItem<Node>
{
    public int gridX;
    public int gridY;
    public bool isBuildable = true;
    public bool isWalkable = true;
    public bool isReserved = false; // this will help us to prevent unit collide
    public bool isSpawnPoint = false;
    public Producible reservedProducible;
    public int gCost;
    public int hCost;
    public Node parent;
    public List<Node> neighbours = new List<Node>();
    private int _heapIndex;
    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public int HeapIndex
    {
        get
        {
            return _heapIndex;
        }
        set
        {
            _heapIndex = value;
        }
    }
    
    public int CompareTo(Node node)
    {
        int compare = fCost.CompareTo(node.fCost);
        if (compare == 0)
            compare = hCost.CompareTo(node.hCost);

        return -compare;
    }

    public void SetGridPos(int x, int y)
    {
        gridX = x;
        gridY = y;
    }

    private void OnMouseEnter()
    {
        GridManager.instance.UpdateMouseOnNode(gridX, gridY);
    }

    public Vector2 GetGridPos()
    {
        return new Vector2(gridX, gridY);
    }
}