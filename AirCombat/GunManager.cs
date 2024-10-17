using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    [SerializeField] public GameObject[] QTETimeLine;
    [SerializeField] public QTEManager QTE;

    [Header("�Ѿ� �����հ� ����")]
    public GameObject[] bulletList; //�Ѿ� ������
    public WeaponData[] bulletInfo;
    public Transform bulletParent;

    GameObject equippedBullet;
    int eqGunIndex;                  //���� ���� ���� �Ѿ� 

    [Header("Ÿ�� ����ũ")]
    [SerializeField] LayerMask enemyLayer;

    [Header("�Ѿ� ����Ʈ")]
    public ParticleSystem muzzleParticles;

    [Header("�̻��� �߻� ��ġ")]
    public Transform firePos;

    [Header("�Ѿ� �����")]
    [SerializeField] float bulletDamage;

    [Header("�Ѿ� ��ź��")]
    [SerializeField] int bulletCount;
    int bulletLevel;

    [Header("�Ѿ� ������")]
    bool onBulletDelay;
    float bulletDelayTime;
    float bulletDelayTimer;
    Coroutine bTimeCoroutine;

    //�Ѿ� ��ǲ�ý���
    GameInput gameInput;
    InputAction gunFire;
    InputAction gunFire_Stick;
    float fireValue;
    
    private void Awake()  //Ǯ���� pools ����Ʈ�� ���� �� �ʱ�ȭ�Ѵ�.  
    {
        gameInput = new GameInput();     
        gunFire = gameInput.Player.GunFire;     
        gunFire_Stick = gameInput.Player.GunFire_Stick;
    }

    void Start()
    {
        bulletDamage = bulletInfo[eqGunIndex].damages[0];
        bulletDelayTime = bulletInfo[eqGunIndex].attackDelay;
        bulletCount = bulletInfo[eqGunIndex].maxCounts[0];
    }

    private void OnEnable()
    {
        gunFire.Enable();     
        gunFire_Stick.Enable();
    }

    private void FixedUpdate()
    {
        if(bulletCount > 0)
        GunFire();
    }

    public void GunFire()
    {
        if (!GameManager.instance.isStick)
            fireValue = gunFire.ReadValue<float>();
        else
            fireValue = gunFire_Stick.ReadValue<float>();

        if (fireValue >= 1f && !onBulletDelay)
        {
            onBulletDelay = true;
            InvokeRepeating("FireBullet", bulletDelayTime, 0);
            if (!audioSource.isPlaying || audioSource.volume <= 0.3f)
            {
                audioSource.volume = 0.5f;
                audioSource.Play();
            }
            muzzleParticles.Play();
        }

        if (fireValue <= 0f)
        {
            if (audioSource.volume > 0)
                audioSource.volume -= 0.03f;
            else if (audioSource.volume == 0f)
                audioSource.Stop();
            CancelInvoke("FireBullet");
            onBulletDelay = false;
        }
    }

    void FireBullet()
    {
        onBulletDelay = false;

        for (int index = 0; index < PoolManager.instance.prefabs.Length; index++)
            if (PoolManager.instance.prefabs[index] == bulletList[eqGunIndex])
            {
                equippedBullet = PoolManager.instance.Get(index);
                equippedBullet.transform.parent = PoolManager.instance.transform.GetChild(index);
                equippedBullet.transform.position = firePos.position;
                equippedBullet.transform.rotation = transform.rotation;
                break;
            }

        RaycastHit hit;
        if(Physics.Raycast(transform.position, transform.forward, out hit, 200f, enemyLayer))
        {
            hit.transform.GetComponent<EnemyHPManager>().EnemyTakeDamage(bulletDamage);
        }

        bulletCount--;
        RumbleManager.instance.RumblePulse(0.05f, 0, bulletDelayTime);
    }
}
