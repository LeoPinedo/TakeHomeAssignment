using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona la secuencia correcta de apriete de tuercas y valida las selecciones del jugador.
/// </summary>
public class SequenceManager : MonoBehaviour
{
    [Header("Secuencia Correcta")]
    [Tooltip("Orden correcto de apriete: 2, 5, 3, 6, 4, 7, 8, 1 (basado en Nut IDs en sus posiciones físicas)")]
    [SerializeField] private int[] correctSequence = { 1, 2, 3, 4, 5, 6, 7, 8 };
    
    private int[] originalSequence; // Guardar la secuencia original
    
    private int currentStep = 0;
    private int totalSteps;
    private int tutorialLimit = -1; // -1 = sin límite, número = límite para tutorial
    
    // Eventos para comunicación entre scripts
    public System.Action<int> OnCorrectNutSelected;
    public System.Action<int> OnWrongNutSelected;
    public System.Action OnSequenceCompleted;
    
    private void Awake()
    {
        // Generar secuencia automáticamente desde los Nut IDs en la escena
        GenerateSequenceFromNutIDs();
        
        totalSteps = correctSequence.Length;
        // Guardar la secuencia original SIEMPRE al inicio
        originalSequence = new int[correctSequence.Length];
        System.Array.Copy(correctSequence, originalSequence, correctSequence.Length);
        Debug.Log($"💾 Secuencia original guardada en Awake: [{string.Join(", ", originalSequence)}]");
    }
    
    /// <summary>
    /// Verifica que todos los Nut IDs del 1 al 8 existan en la escena.
    /// La secuencia correcta es: 2, 5, 3, 6, 4, 7, 8, 1 (basada en las posiciones físicas de cada Nut ID).
    /// </summary>
    private void GenerateSequenceFromNutIDs()
    {
        // Buscar todos los Nut en la escena
        Nut[] allNuts = FindObjectsOfType<Nut>(true); // Incluir inactivos
        
        if (allNuts == null || allNuts.Length == 0)
        {
            Debug.LogWarning("⚠️ No se encontraron Nut en la escena. Usando secuencia por defecto.");
            return;
        }
        
        // Crear un diccionario para mapear Nut ID -> Nut
        System.Collections.Generic.Dictionary<int, Nut> nutDictionary = new System.Collections.Generic.Dictionary<int, Nut>();
        foreach (Nut nut in allNuts)
        {
            if (nut != null && !nutDictionary.ContainsKey(nut.NutID))
            {
                nutDictionary[nut.NutID] = nut;
            }
        }
        
        // Verificar que todos los Nut IDs del 1 al 8 existan
        System.Collections.Generic.List<int> missingNuts = new System.Collections.Generic.List<int>();
        for (int i = 1; i <= 8; i++)
        {
            if (!nutDictionary.ContainsKey(i))
            {
                missingNuts.Add(i);
            }
        }
        
        if (missingNuts.Count > 0)
        {
            Debug.LogWarning($"⚠️ Faltan Nut IDs en la escena: [{string.Join(", ", missingNuts)}]");
        }
        else
        {
            Debug.Log("✅ Todos los Nut IDs del 1 al 8 encontrados en la escena");
        }
        
        // La secuencia correcta es del 1 al 8 en orden secuencial
        // Esto significa que primero se valida el Nut ID 1, luego el 2, luego el 3, etc.
        // NO cambiar esta secuencia - se usa tal cual está configurada en el Inspector
        // correctSequence ya está configurada en el Inspector, solo verificamos que los Nut IDs existan
        
        Debug.Log($"✅ Secuencia configurada: [{string.Join(", ", correctSequence)}]");
        Debug.Log($"📋 Orden de validación: {correctSequence[0]} → {correctSequence[1]} → {correctSequence[2]} → {correctSequence[3]} → {correctSequence[4]} → {correctSequence[5]} → {correctSequence[6]} → {correctSequence[7]}");
    }
    
