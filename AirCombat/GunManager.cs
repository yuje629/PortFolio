using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    [SerializeField] public GameObject[] QTETimeLine;
    [SerializeField] public QTEManager QTE;

    [Header("ÃÑ¾Ë ÇÁ¸®ÆÕ°ú Á¤º¸")]
    public GameObject[] bulletList; //ÃÑ¾Ë ÇÁ¸®ÆÕ
    public WeaponData[] bulletInfo;
    public Transform bulletParent;

    GameObject equippedBullet;
    int eqGunIndex;                  //ÇöÀç ÀåÂø ÁßÀÎ ÃÑ¾Ë 

    [Header("Å¸°Ù ¸¶½ºÅ©")]
    [SerializeField] LayerMask enemyLayer;

    [Header("ÃÑ¾Ë ÀÌÆåÆ®")]
    public ParticleSystem muzzleParticles;

    [Header("¹Ì»çÀÏ ¹ß»ç À§Ä¡")]
    public Transform firePos;

    [Header("ÃÑ¾Ë ´ë¹ÌÁö")]
    [SerializeField] float bulletDamage;

    [Header("ÃÑ¾Ë ÀåÅº¼ö")]
    [SerializeField] int bulletCount;
    int bulletLevel;

    [Header("ÃÑ¾Ë µô·¹ÀÌ")]
    bool onBulletDelay;
    float bulletDelayTime;
    float bulletDelayTimer;
    Coroutine bTimeCoroutine;

    //ÃÑ¾Ë ÀÎÇ²½Ã½ºÅÛ
    GameInput gameInput;
    InputAction gunFire;
    InputAction gunFire_Stick;
    float fireValue;
    
    private void Awake()  //Ç®¸µ¿ë pools ¸®½ºÆ®¸¦ »ý¼º ¹× ÃÊ±âÈ­ÇÑ´Ù.  
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
