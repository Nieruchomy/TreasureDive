using UnityEngine;

public class BezierSpline : MonoBehaviour {
    [HideInInspector]
    public BezierSplinePath path;

    public void CreatePath()
    {
        path = new BezierSplinePath(transform.position);
    }

    void Reset()
    {
        CreatePath();
    }

}
