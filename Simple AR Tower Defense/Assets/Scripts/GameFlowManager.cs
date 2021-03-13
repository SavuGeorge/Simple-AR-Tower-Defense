using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameFlowManager : MonoBehaviour {

    // config vars
    public int startingLives;
    public int startingGold;
    public int towerCost;
    public int wallCost;
    public float spawnSpeed;
    public float spawnSpeedDecrease;
    private float spawnTimer;
    public int startWaveCount;
    public int waveCountIncrease;
    private int currentWaveMax;
    private int currentWaveCounter;

    //state vars
    [HideInInspector]
    public int playerLives;
    [HideInInspector]
    public int playerGold;
    [HideInInspector]
    public List<EnemyAI> enemyList;
    [HideInInspector]
    public List<Tower> towerList;
    private bool wavePhase;


    //ref vars
    public GameObject enemyPrefab;
    public PlacementManager placeManager;
    public FieldManager field;
    public Button buildTowerButton;
    public Button buildWallButton;
    public Button demolishButton;
    public Button startWaveButton;
    public Text goldText;
    public Text livesText;
    public Text towerCostText;
    public Text wallCostText;


    public static GameFlowManager instance;


    public void StartWave() {
        field.ProcessTowerAreaMatrix();

        spawnTimer = spawnSpeed;
        currentWaveCounter = currentWaveMax;
        currentWaveMax += waveCountIncrease;
        wavePhase = true;

        startWaveButton.interactable = false;
        buildTowerButton.interactable = false;
        buildWallButton.interactable = false;
        demolishButton.interactable = false;
        startWaveButton.interactable = false;
        placeManager.ResetButtons();
    }

    private void EndWave() {
        wavePhase = false;

        startWaveButton.interactable = true;
        buildTowerButton.interactable = true;
        buildWallButton.interactable = true;
        demolishButton.interactable = true;
        startWaveButton.interactable = true;

        placeManager.placeState = PlacementManager.PlaceStateType.None;

        spawnSpeed -= spawnSpeedDecrease;
        if (spawnSpeed <= 0.01f) {
            spawnSpeed = 0.01f;
        }


        RefreshGoldPermissionsAndUI();
    }

    private void Update(){
        if (wavePhase) {
            if (currentWaveCounter > 0) {
                if (spawnTimer <= 0) {
                    SpawnGuy();
                    currentWaveCounter--;
                    spawnTimer += spawnSpeed;
                }
                else {
                    spawnTimer -= Time.deltaTime;
                }
            }
            else {
                if(enemyList.Count == 0) {
                    EndWave();
                }
            }
        }
    }

    

    private void SpawnGuy() {
        Instantiate(enemyPrefab, field.worldPortalPos, Quaternion.identity);
    }


    

    void Start() {
        instance = this;
        InitGame();
    }

    private void GameOver() {
        print("GameOver");
    }

    private void InitGame() {
        playerLives = startingLives;
        playerGold = startingGold;
        RefreshGoldPermissionsAndUI();
        RefreshLivesUI();
        towerCostText.text = towerCost.ToString();
        wallCostText.text = wallCost.ToString();

        wavePhase = false;
        currentWaveMax = startWaveCount;

    }

    public void ChangeGold(int amount) {
        playerGold += amount;

        RefreshGoldPermissionsAndUI();
    }

    public void ChangeLives(int amount) {
        playerLives += amount;
        RefreshLivesUI();
        if (playerLives <= 0) {
            GameOver();
        }
    }

    private void RefreshGoldPermissionsAndUI() {
        if (playerGold < towerCost) {
            buildTowerButton.interactable = false;
            if(placeManager.placeState == PlacementManager.PlaceStateType.Tower) {
                placeManager.placeState = PlacementManager.PlaceStateType.None;
            }
        }
        else {
            buildTowerButton.interactable = true;
        }

        if (playerGold < wallCost) {
            buildWallButton.interactable = false;
            if (placeManager.placeState == PlacementManager.PlaceStateType.Wall) {
                placeManager.placeState = PlacementManager.PlaceStateType.None;
            }
        }
        else {
            buildWallButton.interactable = true;
        }
        goldText.text = playerGold.ToString();
        livesText.text = playerLives.ToString();
    }

    private void RefreshLivesUI() {
        livesText.text = playerLives.ToString();
    }




}
