using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class Road
{
    public int a;
    public int b;
    public float roadWidth;
    public float buildingDepth;

    public int numberOfBuildingsOnRoadPos = 2;
    public int numberOfBuildingsOnRoadNeg = 2;

    public float posAShortening;
    public float posBShortening;
    public float negAShortening;
    public float negBShortening;

    [FormerlySerializedAs("buildingsLeft")] public bool buildingsNegatif;
    [FormerlySerializedAs("buildingsRight")] public bool buildingsPositive;
}

public class Line
{
    public Vector2 a;
    public Vector2 b;
    public Vector2 normal;
    public float leftConstraint = 0;
    public float rightConstraint = 1;
    public int parentRoad;
}

public class City : MonoBehaviour
{
    [SerializeField] public List<Vector2> intersections;
    [SerializeField] public Road[] roads;

    public Curve cornerPrefab;
    public Curve middlePrefab;

    private Dictionary<int, List<Road>> cornersRoadsLookup = new Dictionary<int, List<Road>>();

    public bool handles;

    public void Start()
    {
        List<Line> frontLinesPos = new List<Line>();
        List<Line> backLinesPos = new List<Line>();
        List<Line> frontLinesNeg = new List<Line>();
        List<Line> backLinesNeg = new List<Line>();

        for (int a = 0; a < roads.Length; a++)
        {
            Vector2 road = intersections[roads[a].b] - intersections[roads[a].a];
            Vector2 roadNormal = Normal(road);
            
            Line aFrontPos = new Line();
            aFrontPos.a = intersections[roads[a].a] + roadNormal * (roads[a].roadWidth) - (Normal(roadNormal) * roads[a].posAShortening);
            aFrontPos.b = intersections[roads[a].b] + roadNormal * (roads[a].roadWidth) + (Normal(roadNormal) * roads[a].posBShortening);
            aFrontPos.normal = -roadNormal;
            aFrontPos.parentRoad = a;
            Line aBackPos = new Line();
            aBackPos.a = intersections[roads[a].a] + roadNormal * (roads[a].roadWidth + roads[a].buildingDepth);
            aBackPos.b = intersections[roads[a].b] + roadNormal * (roads[a].roadWidth + roads[a].buildingDepth);
            aBackPos.normal = -roadNormal;
            aBackPos.parentRoad = a;

            frontLinesPos.Add(aFrontPos);
            backLinesPos.Add(aBackPos);

            Line aFrontNeg = new Line();
            aFrontNeg.a = (intersections[roads[a].a] - roadNormal * (roads[a].roadWidth))  - (Normal(roadNormal) * roads[a].negAShortening);
            aFrontNeg.b = intersections[roads[a].b] - roadNormal * (roads[a].roadWidth) + (Normal(roadNormal) * roads[a].negBShortening);
            aFrontNeg.normal = roadNormal;
            aFrontNeg.parentRoad = a;
            Line aBackNeg = new Line();
            aBackNeg.a = intersections[roads[a].a] - roadNormal * (roads[a].roadWidth + roads[a].buildingDepth);
            aBackNeg.b = intersections[roads[a].b] - roadNormal * (roads[a].roadWidth + roads[a].buildingDepth);
            aBackNeg.normal = roadNormal;
            aBackNeg.parentRoad = a;

            frontLinesNeg.Add(aFrontNeg);
            backLinesNeg.Add(aBackNeg);
        }

        for (int a = 0; a < roads.Length - 1; a++)
        {
            Vector2 roadNormalA = Normal(intersections[roads[a].b] -
                                         intersections[roads[a].a]);
            
            for (int b = a + 1; b < roads.Length; b++)
            {
                if (b == a)
                    continue;

                Vector2 roadNormalB = Normal(intersections[roads[b].b] -
                                             intersections[roads[b].a]);

                bool aPosBNeg = CheckCorner(frontLinesPos[a], frontLinesNeg[b], out Vector2 PosNegVector);
                bool aPosBNegFlip = CheckCorner(frontLinesPos[a], frontLinesNeg[b], out Vector2 PosNegFlipVector, -1);
                bool aNegBPos = CheckCorner(frontLinesNeg[a], frontLinesPos[b], out Vector2 NegPosVector);
                bool aNegBPosFlip = CheckCorner(frontLinesNeg[a], frontLinesPos[b], out Vector2 NegPosFlipVector, -1);
                bool aPosBPos = CheckCorner(frontLinesPos[a], frontLinesPos[b], out Vector2 PosPosVector);
                bool aPosBPosFlip = CheckCorner(frontLinesPos[a], frontLinesPos[b], out Vector2 PosPosFlipVector,-1);
                bool aNegBNeg = CheckCorner(frontLinesNeg[a], frontLinesNeg[b], out Vector2 NegNegVector);

                if (aPosBNeg)
                {
                    bool hit = CheckCorner(backLinesPos[a], backLinesNeg[b], out Vector2 back);

                    if (hit && (roads[a].buildingsPositive || roads[b].buildingsNegatif))
                    {
                        int[] order = new[]
                        {
                            0, 3, 1, 2
                        };
                        PlaceCornerBuildings(PosNegVector, back, frontLinesPos[a], frontLinesNeg[b], order);
                    }
                }
                
                if (aPosBNegFlip)
                {
                    bool hit = CheckCorner(backLinesPos[a], backLinesNeg[b], out Vector2 back, -1);

                    if (hit && (roads[a].buildingsPositive || roads[b].buildingsNegatif))
                    {
                        int[] order = new[]
                        {
                            0, 2, 1, 3
                        };
                        PlaceCornerBuildings(PosNegFlipVector, back, frontLinesPos[a], frontLinesNeg[b], order);
                    }
                }

                if (aNegBPos)
                {
                    bool hit = CheckCorner(backLinesNeg[a], backLinesPos[b], out Vector2 back);

                    if (hit && (roads[a].buildingsNegatif || roads[b].buildingsPositive))
                    {
                        int[] order = new[]
                        {
                            0, 3, 1, 2
                        };
                        PlaceCornerBuildings(NegPosVector, back, frontLinesNeg[a], frontLinesPos[b], order);
                    }
                }

                if (aNegBPosFlip)
                {
                    bool hit = CheckCorner(backLinesNeg[a], backLinesPos[b], out Vector2 back, -1);

                    if (hit && (roads[a].buildingsNegatif || roads[b].buildingsPositive))
                    {
                        int[] order = new[]
                        {
                            0, 2, 1, 3
                        };
                        PlaceCornerBuildings(NegPosFlipVector, back, frontLinesNeg[a], frontLinesPos[b], order);
                    }
                }

                if (aPosBPos)
                {
                    bool hit = CheckCorner(backLinesPos[a], backLinesPos[b], out Vector2 back);

                    if (hit && (roads[a].buildingsPositive || roads[b].buildingsPositive))
                    {
                        int[] order = new[]
                        {
                            0, 2, 1, 3
                        };
                        PlaceCornerBuildings(PosPosVector, back, frontLinesPos[a], frontLinesPos[b], order);
                    }
                }

                if (aPosBPosFlip)
                {
                    bool hit = CheckCorner(backLinesPos[a], backLinesPos[b], out Vector2 back, -1);

                    if (hit && (roads[a].buildingsPositive || roads[b].buildingsPositive))
                    {
                        int[] order = new[]
                        {
                            0, 3, 1, 2
                        };
                        PlaceCornerBuildings(PosPosFlipVector, back, frontLinesPos[a], frontLinesPos[b], order);
                    }
                }

                if (aNegBNeg)
                {
                    bool hit = CheckCorner(backLinesNeg[a], backLinesNeg[b], out Vector2 back);

                    if (hit && (roads[a].buildingsNegatif || roads[b].buildingsNegatif))
                    {
                        int[] order = new[]
                        {
                            0, 2, 1, 3
                        };
                        PlaceCornerBuildings(NegNegVector, back, frontLinesNeg[a], frontLinesNeg[b], order);
                    }
                }
            }
        }

        for (int i = 0; i < frontLinesPos.Count; i++)
        {
            Vector2 left = Vector2.Lerp(frontLinesPos[i].a, frontLinesPos[i].b, frontLinesPos[i].leftConstraint);
            Vector2 right = Vector2.Lerp(frontLinesPos[i].a, frontLinesPos[i].b, frontLinesPos[i].rightConstraint);

            Gizmos.color = Color.yellow;
            for (int j = 0; j < roads[frontLinesPos[i].parentRoad].numberOfBuildingsOnRoadPos; j++)
            {
                if(!roads[frontLinesPos[i].parentRoad].buildingsPositive)
                    continue;
                float t = (1f / roads[frontLinesPos[i].parentRoad].numberOfBuildingsOnRoadPos) * j;
                float tr = (1f / roads[frontLinesPos[i].parentRoad].numberOfBuildingsOnRoadPos) * (j + 1);

                Vector2 normal = Normal(frontLinesPos[i].b - frontLinesPos[i].a);
                
                List<Vector3> points = new List<Vector3>
                {
                    Conversion(Vector2.Lerp(left, right, t)),
                    Conversion(Vector2.Lerp(left, right, t) + (normal * roads[frontLinesPos[i].parentRoad].buildingDepth)),
                    Conversion(Vector2.Lerp(left, right, tr)  + (normal * roads[frontLinesPos[i].parentRoad].buildingDepth)),
                    Conversion(Vector2.Lerp(left, right, tr)),
                };

                Instantiate(middlePrefab).points = points;
            }
        }

        for (int i = 0; i < frontLinesNeg.Count; i++)
        {
            Vector2 left = Vector2.Lerp(frontLinesNeg[i].a, frontLinesNeg[i].b, frontLinesNeg[i].leftConstraint);
            Vector2 right = Vector2.Lerp(frontLinesNeg[i].a, frontLinesNeg[i].b, frontLinesNeg[i].rightConstraint);

            Gizmos.color = Color.yellow;
            for (int j = 0; j < roads[frontLinesNeg[i].parentRoad].numberOfBuildingsOnRoadNeg; j++)
            {
                if(!roads[frontLinesNeg[i].parentRoad].buildingsNegatif)
                    continue;
                float t = (1f / roads[frontLinesNeg[i].parentRoad].numberOfBuildingsOnRoadNeg) * j;
                float tr = (1f / roads[frontLinesNeg[i].parentRoad].numberOfBuildingsOnRoadNeg) * (j + 1);

                Vector2 normal = Normal(frontLinesNeg[i].a - frontLinesNeg[i].b);
                
                List<Vector3> points = new List<Vector3>
                {
                    Conversion(Vector2.Lerp(left, right, tr)),
                    Conversion(Vector2.Lerp(left, right, tr)  + (normal * roads[frontLinesNeg[i].parentRoad].buildingDepth)),
                    Conversion(Vector2.Lerp(left, right, t) + (normal * roads[frontLinesNeg[i].parentRoad].buildingDepth)),
                    Conversion(Vector2.Lerp(left, right, t)),
                };

                Instantiate(middlePrefab).points = points;
            }
        }
    }

