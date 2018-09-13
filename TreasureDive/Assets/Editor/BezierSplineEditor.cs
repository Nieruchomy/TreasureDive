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

    void OnEnable()
    {
        spline = target as BezierSpline;
        if (spline.path == null)
            spline.CreatePath();
        path = spline.path;
        splineTransform = spline.transform;
        splineRotation = spline.transform.rotation;
    }

    void OnSceneGUI()
    {
        Input();
        Draw();
    }

    void Input()
    {
        Event guiEvent = Event.current;
        Vector2 mousePos = HandleUtility.GUIPointToScreenPixelCoordinate(guiEvent.mousePosition);

        if(guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
        {
            float minDstToAnchor = 10f; // TODO: make inspector
            int closestAnchorIndex = -1;

            for (int i = 0; i < path.PointCount; i += 3)
            {  
                float dst = Vector2.Distance(mousePos, Camera.current.WorldToScreenPoint(splineTransform.TransformPoint(path[i])));
                if(dst < minDstToAnchor)
                {
                    minDstToAnchor = dst;
                    closestAnchorIndex = i;
                }
            }
            if (closestAnchorIndex != -1)
            {
                Undo.RecordObject(spline, "Delete Segment");
                path.DeleteSegment(closestAnchorIndex);
            }
        }

        //HandleUtility.AddDefaultControl(0);
    }

    void Draw()
    {
        /*********** DRAW CURVES **********/
        for (int i = 0; i < path.SegmentCount; i++)
        {
            Vector3[] points = path.GetPointsInSegment(i);
            Handles.color = Color.grey;

            //TODO: clean up this
            Vector3 p0 = splineTransform.TransformPoint(points[0]);
            Vector3 p1 = splineTransform.TransformPoint(points[1]);
            Vector3 p2 = splineTransform.TransformPoint(points[2]);
            Vector3 p3 = splineTransform.TransformPoint(points[3]);

            Handles.DrawLine(p1, p0);
            Handles.DrawLine(p2, p3); 
            Handles.DrawBezier(p0, p3, p1, p2, spline.splineColor, null, 2);
        }

        /*********** DRAW HANDLES **********/
        for (int i = 0; i < path.PointCount; i++)
        {
            DrawPoint(i);
        }
    }

    void DrawPoint(int index)
    {
        Vector3 pointWorldPos = splineTransform.TransformPoint(path[index]);

        Handles.color = Color.red;

        EditorGUI.BeginChangeCheck(); // start check

        Vector3 newPosition = Handles.FreeMoveHandle(pointWorldPos, splineRotation, .1f, Vector3.zero, Handles.DotHandleCap);

        if (EditorGUI.EndChangeCheck()) // end check 
        {
            selectedIndex = index;
            path.MovePoint(index, splineTransform.InverseTransformPoint(newPosition));
            Undo.RecordObject(spline, "Move Point");
            EditorUtility.SetDirty(spline);
            Repaint();
        }

        //Set label with basic info
        Handles.Label(splineTransform.TransformPoint(path[index]) + (Vector3.up * 0.5f) + (Vector3.left * 1.5f),
                      path[index].ToString() + "\n Index: " + index);
    }

    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();
        EditorGUI.BeginChangeCheck();

        /*********** DISPLAY CURRENT POINT POS **********/
        if (selectedIndex >= 0 && selectedIndex < path.PointCount)
        {
            DrawSelectedPointInspector();
        }

        /*********** Create New **********/
        if (GUILayout.Button("Create New"))
        {
            Undo.RecordObject(spline, "Create New");
            EditorUtility.SetDirty(spline);
            spline.CreatePath();
            path = spline.path;
        }

        /*********** BUTTON ADD SEGMENT **********/
        if (GUILayout.Button("Add Segment"))
        {
            Undo.RecordObject(spline, "Add Segment");
            EditorUtility.SetDirty(spline);
            path.AddSegment();
        }

        /*********** TOGGLE CLOSED **********/
        bool isClosed = GUILayout.Toggle(path.IsClosed, "Closed");
        if (isClosed != path.IsClosed)
        {
            Undo.RecordObject(spline, "Toggle Closed");
            EditorUtility.SetDirty(spline);
            path.IsClosed = isClosed;
        }

        /*********** TOGGLE AUTOSET **********/
       
        bool isAutoSet = GUILayout.Toggle(path.AutoSetPoints, "Auto Set Points");
        if(path.AutoSetPoints != isAutoSet)
        {
            Undo.RecordObject(spline, "Toggle AutoSet");
            EditorUtility.SetDirty(spline);
            path.AutoSetPoints = isAutoSet;
        }
        if(EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }
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
            path.MovePoint(selectedIndex,point);
        }
    }
}