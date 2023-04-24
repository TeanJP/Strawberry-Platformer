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
    }

    private void ClampCameraPosiion()
    {
        float xClamped = Mathf.Clamp(mainCamera.transform.position.x, levelBoundaries.left + mainCameraDimensions.x, levelBoundaries.right - mainCameraDimensions.x);
        float yClamped = Mathf.Clamp(mainCamera.transform.position.y, levelBoundaries.bottom + mainCameraDimensions.y, levelBoundaries.top - mainCameraDimensions.y);

        mainCamera.transform.position = new Vector3(xClamped, yClamped, cameraOffest.z);
    }
}
