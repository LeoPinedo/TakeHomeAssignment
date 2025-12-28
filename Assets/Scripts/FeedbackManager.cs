using UnityEngine;
using System.Collections;

/// <summary>
/// Maneja todas las animaciones, efectos visuales, sonidos y feedback del juego.
/// </summary>
public class FeedbackManager : MonoBehaviour
{
    [Header("Materiales")]
    [Tooltip("Material para tuerca ajustada correctamente")]
    [SerializeField] private Material tightenedMaterial;
    
    [Tooltip("Material para tuerca resaltada (tutorial)")]
    [SerializeField] private Material highlightedMaterial;
    
    [Tooltip("Material para selección incorrecta")]
    [SerializeField] private Material wrongMaterial;
    
    [Header("Animaciones")]
    [Tooltip("Duración de la animación de ajuste")]
    [SerializeField] private float tightenAnimationDuration = 1f;
    
    [Tooltip("Duración de la animación del perno bajando")]
    [SerializeField] private float boltNutDownDuration = 0.5f;
    
    [Tooltip("Posición Y objetivo del perno (ajustar según tu escena - debe bajar más)")]
    [SerializeField] private float boltNutTargetY = 100f;
    
    [Tooltip("Posición Y objetivo de la tuerca (ajustar según tu escena - debe bajar más)")]
    [SerializeField] private float screwTargetY = 2f;
    
    [Tooltip("Tiempo de espera entre bajar el perno y bajar la tuerca (segundos)")]
    [SerializeField] private float waitBetweenAnimations = 3.0f;
    
    [Tooltip("Ángulo de rotación al ajustar")]
    [SerializeField] private float tightenRotationAngle = 360f;
    
    [Tooltip("Usar posición del mundo en lugar de posición local (útil si los objetos están rotados)")]
    [SerializeField] private bool useWorldPosition = false;
    
    [Tooltip("Escala al hacer focus (zoom)")]
    [SerializeField] private float focusScale = 1.2f;
    
    [Tooltip("Duración del efecto de focus")]
    [SerializeField] private float focusDuration = 0.3f;
    
    [Header("Efectos Visuales")]
    [Tooltip("Intensidad del resaltado")]
    [SerializeField] private float highlightIntensity = 1.5f;
    
    [Tooltip("Color de resaltado")]
    [SerializeField] private Color highlightColor = Color.yellow;
    
    [Tooltip("Duración del feedback de error")]
    [SerializeField] private float errorFeedbackDuration = 0.5f;
    
    [Header("Sonidos (Opcional)")]
    [Tooltip("AudioSource para sonidos")]
    [SerializeField] private AudioSource audioSource;
    
    [Tooltip("Clip de sonido para acierto")]
    [SerializeField] private AudioClip correctSound;
    
    [Tooltip("Clip de sonido para error")]
    [SerializeField] private AudioClip wrongSound;
    
    private Camera mainCamera;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    
    // Diccionario para guardar posiciones iniciales de objetos (para resetear después del tutorial)
    private System.Collections.Generic.Dictionary<Transform, Vector3> originalPositions = new System.Collections.Generic.Dictionary<Transform, Vector3>();
    private System.Collections.Generic.Dictionary<Transform, Vector3> originalRotations = new System.Collections.Generic.Dictionary<Transform, Vector3>();
    
    // Para animación durante la carga
    private System.Collections.Generic.Dictionary<Nut, TighteningProgress> activeTighteningProgress = new System.Collections.Generic.Dictionary<Nut, TighteningProgress>();
    
