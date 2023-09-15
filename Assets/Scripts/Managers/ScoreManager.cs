using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI scoreText = null;
    [SerializeField]
    private GameObject comboDisplay = null;
    private TextMeshProUGUI comboCountText = null;
    private Image comboTimerDisplay = null;
    private TextMeshProUGUI comboScoreText = null;

    private int totalScore = 0;

    private int checkpointScore = 0;

    private int comboCount = 0;
    private int comboScore = 0;

    private float comboMultiplier = 1f;
    private int comboStep = 10;
    private float multiplierStep = 0.25f;
    private float maxComboMultiplier = 2.5f;
    
    private float comboDecayTime = 4f;
    private float comboTimer;

    void Start()
    {
        if (PlayerPrefs.HasKey("Checkpoint Score"))
        {
            totalScore = PlayerPrefs.GetInt("Checkpoint Score");
        }

        UpdateScoreText();

        comboTimer = comboDecayTime;

        comboCountText = comboDisplay.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        comboTimerDisplay = comboDisplay.transform.GetChild(2).transform.GetChild(1).GetComponent<Image>();
        comboScoreText = comboDisplay.transform.GetChild(3).GetComponent<TextMeshProUGUI>();

        comboDisplay.SetActive(false);
    }

    void Update()
    {
        DecrementComboTimer(Time.deltaTime);
    }

    private void DecrementComboTimer(float deltaTime)
    {
        if (comboTimer > 0f && comboCount > 0)
        {
            comboTimer -= deltaTime;

            comboTimerDisplay.fillAmount = Mathf.Max(0f, comboTimer / comboDecayTime);

            if (comboTimer <= 0f)
            {
                totalScore += Mathf.RoundToInt(comboScore * comboMultiplier);

                UpdateScoreText();

                comboCount = 0;
                comboMultiplier = 1f;

                comboDisplay.SetActive(false);
            }
        }
    }

    public void AddScore(int scoreToAdd)
    {
        if (!comboDisplay.activeInHierarchy)
        {
            comboDisplay.SetActive(true);
        }

        comboCount++;

        if (comboCount % comboStep == 0 && comboMultiplier != maxComboMultiplier)
        {
            comboMultiplier = 1f + (multiplierStep * comboCount / 10);
        }

        comboScore += scoreToAdd; 
        comboTimer = comboDecayTime;

        UpdateComboText();
    }

    private void UpdateScoreText()
    {
        scoreText.text = totalScore.ToString();
    }

    private void UpdateComboText()
    {
        comboCountText.text = "x" + comboCount;
        comboScoreText.text = comboScore + " x " + comboMultiplier;
    }

    public void SetCheckpointScore()
    {
        checkpointScore = totalScore + Mathf.RoundToInt(comboScore * comboMultiplier);
    }

    public void ResetCheckpointScore()
    {
        PlayerPrefs.SetInt("Checkpoint Score", 0);
    }

    public void SaveCheckpointScore()
    {
        PlayerPrefs.SetInt("Checkpoint Score", checkpointScore);
    }
}
