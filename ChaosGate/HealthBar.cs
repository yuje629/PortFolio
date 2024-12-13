using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider healthSlider;
    public Slider easeHealthSlider;
    public float maxHP;
    public float curHP;

    private float lerpSpeed = 0.01f;
    public GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.instance;

        if (healthSlider != null)
            healthSlider.maxValue = gameManager._maxhp;
        if (easeHealthSlider != null)
            easeHealthSlider.maxValue = gameManager._maxhp;
    }

    void Update()
    {
        UpdateHealthSlider();
        UpdateEaseHealthSlider();

        
        //if (Input.GetKeyUp(KeyCode.UpArrow))
        //    IncreaseHP(10);
        //if (Input.GetKeyUp(KeyCode.DownArrow))
        //    DecreaseHP(10);

    }

    //테스트용--시작
    private void IncreaseHP(float health)
    {
        gameManager._hp += health;
    }

    private void DecreaseHP(float health)
    {
        gameManager._hp -= health;
    }
    //테스트용--끝

    public void UpdateHealthSlider()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = gameManager._maxhp;
            healthSlider.value = gameManager._hp;
        }
    }

    public void UpdateEaseHealthSlider()
    {
        if (easeHealthSlider != null)
        {
            easeHealthSlider.maxValue = healthSlider.maxValue;

            if (easeHealthSlider.value > gameManager._hp)
                easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, gameManager._hp, lerpSpeed);
            else
                easeHealthSlider.value = gameManager._hp;
        }
    }
}
