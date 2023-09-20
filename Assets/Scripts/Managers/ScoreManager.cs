using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    private GameManager gameManager = null;

    [SerializeField]
    private TextMeshProUGUI scoreText = null;
    [SerializeField]
    private GameObject comboDisplay = null;
    private TextMeshProUGUI comboCountText = null;
    private Image comboTimerDisplay = null;
    private TextMeshProUGUI comboScoreText = null;

    private int score = 0;

    private int checkpointScore = 0;

    private int comboCount = 0;
    private int comboScore = 0;

    private float comboMultiplier = 1f;
    private int comboStep = 10;
    private float multiplierStep = 0.25f;
    private float maxComboMultiplier = 2.5f;
    
    private float comboDecayTime = 4f;
    private float comboTimer;

    private int deathCount = 0;
    private float timeInLevel = 0f;

    [SerializeField]
    private int deathsBonusLimit = 25;
    [SerializeField]
    private int deathsBonusTarget = 0;
    [SerializeField]
    private float timeBonusLimit = 300f;
    [SerializeField]
    private float timeBonusTarget = 30f;
    [SerializeField]
    private float escapeBonusTarget = 30f; 

    [SerializeField]
    private int maxDeathsBonus = 100000;
    [SerializeField]
    private int maxTimeBonus = 100000;
    [SerializeField]
    private int maxEscapeBonus = 100000;

    [SerializeField]
    private GameObject scoreScreen = null;

    [SerializeField]
    private TextMeshProUGUI finalScoreText = null;
    [SerializeField]
    private Transform timeBonusDisplay = null;
    [SerializeField]
    private Transform escapeBonusDisplay = null;
    [SerializeField]
    private Transform deathsBonusDisplay = null;
    [SerializeField]
    private TextMeshProUGUI totalScoreText = null;

    private int highScore = 0;

    void Start()
    {
        gameManager = GameManager.Instance;

        if (PlayerPrefs.HasKey("Death Count"))
        {
            deathCount = PlayerPrefs.GetInt("Death Count");
        }

        if (PlayerPrefs.HasKey("Time in Level"))
        {
            timeInLevel = PlayerPrefs.GetFloat("Time in Level");
        }

        if (PlayerPrefs.HasKey("Checkpoint Score"))
        {
            score = PlayerPrefs.GetInt("Checkpoint Score");
            checkpointScore = score;
        }

        string levelHighScore = gameManager.GetActiveSceneName() + " High Score";

        if (PlayerPrefs.HasKey(levelHighScore))
        {
            highScore = PlayerPrefs.GetInt(levelHighScore);
        }

        Debug.Log("HIGHSCORE: " + highScore);

        UpdateScoreText();

        comboTimer = comboDecayTime;

        comboCountText = comboDisplay.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        comboTimerDisplay = comboDisplay.transform.GetChild(2).transform.GetChild(1).GetComponent<Image>();
        comboScoreText = comboDisplay.transform.GetChild(3).GetComponent<TextMeshProUGUI>();

        comboDisplay.SetActive(false);

        scoreScreen.SetActive(false);
    }

    void Update()
    {
        DecrementComboTimer(Time.deltaTime);
        timeInLevel += Time.deltaTime;
    }

    private void DecrementComboTimer(float deltaTime)
    {
        if (comboTimer > 0f && comboCount > 0)
        {
            comboTimer -= deltaTime;

            comboTimerDisplay.fillAmount = Mathf.Max(0f, comboTimer / comboDecayTime);

            if (comboTimer <= 0f)
            {
                EndCombo();
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
        scoreText.text = score.ToString("n0");
    }

    private void UpdateComboText()
    {
        comboCountText.text = "x" + comboCount;
        comboScoreText.text = comboScore.ToString("n0") + " x " + comboMultiplier;
    }

    public void SetCheckpointScore()
    {
        checkpointScore = score + Mathf.RoundToInt(comboScore * comboMultiplier);
    }

    public void ResetCheckpointScore()
    {
        PlayerPrefs.SetInt("Checkpoint Score", 0);
    }

    public void SaveCheckpointScore()
    {
        PlayerPrefs.SetInt("Checkpoint Score", checkpointScore);
    }

    public void EndCombo()
    {
        score += Mathf.RoundToInt(comboScore * comboMultiplier);

        UpdateScoreText();

        comboCount = 0;
        comboMultiplier = 1f;

        comboDisplay.SetActive(false);
    }

    public int GetDeathCount()
    {
        return deathCount;
    }

    public void IncrementDeathCount()
    {
        deathCount++;
        PlayerPrefs.SetInt("Death Count", deathCount);
    }

    public void ResetDeathCount()
    {
        PlayerPrefs.SetInt("Death Count", 0);
    }

    public void SaveTimeInLevel()
    {
        PlayerPrefs.SetFloat("Time in Level", timeInLevel);
    }

    public float GetTimeInLevel()
    {
        return timeInLevel;
    }

    public void ResetTimeInLevel()
    {
        PlayerPrefs.SetFloat("Time in Level", 0f);
    }

    public void OpenScoreScreen()
    {
        scoreScreen.SetActive(true);

        finalScoreText.text = score.ToString("n0");

        int timeBonus = GetBonus(timeInLevel, timeBonusLimit, timeBonusTarget, maxTimeBonus);
        
        DisplayBonus(timeBonusDisplay, GetTimerText(timeInLevel), timeInLevel, timeBonusLimit, timeBonusTarget, timeBonus.ToString("n0"));

        float escapeTimeLimit = gameManager.GetEscapeTimeLimit();
        float escapeTime = escapeTimeLimit - gameManager.GetEscapeTimeRemaining();

        int escapeBonus = GetBonus(escapeTime, escapeTimeLimit, escapeBonusTarget, maxEscapeBonus);

        DisplayBonus(escapeBonusDisplay, GetTimerText(escapeTime), escapeTime, escapeTimeLimit, escapeBonusTarget, escapeBonus.ToString("n0"));

        int deathsBonus = GetBonus(deathCount, deathsBonusLimit, deathsBonusTarget, maxDeathsBonus);

        DisplayBonus(deathsBonusDisplay, deathCount.ToString("n0"), deathCount, deathsBonusLimit, deathsBonusTarget, deathsBonus.ToString("n0"));

        int totalScore = score + timeBonus + escapeBonus + deathsBonus;

        totalScoreText.text = totalScore.ToString("n0");

        if (totalScore > highScore)
        {
            PlayerPrefs.SetInt(gameManager.GetActiveSceneName() + " High Score", totalScore);
        }
    }

    private int GetBonus(float value, float limit, float target, float maxBonus)
    {
        float bonus = maxBonus * GetBonusPercentage(value, limit, target);
        bonus = Mathf.Ceil(bonus / 100f) * 100f;

        return (int)bonus;
    }

    private float GetBonusPercentage(float value, float limit, float target)
    {
        return Mathf.InverseLerp(limit, target, value);
    }

    private void DisplayBonus(Transform bonusDisplay, string valueString, float value, float limit, float target, string bonusString)
    {
        bonusDisplay.GetChild(1).GetComponent<TextMeshProUGUI>().text = valueString;
        bonusDisplay.GetChild(2).transform.GetChild(1).GetComponent<Image>().fillAmount = GetBonusPercentage(value, limit, target);
        bonusDisplay.GetChild(3).GetComponent<TextMeshProUGUI>().text = bonusString;
    }

    private string GetTimerText(float timerValue)
    {
        timerValue = Mathf.Max(timerValue, 0f);

        int minutesCount = 0;

        while (timerValue >= 60f)
        {
            timerValue -= 60f;
            minutesCount++;
        }

        string minutes = minutesCount.ToString();

        if (minutesCount < 10)
        {
            minutes = "0" + minutes;
        }

        int secondsCount = Mathf.FloorToInt(timerValue);
        string seconds = secondsCount.ToString();

        if (secondsCount < 10)
        {
            seconds = "0" + seconds;
        }

        timerValue -= secondsCount;

        int millisecondsCount = Mathf.CeilToInt(timerValue * 100f);
        string milliseconds = millisecondsCount.ToString();

        if (millisecondsCount < 10)
        {
            milliseconds = "0" + milliseconds;
        }
        else if (millisecondsCount >= 100)
        {
            milliseconds = milliseconds.Substring(0, 2);
        }

        return minutes + ":" + seconds + ":" + milliseconds;
    }
}
