#!/bin/bash
cd '/mnt/c/Users/cody.av/Documents/Repositories/Development/Claude-Code---Unity-Integrations'

echo "=========================================="
echo "Claude Code Worker: Coordinator"
echo "=========================================="
echo "Task: Project Coordinator"
echo "=========================================="
echo ""

# Load Unity command functions
source ./unity_commands.sh

echo ""
echo "Worker 'Coordinator' is ready!"
echo "Available Unity commands: unity_help"
echo ""

# Show initial context if provided
if [ -n "Project Goal: Build a complete 3D Tower Defense Game with towers that shoot at enemies, enemy waves, health/score systems, upgrade mechanics, and particle effects

As coordinator, you should:
1. Break down the goal into subtasks
2. Spawn appropriate specialist workers
3. Monitor progress via Unity commands
4. Coordinate between workers
5. Ensure project completion

You can spawn workers using:
- spawn_script_worker <name> <requirements>
- spawn_ui_worker <task>
- spawn_scene_worker <task>  
- spawn_animation_worker <task>
- spawn_test_worker <task>" ]; then
echo "Initial Task:"
cat << 'TASK_EOF'
Project Goal: Build a complete 3D Tower Defense Game with towers that shoot at enemies, enemy waves, health/score systems, upgrade mechanics, and particle effects

As coordinator, you should:
1. Break down the goal into subtasks
2. Spawn appropriate specialist workers
3. Monitor progress via Unity commands
4. Coordinate between workers
5. Ensure project completion

You can spawn workers using:
- spawn_script_worker <name> <requirements>
- spawn_ui_worker <task>
- spawn_scene_worker <task>  
- spawn_animation_worker <task>
- spawn_test_worker <task>
TASK_EOF
echo ""
echo "=========================================="
echo ""
fi

# Start Claude Code with worker context
claude --dangerously-skip-permissions "Project Coordinator

Worker Name: Coordinator

Available Unity Commands:
- unity_list_objects
- unity_get_scene_info  
- unity_add_component <object> <script>
- unity_remove_component <object> <component>
- unity_set_property <object> <component> <property> <value>
- unity_create_object <name> [x,y,z]
- unity_delete_object <name>
- unity_set_parent <child> <parent>

Project Goal: Build a complete 3D Tower Defense Game with towers that shoot at enemies, enemy waves, health/score systems, upgrade mechanics, and particle effects

As coordinator, you should:
1. Break down the goal into subtasks
2. Spawn appropriate specialist workers
3. Monitor progress via Unity commands
4. Coordinate between workers
5. Ensure project completion

You can spawn workers using:
- spawn_script_worker <name> <requirements>
- spawn_ui_worker <task>
- spawn_scene_worker <task>  
- spawn_animation_worker <task>
- spawn_test_worker <task>"
