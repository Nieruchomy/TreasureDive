using UnityEngine;

public class BezierSpline : MonoBehaviour {
    [HideInInspector]
    public BezierSplinePath path;

    public Color anchorColor = Color.red;
    public Color guideColor = Color.grey;
    public Color splineColor = Color.black;
    public float anchorDiameter = 0.1f;
    public float guideDiameter = 0.75f;
    public bool displayControlPoints = true;

    public void CreatePath()
    {
        path = new BezierSplinePath(transform.position);
    }

    void Reset()
    {
        CreatePath();
    }

}
