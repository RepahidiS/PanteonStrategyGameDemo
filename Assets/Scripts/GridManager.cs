using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    // grid infos
    public int topLeftX;
    public int topLeftY;
    public int gridWidth = 0;
    public int gridHeight = 0;
    public GameObject pathPrefab;
    public Node hoverNode;

    // grid colors
    public Color gridCanBuildColor;
    public Color gridCannotBuildColor;
    public Color gridHighlightColor;
    private Color _gridInvisible = new Color(0.0f, 0.0f, 0.0f, 0.0f);

    // path infos
    private Node[,] _nodes;
    private Node _sourceNode;
    private Node _destinationNode;

    // mouse highlighter
    private List<Node> _highlightedNodes = new List<Node>();
    private List<Node> _hoverHighLightedNodes = new List<Node>();

    // counters
    public int buildingNodeCount = 0;
    public int _walkableNodeCount = 0;
    public int _notReservedNodeCount = 0;

    void Start()
    {
        instance = this;
        ParseNodes();
        SetNodeNeighbourhood();
    }

    public int MaxSize
    {
        get
        {
            return gridWidth * gridHeight;
        }
    }

    // this method help us to create all paths with given dimension data
    public void CreatePathNodes()
    {
        if(pathPrefab == null)
        {
            Debug.LogError("Need path prefab to create nodes.");
            Debug.Break();
        }

        int index = 0;
        for(int i = topLeftY; i > topLeftY - gridHeight; i--)
        {
            for(int j = topLeftX; j < topLeftX + gridWidth; j++)
            {
                GameObject currentNode = Instantiate(pathPrefab, new Vector3(j, i, 0.0f), Quaternion.identity, transform);
                currentNode.AddComponent<Node>();
                currentNode.name += " " + index;
                index++;
            }
        }
    }

    // this method parsing created nodes to _nodes array
    private void ParseNodes()
    {
        _nodes = new Node[gridHeight, gridWidth];
        for (int i = 0; i < gridHeight; i++) // row by row
        {
            for (int j = 0; j < gridWidth; j++)
            {
                GameObject currentNode = transform.GetChild(i * gridWidth + j).gameObject;
                currentNode.GetComponent<Node>().SetGridPos(j, i);
                _nodes[i, j] = currentNode.GetComponent<Node>();

                if (_nodes[i, j].isWalkable)
                {
                    _walkableNodeCount++;
                    _notReservedNodeCount++;
                }
            }
        }
    }

    // this method caching node neighbourhood to their "neighbours" list
    private void SetNodeNeighbourhood()
    {
        for(int i = 0; i < gridHeight; i++)
        {
            for(int j = 0; j < gridWidth; j++)
            {
                bool leftEdge = j == 0;
                bool rightEdge = j == gridWidth - 1;

                if (i > 0)
                {
                    _nodes[i, j].neighbours.Add(_nodes[i - 1, j]); // top neighbour

                    if (!leftEdge)
                        _nodes[i, j].neighbours.Add(_nodes[i - 1, j - 1]); // top left neighbour

                    if (!rightEdge)
                        _nodes[i, j].neighbours.Add(_nodes[i - 1, j + 1]); // top right neighbour
                }

                if(i < gridHeight - 1)
                {
                    _nodes[i, j].neighbours.Add(_nodes[i + 1, j]); // bottom neighbour

                    if (!leftEdge)
                        _nodes[i, j].neighbours.Add(_nodes[i + 1, j - 1]); // bottom left neighbour

                    if (!rightEdge)
                        _nodes[i, j].neighbours.Add(_nodes[i + 1, j + 1]); // bottom right neighbour
                }

                if (!leftEdge)
                    _nodes[i, j].neighbours.Add(_nodes[i, j - 1]); // left neighbour

                if (!rightEdge)
                    _nodes[i, j].neighbours.Add(_nodes[i, j + 1]); // right neighbour
            }
        }
    }

    // this method finding shortest path from sourceNode to destinationNode using A* alghorithm
    public List<Node> FindPath(Node sourceNode, Node destinationNode)
    {
        _sourceNode = sourceNode;
        _destinationNode = destinationNode;

        Heap<Node> openSet = new Heap<Node>(MaxSize); // Heap because it's faster than List
        HashSet<Node> closedSet = new HashSet<Node>(); // HashSet because it's faster than List about large data
        openSet.Add(_sourceNode);

        while(openSet.Count > 0)
        {
            Node currentNode = openSet.RemoveFirst();
            closedSet.Add(currentNode);

            if (currentNode == _destinationNode)
                return RetracePath();

            foreach (Node neighbour in currentNode.neighbours)
            {
                if (!neighbour.isWalkable || closedSet.Contains(neighbour))
                    continue;

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if(newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, _destinationNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }

        return null;
    }

    // this method retracing path from destinationNode to sourceNode and reversing it
    private List<Node> RetracePath()
    {
        List<Node> path = new List<Node>();
        Node currentNode = _destinationNode;

        while(currentNode != _sourceNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();

        return path;
    }

    // this method calculating distance between two nodes
    private int GetDistance(Node nodeA, Node nodeB)
    {
        int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);
        return 14 * distX + 10 * (distY - distX);
    }

    // this method updating hover node and highlighting it
    public void UpdateMouseOnNode(int x, int y)
    {
        hoverNode = _nodes[y, x];

        if (GameManager.instance.playMode == PlayModes.Play)
        {
            foreach (Node node in _hoverHighLightedNodes)
                node.GetComponent<SpriteRenderer>().color = _gridInvisible;

            _hoverHighLightedNodes.Clear();

            _hoverHighLightedNodes.Add(hoverNode);
            hoverNode.GetComponent<SpriteRenderer>().color = gridHighlightColor;
            Color edgeColor = new Color(gridHighlightColor.r, gridHighlightColor.g, gridHighlightColor.b, 0.2f);

            foreach(Node node in hoverNode.neighbours)
            {
                _hoverHighLightedNodes.Add(node);
                node.GetComponent<SpriteRenderer>().color = edgeColor;
            }
        }
    }

    // this method updating hover node by building size and helping us to keep pointer at bottom-left on building
    public void UpdateMouseOnNodeByBuildingSize(int buildingWidth, int buildingHeight)
    {
        if(hoverNode.gridX + buildingWidth > gridWidth)
            hoverNode = _nodes[hoverNode.gridY, gridWidth - buildingWidth];

        if (hoverNode.gridY < buildingHeight - 1)
            hoverNode = _nodes[buildingHeight - 1, hoverNode.gridX];
    }

    // this method checking buildable status by buildingData
    public bool CanBuildHere(BuildingData buildingData)
    {
        for(int i = hoverNode.gridY + buildingData.secureSpawnHeight; i > hoverNode.gridY - buildingData.height - buildingData.secureSpawnHeight; i--)
        {
            for(int j = hoverNode.gridX - buildingData.secureSpawnWidth; j < hoverNode.gridX + buildingData.width + buildingData.secureSpawnWidth; j++)
            {
                if (i >= gridHeight || i < 0 || j >= gridWidth || j < 0 || !_nodes[i, j].isBuildable)
                    return false;
            }
        }

        return true;
    }

    public bool CanPlaceSpawnPointHere()
    {
        return hoverNode.isWalkable;
    }

    // this method highlighting building place by buildingData
    // helping us to show current buildable/non-buildable status and showing secureSpawn are around the building
    public void HighLightBuildingPlace(BuildingData buildingData, bool canBuildHere)
    {
        foreach(Node node in _highlightedNodes)
            node.GetComponent<SpriteRenderer>().color = node.isBuildable ? gridCanBuildColor : gridCannotBuildColor;

        _highlightedNodes.Clear();

        for (int i = hoverNode.gridY + buildingData.secureSpawnHeight; i > hoverNode.gridY - buildingData.height - buildingData.secureSpawnHeight; i--)
        {
            for (int j = hoverNode.gridX - buildingData.secureSpawnWidth; j < hoverNode.gridX + buildingData.width + buildingData.secureSpawnWidth; j++)
            {
                if (i >= gridHeight || i < 0 || j >= gridWidth || j < 0)
                    continue;

                _nodes[i, j].GetComponent<SpriteRenderer>().color = canBuildHere ? gridHighlightColor : gridCannotBuildColor;
                _highlightedNodes.Add(_nodes[i, j]);
            }
        }
    }

    public void ShowBuildableGrid()
    {
        foreach(Node node in _nodes)
            node.GetComponent<SpriteRenderer>().color = node.isBuildable ? gridCanBuildColor : gridCannotBuildColor;
    }

    public void ShowWalkableGrid()
    {
        foreach (Node node in _nodes)
            node.GetComponent<SpriteRenderer>().color = node.isWalkable ? gridCanBuildColor : gridCannotBuildColor;
    }

    public void HideGrid()
    {
        foreach (Node node in _nodes)
            node.GetComponent<SpriteRenderer>().color = _gridInvisible;
    }

    // this method updating buildable status of node whose one indexes are given with parameters
    // helping us to prevent from soldiers mark this node to buildable
    public void SetNodeBuildable(int x, int y, bool buildable, bool byBuilding = false)
    {
        if(byBuilding)
            _nodes[x, y].isBuildable = buildable;
        else
        {
            if (buildable)
            {
                // if this node is not a spawn point
                if (!_nodes[x, y].isSpawnPoint)
                    _nodes[x, y].isBuildable = true;
            }else _nodes[x, y].isBuildable = false;
        }
    }

    // this method updating walkable status of node whose one indexes are given with parameters
    // helping us to tracking total walkable node count
    public void SetNodeWalkable(int x, int y, bool walkable)
    {
        if (walkable && !_nodes[x, y].isWalkable)
            _walkableNodeCount++;
        else if (!walkable && _nodes[x, y].isWalkable)
            _walkableNodeCount--;
        
        _nodes[x, y].isWalkable = walkable;
    }

    // this method updating notReserved status of node whose one indexes are given with parameters
    // helping us to tracking total notReserved node count
    public void SetNodeReserved(int x, int y, bool reserved, Producible reservedTo)
    {
        if (reserved && !_nodes[x, y].isReserved)
            _notReservedNodeCount--;
        else if (!reserved && _nodes[x, y].isReserved)
            _notReservedNodeCount++;

        _nodes[x, y].isReserved = reserved;
        _nodes[x, y].reservedProducible = reservedTo;

        // TODO : just for debugging
        //_nodes[x, y].GetComponent<SpriteRenderer>().color = reserved ? _gridInvisible : Color.red;
    }

    public Node GetNode(int x, int y)
    {
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
            return null;
        return _nodes[y, x];
    }

    public Node GetNode(Vector2 pos)
    {
        if (pos.x < 0 || pos.x >= gridWidth || pos.y < 0 || pos.y >= gridHeight)
            return null;
        return _nodes[(int)pos.y, (int)pos.x];
    }

    public Node GetNodeByPos(Vector2 pos)
    {
        for (int i = 0; i < gridHeight; i++)
        {
            for (int j = 0; j < gridWidth; j++)
            {
                if (_nodes[i, j].transform.position.x == pos.x && _nodes[i, j].transform.position.y == pos.y)
                    return _nodes[i, j];
            }
        }

        return null;
    }

    public bool IsEveryNodeIsFilled()
    {
        return _walkableNodeCount == GameManager.instance.producibleManager.spawnedProducibles.Count;
    }

    // this method finding closest notReserved and walkable node from given node
    // helping us soldier spawning to closest node when spawn point is filled by another soldier
    // or moving soldiers to filled node by another soldier
    public Node GetClosestNotReservedNode(Node node, Producible producible = null, int tryCount = 1)
    {
        if ((node.isWalkable && !node.isReserved)
        || (node.isWalkable && producible != null && node.reservedProducible == producible))
            return node;

        if(_walkableNodeCount > 0 && _notReservedNodeCount >= buildingNodeCount)
        {
            for (int i = node.gridY + tryCount; i > node.gridY - 1 - tryCount; i--)
            {
                for (int j = node.gridX - tryCount; j < node.gridX + 1 + tryCount; j++)
                {
                    if (i >= gridHeight || i < 0 || j >= gridWidth || j < 0)
                        continue;

                    if ((_nodes[i, j].isWalkable && !_nodes[i, j].isReserved)
                    || (_nodes[i, j].isWalkable && producible != null && _nodes[i, j].reservedProducible == producible))
                        return _nodes[i, j];
                }
            }

            return GetClosestNotReservedNode(node, producible, tryCount + 1);
        }

        return null;
    }

    // this method converts selection direction to top-right and updating selectedNodes
    public List<Node> GetSelectedNodes(Node startNode, Node endNode)
    {
        // we can keep this default values when selection to right + up
        Vector2 start = new Vector2(startNode.gridX, startNode.gridY);
        Vector2 end = new Vector2(endNode.gridX, endNode.gridY);

        // selection to left - down
        if (start.x > end.x && start.y < end.y)
        {
            // swap start & end positions
            start = new Vector2(endNode.gridX, endNode.gridY);
            end = new Vector2(startNode.gridX, startNode.gridY);
        }

        // selection to left
        if (start.x > end.x)
        {
            start = new Vector2(endNode.gridX, startNode.gridY);
            end = new Vector2(startNode.gridX, endNode.gridY);
        }

        // selection to down
        if (start.y < end.y)
        {
            start = new Vector2(startNode.gridX, endNode.gridY);
            end = new Vector2(endNode.gridX, startNode.gridY);
        }

        List<Node> selectedNodes = new List<Node>();
        for (int i = (int)start.y; i > end.y - 1; i--)
        {
            for (int j = (int)start.x; j < end.x + 1; j++)
                selectedNodes.Add(_nodes[i, j]);
        }

        return selectedNodes;
    }
}