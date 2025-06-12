#!/bin/bash

# Claude Code Multi-Worker System
# Spawns multiple specialized Claude Code instances for complex Unity tasks

UNITY_PROJECT_PATH="/mnt/c/Users/cody.av/Documents/Repositories/Development/Claude-Code---Unity-Integrations"

# Function to spawn a specialized Claude Code worker
spawn_claude_worker() {
    local worker_name="$1"
    local task_description="$2"
    local initial_prompt="$3"
    
    if [ -z "$worker_name" ] || [ -z "$task_description" ]; then
        echo "Usage: spawn_claude_worker <worker_name> <task_description> [initial_prompt]"
        return 1
    fi
    
    # Create worker-specific directory for communication
    local worker_dir="$UNITY_PROJECT_PATH/Assets/ClaudeCodeIntegration/Workers/$worker_name"
    mkdir -p "$worker_dir"
    
    # Create worker initialization script
    local worker_script="$worker_dir/worker_init.sh"
    
    cat > "$worker_script" << EOF
#!/bin/bash
cd '$UNITY_PROJECT_PATH'

echo "=========================================="
echo "Claude Code Worker: $worker_name"
echo "=========================================="
echo "Task: $task_description"
echo "=========================================="
echo ""

# Load Unity command functions
source ./unity_commands.sh

echo ""
echo "Worker '$worker_name' is ready!"
echo "Available Unity commands: unity_help"
echo ""

# Show initial context if provided
if [ -n "$initial_prompt" ]; then
echo "Initial Task:"
cat << 'TASK_EOF'
$initial_prompt
TASK_EOF
echo ""
echo "=========================================="
echo ""
fi

# Start Claude Code with worker context
claude --dangerously-skip-permissions "$task_description

Worker Name: $worker_name

Available Unity Commands:
- unity_list_objects
- unity_get_scene_info  
- unity_add_component <object> <script>
- unity_remove_component <object> <component>
- unity_set_property <object> <component> <property> <value>
- unity_create_object <name> [x,y,z]
- unity_delete_object <name>
- unity_set_parent <child> <parent>

$initial_prompt"
EOF

    chmod +x "$worker_script"
    
    # Launch worker in new terminal window
    cmd.exe /c start "Claude Worker: $worker_name" wsl bash "'$worker_script'"
    
    echo "Spawned Claude worker: $worker_name"
    echo "Task: $task_description"
}

# Predefined worker types for common Unity tasks

# Script Development Worker
spawn_script_worker() {
    local script_name="$1"
    local requirements="$2"
    
    spawn_claude_worker "ScriptDev_$script_name" \
        "Script Development Specialist" \
        "Create a MonoBehaviour script called '$script_name' with these requirements:
$requirements

Steps:
1. Create the script in Assets/Scripts/
2. Implement the required functionality  
3. Test and attach to appropriate GameObjects
4. Report completion status"
}

# UI Development Worker  
spawn_ui_worker() {
    local ui_task="$1"
    
    spawn_claude_worker "UI_Designer" \
        "UI Development Specialist" \
        "UI Task: $ui_task

Available UI commands:
- Create Canvas objects
- Add UI components (Button, Text, Image, etc.)
- Set up event handlers
- Configure layouts and positioning"
}

# Scene Setup Worker
spawn_scene_worker() {
    local scene_task="$1"
    
    spawn_claude_worker "Scene_Builder" \
        "Scene Setup Specialist" \
        "Scene Task: $scene_task

Focus on:
- Creating GameObjects and hierarchies
- Setting up lighting and cameras
- Organizing scene structure
- Adding environmental components"
}

# Animation/Movement Worker
spawn_animation_worker() {
    local animation_task="$1"
    
    spawn_claude_worker "Animator" \
        "Animation & Movement Specialist" \
        "Animation Task: $animation_task

Specializes in:
- Movement scripts and behaviors
- Transform animations
- Physics-based motion
- Interactive object behaviors"
}

# Testing/Debugging Worker
spawn_test_worker() {
    local test_task="$1"
    
    spawn_claude_worker "Tester" \
        "Testing & Debug Specialist" \
        "Testing Task: $test_task

Responsibilities:
- Test existing functionality
- Debug issues and errors
- Verify component behaviors
- Report findings and fixes"
}

# Coordination Worker (Master)
spawn_coordinator() {
    local project_goal="$1"
    
    spawn_claude_worker "Coordinator" \
        "Project Coordinator" \
        "Project Goal: $project_goal

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
}

# Complex project handler
handle_complex_project() {
    local project_description="$1"
    
    echo "Handling complex project: $project_description"
    echo ""
    echo "Spawning Coordinator and initial workers..."
    
    # Spawn coordinator
    spawn_coordinator "$project_description"
    
    # Give coordinator time to start
    sleep 2
    
    echo ""
    echo "Workers spawned! Check the terminal windows for progress."
    echo "Coordinator will manage the project and spawn additional workers as needed."
}

# Show available commands
show_worker_help() {
    echo "Claude Code Multi-Worker System"
    echo ""
    echo "Basic Commands:"
    echo "  spawn_claude_worker <name> <task> [prompt]  - Spawn generic worker"
    echo ""
    echo "Specialized Workers:"
    echo "  spawn_script_worker <name> <requirements>   - Script development"
    echo "  spawn_ui_worker <task>                      - UI development"
    echo "  spawn_scene_worker <task>                   - Scene setup"
    echo "  spawn_animation_worker <task>               - Animation/movement"
    echo "  spawn_test_worker <task>                    - Testing/debugging"
    echo ""
    echo "Project Management:"
    echo "  spawn_coordinator <goal>                    - Project coordinator"
    echo "  handle_complex_project <description>        - Full project handler"
    echo ""
    echo "Examples:"
    echo "  spawn_script_worker \"PlayerController\" \"WASD movement, jumping, collision\""
    echo "  spawn_ui_worker \"Create main menu with start/quit buttons\""
    echo "  handle_complex_project \"Build a complete 3D platformer game\""
}

# Auto-source check
if [[ "${BASH_SOURCE[0]}" != "${0}" ]]; then
    echo "Claude Code Multi-Worker System loaded."
    echo "Type 'show_worker_help' for available commands."
else
    show_worker_help
fi