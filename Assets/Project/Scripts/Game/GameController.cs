using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

    [Header("Gameplay")]
    public Player player;
    public GameMode gameMode;
    public float duration = 15f;
    public float depthRange = 4f;
    public float horizontalRange;

    [Header("Endless gameplay")]
    public bool isEndless;
    public float timeToIncreaseDifficulty = 10f;

    [Header("Visuals")]
    public Camera gameCamera;
    //import text field
    public Text infoText;
    public GameObject upperWall;
    public GameObject lowerWall;

    [Header("Game: jumpers")]
    //Create the jumpingEnemyPrefab reference
    public GameObject jumpingEnemyPrefab;

    [Header("Game: rollers")]
    //Create the rollingEnemyPrefab reference
    public GameObject rollingEnemyPrefab;

    [Header("Game: bouncers")]
    public GameObject bouncingEnemyPrefab;
    public int bouncingEnemiesAmount = 1;

    [Header("Game: crawlers")]
    public GameObject crawlingEnemyPrefab;
    public int crawlerMaximumDifficulty = 5;
    public float crawlerSpawnIntervalMaximum;
    public float crawlerSpawnIntervalMinimum;

    private float timer;
    private bool gameOver;
    private bool win;
    private int difficulty = 1;
    //Return to menu in 3 sec
    private float resetTimer = 3f;

    private float crawlerSpawnTimer;

    public float Highscore {
        get {
            string key = "Highscore" + gameMode.ToString();
            return PlayerPrefs.GetFloat(key);
        }
        set {
            string key = "Highscore" + gameMode.ToString();
            PlayerPrefs.SetFloat(key, value);
        }
    }

    public enum GameMode {
        None,
        Jumpers,
        Rollers,
        Bouncers,
        Crawlers
    }

    // Use this for initialization
    void Awake() {
        // Load data.
        if (LevelManager.Instance.GameMode != GameMode.None) {
            gameMode = LevelManager.Instance.GameMode;
        }

        isEndless = LevelManager.Instance.IsEndless;

        //Debug.Log(gameCamera.aspect);
        // Set the horizontal range.
        float baseAspect = 9f / 16f;
        float aspectVariation = gameCamera.aspect / baseAspect;
        horizontalRange = (aspectVariation * gameCamera.orthographicSize) / 2f;

        // Set variables
        player.DepthRange = depthRange;
        player.HorizontalRange = horizontalRange;
        player.onKill = OnPlayerKilled;

        // Set the bounds' positions.
        upperWall.transform.position = new Vector3(upperWall.transform.position.x, upperWall.transform.position.y, depthRange + 1.25f);
        lowerWall.transform.position = new Vector3(lowerWall.transform.position.x, lowerWall.transform.position.y, -depthRange - 1.25f);

        // Set the game timer.
        timer = isEndless ? 0 : duration;

        //Get level parameters from the level manager.
        LevelParameter[] parameters = LevelManager.Instance.Parameters;

        // Set game mode.
        switch (gameMode) {
            case GameMode.Jumpers:
                player.LockZ = true;

                SpawnEnemy();
                break;
            case GameMode.Rollers:
                player.LockZ = false;

                SpawnEnemy();
                break;
            case GameMode.Bouncers:

                if (GetParameterValue(parameters, "enemiesAmount") != 0) {
                    bouncingEnemiesAmount = (int)GetParameterValue(parameters, "enemiesAmount");
                }

                player.LockZ = true;
                if (isEndless == false) {
                    for (int i = 0; i < bouncingEnemiesAmount; i++) {
                        SpawnEnemy();
                        difficulty++;
                    }
                } else {
                    SpawnEnemy();
                }
                break;
            case GameMode.Crawlers:
                player.LockZ = true;

                player.CanJump = true;
                crawlerSpawnTimer = crawlerSpawnIntervalMaximum;
                break;
            default:
                Debug.LogWarning("This game mode wasn't implemented!");
                break;
        }
    }

    void SpawnEnemy() {
        switch (gameMode) {
            case GameMode.Jumpers:
                GameObject jumpingEnemyObject = Instantiate(jumpingEnemyPrefab);
                jumpingEnemyObject.transform.SetParent(transform);
                jumpingEnemyObject.GetComponent<JumpingEnemy>().HorizontalRange = horizontalRange;
                break;
            case GameMode.Rollers:
                GameObject rollingEnemyObject = Instantiate(rollingEnemyPrefab);
                rollingEnemyObject.transform.SetParent(transform);
                rollingEnemyObject.GetComponent<RollingEnemy>().DepthRange = depthRange;
                rollingEnemyObject.GetComponent<RollingEnemy>().HorizontalRange = horizontalRange;
                break;
            case GameMode.Bouncers:
                GameObject bouncingEnemyObject = Instantiate(bouncingEnemyPrefab);
                bouncingEnemyObject.transform.SetParent(transform);
                bouncingEnemyObject.transform.position = new Vector3(
                    (difficulty % 2 == 0) ? horizontalRange : -horizontalRange,
                    bouncingEnemyObject.transform.position.y,
                    bouncingEnemyObject.transform.position.z
                );
                bouncingEnemyObject.GetComponent<BouncingEnemy>().DepthRange = depthRange;
                bouncingEnemyObject.GetComponent<BouncingEnemy>().HorizontalRange = horizontalRange;
                break;
            default:
                Debug.LogWarning("This game mode has a different spawn mode!");
                break;
        }
    }

    // Update is called once per frame
    void Update() {
        //Check if the player is dead
        if (player == null) {
            gameOver = true;
            win = false;
        }
        // Check if the game is finished in the regular mode.
        if (isEndless == false) {
            timer -= Time.deltaTime;
            if (timer > 0f) {
                infoText.text = "Time: " + Mathf.Floor(timer);
                //check if the payer has won
            } else {
                //Tells that player is alive
                if (player != null) {
                    gameOver = true;
                    win = true;
                    //Set the player invincible
                    player.Invincible = true;
                }
            }

            // Send the game over message.
            if (gameOver == true) {
                if (win) {
                    infoText.text = "You win!";
                } else {
                    infoText.text = "You lose!";
                }
            }
        } else {
            if (player != null) {
                //Make timer count up.
                timer += Time.deltaTime;

                // Update the label
                infoText.text = "Time: " + Mathf.Floor(timer);

                // Increase difficulty
                if (timer > timeToIncreaseDifficulty * difficulty) {
                    difficulty++;
                    SpawnEnemy();
                }
            } else {

                // Send the game over message.
                infoText.text = "Game Over!\n";
                infoText.text += "Your time: " + Mathf.Floor(timer) + "\n";
                infoText.text += "Highscore: " + Mathf.Floor(Highscore) + "\n";
            }
        }

        // Crawler game logic.
        if (gameMode == GameMode.Crawlers) {
            // Crawler game logic.

            crawlerSpawnTimer -= Time.deltaTime;
            if (crawlerSpawnTimer <= 0f) {
                crawlerSpawnTimer = crawlerSpawnIntervalMaximum;

                GameObject crawlingEnemyObject = Instantiate(crawlingEnemyPrefab);
                crawlingEnemyObject.transform.SetParent(transform);
                crawlingEnemyObject.transform.position = new Vector3(
                    (Random.value > 0.5f) ? (horizontalRange + 0.8f) : (-horizontalRange - 0.8f),
                    crawlingEnemyObject.transform.position.y,
                    crawlingEnemyObject.transform.position.z
                    );

            }
        }
        //the game is over return to Menu
        if (gameOver == true) {
            resetTimer -= Time.deltaTime;
            if (resetTimer <= 0) {
                SceneManager.LoadScene("Menu");
            }
        }

    }

    void OnPlayerKilled() {
        if (timer > Highscore) {
            Highscore = timer;
        }
    }

    private float GetParameterValue(LevelParameter[] parameters, string key) {
        foreach (LevelParameter parameter in parameters) {
            if (parameter.key == key) {
                return parameter.value;
            }
        }

        return 0;
    }
}
