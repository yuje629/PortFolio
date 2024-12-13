using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class DamageText : MonoBehaviour
{
    [SerializeField] private GameObject cam;
    [SerializeField] private TMP_Text damage_Text;
    [SerializeField] private Animator animator;
    private Transform _transform;

    private void Awake()
    {
        _transform = transform;
    }

    private void Start()
    {
        cam = FindObjectOfType<Camera>().gameObject;
    }

    private void OnEnable()
    {
    }

    public void Init(float damage, Vector3 pos, bool isCritical)
    {
        damage_Text.text = damage.ToString("F2");
        _transform.position = pos;
        animator.SetBool("IsCritical", isCritical);
    }

    private void Update()
    {
        _transform.LookAt(cam.transform.position);
    }

    public void DestroyObject()
    {
        gameObject.SetActive(false);
    }
}
