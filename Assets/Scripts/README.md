Sistema de Minijuego - Ajuste de Tuercas

Descripción

Sistema completo para un minijuego educativo que entrena a los jugadores en el orden correcto de ajuste de tuercas en un patrón octagonal, siguiendo metodología de gamificación.

Arquitectura de Scripts
1. SequenceManager.cs
- Gestiona la secuencia correcta de apriete: `[1, 2, 3, 4, 5, 6, 7, 8]`
- Valida las selecciones del jugador
- Emite eventos para comunicación entre sistemas
- Ubicación: GameObject vacío en la escena 

2. Nut.cs
- Script para cada tuerca individual
- Detecta clicks mediante raycast
- Maneja estados: Idle, Highlighted, Selected, Correct, Wrong
- Ubicación: Componente en cada GameO
- Object `Bolt_Nut`

3. FeedbackManager.cs
- Maneja animaciones de ajuste (rotación)
- Cambia materiales según estado
- Efecto de focus/zoom al hacer click
- Sonidos de acierto/error (opcional)
- Ubicación: GameObject vacío en la escena (ej: "Managers")

4. GameManager.cs
- Controla el flujo principal del juego
- Gestiona tiempo límite e intentos
- Coordina todos los sistemas
- Maneja estados: Tutorial, Juego Activo, Completado, Game Over
- Ubicación: GameObject vacío en la escena
  
5. UIManager.cs
- Gestiona toda la interfaz de usuario
- Muestra progreso, tiempo, intentos
- Paneles de tutorial, completado y game over
- Mensajes de feedback temporales
- Ubicación: GameObject con Canvas (UI)

6. TutorialManager.cs
- Gestiona el tutorial inicial
- Resalta tuercas en secuencia
- Enseña la mecánica del juego
- Ubicación: GameObject vacío en la escena

Características Implementadas

Sistema de Raycast: Detección de clicks en tuercas  
Feedback Visual: Animaciones, cambios de material, resaltado   
Gamificación: Tiempo, intentos, progreso visual  
Tutorial Inicial: Enseña la mecánica paso a paso  
Validación de Secuencia: Verifica orden correcto  
UI Completa: Progreso, tiempo, intentos, mensajes  
✅ **Estados de Tuerca**: Idle, Highlighted, Correct, Wrong  

