using UnityEngine;

public class PlayerController : MonoBehaviour {
    public Vector3 velocity;
    public float speed = 5;
    public float walkAcceleration = 1;

    public int jumpforce = 5;
    public float jumpDecelleration = 1;

    public float groundedRayLength = 0.1f;
    public LayerMask layerMaskForGrounded;

    private Rigidbody2D rb;
    private Collider2D boxCollider;
    private Transform bodyController;
    private TerrainNoise terrainNoise;
    [HideInInspector]
    public bool facingRight = true;
    private bool facingNeedsUpdating;
    private readonly Vector3 leftFacing = new Vector3(-1, 1, 1);

    [HideInInspector] public PlayerInventoryController playerInventoryController;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<Collider2D>();
        playerInventoryController = FindObjectOfType<PlayerInventoryController>();
        bodyController = transform.Find("BodyController");
        terrainNoise = FindObjectOfType<TerrainNoise>();
    }

    private void Start() {
        //spawn player on surface
        var x = transform.position.x;
        var result = terrainNoise.Noise1D(x);
        transform.position = new Vector3(x, result + 1.5f, 0);
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

        if (facingNeedsUpdating) {
            bodyController.localScale = facingRight ? Vector3.one : leftFacing;
            facingNeedsUpdating = false;
        }
    }

    private void FixedUpdate() {
        var xInput = Input.GetAxisRaw("Horizontal");

        //moving right
        if (xInput > 0) {
            if (!facingRight) {
                facingRight = true;
                facingNeedsUpdating = true;
            }
        }
        //moving left
        else if (xInput < 0) {
            if (facingRight) {
                facingRight = false;
                facingNeedsUpdating = true;
            }
        }

        velocity.x = Mathf.MoveTowards(velocity.x, (speed / 100) * xInput, walkAcceleration * Time.deltaTime);

        rb.velocity = velocity;
    }

    private bool IsGrounded() {
        var bounds = boxCollider.bounds;
        var hit = Physics2D.BoxCast(bounds.center, bounds.size, 0f, Vector2.down, groundedRayLength, layerMaskForGrounded.value);
        return hit.collider;
    }
}