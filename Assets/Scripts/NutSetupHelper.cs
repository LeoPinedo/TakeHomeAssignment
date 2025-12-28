using UnityEngine;

/// <summary>
/// Script helper para configurar rápidamente las tuercas en el inspector.
/// Coloca este script en un GameObject vacío y úsalo para configurar todas las tuercas a la vez.
/// </summary>
public class NutSetupHelper : MonoBehaviour
{
    [Header("Configuración Rápida")]
    [Tooltip("Arrastra aquí todos los GameObjects Bolt_X (padres)")]
    [SerializeField] private GameObject[] boltParents;
    
    [ContextMenu("Configurar Todas las Tuercas")]
    public void SetupAllNuts()
    {
        if (boltParents == null || boltParents.Length == 0)
        {
            Debug.LogWarning("No hay Bolt Parents asignados. Buscando automáticamente...");
            
            // Buscar todos los objetos que empiezan con "Bolt_"
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            System.Collections.Generic.List<GameObject> foundBolts = new System.Collections.Generic.List<GameObject>();
            
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.StartsWith("Bolt_"))
                {
                    foundBolts.Add(obj);
                }
            }
            
            boltParents = foundBolts.ToArray();
        }
        
        int configuredCount = 0;
        
        foreach (GameObject boltParent in boltParents)
        {
            if (boltParent == null) continue;
            
            // Buscar el hijo Bolt_Nut
            Transform nutTransform = boltParent.transform.Find("Bolt_Nut");
            if (nutTransform == null)
            {
                // Intentar buscar con números
                for (int i = 0; i < 10; i++)
                {
                    nutTransform = boltParent.transform.Find($"Bolt_Nut ({i})");
                    if (nutTransform != null) break;
                }
            }
            
            if (nutTransform == null)
            {
                Debug.LogWarning($"No se encontró Bolt_Nut en {boltParent.name}");
                continue;
            }
            
            // Obtener o agregar componente Nut
            Nut nutComponent = nutTransform.GetComponent<Nut>();
            if (nutComponent == null)
            {
                nutComponent = nutTransform.gameObject.AddComponent<Nut>();
            }
            
            // Obtener o agregar Collider si no existe
            Collider collider = nutTransform.GetComponent<Collider>();
            if (collider == null)
            {
                // Intentar agregar un BoxCollider o MeshCollider
                MeshFilter meshFilter = nutTransform.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    MeshCollider meshCollider = nutTransform.gameObject.AddComponent<MeshCollider>();
                    meshCollider.convex = true;
                }
                else
                {
                    BoxCollider boxCollider = nutTransform.gameObject.AddComponent<BoxCollider>();
                }
            }
            
            // Extraer ID del nombre del padre (Bolt_1, Bolt_2, etc.)
            string parentName = boltParent.name;
            if (parentName.StartsWith("Bolt_"))
            {
                string idString = parentName.Substring(5); // "Bolt_".Length = 5
                if (int.TryParse(idString, out int nutID))
                {
                    // Establecer el nutID usando reflexión (solo funciona en editor)
                    #if UNITY_EDITOR
                    var serializedObject = new UnityEditor.SerializedObject(nutComponent);
                    var nutIDProperty = serializedObject.FindProperty("nutID");
                    if (nutIDProperty != null)
                    {
                        nutIDProperty.intValue = nutID;
                        serializedObject.ApplyModifiedProperties();
                    }
                    #else
                    // En runtime, el ID debe configurarse manualmente en el inspector
                    Debug.LogWarning($"Configura manualmente el nutID = {nutID} en el inspector para {nutTransform.name}");
                    #endif
                    
                    configuredCount++;
                    Debug.Log($"Configurada tuerca {nutID} en {boltParent.name}");
                }
            }
        }
        
        Debug.Log($"Configuración completada: {configuredCount} tuercas configuradas.");
    }
    
    [ContextMenu("Buscar y Asignar Bolt Parents")]
    public void FindAndAssignBoltParents()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        System.Collections.Generic.List<GameObject> foundBolts = new System.Collections.Generic.List<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.StartsWith("Bolt_") && obj.transform.parent == null)
            {
                foundBolts.Add(obj);
            }
        }
        
        boltParents = foundBolts.ToArray();
        Debug.Log($"Encontrados {boltParents.Length} Bolt Parents");
    }
}

