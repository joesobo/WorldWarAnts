using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldItem : MonoBehaviour {
    private static readonly float dropSpeed = 5f;
    private static readonly float maxDropTime = 5;

    public float attractSpeed = 3f;
    public int attractRadius = 3;
    public float dropTime;

    private PlayerInventoryController playerInventoryController;
    private Item currentItem;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D itemCollider;

    public static WorldItem SpawnWorldItem(Vector3 position, Item item) {
        var transform = Instantiate(ItemAssets.Instance.worldItemPrefab, position, Quaternion.identity);

        var worldItem = transform.GetComponent<WorldItem>();
        worldItem.SetItem(item);

        return worldItem;
    }

    public static void DropItem(Vector3 position, Item item, bool direction) {
        var randomDir = direction ? new Vector3(Random.Range(0.25f, 1f), Random.Range(0.25f, 1f), 0) : new Vector3(Random.Range(-0.25f, -1f), Random.Range(0.25f, 1f), 0);

        var worldItem = SpawnWorldItem(position + (randomDir * dropSpeed), item);
        worldItem.dropTime = maxDropTime;
        worldItem.GetComponent<Rigidbody2D>().AddForce(randomDir * dropSpeed, ForceMode2D.Impulse);
    }

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        itemCollider = GetComponent<BoxCollider2D>();
        playerInventoryController = FindObjectOfType<PlayerInventoryController>();
    }

    private void Update() {
        var minDist = float.MaxValue;
        Collider2D saveCollider = null;

        foreach (var hitCollider in Physics2D.OverlapCircleAll(transform.position, attractRadius)) {
            var distance = Vector2.Distance(transform.position, hitCollider.transform.position);
            var worldItem = hitCollider.gameObject.GetComponent<WorldItem>();
            PlayerController player;

            if (currentItem.IsStackable() && worldItem != null && hitCollider is BoxCollider2D && hitCollider != itemCollider) {
                if (distance < minDist) {
                    minDist = distance;
                    if (worldItem.currentItem.amount < Item.MaxAmount && currentItem.amount < Item.MaxAmount && currentItem.itemType == worldItem.currentItem.itemType) {
                        saveCollider = hitCollider;
                    }
                }
            } else if ((player = hitCollider.GetComponent<PlayerController>()) != null && dropTime <= 0) {
                if (playerInventoryController.HasRoom(currentItem)) {
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

        if (player != null && playerInventoryController.HasRoom(currentItem) && dropTime <= 0) {
            playerInventoryController.AddItem(currentItem);
            DestroySelf();
        } else if (currentItem.IsStackable() && worldItem != null && worldItem != this && currentItem.itemType == worldItem.currentItem.itemType && currentItem.amount < Item.MaxAmount && worldItem.currentItem.amount < Item.MaxAmount) {
            if (currentItem.amount > worldItem.currentItem.amount) {
                worldItem.DestroySelf();
            } else {
                var totalAmount = worldItem.currentItem.amount + currentItem.amount;
                if (totalAmount <= Item.MaxAmount) {
                    worldItem.currentItem.amount += currentItem.amount;
                    DestroySelf();
                } else {
                    worldItem.currentItem.amount = Item.MaxAmount;
                    currentItem.amount = totalAmount - Item.MaxAmount;
                }
                worldItem.dropTime = dropTime;
            }
        }
    }

    private void SetItem(Item newItem) {
        currentItem = newItem;
        spriteRenderer.sprite = newItem.GetSprite();
    }

    private void DestroySelf() {
        Destroy(gameObject);
    }
}