    /// <summary>
    /// Valida si la tuerca seleccionada es la correcta en la secuencia.
    /// </summary>
    /// <param name="nutID">ID de la tuerca seleccionada (1-8)</param>
    /// <returns>True si es correcta, False si es incorrecta</returns>
    public bool ValidateNutSelection(int nutID)
    {
        // Si hay un límite de tutorial, verificar que no se haya alcanzado
        int maxSteps = (tutorialLimit > 0) ? tutorialLimit : totalSteps;
        
        if (currentStep >= maxSteps)
        {
            Debug.Log($"⚠️ Secuencia ya completada. Paso actual: {currentStep}, Máximo: {maxSteps}");
            return false; // Secuencia ya completada (o límite de tutorial alcanzado)
        }
        
        int expectedNut = correctSequence[currentStep];
        
        Debug.Log($"🔍 Validando: Tuerca seleccionada = {nutID}, Esperada = {expectedNut}, Paso actual = {currentStep + 1}/{maxSteps}, Secuencia = [{string.Join(", ", correctSequence)}]");
        
        if (nutID == expectedNut)
        {
            currentStep++;
            Debug.Log($"✅ Tuerca {nutID} correcta! Avanzando al paso {currentStep + 1}/{maxSteps}");
            OnCorrectNutSelected?.Invoke(nutID);
            
            // Verificar si se completó la secuencia (o el tutorial)
            if (currentStep >= maxSteps)
            {
                if (tutorialLimit > 0 && currentStep >= tutorialLimit)
                {
                    // Tutorial completado, pero no la secuencia completa
                    // No invocar OnSequenceCompleted todavía
                    Debug.Log("🎓 Tutorial completado!");
                }
                else if (currentStep >= totalSteps)
                {
                    // Secuencia completa terminada
                    Debug.Log("🎉 Secuencia completa terminada!");
                    OnSequenceCompleted?.Invoke();
                }
            }
            
            return true;
        }
        else
        {
            Debug.Log($"❌ Tuerca {nutID} incorrecta. Se esperaba {expectedNut} en el paso {currentStep + 1}");
            OnWrongNutSelected?.Invoke(nutID);
            return false;
        }
    }
    
    /// <summary>
    /// Obtiene el ID de la siguiente tuerca correcta en la secuencia.
    /// </summary>
    public int GetNextCorrectNutID()
    {
        if (currentStep >= totalSteps)
            return -1; // Secuencia completada
        
        return correctSequence[currentStep];
    }
    
    /// <summary>
    /// Obtiene la secuencia correcta completa.
    /// </summary>
    public int[] GetCorrectSequence()
    {
        return correctSequence;
    }
    
    /// <summary>
    /// Obtiene el paso actual (0-based).
    /// </summary>
    public int GetCurrentStep()
    {
        return currentStep;
    }
    
    /// <summary>
    /// Obtiene el total de pasos.
    /// </summary>
    public int GetTotalSteps()
    {
        return totalSteps;
    }
    
    /// <summary>
    /// Obtiene el progreso como porcentaje (0-1).
    /// </summary>
    public float GetProgress()
    {
        return (float)currentStep / totalSteps;
    }
    
    /// <summary>
    /// Reinicia la secuencia (útil para reintentar).
    /// </summary>
    public void ResetSequence()
    {
        currentStep = 0;
        tutorialLimit = -1; // Quitar límite de tutorial
        
        // Restaurar la secuencia original
        if (originalSequence != null && originalSequence.Length > 0)
        {
            correctSequence = new int[originalSequence.Length];
            System.Array.Copy(originalSequence, correctSequence, originalSequence.Length);
            totalSteps = correctSequence.Length;
            Debug.Log($"🔄 Secuencia restaurada: [{string.Join(", ", correctSequence)}], Total pasos: {totalSteps}");
        }
        else
        {
            Debug.LogWarning("⚠️ No hay secuencia original guardada para restaurar!");
        }
    }
    
    /// <summary>
    /// Establece un límite para el tutorial (solo validar las primeras N tuercas).
    /// </summary>
    public void SetTutorialLimit(int limit)
    {
        tutorialLimit = limit;
        currentStep = 0; // Resetear al cambiar el límite
    }
    
    /// <summary>
    /// Establece una secuencia personalizada para el tutorial (tuercas aleatorias).
    /// </summary>
    public void SetTutorialSequence(int[] sequence)
    {
        if (sequence != null && sequence.Length > 0)
        {
            // Crear una copia temporal de la secuencia para el tutorial
            int[] tempSequence = new int[sequence.Length];
            System.Array.Copy(sequence, tempSequence, sequence.Length);
            
            // Reemplazar temporalmente la secuencia correcta
            int[] originalSequence = correctSequence;
            correctSequence = tempSequence;
            totalSteps = tempSequence.Length;
            currentStep = 0;
            
            Debug.Log($"Secuencia del tutorial establecida: {string.Join(", ", tempSequence)}");
        }
    }
    
    /// <summary>
    /// Quita el límite de tutorial (vuelve a la secuencia completa).
    /// </summary>
    public void RemoveTutorialLimit()
    {
        tutorialLimit = -1;
        // Restaurar la secuencia original guardada
        if (originalSequence != null && originalSequence.Length > 0)
        {
            correctSequence = new int[originalSequence.Length];
            System.Array.Copy(originalSequence, correctSequence, originalSequence.Length);
            totalSteps = correctSequence.Length;
            Debug.Log($"✅ Límite de tutorial removido. Secuencia restaurada: [{string.Join(", ", correctSequence)}]");
        }
        else
        {
            Debug.LogWarning("⚠️ No hay secuencia original guardada. Usando secuencia actual.");
        }
    }
    
    /// <summary>
    /// Verifica si la secuencia está completada.
    /// </summary>
    public bool IsSequenceCompleted()
    {
        return currentStep >= totalSteps;
    }
}

