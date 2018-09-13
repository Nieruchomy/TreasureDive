using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RopeCreator))]
public class RopeEditor : Editor {
    RopeCreator rope;

    void OnSceneGUI()
    {
        if(rope.autoUpdate && Event.current.type == EventType.Repaint)
        {
            rope.UpdateRope();
        }
    }
    void OnEnable()
    {
        rope = target as RopeCreator;
    }
}