    private void OnDrawGizmos()
    {
        test = 0;
        for (int i = 0; i < intersections.Count; i++)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(Conversion(intersections[i]), .1f);
        }

        foreach (Road road in roads)
        {
            if (cornersRoadsLookup.ContainsKey(road.a))
            {
                cornersRoadsLookup[road.a].Add(road);
            }
            else
            {
                cornersRoadsLookup.Add(road.a, new List<Road>());
                cornersRoadsLookup[road.a].Add(road);
            }

            if (cornersRoadsLookup.ContainsKey(road.b))
            {
                cornersRoadsLookup[road.b].Add(road);
            }
            else
            {
                cornersRoadsLookup.Add(road.b, new List<Road>());
                cornersRoadsLookup[road.b].Add(road);
            }
        }

        List<Line> frontLinesPos = new List<Line>();
        List<Line> backLinesPos = new List<Line>();
        List<Line> frontLinesNeg = new List<Line>();
        List<Line> backLinesNeg = new List<Line>();

        for (int a = 0; a < roads.Length; a++)
        {
            Gizmos.color = Color.white;

            Gizmos.DrawLine(Conversion(intersections[roads[a].a]), Conversion(intersections[roads[a].b]));

            Vector2 road = intersections[roads[a].b] - intersections[roads[a].a];
            Vector2 roadNormal = Normal(road);

            Gizmos.color = Color.blue;

            Gizmos.DrawLine(Conversion(intersections[roads[a].a]), Conversion(intersections[roads[a].a] + roadNormal));

            Gizmos.color = Color.red;

            Gizmos.DrawLine(Conversion(intersections[roads[a].a] + (roadNormal * roads[a].roadWidth) - (Normal(roadNormal) * roads[a].posAShortening)),
                Conversion(intersections[roads[a].b] + (roadNormal * roads[a].roadWidth) + (Normal(roadNormal) * roads[a].posBShortening)));

            Gizmos.DrawLine(Conversion(intersections[roads[a].a] - (roadNormal * roads[a].roadWidth) - (Normal(roadNormal) * roads[a].negAShortening)),
                Conversion(intersections[roads[a].b] - (roadNormal * roads[a].roadWidth) + (Normal(roadNormal) * roads[a].negBShortening)));

            Gizmos.color = Color.cyan;

            if (roads[a].buildingsPositive)
            {
                Gizmos.DrawLine(
                    Conversion(intersections[roads[a].a] + roadNormal * (roads[a].roadWidth + roads[a].buildingDepth)),
                    Conversion(intersections[roads[a].b] + roadNormal * (roads[a].roadWidth + roads[a].buildingDepth)));
            }

            if (roads[a].buildingsNegatif)
            {
                Gizmos.DrawLine(
                    Conversion(intersections[roads[a].a] -
                               roadNormal * (roads[a].roadWidth + roads[a].buildingDepth)),
                    Conversion(intersections[roads[a].b] -
                               roadNormal * (roads[a].roadWidth + roads[a].buildingDepth)));
            }


            Line aFrontPos = new Line();
            aFrontPos.a = intersections[roads[a].a] + roadNormal * (roads[a].roadWidth) - (Normal(roadNormal) * roads[a].posAShortening);
            aFrontPos.b = intersections[roads[a].b] + roadNormal * (roads[a].roadWidth) + (Normal(roadNormal) * roads[a].posBShortening);
            aFrontPos.normal = -roadNormal;
            aFrontPos.parentRoad = a;
            Line aBackPos = new Line();
            aBackPos.a = intersections[roads[a].a] + roadNormal * (roads[a].roadWidth + roads[a].buildingDepth);
            aBackPos.b = intersections[roads[a].b] + roadNormal * (roads[a].roadWidth + roads[a].buildingDepth);
            aBackPos.normal = -roadNormal;
            aBackPos.parentRoad = a;

            frontLinesPos.Add(aFrontPos);
            backLinesPos.Add(aBackPos);

            Line aFrontNeg = new Line();
            aFrontNeg.a = (intersections[roads[a].a] - roadNormal * (roads[a].roadWidth))  - (Normal(roadNormal) * roads[a].negAShortening);
            aFrontNeg.b = intersections[roads[a].b] - roadNormal * (roads[a].roadWidth) + (Normal(roadNormal) * roads[a].negBShortening);
            aFrontNeg.normal = roadNormal;
            aFrontNeg.parentRoad = a;
            Line aBackNeg = new Line();
            aBackNeg.a = intersections[roads[a].a] - roadNormal * (roads[a].roadWidth + roads[a].buildingDepth);
            aBackNeg.b = intersections[roads[a].b] - roadNormal * (roads[a].roadWidth + roads[a].buildingDepth);
            aBackNeg.normal = roadNormal;
            aBackNeg.parentRoad = a;

            frontLinesNeg.Add(aFrontNeg);
            backLinesNeg.Add(aBackNeg);
        }

