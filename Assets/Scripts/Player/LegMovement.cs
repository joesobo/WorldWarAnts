using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegMovement : MonoBehaviour {
    public Transform currentTarget;
    public Transform desiredTarget;
    public float speed = 1;
    public float maxDistance = 0.1f;
    public float maxTargetDist = 0.3f;
    public AnimationCurve yCurve;

    private float timer = 0;
    private bool moving = false;

    private void Start() {
        currentTarget.position = FindObjectOfType<PlayerController>().transform.position + desiredTarget.position;
    }

    private void Update() {
        float dist = Vector2.Distance(transform.position, currentTarget.position);

        if (dist > maxDistance) {
            moving = true;
        }
        else if (dist == 0) {
            moving = false;
        }

        // Move Foot
        if (moving) {
            timer += Time.deltaTime;
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(desiredTarget.position.x, currentTarget.position.y + yCurve.Evaluate(timer)), speed * Time.deltaTime);
        }
        // Clamp Foot
        else {
            transform.position = currentTarget.position;
            timer = 0;
        }
    }

    private void FixedUpdate() {
        // transform.position = Vector2.MoveTowards(transform.position, currentTarget.position, speed * Time.deltaTime);

        float dist = Vector2.Distance(currentTarget.position, desiredTarget.position);

        if (dist > maxTargetDist) {
            currentTarget.position = desiredTarget.position;
        }
    }
}
