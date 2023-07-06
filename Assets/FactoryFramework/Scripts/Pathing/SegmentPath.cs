using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FactoryFramework
{
    [System.Serializable]
    public class SegmentPath : IPath
    {
        public Vector3 start, end;
        public Vector3 startdir, enddir;

        public Vector3 dir, up, right;


        private bool _isValid = true;
        public bool IsValid => _isValid;
        public SegmentPath(Vector3 s, Vector3 e)
        {
            start = s;
            end = e;
            startdir = (e - s).normalized;
            enddir = startdir;

            CalculateCompanionVectors();

            _isValid = start != end;
        }
        public SegmentPath(Vector3 s, Vector3 sdir, Vector3 e, Vector3 edir)
        {
            start = s;
            end = e;
            startdir = sdir;
            enddir = edir;

            CalculateCompanionVectors();

            _isValid = start != end;
        }

        public void CalculateCompanionVectors()
        {
            dir = (end - start).normalized;
            right = Vector3.Cross(dir, Vector3.up).normalized * -1;
            up = Vector3.Cross(dir, right).normalized;
        }

        public Vector3 GetEnd() => end;

        public Vector3 GetStart() => start;

        public float GetTotalLength() => Vector3.Distance(start, end);

        public (Vector3, float) GetClosestPoint(Vector3 worldPoint)
        {
            float t0 = Vector3.Dot(dir, worldPoint - start) / Vector3.Dot(dir, dir);
            if (t0 < 0) t0 = 0;
            if (t0 > GetTotalLength()) t0 = GetTotalLength();
            Vector3 closest = start + (dir * t0);

            return (closest, t0 / GetTotalLength());
        }

        public Vector3 GetWorldPointFromPathSpace(float pathPercent) => Vector3.Lerp(start, end, pathPercent);

        public Vector3 GetDirectionAtPoint(float pathPercent)
        {
            if (pathPercent <= 0f) return startdir;
            if (pathPercent >= 1f) return enddir;
            return dir;
        }

        public Vector3 GetRightAtPoint(float pathPercent)
        {
            if (pathPercent <= 0f)
                return Vector3.Cross(startdir, Vector3.up).normalized * -1;
            if (pathPercent >= 1f)
                return Vector3.Cross(enddir, Vector3.up).normalized * -1;

            return right;
        }

        public Vector3 GetUpAtPoint(float pathPercent)
        {
            if (pathPercent <= 0f || pathPercent >= 1f)
                return Vector3.Cross(GetDirectionAtPoint(pathPercent), GetRightAtPoint(pathPercent)).normalized;

            return up;
        }

        public (Vector3, Vector3, Vector3) GetPathVectors(float pathPercent)
        {
            return (GetDirectionAtPoint(pathPercent), GetRightAtPoint(pathPercent), GetUpAtPoint(pathPercent));
        }
        public Quaternion GetRotationAtPoint(float pathPercent)
        {
            return Quaternion.LookRotation(GetDirectionAtPoint(pathPercent), GetUpAtPoint(pathPercent));
        }
    }
}