        for (int a = 0; a < roads.Length - 1; a++)
        {
            Vector2 roadNormalA = Normal(intersections[roads[a].b] -
                                         intersections[roads[a].a]);

            Gizmos.color = Color.black;
            Gizmos.DrawSphere(Conversion(Vector2.Lerp(intersections[roads[a].a],intersections[roads[a].b],.5f)),.1f);

            for (int b = a + 1; b < roads.Length; b++)
            {
                if (b == a)
                    continue;
                
                Vector2 roadNormalB = Normal(intersections[roads[b].b] -
                                             intersections[roads[b].a]);
                bool aPosBNeg = CheckCorner(frontLinesPos[a], frontLinesNeg[b], out Vector2 PosNegVector);
                bool aPosBNegFlip = CheckCorner(frontLinesPos[a], frontLinesNeg[b], out Vector2 PosNegFlipVector, -1);
                bool aNegBPos = CheckCorner(frontLinesNeg[a], frontLinesPos[b], out Vector2 NegPosVector);
                bool aNegBPosFlip = CheckCorner(frontLinesNeg[a], frontLinesPos[b], out Vector2 NegPosFlipVector, -1);
                bool aPosBPos = CheckCorner(frontLinesPos[a], frontLinesPos[b], out Vector2 PosPosVector);
                bool aPosBPosFlip = CheckCorner(frontLinesPos[a], frontLinesPos[b], out Vector2 PosPosFlipVector, -1);
                bool aNegBNeg = CheckCorner(frontLinesNeg[a], frontLinesNeg[b], out Vector2 NegNegVector);

                if (aPosBNeg)
                {
                    bool hit = CheckCorner(backLinesPos[a], backLinesNeg[b], out Vector2 back);

                    if (hit)
                    {
                        DrawBuildingGizmos(PosNegVector, back, frontLinesPos[a], frontLinesNeg[b]);
                    }
                }
                
                if (aPosBNegFlip)
                {
                    bool hit = CheckCorner(backLinesPos[a], backLinesNeg[b], out Vector2 back, -1);

                    if (hit)
                    {
                        DrawBuildingGizmos(PosNegFlipVector, back, frontLinesPos[a], frontLinesNeg[b]);
                    }
                }

                if (aNegBPos)
                {
                    bool hit = CheckCorner(backLinesNeg[a], backLinesPos[b], out Vector2 back);

                    if (hit)
                    {
                        DrawBuildingGizmos(NegPosVector, back, frontLinesNeg[a], frontLinesPos[b]);
                    }
                }

                if (aNegBPosFlip)
                {
                    bool hit = CheckCorner(backLinesNeg[a], backLinesPos[b], out Vector2 back, -1);

                    if (hit)
                    {
                        DrawBuildingGizmos(NegPosFlipVector, back, frontLinesNeg[a], frontLinesPos[b]);
                    }
                }

                if (aPosBPos)
                {
                    bool hit = CheckCorner(backLinesPos[a], backLinesPos[b], out Vector2 back);

                    if (hit)
                    {
                        DrawBuildingGizmos(PosPosVector, back, frontLinesPos[a], frontLinesPos[b]);
                    }
                }

                if (aPosBPosFlip)
                {
                    bool hit = CheckCorner(backLinesPos[a], backLinesPos[b], out Vector2 back, -1);

                    if (hit)
                    {
                        DrawBuildingGizmos(PosPosFlipVector, back, frontLinesPos[a], frontLinesPos[b]);
                    }
                }

                if (aNegBNeg)
                {
                    bool hit = CheckCorner(backLinesNeg[a], backLinesNeg[b], out Vector2 back);

                    if (hit)
                    {
                        DrawBuildingGizmos(NegNegVector, back, frontLinesNeg[a], frontLinesNeg[b]);
                    }
                }
            }
        }

