using UnityEngine;

public class PlayerController : MonoBehaviour {
    public Vector3 velocity;
    public float speed = 5;
    public float walkAcceleration = 1;

    public int jumpforce = 5;
    public float jumpDecelleration = 1;

    public float groundedRayLength = 0.1f;
    public LayerMask layerMaskForGrounded;

    [SerializeField] public UI_Inventory uiInventory;
    public int inventorySize = 32;

    private Rigidbody2D rb;
    private Collider2D boxCollider;
    private Inventory inventory;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<Collider2D>();
        inventory = new Inventory(inventorySize);
        uiInventory.SetPlayer(this);
        uiInventory.SetInventory(inventory);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded()) {
            velocity.y = jumpforce;
        } else {
            if (velocity.y > 0) {
                velocity.y -= Time.deltaTime * jumpDecelleration;
            }
        }

        if (velocity.y < 0) {
            velocity.y = 0;
        }
    }

    public void AddToInventory(WorldItem worldItem) {
        inventory.AddItem(worldItem.GetItem());
        worldItem.DestroySelf();
    }

    private void FixedUpdate() {
        var xInput = Input.GetAxisRaw("Horizontal");

        velocity.x = Mathf.MoveTowards(velocity.x, (speed / 100) * xInput, walkAcceleration * Time.deltaTime);

        rb.velocity = velocity;
    }

    private bool IsGrounded() {
        var bounds = boxCollider.bounds;
        var hit = Physics2D.BoxCast(bounds.center, bounds.size, 0f, Vector2.down, groundedRayLength, layerMaskForGrounded.value);
        return hit.collider;
    }
}