    private class TighteningProgress
    {
        public Transform nutTransform;         // Bolt_Nut (tuerca) - debe girar
        public Transform screwTransform;       // Bolt_Screw (perno) - debe bajar
        public Vector3 nutStartPos;
        public Vector3 screwStartPos;
        public Vector3 nutStartRotation;
        public Vector3 nutTargetPos;
        public Vector3 screwTargetPos;
        public float targetRotation;
    }
    
    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            originalCameraPosition = mainCamera.transform.position;
            originalCameraRotation = mainCamera.transform.rotation;
        }
        
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }
    
    /// <summary>
    /// Inicia el proceso de ajuste gradual (se llama cuando se inicia el click).
    /// Guarda las posiciones iniciales y prepara la animación.
    /// Nut = tuerca (debe girar), Screw = perno (debe bajar)
    /// </summary>
    public void StartTighteningProgress(Nut nut, Transform screwTransform)
    {
        if (nut == null || screwTransform == null) return;
        
        Transform nutTransform = nut.GetTransform(); // Bolt_Nut (la tuerca) - donde está el script Nut.cs
        if (nutTransform == null) return;
        
        // Si ya existe un progreso activo, cancelarlo primero
        if (activeTighteningProgress.ContainsKey(nut))
        {
            CancelTighteningProgress(nut);
        }
        
        // Guardar posiciones iniciales
        TighteningProgress progress = new TighteningProgress();
        progress.nutTransform = nutTransform;        // Tuerca (debe girar)
        progress.screwTransform = screwTransform;    // Perno (debe bajar)
        
        if (useWorldPosition)
        {
            progress.nutStartPos = nutTransform.position;
            progress.screwStartPos = screwTransform.position;
        }
        else
        {
            progress.nutStartPos = nutTransform.localPosition;
            progress.screwStartPos = screwTransform.localPosition;
        }
        
        progress.nutStartRotation = nutTransform.localEulerAngles;
        
        // Calcular posiciones objetivo
        progress.nutTargetPos = progress.nutStartPos;
        progress.screwTargetPos = progress.screwStartPos;
        
        if (useWorldPosition)
        {
            // El perno (screw) baja más
            progress.screwTargetPos.y = boltNutTargetY; // Usar boltNutTargetY para el perno
            // La tuerca (nut) baja menos
            progress.nutTargetPos.y = screwTargetY; // Usar screwTargetY para la tuerca
        }
        else
        {
            // Calcular diferencia para posición local
            Vector3 currentWorldScrewPos = screwTransform.position;
            float worldScrewYDifference = currentWorldScrewPos.y - boltNutTargetY;
            progress.screwTargetPos.y = progress.screwStartPos.y - worldScrewYDifference;
            
            Vector3 currentWorldNutPos = nutTransform.position;
            float worldNutYDifference = currentWorldNutPos.y - screwTargetY;
            progress.nutTargetPos.y = progress.nutStartPos.y - worldNutYDifference;
        }
        
        progress.targetRotation = tightenRotationAngle;
        
        activeTighteningProgress[nut] = progress;
    }
    
    /// <summary>
    /// Actualiza la animación gradual basada en el progreso (0 a 1).
    /// Se llama cada frame mientras se mantiene presionado.
    /// El perno (screw) baja, la tuerca (nut) gira.
    /// </summary>
    public void UpdateTighteningProgress(Nut nut, float progress)
    {
        if (nut == null || !activeTighteningProgress.ContainsKey(nut)) return;
        
        TighteningProgress tightening = activeTighteningProgress[nut];
        progress = Mathf.Clamp01(progress);
        
        // Interpolar posiciones y rotación
        // El perno (screw) baja
        Vector3 currentScrewPos = Vector3.Lerp(tightening.screwStartPos, tightening.screwTargetPos, progress);
        // La tuerca (nut) baja un poco
        Vector3 currentNutPos = Vector3.Lerp(tightening.nutStartPos, tightening.nutTargetPos, progress);
        // La tuerca (nut) gira
        float currentRotation = Mathf.Lerp(0f, tightening.targetRotation, progress);
        
        // Aplicar posiciones
        if (useWorldPosition)
        {
            tightening.screwTransform.position = currentScrewPos;  // Perno baja
            tightening.nutTransform.position = currentNutPos;      // Tuerca baja
        }
        else
        {
            tightening.screwTransform.localPosition = currentScrewPos;  // Perno baja
            tightening.nutTransform.localPosition = currentNutPos;     // Tuerca baja
        }
        
        // Aplicar rotación a la tuerca (nut)
        Vector3 newRotation = tightening.nutStartRotation;
        newRotation.z += currentRotation; // Rotar en Z (o el eje que prefieras)
        tightening.nutTransform.localEulerAngles = newRotation;
    }
    
    /// <summary>
    /// Cancela el proceso de ajuste y restaura las posiciones iniciales.
    /// </summary>
    public void CancelTighteningProgress(Nut nut)
    {
        if (nut == null || !activeTighteningProgress.ContainsKey(nut)) return;
        
        TighteningProgress tightening = activeTighteningProgress[nut];
        
        // Restaurar posiciones iniciales
        if (useWorldPosition)
        {
            tightening.screwTransform.position = tightening.screwStartPos;  // Perno
            tightening.nutTransform.position = tightening.nutStartPos;      // Tuerca
        }
        else
        {
            tightening.screwTransform.localPosition = tightening.screwStartPos;  // Perno
            tightening.nutTransform.localPosition = tightening.nutStartPos;     // Tuerca
        }
        
        // Restaurar rotación inicial de la tuerca
        tightening.nutTransform.localEulerAngles = tightening.nutStartRotation;
        
        activeTighteningProgress.Remove(nut);
    }
    
    /// <summary>
    /// Completa la animación de ajuste (se llama cuando se valida correctamente).
    /// Termina la animación y cambia el material.
    /// </summary>
    public void CompleteTighteningAnimation(Nut nut)
    {
        if (nut == null || !activeTighteningProgress.ContainsKey(nut)) return;
        
        TighteningProgress tightening = activeTighteningProgress[nut];
        
        // Asegurar posiciones finales
        if (useWorldPosition)
        {
            tightening.screwTransform.position = tightening.screwTargetPos;  // Perno
            tightening.nutTransform.position = tightening.nutTargetPos;      // Tuerca
        }
        else
        {
            tightening.screwTransform.localPosition = tightening.screwTargetPos;  // Perno
            tightening.nutTransform.localPosition = tightening.nutTargetPos;     // Tuerca
        }
        
        // Asegurar rotación final de la tuerca
        Vector3 finalRotation = tightening.nutStartRotation;
        finalRotation.z += tightening.targetRotation;
        tightening.nutTransform.localEulerAngles = finalRotation;
        
        // Cambiar material
        ChangeNutMaterial(nut, true);
        
        // Limpiar
        activeTighteningProgress.Remove(nut);
    }
    
    /// <summary>
    /// Inicia la animación completa de ajuste: primero baja el perno, luego la tuerca se ajusta.
    /// (Método legacy - ahora usamos el sistema de progreso gradual)
    /// </summary>
    public void StartTighteningAnimation(Nut nut, Transform boltNutTransform)
    {
        if (nut == null) return;
        
            // Si hay un progreso activo, completarlo
            if (activeTighteningProgress.ContainsKey(nut))
            {
                CompleteTighteningAnimation(nut);
            }
            else
            {
                // Si no hay progreso, usar la animación completa (legacy)
                // boltNutTransform es en realidad el screw (perno)
                StartCoroutine(CompleteTighteningAnimationCoroutine(nut, boltNutTransform));
            }
    }
    
    /// <summary>
    /// Corrutina completa: primero baja el perno (Bolt_Screw), luego baja y rota la tuerca (Bolt_Nut).
    /// Nut = tuerca (debe girar), Screw = perno (debe bajar)
    /// </summary>
    private IEnumerator CompleteTighteningAnimationCoroutine(Nut nut, Transform screwTransform)
    {
        Transform nutTransform = nut.GetTransform(); // Bolt_Nut (la tuerca) - donde está el script
        if (nutTransform == null) yield break;
        
        // Guardar posiciones iniciales (usar world o local según configuración)
        Vector3 screwStartPos = Vector3.zero;  // Perno (Bolt_Screw)
        Vector3 nutStartPos = Vector3.zero;    // Tuerca (Bolt_Nut)
        
        if (useWorldPosition)
        {
            screwStartPos = screwTransform != null ? screwTransform.position : Vector3.zero;
            nutStartPos = nutTransform.position;
        }
        else
        {
            screwStartPos = screwTransform != null ? screwTransform.localPosition : Vector3.zero;
            nutStartPos = nutTransform.localPosition;
        }
        
        Vector3 nutStartRotation = nutTransform.localEulerAngles;
        
        // Focus de cámara desactivado - la cámara permanece en su posición original
        
        // PASO 1: Bajar el perno (Bolt_Screw) primero
        // Calcular posición objetivo basada en el Y objetivo
        Vector3 screwTargetPos = screwStartPos;
        
        if (useWorldPosition)
        {
            // Si usamos posición del mundo, el Y objetivo es absoluto
            // El perno (screw) baja más
            screwTargetPos.y = boltNutTargetY;
        }
        else
        {
            // Si usamos posición local, necesitamos calcular la diferencia
            // Obtener la posición del mundo actual para calcular la diferencia
            Vector3 currentWorldPos = screwTransform.position;
            float worldYDifference = currentWorldPos.y - boltNutTargetY;
            // Aplicar la diferencia a la posición local
            screwTargetPos.y = screwStartPos.y - worldYDifference;
        }
        
        float actualScrewDistance = Mathf.Abs(screwStartPos.y - screwTargetPos.y);
        Debug.Log($"🔧 Perno: Posición inicial Y = {screwStartPos.y}, Posición objetivo Y = {screwTargetPos.y}, Distancia = {actualScrewDistance}, UseWorld = {useWorldPosition}");
        
        // Bajar el perno primero
        float elapsed1 = 0f;
        
        while (elapsed1 < boltNutDownDuration)
        {
            elapsed1 += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, elapsed1 / boltNutDownDuration);
            
            // Bajar el perno (screw)
            Vector3 newScrewPos = Vector3.Lerp(screwStartPos, screwTargetPos, progress);
            
            if (useWorldPosition)
            {
                screwTransform.position = newScrewPos;
            }
            else
            {
                screwTransform.localPosition = newScrewPos;
            }
            
            yield return null;
        }
        
        // Asegurar posición final del perno (bajado)
        if (useWorldPosition)
        {
            screwTransform.position = screwTargetPos;
        }
        else
        {
            screwTransform.localPosition = screwTargetPos;
        }
        Debug.Log($"✅ Perno bajado: Y = {screwTargetPos.y}");
        
        // Esperar entre bajar el perno y bajar/rotar la tuerca
        yield return new WaitForSeconds(waitBetweenAnimations);
        
        // PASO 2: Bajar la tuerca (Bolt_Nut) y rotarla simultáneamente
        Vector3 nutTargetPos = nutStartPos;
        
        if (useWorldPosition)
        {
            // Si usamos posición del mundo, el Y objetivo es absoluto
            // La tuerca (nut) baja menos
            nutTargetPos.y = screwTargetY;
        }
        else
        {
            // Si usamos posición local, necesitamos calcular la diferencia
            // Obtener la posición del mundo actual para calcular la diferencia
            Vector3 currentWorldPos = nutTransform.position;
            float worldYDifference = currentWorldPos.y - screwTargetY;
            // Aplicar la diferencia a la posición local
            nutTargetPos.y = nutStartPos.y - worldYDifference;
        }
        
        float actualNutDistance = Mathf.Abs(nutStartPos.y - nutTargetPos.y);
        Debug.Log($"🔩 Tuerca: Posición inicial Y = {nutStartPos.y}, Posición objetivo Y = {nutTargetPos.y}, Distancia = {actualNutDistance}, UseWorld = {useWorldPosition}");
        
        // Duración restante para bajar y rotar la tuerca simultáneamente
        float remainingDuration = tightenAnimationDuration - boltNutDownDuration - waitBetweenAnimations;
        float elapsed2 = 0f;
        
        while (elapsed2 < remainingDuration)
        {
            elapsed2 += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, elapsed2 / remainingDuration);
            
            // Bajar la tuerca
            Vector3 newNutPos = Vector3.Lerp(nutStartPos, nutTargetPos, progress);
            
            if (useWorldPosition)
            {
                nutTransform.position = newNutPos;
            }
            else
            {
                nutTransform.localPosition = newNutPos;
            }
            
            // Rotar la tuerca mientras baja
            float currentRotation = Mathf.Lerp(0f, tightenRotationAngle, progress);
            Vector3 newRotation = nutStartRotation;
            newRotation.z += currentRotation;
            nutTransform.localEulerAngles = newRotation;
            
            yield return null;
        }
        
        // Asegurar posición final de la tuerca
        if (useWorldPosition)
        {
            nutTransform.position = nutTargetPos;
        }
        else
        {
            nutTransform.localPosition = nutTargetPos;
        }
        Debug.Log($"✅ Tuerca finalizada: Y = {nutTargetPos.y}");
        
        // Asegurar rotación final de la tuerca
        Vector3 finalRotation = nutStartRotation;
        finalRotation.z += tightenRotationAngle;
        nutTransform.localEulerAngles = finalRotation;
        
        // Cambiar material de la tuerca después de completar la animación
        if (nut != null)
        {
            ChangeNutMaterial(nut, true);
        }
    }
    
    /// <summary>
    /// Reproduce la animación de ajuste de tuerca (método legacy, mantenido por compatibilidad).
    /// </summary>
    public void PlayTightenAnimation(Nut nut)
    {
        if (nut == null) return;
        
        StartCoroutine(TightenAnimationCoroutine(nut));
    }
    
    /// <summary>
    /// Corrutina para la animación de ajuste (solo rotación, método legacy).
    /// </summary>
    private IEnumerator TightenAnimationCoroutine(Nut nut)
    {
        Transform nutTransform = nut.GetTransform();
        if (nutTransform == null) yield break;
        
        Vector3 startRotation = nutTransform.localEulerAngles;
        float elapsed = 0f;
        
        while (elapsed < tightenAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / tightenAnimationDuration;
            
            // Rotación suave
            float currentRotation = Mathf.Lerp(0f, tightenRotationAngle, progress);
            nutTransform.localEulerAngles = startRotation + new Vector3(0, currentRotation, 0);
            
            yield return null;
        }
        
        // Asegurar rotación final
        nutTransform.localEulerAngles = startRotation + new Vector3(0, tightenRotationAngle, 0);
    }
    
    /// <summary>
    /// Cambia el material de la tuerca.
    /// </summary>
    public void ChangeNutMaterial(Nut nut, bool isTightened)
    {
        if (nut == null) return;
        
        Renderer renderer = nut.GetRenderer();
        if (renderer == null) return;
        
        Material[] materials = renderer.materials;
        
        if (isTightened && tightenedMaterial != null)
        {
            // Cambiar el primer material
            materials[0] = tightenedMaterial;
            renderer.materials = materials;
        }
    }
    
    /// <summary>
    /// Aplica el feedback visual según el estado de la tuerca.
    /// </summary>
    public void ApplyNutStateVisual(Nut nut, Nut.NutState state)
    {
        if (nut == null) return;
        
        Renderer renderer = nut.GetRenderer();
        if (renderer == null) return;
        
        Material[] materials = renderer.materials;
        
        switch (state)
        {
            case Nut.NutState.Highlighted:
                // Resaltar para tutorial
                if (highlightedMaterial != null)
                {
                    materials[0] = highlightedMaterial;
                }
                else
                {
                    // Fallback: cambiar color de emisión
                    materials[0].EnableKeyword("_EMISSION");
                    materials[0].SetColor("_EmissionColor", highlightColor * highlightIntensity);
                }
                renderer.materials = materials;
                break;
                
            case Nut.NutState.Wrong:
                // Feedback de error
                StartCoroutine(ErrorFeedbackCoroutine(nut));
                break;
                
            case Nut.NutState.Idle:
                // Restaurar material original
                Material originalMaterial = nut.GetOriginalMaterial();
                if (originalMaterial != null)
                {
                    materials[0] = originalMaterial;
                    materials[0].DisableKeyword("_EMISSION");
                    renderer.materials = materials;
                }
                break;
        }
    }
    
    /// <summary>
    /// Corrutina para feedback de error (parpadeo rojo).
    /// </summary>
    private IEnumerator ErrorFeedbackCoroutine(Nut nut)
    {
        Renderer renderer = nut.GetRenderer();
        if (renderer == null) yield break;
        
        Material[] materials = renderer.materials;
        Material originalMaterial = materials[0];
        
        // Cambiar a material de error
        if (wrongMaterial != null)
        {
            materials[0] = wrongMaterial;
            renderer.materials = materials;
        }
        else
        {
            // Fallback: tinte rojo
            materials[0].color = Color.red;
            renderer.materials = materials;
        }
        
        // Reproducir sonido de error
        PlaySound(wrongSound);
        
        yield return new WaitForSeconds(errorFeedbackDuration);
        
        // Restaurar material original
        if (nut != null && renderer != null)
        {
            materials[0] = originalMaterial;
            renderer.materials = materials;
            
            // Resetear estado a Idle si no está ajustada
            if (!nut.IsTightened)
            {
                nut.SetState(Nut.NutState.Idle);
            }
        }
    }
    
    /// <summary>
    /// Hace focus (zoom) en una tuerca cuando se selecciona.
    /// </summary>
    public void FocusOnNut(Nut nut)
    {
        if (nut == null || mainCamera == null) return;
        
        StartCoroutine(FocusCoroutine(nut));
    }
    
    /// <summary>
    /// Corrutina para enfocar la acción de ajuste desde un ángulo lateral claro.
    /// Enfoca el GameObject padre (Bolt_X) que contiene ambos elementos.
    /// </summary>
    private IEnumerator FocusOnTighteningAction(Transform screwTransform, Transform boltNutTransform)
    {
        if (mainCamera == null || screwTransform == null) yield break;
        
        // Guardar posición original si no está guardada
        if (originalCameraPosition == Vector3.zero && mainCamera.transform.position != Vector3.zero)
        {
            originalCameraPosition = mainCamera.transform.position;
            originalCameraRotation = mainCamera.transform.rotation;
        }
        
        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;
        
        // Obtener el GameObject padre (Bolt_X) que contiene ambos
        Transform parentTransform = screwTransform.parent;
        if (parentTransform == null && boltNutTransform != null)
        {
            parentTransform = boltNutTransform.parent;
        }
        
        // Calcular punto objetivo: usar el padre si existe, sino el punto medio
        Vector3 targetPoint;
        if (parentTransform != null)
        {
            // Usar la posición del padre (Bolt_X)
            targetPoint = parentTransform.position;
        }
        else
        {
            // Fallback: punto medio entre perno y tuerca
            targetPoint = screwTransform.position;
            if (boltNutTransform != null)
            {
                targetPoint = (screwTransform.position + boltNutTransform.position) * 0.5f;
            }
        }
        
        // Posición de cámara: lateral y ligeramente arriba para ver mejor la acción completa
        // Ajustar distancia según el tamaño del objeto
        Vector3 offset = new Vector3(0.6f, 0.4f, -1.0f); // Lateral derecho, arriba, atrás
        Vector3 targetPosition = targetPoint + offset;
        
        float elapsed = 0f;
        float halfDuration = focusDuration * 0.5f;
        
        // Ir hacia la posición de focus
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, elapsed / halfDuration);
            
            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
            mainCamera.transform.rotation = Quaternion.Slerp(startRotation, 
                Quaternion.LookRotation(targetPoint - targetPosition), progress);
            
            yield return null;
        }
        
        // Mantener focus durante la animación completa
        yield return new WaitForSeconds(tightenAnimationDuration + 0.2f);
        
        // Volver a posición original
        elapsed = 0f;
        Vector3 returnStartPosition = mainCamera.transform.position;
        Quaternion returnStartRotation = mainCamera.transform.rotation;
        
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, elapsed / halfDuration);
            
            mainCamera.transform.position = Vector3.Lerp(returnStartPosition, originalCameraPosition, progress);
            mainCamera.transform.rotation = Quaternion.Slerp(returnStartRotation, originalCameraRotation, progress);
            
            yield return null;
        }
        
        // Asegurar posición final
        mainCamera.transform.position = originalCameraPosition;
        mainCamera.transform.rotation = originalCameraRotation;
    }
    
    /// <summary>
    /// Corrutina para el efecto de focus (zoom suave) - método legacy.
    /// </summary>
    private IEnumerator FocusCoroutine(Nut nut)
    {
        if (mainCamera == null || nut == null) yield break;
        
        Transform nutTransform = nut.GetTransform();
        if (nutTransform == null) yield break;
        
        // Guardar posición y rotación originales si no están guardadas
        if (originalCameraPosition == Vector3.zero && mainCamera.transform.position != Vector3.zero)
        {
            originalCameraPosition = mainCamera.transform.position;
            originalCameraRotation = mainCamera.transform.rotation;
        }
        
        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;
        
        // Calcular posición objetivo (más cerca de la tuerca)
        Vector3 directionToNut = (nutTransform.position - startPosition).normalized;
        Vector3 targetPosition = nutTransform.position - directionToNut * 1.5f; // Distancia de zoom
        
        float elapsed = 0f;
        float halfDuration = focusDuration * 0.5f;
        
        // Ir hacia la tuerca
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, elapsed / halfDuration);
            
            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
            mainCamera.transform.rotation = Quaternion.Slerp(startRotation, 
                Quaternion.LookRotation(nutTransform.position - mainCamera.transform.position), progress);
            
            yield return null;
        }
        
        // Mantener focus un momento
        yield return new WaitForSeconds(0.2f);
        
        // Volver a posición original
        elapsed = 0f;
        Vector3 returnStartPosition = mainCamera.transform.position;
        Quaternion returnStartRotation = mainCamera.transform.rotation;
        
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, elapsed / halfDuration);
            
            mainCamera.transform.position = Vector3.Lerp(returnStartPosition, originalCameraPosition, progress);
            mainCamera.transform.rotation = Quaternion.Slerp(returnStartRotation, originalCameraRotation, progress);
            
            yield return null;
        }
        
        // Asegurar posición final
        mainCamera.transform.position = originalCameraPosition;
        mainCamera.transform.rotation = originalCameraRotation;
    }
    
    /// <summary>
    /// Reproduce un sonido.
    /// </summary>
    public void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    /// <summary>
    /// Reproduce sonido de acierto.
    /// </summary>
    public void PlayCorrectSound()
    {
        PlaySound(correctSound);
    }
    
    /// <summary>
    /// Reproduce sonido de error.
    /// </summary>
    public void PlayWrongSound()
    {
        PlaySound(wrongSound);
    }
    
    /// <summary>
    /// Resetea la animación de ajuste si fue incorrecto.
    /// </summary>
    public void ResetTighteningAnimation(Nut nut, Transform boltNutTransform)
    {
        if (nut == null) return;
        
        Transform screwTransform = nut.GetTransform();
        if (screwTransform == null) return;
        
        // Resetear posiciones a las originales si están guardadas
        if (originalPositions.ContainsKey(screwTransform))
        {
            if (useWorldPosition)
            {
                screwTransform.position = originalPositions[screwTransform];
            }
            else
            {
                screwTransform.localPosition = originalPositions[screwTransform];
            }
            
            if (originalRotations.ContainsKey(screwTransform))
            {
                screwTransform.localEulerAngles = originalRotations[screwTransform];
            }
        }
        
        if (boltNutTransform != null && originalPositions.ContainsKey(boltNutTransform))
        {
            if (useWorldPosition)
            {
                boltNutTransform.position = originalPositions[boltNutTransform];
            }
            else
            {
                boltNutTransform.localPosition = originalPositions[boltNutTransform];
            }
        }
    }
    
    /// <summary>
    /// Resetea todas las posiciones de tuercas y pernos a sus posiciones originales.
    /// Útil para resetear después del tutorial.
    /// </summary>
    public void ResetAllPositions()
    {
        foreach (var kvp in originalPositions)
        {
            Transform transform = kvp.Key;
            Vector3 originalPos = kvp.Value;
            
            if (transform != null)
            {
                if (useWorldPosition)
                {
                    transform.position = originalPos;
                }
                else
                {
                    transform.localPosition = originalPos;
                }
            }
        }
        
        foreach (var kvp in originalRotations)
        {
            Transform transform = kvp.Key;
            Vector3 originalRot = kvp.Value;
            
            if (transform != null)
            {
                transform.localEulerAngles = originalRot;
            }
        }
        
        Debug.Log($"✅ Posiciones reseteadas: {originalPositions.Count} objetos restaurados");
    }
    
    /// <summary>
    /// Guarda las posiciones iniciales de todos los objetos en la escena.
    /// </summary>
    public void SaveInitialPositions()
    {
        originalPositions.Clear();
        originalRotations.Clear();
        
        // Buscar todos los Nut en la escena
        Nut[] allNuts = FindObjectsOfType<Nut>();
        
        foreach (Nut nut in allNuts)
        {
            if (nut == null) continue;
            
            Transform screwTransform = nut.GetTransform();
            if (screwTransform == null) continue;
            
            // Guardar posición y rotación de la tuerca
            if (useWorldPosition)
            {
                originalPositions[screwTransform] = screwTransform.position;
            }
            else
            {
                originalPositions[screwTransform] = screwTransform.localPosition;
            }
            originalRotations[screwTransform] = screwTransform.localEulerAngles;
            
            // Buscar el perno hermano
            if (screwTransform.parent != null)
            {
                foreach (Transform sibling in screwTransform.parent)
                {
                    if (sibling != screwTransform && (sibling.name.Contains("Bolt_Nut") || sibling.name.Contains("Nut")))
                    {
                        if (useWorldPosition)
                        {
                            originalPositions[sibling] = sibling.position;
                        }
                        else
                        {
                            originalPositions[sibling] = sibling.localPosition;
                        }
                        break;
                    }
                }
            }
        }
        
        Debug.Log($"💾 Posiciones iniciales guardadas: {originalPositions.Count} objetos");
    }
}