        for (int i = 0; i < frontLinesPos.Count; i++)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(Conversion(frontLinesPos[i].a), .1f);
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(
                Conversion(Vector2.Lerp(frontLinesPos[i].a, frontLinesPos[i].b, frontLinesPos[i].leftConstraint)), .1f);
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(
                Conversion(Vector2.Lerp(frontLinesPos[i].a, frontLinesPos[i].b, frontLinesPos[i].rightConstraint)),
                .1f);

            Vector2 left = Vector2.Lerp(frontLinesPos[i].a, frontLinesPos[i].b, frontLinesPos[i].leftConstraint);
            Vector2 right = Vector2.Lerp(frontLinesPos[i].a, frontLinesPos[i].b, frontLinesPos[i].rightConstraint);

            Vector2 normal = Normal(frontLinesPos[i].a - frontLinesPos[i].b);
            
            Gizmos.color = Color.yellow;
            for (int j = 0; j < roads[frontLinesPos[i].parentRoad].numberOfBuildingsOnRoadPos; j++)
            {
                if(!roads[frontLinesPos[i].parentRoad].buildingsPositive)
                    continue;
                
                float t = (1f / roads[frontLinesPos[i].parentRoad].numberOfBuildingsOnRoadPos) * j;
                
                float tr = (1f / roads[frontLinesPos[i].parentRoad].numberOfBuildingsOnRoadPos) * (j + 1);

                Vector2 lf = Vector2.Lerp(left, right, t);
                Vector2 rf = Vector2.Lerp(left, right, tr);

                Vector2 lb = Vector2.Lerp(left, right, t) -
                             (normal * roads[frontLinesPos[i].parentRoad].buildingDepth);
                Vector2 rb = Vector2.Lerp(left, right, tr) -
                             (normal * roads[frontLinesPos[i].parentRoad].buildingDepth);
                
                Gizmos.DrawLine(Conversion(lf), Conversion(rf));
                Gizmos.DrawLine(Conversion(rf),Conversion(rb));
                Gizmos.DrawLine(Conversion(rb),Conversion(lb));
                Gizmos.DrawLine(Conversion(lb),Conversion(lf));
            }
        }

        for (int i = 0; i < frontLinesNeg.Count; i++)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(Conversion(frontLinesNeg[i].a), .1f);
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(
                Conversion(Vector2.Lerp(frontLinesNeg[i].a, frontLinesNeg[i].b, frontLinesNeg[i].leftConstraint)), .1f);
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(
                Conversion(Vector2.Lerp(frontLinesNeg[i].a, frontLinesNeg[i].b, frontLinesNeg[i].rightConstraint)),
                .1f);

            Vector2 left = Vector2.Lerp(frontLinesNeg[i].a, frontLinesNeg[i].b, frontLinesNeg[i].leftConstraint);
            Vector2 right = Vector2.Lerp(frontLinesNeg[i].a, frontLinesNeg[i].b, frontLinesNeg[i].rightConstraint);

            Vector2 normal = Normal(frontLinesNeg[i].a - frontLinesNeg[i].b);
            
            Gizmos.color = Color.yellow;
            for (int j = 0; j < roads[frontLinesNeg[i].parentRoad].numberOfBuildingsOnRoadNeg; j++)
            {
                if(!roads[frontLinesNeg[i].parentRoad].buildingsNegatif)
                    continue;
                
                float t = (1f / roads[frontLinesNeg[i].parentRoad].numberOfBuildingsOnRoadNeg) * j;
                
                float tr = (1f / roads[frontLinesNeg[i].parentRoad].numberOfBuildingsOnRoadNeg) * (j + 1);

                Vector2 lf = Vector2.Lerp(left, right, t);
                Vector2 rf = Vector2.Lerp(left, right, tr);

                Vector2 lb = Vector2.Lerp(left, right, t) +
                                        (normal * roads[frontLinesNeg[i].parentRoad].buildingDepth);
                Vector2 rb = Vector2.Lerp(left, right, tr) +
                                        (normal * roads[frontLinesNeg[i].parentRoad].buildingDepth);
                
                Gizmos.DrawLine(Conversion(lf), Conversion(rf));
                Gizmos.DrawLine(Conversion(rf),Conversion(rb));
                Gizmos.DrawLine(Conversion(rb),Conversion(lb));
                Gizmos.DrawLine(Conversion(lb),Conversion(lf));
            }
        }
    }

    private int test;

    private void DrawBuildingGizmos(Vector2 front, Vector2 back, Line a, Line b)
    {
        test++;
        Vector2 ftb = back - front;
        Vector2 baseLeft = a.a - front;
        Vector2 baseRight = b.a - front;

        float dotLeft = Vector2.Dot(ftb, baseLeft.normalized);
        float dotRight = Vector2.Dot(ftb, baseRight.normalized);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(Conversion(front), .1f);
        Gizmos.DrawSphere(Conversion(back), .1f);
        
        //Gizmos.color = Color.magenta;
        //Gizmos.DrawSphere(Conversion(front + (baseRight.normalized * dotRight)), .1f);
        Gizmos.DrawLine(Conversion(front + (baseRight.normalized * dotRight)), Conversion(front));
        Gizmos.DrawLine(Conversion(front + (baseRight.normalized * dotRight)), Conversion(back));
        Gizmos.DrawSphere(Conversion(b.b), .1f);
        //Gizmos.color = Color.blue;
        //Gizmos.DrawSphere(Conversion(front + (baseLeft.normalized * dotLeft)), .1f);
        Gizmos.DrawLine(Conversion(front + (baseLeft.normalized * dotLeft)), Conversion(front));
        Gizmos.DrawLine(Conversion(front + (baseLeft.normalized * dotLeft)), Conversion(back));

        float leftBound = InverseLerp(a.a, a.b, front + (baseLeft.normalized * dotLeft));
        if (leftBound < .5f)
        {
            a.leftConstraint = leftBound;
        }
        else
        {
            a.rightConstraint = leftBound;
        }

        float rightBound = InverseLerp(b.a, b.b, front + (baseRight.normalized * dotRight));
        if (rightBound < .5f)
        {
            b.leftConstraint = rightBound;
        }
        else
        {
            b.rightConstraint = rightBound;
        }

        //Gizmos.color = Color.red;
        //Gizmos.DrawSphere(Conversion(ftb), .1f);
    }

    private void PlaceCornerBuildings(Vector2 front, Vector2 back, Line a, Line b, int[] windingOrder)
    {
        Vector2 ftb = back - front;
        Vector2 baseLeft = a.a - front;
        Vector2 baseRight = b.a - front;

        float dotLeft = Vector2.Dot(ftb, baseLeft.normalized);
        float dotRight = Vector2.Dot(ftb, baseRight.normalized);
        
        float leftBound = InverseLerp(a.a, a.b, front + (baseLeft.normalized * dotLeft));
        if (leftBound < .5f)
        {
            a.leftConstraint = leftBound;
        }
        else
        {
            a.rightConstraint = leftBound;
        }

        float rightBound = InverseLerp(b.a, b.b, front + (baseRight.normalized * dotRight));
        if (rightBound < .5f)
        {
            b.leftConstraint = rightBound;
        }
        else
        {
            b.rightConstraint = rightBound;
        }
        
        List<Vector3> points = new List<Vector3>
        {
            Conversion(front),
            Conversion(back),
            Conversion(front + (baseLeft.normalized * dotLeft)),
            Conversion(front + (baseRight.normalized * dotRight)),
        };

        Vector3[] pointsWound = new Vector3[4];
        
        for (int i = 0; i < windingOrder.Length; i++)
        {
            pointsWound[i] = points[windingOrder[i]];
        }
        
        Instantiate(cornerPrefab).points = pointsWound.ToList();
    }

    private bool CheckCorner(Line A, Line B, out Vector2 point, int normalRemap = 1)
    {
        Vector2 ltb = A.b - B.a;

        float ballDistance = Vector2.Dot(ltb, Normal(B.b - B.a) * normalRemap);

        //compare distance with ball radius
        if (ballDistance <= 0)
        {
            Gizmos.color = Color.yellow;
            Vector2 velocity = A.a - A.b;
            float a = Vector2.Dot((B.a - A.a), Normal(B.a - B.b) * normalRemap);
            float b = Vector2.Dot(velocity, Normal(B.b - B.a) * normalRemap);
            float t = a / b;
            Vector2 desiredPos = A.a + velocity * t;
            desiredPos = Vector2.Lerp(A.a, A.b, t);
            Vector2 lineVector = B.b - B.a;
            float lineLength = lineVector.magnitude;
            Vector2 _ballToLine = desiredPos - B.a;

            float dotProduct = Vector2.Dot(_ballToLine, lineVector.normalized);

            /*
            Debug.Log("A = " + a);
            Debug.Log("B = " + b);
            Debug.Log("T = " + t);
            Debug.Log("LineVector = " + lineVector);
            Debug.Log("LineLength = " + lineVector.magnitude);
            Debug.Log("dotProduct = " + dotProduct);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(Conversion(A.a), 0.1f);
            Gizmos.DrawSphere(Conversion(A.b), 0.1f);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(Conversion(B.a), 0.1f);
            Gizmos.DrawSphere(Conversion(B.b), 0.1f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(Conversion(desiredPos), 0.1f);

            Gizmos.DrawLine(Conversion(A.a), Conversion(A.a + A.normal));
            */

            if ((dotProduct >= 0 && dotProduct <= lineLength) && !(b <= 0) && !(a < 0))
            {
                //Gizmos.color = Color.red;
                //Gizmos.DrawSphere(Conversion(desiredPos), 0.25f);
                point = desiredPos;
                return true;
            }
        }

        //Gizmos.color = Color.red;
        //Gizmos.DrawSphere(Conversion(A.a),.1f);
        
        point = new Vector2();
        return false;
    }

    private Vector2 Normal(Vector2 input)
    {
        return new Vector2(-input.y, input.x).normalized;
    }

    public Vector3 Conversion(Vector2 vector2)
    {
        return new Vector3(vector2.x, 0, vector2.y);
    }

    public Vector2 Conversion(Vector3 vector3)
    {
        return new Vector2(vector3.x, vector3.z);
    }

    public float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        Vector3 AB = b - a;
        Vector3 AV = value - a;
        return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
    }
}