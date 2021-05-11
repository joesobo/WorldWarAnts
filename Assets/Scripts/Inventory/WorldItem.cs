using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldItem : MonoBehaviour {
    private static float dropSpeed = 5f;
    private Item item;
    private SpriteRenderer spriteRenderer;

    public static WorldItem SpawnWorldItem(Vector3 position, Item item) {
        Transform transform = Instantiate(ItemAssets.Instance.worldItemPrefab, position, Quaternion.identity);

        WorldItem worldItem = transform.GetComponent<WorldItem>();
        worldItem.SetItem(item);

        return worldItem;
    }
    
    public static WorldItem DropItem(Vector3 position, Item item) {
        Vector3 randomDir = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), 0);
        WorldItem worldItem = SpawnWorldItem(position + (randomDir * dropSpeed), item);
        worldItem.GetComponent<Rigidbody2D>().AddForce(randomDir * dropSpeed, ForceMode2D.Impulse);
        return worldItem;
    }

    void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetItem(Item item) {
        this.item = item;
        spriteRenderer.sprite = item.GetSprite();
    }

    public Item GetItem() {
        return item;
    }

    public void DestroySelf() {
        Destroy(gameObject);
    }
}
