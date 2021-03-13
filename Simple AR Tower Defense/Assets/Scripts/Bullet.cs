using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    public float speed;
    public int damage;


    private EnemyAI target;


    void Update() {
        if (target != null) {
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target.transform.position) < 0.01f) {
                HitEnemy();
            }
        }
        else {
            Destroy(gameObject,0.01f);
        }
    }

    void HitEnemy() {
        target.TakeDamage(damage);
        Destroy(this.gameObject, 0.01f);
    }

    public void Init(EnemyAI t) {
        target = t;
    }

}
