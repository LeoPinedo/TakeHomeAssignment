using UnityEngine;
using System.Collections;

/// <summary>
/// Controla el flujo principal del juego: tiempo, intentos, estados y comunicación entre sistemas.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Configuración del Juego")]
    [Tooltip("Tiempo límite en segundos (0 = sin límite)")]
    [SerializeField] private float timeLimit = 120f;
    
    [Tooltip("Número máximo de intentos en juego guiado (0 = ilimitados)")]
    [SerializeField] private int maxAttempts = 3;
    
    [Tooltip("Número máximo de intentos en simulación (sin ayuda)")]
    [SerializeField] private int simulationMaxAttempts = 6;
    
    [Header("Referencias")]
    [Tooltip("Referencia al SequenceManager")]
    [SerializeField] private SequenceManager sequenceManager;
    
    [Tooltip("Referencia al FeedbackManager")]
    [SerializeField] private FeedbackManager feedbackManager;
    
    [Tooltip("Referencia al UIManager")]
    [SerializeField] private UIManager uiManager;
    
    [Tooltip("Referencia al TutorialManager")]
    [SerializeField] private TutorialManager tutorialManager;
    
    [Header("Estado del Juego")]
    [SerializeField] private bool isGameActive = false;
    [SerializeField] private bool isTutorialActive = false;
    [SerializeField] private bool isGuidedGameMode = false; // Juego con ayuda (muestra highlights)
    [SerializeField] private bool isSimulationMode = false; // Simulación sin ayuda
    
    // Variables de juego
    private float currentTime = 0f;
    private int currentAttempts = 0;
    private bool isGameCompleted = false;
    private bool isGameOver = false;
    
    // Eventos
    public System.Action OnGameStart;
    public System.Action OnGameComplete;
    public System.Action OnGameOver;
    public System.Action OnAttemptFailed;
    
    // Singleton pattern (opcional, para fácil acceso)
    public static GameManager Instance { get; private set; }
    
    // Propiedades públicas
    public float CurrentTime => currentTime;
    public int CurrentAttempts => currentAttempts;
    public bool IsGameActive => isGameActive;
    public bool IsGameCompleted => isGameCompleted;
    public bool IsGuidedGameMode => isGuidedGameMode;
    public bool IsSimulationMode => isSimulationMode;
    public float TimeRemaining => timeLimit > 0 ? Mathf.Max(0, timeLimit - currentTime) : float.MaxValue;
    
    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        // Buscar referencias si no están asignadas
        if (sequenceManager == null)
            sequenceManager = FindObjectOfType<SequenceManager>();
        
        if (feedbackManager == null)
            feedbackManager = FindObjectOfType<FeedbackManager>();
        
        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();
        
        if (tutorialManager == null)
            tutorialManager = FindObjectOfType<TutorialManager>();
        
        // Suscribirse a eventos del SequenceManager
        if (sequenceManager != null)
        {
            sequenceManager.OnCorrectNutSelected += HandleCorrectNutSelected;
            sequenceManager.OnWrongNutSelected += HandleWrongNutSelected;
            sequenceManager.OnSequenceCompleted += HandleSequenceCompleted;
        }
        
        // Iniciar tutorial
        StartTutorial();
    }
    
    private void Update()
    {
        if (isGameActive && !isGameCompleted && !isGameOver)
        {
            // Actualizar tiempo
            currentTime += Time.deltaTime;
            
            // Verificar tiempo límite
            if (timeLimit > 0 && currentTime >= timeLimit)
            {
                HandleTimeOut();
            }
            
            // Actualizar UI
            if (uiManager != null)
            {
                uiManager.UpdateTime(currentTime, timeLimit);
            }
        }
    }
    
    /// <summary>
    /// Inicia el tutorial.
    /// </summary>
    public void StartTutorial()
    {
        isTutorialActive = true;
        isGameActive = false;
        
        // Guardar posiciones iniciales de todos los objetos antes del tutorial
        if (feedbackManager != null)
        {
            feedbackManager.SaveInitialPositions();
        }
        
        if (tutorialManager != null)
        {
            tutorialManager.StartTutorial();
        }
        
        if (uiManager != null)
        {
            uiManager.ShowTutorialPanel(true);
        }
    }
    
    /// <summary>
    /// Finaliza el tutorial e inicia el juego real con las 8 tuercas.
    /// </summary>
    public void EndTutorial()
    {
        isTutorialActive = false;
        isGameActive = true;
        
        if (tutorialManager != null)
        {
            tutorialManager.EndTutorial();
        }
        
        // Activar todas las tuercas (mostrar las que estaban ocultas en el tutorial)
        // Usar Resources.FindObjectsOfTypeAll para encontrar objetos desactivados
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        
        // Activar todos los Bolt_X (padres) que están en la escena actual
        for (int i = 1; i <= 8; i++)
        {
            string boltName = $"Bolt_{i}";
            foreach (GameObject obj in allObjects)
            {
                // Verificar que sea el objeto correcto, esté en la escena y no sea un prefab
                if (obj != null && 
                    obj.name == boltName && 
                    obj.scene.isLoaded && 
                    !obj.name.Contains("(Clone)") && // No es una instancia de prefab duplicada
                    obj.transform.parent == null) // Es un objeto raíz
                {
                    obj.SetActive(true);
                    Debug.Log($"✅ Activado: {obj.name}");
                    break; // Encontrado, pasar al siguiente
                }
            }
        }
        
        // Verificar que se activaron correctamente
        int activatedCount = 0;
        for (int i = 1; i <= 8; i++)
        {
            GameObject bolt = GameObject.Find($"Bolt_{i}");
            if (bolt != null && bolt.activeInHierarchy)
            {
                activatedCount++;
            }
        }
        Debug.Log($"Total de Bolt_X activados: {activatedCount}/8");
        
        // Resetear secuencia para el juego guiado (todas las 8 tuercas con ayuda)
        if (sequenceManager != null)
        {
            sequenceManager.RemoveTutorialLimit(); // Quitar límite de tutorial y restaurar secuencia original
            sequenceManager.ResetSequence(); // Asegurar que el paso actual esté en 0
            Debug.Log("✅ Secuencia restaurada y límite de tutorial removido");
        }
        
        // Esperar un frame para que los objetos se activen antes de buscar las tuercas
        StartCoroutine(ResetAllNutsAfterActivation());
        
        // Activar modo guiado (con ayuda visual)
        isGuidedGameMode = true;
        isSimulationMode = false;
        
        if (uiManager != null)
        {
            uiManager.ShowTutorialPanel(false);
            uiManager.ShowFeedback("¡Juego Guiado! Observa el orden correcto. Ajusta las 8 tuercas.", Color.cyan);
        }
        
        // Iniciar highlights para mostrar el orden correcto (después de resetear las tuercas)
        StartCoroutine(StartGuidedGameAfterReset());
        
        OnGameStart?.Invoke();
    }
    
    /// <summary>
    /// Inicia los highlights del juego guiado después de resetear las tuercas.
    /// </summary>
    private System.Collections.IEnumerator StartGuidedGameAfterReset()
    {
        yield return new WaitForSeconds(1f); // Esperar a que se reseteen las tuercas
        
        if (tutorialManager != null)
        {
            tutorialManager.StartGuidedGameHighlights();
        }
    }
    
    /// <summary>
    /// Maneja cuando se selecciona una tuerca correcta.
    /// </summary>
    private void HandleCorrectNutSelected(int nutID)
    {
        // Reproducir sonido de acierto
        if (feedbackManager != null)
        {
            feedbackManager.PlayCorrectSound();
        }
        
        // Si estamos en tutorial y se completaron las 3 tuercas, pasar al juego real
        if (isTutorialActive && sequenceManager != null)
        {
            int currentStep = sequenceManager.GetCurrentStep();
            if (currentStep >= 3) // Tutorial completado (3 tuercas)
            {
                // Esperar un momento y luego pasar al juego real
                StartCoroutine(TransitionToRealGame());
                return;
            }
        }
        
        // Actualizar UI
        if (uiManager != null && sequenceManager != null)
        {
            uiManager.UpdateProgress(sequenceManager.GetProgress());
            
            if (isTutorialActive)
            {
                uiManager.ShowFeedback($"¡Correcto! ({sequenceManager.GetCurrentStep()}/3 del tutorial)", Color.green);
            }
            else
            {
                uiManager.ShowFeedback("¡Correcto!", Color.green);
            }
        }
    }
    
    /// <summary>
    /// Transición del tutorial al juego real.
    /// </summary>
    private System.Collections.IEnumerator TransitionToRealGame()
    {
        yield return new WaitForSeconds(1.5f);
        
        if (uiManager != null)
        {
            uiManager.ShowFeedback("¡Tutorial completado! Iniciando juego real...", Color.yellow);
        }
        
        // Resetear posiciones de las tuercas y pernos del tutorial
        if (feedbackManager != null)
        {
            feedbackManager.ResetAllPositions();
        }
        
        yield return new WaitForSeconds(1f);
        
        // Finalizar tutorial e iniciar juego real
        EndTutorial();
    }
    
    /// <summary>
    /// Maneja cuando se selecciona una tuerca incorrecta.
    /// </summary>
    private void HandleWrongNutSelected(int nutID)
    {
        // Solo contar intentos si el juego está activo (no en tutorial)
        if (!isGameActive || isTutorialActive)
        {
            // En tutorial, solo mostrar feedback pero no contar intentos
            if (uiManager != null)
            {
                uiManager.ShowFeedback("Incorrecto. Intenta de nuevo.", Color.red);
            }
            return;
        }
        
        // Determinar el máximo de intentos según el modo
        int currentMaxAttempts = isSimulationMode ? simulationMaxAttempts : maxAttempts;
        
        // Incrementar intentos
        currentAttempts++;
        
        Debug.Log($"❌ Tuerca incorrecta seleccionada. Intentos: {currentAttempts}/{currentMaxAttempts} (Modo: {(isSimulationMode ? "Simulación" : "Guiado")})");
        
        // Reproducir sonido de error
        if (feedbackManager != null)
        {
            feedbackManager.PlayWrongSound();
        }
        
        // Actualizar UI
        if (uiManager != null)
        {
            uiManager.UpdateAttempts(currentAttempts, currentMaxAttempts);
            uiManager.ShowFeedback($"Incorrecto. Intentos restantes: {currentMaxAttempts - currentAttempts}", Color.red);
        }
        
        // Verificar si se agotaron los intentos
        if (currentMaxAttempts > 0 && currentAttempts >= currentMaxAttempts)
        {
            Debug.Log($"💀 Game Over: Se agotaron los intentos (Modo: {(isSimulationMode ? "Simulación" : "Guiado")})");
            HandleGameOver();
        }
        else
        {
            OnAttemptFailed?.Invoke();
        }
    }
    
    /// <summary>
    /// Maneja cuando se completa la secuencia.
    /// </summary>
    private void HandleSequenceCompleted()
    {
        // Si estamos en juego guiado (con ayuda), pasar a simulación
        if (isGuidedGameMode && !isSimulationMode)
        {
            StartCoroutine(TransitionToSimulation());
            return;
        }
        
        // Si estamos en simulación, el juego está completamente terminado
        if (isSimulationMode)
        {
            isGameCompleted = true;
            isGameActive = false;
            
            if (uiManager != null)
            {
                uiManager.ShowGameCompletePanel(true, currentTime, currentAttempts);
            }
            
            if (feedbackManager != null)
            {
                feedbackManager.PlayCorrectSound();
            }
            
            OnGameComplete?.Invoke();
        }
        else
        {
            // Completado en modo guiado
            isGameCompleted = true;
            isGameActive = false;
            
            if (uiManager != null)
            {
                uiManager.ShowGameCompletePanel(true, currentTime, currentAttempts);
            }
            
            if (feedbackManager != null)
            {
                feedbackManager.PlayCorrectSound();
            }
            
            OnGameComplete?.Invoke();
        }
    }
    
    /// <summary>
    /// Transición del juego guiado a la simulación (sin ayuda).
    /// </summary>
    private System.Collections.IEnumerator TransitionToSimulation()
    {
        yield return new WaitForSeconds(1.5f);
        
        if (uiManager != null)
        {
            uiManager.ShowFeedback("¡Juego completado! Iniciando simulación (sin ayuda)...", Color.cyan);
        }
        
        // Resetear posiciones de todas las tuercas
        if (feedbackManager != null)
        {
            feedbackManager.ResetAllPositions();
        }
        
        // Resetear todas las tuercas
        Nut[] nuts = FindObjectsOfType<Nut>(true);
        foreach (Nut nut in nuts)
        {
            if (nut != null)
            {
                nut.ResetNut();
            }
        }
        
        yield return new WaitForSeconds(1f);
        
        // Iniciar simulación
        StartSimulation();
    }
    
    /// <summary>
    /// Inicia la simulación sin ayuda (sin highlights, solo 6 intentos).
    /// </summary>
    public void StartSimulation()
    {
        Debug.Log("🎮 Iniciando Simulación (sin ayuda)");
        
        // Detener highlights si están activos
        if (tutorialManager != null)
        {
            tutorialManager.StopGuidedGameHighlights();
        }
        
        // Resetear variables
        currentTime = 0f;
        currentAttempts = 0;
        isGameCompleted = false;
        isGameOver = false;
        isTutorialActive = false;
        isGuidedGameMode = false;
        isSimulationMode = true;
        isGameActive = true;
        
        // Resetear secuencia
        if (sequenceManager != null)
        {
            sequenceManager.ResetSequence();
            sequenceManager.RemoveTutorialLimit();
            Debug.Log("✅ Secuencia reseteada para simulación");
        }
        
        // Resetear UI
        if (uiManager != null)
        {
            uiManager.ResetUI();
            uiManager.ShowFeedback("Simulación: Sin ayuda visual. 6 intentos disponibles.", Color.yellow);
        }
        
        OnGameStart?.Invoke();
    }
    
    /// <summary>
    /// Maneja cuando se agota el tiempo.
    /// </summary>
    private void HandleTimeOut()
    {
        isGameOver = true;
        isGameActive = false;
        
        if (uiManager != null)
        {
            uiManager.ShowFeedback("¡Tiempo agotado!", Color.red);
        }
        
        HandleGameOver();
    }
    
    /// <summary>
    /// Maneja el fin del juego (derrota).
    /// </summary>
    private void HandleGameOver()
    {
        isGameOver = true;
        isGameActive = false;
        
        if (uiManager != null)
        {
            uiManager.ShowGameOverPanel(true);
        }
        
        OnGameOver?.Invoke();
    }
    
    /// <summary>
    /// Reinicia el juego.
    /// </summary>
    public void RestartGame()
    {
        Debug.Log("🔄 Reiniciando juego...");
        
        // Detener cualquier corrutina activa
        StopAllCoroutines();
        
        // Resetear variables
        currentTime = 0f;
        currentAttempts = 0;
        isGameCompleted = false;
        isGameOver = false;
        isGameActive = false;
        isTutorialActive = false;
        
        // Ocultar todos los paneles
        if (uiManager != null)
        {
            uiManager.ShowGameCompletePanel(false, 0f, 0);
            uiManager.ShowGameOverPanel(false);
            uiManager.ShowTighteningProgress(Vector3.zero, 0f, false);
        }
        
        // Resetear secuencia
        if (sequenceManager != null)
        {
            sequenceManager.ResetSequence();
            Debug.Log("✅ Secuencia reseteada");
        }
        else
        {
            Debug.LogError("❌ SequenceManager no encontrado!");
        }
        
        // Resetear posiciones de todos los objetos
        if (feedbackManager != null)
        {
            feedbackManager.ResetAllPositions();
            Debug.Log("✅ Posiciones reseteadas");
        }
        else
        {
            Debug.LogError("❌ FeedbackManager no encontrado!");
        }
        
        // Resetear todas las tuercas
        Nut[] nuts = FindObjectsOfType<Nut>(true); // Incluir inactivos
        int resetCount = 0;
        foreach (Nut nut in nuts)
        {
            if (nut != null)
            {
                nut.ResetNut();
                resetCount++;
            }
        }
        Debug.Log($"✅ {resetCount} tuercas reseteadas");
        
        // Resetear UI
        if (uiManager != null)
        {
            uiManager.ResetUI();
            Debug.Log("✅ UI reseteada");
        }
        else
        {
            Debug.LogError("❌ UIManager no encontrado!");
        }
        
        // Reiniciar tutorial
        StartTutorial();
        
        Debug.Log("✅ Juego reiniciado completamente");
    }
    
    /// <summary>
    /// Pausa/reanuda el juego.
    /// </summary>
    public void SetPause(bool pause)
    {
        Time.timeScale = pause ? 0f : 1f;
    }
    
    /// <summary>
    /// Corrutina para resetear todas las tuercas después de activar los GameObjects.
    /// </summary>
    private System.Collections.IEnumerator ResetAllNutsAfterActivation()
    {
        // Esperar un frame para que los objetos se activen
        yield return null;
        
        // Ahora buscar todas las tuercas (ya están activas)
        Nut[] allNuts = FindObjectsOfType<Nut>();
        foreach (Nut nut in allNuts)
        {
            if (nut != null)
            {
                nut.ResetNut();
            }
        }
    }
    
    private void OnDestroy()
    {
        // Desuscribirse de eventos
        if (sequenceManager != null)
        {
            sequenceManager.OnCorrectNutSelected -= HandleCorrectNutSelected;
            sequenceManager.OnWrongNutSelected -= HandleWrongNutSelected;
            sequenceManager.OnSequenceCompleted -= HandleSequenceCompleted;
        }
    }
}

