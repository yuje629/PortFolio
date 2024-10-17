using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMissileManager : MonoBehaviour
{
    [Header("�̻��� ����")]
    [SerializeField] GameObject eMissilePrefab;  
    public Transform[] firePos;    //enemy �̻��� �߻� ��ġ\

    [Header("�̻��� ����")]
    [SerializeField] float fireAngle;
    [SerializeField] float fireDistance;
    PlayerCtrl player;          //�÷��̾� ��ġ �޾ƿ��� ����
    Transform targetTransform;  //�÷��̾� ��ġ ���� ����

    [Header("�̻��� ������")]
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

    float GetAngleBetweenTransform(Transform otherTransform)  //Ÿ����ġ�� enemy��ġ�� �ޱ� ���ϱ� (0~180�� ������ ������ ��ȯ)
    {
        Vector3 direction = transform.forward;
        Vector3 diff = otherTransform.position - transform.position;
        return Vector3.Angle(diff, direction);
    }

    void CheckTargetLock()   //���� üũ, �÷��̾ enemy missile ���� distance ��, ���� angle �ȿ� ������ ������ �ȴ�.
    {
        if(player != null)
            targetTransform = player.transform;  //�÷��̾��� �ǽð� ��ġ�� �޾ƿ�.

        if (targetTransform == null)  //�÷��̾ ������ �Լ��� ��������
        {
            return;
        }

        float targetDistance = Vector3.Distance(targetTransform.position, transform.position);  //�÷��̾�� enemy ������ �Ÿ�
        float targetAngle = GetAngleBetweenTransform(targetTransform);   //�÷��̾�� enemy ������ �ޱ� �� �޾ƿ���

        if (targetDistance > fireDistance || targetAngle > fireAngle || onMissileDelay)  //�÷��̾ lockon �Ÿ� �ۿ� �ְų� lockon�ޱ� �ۿ� ������ �Լ��� ��������
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
