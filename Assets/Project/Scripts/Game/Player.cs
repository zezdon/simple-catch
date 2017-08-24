using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : MonoBehaviour {

    public float speed = 8f;
    public float movementAmplitude = 10F;
    public float jumpingAngle = 45f;
    public float jumpingForce = 10f;
    public float maximumVerticalVelocity = 8f;
    public float maximumVerticalPosition = 9f;
    //referens to the players Model
    public GameObject model;
    public Action onKill;

    private Rigidbody playerRigidbody;
    private Vector3 originalPosition;
    private Vector3 targetPosition;
    //rotation smooth
    private Quaternion targetRotation;
    private Vector2 clickOrigin;
    private bool invincible = false;
    private bool lockZ = false;
    private bool canJump = false;
    private bool demonstrationMode = false;
    private bool lookingLeft = false;
    private float depthRange;
    private float horizontalRange;
    private bool isJumping;

    public bool Invincible { set { invincible = value; } }
    public bool LockZ { set { lockZ = value; } }
    public bool CanJump { set { canJump = value; } }
    public bool DemonstrationMode { set { demonstrationMode = value; } }
    public float DepthRange { set { depthRange = value; } }
    public float HorizontalRange { set { horizontalRange = value; } }

    // Use this for initialization
    void Start() {
        clickOrigin = Vector2.zero;
        targetRotation = Quaternion.identity;
        playerRigidbody = GetComponent<Rigidbody>();

        if (demonstrationMode) {
            playerRigidbody.useGravity = false;
        }
    }

    // Update is called once per frame
    void Update() {
        Vector2 viewportCoordinates = new Vector2(
            Input.mousePosition.x / Screen.width,
            Input.mousePosition.y / Screen.height
            );

        // Clicking or touching

        if (isJumping == false && Input.GetMouseButton(0)) {
            if (clickOrigin == Vector2.zero) {
                originalPosition = transform.position;
                clickOrigin = viewportCoordinates;
            } else {
                Vector2 variation = viewportCoordinates - clickOrigin;
                // Set the player's target position.
                targetPosition = new Vector3(
                    originalPosition.x + variation.x * movementAmplitude,
                    transform.position.y,
                    lockZ ? transform.position.z : originalPosition.z + variation.y * movementAmplitude
                );
                lookingLeft = targetPosition.x < transform.position.x;
            }
        } else {
            // released the mouse
            if (clickOrigin != Vector2.zero) {
                if (canJump) {
                    isJumping = true;

                    GetComponent<Rigidbody>().AddForce(
                        Mathf.Cos(jumpingAngle * Mathf.Deg2Rad) * jumpingForce * (lookingLeft ? -1 : 1),
                        Mathf.Sin(jumpingAngle * Mathf.Deg2Rad) * jumpingForce,
                        0
                        );
                }
            }

            clickOrigin = Vector2.zero;
        }
        // Movement logic.
        // the is control to change player direction (eye) 
        if (isJumping == false) {
            // fix the startup bug with Vector3.Lerp
            Vector3 smoothPosition = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * speed);
            transform.position = new Vector3(smoothPosition.x, transform.position.y, smoothPosition.z);
        }
        // Rotate the player's model.
        targetRotation = Quaternion.Euler(0, (lookingLeft ? 180 : 0), 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10F);

        // Keep the player within depth range.
        if (transform.position.z > depthRange) {
            transform.position = new Vector3(transform.position.x, transform.position.y, depthRange);
        } else if (transform.position.z < -depthRange) {
            transform.position = new Vector3(transform.position.x, transform.position.y, -depthRange);
        }

        // Keep the player within horizontal range.
        if (transform.position.x > horizontalRange) {
            transform.position = new Vector3(horizontalRange, transform.position.y, transform.position.z);
        } else if (transform.position.x < -horizontalRange) {
            transform.position = new Vector3(-horizontalRange, transform.position.y, transform.position.z);
        }
    }
    //kill the player
    public void Kill() {
        //if player invincible stop
        if (invincible) {
            return;
        }

        if (onKill != null) {
            onKill();
        }

        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision) {
        if (collision.transform.tag == "Floor") {
            isJumping = false;
            targetPosition = new Vector3(transform.position.x, targetPosition.y, targetPosition.z);
        }
    }
}
