using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BezierSplinePath
{
    [SerializeField] List<Vector3> points;
    [SerializeField] bool isClosed = false;
    [SerializeField] bool autoSetPoints;

    public Vector3 this[int index]
    {
        get
        {
            return points[index];
        } 
    }

    public int SegmentCount
    {
        get
        {
            return points.Count / 3;
        }
    }

    public int PointCount
    {
        get
        {
            return points.Count;
        }
    }

    public bool AutoSetPoints
    {
        get
        {
            return autoSetPoints;
        }
        set
        {
            if(autoSetPoints != value)
            {
                autoSetPoints = value;
                if (autoSetPoints)
                {
                    AutoSetAllPoints();
                }
            }
            
        }
    }

    public bool IsClosed
    {
        get
        {
            return isClosed;
        }
        set
        {
            if(isClosed != value)
            {
                isClosed = value;

                if (isClosed)
                {
                    points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
                    points.Add(points[0] * 2 - points[1]);
                    if (autoSetPoints)
                    {
                        AutoSetAnchorGuides(0);
                        AutoSetAnchorGuides(points.Count - 3);
                    }
                }
                else
                {
                    points.RemoveRange(points.Count - 2, 2);
                    if (autoSetPoints)
                    {
                        AutoSetStartAndEndGuides();
                    }
                }
            }
        }
    }

    public BezierSplinePath(Vector3 pos)
    {
        points = new List<Vector3>
        {
            pos,
            pos + (Vector3.right * 0.5f) + Vector3.up,
            pos + (Vector3.right * 1.5f) + Vector3.down,
            pos + (Vector3.right * 2.0f)
        };
    }

    public void AddSegment()
    {
        Vector3 point = points[points.Count - 1];
        points.Add(point + Vector3.right);
        points.Add(point + Vector3.right * 2);
        points.Add(point + Vector3.right * 3);

        if(autoSetPoints)
        {
            AutoSetAllAffectedPoints(points.Count - 1);
        }
    }

    public void DeleteSegment(int anchorIndex)
    {
        if(SegmentCount > 2 || !isClosed && SegmentCount > 1)
        {
            if (anchorIndex == 0)
            {
                if (isClosed)
                {
                    points[points.Count - 1] = points[2];
                }
                points.RemoveRange(0, 3);
            }
            else if (anchorIndex == points.Count - 1 && !isClosed)
            {
                points.RemoveRange(anchorIndex - 2, 3);
            }
            else
            {
                points.RemoveRange(anchorIndex - 1, 3);
            }
        }

    }

    public void MovePoint(int i, Vector3 newPosition)
    {
        Vector3 delta = newPosition - points[i];

        if(i % 3 == 0 || !autoSetPoints)
        {
            points[i] = newPosition;

            if (autoSetPoints)
            {
                AutoSetAllAffectedPoints(i);
            }
            else
            {
                if (i % 3 == 0) // if move an anchor point, than move control points at the same distance
                {
                    if (i - 1 >= 0 || isClosed)
                        points[LoopIndex(i - 1)] += delta;
                    if (i + 1 < points.Count || isClosed)
                        points[LoopIndex(i + 1)] += delta;
                }
                else
                {       // else move a guide with an opposite guide
                    bool nextIsAnchor = (i + 1) % 3 == 0;
                    int oppositeGuideIndex = (nextIsAnchor) ? i + 2 : i - 2;
                    int anchorIndex = (nextIsAnchor) ? i + 1 : i - 1;

                    if (oppositeGuideIndex >= 0 && oppositeGuideIndex < points.Count || isClosed)
                    {
                        float dst = (points[LoopIndex(anchorIndex)] - points[LoopIndex(oppositeGuideIndex)]).magnitude;
                        Vector3 dir = (points[LoopIndex(anchorIndex)] - newPosition).normalized;
                        points[LoopIndex(oppositeGuideIndex)] = points[LoopIndex(anchorIndex)] + dir * dst;
                    }
                }
            }
        }
      
    }

    public Vector3 GetPoint(float t)
    {
        int i; // segment index
        if (t >= 1f)
        {
            t = 1f;
            i = points.Count - 4;
        }
        else
        {
            t = Mathf.Clamp01(t) * SegmentCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }

        return Bezier.CalculateCubic(points[i], points[i + 1], points[i + 2], points[i + 3], t);
    }

    public Vector3[] GetPointsInSegment(int i)
    {
        return new Vector3[] {
            points[i * 3], points[i * 3 + 1], points[i * 3 + 2], points[LoopIndex(i * 3 + 3)]
        };
    }

    public Vector3[] CalculateEvenlySpacedPoints(float spacing, float resolution = 1)
    {
        List<Vector3> evenlySpacedPoints = new List<Vector3>();
        evenlySpacedPoints.Add(points[0]);
        Vector3 previousPoint = points[0];
        float dstSinceLastEvenPoint = 0;

        for (int segmentIndex = 0; segmentIndex < SegmentCount; segmentIndex++)
        {
            Vector3[] p = GetPointsInSegment(segmentIndex);
            float controlNetLength = Vector3.Distance(p[0], p[1]) + Vector3.Distance(p[1], p[2])
                + Vector3.Distance(p[2], p[3]);
            float estimatedCurveLength = Vector3.Distance(p[0], p[3]) + controlNetLength * 0.5f;
            int division = Mathf.CeilToInt(estimatedCurveLength * resolution * 10);
            float t = 0;
            while (t <= 1f)
            {
                t += 1f / division;
                Vector3 pointOnCurve = Bezier.CalculateCubic(p[0], p[1], p[2], p[3], t);
                dstSinceLastEvenPoint += Vector3.Distance(previousPoint, pointOnCurve);

                while (dstSinceLastEvenPoint >= spacing)
                {
                    float overShootDistance = dstSinceLastEvenPoint - spacing;
                    Vector3 newEvenlySpacedPoint = pointOnCurve + (previousPoint - pointOnCurve).normalized * overShootDistance;
                    evenlySpacedPoints.Add(newEvenlySpacedPoint);
                    dstSinceLastEvenPoint = overShootDistance;
                    previousPoint = newEvenlySpacedPoint;
                }
                previousPoint = pointOnCurve;
            }
        }
        return evenlySpacedPoints.ToArray();
    }

    int LoopIndex(int i)  
    {
        return (i + points.Count) % points.Count; 
    }

    void AutoSetAllAffectedPoints(int updateAnchorIndex)
    {
        for(int i = updateAnchorIndex - 3; i <= updateAnchorIndex + 3; i += 3)
        {
            if(i >=0 && i < points.Count || isClosed)
            {
                AutoSetAnchorGuides(LoopIndex(i));
            }
        }
        AutoSetStartAndEndGuides();
    }

    void AutoSetAllPoints()
    {
        for (int i = 0; i < points.Count; i += 3)
        {
            AutoSetAnchorGuides(i);
        }
        AutoSetStartAndEndGuides();
    }

    void AutoSetAnchorGuides(int anchorIndex)
    {
        Vector3 anchorPos = points[anchorIndex];
        Vector3 dir = Vector3.zero;
        float[] neighbourDistances = new float[2];

        if(anchorIndex - 3 >= 0 || isClosed)
        {
            Vector3 offset = points[LoopIndex(anchorIndex - 3)] - anchorPos;
            dir += offset.normalized;
            neighbourDistances[0] = offset.magnitude;
        }

        if (anchorIndex + 3 >= 0 || isClosed)
        {
            Vector3 offset = points[LoopIndex(anchorIndex + 3)] - anchorPos;
            dir -= offset.normalized;
            neighbourDistances[1] = -offset.magnitude;
        }

        dir.Normalize();

        for (int i = 0; i < 2; i++)
        {
            int guideIndex = anchorIndex + i * 2 - 1;
            if(guideIndex >= 0 && guideIndex < points.Count || isClosed)
            {
                points[LoopIndex(guideIndex)] = anchorPos + dir * neighbourDistances[i] * 0.5f;
            }
        }
    }

    void AutoSetStartAndEndGuides()
    {
        if(!isClosed)
        {
            points[1] = (points[0] + points[2]) * 0.5f;
            points[points.Count - 2] = (points[points.Count - 1] + points[points.Count - 3]) * 0.5f;
        }
    }
}

/*
public void AddSegment(Vector3 anchorPos)
{
    Vector3 endPoint = points[points.Count - 1];
    Vector3 endPointGuide = points[points.Count - 2];
    points.Add(endPoint * 2 - endPointGuide); // Opposite to last point's guide point
    points.Add((points[points.Count - 3] + anchorPos) * 0.5f); // Between previous and input position
    points.Add(anchorPos);

    if (autoSetPoints)
    {
        AutoSetAllAffectedPoints(points.Count - 1);
    }
}

*/