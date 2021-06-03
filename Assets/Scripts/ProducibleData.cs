using UnityEngine;

[CreateAssetMenu(fileName = "ProducibleData", menuName = "ScriptableObjects/Producible", order = 2)]
public class ProducibleData : ScriptableObject
{
    public Sprite image;
    public GameObject prefab;
    public string strName;
    public float movementSpeed;
    public int width; // 32 pixel is equals to 1 unit
    public int height; // 32 pixel is equals to 1 unit
    public bool unlocked;
    public BuildingData buildToUnlock; // chaining building & producible unlock
    public int objectPoolSize;
    // TODO : maybe we can implement some in game currency things
}