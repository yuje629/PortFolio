using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [HideInInspector] public SoundManager soundManager;
    public bool             isHooked;
    public LayerMask        groundLayer;
    public LayerMask        ObstacleLayer;
    protected Transform     target;
    protected Transform     myTransform;
    protected float         colSize;
    protected SlowMotion    slow;

    protected Transform GetPlayer()
    { 
        target = GameObject.FindGameObjectWithTag("Player").transform;

        return target;
    }

    protected bool ObstacleHitCheck(Vector2 direction)
    {
        if (Physics2D.Raycast(myTransform.position, direction, colSize * 1.2f, ObstacleLayer))
        {
            return true;
        }
        return false;
    }

    public void onStartHit()
    {
        isHooked = true;
    }

    protected void onEndHit()
    {
        isHooked = false;
    }

    protected void CheckGameEnd()
    {
        if(GameManager.instance.isClear || GameManager.instance.isPlayerDead)
            Destroy(gameObject);
    }
}
