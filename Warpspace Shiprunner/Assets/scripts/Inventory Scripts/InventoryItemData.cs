using UnityEngine;

[CreateAssetMenu(menuName = "Inventory item Data")]
public class InventoryItemData : ScriptableObject
{

    public string id;
    public string displayName;
    public Sprite icon;
    public GameObject prefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
