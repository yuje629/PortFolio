using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMissileManager : MonoBehaviour
{
    [Header("미사일 정보")]
    [SerializeField] GameObject eMissilePrefab;  
    public Transform[] firePos;    //enemy 미사일 발사 위치\

    [Header("미사일 락온")]
    [SerializeField] float fireAngle;
    [SerializeField] float fireDistance;
    PlayerCtrl player;          //플레이어 위치 받아오기 위해
    Transform targetTransform;  //플레이어 위치 저장 변수

    [Header("미사일 딜레이")]
    [SerializeField] float eFireDelayTime;
    [SerializeField] float eFireDelayTimer = 0;
    bool onMissileDelay = false;
    Coroutine eMissileDelayCoroutine;

    void Start()
    {
        player = GameManager.PlayerCtrl;
    }

    void Update()
    {
        CheckTargetLock();
    }

    float GetAngleBetweenTransform(Transform otherTransform)  //타겟위치와 enemy위치의 앵글 구하기 (0~180도 사이의 각도를 반환)
    {
        Vector3 direction = transform.forward;
        Vector3 diff = otherTransform.position - transform.position;
        return Vector3.Angle(diff, direction);
    }

    void CheckTargetLock()   //락온 체크, 플레이어가 enemy missile 락온 distance 안, 락온 angle 안에 들어오면 락온이 된다.
    {
        if(player != null)
            targetTransform = player.transform;  //플레이어의 실시간 위치를 받아옴.

        if (targetTransform == null)  //플레이어가 없으면 함수를 빠져나감
        {
            return;
        }

        float targetDistance = Vector3.Distance(targetTransform.position, transform.position);  //플레이어와 enemy 사이의 거리
        float targetAngle = GetAngleBetweenTransform(targetTransform);   //플레이어와 enemy 사이의 앵글 값 받아오기

        if (targetDistance > fireDistance || targetAngle > fireAngle || onMissileDelay)  //플레이어가 lockon 거리 밖에 있거나 lockon앵글 밖에 있으면 함수를 빠져나감
        {
            return;
        }
        else
        {
            LaunchEnemyMissile();
        }
    }

    public void LaunchEnemyMissile()
    {
        StartCoroutine(EnemyMissileDelay());

        for(int index = 0; index < PoolManager.instance.prefabs.Length; index++)
        {
            if(PoolManager.instance.prefabs[index] == eMissilePrefab)
            {
                GameObject enemyMissile = PoolManager.instance.Get(index);
                enemyMissile.transform.parent = PoolManager.instance.transform.GetChild(index);
                enemyMissile.transform.position = firePos[Random.Range(0, 2)].transform.position;
                enemyMissile.transform.rotation = transform.rotation;
                enemyMissile.transform.GetComponent<EnemyMissile>().EnemyMissileSetSpeed(transform.parent.GetComponent<Enemy>().speed);
                break;
            }
        }
    }

    IEnumerator EnemyMissileDelay()
    {
        while (eFireDelayTimer < eFireDelayTime) 
        {
            onMissileDelay = true;
            eFireDelayTimer += Time.deltaTime;                                     
            yield return null;
        }

        if (eFireDelayTimer >= eFireDelayTime) 
        {
            eFireDelayTimer = 0;
            onMissileDelay = false;
        }
    }
}
