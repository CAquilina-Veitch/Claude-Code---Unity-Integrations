# Claude Code Project Instructions

## Unity Log Reader

This project includes a Unity log reader tool at `Assets/ClaudeCodeIntegration/LogReader/UnityConsoleReader.exe`.

You can check Unity console errors and warnings by running:
`./Assets/ClaudeCodeIntegration/LogReader/UnityConsoleReader.exe`

This tool will force recompilation and show recent Unity console errors/warnings.

## Unity Commands

This project has Unity integration commands available through `unity_commands.sh`. 

**IMPORTANT**: You must SOURCE the script first to load the functions:

```bash
source unity_commands.sh && unity_attach_script "GameObject" "SpinObject"
```

Available commands:
- `unity_add_component <object> <script>` - Add component to object
- `unity_attach_script <object> <script>` - Quick script attachment
- `unity_set_property <object> <component> <property> <value>` - Set component property
- `unity_create_object <name> [x,y,z]` - Create new GameObject
- `unity_list_objects` - List all GameObjects
- `unity_get_scene_info` - Get detailed scene info

Always use `source unity_commands.sh && [command]` format to ensure functions are loaded.

## Multi-Worker System

For complex projects, you can spawn multiple Claude Code workers:

```bash
source spawn_claude_workers.sh

# Spawn specialized workers
spawn_script_worker "PlayerController" "WASD movement, jumping, collision detection"
spawn_ui_worker "Create main menu with start/quit buttons"
spawn_scene_worker "Set up lighting and camera positioning"

# Or handle a complete complex project
handle_complex_project "Build a 3D platformer with collectibles and enemies"
```

Available worker types:
- `spawn_coordinator <goal>` - Project manager that spawns other workers
- `spawn_script_worker <name> <requirements>` - Script development specialist
- `spawn_ui_worker <task>` - UI development specialist  
- `spawn_scene_worker <task>` - Scene setup specialist
- `spawn_animation_worker <task>` - Animation/movement specialist
- `spawn_test_worker <task>` - Testing/debugging specialist

Each worker gets its own terminal window with full Unity command access.