using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingEnemy : MonoBehaviour {

    public float speed;
    public float waitingDuration = 2f;
    public float[] depthPercentages;

    private float depthRange;
    private float horizontalRange;
    private bool movingLeft;
    private float waitingTimer;
    private bool frozen;

    public float DepthRange { set { depthRange = value; } }
    public float HorizontalRange { set { horizontalRange = value; } }

    // Use this for initialization
    void Start() {
        // Start the rolling enemy in a random place.
        transform.position = new Vector3(
            Random.value > 0.5f ? horizontalRange : -horizontalRange,
            transform.position.y,
            transform.position.z
        );

        //movingLeft = transform.position.x > 0 ? true : false;
        movingLeft = transform.position.x > 0;

        SetValues();
    }

    // Update is called once per frame
    void Update() {
        if (waitingTimer > 0 || frozen) {
            waitingTimer -= Time.deltaTime;
            //Make the enemy wait.
            GetComponent<Rigidbody>().velocity = Vector3.zero;
        } else {
            //Move the enemy to the left or right
            GetComponent<Rigidbody>().velocity = new Vector3(
                movingLeft ? -speed : speed,
                GetComponent<Rigidbody>().velocity.y,
                GetComponent<Rigidbody>().velocity.z
            );
        }

        //Check for direction change.
        //if (movingLeft && transform.position.x < -horizontalRange) {
        //if (movingLeft == true) {
        //    movingLeft = false;
        //} else {
        //    movingLeft = true;
        //}
        //    movingLeft = !movingLeft;
        //} else if (!movingLeft && transform.position.x > horizontalRange) {
        //    movingLeft = !movingLeft;
        //}
        if ((movingLeft && transform.position.x < -horizontalRange) ||
            (!movingLeft && transform.position.x > horizontalRange)) {
            movingLeft = !movingLeft;

            SetValues();
        }
    }

    private void SetValues() {
        //countdown clock
        waitingTimer = waitingDuration;
        //Set the enemy random position
        transform.position = new Vector3(
        transform.position.x,
        transform.position.y,
        ((depthRange + depthRange) * depthPercentages[Random.Range(0, depthPercentages.Length)]) - depthRange
        );
    }

    void OnCollisionEnter(Collision collision) {
        //Collision with the player and kill the player
        if (collision.transform.GetComponent<Player>() != null) {
            frozen = true;
            collision.transform.GetComponent<Player>().Kill();
        }
    }
}
