#!/bin/bash
cd '/mnt/c/Users/cody.av/Documents/Repositories/Development/Claude-Code---Unity-Integrations'

echo "=========================================="
echo "Claude Code Worker: Scene_Builder"
echo "=========================================="
echo "Task: Scene Setup Specialist"
echo "=========================================="
echo ""

# Load Unity command functions
source ./unity_commands.sh

echo ""
echo "Worker 'Scene_Builder' is ready!"
echo "Available Unity commands: unity_help"
echo ""

# Show initial context if provided
if [ -n "Scene Task: Create 3D tower defense scene with enemy paths, tower placement zones, and game environment

Focus on:
- Creating GameObjects and hierarchies
- Setting up lighting and cameras
- Organizing scene structure
- Adding environmental components" ]; then
echo "Initial Task:"
cat << 'TASK_EOF'
Scene Task: Create 3D tower defense scene with enemy paths, tower placement zones, and game environment

Focus on:
- Creating GameObjects and hierarchies
- Setting up lighting and cameras
- Organizing scene structure
- Adding environmental components
TASK_EOF
echo ""
echo "=========================================="
echo ""
fi

# Start Claude Code with worker context
claude --dangerously-skip-permissions "Scene Setup Specialist

Worker Name: Scene_Builder

Available Unity Commands:
- unity_list_objects
- unity_get_scene_info  
- unity_add_component <object> <script>
- unity_remove_component <object> <component>
- unity_set_property <object> <component> <property> <value>
- unity_create_object <name> [x,y,z]
- unity_delete_object <name>
- unity_set_parent <child> <parent>

Scene Task: Create 3D tower defense scene with enemy paths, tower placement zones, and game environment

Focus on:
- Creating GameObjects and hierarchies
- Setting up lighting and cameras
- Organizing scene structure
- Adding environmental components"
