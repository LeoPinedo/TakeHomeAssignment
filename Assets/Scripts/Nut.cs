using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Representa una tuerca individual. Detecta interacción con raycast y maneja sus estados.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Nut : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("ID de esta tuerca (1-8)")]
    [SerializeField] private int nutID = 1;
    
    [Header("Referencias")]
    [Tooltip("Renderer de la tuerca para cambiar materiales")]
    [SerializeField] private Renderer nutRenderer;
    
    [Tooltip("Transform de la tuerca para animaciones")]
    [SerializeField] private Transform nutTransform;
    
    private Material originalMaterial;
    private Material[] originalMaterials;
    private bool isTightened = false;
    private bool isHighlighted = false;
    private bool isBeingPressed = false;
    private bool hasValidated = false; // Evitar validación múltiple
    private float pressStartTime = 0f;
    private float requiredPressDuration = 1f; // 1 segundo requerido
    
    // Referencias a managers
    private SequenceManager sequenceManager;
    private FeedbackManager feedbackManager;
    private UIManager uiManager;
    private Camera mainCamera;
    
    // Referencia al perno (Bolt_Screw) que es el hermano
    // Nota: Nut = tuerca (donde está este script), Screw = perno (el hermano)
    private Transform boltNutTransform; // En realidad es el screw (perno), mantener nombre por compatibilidad
    
    // Estados
    public enum NutState
    {
        Idle,           // Estado normal
        Highlighted,    // Resaltada (tutorial)
        Selected,       // Seleccionada (click)
        Correct,        // Ajustada correctamente
        Wrong           // Selección incorrecta
    }
    
    private NutState currentState = NutState.Idle;
    
    public int NutID => nutID;
    public bool IsTightened => isTightened;
    public NutState CurrentState => currentState;
    
    private void Awake()
    {
        // Buscar componentes si no están asignados
        if (nutRenderer == null)
            nutRenderer = GetComponent<Renderer>();
        
        if (nutTransform == null)
            nutTransform = transform;
        
        // Guardar materiales originales
        if (nutRenderer != null)
        {
            originalMaterials = nutRenderer.materials;
            if (originalMaterials.Length > 0)
                originalMaterial = originalMaterials[0];
        }
        
        mainCamera = Camera.main;
    }
    
    private void Start()
    {
        // Buscar managers en la escena
        sequenceManager = FindObjectOfType<SequenceManager>();
        feedbackManager = FindObjectOfType<FeedbackManager>();
        uiManager = FindObjectOfType<UIManager>();
        
        if (sequenceManager == null)
            Debug.LogError($"SequenceManager no encontrado en la escena para Nut {nutID}");
        
        if (feedbackManager == null)
            Debug.LogWarning($"FeedbackManager no encontrado en la escena para Nut {nutID}");
        
        if (uiManager == null)
            Debug.LogWarning($"UIManager no encontrado en la escena para Nut {nutID}");
        
        // Buscar el perno (Bolt_Screw) que es el hermano
        // Este script está en Bolt_Nut (tuerca), y Bolt_Screw (perno) es el hermano
        if (transform.parent != null)
        {
            // Buscar Bolt_Screw en los hermanos del padre
            foreach (Transform sibling in transform.parent)
            {
                // Buscar el que se llama Bolt_Screw (el perno)
                if (sibling != transform && sibling.name.Contains("Bolt_Screw"))
                {
                    boltNutTransform = sibling; // Renombrar variable después
                    Debug.Log($"Nut {nutID}: Encontrado Bolt_Screw (perno) hermano: {sibling.name}");
                    break;
                }
            }
            
            if (boltNutTransform == null)
            {
                Debug.LogWarning($"Nut {nutID}: No se encontró Bolt_Screw (perno) hermano. Buscando alternativas...");
                // Intentar buscar cualquier hermano que no sea este
                foreach (Transform sibling in transform.parent)
                {
                    if (sibling != transform)
                    {
                        boltNutTransform = sibling;
                        Debug.Log($"Nut {nutID}: Usando hermano alternativo: {sibling.name}");
                        break;
                    }
                }
            }
        }
    }
    
    private void Update()
    {
        if (isTightened) return;
        
        // Detectar cuando se presiona el click
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleMouseClickStart();
        }
        
        // Detectar cuando se mantiene presionado
        if (isBeingPressed && Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            // Verificar que el raycast siga apuntando a esta tuerca
            if (!IsMouseOverNut())
            {
                // El mouse ya no está sobre esta tuerca, cancelar
                CancelTightening();
                return;
            }
            
            // Calcular progreso de carga (0 a 1)
            float elapsedTime = Time.time - pressStartTime;
            float progress = Mathf.Clamp01(elapsedTime / requiredPressDuration);
            
            // Actualizar animación gradual (perno baja y tuerca gira)
            if (feedbackManager != null)
            {
                feedbackManager.UpdateTighteningProgress(this, progress);
            }
            
            // Mostrar barra de carga
            if (uiManager != null)
            {
                uiManager.ShowTighteningProgress(transform.position, progress, true);
            }
            
            // Si se completó el tiempo requerido, validar (pero seguir manteniendo presionado para completar animación)
            if (elapsedTime >= requiredPressDuration && !isTightened && currentState != NutState.Correct && !hasValidated)
            {
                // Validar solo una vez cuando se alcanza el tiempo
                ValidateAndExecute();
                hasValidated = true; // Marcar que ya se validó
            }
        }
        
        // Detectar cuando se suelta el click
        if (isBeingPressed && Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            float elapsedTime = Time.time - pressStartTime;
            
            // Ocultar barra de carga
            if (uiManager != null)
            {
                uiManager.ShowTighteningProgress(transform.position, 0f, false);
            }
            
            // Si no se completó el tiempo requerido, cancelar animación
            if (elapsedTime < requiredPressDuration)
            {
                // Cancelar animación y restaurar posiciones
                if (feedbackManager != null)
                {
                    feedbackManager.CancelTighteningProgress(this);
                }
                isBeingPressed = false;
                hasValidated = false;
                SetState(NutState.Idle);
                return;
            }
            
            // Si se completó el tiempo pero aún no se validó, validar ahora
            if (!isTightened && currentState != NutState.Correct && !hasValidated)
            {
                ValidateAndExecute();
                hasValidated = true;
            }
            
            // Si ya se validó como correcto, completar la animación al 100%
            if (currentState == NutState.Correct && !isTightened)
            {
                // Completar la animación gradual al 100%
                if (feedbackManager != null)
                {
                    feedbackManager.CompleteTighteningAnimation(this);
                }
                
                isBeingPressed = false;
                hasValidated = false;
                TightenNut();
            }
            else if (currentState != NutState.Correct)
            {
                // Si no es correcto, asegurarse de que isBeingPressed se resetee
                isBeingPressed = false;
                hasValidated = false;
            }
        }
    }
    
    /// <summary>
    /// Verifica si el mouse está sobre esta tuerca usando raycast.
    /// </summary>
    private bool IsMouseOverNut()
    {
        if (mainCamera == null || Mouse.current == null) return false;
        
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        
        float maxDistance = 100f;
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            Transform hitTransform = hit.collider.transform;
            return hitTransform == transform || hitTransform.IsChildOf(transform);
        }
        
        return false;
    }
    
    /// <summary>
    /// Cancela el proceso de ajuste.
    /// </summary>
    private void CancelTightening()
    {
        isBeingPressed = false;
        
        // Cancelar animación gradual y restaurar posiciones
        if (feedbackManager != null)
        {
            feedbackManager.CancelTighteningProgress(this);
        }
        
        if (uiManager != null)
        {
            uiManager.ShowTighteningProgress(transform.position, 0f, false);
        }
        SetState(NutState.Idle);
    }
    
    /// <summary>
    /// Valida y ejecuta la animación cuando se completa el tiempo de carga.
    /// </summary>
    private void ValidateAndExecute()
    {
        // Evitar validar múltiples veces
        if (isTightened || currentState == NutState.Correct) return;
        
        // Validar con SequenceManager
        if (sequenceManager != null)
        {
            bool isCorrect = sequenceManager.ValidateNutSelection(nutID);
            
            Debug.Log($"🔍 Validando tuerca {nutID}: {(isCorrect ? "✅ Correcta" : "❌ Incorrecta")}");
            
            if (isCorrect)
            {
                // La animación gradual continuará actualizándose hasta que se suelte el botón
                // Cuando se suelte, se completará al 100%
                SetState(NutState.Correct);
                Debug.Log($"✅ Tuerca {nutID} validada como correcta. Esperando que se suelte el botón para completar animación.");
                // NO establecer isBeingPressed = false todavía, para que la animación continúe
                // NO llamar TightenNut() todavía, se llamará cuando se suelte el botón
            }
            else
            {
                // Cancelar animación y restaurar posiciones
                if (feedbackManager != null)
                {
                    feedbackManager.CancelTighteningProgress(this);
                }
                
                isBeingPressed = false;
                SetState(NutState.Wrong);
                Debug.Log($"❌ Tuerca {nutID} incorrecta. Animación cancelada.");
                // NO iniciar animación si es incorrecto - solo mostrar feedback de error
            }
        }
    }
    
    /// <summary>
    /// Maneja cuando se inicia el click del mouse usando raycast.
    /// </summary>
    private void HandleMouseClickStart()
    {
        if (mainCamera == null || Mouse.current == null) return;
        
        // Usar nuevo Input System para obtener posición del mouse
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        
        // Raycast para detectar si se clickeó esta tuerca
        float maxDistance = 100f;
        
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            // Verificar si el hit es esta tuerca o alguno de sus hijos
            Transform hitTransform = hit.collider.transform;
            
            // Verificar si es esta tuerca directamente
            if (hitTransform == transform || hitTransform.IsChildOf(transform))
            {
                OnNutClickStart();
                return;
            }
            
            // También verificar si el padre es esta tuerca
            if (hitTransform.parent != null && 
                (hitTransform.parent == transform || hitTransform.parent.IsChildOf(transform)))
            {
                OnNutClickStart();
            }
        }
    }
    
    /// <summary>
    /// Maneja cuando se suelta el click del mouse (ya no se usa, se maneja en Update).
    /// </summary>
    private void HandleMouseClickRelease()
    {
        // Este método ya no se usa, la lógica está en Update()
    }
    
    /// <summary>
    /// Se llama cuando se inicia el click en la tuerca (Bolt_Screw).
    /// </summary>
    private void OnNutClickStart()
    {
        if (isTightened || isBeingPressed) return;
        
        isBeingPressed = true;
        hasValidated = false; // Resetear flag de validación
        pressStartTime = Time.time; // Guardar tiempo de inicio
        SetState(NutState.Selected);
        
        // Iniciar animación gradual (perno baja y tuerca gira durante la carga)
        if (feedbackManager != null)
        {
            feedbackManager.StartTighteningProgress(this, boltNutTransform);
        }
        
        // Mostrar barra de carga inicial
        if (uiManager != null)
        {
            uiManager.ShowTighteningProgress(transform.position, 0f, true);
        }
    }
    
    /// <summary>
    /// Ajusta la tuerca (cambio de material - la animación ya se completó).
    /// </summary>
    private void TightenNut()
    {
        isTightened = true;
        isBeingPressed = false;
        
        // Cambiar material (la animación ya se completó en StartTighteningAnimation)
        if (feedbackManager != null)
        {
            feedbackManager.ChangeNutMaterial(this, true);
        }
    }
    
    /// <summary>
    /// Establece el estado visual de la tuerca.
    /// </summary>
    public void SetState(NutState newState)
    {
        if (currentState == newState) return;
        
        currentState = newState;
        
        // Aplicar feedback visual según el estado
        if (feedbackManager != null)
        {
            feedbackManager.ApplyNutStateVisual(this, newState);
        }
    }
    
    /// <summary>
    /// Resalta la tuerca (para tutorial).
    /// </summary>
    public void Highlight(bool highlight)
    {
        isHighlighted = highlight;
        
        if (highlight)
        {
            SetState(NutState.Highlighted);
        }
        else if (!isTightened)
        {
            SetState(NutState.Idle);
        }
    }
    
    /// <summary>
    /// Obtiene el renderer de la tuerca.
    /// </summary>
    public Renderer GetRenderer()
    {
        return nutRenderer;
    }
    
    /// <summary>
    /// Obtiene el transform de la tuerca.
    /// </summary>
    public Transform GetTransform()
    {
        return nutTransform;
    }
    
    /// <summary>
    /// Obtiene el material original.
    /// </summary>
    public Material GetOriginalMaterial()
    {
        return originalMaterial;
    }
    
    /// <summary>
    /// Resetea la tuerca a su estado inicial.
    /// </summary>
    public void ResetNut()
    {
        isTightened = false;
        isHighlighted = false;
        SetState(NutState.Idle);
        
        // Restaurar material original
        if (nutRenderer != null && originalMaterials != null)
        {
            nutRenderer.materials = originalMaterials;
        }
    }
    
    // Método para debug en el editor
    private void OnValidate()
    {
        if (nutID < 1 || nutID > 8)
        {
            Debug.LogWarning($"Nut ID debe estar entre 1 y 8. Actual: {nutID}");
        }
    }
}

