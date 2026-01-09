using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRMicrogesturePrefabController : MonoBehaviour
{
    [Header("Prefab Settings")]
    [SerializeField] private List<GameObject> prefabs = new List<GameObject>();
    private int currentPrefabIndex = 0;

    [Header("Settings")]
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float initialDistance = 3f;
    [SerializeField] private float transitionDuration = 1f;

    [Header("Gesture Event Sources")]
    [SerializeField] private OVRMicrogestureEventSource rightHandGestureSource;
    [SerializeField] private OVRMicrogestureEventSource leftHandGestureSource;

    [Header("Components")]
    [SerializeField] private Camera xrCamera;

    private Vector3 rotationAxis = Vector3.zero;
    private bool isRotating = false;
    private bool isTransitioning = false;
    private bool isZooming = false;
    private bool isZoomingIn = false;

    void Start()
    {
        if (rightHandGestureSource != null)
            rightHandGestureSource.GestureRecognizedEvent.AddListener(OnRightHandGesture);

        if (leftHandGestureSource != null)
            leftHandGestureSource.GestureRecognizedEvent.AddListener(OnLeftHandGesture);

        InitializePrefabs();
        PositionCurrentPrefabInView();
    }

    void Update()
    {
        HandleRotation();
        HandleZoom();
    }

    void OnRightHandGesture(OVRHand.MicrogestureType gesture)
    {
        switch (gesture)
        {
            case OVRHand.MicrogestureType.SwipeLeft:
                StartRotation(Vector3.up);
                break;
            case OVRHand.MicrogestureType.SwipeRight:
                StartRotation(-Vector3.up);
                break;
            case OVRHand.MicrogestureType.SwipeForward:
                StartRotation(Vector3.right);
                break;
            case OVRHand.MicrogestureType.SwipeBackward:
                StartRotation(-Vector3.right);
                break;
            case OVRHand.MicrogestureType.ThumbTap:
                StopRotation();
                break;
        }
    }

    void OnLeftHandGesture(OVRHand.MicrogestureType gesture)
    {
        switch (gesture)
        {
            case OVRHand.MicrogestureType.SwipeLeft:
                ChangeToPreviousPrefab();
                break;
            case OVRHand.MicrogestureType.SwipeRight:
                ChangeToNextPrefab();
                break;
            case OVRHand.MicrogestureType.SwipeForward:
                StartZoom(true);
                break;
            case OVRHand.MicrogestureType.SwipeBackward:
                StartZoom(false);
                break;
            case OVRHand.MicrogestureType.ThumbTap:
                StopZoom();
                break;
        }
    }

    void InitializePrefabs()
    {
        for (int i = 0; i < prefabs.Count; i++)
            prefabs[i].SetActive(i == currentPrefabIndex);
    }

    void PositionCurrentPrefabInView()
    {
        var currentPrefab = prefabs[currentPrefabIndex];
        var targetPosition = xrCamera.transform.position + (xrCamera.transform.forward * initialDistance);
        currentPrefab.transform.position = targetPosition;

        var directionToCamera = (xrCamera.transform.position - currentPrefab.transform.position).normalized;
        currentPrefab.transform.rotation = Quaternion.LookRotation(-directionToCamera);
    }

    void StartRotation(Vector3 axis)
    {
        rotationAxis = axis;
        isRotating = true;
    }

    void StopRotation()
    {
        isRotating = false;
        rotationAxis = Vector3.zero;
    }

    void StartZoom(bool zoomIn)
    {
        isZooming = true;
        isZoomingIn = zoomIn;
    }

    void StopZoom()
    {
        isZooming = false;
    }

    void HandleRotation()
    {
        if (!isRotating) return;
        prefabs[currentPrefabIndex].transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime, Space.World);
    }

    void HandleZoom()
    {
        if (!isZooming) return;

        if (isZoomingIn) ZoomIn();
        else ZoomOut();
    }

    void ZoomIn()
    {
        var currentPrefab = prefabs[currentPrefabIndex];
        var direction = (xrCamera.transform.position - currentPrefab.transform.position).normalized;
        var newPosition = currentPrefab.transform.position + direction * zoomSpeed * Time.deltaTime;

        float distanceToCamera = Vector3.Distance(newPosition, xrCamera.transform.position);

        if (distanceToCamera > minDistance)
        {
            currentPrefab.transform.position = newPosition;
        }
    }

    void ZoomOut()
    {
        var currentPrefab = prefabs[currentPrefabIndex];
        var direction = (currentPrefab.transform.position - xrCamera.transform.position).normalized;
        var newPosition = currentPrefab.transform.position + direction * zoomSpeed * Time.deltaTime;

        float distanceToCamera = Vector3.Distance(newPosition, xrCamera.transform.position);

        if (distanceToCamera < maxDistance)
        {
            currentPrefab.transform.position = newPosition;
        }
    }

    void ChangeToPreviousPrefab()
    {
        if (isTransitioning) return;

        int previousIndex = currentPrefabIndex;
        currentPrefabIndex = (currentPrefabIndex - 1 + prefabs.Count) % prefabs.Count;
        StartCoroutine(ConveyorTransition(previousIndex, currentPrefabIndex, true));
    }

    void ChangeToNextPrefab()
    {
        if (isTransitioning) return;

        int previousIndex = currentPrefabIndex;
        currentPrefabIndex = (currentPrefabIndex + 1) % prefabs.Count;
        StartCoroutine(ConveyorTransition(previousIndex, currentPrefabIndex, false));
    }

    IEnumerator ConveyorTransition(int fromIndex, int toIndex, bool movingLeft)
    {
        isTransitioning = true;

        bool wasRotating = isRotating;
        bool wasZooming = isZooming;
        Vector3 prevRotationAxis = rotationAxis;
        bool wasZoomingIn = isZoomingIn;

        StopRotation();
        StopZoom();

        var fromPrefab = prefabs[fromIndex];
        var toPrefab = prefabs[toIndex];
        var centerPosition = fromPrefab.transform.position;
        var offset = Vector3.right * 5f;

        toPrefab.transform.position = centerPosition + (movingLeft ? -offset : offset);
        toPrefab.SetActive(true);

        float elapsedTime = 0f;
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / transitionDuration;

            if (movingLeft)
            {
                fromPrefab.transform.position = Vector3.Lerp(centerPosition, centerPosition + offset, progress);
                toPrefab.transform.position = Vector3.Lerp(centerPosition - offset, centerPosition, progress);
            }
            else
            {
                fromPrefab.transform.position = Vector3.Lerp(centerPosition, centerPosition - offset, progress);
                toPrefab.transform.position = Vector3.Lerp(centerPosition + offset, centerPosition, progress);
            }
            yield return null;
        }

        fromPrefab.SetActive(false);
        fromPrefab.transform.position = centerPosition;
        toPrefab.transform.position = centerPosition;

        if (wasRotating) StartRotation(prevRotationAxis);
        if (wasZooming) StartZoom(wasZoomingIn);

        isTransitioning = false;
    }

    void OnDestroy()
    {
        if (rightHandGestureSource != null)
            rightHandGestureSource.GestureRecognizedEvent.RemoveListener(OnRightHandGesture);

        if (leftHandGestureSource != null)
            leftHandGestureSource.GestureRecognizedEvent.RemoveListener(OnLeftHandGesture);
    }
}