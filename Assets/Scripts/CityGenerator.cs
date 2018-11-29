using System.Collections;
using System.Collections.Generic;
using Priority_Queue;
using UnityEngine;


public class CityGenerator :MonoBehaviour {

    SimplePriorityQueue<RoadQuery> q = new SimplePriorityQueue<RoadQuery>();
    List<Segment> segments;

    struct Road
    {
        public float angle;
        float length;

        public Vector2 end(Query query)
        {
            float angle = (float) (this.angle * Mathf.PI / 180.0);

            Vector2 direction = new Vector2(
                Mathf.Sin((query.prev_angle + angle)),
                Mathf.Cos((query.prev_angle + angle)));
            Vector2 end = query.origin + direction * length;
            
            return end;
        }

        public Road(float angle_in, float length_in)
        {
            angle = angle_in;
            length = length_in;
        }
    }

    

    struct Query
    {
        public Vector2 origin;
        public float prev_angle;
        public Query(Vector2 origin_in, float prev_angle_in)
        {
            origin = origin_in;
            prev_angle = prev_angle_in;
        }
    }

    struct RoadQuery
    {
        public int timer;
        public int lifetime;
        public Road road;
        public Query query;
        bool valid;

        public RoadQuery(int timer_in, int lifetime_in, Road road_in, Query query_in, bool valid_in)
        {
            timer = timer_in;
            lifetime = lifetime_in;
            road = road_in;
            query = query_in;
            valid = valid_in;

        }
    }

    struct Segment
    {
        public Vector2 start;
        public Vector2 end;

        public Segment(Vector2 start_point, Vector2 end_point)
        {
            start = start_point;
            end = end_point;
        }

        public bool Intersects(Segment other)
        {
            Vector2 compare = end - start;

            Vector2 vs = other.start - start;
            float o1 = vs.x * compare.y - vs.y * compare.x;
            vs = other.end - start;
            float o2 = vs.x * compare.y - vs.y * compare.x;

            compare = other.end - other.start;

            vs = start - other.start;
            float t1 = vs.x * compare.y - vs.x * compare.x; ;
            vs = end - other.start;
            float t2 = vs.x * compare.y - vs.x * compare.x;

            return o1 * o2 < 0 && t1 * t2 < 0;
        }
    }
   


    Segment CreateSegment(Road road, Query query)
    {
        Vector2 start = query.origin;
        Vector2 end = road.end(query);
    
        return new Segment(start, end);
    }
 
    bool LocalConstraints(RoadQuery rq, List<Segment> segments)
    {
        Vector2 start = rq.query.origin;
        Vector2 end = rq.road.end(rq.query);

        float exp = 6.0f;

        Segment current = new Segment(start, end);

        for (int i = 0; i < segments.Count; i++){
            bool intersects = current.Intersects(segments[i]);
            float err = Mathf.Abs(current.start.x - segments[i].end.x) + Mathf.Abs(current.start.y - segments[i].end.y);
            bool share = err * err < 32.0f * Mathf.Epsilon;

            if(intersects && !share)
            {
                return false;
            }

        }

        return true;
    }

    RoadQuery[] GlobalGoals(int t, int l, Road road, Query query)
    {
        double prev_angle = query.prev_angle + road.angle * Mathf.PI / 180.0;
        float angle = Random.Range(45.0f, -45.0f);
        float p = 1.0f;

        RoadQuery a = MakeRoad(road, (float)(angle - p * 90.0), t + 64, l + 3, 0.4f, (float)prev_angle, query);
        RoadQuery b= MakeRoad(road, angle, 1, l + 1, 0.5f, (float)prev_angle, query);
        RoadQuery c = MakeRoad(road, (float)(angle + p * 90.0), t + 64, l + 3, 0.4f, (float)prev_angle, query);

        return new RoadQuery[3] { a, b, c };
    }

    RoadQuery MakeRoad(Road road, float angle, int offset, int lt, float len, float prev_angle, Query query_in)
    {
        Road mouse_road = new Road(angle, len);
        Query query = new Query(road.end(query_in), prev_angle); // this is wrong
        return new RoadQuery(offset, lt, mouse_road, query, lt < 16);  
    }

    void Start()
    {
        Road ir = new Road(45.0f, 0.5f);
        Query iq = new Query(new Vector2(0, 0), 0.0f);
        RoadQuery initial_query = new RoadQuery(0, 0, ir, iq, true);
        segments = new List<Segment>();

        q.Enqueue(initial_query, initial_query.timer);
        //!(q.Count == 0)

        int count = 0;
     
        while (count < 1000)
        {
            RoadQuery rq = q.Dequeue();

            if (!LocalConstraints(rq, segments)) continue;

            segments.Add(CreateSegment(rq.road, rq.query));

            RoadQuery[] abc = GlobalGoals(rq.timer, rq.lifetime, rq.road, rq.query);
         
            q.Enqueue(abc[0], abc[0].timer);
            q.Enqueue(abc[1], abc[1].timer);
            q.Enqueue(abc[2], abc[2].timer);
            count++;
        }

    }

     void Update()
    {
        //TODO: implement line writing

        for(int i = 0; i < segments.Count; i++)
        {
          Debug.DrawLine(new Vector3(segments[i].start.x, 0, segments[i].start.y), new Vector3(segments[i].end.x, 0, segments[i].end.y), Color.white, 2.5f);
        }
        
    }

}
