using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MissileManager : MonoBehaviour
{
    /// <summary>
    /// 
    /// �̻��� �̸�      Ư¡         �ε���
    /// AIM120       ���ݷ��� ����      0
    /// AIM9         ���� �ӵ��� ����   1
    /// 
    /// </summary>

    [SerializeField] public GameObject[] QTETimeLine;
    [SerializeField] public QTEManager QTE;

    [Header("�̻��� �����հ� ����")]    
    public GameObject[] missileList;  //�̻��� ����Ʈ 
    public WeaponData[] missileInfo;  //�̻��� ������ ��� ScriptableObject

    List<int> equippedMissile;                //���������� �̻��� ����Ʈ
    [HideInInspector] public int eqMissileIndex = 0;  //���� ���� ���� ���� �ε���

    public bool fireEnable;
    
    GameObject missileObjL;    //���� �߻��� �� �ִ� ���� �̻���
    GameObject missileObjR;    //���� �߻��� �� �ִ� ������ �̻���

    [Header("�̻��� �����/��ź��")]
    [HideInInspector] public int[] missileCount;        //���� ���� �̻��� ����
    public int[] damageLevel;
    public int[] maxCountLevel;

    [Header("�̻��� �߻� ��ġ")]
    public Transform missileFirePosL;     //���� �̻��� �߻� ��ġ
    public Transform missileFirePosR;     //������ �̻��� �߻� ��ġ

    [Header("�̻��� ������")]
    [SerializeField] bool[] onDelayL;     //���� �̻��� fire��������
    [SerializeField] bool[] onDelayR;     //index 0~1 ������ �̻��� fire ��������

    [Header("�̻��� ��Ÿ��")]
    float[] missileCoolTime;              //�̻��� ��Ÿ�� �ð� ����
    public float[] missileCoolTimerL;     //���� �̻��� �߻� ���ɱ��� ���� �ð�
    public float[] missileCoolTimerR;     //������ �̻��� �߻� ���ɱ��� ���� �ð�
    //Coroutine timeCoroutine;            //�ڷ�ƾ ���� ����

    [Header("LockOn")]
    [SerializeField]
    public Transform targeting;
    TargetObject targets;//Ÿ���� �ҷ����� ����- 
    public List<GameObject> enemies = new List<GameObject>();
    Missile missileObj;

    private void Awake()
    {
        equippedMissile = new List<int>();

        missileCoolTime = new float[missileList.Length];
        missileCoolTime[0] = missileInfo[0].attackDelay;
        missileCoolTime[1] = missileInfo[1].attackDelay;
    }

    private void Start()
    {
        fireEnable = false;

        GameObject[] enemyArray = GameObject.FindGameObjectsWithTag("Enemy");
        enemies.AddRange(enemyArray);

        Invoke("FireEnableTrue", 5f);

        //��ź�� �Ҵ�
        int index;
        for(index = 0; index < missileList.Length; index++)
        {
            missileCount[index] = missileInfo[index].maxCounts[0];
        }

        //���� ���� �̻��� �ε��� �ʱ�ȭ
        eqMissileIndex = 0;

        AddMissile(0);
        AddMissile(1);
    }
    
    private void Update()
    {
        GameObject[] enemyArray = GameObject.FindGameObjectsWithTag("Enemy");
        enemies.AddRange(enemyArray);
        FindCloseEnemy();
        DelayMissileCheck();

        if (missileObjL != null)
            missileObjL.GetComponent<Missile>().target = targeting;
        if (missileObjR != null)
            missileObjR.GetComponent<Missile>().target = targeting;

    }

    void FireEnableTrue()
    {
        fireEnable = true;
    }

    //�̻��� �߻� ���� üũ�ϴ� �Լ�
    public void OnFire(InputValue value)
    {
        if (!GameManager.instance.isStick && value.isPressed && missileCount[eqMissileIndex] > 0 && fireEnable)
        {
            FireMissile(); //�̻��� ��� �Լ�   
        }
    }

    //�̻��� �߻� ���� üũ�ϴ� �Լ�(��ƽ)
    void OnFireMissile_Stick(InputValue value)
    {
        if (GameManager.instance.isStick && value.isPressed && missileCount[eqMissileIndex] > 0 && fireEnable)
        {
            FireMissile();
        }
    }

    //���� ��ü ��ư �Է¹޴� �Լ�(�е�)
    void OnWeaponChange(InputValue value)
    {
        if(!GameManager.instance.isStick && value.isPressed)
        {
            WeaponChange();
        }
    }

    //���� ��ü ��ư �Է¹޴� �Լ�(��ƽ)
    void OnWeaponChange_Stick(InputValue value)
    {
        if(GameManager.instance.isStick && value.isPressed)
        {
            WeaponChange();
        }
    }

    //���⸦ ��ü�ϴ� �Լ�
    void WeaponChange()
    {
        if (eqMissileIndex < equippedMissile.Count - 1 && equippedMissile.Contains(eqMissileIndex + 1))
        {
            eqMissileIndex++;  //���� ���� ���� �ε��� 1 ����( ���� ����)
        }
        else if (eqMissileIndex == equippedMissile.Count - 1 && equippedMissile.Contains(0))
        {
            eqMissileIndex = equippedMissile[0];
        }

        Debug.Log("���� ���� ���� ���� �ε��� : " + eqMissileIndex);
    }

    //�̻��� �߻��ϴ� �Լ�
    void FireMissile()
    {
        if (missileCount[eqMissileIndex] > 0)   //�̻����� ������ ������
        {
            if (!onDelayL[eqMissileIndex] && missileCount[eqMissileIndex] % 2 == 0)   //���Ⱑ �߻簡���� �����̰�, �̻��� ������ ¦����� (���� �̻����� �߻�ȴ�)   
            {
                for (int index = 0; index < PoolManager.instance.prefabs.Length; index++)
                    if (PoolManager.instance.prefabs[index] == missileList[eqMissileIndex])
                    {
                        missileObjL = PoolManager.instance.Get(index); 
                        missileObjL.transform.parent = PoolManager.instance.transform.GetChild(index);   
                        missileObjL.transform.position = missileFirePosL.transform.position;    //������Ʈ �߻���ġ �ʱ�ȭ   
                        missileObjL.transform.rotation = transform.rotation;

                        if (targeting = null)
                        {
                            missileObjL.GetComponent<Missile>().target = null;
                        }

                        if (missileObjL.GetComponent<Missile>().angle < missileObjL.GetComponent<Missile>().boresightAngle)
                        {
                            missileObjL.GetComponent<Missile>().target = targeting;
                        }

                        missileCoolTimerL[eqMissileIndex] = 0f;       //�߻縦 �ϹǷ� ��Ÿ�ӿ� 0�� �����Ѵ�.   
                        onDelayL[eqMissileIndex] = true;              //������ ���°� �ȴ�.

                        missileCount[eqMissileIndex]--;               //�̻����� �߻������Ƿ� ź���� 1�� �پ���.   
                        RumbleManager.instance.RumblePulse(0.25f, 1f, 0.25f);   //����   
                        break;
                    }
            }


            else if (!onDelayR[eqMissileIndex] && missileCount[eqMissileIndex] % 2 == 1)   
            {
                for (int index = 0; index < PoolManager.instance.prefabs.Length; index++)
                    if (PoolManager.instance.prefabs[index] == missileList[eqMissileIndex])
                    {
                        missileObjR = PoolManager.instance.Get(index); 
                        missileObjR.transform.parent = PoolManager.instance.transform.GetChild(index);   
                        missileObjR.transform.position = missileFirePosR.transform.position;    //������Ʈ �߻���ġ �ʱ�ȭ   
                        missileObjR.transform.rotation = transform.rotation;

                        if (targeting = null)
                        {
                            missileObjR.GetComponent<Missile>().target = null;
                        }
                        if (missileObjR.GetComponent<Missile>().angle < missileObjR.GetComponent<Missile>().boresightAngle)
                        {
                            missileObjR.GetComponent<Missile>().target = targeting;
                        }

                        missileCoolTimerR[eqMissileIndex] = 0f;           //�߻縦 �ϹǷ� ��Ÿ���� 0���� �ʱ�ȭ�Ѵ�.   
                        onDelayR[eqMissileIndex] = true;

                        missileCount[eqMissileIndex]--;     
                        RumbleManager.instance.RumblePulse(0.25f, 1f, 0.25f);   //����   
                        break;
                    }
            }

        }
    }

    void DelayMissileCheck()
    {
        if (onDelayL[0])
        {
            FireCoolL(0);
        }

        if (onDelayL[1])
        {
            FireCoolL(1);
        }

        if (onDelayR[0])
        {
            FireCoolR(0);
        }

        if (onDelayR[1])
        {
            FireCoolR(1);
        }
    }

    //���� �̻��� ������ 
    void FireCoolL(int index)
    {
        if (missileCoolTimerL[index] < missileCoolTime[index])  //duration���� Ÿ�̸Ӱ� ������ Time.deltaTime�� ���Ѵ�.    
        {
            missileCoolTimerL[index] += Time.deltaTime;    //fire�ϰ� ��Ÿ���� ���ư��Ƿ� ���� �̻��� �������� �߻簡 �Ұ����ϴ�  

            if (missileCoolTimerL[index] >= missileCoolTime[index])     //��Ÿ���� ������  
            {
                onDelayL[index] = false;     //����� �߻簡���� ���°� �ȴ� 
            }
        }
    }

    //������ �̻��� ������
    void FireCoolR(int index)
    {
        if (missileCoolTimerR[index] < missileCoolTime[index])  //��Ÿ���� �� á����    
        {
            missileCoolTimerR[index] += Time.deltaTime;   

            if (missileCoolTimerR[index] >= missileCoolTime[index])   
            {
                onDelayR[index] = false;   
            }
        }
    }

    public void SortMissile()  //����(�̻���) ����  , ���� UI ǥ�� ��ũ��Ʈ���� ������
    {
        equippedMissile.Sort();   
    }

    //�߻� ������ �̻��� �߰�
    public void AddMissile(int index) //��밡���� ���� (�ε�����) �߰� ,GM���� ������
    {
        if (!equippedMissile.Contains(index))
        {
            // Debug.Log(index + "���� �߰�");
            equippedMissile.Add(index);   

            eqMissileIndex = index;
        }
    }

    public void FindCloseEnemy()
    {
        float closeDistance = Mathf.Infinity;
        Transform closeTarget = null;

        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < closeDistance)
                {
                    closeDistance = distance;
                    closeTarget = enemy.transform;
                }
            }
        }
        targeting = closeTarget;
    }
}
