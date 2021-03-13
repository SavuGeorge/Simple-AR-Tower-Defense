using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    //config vars
    public GameObject bulletPrefab;
    public float range;
    public float shootSpeed;
    private float shootTimer;


    private GameFlowManager flowManager;
    private FieldManager field;

    private Vector2 pos;
    private EnemyAI target;
    private float targetDistanceToBase;


    void Start() {
        flowManager = GameFlowManager.instance;
        field = flowManager.field;
        pos = new Vector2(transform.position.x, transform.position.z);
        shootTimer = shootSpeed;
    }

    void Update() {
        // shoots at the enemy closest to the base that is within range;
        if (shootTimer <= 0) {
            target = null;
            foreach (EnemyAI enemy in flowManager.enemyList) {
                Vector2 epos = new Vector2(enemy.transform.position.x, enemy.transform.position.z);
                if (Vector2.Distance(pos, epos) < range) {
                    if (target == null) {
                        target = enemy;
                        targetDistanceToBase = Vector2.Distance(field.worldBasePos, epos);
                    }
                    else {
                        if (Vector2.Distance(field.worldBasePos, epos) < targetDistanceToBase) {
                            target = enemy;
                            targetDistanceToBase = Vector2.Distance(field.worldBasePos, epos);
                        }
                    }
                }
            }

            if (target) {
                Shoot(target);
                shootTimer = shootSpeed;
            }
        }
        else {
            shootTimer -= Time.deltaTime;
        }

    }


    void Shoot(EnemyAI shootingTarget) {
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        bullet.GetComponent<Bullet>().Init(shootingTarget);
    }


    public void Demolish() {
        flowManager.towerList.Remove(this);
        Vector2Int gridPos = field.WorldToGrid(this.transform.position);
        field.refMatrix[gridPos.x][gridPos.y] = null;
        field.aiMatrix[gridPos.y][gridPos.x] = 0;
        Destroy(gameObject, 0.01f);
    }


}
