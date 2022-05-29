using UnityEngine;

public class KeepMapView : MonoBehaviour
{
    private Vector2 screenResolution;
    private Camera _camera;
    public Vector2 desiderAspectRatio = new Vector2(16, 9);
    public float mapWidth = 10.6f;

    private void Start()
    {
        _camera = GetComponent<Camera>();
        UpdateCamera();
    }

    private void OnValidate()
    {
        if(_camera == null)
        {
            _camera = GetComponent<Camera>();
        }

        UpdateCamera();
    }

    private void Update()
    {
        if(screenResolution.x != Screen.width || screenResolution.y != Screen.height)
        {
            UpdateCamera();
        }
    }

    private void UpdateCamera()
    {
        screenResolution = new Vector2(Screen.width, Screen.height);

        /*
        float desiredAspect = desiderAspectRatio.x / desiderAspectRatio.y;
        float actualAspect = _camera.aspect;
        float ratio = desiredAspect / actualAspect;

        float newHeight = mapWidth * ratio;
        _camera.orthographicSize = newHeight / 2;*/

        _camera.orthographicSize = mapWidth / 2 / _camera.aspect;

        _camera.transform.position = new Vector3(0, transform.position.y, -1);
    }
}
