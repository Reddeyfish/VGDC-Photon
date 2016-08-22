using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;

public abstract class TimeCollider : MonoBehaviour
{
    Stack<TimestampedData<Vector3>> positionHistory = new Stack<TimestampedData<Vector3>>();
    public IEnumerable<TimestampedData<Vector3>> PositionHistory { get { return positionHistory; } }

    public TimePhysicsLayers layer = TimePhysicsLayers.NONE;

    double beginTime = -1;
    public double BeginTime
    {
        get
        {
            if (positionHistory.Count == 0)
            {
                throw new System.InvalidOperationException("TimeCollider has no data");
            }
            else
            {
                return beginTime;
            }
        }
    }

    public double EndTime
    {
        get
        {
            if (positionHistory.Count == 0)
            {
                throw new System.InvalidOperationException("TimeCollider has no data");
            }
            else
            {
                return positionHistory.Peek().outputTime;
            }
        }
    }

    protected virtual void Awake()
    {
        TimePhysics.timePhysicsColliders.Add(this);
    }

    protected virtual void OnDestroy()
    {
        TimePhysics.timePhysicsColliders.Remove(this);
    }

    public void Add(TimestampedData<Vector3> data)
    {
        Assert.IsTrue(positionHistory.Count == 0 || positionHistory.Peek().outputTime < data.outputTime, "Data needs to be in chronological order");

        if (positionHistory.Count == 0)
        {
            beginTime = data.outputTime;
        }

        positionHistory.Push(data);
    }

    public void Clear()
    {
        positionHistory.Clear();
        beginTime = -1;
    }

    public bool TimeInRange(double time)
    {
        if (positionHistory.Count <= 1)
        {
            return false;
        }
        return time > BeginTime && time < EndTime;
    }

    public Vector3 PositionAtTime(double time)
    {
        if (positionHistory.Count == 0)
        {
            throw new System.InvalidOperationException("TimeCollider has no data");
        }
        else if (positionHistory.Count == 1 || time > positionHistory.Peek().outputTime)
        {
            return positionHistory.Peek();
        }

        TimestampedData<Vector3> end = null;

        foreach (TimestampedData<Vector3> data in PositionHistory)
        {
            if (time > data.outputTime)
            {
                float lerpValue = Mathd.InverseLerp(data.outputTime, end.outputTime, time);
                return Vector3.Lerp(data, end, lerpValue);
            }
            else
            {
                end = data;
            }
        }

        return end; //we have reached the end of all the data we have
    }

    public abstract bool raycastHit(Vector3 rayOrigin, Vector3 rayUnitDirection, double time);

    public bool raycastHit(Ray ray, double time)
    {
        return raycastHit(ray.origin, ray.direction, time);
    }

    protected abstract void OnDrawGizmosSelected();
}