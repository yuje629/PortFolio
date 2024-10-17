using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Jobs;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class DarkMagician : MonoBehaviour, IDamageable
{
    [SerializeField] private int HP;
    public GameObject[] bossEffect;
    [SerializeField] PoolManager poolManager;
    SoundManager soundManager;
    [SerializeField] GameObject[] obstacles;

    [Header("Pattern1-MapChange")]
    public GameObject overviewCam;
    public GameObject[] mapObject;   //0:맵1, 몹소환 패턴있음  //1:맵2  //2:맵3
    private GameObject currentMap;
    public Material mapMaterial;
    private float fade;
    public Transform mapChangePlayerPos;

    [Header("Pattern2-monster")]
    public GameObject[] monsterPrefab;
    public Transform[] monsterSpawnPos;
    public GameObject monsterPortal;

    [Header("Pattern3-8Direction")]
    public GameObject attackBall;
    public GameObject[] warningLine;

    [Header("Pattern4-MagicCircle")]
    public GameObject magicCircle;
    public BoxCollider2D[] magicCircleRange;

    [Header("Pattern5-Teleport")]
    public Transform[] teleportPos;
    private int randTeleportPos;
    public float currentPos;

    [Header("Pattern6-DropStones")]
    public Animator dropStoneAnim;
    public CameraController cc;

    [Header("Pattern7-Laser")]
    public GameObject bossLaser;
    public GameObject laserWarningLine;
    public Transform laserStartPos;
    public Transform laserEndPos;

    [Header("MagicBall")]
    public Transform[] magicBallSpawnPos;
    public GameObject[] magicBall;

    private Transform target;
    private BoxCollider2D targetCol;
    private Rigidbody2D targetRigid;
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        //플레이어 관련 초기화
        target = GameObject.FindGameObjectWithTag("Player").transform;
        targetCol = target.GetComponent<BoxCollider2D>();
        targetRigid = target.GetComponent<Rigidbody2D>();
        soundManager = GameObject.FindWithTag("Sound").GetComponent<SoundManager>();
    }

    private void OnEnable()
    {
        //맵 체인지 스킬 관련 초기화
        currentMap = mapObject[0];
        fade = 1;
        mapMaterial.SetFloat("_Fade", fade);

        //보스 초기화
        currentPos = 1;
        anim.SetTrigger("doIdle");

        //보스 패턴 시작
        StartCoroutine(BossPattern());
    }

    void Update()
    {
        if (GameManager.instance.isClear || GameManager.instance.isPlayerDead)
            StopAllCoroutines();
    }

    IEnumerator BossPattern()
    {
        int mapIndex = 0;

        while (true)
        {
            mapIndex++;
            if (mapIndex >= mapObject.Length)
                mapIndex = 0;
            yield return YieldCache.WaitForSeconds(8f);
            StartCoroutine(SpawnMob());
            SpawnMagicBall();
            yield return YieldCache.WaitForSeconds(10f);
            StartCoroutine(DropStones()); 
            yield return YieldCache.WaitForSeconds(8f);
            StartCoroutine(ShootLaser());
            yield return YieldCache.WaitForSeconds(10f);
            StartCoroutine(Attack8Direction());
            yield return YieldCache.WaitForSeconds(10f);
            StartCoroutine(SpawnMagicCircle());
            yield return YieldCache.WaitForSeconds(10f);
            StartCoroutine(Attack8Direction());
            yield return YieldCache.WaitForSeconds(10f);
            StartCoroutine(SpawnMob());
            yield return YieldCache.WaitForSeconds(10f);
            SpawnMagicBall();
            StartCoroutine(SpawnMagicCircle());
            yield return YieldCache.WaitForSeconds(10f);
            StartCoroutine(ThrowTargetAway());
            yield return YieldCache.WaitForSeconds(3f);
            StartCoroutine(ChangeMap(mapIndex));
            yield return YieldCache.WaitForSeconds(10f);
        }
    }

    IEnumerator ThrowTargetAway()
    {
        overviewCam.SetActive(true);
        //플레이어 이동하는 동안 충돌X, 컨트롤 못 하게
        targetCol.enabled = false;
        targetRigid.velocity = Vector2.zero;
        float existingGravityScale = targetRigid.gravityScale;
        targetRigid.gravityScale = 0;
        target.GetComponent<Movement>().enabled = false;
        target.GetComponent<Hook>().enabled = false;
        target.GetComponent<Hook>().InitHook();
        target.transform.GetChild(0).gameObject.SetActive(false);

        float speed = 1f;

        while (Vector2.Distance(target.position, mapChangePlayerPos.position) >= 0.2f)
        {
            target.position = Vector3.MoveTowards(target.position, mapChangePlayerPos.position, speed);
            yield return null;
        }
        //플레이어 이동 끝나서 충돌O
        targetCol.enabled = true;
        targetRigid.gravityScale = existingGravityScale;
        yield return YieldCache.WaitForSeconds(2f);
    }


    IEnumerator ChangeMap(int index)  //RED
    {
        soundManager.PlaySFX(soundManager.magic);
        bossEffect[2].SetActive(true);
        anim.SetTrigger("doAttack2");

        yield return YieldCache.WaitForSeconds(2f);

        soundManager.PlaySFX(soundManager.mapChange);
        while (fade >= 0)
        {
            fade -= Time.deltaTime;

            mapMaterial.SetFloat("_Fade", fade);

            yield return null;
        }

        currentMap.SetActive(false);

        mapObject[index].SetActive(true);

        currentMap = mapObject[index];

        soundManager.PlaySFX(soundManager.mapChange);
        while (fade <= 1)
        {
            fade += Time.deltaTime;

            mapMaterial.SetFloat("_Fade", fade);

            yield return null;
        }
        overviewCam.SetActive(false);

        //맵 전환 끝나서 플레이어 컨트롤 가능
        target.GetComponent<Movement>().enabled = true;
        target.GetComponent<Hook>().enabled = true;

        yield return YieldCache.WaitForSeconds(1f);

        target.transform.GetChild(0).gameObject.SetActive(true);
    }

    IEnumerator TeleportCoroutine()
    {
        randTeleportPos = Random.Range(0, teleportPos.Length);
        if (currentPos != randTeleportPos)
        {
            currentPos = randTeleportPos;
            anim.SetTrigger("doTeleport");
        }
        yield return null;
    }

    private void Teleportation()
    {
        transform.position = teleportPos[randTeleportPos].position;
    }

    IEnumerator SpawnMob()  //Indigo
    {
        soundManager.PlaySFX(soundManager.magic);
        bossEffect[1].SetActive(true);
        anim.SetTrigger("doAttack2");

        yield return YieldCache.WaitForSeconds(2f);


        soundManager.PlaySFX(soundManager.portal);
        //몬스터포탈 소환
        for (int i = 0; i < monsterSpawnPos.Length; i++)
        {
            GameObject portal = Instantiate(monsterPortal, monsterSpawnPos[i].position, Quaternion.identity);
            Destroy(portal, 2f);
        }

        yield return YieldCache.WaitForSeconds(1f);

        int rand;

        //지상 몬스터 소환
        for (int j = 0; j < 3; j++)
        {
            rand = Random.Range(0, 3);
            GameObject monster = Instantiate(monsterPrefab[rand], monsterSpawnPos[j].position,Quaternion.identity);
            Destroy(monster, 18f);
        }

        //공중 몬스터 소환
        for (int k = 0; k < 3; k++)
        {
            GameObject monster = Instantiate(monsterPrefab[3], monsterSpawnPos[3 + k].position, Quaternion.identity);
            Destroy(monster, 18f);
        }
    }

    IEnumerator Attack8Direction() //skyblue
    {
        soundManager.PlaySFX(soundManager.magic);
        bossEffect[3].SetActive(true);
        anim.SetTrigger("doAttack1");

        yield return YieldCache.WaitForSeconds(2f);

        for (int i = 0; i < warningLine.Length; i++)
        {
            float radian = i * 45 * Mathf.Deg2Rad;
            Vector2 lineStartPos = new Vector2(transform.position.x + 5 * Mathf.Cos(radian), transform.position.y + 5 * Mathf.Sin(radian));
            Vector2 lineEndPos = new Vector2(transform.position.x + 100 * Mathf.Cos(radian), transform.position.y + 100 * Mathf.Sin(radian));
            warningLine[i].GetComponent<WarningLine>().LineInit(3, lineStartPos, lineEndPos, 4f);
            warningLine[i].SetActive(true);
        }

        yield return YieldCache.WaitForSeconds(1f);

        for (int j = 0; j < 3; j++)
        {
            soundManager.PlaySFX(soundManager.attackBall);
            for (int k = 0; k < 360; k += 45)
            {
                var go = poolManager.GetGo("AttackBall");
                go.transform.position = transform.position;
                go.transform.rotation = Quaternion.Euler(0, 0, k);
            }

            yield return YieldCache.WaitForSeconds(1f);
        }
    }

    IEnumerator SpawnMagicCircle()  //YELLOW
    {
        soundManager.PlaySFX(soundManager.magicCircle);
        bossEffect[4].SetActive(true);
        anim.SetTrigger("doAttack1");
        yield return YieldCache.WaitForSeconds(2f);

        int rand = Random.Range(3, magicCircleRange.Length);
        for (int i = 0; i < rand; i++)
        {
            float randX = Random.Range(magicCircleRange[i].bounds.min.x, magicCircleRange[i].bounds.max.x);
            float randY = Random.Range(magicCircleRange[i].bounds.min.y, magicCircleRange[i].bounds.max.y);

            Vector2 spawnPos = new Vector2(randX, randY);

            var go = poolManager.GetGo("MagicCircle");
            go.transform.position = spawnPos;
            go.transform.rotation = Quaternion.identity;
        }
    }

    private void SpawnMagicBall()
    {
        for(int i = 0; i < magicBall.Length; i++)
        {
            if (!magicBall[i].activeSelf)
            {
                magicBall[i].SetActive(true);
                magicBall[i].GetComponent<Grap>().Init(magicBallSpawnPos[i].position);
            }
        }
    }

    IEnumerator DropStones()
    {
        soundManager.PlaySFX(soundManager.magic);
        bossEffect[0].SetActive(true);
        anim.SetTrigger("doAttack1");
        yield return YieldCache.WaitForSeconds(0.5f);

        soundManager.PlaySFX(soundManager.earthquake);
        cc.Shake(4f, 0.11f, true);
        dropStoneAnim.SetTrigger("doDrop");
    }

    IEnumerator ShootLaser()
    {
        soundManager.PlaySFX(soundManager.magic);
        bossEffect[5].SetActive(true);
        anim.SetTrigger("doAttack2");
        laserWarningLine.GetComponent<WarningLine>().LineInit(2, laserStartPos.position, laserEndPos.position, 2f);
        laserWarningLine.SetActive(true);
        yield return YieldCache.WaitForSeconds(3f);

        soundManager.PlaySFX(soundManager.Laser);
        bossLaser.SetActive(true);
        bossLaser.GetComponent<BossLaser>().StartRotateCoroutine(359, 20f);
    }

    public void TakeDamage(float damage)
    {
        HP -= (int)damage;
        anim.SetTrigger("doHit");
        Debug.Log("BossHit" + DateTime.Now);
        if (HP <= 0)
        {
            anim.SetTrigger("doDie");
            GameManager.instance.isClear = true;
            HideObstacle();
        }
    }

    private void HideObstacle()
    {
        for(int i = 0; i < obstacles.Length; i++)
        {
            obstacles[i].SetActive(false);
        }
    }
}
