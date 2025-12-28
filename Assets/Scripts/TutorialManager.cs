using UnityEngine;
using System.Collections;

/// <summary>
/// Gestiona el tutorial inicial que enseña los controles y la mecánica del juego.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [Header("Configuración del Tutorial")]
    [Tooltip("Duración del resaltado de cada tuerca en el tutorial")]
    [SerializeField] private float highlightDuration = 2f;
    
    [Tooltip("Tiempo entre resaltados")]
    [SerializeField] private float highlightInterval = 0.5f;
    
    [Tooltip("¿Mostrar tutorial automáticamente al inicio?")]
    [SerializeField] private bool autoStartTutorial = true;
    
    [Tooltip("Número de tuercas en el tutorial (solo las primeras)")]
    [SerializeField] private int tutorialNutCount = 3;
    
    [Header("Referencias")]
    [Tooltip("Referencia al SequenceManager")]
    [SerializeField] private SequenceManager sequenceManager;
    
    [Tooltip("Referencia al FeedbackManager")]
    [SerializeField] private FeedbackManager feedbackManager;
    
    [Tooltip("Referencia al GameManager (para verificar modo simulación)")]
    [SerializeField] private GameManager gameManager;
    
    private Nut[] allNuts;
    private bool isTutorialActive = false;
    private Coroutine tutorialCoroutine;
    private GameObject[] hiddenBoltParents; // Guardar referencias a los objetos ocultos
    private int[] tutorialSequence; // Secuencia aleatoria de tuercas para el tutorial
    
    private void Awake()
    {
        // Buscar todas las tuercas en la escena
        allNuts = FindObjectsOfType<Nut>();
        
        // Ordenar por ID
        System.Array.Sort(allNuts, (a, b) => a.NutID.CompareTo(b.NutID));
        
        // Buscar referencias si no están asignadas
        if (sequenceManager == null)
            sequenceManager = FindObjectOfType<SequenceManager>();
        
        if (feedbackManager == null)
            feedbackManager = FindObjectOfType<FeedbackManager>();
        
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();
    }
    
    private void Start()
    {
        if (autoStartTutorial)
        {
            // El GameManager iniciará el tutorial
        }
    }
    
    /// <summary>
    /// Inicia el tutorial.
    /// </summary>
    public void StartTutorial()
    {
        if (isTutorialActive) return;
        
        isTutorialActive = true;
        
        // Generar secuencia aleatoria de 3 tuercas
        GenerateRandomTutorialSequence();
        
        // Resetear secuencia para el tutorial y establecer límite de 3 tuercas
        if (sequenceManager != null)
        {
            sequenceManager.ResetSequence();
            sequenceManager.SetTutorialSequence(tutorialSequence); // Usar la secuencia aleatoria
            sequenceManager.SetTutorialLimit(tutorialNutCount);
        }
        
        // Resetear todas las tuercas
        foreach (Nut nut in allNuts)
        {
            if (nut != null)
            {
                nut.ResetNut();
            }
        }
        
        // Iniciar corrutina del tutorial
        if (tutorialCoroutine != null)
        {
            StopCoroutine(tutorialCoroutine);
        }
        
        tutorialCoroutine = StartCoroutine(TutorialSequenceCoroutine());
    }
    
    /// <summary>
    /// Genera una secuencia aleatoria de 3 tuercas para el tutorial.
    /// </summary>
    private void GenerateRandomTutorialSequence()
    {
        // Crear lista con todas las tuercas disponibles (1-8)
        System.Collections.Generic.List<int> availableNuts = new System.Collections.Generic.List<int>();
        for (int i = 1; i <= 8; i++)
        {
            availableNuts.Add(i);
        }
        
        // Seleccionar 3 tuercas aleatorias
        tutorialSequence = new int[tutorialNutCount];
        for (int i = 0; i < tutorialNutCount; i++)
        {
            int randomIndex = Random.Range(0, availableNuts.Count);
            tutorialSequence[i] = availableNuts[randomIndex];
            availableNuts.RemoveAt(randomIndex); // Remover para no repetir
        }
        
        // Ordenar la secuencia para mantener el orden correcto
        System.Array.Sort(tutorialSequence);
        
        Debug.Log($"Tutorial: Secuencia aleatoria generada: {string.Join(" → ", tutorialSequence)}");
    }
    
    /// <summary>
    /// Finaliza el tutorial.
    /// </summary>
    public void EndTutorial()
    {
        if (!isTutorialActive) return;
        
        isTutorialActive = false;
        
        // Detener corrutina
        if (tutorialCoroutine != null)
        {
            StopCoroutine(tutorialCoroutine);
            tutorialCoroutine = null;
        }
        
        // Reactivar todos los objetos que estaban ocultos
        if (hiddenBoltParents != null)
        {
            foreach (GameObject boltParent in hiddenBoltParents)
            {
                if (boltParent != null)
                {
                    boltParent.SetActive(true);
                    Debug.Log($"✅ Reactivado desde TutorialManager: {boltParent.name}");
                }
            }
            hiddenBoltParents = null; // Limpiar referencias
        }
        
        // Quitar resaltado de todas las tuercas
        foreach (Nut nut in allNuts)
        {
            if (nut != null)
            {
                nut.Highlight(false);
            }
        }
    }
    
    /// <summary>
    /// Inicia los highlights para el juego guiado (muestra el orden correcto de las 8 tuercas).
    /// </summary>
    public void StartGuidedGameHighlights()
    {
        if (sequenceManager == null) return;
        
        // Detener cualquier corrutina anterior
        if (tutorialCoroutine != null)
        {
            StopCoroutine(tutorialCoroutine);
        }
        
        // Iniciar corrutina para mostrar highlights del juego guiado
        tutorialCoroutine = StartCoroutine(GuidedGameHighlightsCoroutine());
    }
    
    /// <summary>
    /// Corrutina que muestra los highlights del juego guiado (todas las 8 tuercas en orden).
    /// </summary>
    private IEnumerator GuidedGameHighlightsCoroutine()
    {
        // Esperar un momento antes de empezar
        yield return new WaitForSeconds(1f);
        
        // Obtener la secuencia correcta del SequenceManager
        int[] correctSequence = sequenceManager.GetCorrectSequence();
        
        if (correctSequence == null || correctSequence.Length == 0)
        {
            Debug.LogWarning("⚠️ No se pudo obtener la secuencia correcta para el juego guiado");
            yield break;
        }
        
        // Mostrar mensaje inicial
        Debug.Log($"Juego Guiado: Observa el orden correcto: {string.Join(" → ", correctSequence)}");
        
        // Resaltar cada tuerca de la secuencia correcta
        foreach (int nutID in correctSequence)
        {
            Nut nut = System.Array.Find(allNuts, n => n != null && n.NutID == nutID);
            
            if (nut != null)
            {
                // Resaltar la tuerca
                nut.Highlight(true);
                
                // Esperar
                yield return new WaitForSeconds(highlightDuration);
                
                // Quitar resaltado
                nut.Highlight(false);
                
                // Esperar entre resaltados
                yield return new WaitForSeconds(highlightInterval);
            }
        }
        
        Debug.Log("✅ Secuencia del juego guiado mostrada. Ahora puedes empezar a jugar.");
    }
    
    /// <summary>
    /// Detiene los highlights del juego guiado.
    /// </summary>
    public void StopGuidedGameHighlights()
    {
        // Detener corrutina
        if (tutorialCoroutine != null)
        {
            StopCoroutine(tutorialCoroutine);
            tutorialCoroutine = null;
        }
        
        // Quitar resaltado de todas las tuercas
        foreach (Nut nut in allNuts)
        {
            if (nut != null)
            {
                nut.Highlight(false);
            }
        }
    }
    
    /// <summary>
    /// Corrutina que muestra la secuencia del tutorial (solo las primeras 3 tuercas).
    /// </summary>
    private IEnumerator TutorialSequenceCoroutine()
    {
        // Esperar un momento antes de empezar
        yield return new WaitForSeconds(1f);
        
        // Ocultar las tuercas que NO están en la secuencia del tutorial y guardar referencias
        System.Collections.Generic.List<GameObject> hiddenObjects = new System.Collections.Generic.List<GameObject>();
        foreach (Nut nut in allNuts)
        {
            if (nut != null)
            {
                // Verificar si esta tuerca está en la secuencia del tutorial
                bool isInTutorial = System.Array.IndexOf(tutorialSequence, nut.NutID) >= 0;
                
                if (!isInTutorial)
                {
                    // Ocultar las tuercas que no son del tutorial
                    GameObject parent = nut.gameObject.transform.parent.gameObject;
                    if (parent != null)
                    {
                        parent.SetActive(false);
                        hiddenObjects.Add(parent);
                        Debug.Log($"Ocultado en tutorial: {parent.name}");
                    }
                }
            }
        }
        hiddenBoltParents = hiddenObjects.ToArray();
        
        // Mostrar mensaje inicial
        Debug.Log($"Tutorial: Observa el orden de las tuercas: {string.Join(" → ", tutorialSequence)}. Luego practica haciendo click en ellas.");
        
        // Resaltar cada tuerca del tutorial en secuencia
        foreach (int nutID in tutorialSequence)
        {
            Nut nut = System.Array.Find(allNuts, n => n != null && n.NutID == nutID);
            
            if (nut != null)
            {
                // Resaltar la tuerca
                nut.Highlight(true);
                
                // Esperar
                yield return new WaitForSeconds(highlightDuration);
                
                // Quitar resaltado
                nut.Highlight(false);
                
                // Intervalo entre tuercas
                yield return new WaitForSeconds(highlightInterval);
            }
        }
        
        // Mensaje: ahora el jugador puede practicar
        Debug.Log($"Ahora practica: Haz click y mantén presionado en las tuercas en el orden {string.Join(" → ", tutorialSequence)}");
        
        yield return new WaitForSeconds(1f);
        
        // Resaltar la primera tuerca del tutorial (aleatoria) para que el jugador sepa por dónde empezar
        if (tutorialSequence != null && tutorialSequence.Length > 0)
        {
            int firstNutID = tutorialSequence[0];
            Nut firstNut = System.Array.Find(allNuts, n => n != null && n.NutID == firstNutID);
            if (firstNut != null)
            {
                firstNut.Highlight(true);
            }
        }
        
        // Esperar a que el jugador complete el tutorial (las primeras 3 tuercas)
        // Esto se manejará cuando se completen las 3 tuercas del tutorial
    }
    
    /// <summary>
    /// Actualiza el resaltado durante el juego (resalta la siguiente tuerca correcta).
    /// Solo funciona en modo guiado, NO en simulación.
    /// </summary>
    public void UpdateHighlight()
    {
        // No mostrar highlights si estamos en modo simulación
        if (gameManager != null && gameManager.IsSimulationMode)
        {
            // Asegurarse de que no haya highlights activos en simulación
            foreach (Nut nut in allNuts)
            {
                if (nut != null && !nut.IsTightened)
                {
                    nut.Highlight(false);
                }
            }
            return;
        }
        
        // Solo mostrar highlights en modo guiado (no en tutorial)
        if (!isTutorialActive && sequenceManager != null && gameManager != null && gameManager.IsGuidedGameMode)
        {
            // Quitar resaltado de todas las tuercas
            foreach (Nut nut in allNuts)
            {
                if (nut != null && !nut.IsTightened)
                {
                    nut.Highlight(false);
                }
            }
            
            // Resaltar la siguiente tuerca correcta
            int nextNutID = sequenceManager.GetNextCorrectNutID();
            if (nextNutID > 0)
            {
                Nut nextNut = System.Array.Find(allNuts, n => n != null && n.NutID == nextNutID);
                if (nextNut != null && !nextNut.IsTightened)
                {
                    nextNut.Highlight(true);
                }
            }
        }
    }
    
    private void Update()
    {
        // Actualizar resaltado continuamente (solo si no está en tutorial activo y no está en simulación)
        if (!isTutorialActive)
        {
            UpdateHighlight();
        }
    }
    
    public bool IsTutorialActive => isTutorialActive;
}

