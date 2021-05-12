using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldItem : MonoBehaviour {
    private const float DropSpeed = 5f;
    private Item item;
    private SpriteRenderer spriteRenderer;

    public static WorldItem SpawnWorldItem(Vector3 position, Item item) {
        var transform = Instantiate(ItemAssets.Instance.worldItemPrefab, position, Quaternion.identity);

        var worldItem = transform.GetComponent<WorldItem>();
        worldItem.SetItem(item);

        return worldItem;
    }

    public static WorldItem DropItem(Vector3 position, Item item) {
        var randomDir = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), 0);
        var worldItem = SpawnWorldItem(position + (randomDir * DropSpeed), item);
        worldItem.GetComponent<Rigidbody2D>().AddForce(randomDir * DropSpeed, ForceMode2D.Impulse);
        return worldItem;
    }

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collider) {
        //TODO: if within trigger area start moving towards collider
    }

    private void OnCollisionEnter2D(Collision2D collisionInfo) {
        var player = collisionInfo.gameObject.GetComponent<PlayerController>();
        var worldItem = collisionInfo.gameObject.GetComponent<WorldItem>();

        if (player != null) {
            player.AddToInventory(this);
        }

        if (worldItem != null && worldItem != this && item.itemType == worldItem.item.itemType) {
            if (item.amount > worldItem.item.amount) {
                worldItem.DestroySelf();
            } else {
                worldItem.item.amount += item.amount;
                DestroySelf();
            }
        }
    }

    private void SetItem(Item newItem) {
        item = newItem;
        spriteRenderer.sprite = newItem.GetSprite();
    }

    public Item GetItem() {
        return item;
    }

    public void DestroySelf() {
        Destroy(gameObject);
    }
}
