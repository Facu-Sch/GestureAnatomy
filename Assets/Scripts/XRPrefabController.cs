using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class XRPrefabController : MonoBehaviour
{
    [Header("Prefab Settings")]
    [SerializeField] private List<GameObject> prefabs = new List<GameObject>();
    [SerializeField] private int currentPrefabIndex = 0;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 90f; // grados por segundo

    [Header("Camera Settings")]
    [SerializeField] private Camera xrCamera;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float initialDistance = 3f; // Distancia inicial del prefab respecto a la cámara

    [Header("Conveyor Effect Settings")]
    [SerializeField] private float transitionDuration = 1f;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // Variables privadas
    private Vector3 rotationAxis = Vector3.zero;
    private bool isRotating = false;
    private bool isTransitioning = false;
    private Vector3 originalCameraPosition;
    private Vector3 targetCameraPosition;

    // Input System variables
    private InputAction upArrowAction;
    private InputAction downArrowAction;
    private InputAction leftArrowAction;
    private InputAction rightArrowAction;
    private InputAction spaceAction;
    private InputAction aKeyAction;
    private InputAction dKeyAction;
    private InputAction zKeyAction;
    private InputAction xKeyAction;

    void Start()
    {
        // Si no se asigna la cámara, buscar la cámara principal
        if (xrCamera == null)
        {
            xrCamera = Camera.main;
            if (xrCamera == null)
            {
                xrCamera = FindFirstObjectByType<Camera>();
            }
        }

        // Guardar posición original de la cámara
        if (xrCamera != null)
        {
            originalCameraPosition = xrCamera.transform.position;
            targetCameraPosition = originalCameraPosition;
        }

        // Validar que tengamos prefabs
        if (prefabs.Count == 0)
        {
            Debug.LogWarning("No se han asignado prefabs a la lista");
            return;
        }

        // Inicializar prefabs - todos desactivados excepto el primero
        InitializePrefabs();

        // Posicionar el primer prefab frente a la cámara
        PositionCurrentPrefabInView();

        // Configurar Input System
        SetupInputActions();
    }

    void SetupInputActions()
    {
        // Configurar acciones de input
        upArrowAction = new InputAction("UpArrow", binding: "<Keyboard>/upArrow");
        downArrowAction = new InputAction("DownArrow", binding: "<Keyboard>/downArrow");
        leftArrowAction = new InputAction("LeftArrow", binding: "<Keyboard>/leftArrow");
        rightArrowAction = new InputAction("RightArrow", binding: "<Keyboard>/rightArrow");
        spaceAction = new InputAction("Space", binding: "<Keyboard>/space");
        aKeyAction = new InputAction("AKey", binding: "<Keyboard>/a");
        dKeyAction = new InputAction("DKey", binding: "<Keyboard>/d");
        zKeyAction = new InputAction("ZKey", binding: "<Keyboard>/z");
        xKeyAction = new InputAction("XKey", binding: "<Keyboard>/x");

        // Suscribir eventos
        upArrowAction.performed += _ => StartRotation(Vector3.right);
        downArrowAction.performed += _ => StartRotation(-Vector3.right);
        leftArrowAction.performed += _ => StartRotation(Vector3.up);
        rightArrowAction.performed += _ => StartRotation(-Vector3.up);
        spaceAction.performed += _ => StopRotation();
        aKeyAction.performed += _ => ChangeToPreviousPrefab();
        dKeyAction.performed += _ => ChangeToNextPrefab();

        // Habilitar todas las acciones
        upArrowAction.Enable();
        downArrowAction.Enable();
        leftArrowAction.Enable();
        rightArrowAction.Enable();
        spaceAction.Enable();
        aKeyAction.Enable();
        dKeyAction.Enable();
        zKeyAction.Enable();
        xKeyAction.Enable();
    }

    void Update()
    {
        HandleZoomInput();
        HandleRotation();
    }

    void HandleZoomInput()
    {
        // Zoom - estas acciones necesitan ser continuas
        if (zKeyAction.IsPressed())
        {
            ZoomIn();
        }
        else if (xKeyAction.IsPressed())
        {
            ZoomOut();
        }
    }

    void InitializePrefabs()
    {
        for (int i = 0; i < prefabs.Count; i++)
        {
            if (prefabs[i] != null)
            {
                prefabs[i].SetActive(i == currentPrefabIndex);
            }
        }
    }

    void PositionCurrentPrefabInView()
    {
        if (xrCamera == null || prefabs.Count == 0 || prefabs[currentPrefabIndex] == null)
            return;

        GameObject currentPrefab = prefabs[currentPrefabIndex];

        // Calcular posición frente a la cámara
        Vector3 cameraForward = xrCamera.transform.forward;
        Vector3 targetPosition = xrCamera.transform.position + (cameraForward * initialDistance);

        // Posicionar el prefab
        currentPrefab.transform.position = targetPosition;

        // Opcional: hacer que el prefab mire hacia la cámara
        Vector3 directionToCamera = (xrCamera.transform.position - currentPrefab.transform.position).normalized;
        if (directionToCamera != Vector3.zero)
        {
            currentPrefab.transform.rotation = Quaternion.LookRotation(-directionToCamera);
        }

        Debug.Log($"Prefab '{currentPrefab.name}' posicionado frente a la cámara a {initialDistance} unidades de distancia");
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

    void HandleRotation()
    {
        if (!isRotating || prefabs.Count == 0 || prefabs[currentPrefabIndex] == null)
            return;

        GameObject currentPrefab = prefabs[currentPrefabIndex];
        currentPrefab.transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime, Space.World);
    }

    void ChangeToPreviousPrefab()
    {
        if (prefabs.Count <= 1 || isTransitioning)
            return;

        int previousIndex = currentPrefabIndex;
        currentPrefabIndex = (currentPrefabIndex - 1 + prefabs.Count) % prefabs.Count;

        StartCoroutine(ConveyorTransition(previousIndex, currentPrefabIndex, true));
    }

    void ChangeToNextPrefab()
    {
        if (prefabs.Count <= 1 || isTransitioning)
            return;

        int previousIndex = currentPrefabIndex;
        currentPrefabIndex = (currentPrefabIndex + 1) % prefabs.Count;

        StartCoroutine(ConveyorTransition(previousIndex, currentPrefabIndex, false));
    }

    IEnumerator ConveyorTransition(int fromIndex, int toIndex, bool movingLeft)
    {
        isTransitioning = true;

        // Detener rotación durante la transición
        bool wasRotating = isRotating;
        Vector3 previousRotationAxis = rotationAxis;
        StopRotation();

        GameObject fromPrefab = prefabs[fromIndex];
        GameObject toPrefab = prefabs[toIndex];

        // Posiciones para el efecto de banda transportadora
        Vector3 centerPosition = fromPrefab.transform.position;
        Vector3 offscreenDistance = Vector3.right * 5f; // Ajustar según necesidad

        Vector3 leftPosition = centerPosition - offscreenDistance;
        Vector3 rightPosition = centerPosition + offscreenDistance;

        // Configurar posiciones iniciales
        if (movingLeft)
        {
            toPrefab.transform.position = leftPosition;
        }
        else
        {
            toPrefab.transform.position = rightPosition;
        }

        // Activar el nuevo prefab
        toPrefab.SetActive(true);

        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = transitionCurve.Evaluate(elapsedTime / transitionDuration);

            if (movingLeft)
            {
                // Mover hacia la izquierda
                fromPrefab.transform.position = Vector3.Lerp(centerPosition, rightPosition, progress);
                toPrefab.transform.position = Vector3.Lerp(leftPosition, centerPosition, progress);
            }
            else
            {
                // Mover hacia la derecha
                fromPrefab.transform.position = Vector3.Lerp(centerPosition, leftPosition, progress);
                toPrefab.transform.position = Vector3.Lerp(rightPosition, centerPosition, progress);
            }

            yield return null;
        }

        // Finalizar transición
        fromPrefab.SetActive(false);
        fromPrefab.transform.position = centerPosition; // Resetear posición
        toPrefab.transform.position = centerPosition;

        // Restaurar rotación si estaba activa
        if (wasRotating)
        {
            StartRotation(previousRotationAxis);
        }

        isTransitioning = false;
    }

    void ZoomIn()
    {
        if (xrCamera == null || prefabs.Count == 0 || prefabs[currentPrefabIndex] == null)
            return;

        GameObject currentPrefab = prefabs[currentPrefabIndex];
        Vector3 directionToTarget = (currentPrefab.transform.position - xrCamera.transform.position).normalized;
        Vector3 newPosition = xrCamera.transform.position + directionToTarget * zoomSpeed * Time.deltaTime;

        float distanceToTarget = Vector3.Distance(newPosition, currentPrefab.transform.position);

        if (distanceToTarget > minDistance)
        {
            xrCamera.transform.position = newPosition;
            targetCameraPosition = newPosition;
        }
    }

    void ZoomOut()
    {
        if (xrCamera == null || prefabs.Count == 0 || prefabs[currentPrefabIndex] == null)
            return;

        GameObject currentPrefab = prefabs[currentPrefabIndex];
        Vector3 directionFromTarget = (xrCamera.transform.position - currentPrefab.transform.position).normalized;
        Vector3 newPosition = xrCamera.transform.position + directionFromTarget * zoomSpeed * Time.deltaTime;

        float distanceToTarget = Vector3.Distance(newPosition, currentPrefab.transform.position);

        if (distanceToTarget < maxDistance)
        {
            xrCamera.transform.position = newPosition;
            targetCameraPosition = newPosition;
        }
    }

    void OnDisable()
    {
        // Desabilitar todas las acciones de input al desactivar el componente
        upArrowAction?.Disable();
        downArrowAction?.Disable();
        leftArrowAction?.Disable();
        rightArrowAction?.Disable();
        spaceAction?.Disable();
        aKeyAction?.Disable();
        dKeyAction?.Disable();
        zKeyAction?.Disable();
        xKeyAction?.Disable();
    }

    void OnDestroy()
    {
        // Limpiar las acciones de input al destruir el objeto
        upArrowAction?.Dispose();
        downArrowAction?.Dispose();
        leftArrowAction?.Dispose();
        rightArrowAction?.Dispose();
        spaceAction?.Dispose();
        aKeyAction?.Dispose();
        dKeyAction?.Dispose();
        zKeyAction?.Dispose();
        xKeyAction?.Dispose();
    }

    // Métodos públicos para debugging o llamadas externas
    public void SetRotationSpeed(float newSpeed)
    {
        rotationSpeed = newSpeed;
    }

    public void SetZoomSpeed(float newSpeed)
    {
        zoomSpeed = newSpeed;
    }

    public int GetCurrentPrefabIndex()
    {
        return currentPrefabIndex;
    }

    public GameObject GetCurrentPrefab()
    {
        if (prefabs.Count > 0 && currentPrefabIndex >= 0 && currentPrefabIndex < prefabs.Count)
        {
            return prefabs[currentPrefabIndex];
        }
        return null;
    }

    // Método para añadir prefabs dinámicamente
    public void AddPrefab(GameObject newPrefab)
    {
        if (newPrefab != null)
        {
            prefabs.Add(newPrefab);
            newPrefab.SetActive(false);
        }
    }

    // Método público para reposicionar el prefab actual frente a la cámara
    public void RepositionCurrentPrefabInView()
    {
        PositionCurrentPrefabInView();
    }
}