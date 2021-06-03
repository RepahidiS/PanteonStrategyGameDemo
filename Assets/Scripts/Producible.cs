using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Producible : MonoBehaviour
{
    public GameManager gameManager;

    public ProducibleData data;
    public Node currentNode;
    private GameObject _selectionHighlighter;
    private List<Node> _path;

    private float _clickCount = 0;
    private float _clickTime = 0;
    private float _clickDelay = 0.2f;

    private void Start()
    {
        gameManager = GameManager.instance;

        if(transform.childCount == 0)
        {
            Debug.LogError(name + " this producible need to have a selection highlighter.");
            Debug.Break();
        }

        _selectionHighlighter = transform.GetChild(0).gameObject;
        Deselect();
        _path = new List<Node>();
    }

    private void FixedUpdate()
    {
        if (_path.Count > 0)
        {
            transform.position = Vector2.MoveTowards(transform.position, _path[0].transform.position, data.movementSpeed * Time.deltaTime);

            // this control will prevent us from some never ending movements
            if (Vector2.Distance(transform.position, _path[0].transform.position) <= 0.1f)
            {
                if(currentNode.reservedProducible == this)
                {
                    gameManager.gridManager.SetNodeReserved(currentNode.gridY, currentNode.gridX, false, null);
                    gameManager.gridManager.SetNodeBuildable(currentNode.gridY, currentNode.gridX, true);
                    //currentNode.GetComponent<SpriteRenderer>().color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                }

                // set current node to _path[0]
                currentNode = _path[0];

                // set position to _path[0]'s position
                transform.position = _path[0].transform.position;

                // remove current waypoint from _path
                _path.RemoveAt(0);
            }
        }
        else if(currentNode.isReserved && currentNode.reservedProducible != this)
        {
            Node walkableNode = gameManager.gridManager.GetClosestNotReservedNode(currentNode, this);
            if (walkableNode == null)
            {
                Debug.Log("this shouldn't be happening...");
                gameManager.uiManager.ShowMessage("There is no empty space to move selected units.");
                return;
            }

            gameManager.producibleManager.SetProduciblePath(this, walkableNode);
        }
        else if(transform.position != currentNode.transform.position)
        {
            if (Vector2.Distance(transform.position, currentNode.transform.position) > 0.1f)
                transform.position = Vector2.MoveTowards(transform.position, currentNode.transform.position, data.movementSpeed * Time.deltaTime);
            else
            {
                transform.position = currentNode.transform.position;
                //currentNode.GetComponent<SpriteRenderer>().color = Color.red;
            }
        }
    }

    // this method updating path of producible
    // helping us to change node reserved and buildable status
    public void SetPath(List<Node> path)
    {
        // if path is changed
        if(_path.Count > 0)
        {
            // if that node is not reserved to another unit
            if(_path[_path.Count - 1].reservedProducible == this)
            {
                // destination node has changed
                if((path != null && path.Count > 0 && path[path.Count - 1] != _path[_path.Count - 1])
                || (path == null || path.Count == 0))
                {
                    // make notReserved & buildable current destination node
                    gameManager.gridManager.SetNodeReserved(_path[_path.Count - 1].gridY, _path[_path.Count - 1].gridX, false, null);
                    gameManager.gridManager.SetNodeBuildable(_path[_path.Count - 1].gridY, _path[_path.Count - 1].gridX, true);

                    //_path[_path.Count - 1].GetComponent<SpriteRenderer>().color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                }
            }
        }

        _path.Clear();

        if(path != null)
        {
            _path = path;

            if(_path.Count > 0)
            {
                if(_path[_path.Count - 1].reservedProducible != this)
                {
                    // make reserved & non-buildable current destination node
                    gameManager.gridManager.SetNodeReserved(_path[_path.Count - 1].gridY, _path[_path.Count - 1].gridX, true, this);
                    gameManager.gridManager.SetNodeBuildable(_path[_path.Count - 1].gridY, _path[_path.Count - 1].gridX, false);

                    //_path[_path.Count - 1].GetComponent<SpriteRenderer>().color = Color.red;
                }
            }
        }
    }

    // this method handling clicks and double clicks and helping us to select producibles
    private void OnMouseDown()
    {
        if(gameManager.playMode == PlayModes.Play)
        {
            _clickCount++;
            if (_clickCount == 1)
            {
                _clickTime = Time.time;
                gameManager.producibleManager.SetSelectedProducible(this);
            }
            else if (_clickCount > 1 && Time.time - _clickTime > _clickDelay)
            {
                _clickCount = 1;
                _clickTime = Time.time;
                gameManager.producibleManager.SetSelectedProducible(this);
            }
            else if (_clickCount > 1 && Time.time - _clickTime < _clickDelay)
            {
                _clickCount = 0;
                gameManager.producibleManager.SetSelectedProducibleAll(this);
            }
        }
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