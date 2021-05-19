using UnityEngine;

public class ItemAssets : MonoBehaviour {
    public static ItemAssets Instance { get; private set; }

    private void Awake() {
        Instance = this;
    }

    public Transform worldItemPrefab;

    public Sprite dirtSprite;
    public Sprite stoneSprite;
    public Sprite testSprite;
}
