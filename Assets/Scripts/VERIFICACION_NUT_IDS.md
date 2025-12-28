# 🔍 GUÍA DE VERIFICACIÓN - Nut IDs

## ⚠️ Problema Común

Los Nut IDs deben corresponder a las **posiciones físicas** en el diagrama, NO a los nombres de los GameObjects.

## 📋 Cómo Verificar

### Paso 1: Identificar las Posiciones Físicas

Según el diagrama, las posiciones son:

1. **Posición 1**: Arriba, centro
2. **Posición 2**: Abajo, centro
3. **Posición 3**: Derecha, medio (horizontal)
4. **Posición 4**: Izquierda, medio (horizontal)
5. **Posición 5**: Arriba-derecha, diagonal
6. **Posición 6**: Abajo-izquierda, diagonal
7. **Posición 7**: Abajo-derecha, diagonal
8. **Posición 8**: Arriba-izquierda, diagonal

### Paso 2: Verificar en Unity

1. En la **Scene View**, identifica cada perno según su posición física
2. Selecciona el GameObject `Bolt_Nut` dentro de cada `Bolt_X`
3. En el Inspector, verifica el **Nut ID** del componente `Nut`
4. El Nut ID debe coincidir con la posición física (1-8)

### Paso 3: Corregir si es Necesario

Si un GameObject `Bolt_X` tiene un Nut ID incorrecto:

1. Selecciona el `Bolt_Nut` hijo
2. En el componente `Nut`, cambia el **Nut ID** al valor correcto según su posición física
3. Repite para todos los pernos

## ✅ Secuencia Correcta

La secuencia correcta según el diagrama es: **2 → 5 → 3 → 6 → 4 → 7 → 8 → 1**

Esto significa que el primer perno a ajustar es el que está en la **posición 2** (abajo, centro), luego el de la **posición 5** (arriba-derecha), etc.

## 🐛 Debug

Si aún no funciona, revisa la consola de Unity. Deberías ver logs como:

```
🔍 Validando: Tuerca seleccionada = X, Esperada = Y, Paso actual = Z/8
```

- **Tuerca seleccionada**: El Nut ID del perno que clickeaste
- **Esperada**: El Nut ID que debería ser según la secuencia
- Si no coinciden, el Nut ID está mal asignado

## 📝 Ejemplo

Si tienes:
- `Bolt_1` físicamente en la posición 2 (abajo, centro) → Nut ID debe ser `2`
- `Bolt_2` físicamente en la posición 5 (arriba-derecha) → Nut ID debe ser `5`
- `Bolt_3` físicamente en la posición 3 (derecha, medio) → Nut ID debe ser `3`
- etc.

**NO importa el nombre del GameObject**, solo importa la **posición física** en el diagrama.

