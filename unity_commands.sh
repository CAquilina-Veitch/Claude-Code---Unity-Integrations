#!/bin/bash

# Unity Command Helper Functions for Claude Code
# This script provides easy functions to interact with Unity through the CommandProcessor

UNITY_PROJECT_PATH="/mnt/c/Users/cody.av/Documents/Repositories/Development/Claude-Code---Unity-Integrations"
COMMANDS_DIR="$UNITY_PROJECT_PATH/Assets/ClaudeCodeIntegration/Commands"
RESPONSES_DIR="$UNITY_PROJECT_PATH/Assets/ClaudeCodeIntegration/Responses"

# Ensure directories exist
mkdir -p "$COMMANDS_DIR"
mkdir -p "$RESPONSES_DIR"

# Generate unique command ID
generate_id() {
    echo "cmd_$(date +%s)_$$"
}

# Send command and wait for response
send_unity_command() {
    local command="$1"
    local timeout="${2:-10}"  # Default 10 second timeout
    
    local cmd_id=$(generate_id)
    local cmd_file="$COMMANDS_DIR/${cmd_id}.cmd"
    local response_file="$RESPONSES_DIR/${cmd_id}.response"
    
    # Write command (using echo to avoid permission prompts)
    echo "$command" > "$cmd_file"
    
    # Wait for response
    local count=0
    while [ ! -f "$response_file" ] && [ $count -lt $timeout ]; do
        sleep 0.1
        count=$((count + 1))
    done
    
    if [ -f "$response_file" ]; then
        cat "$response_file"
        rm -f "$response_file"
        return 0
    else
        echo "ERROR: Timeout waiting for Unity response"
        rm -f "$cmd_file"  # Clean up if timeout
        return 1
    fi
}

# Unity Command Functions

# Add a component to a GameObject
unity_add_component() {
    local object_name="$1"
    local script_name="$2"
    
    if [ -z "$object_name" ] || [ -z "$script_name" ]; then
        echo "Usage: unity_add_component <object_name> <script_name>"
        return 1
    fi
    
    send_unity_command "ADD_COMPONENT:$object_name:$script_name"
}

# Remove a component from a GameObject
unity_remove_component() {
    local object_name="$1"
    local component_name="$2"
    
    if [ -z "$object_name" ] || [ -z "$component_name" ]; then
        echo "Usage: unity_remove_component <object_name> <component_name>"
        return 1
    fi
    
    send_unity_command "REMOVE_COMPONENT:$object_name:$component_name"
}

# Set a property on a component
unity_set_property() {
    local object_name="$1"
    local component_name="$2"
    local property_name="$3"
    local value="$4"
    
    if [ -z "$object_name" ] || [ -z "$component_name" ] || [ -z "$property_name" ] || [ -z "$value" ]; then
        echo "Usage: unity_set_property <object_name> <component_name> <property_name> <value>"
        return 1
    fi
    
    send_unity_command "SET_PROPERTY:$object_name:$component_name:$property_name:$value"
}

# Create a new GameObject
unity_create_object() {
    local object_name="$1"
    local position="$2"  # Optional: "x,y,z"
    
    if [ -z "$object_name" ]; then
        echo "Usage: unity_create_object <object_name> [position]"
        return 1
    fi
    
    if [ -n "$position" ]; then
        send_unity_command "CREATE_OBJECT:$object_name:$position"
    else
        send_unity_command "CREATE_OBJECT:$object_name"
    fi
}

# Delete a GameObject
unity_delete_object() {
    local object_name="$1"
    
    if [ -z "$object_name" ]; then
        echo "Usage: unity_delete_object <object_name>"
        return 1
    fi
    
    send_unity_command "DELETE_OBJECT:$object_name"
}

# Get detailed scene information
unity_get_scene_info() {
    send_unity_command "GET_SCENE_INFO"
}

# List all objects in the scene
unity_list_objects() {
    send_unity_command "LIST_OBJECTS"
}

# Set parent-child relationship between GameObjects
unity_set_parent() {
    local child_name="$1"
    local parent_name="$2"
    
    if [ -z "$child_name" ] || [ -z "$parent_name" ]; then
        echo "Usage: unity_set_parent <child_name> <parent_name>"
        return 1
    fi
    
    send_unity_command "SET_PARENT:$child_name:$parent_name"
}

# Set primitive mesh for an object
unity_set_primitive_mesh() {
    local object_name="$1"
    local primitive_type="$2"
    
    if [ -z "$object_name" ] || [ -z "$primitive_type" ]; then
        echo "Usage: unity_set_primitive_mesh <object_name> <primitive_type>"
        echo "Supported types: PLANE, CUBE, SPHERE, CYLINDER, CAPSULE"
        return 1
    fi
    
    send_unity_command "SET_PRIMITIVE_MESH:$object_name:$primitive_type"
}

# Quick function to attach a script to an object (common use case)
unity_attach_script() {
    local object_name="$1"
    local script_name="$2"
    
    echo "Attaching '$script_name' to '$object_name'..."
    unity_add_component "$object_name" "$script_name"
}

# Function to show all available commands
unity_help() {
    echo "Unity Command Functions:"
    echo "  unity_add_component <object> <script>      - Add component to object"
    echo "  unity_remove_component <object> <component> - Remove component from object"  
    echo "  unity_set_property <object> <component> <property> <value> - Set component property"
    echo "  unity_create_object <name> [x,y,z]         - Create new GameObject"
    echo "  unity_delete_object <name>                 - Delete GameObject"
    echo "  unity_get_scene_info                       - Get detailed scene info"
    echo "  unity_list_objects                         - List all GameObjects"
    echo "  unity_set_parent <child> <parent>          - Set parent-child relationship"
    echo "  unity_set_primitive_mesh <object> <type>   - Set primitive mesh (PLANE, CUBE, etc.)"
    echo "  unity_attach_script <object> <script>      - Quick script attachment"
    echo "  unity_create_and_attach <object> <script>  - Create script file AND attach it"
    echo ""
    echo "Examples:"
    echo "  unity_list_objects"
    echo "  unity_attach_script \"Cube\" \"SpinObject\""
    echo "  unity_set_property \"Cube\" \"SpinObject\" \"speed\" \"2.0\""
    echo "  unity_create_object \"NewCube\" \"0,5,0\""
    echo "  unity_create_and_attach \"Cube\" \"RotateScript\""
}

# Advanced function to create a script file AND attach it in one go
unity_create_and_attach() {
    local object_name="$1"
    local script_name="$2"
    local script_content="$3"
    
    if [ -z "$object_name" ] || [ -z "$script_name" ]; then
        echo "Usage: unity_create_and_attach <object_name> <script_name> [script_content]"
        return 1
    fi
    
    local script_path="$UNITY_PROJECT_PATH/Assets/Scripts/${script_name}.cs"
    
    # Create default script content if none provided
    if [ -z "$script_content" ]; then
        script_content="using UnityEngine;

public class ${script_name} : MonoBehaviour
{
    void Start()
    {
        
    }
    
    void Update()
    {
        
    }
}"
    fi
    
    # Create the script file
    echo "$script_content" > "$script_path"
    echo "Created script: $script_path"
    
    # Wait a moment for Unity to compile
    sleep 1
    
    # Attach the component
    unity_add_component "$object_name" "$script_name"
}

# Auto-source check - if script is being sourced, don't run anything
if [[ "${BASH_SOURCE[0]}" != "${0}" ]]; then
    echo "Unity command functions loaded. Type 'unity_help' for available commands."
else
    # Script was executed directly, show help
    unity_help
fi