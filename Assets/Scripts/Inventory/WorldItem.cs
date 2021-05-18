using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldItem : MonoBehaviour {
    private static readonly float dropSpeed = 5f;
    private static readonly float maxDropTime = 5;

    public float attractSpeed = 3f;
    public int attractRadius = 3;

    public float dropTime;
    private Item item;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D itemCollider;

    public static WorldItem SpawnWorldItem(Vector3 position, Item item) {
        var transform = Instantiate(ItemAssets.Instance.worldItemPrefab, position, Quaternion.identity);

        var worldItem = transform.GetComponent<WorldItem>();
        worldItem.SetItem(item);

        return worldItem;
    }

    public static void DropItem(Vector3 position, Item item, bool direction) {
        Vector3 randomDir;

        if (direction) {
            randomDir = new Vector3(Random.Range(0.25f, 1f), Random.Range(0.25f, 1f), 0);
        } else {
            randomDir = new Vector3(Random.Range(-0.25f, -1f), Random.Range(0.25f, 1f), 0);
        }

        var worldItem = SpawnWorldItem(position + (randomDir * dropSpeed), item);
        worldItem.dropTime = maxDropTime;
        worldItem.GetComponent<Rigidbody2D>().AddForce(randomDir * dropSpeed, ForceMode2D.Impulse);
    }

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        itemCollider = GetComponent<BoxCollider2D>();
    }

    private void Update() {
        var minDist = float.MaxValue;
        Collider2D saveCollider = null;

        foreach (var hitCollider in Physics2D.OverlapCircleAll(transform.position, attractRadius)) {
            var distance = Vector2.Distance(transform.position, hitCollider.transform.position);
            var worldItem = hitCollider.gameObject.GetComponent<WorldItem>();
            PlayerController player;

            if (item.IsStackable() && worldItem != null && hitCollider is BoxCollider2D && hitCollider != itemCollider) {
                if (distance < minDist) {
                    minDist = distance;
                    if (worldItem.item.amount < Item.maxAmount && item.amount < Item.maxAmount && item.itemType == worldItem.item.itemType) {
                        saveCollider = hitCollider;
                    }
                }
            } else if ((player = hitCollider.GetComponent<PlayerController>()) != null && dropTime <= 0) {
                if (player.inventoryController.HasRoom(item)) {
                    saveCollider = hitCollider;
                    break;
                }
            }
        }

        if (saveCollider != null) {
            transform.position = Vector2.MoveTowards(transform.position, saveCollider.transform.position, attractSpeed * Time.deltaTime);
        }

        dropTime -= Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collisionInfo) {
        var player = collisionInfo.gameObject.GetComponent<PlayerController>();
        var worldItem = collisionInfo.gameObject.GetComponent<WorldItem>();

        if (player != null && player.inventoryController.HasRoom(item) && dropTime <= 0) {
            player.inventoryController.AddItem(this.GetItem());
            this.DestroySelf();
        } else if (item.IsStackable() && worldItem != null && worldItem != this && item.itemType == worldItem.item.itemType && item.amount < Item.maxAmount && worldItem.item.amount < Item.maxAmount) {
            if (item.amount > worldItem.item.amount) {
                worldItem.DestroySelf();
            } else {
                var totalAmount = worldItem.item.amount + item.amount;
                if (totalAmount <= Item.maxAmount) {
                    worldItem.item.amount += item.amount;
                    DestroySelf();
                } else {
                    worldItem.item.amount = Item.maxAmount;
                    item.amount = totalAmount - Item.maxAmount;
                }
                worldItem.dropTime = dropTime;
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
