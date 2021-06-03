using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "ScriptableObjects/Building", order = 1)]
public class BuildingData : ScriptableObject
{
    public Sprite image;
    public GameObject prefab;
    public string strName;
    public int width; // 32 pixel is equals to 1 unit
    public int height; // 32 pixel is equals to 1 unit
    public bool canProduceUnit;
    public ProducibleData producible;
    public int secureSpawnWidth;  // this will help us to keep our spawn paths are safe
    public int secureSpawnHeight; // this will help us to keep our spawn paths are safe
    public bool unlocked;
    public BuildingData buildToUnlock; // chaining building & producible unlock
    public int objectPoolSize;
}