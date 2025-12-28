# 📋 GUÍA PASO A PASO - CONFIGURACIÓN EN EL INSPECTOR

## 🎯 Situación Actual
Tienes los GameObjects `Bolt_1` a `Bolt_8` dentro de un GameObject vacío (padre).

---

## 📝 PASO 1: Crear la Estructura de Managers

### 1.1 Crear GameObject "Managers"
1. En la **Hierarchy**, click derecho → **Create Empty**
2. Renombrar a: `"Managers"`
3. Dejar en posición (0, 0, 0) - no importa la posición

### 1.2 Crear los Managers hijos
Dentro de `Managers`, crear 4 GameObjects vacíos (click derecho en Managers → Create Empty):

1. **GameManager**
   - Agregar componente: `Game Manager` (script)
   
2. **SequenceManager**
   - Agregar componente: `Sequence Manager` (script)
   
3. **FeedbackManager**
   - Agregar componente: `Feedback Manager` (script)
   
4. **TutorialManager**
   - Agregar componente: `Tutorial Manager` (script)

---

## 🔩 PASO 2: Configurar las Tuercas (Bolt_Nut)

### 2.1 Para cada Bolt_X (Bolt_1 a Bolt_8):

1. **Expandir** el GameObject `Bolt_X` en la Hierarchy
2. **Seleccionar** el hijo `Bolt_Nut` (o `Bolt_Nut (X)`)
3. En el **Inspector**, click en **Add Component**
4. Buscar y agregar: `Nut` (script)

### 2.2 Configurar cada Nut:

**⚠️ IMPORTANTE**: Los Nut IDs deben corresponder a las **posiciones físicas** en el diagrama, NO a los nombres de los GameObjects.

En el componente **Nut** del Inspector:

- **Nut ID**: Asignar según la posición física en el diagrama:
  - **Posición 1** (arriba, centro) → Nut ID = `1`
  - **Posición 2** (abajo, centro) → Nut ID = `2`
  - **Posición 3** (derecha, medio) → Nut ID = `3`
  - **Posición 4** (izquierda, medio) → Nut ID = `4`
  - **Posición 5** (arriba-derecha, diagonal) → Nut ID = `5`
  - **Posición 6** (abajo-izquierda, diagonal) → Nut ID = `6`
  - **Posición 7** (abajo-derecha, diagonal) → Nut ID = `7`
  - **Posición 8** (arriba-izquierda, diagonal) → Nut ID = `8`

**Ejemplo**: Si el GameObject se llama `Bolt_1` pero está físicamente en la posición 2 (abajo, centro), entonces debe tener Nut ID = `2`, NO `1`.

**Secuencia correcta según diagrama**: `2 → 5 → 3 → 6 → 4 → 7 → 8 → 1`

- **Nut Renderer**:
  - Si está vacío, arrastrar el componente **Mesh Renderer** del mismo GameObject
  - O buscar en el dropdown y seleccionar el Renderer

- **Nut Transform**:
  - Si está vacío, arrastrar el componente **Transform** del mismo GameObject
  - O dejar vacío (se asigna automáticamente)

### 2.3 Verificar Collider en cada Bolt_Nut:

1. Seleccionar `Bolt_Nut`
2. En el Inspector, verificar si tiene **Collider** (BoxCollider, MeshCollider, etc.)
3. **Si NO tiene Collider**:
   - Click en **Add Component**
   - Agregar **Mesh Collider** (si tiene mesh)
     - Marcar **Convex** = ✅
   - O agregar **Box Collider** (más simple)
4. **Importante**: El Collider NO debe estar marcado como **Is Trigger** ✅

**Repetir esto para los 8 Bolt_Nut (uno por cada Bolt_1 a Bolt_8)**

---

## ⚙️ PASO 3: Configurar SequenceManager

1. Seleccionar el GameObject **SequenceManager** (dentro de Managers)
2. En el Inspector, componente **Sequence Manager**:
   - **Correct Sequence**: 
     - Tamaño: `8`
     - Elemento 0: `1`
     - Elemento 1: `2`
     - Elemento 2: `3`
     - Elemento 3: `4`
     - Elemento 4: `5`
     - Elemento 5: `6`
     - Elemento 6: `7`
     - Elemento 7: `8`

