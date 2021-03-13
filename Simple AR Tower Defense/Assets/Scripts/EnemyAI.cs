using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{

    //settings
    public float speed;
    public int startHP;
    private int currentHP;
    public float spawnerSpeed;
    private float spawnTimer;
    public int maxExtraSpawns;
    private int extraSpawnsCounter;
    [HideInInspector]
    public bool secondGeneration = false;

    public int kamikazeBounty = 20;
    public int chargerBounty = 10;
    public int spawnerBounty = 20;
    public int destroyerBounty = 30;
    public int cowardBounty = 50;

    //refs
    private Material mat;
    private FieldManager field;
    private List<Vector2Int> gridPath;
    private List<Vector3> worldPath;
    private int pathIndex;
    private Vector2Int gridPos;
    private Vector2Int gridGoal;
    private GameObject destroyerTarget;

    enum StateType { none, kamikaze, destroyer, spawner, charger, coward }
    StateType state;


    GameFlowManager flowManager;

    void Start() {
        flowManager = GameFlowManager.instance;
        flowManager.enemyList.Add(this);
        field = flowManager.field;
        mat = gameObject.GetComponent<MeshRenderer>().material;

        //SetState(StateType.charger);
        RandomizeState();

        currentHP = startHP;
    }

    void Update() { 

        if (state == StateType.kamikaze || state == StateType.charger) { // Pathfinds to player base, explodes on contact
            Move();
        }
        else if (state == StateType.destroyer) { // Targets a building, tries to destroy it
            Move();
        }
        else if (state == StateType.spawner) { // runs off somewhere and spawns a few more enemies
            Move();
            if(spawnTimer <= 0) {
                spawnTimer += spawnerSpeed;
                SpawnSecondGenerationGuy();
                extraSpawnsCounter++;
                if(extraSpawnsCounter == maxExtraSpawns) {
                    RandomizeState();
                }
            }
            else {
                spawnTimer -= Time.deltaTime;
            }
        }
        else if(state == StateType.coward){
            Move();
        }

    }

    private void SpawnSecondGenerationGuy() {
        GameObject newGuy = Instantiate(flowManager.enemyPrefab, this.transform.position, Quaternion.identity);
        newGuy.GetComponent<EnemyAI>().secondGeneration = true;
    }

    void Move() {

        if (worldPath.Count != 0) {
            if (pathIndex >= worldPath.Count - 1) {
                PathEnd();
            }

            transform.position = Vector3.MoveTowards(transform.position, worldPath[pathIndex + 1], Time.deltaTime * speed);
            gridPos = field.WorldToGrid(transform.position);
            if (gridPos == gridPath[pathIndex + 1]) {
                pathIndex++;
            }

            if (pathIndex >= worldPath.Count - 1) {
                PathEnd();       
            }
        }
        else {
            SetState(StateType.destroyer);
        }
    }


    void PathEnd() {
        if (state == StateType.kamikaze || state == StateType.charger) {
            KamikazeBoom();
            state = StateType.none;
        }
        else if (state == StateType.destroyer) {
            DestroyerBoom();
        }
        else if (state == StateType.spawner) {
            RecalculateGoals();
            RecalculatePath();
        }
        else if (state == StateType.coward) {
            currentHP = startHP;
            RandomizeState();
        }
    }

    void KamikazeBoom() {
        flowManager.ChangeLives(-1);
        Die();
    }

    void DestroyerBoom() {
        if (destroyerTarget) {
            Tower tscript = destroyerTarget.GetComponent<Tower>();
            if (tscript) {
                tscript.Demolish();
            }
            else {
                field.refMatrix[gridGoal.x][gridGoal.y] = null;
                field.aiMatrix[gridGoal.y][gridGoal.x] = 0;
                Destroy(destroyerTarget, 0.01f);
            }
            Die();
        }
        else {
            SetState(StateType.charger);
        }
    }

    void RandomizeState() {
        float randomvar = Random.Range(0.0f, 1.0f);

        if (randomvar <= 0.5f) {
            SetState(StateType.charger);
        }
        else if(randomvar <= 0.75f) {
            SetState(StateType.kamikaze);
        }
        else if(randomvar <= 0.9f) {
            SetState(StateType.destroyer);
        }
        else {
            SetState(StateType.spawner);
        }

    }

    void RecalculateGoals() {
        if (state == StateType.kamikaze || state == StateType.charger) {
            SetGoal(field.basePos);
        }
        else if (state == StateType.destroyer) {
            GameObject target = null;
            float targetDistance = field.tilesX * field.tileSize + field.tilesY * field.tileSize;
            for(int i = 1; i < field.tilesX-1; i++) {
                for (int j = 1; j < field.tilesY-1; j++) {
                    if(field.refMatrix[i][j] != null) {
                        float dist = Vector3.Distance(transform.position, field.refMatrix[i][j].transform.position);
                        if (dist < targetDistance) {
                            target = field.refMatrix[i][j];
                            targetDistance = dist;
                        }
                    }
                }
            }
            if(target == null) {
                RandomizeState();   
            }
            else {
                SetGoal(field.WorldToGrid(target.transform.position));
                destroyerTarget = target;
            }
        }
        else if (state == StateType.spawner) {
            SetGoal(new Vector2Int(Random.Range(0, field.tilesX), Random.Range(0,field.tilesY)));
        }
        else if(state == StateType.coward) {
            SetGoal(field.portalPos);
        }
    }


    void RecalculatePath() {
        gridPos = field.WorldToGrid(transform.position);
        string type; 
        if (state == StateType.charger || state == StateType.coward) {
            type = "Euclidean"; // takes the shortest path
        }
        else {
            type = "Sneaky"; // dodges tower ranges
        }
        SetPaths(gridPos, gridGoal, type);
        //pathIndex = SetPathIndex();
        pathIndex = 0;
    }


    int SetPathIndex() {
        int i = 0;
        for(i = 0; i < gridPath.Count; i++) {
            if((gridPath[i].x == gridPos.x) && (gridPath[i].y == gridPos.y)) {
                break;
            }
        }
        return i;
    }


    void SetGoal(Vector2Int g) {
        gridGoal = g;
    }

    void SetPaths(Vector2Int current, Vector2Int goal, string type) {
        gridPath = field.GetPath(current, goal, type);
        worldPath = new List<Vector3>();
        foreach (Vector2Int p in gridPath) {
            worldPath.Add(field.GridToWorld(p));
        }
    }


    public void TakeDamage(int dmg) {
        currentHP -= dmg;
        if(currentHP <= 0) {
            Die();
        }
        else {
            float r = Random.Range(0.0f, 1.0f);
            switch (state) {
                case StateType.kamikaze:
                    if(r <= 0.15f) {
                        SetState(StateType.coward);
                    }
                    break;
                case StateType.charger:
                    if (r <= 0.1f) {
                        SetState(StateType.coward);
                    }
                    break;
                case StateType.destroyer:
                    if (r <= 0.3f) {
                        SetState(StateType.coward);
                    }
                    break;
                case StateType.spawner:
                    if (r <= 0.5f) {
                        SetState(StateType.coward);
                    }
                    break;
            }
        }
    }

    void Die() {
        flowManager.enemyList.Remove(this);
        if(state == StateType.kamikaze) {
            flowManager.ChangeGold(kamikazeBounty);
        }
        else if(state == StateType.charger) {
            flowManager.ChangeGold(chargerBounty);
        }
        else if(state == StateType.spawner) {
            flowManager.ChangeGold(spawnerBounty);
        }
        else if(state == StateType.destroyer) {
            flowManager.ChangeGold(destroyerBounty);
        }
        else if(state == StateType.coward) {
            flowManager.ChangeGold(cowardBounty);
        }

        Destroy(gameObject, 0.01f);
    }

    
    void SetState(StateType newState) {

        if (newState == StateType.spawner) {
            if (secondGeneration == true || extraSpawnsCounter >= maxExtraSpawns) {
                SetState(StateType.kamikaze);
                return;
            }
        }

        state = newState;

        switch (state) {
            case StateType.kamikaze: // kamikazes sneak around tower ranges, picking the safest path regadless of length
                mat.color = Color.black;
                break;
            case StateType.destroyer: // destroyers pick the closest structure and try to demolish it
                mat.color = Color.yellow; 
                break;
            case StateType.spawner: // Spawners run around randomly and make more guys
                mat.color = Color.blue;
                spawnTimer = spawnerSpeed;
                break;
            case StateType.charger: // chargers have dumb pathfinding, but get boosts to their stats
                mat.color = Color.red;
                currentHP += startHP * 3;
                speed *= 2;
                break;
            case StateType.coward: // cowards try to run back to the portal, but may return to normal while running. They also gain extra HP.
                mat.color = Color.green;
                currentHP += startHP * 2;
                break;
        }

        RecalculateGoals();
        RecalculatePath();


    }


}
