using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    private Camera mainCamera = null;
    private Vector2 mainCameraDimensions;

    private Strawberry strawberry = null;

    [System.Serializable]
    private class LevelBoundaries
    {
        public float left;
        public float right;
        public float top;
        public float bottom;

        public LevelBoundaries(float left = -10f, float right = 10f, float top = 6f, float bottom = -6f)
        {
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }
    }

    [SerializeField]
    private LevelBoundaries levelBoundaries = new LevelBoundaries();

    private Vector3 cameraOffest = new Vector3(0f, 0f, -10f);

    private bool strawberryOffscreen = false;
    [SerializeField]
    private float offscreenAllowance = 3f;
    private float offscreenTimer;
    [SerializeField]
    private float offscreenLeniency = 0.8f;

    void Start()
    {
        strawberry = GameManager.Instance.GetStrawberryInstance();

        mainCamera = Camera.main;
        mainCameraDimensions = new Vector2(mainCamera.orthographicSize * mainCamera.aspect, mainCamera.orthographicSize);
    }

    void Update()
    {
        mainCamera.transform.position = strawberry.transform.position + cameraOffest;
        ClampCameraPosiion();

        CheckStrawberryOffscreen();
        /*
        if (strawberryOffscreen)
        {
            offscreenTimer = Mathf.Max(offscreenTimer - Time.deltaTime, 0f);

            if (offscreenTimer == 0f)
            {
                strawberry.SetDefeated();
            }
        }
        */
    }

    private void ClampCameraPosiion()
    {
        float xClamped = Mathf.Clamp(mainCamera.transform.position.x, levelBoundaries.left + mainCameraDimensions.x, levelBoundaries.right - mainCameraDimensions.x);
        float yClamped = Mathf.Clamp(mainCamera.transform.position.y, levelBoundaries.bottom + mainCameraDimensions.y, levelBoundaries.top - mainCameraDimensions.y);

        mainCamera.transform.position = new Vector3(xClamped, yClamped, cameraOffest.z);
    }

    private void CheckStrawberryOffscreen()
    {
        Vector2 strawberryMax = strawberry.GetColliderBoundsMax();
        Vector2 strawberryMin = strawberry.GetColliderBoundsMin();

        bool strawberryLyingDown = strawberry.GetLyingDown();

        if (strawberryLyingDown)
        {
            float strawberryCentre = strawberry.GetCentre().x;
            float halfStrawberryWidth = strawberry.GetSpriteRendererWidth() * 0.5f;

            strawberryMax.x = strawberryCentre + halfStrawberryWidth * offscreenLeniency;
            strawberryMin.x = strawberryCentre - halfStrawberryWidth * offscreenLeniency;
        }

        if (strawberryMin.x > levelBoundaries.right || strawberryMin.y > levelBoundaries.top || strawberryMax.x < levelBoundaries.left || strawberryMax.y < levelBoundaries.bottom)
        {
            /*
            if (!strawberryOffscreen)
            {
                offscreenTimer = offscreenAllowance;
            }
            
            strawberryOffscreen = true;
            */

            strawberry.SetDefeated();
        }
        else
        {
            strawberryOffscreen = false;
        }
    }
}
