using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesiredLegTarget : MonoBehaviour {
    private float desiredYPosition;
    
    private void Update() {
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + 1), -Vector2.up, 5f, LayerMask.GetMask("Ground"));
    
        if (hit.collider != null) {
            desiredYPosition = hit.point.y;
        } else {
            desiredYPosition = transform.position.y;
        }

        transform.position = new Vector2(transform.position.x, desiredYPosition);
    }
}
