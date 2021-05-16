using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldItem : MonoBehaviour {
    public static float dropSpeed = 5f;
    public static float maxDropTime = 5;

    public float attractSpeed = 3f;
    public int attractRadius = 3;

    public float dropTime = 0;
    private Item item;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D itemCollider;

    public static WorldItem SpawnWorldItem(Vector3 position, Item item) {
        var transform = Instantiate(ItemAssets.Instance.worldItemPrefab, position, Quaternion.identity);

        var worldItem = transform.GetComponent<WorldItem>();
        worldItem.SetItem(item);

        return worldItem;
    }

    public static WorldItem DropItem(Vector3 position, Item item, bool direction) {
        Vector3 randomDir;

        if (direction) {
            randomDir = new Vector3(Random.Range(0.25f, 1f), Random.Range(0.25f, 1f), 0);
        } else {
            randomDir = new Vector3(Random.Range(-0.25f, -1f), Random.Range(0.25f, 1f), 0);
        }

        var worldItem = SpawnWorldItem(position + (randomDir * dropSpeed), item);
        worldItem.dropTime = maxDropTime;
        worldItem.GetComponent<Rigidbody2D>().AddForce(randomDir * dropSpeed, ForceMode2D.Impulse);
        return worldItem;
    }

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        itemCollider = GetComponent<BoxCollider2D>();
    }

    private void Update() {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, attractRadius);
        float minDist = float.MaxValue;
        Collider2D saveCollider = null;

        foreach (var hitCollider in hitColliders) {
            var distance = Vector2.Distance(transform.position, hitCollider.transform.position);
            var worldItem = hitCollider.gameObject.GetComponent<WorldItem>();

            if (item.IsStackable() && worldItem != null && typeof(BoxCollider2D) == hitCollider.GetType() && hitCollider != itemCollider) {
                if (distance < minDist) {
                    minDist = distance;
                    if (worldItem.item.amount < Item.maxAmount && item.amount < Item.maxAmount && item.itemType == worldItem.item.itemType) {
                        saveCollider = hitCollider;
                    }
                }
            } else if (hitCollider.GetComponent<PlayerController>() != null && dropTime <= 0) {
                saveCollider = hitCollider;
                break;
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

        if (player != null && dropTime <= 0) {
            player.AddToInventory(this);
        } else if (item.IsStackable() && worldItem != null && worldItem != this && item.itemType == worldItem.item.itemType && worldItem.item.amount < Item.maxAmount) {
            if (item.amount > worldItem.item.amount) {
                worldItem.DestroySelf();
            } else {
                int totalAmount = worldItem.item.amount + item.amount;
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
