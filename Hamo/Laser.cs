using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UIElements;

public class Laser : MonoBehaviour
{
    private Transform       target;

    private void OnEnable ()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        float angle = GetAngle(target.position, transform.position);
        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.AngleAxis(angle - 90f, Vector3.forward), Time.deltaTime * 50f);
    }

    private float GetAngle(Vector2 start, Vector2 end)
    {
        Vector2 direction = start - end;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        return angle;
    }
}
