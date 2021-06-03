using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance;

    public GameManager gameManager;

    // camera
    public Vector2 minCameraPos;
    public Vector2 maxCameraPos;
    public float cameraMoveEdgePadding;
    public float cameraMovementSpeed;
    public float cameraMinZoom;
    public float cameraMaxZoom;
    private Vector3 _cameraMoveDir;

    // current building
    public GameObject imgCurrentBuilding;
    public BuildingData currentBuilding;

    // selection things
    public GameObject selectionBox;
    private Node _selectionStartNode;
    private bool _isShiftDown = false;
    public bool IsShiftPressed
    {
        get { return _isShiftDown; }
    }

    void Start()
    {
        instance = this;
        gameManager = GameManager.instance;
        _cameraMoveDir = Vector3.zero;
        selectionBox.SetActive(false);
    }

    void Update()
    {
        // get mouse position and update _cameraMoveDir with this information
        _cameraMoveDir = Vector3.zero;

        if (Input.mousePosition.x <= cameraMoveEdgePadding || Input.GetKey(KeyCode.A))
            _cameraMoveDir += Vector3.left;
        else if (Input.mousePosition.x >= Screen.width - cameraMoveEdgePadding || Input.GetKey(KeyCode.D))
            _cameraMoveDir += Vector3.right;

        if (Input.mousePosition.y >= Screen.height - cameraMoveEdgePadding || Input.GetKey(KeyCode.W))
            _cameraMoveDir += Vector3.up;
        else if (Input.mousePosition.y <= cameraMoveEdgePadding || Input.GetKey(KeyCode.S))
            _cameraMoveDir += Vector3.down;

        // handle mouse scroll event
        if(Input.mouseScrollDelta.y != 0.0f)
        {
            if(gameManager.uiManager.IsPointerOnUIs())
                gameManager.uiManager.ScrollEvent(Input.mouseScrollDelta.y);
            else
            {
                Camera.main.orthographicSize -= Input.mouseScrollDelta.y;
                Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, cameraMinZoom, cameraMaxZoom);
            }
        }

        // update shift status
        _isShiftDown = Input.GetKey(KeyCode.LeftShift);

        // player building something
        if (gameManager.playMode == PlayModes.Build)
        {
            Node flagNode = null;
            bool canBuildHere = false;

            if (imgCurrentBuilding != null && currentBuilding != null)
            {
                gameManager.gridManager.UpdateMouseOnNodeByBuildingSize(currentBuilding.width, currentBuilding.height);

                imgCurrentBuilding.transform.position = gameManager.gridManager.hoverNode.transform.position;

                canBuildHere = gameManager.gridManager.CanBuildHere(currentBuilding);
                gameManager.gridManager.HighLightBuildingPlace(currentBuilding, canBuildHere);

                if (canBuildHere)
                {
                    imgCurrentBuilding.GetComponent<SpriteRenderer>().color = Color.green;
                    if (currentBuilding.canProduceUnit)
                    {
                        Vector2 flagPos = gameManager.gridManager.hoverNode.GetGridPos();
                        flagPos.x -= 1;
                        flagPos.y += 1;
                        flagNode = gameManager.gridManager.GetNode(flagPos);
                        if (flagNode)
                            gameManager.buildingManager.ShowSpawnFlag(flagNode.transform.position);
                        else gameManager.buildingManager.HideSpawnFlag();
                    }
                }
                else
                {
                    imgCurrentBuilding.GetComponent<SpriteRenderer>().color = Color.red;
                    gameManager.buildingManager.HideSpawnFlag();
                }
            }

            if (Input.GetMouseButtonDown(0) && canBuildHere && !gameManager.uiManager.IsPointerOnUIs())
            {
                Building building = gameManager.buildingManager.Build(currentBuilding, gameManager.gridManager.hoverNode.GetGridPos(), gameManager.gridManager.hoverNode.transform.position, flagNode);
                gameManager.SetMode(PlayModes.Play);
                gameManager.buildingManager.SetCurrentInspectingBuilding(building);
            }

            if (Input.GetKeyDown(KeyCode.Escape))
                gameManager.SetMode(PlayModes.Play);
        }
        else if (gameManager.playMode == PlayModes.UpdateSpawnPoint) // player changing spawn point of a building
        {
            bool canPlaceSpawnPointHere = gameManager.gridManager.CanPlaceSpawnPointHere();
            gameManager.buildingManager.UpdateNewSpawnPonintPos(gameManager.gridManager.hoverNode.transform.position);
            gameManager.buildingManager.UpdateNewSpawnPointColor(canPlaceSpawnPointHere ? Color.white : Color.red);

            if(Input.GetMouseButtonDown(0) && canPlaceSpawnPointHere && !gameManager.uiManager.IsPointerOnUIs())
            {
                gameManager.SetMode(PlayModes.Play);
                gameManager.buildingManager.SetSpawnPoint(gameManager.gridManager.hoverNode);
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                gameManager.SetMode(PlayModes.Play);
                gameManager.buildingManager.HideNewSpawnPointFlag();
            }
        }
        else // game is on play mode
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                gameManager.buildingManager.SetCurrentInspectingBuilding(null);

            if (Input.GetMouseButtonDown(0) && !gameManager.uiManager.IsPointerOnUIs())
                _selectionStartNode = gameManager.gridManager.hoverNode;

            if(Input.GetMouseButton(0) && _selectionStartNode != null)
            {
                if (_selectionStartNode != gameManager.gridManager.hoverNode)
                    gameManager.buildingManager.SetCurrentInspectingBuilding(null);

                UpdateSelectionBox(gameManager.gridManager.hoverNode);
            }

            if (Input.GetMouseButtonUp(0) && _selectionStartNode != null)
            {
                selectionBox.SetActive(false);

                if (_selectionStartNode != gameManager.gridManager.hoverNode)
                    gameManager.producibleManager.SetSelectedProducibles(gameManager.gridManager.GetSelectedNodes(_selectionStartNode, gameManager.gridManager.hoverNode));
                else if (!gameManager.producibleManager.unitSelectedWithLastClick)
                {
                    gameManager.producibleManager.DeselectPreviousSelectedProducibles();
                    gameManager.uiManager.UpdateSoldierCounter();

                    if (!gameManager.buildingManager.buildingSelectedWithLastClick)
                        gameManager.buildingManager.SetCurrentInspectingBuilding(null);
                    else gameManager.buildingManager.buildingSelectedWithLastClick = false;
                }
                else
                {
                    if (!gameManager.buildingManager.buildingSelectedWithLastClick)
                        gameManager.buildingManager.SetCurrentInspectingBuilding(null);
                    else gameManager.buildingManager.buildingSelectedWithLastClick = false;

                    gameManager.producibleManager.unitSelectedWithLastClick = false;
                }

                _selectionStartNode = null;
            }

            if(Input.GetMouseButtonDown(1) && gameManager.producibleManager.IsThereAnySelectedProducible() && gameManager.gridManager.hoverNode.isWalkable)
                gameManager.producibleManager.SetSelectedProduciblePath(gameManager.gridManager.hoverNode);
        }
    }

    private void FixedUpdate()
    {
        if(_cameraMoveDir != Vector3.zero)
        {
            Vector3 pos = transform.position + _cameraMoveDir * cameraMovementSpeed * Time.deltaTime;
            pos.x = Mathf.Clamp(pos.x, minCameraPos.x, maxCameraPos.x);
            pos.y = Mathf.Clamp(pos.y, minCameraPos.y, maxCameraPos.y);
            transform.position = pos;
        }
    }

    public void HideCurrentBuilding()
    {
        if (imgCurrentBuilding != null)
            imgCurrentBuilding.SetActive(false);
    }

    // this method updating selection box by currentHoverNode
    public void UpdateSelectionBox(Node currentHoverNode)
    {
        if (_selectionStartNode != currentHoverNode)
        {
            selectionBox.SetActive(true);

            // we can keep this default values when mouse moving to right + up
            int increaseX = 1;
            int increaseY = 1;
            Vector3 pos = _selectionStartNode.transform.position;

            // mouse moving to left
            if (_selectionStartNode.transform.position.x > currentHoverNode.transform.position.x)
            {
                increaseX = -1;
                pos.x += 1.0f;
            }

            // mouse moving to down
            if (_selectionStartNode.transform.position.y > currentHoverNode.transform.position.y)
            {
                increaseY = -1;
                pos.y += 1.0f;
            }

            float scaleX = currentHoverNode.transform.position.x - _selectionStartNode.transform.position.x + increaseX;
            float scaleY = currentHoverNode.transform.position.y - _selectionStartNode.transform.position.y + increaseY;

            selectionBox.transform.position = pos;
            selectionBox.transform.localScale = new Vector3(scaleX, scaleY, 1);
        }else selectionBox.SetActive(false);
    }
}