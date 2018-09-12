using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BezierSpline))]
public class BezierSplineEditor : Editor
{
    BezierSpline spline;
    BezierSplinePath path;
    Transform splineTransform;
    Quaternion splineRotation;

    int selectedIndex = -1;
    float buttonSize = 0.1f;

    private static Color[] modeColors = {
        Color.white,
        Color.yellow,
        Color.cyan
    };

    void OnEnable()
    {
        spline = target as BezierSpline;
        if (spline.path == null)
            spline.CreatePath();
        path = spline.path;
        splineTransform = spline.transform;
        splineRotation = spline.transform.rotation;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        bool isClosed = EditorGUILayout.Toggle("Loop", path.IsClosed);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Toggle Loop");
            EditorUtility.SetDirty(spline);
            path.IsClosed = isClosed;
        }

        if (selectedIndex >= 0 && selectedIndex < path.PointCount)
        {
            DrawSelectedPointInspector();
        }

        if (GUILayout.Button("Add Segment"))
        {
            Undo.RecordObject(spline, "Segment");
            EditorUtility.SetDirty(spline);
            path.AddSegment();
        }
    }

    void OnSceneGUI()
    {
        Vector3 p0 = SetPoint(0);
        for (int i = 1; i < path.PointCount; i += 3)
        {
            Vector3 p1 = SetPoint(i);
            Vector3 p2 = SetPoint(i + 1);
            Vector3 p3 = SetPoint(i + 2);
            DrawSpline(p0, p1, p2, p3);
            p0 = p3;
        }

    }

    Vector3 SetPoint(int index)
    {
        Vector3 pointWorldPos = splineTransform.TransformPoint(path[index]);

        Handles.color = modeColors[(int)path.GetPointModeAt(index)];
        if (Handles.Button(pointWorldPos, splineRotation, buttonSize, buttonSize * 2, Handles.DotHandleCap))
        {
            selectedIndex = index;
            Repaint();
        }

        if (selectedIndex == index)
        {
            EditorGUI.BeginChangeCheck(); // start check

            pointWorldPos = Handles.DoPositionHandle(pointWorldPos, splineRotation);

            if (EditorGUI.EndChangeCheck()) // end check 
            {
                Undo.RecordObject(spline, "Move Point");
                EditorUtility.SetDirty(spline);
                path.SetControlPoint(index, splineTransform.InverseTransformPoint(pointWorldPos));
            }
        }

        //Set label with basic info
        Handles.Label(splineTransform.TransformPoint(path[index]) + (Vector3.up * 0.5f) + (Vector3.left * 1.5f),
                      path[index].ToString() + "\n Index: " + index);

        return pointWorldPos;
    }

    void DrawSpline(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Handles.color = Color.grey;
        Handles.DrawLine(p0, p1);
        Handles.DrawLine(p2, p3);

        Handles.DrawBezier(p0, p3, p1, p2, Color.black, null, 2f);
    }

    void DrawSelectedPointInspector()
    {
        GUILayout.Label("Selected Point");
        EditorGUI.BeginChangeCheck(); // start check

        Vector3 point = EditorGUILayout.Vector3Field("Position", path[selectedIndex]);

        if (EditorGUI.EndChangeCheck()) // end check
        {
            Undo.RecordObject(spline, "Move Point");
            EditorUtility.SetDirty(spline);
            path.SetControlPoint(selectedIndex,point);
        }

        EditorGUI.BeginChangeCheck();

        BezierPointMode mode =
            (BezierPointMode)EditorGUILayout.EnumPopup("Mode", path.GetPointModeAt(selectedIndex));

        if(EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "MChange Mode");
            EditorUtility.SetDirty(spline);
            path.SetPointModeAt(selectedIndex, mode);
        }


    }
}