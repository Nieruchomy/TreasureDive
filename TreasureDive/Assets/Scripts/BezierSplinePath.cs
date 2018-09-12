using System;
using UnityEngine;

[System.Serializable]
public class BezierSplinePath
{
    [SerializeField] Vector3[] points;
    [SerializeField] BezierPointMode[] modes;
    [SerializeField] bool isClosed;

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
            return (points.Length - 1) / 3;
        }
    }

    public int PointCount
    {
        get
        {
            return points.Length;
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
            isClosed = value;
            if (value == true)
            {
                modes[modes.Length - 1] = modes[0];
                SetControlPoint(0, points[0]);
            }
        }
    }

    public BezierSplinePath(Vector3 pos)
    {
        points = new Vector3[]
        {
            pos,
            pos + (Vector3.right * 0.5f) + Vector3.up,
            pos + (Vector3.right * 1.5f) + Vector3.down,
            pos + (Vector3.right * 2.0f)
        };

        modes = new BezierPointMode[]
        {
            BezierPointMode.Free,
            BezierPointMode.Free
        };
    }

    public void AddSegment()
    {
        Vector3 point = points[points.Length - 1];
        Array.Resize(ref points, points.Length + 3);
        points[points.Length - 3] = point + Vector3.right;
        points[points.Length - 2] = point + Vector3.right * 2;
        points[points.Length - 1] = point + Vector3.right * 3;

        Array.Resize(ref modes, modes.Length + 1);
        modes[modes.Length - 1] = modes[modes.Length - 2];
        EnforceMode(points.Length - 4);


        if (isClosed)
        {
            points[points.Length - 1] = points[0];
            modes[modes.Length - 1] = modes[0];
            EnforceMode(0);
        }
    }

    public Vector3 GetPoint(float t)
    {
        int i; // segment index
        if (t >= 1f)
        {
            t = 1f;
            i = points.Length - 4;
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

    public BezierPointMode GetPointModeAt(int index)
    {
        return modes[(index + 1) / 3];
    }

    public void SetPointModeAt(int index, BezierPointMode mode)
    {
        int modeIndex = (index + 1) / 3;
        modes[modeIndex] = mode;
        if (isClosed)
        {
            if (modeIndex == 0)
            {
                modes[modes.Length - 1] = mode;
            }
            else if (modeIndex == modes.Length - 1)
            {
                modes[0] = mode;
            }
        }
        EnforceMode(index);
    }

    private void EnforceMode(int index)
    {
        int modeIndex = (index + 1) / 3;
        BezierPointMode mode = modes[modeIndex];
        if (mode == BezierPointMode.Free || !isClosed && (modeIndex == 0 || modeIndex == modes.Length - 1))
        {
            return;
        }
        int middleIndex = modeIndex * 3;
        int fixedIndex, enforcedIndex;
        if (index <= middleIndex)
        {
            fixedIndex = middleIndex - 1;
            if (fixedIndex < 0)
            {
                fixedIndex = points.Length - 2;
            }
            enforcedIndex = middleIndex + 1;
            if (enforcedIndex >= points.Length)
            {
                enforcedIndex = 1;
            }
        }
        else
        {
            fixedIndex = middleIndex + 1;
            if (fixedIndex >= points.Length)
            {
                fixedIndex = 1;
            }
            enforcedIndex = middleIndex - 1;
            if (enforcedIndex < 0)
            {
                enforcedIndex = points.Length - 2;
            }
        }

        Vector3 middle = points[middleIndex];
        Vector3 enforcedTangent = middle - points[fixedIndex];
        if (mode == BezierPointMode.Aligned)
        {
            enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
        }
        points[enforcedIndex] = middle + enforcedTangent;
    }

    public void SetControlPoint(int index, Vector3 point)
    {
        if (index % 3 == 0)
        {
            Vector3 delta = point - points[index];
            if (isClosed)
            {
                if (index == 0)
                {
                    points[1] += delta;
                    points[points.Length - 2] += delta;
                    points[points.Length - 1] = point;
                }
                else if (index == points.Length - 1)
                {
                    points[0] = point;
                    points[1] += delta;
                    points[index - 1] += delta;
                }
                else
                {
                    points[index - 1] += delta;
                    points[index + 1] += delta;
                }
            }
            else
            {
                if (index > 0)
                {
                    points[index - 1] += delta;
                }
                if (index + 1 < points.Length)
                {
                    points[index + 1] += delta;
                }
            }
        }
        points[index] = point;
        EnforceMode(index);
    }
}