---

## 🎨 PASO 4: Crear Materiales (Opcional pero Recomendado)

### 4.1 Crear Material para Tuercas Ajustadas:
1. En **Project**, click derecho en carpeta `Assets` → **Create** → **Material**
2. Nombrar: `"Material_Tightened"`
3. En el Inspector del material:
   - **Albedo**: Color verde claro (ej: RGB 0, 255, 0)
   - **Metallic**: 0.5
   - **Smoothness**: 0.7
   - (Opcional) Activar **Emission** y poner color verde suave

### 4.2 Crear Material para Resaltado (Tutorial):
1. Crear otro Material: `"Material_Highlighted"`
2. **Albedo**: Color amarillo (ej: RGB 255, 255, 0)
   - O usar el material original con **Emission** activado

### 4.3 Crear Material para Error:
1. Crear otro Material: `"Material_Wrong"`
2. **Albedo**: Color rojo (ej: RGB 255, 0, 0)

---

## 🎬 PASO 5: Configurar FeedbackManager

1. Seleccionar el GameObject **FeedbackManager** (dentro de Managers)
2. En el Inspector, componente **Feedback Manager**:

### 5.1 Materiales:
- **Tightened Material**: Arrastrar `Material_Tightened` (creado en Paso 4.1)
- **Highlighted Material**: Arrastrar `Material_Highlighted` (creado en Paso 4.2)
- **Wrong Material**: Arrastrar `Material_Wrong` (creado en Paso 4.3)

### 5.2 Animaciones (valores por defecto están bien, pero puedes ajustar):
- **Tighten Animation Duration**: `1` (segundos)
- **Tighten Rotation Angle**: `360` (grados)
- **Focus Scale**: `1.2`
- **Focus Duration**: `0.3` (segundos)

### 5.3 Efectos Visuales:
- **Highlight Intensity**: `1.5`
- **Highlight Color**: Amarillo (RGB 255, 255, 0)
- **Error Feedback Duration**: `0.5` (segundos)

### 5.4 Sonidos (Opcional):
- **Audio Source**: 
  - Si quieres sonidos, agregar componente **Audio Source** al FeedbackManager
  - Arrastrarlo al campo **Audio Source**
- **Correct Sound**: Arrastrar clip de audio (si tienes)
- **Wrong Sound**: Arrastrar clip de audio (si tienes)

---

## 🎮 PASO 6: Configurar GameManager

1. Seleccionar el GameObject **GameManager** (dentro de Managers)
2. En el Inspector, componente **Game Manager**:

### 6.1 Configuración del Juego:
- **Time Limit**: `120` (segundos) - o `0` para sin límite
- **Max Attempts**: `3` - o `0` para ilimitados

### 6.2 Referencias (Arrastrar desde Hierarchy):
- **Sequence Manager**: Arrastrar el GameObject `SequenceManager`
- **Feedback Manager**: Arrastrar el GameObject `FeedbackManager`
- **UI Manager**: Arrastrar el GameObject con UI (lo crearemos después)
- **Tutorial Manager**: Arrastrar el GameObject `TutorialManager`

---

## 📚 PASO 7: Configurar TutorialManager

1. Seleccionar el GameObject **TutorialManager** (dentro de Managers)
2. En el Inspector, componente **Tutorial Manager**:

### 7.1 Configuración:
- **Highlight Duration**: `2` (segundos)
- **Highlight Interval**: `0.5` (segundos)
- **Auto Start Tutorial**: ✅ (marcado)

### 7.2 Referencias:
- **Sequence Manager**: Arrastrar el GameObject `SequenceManager`
- **Feedback Manager**: Arrastrar el GameObject `FeedbackManager`

---

## 🖥️ PASO 8: Crear la UI (Interfaz de Usuario)

### 8.1 Crear Canvas:
1. En Hierarchy, click derecho → **UI** → **Canvas**
2. Se creará automáticamente un **EventSystem** también (dejarlo)

### 8.2 Crear GameObject para UIManager:
1. Dentro del **Canvas**, click derecho → **Create Empty**
2. Renombrar: `"UIManager"`
3. Agregar componente: `UI Manager` (script)

