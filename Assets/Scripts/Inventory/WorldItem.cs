using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldItem : MonoBehaviour {
    public float attractSpeed = 3f;
    public int attractRadius = 3;
    public static float dropSpeed = 5f;
    private Item item;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D itemCollider;

    public static WorldItem SpawnWorldItem(Vector3 position, Item item) {
        var transform = Instantiate(ItemAssets.Instance.worldItemPrefab, position, Quaternion.identity);

        var worldItem = transform.GetComponent<WorldItem>();
        worldItem.SetItem(item);

        return worldItem;
    }

    public static WorldItem DropItem(Vector3 position, Item item) {
        var randomDir = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), 0);
        var worldItem = SpawnWorldItem(position + (randomDir * dropSpeed), item);
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

            if (distance < minDist && hitCollider != itemCollider) {
                var type = hitCollider.GetType();

                minDist = distance;
                if (typeof(BoxCollider2D) == type && item.itemType == worldItem.item.itemType) {
                    saveCollider = hitCollider;
                } else if (hitCollider.GetComponent<PlayerController>() != null) {
                    saveCollider = hitCollider;
                    break;
                }
            }
        }

        if (saveCollider != null) {
            transform.position = Vector2.MoveTowards(transform.position, saveCollider.transform.position, (1 / minDist) * attractSpeed * Time.deltaTime);
        }
    }

    private void OnCollisionEnter2D(Collision2D collisionInfo) {
        var player = collisionInfo.gameObject.GetComponent<PlayerController>();
        var worldItem = collisionInfo.gameObject.GetComponent<WorldItem>();

        if (player != null) {
            player.AddToInventory(this);
        } else if (worldItem != null && worldItem != this && item.itemType == worldItem.item.itemType) {
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
