using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace FactoryFramework
{
    [System.Serializable]
    public class SmartPath : IPath
    {
        [SerializeField]
        public PathAnchor start, end;
        [SerializeReference]
        public List<(IPath, float)> subPaths;
        private float turnRadius;
        private float vertTolerance;
        private float vertTurnRadius;

        private float totalLength = 0;

        private bool _isPlaceholder = false;
        private bool _isValid = true;
        public bool IsValid => _isValid;

        public SmartPath(Vector3 s, Vector3 sdir, Vector3 e, Vector3 edir, float tradius, float verticalTolerance, float vertTRadius)
        {
            start = new PathAnchor() { pos = s, forward = sdir, right = Vector3.Cross(Vector3.up, sdir) };
            end = new PathAnchor() { pos = e, forward = edir, right = Vector3.Cross(Vector3.up, edir) };

            Initialize(tradius, verticalTolerance, vertTRadius);
        }
        public SmartPath(PathAnchor s, PathAnchor e, float tradius, float verticalTolerance, float vertTRadius)
        {
            start = s;
            end = e;

            Initialize(tradius, verticalTolerance, vertTRadius);
        }
        public void Initialize(float tradius, float verticalTolerance, float vertTRadius)
        {
            turnRadius = tradius;
            vertTolerance = verticalTolerance;
            vertTurnRadius = vertTRadius;
            Solve();
            _isValid = CheckValid();
        }
        public bool CheckValid()
        {
            if (end.pos == start.pos)
                return false;

            if (float.IsNaN(totalLength) || totalLength < 0)
                return false;

            if (_isPlaceholder)
                return false;

            return true;
        }
        public void Solve()
        {
            subPaths = SolveHelper(start, end, turnRadius, vertTolerance, vertTurnRadius);
            totalLength = subPaths.Aggregate(0f, (total, next) => total + next.Item1.GetTotalLength());
            if (float.IsNaN(totalLength) || totalLength < 0)
            {
                _isPlaceholder = true;

                List<IPath> result = new List<IPath>();
                result.Add(new SegmentPath(start.pos, start.forward, end.pos, end.forward));
                subPaths = PathListToPathDistList(result);
                totalLength = subPaths.Aggregate(0f, (total, next) => total + next.Item1.GetTotalLength());
            }
        }
        public static List<(IPath, float)> SolveHelper(PathAnchor start, PathAnchor end, float turnRadius, float vertTolerance, float vertTurnRadius)
        {
            List<IPath> result;
            if (start.pos == end.pos)
            {
                //this is an invalid case, so lets just return something not buggy
                result = new List<IPath>();
                result.Add(new SegmentPath(start.pos, start.forward, start.pos + start.forward, start.forward));
                return PathListToPathDistList(result);
            }

            if ((end.pos - start.pos).normalized == start.forward && start.forward == end.forward)
            {
                //just a segment
                result = new List<IPath>();
                result.Add(new SegmentPath(start.pos, start.forward, end.pos, end.forward));
                return PathListToPathDistList(result);
            }

            //do horizontal solve
            var ll = TrySolve(start, end, turnRadius, true, true);
            var lr = TrySolve(start, end, turnRadius, true, false);
            var rl = TrySolve(start, end, turnRadius, false, true);
            var rr = TrySolve(start, end, turnRadius, false, false);

            float lldist = TotalDistFromPaths(ll);
            float lrdist = TotalDistFromPaths(lr);
            float rldist = TotalDistFromPaths(rl);
            float rrdist = TotalDistFromPaths(rr);


            if (lldist < lrdist && lldist < rldist && lldist < rrdist)
                result = ll;
            else if (lrdist < lldist && lrdist < rldist && lrdist < rrdist)
                result = lr;
            else if (rldist < lldist && rldist < lrdist && rldist < rrdist)
                result = rl;
            else
                result = rr;


            if (Mathf.Abs(start.pos.y - end.pos.y) > vertTolerance)
            {
                List<IPath> newPath = new List<IPath>();
                newPath.Add(result[0]);
                List<IPath> vertPath = TryVerticalSolve(result[1].GetStart(), result[1].GetEnd(), vertTurnRadius);
                vertPath.ForEach(p => newPath.Add(p));
                newPath.Add(result[2]);
                return PathListToPathDistList(newPath);
            }
            else
                return PathListToPathDistList(result);
        }
        public Vector3 GetEnd() => end.pos;

        public Vector3 GetStart() => start.pos;

        public float GetTotalLength() => totalLength;

        //Return closest point in worldspace as well as in path space (0-1)
        public (Vector3, float) GetClosestPoint(Vector3 worldPoint)
        {
            float minDist = float.MaxValue;
            Vector3 minPoint = Vector3.zero;
            float pathPercent = 0;

            for (int i = 0; i < subPaths?.Count; i++)
            {
                (IPath p, float d) = subPaths[i];
                (Vector3 res, float percent) = p.GetClosestPoint(worldPoint);
                if (Vector3.Distance(worldPoint, res) < minDist)
                {
                    minDist = Vector3.Distance(worldPoint, res);
                    minPoint = res;
                    float start = d;
                    float end = (i == subPaths.Count - 1 ? 1 : subPaths[i + 1].Item2);
                    pathPercent = Mathf.Lerp(start, end, percent);
                }
            }
            return (minPoint, pathPercent);
        }

        public (IPath, float) GetSubPath(float pathPercent)
        {
            if (pathPercent <= 0)
                return (subPaths[0].Item1, 0);

            for (int i = 0; i < subPaths?.Count; i++)
            {
                var (sub, startPercent) = subPaths[i];

                if (pathPercent >= startPercent && (i == subPaths.Count - 1 || pathPercent < subPaths[i + 1].Item2))
                {
                    //convert to subpath space
                    float newMin = startPercent;
                    float newMax = (i == subPaths.Count - 1 ? 1 : subPaths[i + 1].Item2);
                    float segmentRelativeLength = newMax - newMin;
                    float newPercent = (pathPercent - startPercent) / segmentRelativeLength; //Mathf.Lerp(newMin, 1, pathPercent - startPercent);
                    return (sub, newPercent);
                }
            }
            return (null, 0);
        }
        public Vector3 GetWorldPointFromPathSpace(float pathPercent)
        {
            var (sub, per) = GetSubPath(pathPercent);

            if (sub == null)
                return Vector3.zero; //bad TODO

            return sub.GetWorldPointFromPathSpace(per);
        }

        public Vector3 GetDirectionAtPoint(float pathPercent)
        {
            var (sub, per) = GetSubPath(pathPercent);

            if (sub == null)
                return Vector3.zero; //bad TODO

            return sub.GetDirectionAtPoint(per);
        }

        public Vector3 GetRightAtPoint(float pathPercent)
        {
            var (sub, per) = GetSubPath(pathPercent);

            if (sub == null)
                return Vector3.zero; //bad TODO

            return sub.GetRightAtPoint(per);
        }

        public Vector3 GetUpAtPoint(float pathPercent)
        {
            var (sub, per) = GetSubPath(pathPercent);

            if (sub == null)
                return Vector3.zero; //bad TODO

            return sub.GetUpAtPoint(per);
        }

        public (Vector3, Vector3, Vector3) GetPathVectors(float pathPercent)
        {
            var (sub, per) = GetSubPath(pathPercent);

            if (sub == null)
                return (Vector3.zero, Vector3.zero, Vector3.zero); //bad TODO

            return sub.GetPathVectors(per);
        }
        public Quaternion GetRotationAtPoint(float pathPercent)
        {
            var (sub, per) = GetSubPath(pathPercent);

            if (sub == null)
                return Quaternion.identity; //bad TODO

            return sub.GetRotationAtPoint(per);
        }



        public void RotateStart(Vector3 newForward, Vector3 newRight)
        {
            start = new PathAnchor()
            {
                pos = start.pos,
                forward = newForward,
                right = newRight
            };
            Solve();
        }
        public void RotateEnd(Vector3 newForward, Vector3 newRight)
        {
            end = new PathAnchor()
            {
                pos = end.pos,
                forward = newForward,
                right = newRight
            };
            Solve();
        }
        public void MoveStart(Vector3 newPos)
        {
            start = new PathAnchor()
            {
                pos = newPos,
                forward = start.forward,
                right = start.right
            };
            Solve();
        }
        public void MoveEnd(Vector3 newPos)
        {
            end = new PathAnchor()
            {
                pos = newPos,
                forward = end.forward,
                right = end.right
            };
            Solve();
        }

        [System.Serializable]
        public struct PathAnchor
        {
            public Vector3 pos;
            public Vector3 forward;
            public Vector3 right;
        }

        public static List<(IPath, float)> PathListToPathDistList(List<IPath> p)
        {
            List<(IPath, float)> retList = new List<(IPath, float)>();
            float totalDist = TotalDistFromPaths(p);
            float summedDist = 0;
            for (int i = 0; i < p.Count; i++)
            {
                IPath sub = p[i];
                retList.Add((sub, summedDist));

                summedDist += sub.GetTotalLength() / totalDist;
            }

            return retList;
        }
        public static float TotalDistFromPaths(List<IPath> p) => p.Aggregate(0f, (total, next) => total + next.GetTotalLength());
        public static List<IPath> TrySolve(PathAnchor start, PathAnchor end, float turnRadius, bool useStartLeft, bool useEndLeft)
        {
            Vector3 startLeftCircle = start.pos - start.right * turnRadius;
            Vector3 startRightCircle = start.pos + start.right * turnRadius;

            Vector3 endLeftCircle = end.pos - end.right * turnRadius;
            Vector3 endRightCircle = end.pos + end.right * turnRadius;

            Vector3 c1;
            Vector3 c2;
            float theta;
            int mod = 1;

            c1 = useStartLeft ? startLeftCircle : startRightCircle;
            c2 = useEndLeft ? endLeftCircle : endRightCircle;

            //this is what we're looking for
            Vector3 tan1 = Vector3.zero;
            Vector3 tan2 = Vector3.zero;

            mod = useStartLeft ? 1 : -1;


            //4 cases
            //this means we're using inner tangent
            if (useStartLeft != useEndLeft)
            {
                float part1 = Mathf.Atan2(c2.z - c1.z, c2.x - c1.x);
                float part2 = Mathf.Asin(turnRadius * 2 / Vector3Distance2D(c1, c2)) - Mathf.PI * 0.5f;
                theta = part1 + part2 * mod;

                tan1 = new Vector3(c1.x + turnRadius * Mathf.Cos(theta), c1.y, c1.z + turnRadius * Mathf.Sin(theta));
                tan2 = new Vector3(c2.x + turnRadius * Mathf.Cos(theta + Mathf.PI), c2.y, c2.z + turnRadius * Mathf.Sin(theta + Mathf.PI));
            }
            else
            {
                Vector3 dir = c2 - c1;
                dir.y = 0;
                Vector3 rightDir = Vector3.Cross(dir.normalized, Vector3.up);

                tan1 = c1 + rightDir * mod * -1 * turnRadius;
                tan2 = c2 + rightDir * mod * -1 * turnRadius;

            }

            ArcPath startArc = new ArcPath(c1, start.pos, tan1, turnRadius, start.forward, Vector3.up);
            SegmentPath midSegment = new SegmentPath(tan1, tan2);
            ArcPath endArc = new ArcPath(c2, tan2, end.pos, turnRadius, (tan2 - tan1).normalized, Vector3.up);

            return new List<IPath>() { startArc, midSegment, endArc };
        }
        public static List<IPath> TryVerticalSolve(Vector3 start, Vector3 end, float rampRadius)
        {
            Vector3 startUpCircle = start + Vector3.up * rampRadius;
            Vector3 startDownCircle = start - Vector3.up * rampRadius;

            Vector3 endUpCircle = end + Vector3.up * rampRadius;
            Vector3 endDownCircle = end - Vector3.up * rampRadius;

            Vector3 c1 = start.y > end.y ? startDownCircle : startUpCircle;
            Vector3 c2 = start.y > end.y ? endUpCircle : endDownCircle;

            float mod = start.y > end.y ? -1 : 1;

            Vector3 forward = (end - start);
            forward.y = 0;
            forward.Normalize();
            Quaternion test = Quaternion.FromToRotation(forward, Vector3.forward);
            Vector3 localC1 = Vector3.zero;
            Vector3 temp = c2 - c1;
            Vector3 localC2 = test * temp;

            //now y is y, and z is x
            float part1 = Mathf.Atan2(localC2.y - localC1.y, localC2.z - localC1.z);
            //this asin can return NaN if the path isn't possible
            float part2 = Mathf.Asin(rampRadius * 2 / Vector3.Distance(localC1, localC2)) - Mathf.PI * 0.5f;
            float theta = part1 + part2 * mod;

            Vector3 tan1 = new Vector3(0, rampRadius * Mathf.Sin(theta), rampRadius * Mathf.Cos(theta));
            Vector3 tan2 = new Vector3(0, localC2.y + rampRadius * Mathf.Sin(theta + Mathf.PI), localC2.z + rampRadius * Mathf.Cos(theta + Mathf.PI));

            test = Quaternion.Inverse(test);
            Vector3 transtan1 = test * tan1 + (c1);
            Vector3 transtan2 = test * tan2 + (c1);

            Vector3 normal = Vector3.Cross(Vector3.up, forward);
            // Debug.Log($"lc1 {localC1}   lc2 {localC2}");

            return new List<IPath>() {
            new ArcPath(c1, start, transtan1, rampRadius, (end-start).normalized, normal),
            new SegmentPath(transtan1, transtan2),
            new ArcPath(c2, transtan2, end, rampRadius, ((end-start).normalized), normal),
        };
            /*
            float part1 = Mathf.Atan2(c2.y - c1.y, c2.x - c1.x);
            float part2 = Mathf.Asin(turnRadius * 2 / Vector3Distance2D(c1, c2)) - Mathf.PI * 0.5f;
            theta = part1 + part2 * mod;

            tan1 = new Vector3(c1.x + turnRadius * Mathf.Cos(theta), c1.y, c1.z + turnRadius * Mathf.Sin(theta));
            tan2 = new Vector3(c2.x + turnRadius * Mathf.Cos(theta + Mathf.PI), c2.y, c2.z + turnRadius * Mathf.Sin(theta + Mathf.PI));
            */
        }
        public static float Vector3Distance2D(Vector3 a, Vector3 b) => Vector3.Distance(new Vector3(a.x, 0, a.z), new Vector3(b.x, 0, b.z));
    }
}