### 8.3 Crear Elementos de UI:

#### A) Texto de Progreso:
1. Click derecho en **Canvas** → **UI** → **Text - TextMeshPro**
   - Si pide importar TMP, aceptar
2. Renombrar: `"ProgressText"`
3. En el Inspector:
   - **Text**: `"Paso: 0/8"`
   - **Font Size**: `24`
   - **Alignment**: Centro
   - Posicionar arriba a la izquierda

#### B) Barra de Progreso:
1. Click derecho en **Canvas** → **UI** → **Slider**
2. Renombrar: `"ProgressBar"`
3. En el Inspector:
   - **Min Value**: `0`
   - **Max Value**: `1`
   - **Value**: `0`
   - Posicionar debajo del texto de progreso

#### C) Texto de Tiempo:
1. Click derecho en **Canvas** → **UI** → **Text - TextMeshPro**
2. Renombrar: `"TimeText"`
3. **Text**: `"Tiempo: 00:00"`
4. Posicionar arriba a la derecha

#### D) Texto de Intentos:
1. Click derecho en **Canvas** → **UI** → **Text - TextMeshPro**
2. Renombrar: `"AttemptsText"`
3. **Text**: `"Intentos: 0/3"`
4. Posicionar debajo del tiempo

#### E) Texto de Feedback (Mensajes):
1. Click derecho en **Canvas** → **UI** → **Text - TextMeshPro**
2. Renombrar: `"FeedbackText"`
3. **Text**: `""` (vacío)
4. **Font Size**: `30`
5. **Alignment**: Centro
6. Posicionar en el centro de la pantalla
7. **Inicialmente desactivado**: Click en el checkbox junto al nombre del GameObject

#### F) Panel de Tutorial:
1. Click derecho en **Canvas** → **UI** → **Panel**
2. Renombrar: `"TutorialPanel"`
3. Agregar hijo: Click derecho en TutorialPanel → **UI** → **Text - TextMeshPro**
   - Renombrar: `"TutorialText"`
   - **Text**: `"Tutorial: Observa la secuencia de tuercas resaltadas. Luego haz click en el orden correcto."`
   - **Font Size**: `20`
   - **Alignment**: Centro
4. Agregar botón: Click derecho en TutorialPanel → **UI** → **Button - TextMeshPro**
   - Renombrar: `"TutorialSkipButton"`
   - En el hijo Text, cambiar texto a: `"Comenzar"`
5. **Inicialmente activado** (para que aparezca al inicio)

#### G) Panel de Barra de Carga (Ajuste):
1. Click derecho en **Canvas** → **UI** → **Panel**
2. Renombrar: `"TighteningProgressPanel"`
3. Configurar el Panel:
   - **Width**: `200`
   - **Height**: `60`
   - **Anchor**: Centro (puedes ajustarlo después)
   - **Inicialmente desactivado**: ✅ (marcar checkbox)
4. Agregar hijos dentro del Panel:
   
   a) **Slider** (Barra de progreso):
   - Click derecho en TighteningProgressPanel → **UI** → **Slider**
   - Renombrar: `"TighteningProgressBar"`
   - **Min Value**: `0`
   - **Max Value**: `1`
   - **Value**: `0`
   - **Width**: `180`
   - **Height**: `20`
   - Posicionar en el centro del panel
   
   b) **Text - TextMeshPro** (Texto opcional):
   - Click derecho en TighteningProgressPanel → **UI** → **Text - TextMeshPro**
   - Renombrar: `"TighteningProgressText"`
   - **Text**: `"Ajustando... 0%"`
   - **Font Size**: `16`
   - **Alignment**: Centro
   - Posicionar arriba o debajo del slider

#### H) Panel de Completado:
1. Click derecho en **Canvas** → **UI** → **Panel**
2. Renombrar: `"GameCompletePanel"`
3. Agregar hijos:
   - **Text - TextMeshPro**: `"¡Completado!"` (título)
   - **Text - TextMeshPro**: Renombrar `"FinalTimeText"`, Text: `"Tiempo: 00:00"`
   - **Text - TextMeshPro**: Renombrar `"FinalAttemptsText"`, Text: `"Intentos: 0"`
   - **Button**: Renombrar `"RestartButton"`, Text: `"Reiniciar"`
