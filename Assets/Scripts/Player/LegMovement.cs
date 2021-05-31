using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegMovement : MonoBehaviour {
    public Transform currentTarget;
    public Transform desiredTarget;
    public float speed = 1;
    public AnimationCurve yCurve;

    private float timer;

    private void Update() {
        float dist = Vector2.Distance(transform.position, currentTarget.position);

        // Move Foot
        if (dist > 0.01f) {
            timer = 0;
            timer += Time.deltaTime;
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(currentTarget.position.x, currentTarget.position.y + yCurve.Evaluate(timer)), speed * Time.deltaTime);
        } 
        // Clamp Foot
        else {
            transform.position = currentTarget.position;
        }
    }

    private void FixedUpdate() {
        transform.position = Vector2.MoveTowards(transform.position, currentTarget.position, speed * Time.deltaTime);

        float dist = Vector2.Distance(currentTarget.position, desiredTarget.position);

        if (dist > 0.3f) {
            currentTarget.position = desiredTarget.position;
        }
    }
}
