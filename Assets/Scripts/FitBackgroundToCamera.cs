using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundCoverCamera : MonoBehaviour
{
    public float extraScale = 1.02f;

    private Camera cam;
    private SpriteRenderer sr;

    private float lastOrthoSize;
    private float lastAspect;

    void Awake()
    {
        cam = Camera.main;
        sr = GetComponent<SpriteRenderer>();

        FitToCamera();
        SnapToCamera();
    }

    void LateUpdate()
    {
        if (cam == null) return;
        SnapToCamera();
        if (!Mathf.Approximately(cam.orthographicSize, lastOrthoSize) ||
            !Mathf.Approximately(cam.aspect, lastAspect))
        {
            FitToCamera();
        }
    }

    void FitToCamera()
    {
        if (cam == null || sr == null || sr.sprite == null) return;
        if (!cam.orthographic) return;

        float screenHeight = cam.orthographicSize * 2f;
        float screenWidth = screenHeight * cam.aspect;

        float spriteWidth = sr.sprite.rect.width / sr.sprite.pixelsPerUnit;
        float spriteHeight = sr.sprite.rect.height / sr.sprite.pixelsPerUnit;

        float scale = Mathf.Max(screenWidth / spriteWidth, screenHeight / spriteHeight) * extraScale;
        transform.localScale = new Vector3(scale, scale, 1f);

        lastOrthoSize = cam.orthographicSize;
        lastAspect = cam.aspect;
    }

    void SnapToCamera()
    {
        transform.position = new Vector3(
            cam.transform.position.x,
            cam.transform.position.y,
            transform.position.z
        );
    }
}