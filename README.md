# Blessed Classes Mod

A Vintage Story class overhaul mod to promote interdependence between player classes.

## Project Structure

```
BlessedClasses/
├── src/                          # C# source code
│   ├── BlessedClassesModSystem.cs    # Main mod entry point
│   ├── Blocks/                       # Custom block classes
│   ├── BlockBehaviors/               # Reusable block behaviors
│   ├── CollectibleBehaviors/         # Item/collectible behaviors
│   ├── EntityBehaviors/              # Player/entity trait behaviors
│   ├── Patches/                      # Harmony patches (modify game code)
│   ├── Merchant/                     # Trading system logic
│   └── Diagnostics/                  # Debug logging tools
├── assets/                       # Game content (JSON, textures, models)
│   └── blessedclasses/
│       ├── blocktypes/               # Block definitions (JSON)
│       ├── itemtypes/                # Item definitions (JSON)
│       ├── config/                   # Class & trait configs
│       ├── recipes/                  # Crafting recipes
│       ├── patches/                  # JSON patches for game content
│       ├── shapes/                   # 3D models
│       ├── textures/                 # PNG textures
│       └── lang/                     # Localization (en.json)
├── build/CakeBuild/              # Build automation system
├── BlessedClasses.csproj         # C# project configuration
├── BlessedClasses.sln            # Visual Studio solution
└── modinfo.json                  # Mod metadata (version, dependencies)
```

## Getting Started

### Prerequisites

- .NET 8.0 SDK: https://dotnet.microsoft.com/download
- Vintage Story

### Building the Mod

**Linux/Mac:**
```bash
chmod +x build.sh
./build.sh
```

**Windows:**
```powershell
.\build.ps1
```

This will:
1. Validate all JSON files for syntax errors
2. Compile the C# code
3. Package everything into `Releases/blessedclasses_{version}.zip`

### Testing Your Changes

1. Edit code in `src/` or assets in `assets/`
2. Run the build script
3. Copy the mod from `Releases/` to your Vintage Story `Mods/` folder
4. Test in-game
5. Commit your changes in VS Code using the `Source Control` window.

### VS Code Setup

If you're using VS Code, install these extensions:
- C# Dev Kit (ms-dotnettools.csdevkit)
- C# (ms-dotnettools.csharp)

Press **F5** to build and launch the game for debugging.

## Key Files

- **[modinfo.json](modinfo.json)** - Mod metadata (version, name, dependencies)
- **[src/BlessedClassesModSystem.cs](src/BlessedClassesModSystem.cs)** - Main entry point, registers all custom classes
- **[assets/blessedclasses/config/characterclasses.json](assets/blessedclasses/config/characterclasses.json)** - Character class definitions
- **[assets/blessedclasses/config/traits.json](assets/blessedclasses/config/traits.json)** - Trait system definitions

## Resources

- [Vintage Story Wiki](https://wiki.vintagestory.at/index.php/Modding:Advanced)
- [API Documentation](https://apidocs.vintagestory.at/)
- [Official VS Mod Examples Repository](https://github.com/anegostudios/vsmodexamples)

## Example Tasks

### Adding a New Block
1. Create C# class in `src/Blocks/BlockMyBlock.cs`
2. Register it in `BlessedClassesModSystem.cs`: `api.RegisterBlockClass("blessedclasses.myblock", typeof(BlockMyBlock));`
3. Add JSON definition in `assets/blessedclasses/blocktypes/myblock.json`

### Adding a New Trait Behavior
1. Create C# class in `src/EntityBehaviors/MyTraitBehavior.cs`
2. Register it in `BlessedClassesModSystem.cs`
3. Add trait definition to `assets/blessedclasses/config/traits.json`
4. Create a patch in `assets/blessedclasses/patches/` to apply the behavior

### Debugging
- Use `Mod.Logger.Notification("message")` for logging
- Check Vintage Story logs: `%APPDATA%/VintagestoryData/Logs/` (Windows) or `~/.config/VintagestoryData/Logs/` (Linux)

## Project Conventions

- **Class Registration**: Always use lowercase: `"blessedclasses.classname"`
- **File Names**: Match class names (e.g., `BlockCarvedCrock.cs`)
- **Namespaces**: Not required but can help organize larger projects
- **Always call base methods**: When overriding, call `base.MethodName()` first
