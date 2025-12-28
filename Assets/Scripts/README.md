# 🎮 Sistema de Minijuego - Ajuste de Tuercas

## 📋 Descripción

Sistema completo para un minijuego educativo que entrena a los jugadores en el orden correcto de ajuste de tuercas en un patrón octagonal, siguiendo metodología de gamificación.

## 🏗️ Arquitectura de Scripts

### 1. **SequenceManager.cs**
- Gestiona la secuencia correcta de apriete: `[1, 2, 3, 4, 5, 6, 7, 8]`
- Valida las selecciones del jugador
- Emite eventos para comunicación entre sistemas
- **Ubicación**: GameObject vacío en la escena (ej: "Managers")

### 2. **Nut.cs**
- Script para cada tuerca individual
- Detecta clicks mediante raycast
- Maneja estados: Idle, Highlighted, Selected, Correct, Wrong
- **Ubicación**: Componente en cada GameObject `Bolt_Nut`

### 3. **FeedbackManager.cs**
- Maneja animaciones de ajuste (rotación)
- Cambia materiales según estado
- Efecto de focus/zoom al hacer click
- Sonidos de acierto/error (opcional)
- **Ubicación**: GameObject vacío en la escena (ej: "Managers")

### 4. **GameManager.cs**
- Controla el flujo principal del juego
- Gestiona tiempo límite e intentos
- Coordina todos los sistemas
- Maneja estados: Tutorial, Juego Activo, Completado, Game Over
- **Ubicación**: GameObject vacío en la escena (ej: "Managers")

### 5. **UIManager.cs**
- Gestiona toda la interfaz de usuario
- Muestra progreso, tiempo, intentos
- Paneles de tutorial, completado y game over
- Mensajes de feedback temporales
- **Ubicación**: GameObject con Canvas (UI)

### 6. **TutorialManager.cs**
- Gestiona el tutorial inicial
- Resalta tuercas en secuencia
- Enseña la mecánica del juego
- **Ubicación**: GameObject vacío en la escena (ej: "Managers")

### 7. **NutSetupHelper.cs** (Opcional)
- Script helper para configurar rápidamente todas las tuercas
- Usa el menú contextual (click derecho) para ejecutar

## 🚀 Configuración Paso a Paso

### Paso 1: Estructura de la Escena

1. Crear un GameObject vacío llamado `"Managers"` como padre
2. Crear hijos dentro de `Managers`:
   - `GameManager` (con script GameManager)
   - `SequenceManager` (con script SequenceManager)
   - `FeedbackManager` (con script FeedbackManager)
   - `TutorialManager` (con script TutorialManager)

### Paso 2: Configurar las Tuercas

**Opción A - Manual:**
1. Para cada `Bolt_X` (Bolt_1 a Bolt_8):
   - Seleccionar el hijo `Bolt_Nut`
   - Agregar componente `Nut`
   - Asignar `nutID` (1-8) según el nombre del padre
   - Asegurar que tenga un `Collider` (BoxCollider o MeshCollider)
   - Asignar `Renderer` en el inspector del script Nut

**Opción B - Automática (Recomendada):**
1. Crear un GameObject vacío
2. Agregar componente `NutSetupHelper`
3. Click derecho en el componente → `"Buscar y Asignar Bolt Parents"`
4. Click derecho → `"Configurar Todas las Tuercas"`

### Paso 3: Configurar SequenceManager

1. En el inspector, verificar que `Correct Sequence` sea: `1, 2, 3, 4, 5, 6, 7, 8`

### Paso 4: Configurar FeedbackManager

1. **Materiales** (crear si no existen):
   - `Tightened Material`: Material verde/brillante para tuercas ajustadas
   - `Highlighted Material`: Material amarillo para resaltado (tutorial)
   - `Wrong Material`: Material rojo para errores

2. **Animaciones**: Ajustar duración y ángulo según preferencia

3. **Sonidos** (opcional):
   - Asignar `AudioSource`
   - Asignar clips de audio para acierto y error

### Paso 5: Configurar GameManager

1. Asignar referencias a:
   - `SequenceManager`
   - `FeedbackManager`
   - `UIManager`
   - `TutorialManager`

2. Configurar:
   - `Time Limit`: Tiempo límite en segundos (0 = sin límite)
   - `Max Attempts`: Intentos máximos (0 = ilimitados)

### Paso 6: Configurar UIManager

1. Crear un `Canvas` en la escena
2. Agregar script `UIManager` al Canvas o a un GameObject hijo
3. Crear UI elements:
   - **Texto de Progreso**: `TextMeshProUGUI` para "Paso: X/8"
   - **Barra de Progreso**: `Slider` para progreso visual
   - **Texto de Tiempo**: `TextMeshProUGUI` para tiempo
   - **Texto de Intentos**: `TextMeshProUGUI` para intentos
   - **Texto de Feedback**: `TextMeshProUGUI` para mensajes temporales
   - **Paneles**: 
     - `TutorialPanel` (con botón "Comenzar")
     - `GameCompletePanel` (con tiempo final, intentos, botón "Reiniciar")
     - `GameOverPanel` (con botón "Reiniciar")

4. Asignar todas las referencias en el inspector del UIManager

### Paso 7: Configurar TutorialManager

1. Asignar referencias a `SequenceManager` y `FeedbackManager`
2. Ajustar duraciones según preferencia

## 🎯 Características Implementadas

✅ **Sistema de Raycast**: Detección de clicks en tuercas  
✅ **Feedback Visual**: Animaciones, cambios de material, resaltado  
✅ **Focus/Zoom**: Efecto de enfoque al hacer click  
✅ **Gamificación**: Tiempo, intentos, progreso visual  
✅ **Tutorial Inicial**: Enseña la mecánica paso a paso  
✅ **Validación de Secuencia**: Verifica orden correcto  
✅ **UI Completa**: Progreso, tiempo, intentos, mensajes  
✅ **Estados de Tuerca**: Idle, Highlighted, Correct, Wrong  

## 🎨 Materiales Sugeridos

Para mejores resultados, crear materiales con:
- **Tightened Material**: Color verde con emisión suave
- **Highlighted Material**: Color amarillo con emisión más intensa
- **Wrong Material**: Color rojo temporal

## 🔧 Troubleshooting

### Las tuercas no detectan clicks:
- Verificar que cada `Bolt_Nut` tenga un `Collider`
- Verificar que el `Collider` no esté marcado como `Is Trigger`
- Verificar que la cámara tenga tag `MainCamera`

### El tutorial no aparece:
- Verificar que `GameManager` tenga referencia a `TutorialManager`
- Verificar que `UIManager` tenga el `TutorialPanel` asignado

### Las animaciones no funcionan:
- Verificar que `FeedbackManager` tenga referencias correctas
- Verificar que los materiales estén asignados

## 📝 Notas Adicionales

- El sistema usa eventos para comunicación entre scripts (desacoplado)
- Todos los scripts tienen documentación XML
- El código sigue principios SOLID y es fácil de mantener
- Se puede extender fácilmente con más características

## 🎮 Flujo del Juego

1. **Inicio**: Tutorial automático muestra la secuencia
2. **Tutorial**: Resalta cada tuerca en orden
3. **Juego**: Jugador hace click en tuercas en orden correcto
4. **Feedback**: Acierto/error con animaciones y sonidos
5. **Completado**: Muestra tiempo e intentos finales
6. **Reinicio**: Opción para volver a jugar

---

**¡Listo para usar!** 🚀

