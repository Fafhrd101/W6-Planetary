using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MazeGenerator))]
public class MazeGenWindow : Editor
{
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        MazeGenerator maze = target as MazeGenerator;

        if(GUILayout.Button("Create Maze"))
            maze.CreateMaze();
        if(GUILayout.Button("Clear Maze"))
            maze.ClearMaze();
    }
}
