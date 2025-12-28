using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Gestiona toda la interfaz de usuario: progreso, tiempo, intentos, mensajes y paneles.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Paneles")]
    [Tooltip("Panel de tutorial")]
    [SerializeField] private GameObject tutorialPanel;
    
    [Tooltip("Panel de juego completado")]
    [SerializeField] private GameObject gameCompletePanel;
    
    [Tooltip("Panel de juego perdido")]
    [SerializeField] private GameObject gameOverPanel;
    
    [Header("Texto de Progreso")]
    [Tooltip("Texto que muestra el paso actual (ej: 'Paso 3/8')")]
    [SerializeField] private TextMeshProUGUI progressText;
    
    [Tooltip("Barra de progreso")]
    [SerializeField] private Slider progressBar;
    
    [Header("Texto de Tiempo")]
    [Tooltip("Texto que muestra el tiempo")]
    [SerializeField] private TextMeshProUGUI timeText;
    
    [Header("Texto de Intentos")]
    [Tooltip("Texto que muestra los intentos")]
    [SerializeField] private TextMeshProUGUI attemptsText;
    
    [Header("Feedback")]
    [Tooltip("Texto de feedback (mensajes temporales)")]
    [SerializeField] private TextMeshProUGUI feedbackText;
    
    [Tooltip("Duración del mensaje de feedback")]
    [SerializeField] private float feedbackDuration = 2f;
    
    [Header("Barra de Carga (Ajuste)")]
    [Tooltip("Panel de barra de carga (aparece cuando se mantiene presionado)")]
    [SerializeField] private GameObject tighteningProgressPanel;
    
    [Tooltip("Barra de progreso de ajuste")]
    [SerializeField] private Slider tighteningProgressBar;
    
    [Tooltip("Texto de la barra de carga (opcional)")]
    [SerializeField] private TextMeshProUGUI tighteningProgressText;
    
    [Tooltip("Canvas para UI world space (si quieres mostrar la barra cerca de la tuerca)")]
    [SerializeField] private Canvas worldSpaceCanvas;
    
    private Camera mainCamera;
    
    [Header("Panel de Completado")]
    [Tooltip("Texto de tiempo final")]
    [SerializeField] private TextMeshProUGUI finalTimeText;
    
    [Tooltip("Texto de intentos finales")]
    [SerializeField] private TextMeshProUGUI finalAttemptsText;
    
    [Header("Botones")]
    [Tooltip("Botón para reiniciar")]
    [SerializeField] private Button restartButton;
    
    [Tooltip("Botón para salir del tutorial")]
    [SerializeField] private Button tutorialSkipButton;
    
    private SequenceManager sequenceManager;
    private GameManager gameManager;
    private Coroutine feedbackCoroutine;
    
    private void Awake()
    {
        // Buscar referencias
        sequenceManager = FindObjectOfType<SequenceManager>();
        gameManager = FindObjectOfType<GameManager>();
        mainCamera = Camera.main;
        
        // Verificar que GameManager esté encontrado
        if (gameManager == null)
        {
            Debug.LogError("❌ UIManager: GameManager no encontrado en la escena!");
        }
        
        // Configurar botones
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
            Debug.Log("✅ Botón de reiniciar configurado correctamente");
        }
        else
        {
            Debug.LogWarning("⚠️ UIManager: RestartButton no asignado en el Inspector!");
        }
        
        if (tutorialSkipButton != null)
        {
            tutorialSkipButton.onClick.AddListener(OnTutorialSkipClicked);
        }
        
        // Ocultar barra de carga inicialmente
        if (tighteningProgressPanel != null)
        {
            tighteningProgressPanel.SetActive(false);
        }
    }
    
    private void Start()
    {
        // Inicializar UI
        ResetUI();
    }
    
    private void Update()
    {
        // Actualizar progreso continuamente
        if (sequenceManager != null)
        {
            UpdateProgress(sequenceManager.GetProgress());
            
            int currentStep = sequenceManager.GetCurrentStep();
            int totalSteps = sequenceManager.GetTotalSteps();
            
            if (progressText != null)
            {
                progressText.text = $"Paso: {currentStep}/{totalSteps}";
            }
        }
    }
    
    /// <summary>
    /// Actualiza la barra de progreso.
    /// </summary>
    public void UpdateProgress(float progress)
    {
        if (progressBar != null)
        {
            progressBar.value = progress;
        }
    }
    
    /// <summary>
    /// Actualiza el texto de tiempo.
    /// </summary>
    public void UpdateTime(float currentTime, float timeLimit)
    {
        if (timeText != null)
        {
            if (timeLimit > 0)
            {
                float remaining = timeLimit - currentTime;
                int minutes = Mathf.FloorToInt(remaining / 60);
                int seconds = Mathf.FloorToInt(remaining % 60);
                timeText.text = $"Tiempo: {minutes:00}:{seconds:00}";
                
                // Cambiar color si queda poco tiempo
                if (remaining < 30f)
                {
                    timeText.color = Color.red;
                }
                else
                {
                    timeText.color = Color.white;
                }
            }
            else
            {
                int minutes = Mathf.FloorToInt(currentTime / 60);
                int seconds = Mathf.FloorToInt(currentTime % 60);
                timeText.text = $"Tiempo: {minutes:00}:{seconds:00}";
            }
        }
    }
    
    /// <summary>
    /// Actualiza el texto de intentos.
    /// </summary>
    public void UpdateAttempts(int currentAttempts, int maxAttempts)
    {
        if (attemptsText != null)
        {
            if (maxAttempts > 0)
            {
                attemptsText.text = $"Intentos: {currentAttempts}/{maxAttempts}";
                
                // Cambiar color si quedan pocos intentos
                if (currentAttempts >= maxAttempts - 1)
                {
                    attemptsText.color = Color.red;
                }
                else
                {
                    attemptsText.color = Color.white;
                }
            }
            else
            {
                attemptsText.text = $"Intentos: {currentAttempts}";
            }
        }
    }
    
    /// <summary>
    /// Muestra un mensaje de feedback temporal.
    /// </summary>
    public void ShowFeedback(string message, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;
            feedbackText.gameObject.SetActive(true);
            
            // Detener corrutina anterior si existe
            if (feedbackCoroutine != null)
            {
                StopCoroutine(feedbackCoroutine);
            }
            
            feedbackCoroutine = StartCoroutine(HideFeedbackAfterDelay());
        }
    }
    
    /// <summary>
    /// Oculta el feedback después de un tiempo.
    /// </summary>
    private IEnumerator HideFeedbackAfterDelay()
    {
        yield return new WaitForSeconds(feedbackDuration);
        
        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
        }
        
        feedbackCoroutine = null;
    }
    
    /// <summary>
    /// Muestra/oculta el panel de tutorial.
    /// </summary>
    public void ShowTutorialPanel(bool show)
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(show);
        }
    }
    
    /// <summary>
    /// Muestra/oculta el panel de juego completado.
    /// </summary>
    public void ShowGameCompletePanel(bool show, float finalTime, int finalAttempts)
    {
        if (gameCompletePanel != null)
        {
            gameCompletePanel.SetActive(show);
            
            if (show)
            {
                // Actualizar información final
                if (finalTimeText != null)
                {
                    int minutes = Mathf.FloorToInt(finalTime / 60);
                    int seconds = Mathf.FloorToInt(finalTime % 60);
                    finalTimeText.text = $"Tiempo: {minutes:00}:{seconds:00}";
                }
                
                if (finalAttemptsText != null)
                {
                    finalAttemptsText.text = $"Intentos: {finalAttempts}";
                }
                
                // Buscar y configurar el botón de reiniciar dentro del panel
                Button completeRestartButton = gameCompletePanel.GetComponentInChildren<Button>();
                if (completeRestartButton != null)
                {
                    // Remover listeners anteriores para evitar duplicados
                    completeRestartButton.onClick.RemoveAllListeners();
                    // Agregar el listener
                    completeRestartButton.onClick.AddListener(OnRestartClicked);
                    Debug.Log("✅ Botón de reiniciar del GameCompletePanel configurado");
                }
            }
        }
    }
    
    /// <summary>
    /// Muestra/oculta el panel de juego perdido.
    /// </summary>
    public void ShowGameOverPanel(bool show)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(show);
            
            // Si se está mostrando, buscar y configurar el botón de reiniciar dentro del panel
            if (show)
            {
                // Usar corrutina para asegurar que el panel esté completamente activado
                StartCoroutine(ConfigureGameOverButtonCoroutine());
            }
        }
    }
    
    /// <summary>
    /// Corrutina para configurar el botón de reiniciar en el GameOverPanel.
    /// </summary>
    private System.Collections.IEnumerator ConfigureGameOverButtonCoroutine()
    {
        // Esperar un frame para asegurar que el panel esté completamente activado
        yield return null;
        
        // Buscar el botón de reiniciar dentro del GameOverPanel
        Button gameOverRestartButton = gameOverPanel.GetComponentInChildren<Button>(true);
        if (gameOverRestartButton != null)
        {
            // Remover listeners anteriores para evitar duplicados
            gameOverRestartButton.onClick.RemoveAllListeners();
            // Agregar el listener
            gameOverRestartButton.onClick.AddListener(OnRestartClicked);
            Debug.Log("✅ Botón de reiniciar del GameOverPanel configurado");
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontró botón de reiniciar en GameOverPanel");
        }
    }
    
    /// <summary>
    /// Resetea toda la UI a su estado inicial.
    /// </summary>
    public void ResetUI()
    {
        // Resetear progreso
        if (progressBar != null)
        {
            progressBar.value = 0f;
        }
        
        if (progressText != null)
        {
            progressText.text = "Paso: 0/8";
        }
        
        // Resetear tiempo
        if (timeText != null)
        {
            timeText.text = "Tiempo: 00:00";
            timeText.color = Color.white;
        }
        
        // Resetear intentos
        if (attemptsText != null)
        {
            attemptsText.text = "Intentos: 0/3";
            attemptsText.color = Color.white;
        }
        
        // Ocultar feedback
        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
        }
        
        // Ocultar paneles
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
        
        if (gameCompletePanel != null)
            gameCompletePanel.SetActive(false);
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }
    
    /// <summary>
    /// Se llama cuando se hace click en reiniciar.
    /// </summary>
    private void OnRestartClicked()
    {
        Debug.Log("🔄 Botón de reiniciar clickeado");
        
        if (gameManager != null)
        {
            Debug.Log("✅ GameManager encontrado, llamando RestartGame()");
            gameManager.RestartGame();
        }
        else
        {
            Debug.LogError("❌ GameManager no encontrado en UIManager!");
        }
    }
    
    /// <summary>
    /// Se llama cuando se hace click en saltar tutorial.
    /// </summary>
    private void OnTutorialSkipClicked()
    {
        if (gameManager != null)
        {
            gameManager.EndTutorial();
        }
    }
    
    /// <summary>
    /// Muestra/oculta la barra de progreso de ajuste.
    /// </summary>
    /// <param name="worldPosition">Posición en el mundo de la tuerca</param>
    /// <param name="progress">Progreso de 0 a 1</param>
    /// <param name="show">Mostrar u ocultar</param>
    public void ShowTighteningProgress(Vector3 worldPosition, float progress, bool show)
    {
        if (tighteningProgressPanel == null) return;
        
        tighteningProgressPanel.SetActive(show);
        
        if (show)
        {
            // Actualizar barra de progreso
            if (tighteningProgressBar != null)
            {
                tighteningProgressBar.value = progress;
            }
            
            // Actualizar texto si existe
            if (tighteningProgressText != null)
            {
                tighteningProgressText.text = $"Ajustando... {(progress * 100):F0}%";
            }
            
            // Si hay world space canvas, posicionar la barra cerca de la tuerca
            if (worldSpaceCanvas != null && mainCamera != null)
            {
                // Convertir posición del mundo a posición de pantalla
                Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPosition);
                screenPos.y += 50f; // Offset arriba de la tuerca
                
                RectTransform panelRect = tighteningProgressPanel.GetComponent<RectTransform>();
                if (panelRect != null)
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        worldSpaceCanvas.GetComponent<RectTransform>(),
                        screenPos,
                        worldSpaceCanvas.worldCamera,
                        out Vector2 localPoint);
                    panelRect.localPosition = localPoint;
                }
            }
        }
    }
}

