#!/bin/bash
cd '/mnt/c/Users/cody.av/Documents/Repositories/Development/Claude-Code---Unity-Integrations'

echo "=========================================="
echo "Claude Code Worker: ScriptDev_EnemyController"
echo "=========================================="
echo "Task: Script Development Specialist"
echo "=========================================="
echo ""

# Load Unity command functions
source ./unity_commands.sh

echo ""
echo "Worker 'ScriptDev_EnemyController' is ready!"
echo "Available Unity commands: unity_help"
echo ""

# Show initial context if provided
if [ -n "Create a MonoBehaviour script called 'EnemyController' with these requirements:
Enemy movement along paths, health system, death handling, wave spawning mechanics

Steps:
1. Create the script in Assets/Scripts/
2. Implement the required functionality  
3. Test and attach to appropriate GameObjects
4. Report completion status" ]; then
echo "Initial Task:"
cat << 'TASK_EOF'
Create a MonoBehaviour script called 'EnemyController' with these requirements:
Enemy movement along paths, health system, death handling, wave spawning mechanics

Steps:
1. Create the script in Assets/Scripts/
2. Implement the required functionality  
3. Test and attach to appropriate GameObjects
4. Report completion status
TASK_EOF
echo ""
echo "=========================================="
echo ""
fi

# Start Claude Code with worker context
claude --dangerously-skip-permissions "Script Development Specialist

Worker Name: ScriptDev_EnemyController

Available Unity Commands:
- unity_list_objects
- unity_get_scene_info  
- unity_add_component <object> <script>
- unity_remove_component <object> <component>
- unity_set_property <object> <component> <property> <value>
- unity_create_object <name> [x,y,z]
- unity_delete_object <name>
- unity_set_parent <child> <parent>

Create a MonoBehaviour script called 'EnemyController' with these requirements:
Enemy movement along paths, health system, death handling, wave spawning mechanics

Steps:
1. Create the script in Assets/Scripts/
2. Implement the required functionality  
3. Test and attach to appropriate GameObjects
4. Report completion status"