4. **Inicialmente desactivado**

#### H) Panel de Game Over:
1. Click derecho en **Canvas** → **UI** → **Panel**
2. Renombrar: `"GameOverPanel"`
3. Agregar hijos:
   - **Text - TextMeshPro**: `"¡Game Over!"`
   - **Button**: Renombrar `"RestartButton2"`, Text: `"Reiniciar"`
4. **Inicialmente desactivado**

### 8.4 Configurar UIManager:
1. Seleccionar el GameObject **UIManager** (dentro de Canvas)
2. En el Inspector, componente **UI Manager**:

#### Arrastrar Referencias:

**Paneles:**
- **Tutorial Panel**: Arrastrar `TutorialPanel`
- **Game Complete Panel**: Arrastrar `GameCompletePanel`
- **Game Over Panel**: Arrastrar `GameOverPanel`

**Textos de Progreso:**
- **Progress Text**: Arrastrar `ProgressText`
- **Progress Bar**: Arrastrar `ProgressBar`

**Textos de Información:**
- **Time Text**: Arrastrar `TimeText`
- **Attempts Text**: Arrastrar `AttemptsText`
- **Feedback Text**: Arrastrar `FeedbackText`

**Panel de Completado:**
- **Final Time Text**: Arrastrar `FinalTimeText` (dentro de GameCompletePanel)
- **Final Attempts Text**: Arrastrar `FinalAttemptsText` (dentro de GameCompletePanel)

**Botones:**
- **Restart Button**: Arrastrar el botón de `GameCompletePanel`
- **Tutorial Skip Button**: Arrastrar `TutorialSkipButton` (dentro de TutorialPanel)

**Barra de Carga (Ajuste):**
- **Tightening Progress Panel**: Arrastrar `TighteningProgressPanel`
- **Tightening Progress Bar**: Arrastrar `TighteningProgressBar` (Slider)
- **Tightening Progress Text**: Arrastrar `TighteningProgressText` (opcional)
- **World Space Canvas**: (Opcional) Si creaste un Canvas con Render Mode = World Space para mostrar la barra cerca de la tuerca en 3D, arrástralo aquí

---

## ✅ PASO 9: Verificar Configuración

### Checklist Final:

- [ ] ✅ Managers creados (GameManager, SequenceManager, FeedbackManager, TutorialManager)
- [ ] ✅ Cada Bolt_Nut tiene componente Nut con ID correcto (1-8)
- [ ] ✅ Cada Bolt_Nut tiene Collider (no trigger)
- [ ] ✅ SequenceManager tiene secuencia [1,2,3,4,5,6,7,8]
- [ ] ✅ FeedbackManager tiene materiales asignados
- [ ] ✅ GameManager tiene todas las referencias asignadas
- [ ] ✅ TutorialManager tiene referencias asignadas
- [ ] ✅ Canvas creado con todos los elementos UI
- [ ] ✅ UIManager tiene todas las referencias asignadas
- [ ] ✅ Cámara tiene tag "MainCamera"

---

## 🚀 PASO 10: Probar el Juego

1. Click en **Play** ▶️
2. Deberías ver:
   - Panel de tutorial aparecer
   - Las tuercas resaltarse en secuencia
   - Al hacer click en "Comenzar", iniciar el juego
   - Al hacer click en tuercas, deberían responder

---

## 🐛 Troubleshooting

### Las tuercas no detectan clicks:
- Verificar que cada Bolt_Nut tenga Collider
- Verificar que Collider NO esté marcado como "Is Trigger"
- Verificar que la cámara tenga tag "MainCamera"

### El tutorial no aparece:
- Verificar que GameManager tenga referencia a TutorialManager
- Verificar que UIManager tenga TutorialPanel asignado

### Errores en consola:
- Verificar que todas las referencias estén asignadas en los managers
- Verificar que los scripts estén compilados sin errores

---

## 📝 Notas Adicionales

- Si prefieres, puedes usar el **NutSetupHelper** para configurar las tuercas automáticamente (ver README.md)
- Los materiales son opcionales, pero mejoran mucho la experiencia visual
- Los sonidos son completamente opcionales

¡Listo! 🎉

