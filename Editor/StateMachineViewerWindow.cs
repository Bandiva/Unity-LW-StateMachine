using System.Collections.Generic;
using System.Linq;
using LWSM;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class StateMachineViewerWindow : EditorWindow
{
    private List<StateMachine> stateMachines = new();
    private Dictionary<StateMachine, Color> stateMachineColors = new();
    private bool isPlaying;

    private GUIStyle boldLabelStyle;
    private GUIStyle boxStyle;

    [MenuItem("Window/State Machine Viewer")]
    public static void ShowWindow()
    {
        GetWindow<StateMachineViewerWindow>("State Machine Viewer");
    }

    private void OnEnable()
    {
        isPlaying = EditorApplication.isPlaying;
        EditorApplication.update += UpdateWindow;
    }

    private void OnDisable()
    {
        EditorApplication.update -= UpdateWindow;
    }

    private void UpdateWindow()
    {
        if (EditorApplication.isPlaying != isPlaying)
        {
            isPlaying = EditorApplication.isPlaying;
            if (isPlaying)
            {
                RefreshStateMachines();
            }
            else
            {
                // Handle exit from play mode if necessary
            }
        }

        Repaint();
    }

    private void OnGUI()
    {
        InitializeStyles();
        if (EditorApplication.isPlaying)
        {
            foreach (var stateMachine in stateMachines)
            {
                // Track whether the section is expanded or collapsed
                var isExpanded = EditorPrefs.GetBool($"StateMachine_{stateMachine.GetInstanceID()}", true);

                // Create a foldout section
                isExpanded = EditorGUILayout.Foldout(isExpanded, "", true);
                EditorPrefs.SetBool($"StateMachine_{stateMachine.GetInstanceID()}", isExpanded);
                if (isExpanded)
                {
                    EditorGUILayout.BeginVertical();
                    if (stateMachine != null)
                        EditorGUILayout.LabelField(stateMachine.name, boldLabelStyle, GUILayout.Width(200));
                    EditorGUILayout.Space(2);
                    if (GUILayout.Button("Select", GUILayout.Width(100)))
                        Selection.activeGameObject = stateMachine.gameObject;

                    // Display nextState information
                    if (stateMachine.CurrentState != null)
                    {
                        var currentState = stateMachine.CurrentState;
                        var beforeState = stateMachine.States.Values.FirstOrDefault();
                        var beforeStateInfo = beforeState != null ? beforeState.ToString() : "No next state";
                        var currentStateInfo = currentState != null ? currentState.ToString() : "No current state";

                        var originStyle = new GUIStyle();
                        var currentStyle = new GUIStyle();
                        originStyle.normal.textColor = Color.blue;
                        currentStyle.normal.textColor = Color.green;
                        EditorGUILayout.BeginVertical(boxStyle);
                        EditorGUILayout.LabelField($"Origin State: {beforeStateInfo}", originStyle);
                        EditorGUILayout.LabelField($"Current State: {currentStateInfo}", currentStyle);
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginHorizontal();

                        var stateCount = stateMachine.States.Count;
                        var currentStateIndex = 0;

                        // Find the index of the CurrentState
                        var index = 0;
                        foreach (var kvp in stateMachine.States)
                        {
                            if (kvp.Value == stateMachine.CurrentState)
                            {
                                currentStateIndex = index;
                                break;
                            }

                            index++;
                        }


                        for (var i = 0; i < stateCount; i++)
                        {
                            var circleColor = i == currentStateIndex ? Color.green : Color.red;
                            DrawCircle(circleColor);
                        }

                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(5);
                }
            }
        }
        else
        {
            RefreshStateMachines();

            if (GUILayout.Button("Refresh", GUILayout.Width(100))) RefreshStateMachines();
            var notActive = new GUIContent { text = "Play mode is not active" };
            EditorGUILayout.HelpBox(notActive);
            GUILayout.Space(15);

            foreach (var stateMachine in stateMachines)
            {
                EditorGUILayout.BeginVertical();

                // Display the name of the state machine's GameObject
                EditorGUILayout.LabelField(stateMachine.gameObject.name, EditorStyles.boldLabel);

                // Button to select the GameObject in the hierarchy
                if (GUILayout.Button("Select", GUILayout.Width(100)))
                    Selection.activeGameObject = stateMachine.gameObject;

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5); // Add space between StateMachine entries
            }
        }
    }

    private void RefreshStateMachines()
    {
        stateMachines = FindObjectsOfType<StateMachine>().ToList();
        stateMachineColors.Clear(); // Clear the colors when refreshing
        foreach (var stateMachine in stateMachines)
            // Assign a new random color when refreshing
            stateMachineColors[stateMachine] = new Color(Random.value, Random.value, Random.value);
    }

    private void DrawCircle(Color color)
    {
        // Draw a small circle using Handles
        Handles.BeginGUI();
        var circleRect = new Rect(0, 0, 10, 10); // Adjust size as needed

        // Use GUI.DrawTexture to draw the circle
        var circleStyle = new GUIStyle();
        circleStyle.normal.background = CreateCircleTexture(color);

        // Place the circle just below the labels
        EditorGUILayout.LabelField("", circleStyle, GUILayout.Width(10), GUILayout.Height(10));
        Handles.EndGUI();
    }

    private Texture2D CreateCircleTexture(Color color)
    {
        var diameter = 10;
        var texture = new Texture2D(diameter, diameter);
        var pixels = new Color[diameter * diameter];

        // Calculate radius
        var radius = diameter / 2f;
        var center = new Vector2(radius, radius);

        // Fill texture with color in circle shape
        for (var y = 0; y < diameter; y++)
        for (var x = 0; x < diameter; x++)
        {
            var point = new Vector2(x, y);
            if (Vector2.Distance(point, center) <= radius)
                pixels[x + y * diameter] = color;
            else
                pixels[x + y * diameter] = Color.clear;
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    private void InitializeStyles()
    {
        boxStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = Texture2D.grayTexture },
            padding = new RectOffset(10, 10, 10, 10) // Add some padding for aesthetics
        };
        boldLabelStyle = new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 15
        };
    }
}
