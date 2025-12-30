# LiquidGlassShell

Un prototipo de shell de Windows minimalista con estética "Liquid Glass" (vidrio líquido) completamente funcional.

## Requisitos

- .NET 8.0 SDK o superior
- Visual Studio 2022/2025 o Visual Studio Code con extensión de C#
- Windows 10/11

## Estructura del Proyecto

```
LiquidGlassShell/
├── Assets/
│   └── Icons/          # Iconos para el dock (PNG recomendado)
├── Models/             # Modelos de datos
├── Services/           # Servicios (carga de dock, lanzamiento de apps)
├── ViewModels/         # ViewModels (MVVM)
├── App.xaml
├── App.xaml.cs
├── MainWindow.xaml
├── MainWindow.xaml.cs
├── dock.json           # Configuración de items del dock
└── LiquidGlassShell.csproj
```

## Configuración

### Agregar Iconos

1. Coloca archivos PNG en la carpeta `Assets/Icons/`
2. Actualiza `dock.json` con los nombres de archivo correctos
3. Los iconos deben ser cuadrados (recomendado: 256x256 o 512x512)

### Personalizar Dock Items

Edita `dock.json` para agregar, eliminar o modificar items del dock:

```json
{
  "items": [
    {
      "name": "Nombre del App",
      "id": "identificador-unico",
      "icon": "nombre-archivo.png",
      "applicationName": "nombre-ejecutable.exe",
      "executable": "C:\\Ruta\\Completa\\al\\ejecutable.exe",
      "arguments": "parametros opcionales",
      "workingDirectory": "C:\\Directorio\\Trabajo"
    }
  ]
}
```

**Campos disponibles:**
- `name`: Nombre mostrado en el tooltip (requerido)
- `id`: Identificador único (requerido)
- `icon`: Nombre del archivo PNG en Assets/Icons/ (requerido)
- `applicationName`: Nombre del ejecutable o protocolo (ej: "explorer.exe", "calc.exe", "ms-settings:")
- `executable`: Ruta completa al ejecutable (alternativa a applicationName)
- `arguments`: Argumentos opcionales para pasar al ejecutable
- `workingDirectory`: Directorio de trabajo opcional

**Nota:** Usa `applicationName` para aplicaciones del sistema (explorer.exe, calc.exe, etc.) o protocolos (ms-settings:). Usa `executable` para aplicaciones con rutas específicas. Al menos uno de estos dos campos debe estar presente.

## Compilación y Ejecución

### Visual Studio

1. Abre `LiquidGlassShell.sln`
2. Presiona F5 para compilar y ejecutar

### Línea de Comandos

```bash
dotnet build
dotnet run
```

## Controles

- **Escape**: Cierra la aplicación
- **Click en iconos**: Lanza las aplicaciones configuradas en `dock.json`

## Características

- **Dock funcional**: Lanza aplicaciones reales de Windows
- **Efecto Liquid Glass**: Blur y translucidez en el dock con sombras suaves
- **Animaciones sutiles**: Hover effects en los iconos (escala y opacidad)
- **Fullscreen**: Ventana sin bordes que ocupa toda la pantalla
- **Paleta de colores personalizada**: Colores exactos según especificación

## Notas Técnicas

- La ventana inicia en modo fullscreen sin bordes
- El dock utiliza efecto blur (Liquid Glass) para crear profundidad visual
- Los iconos tienen animaciones sutiles al hacer hover (escala 1.15x y cambio de opacidad)
- Los colores están definidos exactamente según la paleta especificada
- El dock flota sobre el contenido con efecto de vidrio translúcido

## Paleta de Colores

- Base: `#17313E`
- Secundario/Vidrio: `#415E72`
- Acento: `#C5B0CD`
- Neutro claro: `#F3E2D4`
