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