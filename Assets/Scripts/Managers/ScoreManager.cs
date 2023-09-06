using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    [SerializeField]
    private Text scoreDisplay = null;
    [SerializeField]
    private Text comboDisplay = null;
    [SerializeField]
    private Text comboMultiplierDisplay = null;

    private int score = 0;

    private int combo = 0;

    private float comboMultiplier = 1f;
    private int comboStep = 10;
    private float multiplierStep = 0.25f;
    private float maxComboMultiplier = 2.5f;
    
    private float comboDecayTime = 4f;
    private float comboTimer;

    void Start()
    {
        comboTimer = comboDecayTime;

        scoreDisplay.text = "SCORE: " + score;

        comboDisplay.enabled = false;
        comboMultiplierDisplay.enabled = false;
    }

    void Update()
    {
        DecrementComboTimer(Time.deltaTime);
    }

    private void DecrementComboTimer(float deltaTime)
    {
        if (comboTimer > 0f && combo > 0)
        {
            comboTimer -= deltaTime;

            if (comboTimer <= 0f)
            {
                combo = 0;
                comboMultiplier = 1f;
            }
        }
    }

    public void AddScore(int scoreToAdd, bool increaseCombo)
    {
        if (increaseCombo)
        {
            combo++;

            if (combo % comboStep == 0 && comboMultiplier != maxComboMultiplier)
            {
                comboMultiplier = 1f + (multiplierStep * combo);
            }
        }

        score += Mathf.RoundToInt(scoreToAdd * comboMultiplier);

        comboTimer = comboDecayTime;
    }
}
