using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeartDisplay : MonoBehaviour
{
    private enum State
    {
        Stopped,
        Increasing,
        Decreasing
    }

    private State state = State.Stopped;

    [SerializeField]
    private float maxY = 5f;
    [SerializeField]
    private float minY = -160f;

    private float targetY;
    private float currentY;

    [SerializeField]
    private float movementSpeed = 6f;

    [SerializeField]
    private TextMeshProUGUI counter = null;
    [SerializeField]
    private RectTransform liquid = null;

    [SerializeField]
    private List<Color32> rainbowColours = new List<Color32>();
    private int firstColour = 0;

    [SerializeField]
    private float rainbowDuration = 0.2f;
    private float rainbowTimer = 0f;

    void Start()
    {
        if (liquid.position.y < minY || liquid.position.y > maxY)
        {
            liquid.localPosition = new Vector2(0f, Mathf.Clamp(liquid.localPosition.y, minY, maxY));
        }

        currentY = liquid.localPosition.y;
        targetY = currentY;
    }

    void Update()
    {
        if (targetY == maxY)
        {
            //Rainbow colours on counter text.
            PlayRainbowAnimation();
        }

        switch (state)
        {
            case State.Increasing:
                currentY += (movementSpeed * Time.deltaTime);
                
                if (currentY > targetY)
                {
                    currentY = targetY;
                }
                break;
            case State.Decreasing:
                currentY -= (movementSpeed * Time.deltaTime);

                if (currentY < targetY)
                {
                    currentY = targetY;
                }
                break;
        }

        if (currentY == targetY)
        {
            state = State.Stopped;
        }
        else
        {
            liquid.localPosition = new Vector2(0f, currentY);
        }
    }

    public void Refresh(float currentHearts, float maxHearts)
    {
        counter.text = currentHearts.ToString();
        counter.color = Color.white;

        targetY = Mathf.Lerp(minY, maxY, currentHearts / maxHearts);

        if (currentY < targetY)
        {
            state = State.Increasing;
        }
        else if (currentY > targetY)
        {
            state = State.Decreasing;
        }
    }

    private void PlayRainbowAnimation()
    {
        rainbowTimer += Time.deltaTime;

        if (rainbowTimer >= rainbowDuration)
        {
            TMP_TextInfo textInfo = counter.textInfo;

            int currentColour = firstColour;

            for (int i = textInfo.characterCount - 1; i >= 0; i--)
            {
                if (textInfo.characterInfo[i].isVisible)
                {
                    int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

                    Color32[] vertexColours = textInfo.meshInfo[materialIndex].colors32;

                    int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                    for (int j = 0; j < 4; j++)
                    {
                        vertexColours[vertexIndex + j] = rainbowColours[currentColour];
                    }
                }

                currentColour++;

                if (currentColour >= rainbowColours.Count)
                {
                    currentColour = 0;
                }
            }

            counter.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            firstColour++;

            if (firstColour >= rainbowColours.Count)
            {
                firstColour = 0;
            }

            rainbowTimer = 0f;
        }
    }
}
