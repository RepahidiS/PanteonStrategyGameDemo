using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayModes
{
    Play,
    Build,
    UpdateSpawnPoint
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public PlayModes playMode = PlayModes.Play;
    public Player player;
    public UIManager uiManager;
    public GridManager gridManager;
    public BuildingManager buildingManager;
    public ProducibleManager producibleManager;

    void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        gridManager.HideGrid();
    }

    // this method just managing game mode and updating related things
    public void SetMode(PlayModes mode)
    {
        if(mode == PlayModes.Play)
        {
            if(playMode == PlayModes.Build)
            {
                player.imgCurrentBuilding.SetActive(false);
                player.imgCurrentBuilding = null;
                player.currentBuilding = null;
            }
            
            uiManager.uiExitBuildMode.Hide();
            gridManager.HideGrid();

            if(playMode != PlayModes.UpdateSpawnPoint)
                buildingManager.HideSpawnFlag();
        }
        else // build mode or updating spawn point
        {
            uiManager.uiExitBuildMode.Show();

            if (mode == PlayModes.UpdateSpawnPoint)
                gridManager.ShowWalkableGrid();
            else gridManager.ShowBuildableGrid();
        }        

        playMode = mode;
    }
}