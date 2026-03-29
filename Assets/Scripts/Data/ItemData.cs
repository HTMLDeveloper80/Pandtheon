using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Items/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int amount = 1;
    public enum Rarity { Uncommon, Common, Rare, Epic, Legendary };
    public Rarity rarity;
    public float dropChance;
}
