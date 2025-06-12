#!/bin/bash
cd '/mnt/c/Users/cody.av/Documents/Repositories/Development/Claude-Code---Unity-Integrations'

echo "=========================================="
echo "Claude Code Worker: UI_Designer"
echo "=========================================="
echo "Task: UI Development Specialist"
echo "=========================================="
echo ""

# Load Unity command functions
source ./unity_commands.sh

echo ""
echo "Worker 'UI_Designer' is ready!"
echo "Available Unity commands: unity_help"
echo ""

# Show initial context if provided
if [ -n "UI Task: Create tower defense UI with health, score, money, wave info, tower placement buttons, and upgrade menus

Available UI commands:
- Create Canvas objects
- Add UI components (Button, Text, Image, etc.)
- Set up event handlers
- Configure layouts and positioning" ]; then
echo "Initial Task:"
cat << 'TASK_EOF'
UI Task: Create tower defense UI with health, score, money, wave info, tower placement buttons, and upgrade menus

Available UI commands:
- Create Canvas objects
- Add UI components (Button, Text, Image, etc.)
- Set up event handlers
- Configure layouts and positioning
TASK_EOF
echo ""
echo "=========================================="
echo ""
fi

# Start Claude Code with worker context
claude --dangerously-skip-permissions "UI Development Specialist

Worker Name: UI_Designer

Available Unity Commands:
- unity_list_objects
- unity_get_scene_info  
- unity_add_component <object> <script>
- unity_remove_component <object> <component>
- unity_set_property <object> <component> <property> <value>
- unity_create_object <name> [x,y,z]
- unity_delete_object <name>
- unity_set_parent <child> <parent>

UI Task: Create tower defense UI with health, score, money, wave info, tower placement buttons, and upgrade menus

Available UI commands:
- Create Canvas objects
- Add UI components (Button, Text, Image, etc.)
- Set up event handlers
- Configure layouts and positioning"
