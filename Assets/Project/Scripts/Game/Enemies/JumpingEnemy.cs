using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpingEnemy : MonoBehaviour {

    public float jumpingForce = 5f;
    public float speed = 3f;

    private float horizontalRange;
    private bool movingDown;
    private float targetHorizontalPosition;

    public float HorizontalRange { set { horizontalRange = value; } }

    // Use this for initialization
    void Start() {
        movingDown = true;
        targetHorizontalPosition = Random.Range(-horizontalRange, horizontalRange);
    }

    // Update is called once per frame
    void Update() {
        // Get the exact moment where the jumping enemy moves down.
        if (movingDown == false) {
            if (GetComponent<Rigidbody>().velocity.y < 0f) {
                movingDown = true;

                targetHorizontalPosition = Random.Range(-horizontalRange, horizontalRange);

            }
        }
        // Moves the jumping enemy horizontally
        Vector3 targetPosition = new Vector3(targetHorizontalPosition, transform.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * speed);
    }

    void OnCollisionEnter(Collision collision) {
        movingDown = false;
        GetComponent<Rigidbody>().AddForce(0, jumpingForce, 0);
        //Collision with the player and kill the player
        if (collision.transform.GetComponent<Player>() != null) {
            collision.transform.GetComponent<Player>().Kill();
        }
    }
}
