using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IDamageable
{
    public void TakeDamage(float damage);
}

interface IDetectable
{
    public void OnDetect();
}
