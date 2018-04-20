using System;
using System.Collections.Generic;

namespace RayTracer
{
    public class Vector3
    {
        public float x, y, z;
        public float length => (float)Math.Sqrt(x * x + y * y + z * z);
        public Vector3(float a=0f, float b=0f, float c=0f) { this.x = a; this.y = b; this.z = c; }
        public Vector3(Vector3 v) { this.x = v.x; this.y = v.y; this.z = v.z; }
        public void Normalize() { float inv = 1f / length;  x *= inv; y *= inv; z *= inv; }

        public Vector3(Vector3 p1, Vector3 p2,bool norm=false)
        {
            x = p2.x - p1.x;
            y = p2.y - p1.y;
            z = p2.z - p1.z;
            if(norm)
            {
                this.Normalize();
            }
        }
        

        public static float Dot(Vector3 u, Vector3 v)
        {
            return u.x * v.x + u.y * v.y + u.z * v.z;
        }

        public float Dot(Vector3 v)
        {
            return x * v.x + y * v.y + z * v.z;
        }

        public static Vector3 Cross(Vector3 u, Vector3 v)
        {
            return new Vector3(u.y * v.z - u.z * v.y,
                            u.z * v.x - u.x * v.z,
                            u.x * v.y - u.y * v.x);
        }

        public static Vector3 operator+(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3 operator-(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector3 operator*(Vector3 a, float b)
        {
            return new Vector3(a.x * b, a.y * b, a.z * b);
        }

        public static Vector3 operator*(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        public static Vector3 operator*(float b, Vector3 a)
        {
            return new Vector3(a.x * b, a.y * b, a.z * b);
        }

        public void mixColor(Vector3 v)
        {
            this.x = (x + v.x) / 2;
            this.y = (y + v.y) / 2;
            this.z = (z + v.z) / 2;
        }

        public void multiplyColor(Vector3 v)
        {
            this.x *= v.x;
            this.y *= v.y;
            this.z *= v.z;
        }
    }

    public class Ray
    {
        public Vector3 origin;
        public Vector3 direction;

        public Ray(Vector3 origin, Vector3 dir)
        {
            this.origin = origin;
            this.direction = dir;
        }

        public Vector3 Eval(float t)
        {
            return origin + direction * t;
        }

    }

    public class Hit
    {
        public Vector3 point;
        public Vector3 normal;
        public float t;
        //public int material;
    }

    public abstract class AbstractObject
    {
        public Material material;

        public abstract bool Intersect(Ray ray, ref Hit hit, float tmin = 0f, float tmax = float.MaxValue);
    }

    public class Triangle : AbstractObject
    {
        public Vector3 p1, p2, p3;

        public Triangle(Vector3 aP1, Vector3 aP2, Vector3 aP3, Material mat)
        {
            p1 = aP1;
            p2 = aP2;
            p3 = aP3;
            material = mat;
        }

        public override bool Intersect(Ray ray, ref Hit hit, float tmin = 0, float tmax = float.MaxValue)
        {
            //hit.point = hit.normal = new Vector3();
            //return true;
            const float EPSILON = 0.0000001f;
            Vector3 vertex0 = p1;
            Vector3 vertex1 = p2;
            Vector3 vertex2 = p3;
            Vector3 rayVector = ray.direction;
            Vector3 edge1, edge2, h, s, q;
            float a, f, u, v;
            edge1 = vertex1 - vertex0;
            edge2 = vertex2 - vertex0;
            h = Vector3.Cross(rayVector , edge2);
            a = edge1.Dot(h);
            if (a > -EPSILON && a < EPSILON)
                return false;
            f = 1 / a;
            s = ray.origin - vertex0;
            u = f * (s.Dot(h));
            if (u < 0.0 || u > 1.0)
                return false;
            q = Vector3.Cross(s , edge1);
            v = f * rayVector.Dot(q);
            if (v < 0.0 || u + v > 1.0)
                return false;
            //trouver ou a lieu l'intersection
            float t = f * edge2.Dot(q);
            if (t > EPSILON) // ray intersection
            {
                hit.point = ray.origin + rayVector * t;
                hit.normal = Vector3.Cross(edge1, edge2);
                hit.normal.Normalize();

                return true;
            }
            else //intersection de la ligne
                return false;
        }
    }

    public class Material
    {
        public Vector3 color;
        public float ambiante, diffuse, speculaire;
        
        public Material(Vector3 aColor, float anAmbiante = 0.3f, float aDiffuse=1f, float aSpeculaire=20f)
        {
            color = aColor;
            ambiante = anAmbiante;
            diffuse = aDiffuse;
            speculaire = aSpeculaire;
        }
    }

    public class Sphere : AbstractObject
    {
        public Vector3 center;
        public float radius;

        public Sphere(Vector3 vector3, float v, Material mat)
        {
            this.center = vector3;
            this.radius = v;
            this.material = mat;
        }

        public override bool Intersect(Ray ray, ref Hit hit, float tmin = 0f, float tmax = float.MaxValue)
        {
            Vector3 oc = ray.origin - center;
            // Le rayon pointe-t-il vers la sphere ?
            float b = Vector3.Dot(oc, ray.direction);
            // Le rayon est-t-il suffisamment proche ?
            float c = Vector3.Dot(oc, oc) - radius * radius;
            float discriminant = b * b - c;

            if(discriminant > 0f)
            {
                float sqrD = (float)Math.Sqrt(discriminant);
                float t = (-b - sqrD);
                if(t < tmax && t > tmin)
                {
                    hit.point = ray.Eval(t);
                    hit.normal = (hit.point - center);
                    hit.normal.Normalize();
                    return true;
                }
                t = (-b + sqrD);
                if (t < tmax && t > tmin)
                {
                    hit.point = ray.Eval(t);
                    hit.normal = (hit.point - center);
                    hit.normal.Normalize();
                    hit.t = t;
                    return true;
                }

                return false;
            }
            else
            {
                return false;
            }
        }
    }

    public class Light
    {

        public Vector3 point, color;
        public float intensite;
        public LIGHT_MODE mode;

        public enum LIGHT_MODE
        {
            AMBIANTE = 1,
            DIFFUSE = 2,
            SPECULAIRE = 3,
            PHONG = 4
        }

        public Light(Vector3 aPoint, Vector3 aColor, float anIntensite=1, LIGHT_MODE aMode= LIGHT_MODE.AMBIANTE)
        {
            mode = aMode;
            point = aPoint;
            color = aColor;
            intensite = anIntensite;
        }

    }

    public class RayCaster
    {
        private float fovY;
        private float aspectRatio;
        private float invWidth;
        private float invHeight;
        private float halfWidth;
        private float halfHeight;

        private Vector3 lightPosition = new Vector3(200f, 100f, -50f);
        private Vector3 lightPosition2 = new Vector3(-100f, -100f, -50f);
        public Vector3 DiffuseLighting(Vector3 N, Vector3 L)
        {
            return new Vector3(1f, 1f, 1f) * Math.Max(0f,Vector3.Dot(N, L));
        }

        public Vector3 DiffuseLighting2(Vector3 N, Vector3 L)
        {
            return new Vector3(1f, 0f, 1f) * Math.Max(0f, Vector3.Dot(N, L));
        }

        public Vector3 Trace(Ray ray, Sphere sphere)
        {
            Vector3 ambient = new Vector3(0f, 0f, 0f);
            Hit hit = new Hit();

            if(sphere.Intersect(ray, ref hit, 0.001f))
            {
                
                Vector3 lightDir = lightPosition - hit.point;
                lightDir.Normalize();
                float attenuation = 1f / (lightDir.length * lightDir.length);
                
                Vector3 lightDir2 = lightPosition2 - hit.point;
                lightDir2.Normalize();
                float attenuation2 = 1f / (lightDir2.length * lightDir2.length);
                
                return attenuation * DiffuseLighting(hit.normal, lightDir) + attenuation2 * DiffuseLighting2(hit.normal, lightDir2);
            }

            return ambient;
        }

        public Vector3 TracePhong(Ray ray, List<AbstractObject> abstractObjects, List<Light> lights)
        {
            Vector3 color = new Vector3();
            bool isFirstLight = true;

            List<Hit> hits = new List<Hit>();
            List<AbstractObject> objects = new List<AbstractObject>();
            List<float> distances = new List<float>();

            float minDistance = float.MaxValue;
            AbstractObject nearestObj = null;
            Hit hit = new RayTracer.Hit() ;

            foreach (AbstractObject obj in abstractObjects)
            {
                if (obj.Intersect(ray, ref hit, 0.001f))
                {
                    float distance = (hit.point.x - ray.origin.x) * (hit.point.x - ray.origin.x) +
                        (hit.point.y - ray.origin.y) * (hit.point.y - ray.origin.y) +
                        (hit.point.z - ray.origin.z) * (hit.point.z - ray.origin.z);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestObj = obj;
                    }
                    
                }
            }

            if (nearestObj == null)
                return color;

            foreach (Light l in lights)
            {
                hits.Clear();
                objects.Clear();
                distances.Clear();

                Vector3 dir = (l.point - hit.point);
                dir.Normalize();
                Ray newRay = new Ray(hit.point,dir);

                float distanceLight = (hit.point.x - l.point.x) * (hit.point.x - l.point.x) +
                            (hit.point.y - l.point.y) * (hit.point.y - l.point.y) +
                            (hit.point.z - l.point.z) * (hit.point.z - l.point.z);

                foreach (AbstractObject obj in abstractObjects)
                {
                    if (obj == nearestObj)
                        continue;
                    Hit h = new Hit();
                    if (obj.Intersect(newRay, ref h, 0.001f))
                    {
                        float distance = (h.point.x - hit.point.x) * (h.point.x - hit.point.x) +
                            (h.point.y - hit.point.y) * (h.point.y - hit.point.y) +
                            (h.point.z - hit.point.z) * (h.point.z - hit.point.z);

                        if (distance > distanceLight)
                            continue;

                        int i = 0;
                        for (; i < distances.Count && distances[i] < distance; i++) ;
                        hits.Insert(i, h);
                        objects.Insert(i, obj);
                        distances.Insert(i, distance);
                    }
                }

                float intensity = 0;
                float specular = 0;


                Material mat = nearestObj.material;
                if (l.mode == Light.LIGHT_MODE.AMBIANTE || l.mode == Light.LIGHT_MODE.PHONG)
                {
                    //Vector3 ambiant = mat.color * mat.ambiante * l.color * l.intensite;
                    intensity += mat.ambiante;
                    //color += ambiant;
                }
                if (objects.Count > 0)
                {
                    color = l.color * mat.color * l.intensite * intensity;
                    return color;
                }

                if (l.mode == Light.LIGHT_MODE.DIFFUSE || l.mode == Light.LIGHT_MODE.PHONG)
                {

                    Vector3 lightDir = l.point - hit.point;
                    lightDir.Normalize();
                    float attenuation = 1f / (lightDir.length * lightDir.length);
                    float dot = Math.Max(0f, Vector3.Dot(hit.normal, lightDir));

                    intensity += attenuation*dot*mat.diffuse;
                    //color += attenuation * l.color * dot * mat.color * mat.diffuse;

                }
                if (l.mode == Light.LIGHT_MODE.SPECULAIRE || l.mode == Light.LIGHT_MODE.PHONG)
                {
                    Vector3 lightDir = l.point - hit.point;
                    lightDir.Normalize();

                    Vector3 V = ray.origin - hit.point;
                    V.Normalize();

                    float dot = Math.Max(0f, Vector3.Dot(hit.normal, lightDir));
                    if(dot>0)
                    {
                        Vector3 R = hit.normal * 2 * Vector3.Dot(lightDir, hit.normal) - lightDir;
                        float spec = (float)Math.Pow(Vector3.Dot(V, R), mat.speculaire * l.intensite);
                        if (spec > 0)
                        {
                            specular = spec;
                        }
                    }
                        
                        //color += l.color * intensite;

                }
                if (intensity > 1)
                    intensity = 1;

                color = l.color * mat.color * l.intensite * intensity;
                color += l.color * l.intensite * specular;


            }
            if (color.x > 1)
                color.x = 1;
            if (color.y > 1)
                color.y = 1;
            if (color.z > 1)
                color.z = 1;

            return color;
        }

        public RayCaster(ref float[] backBuffer, float fovY, int width, int height, ref int totalRayCount)
        {
            this.fovY = fovY;
            this.aspectRatio = (float)width / (float)height;
            this.invWidth = 1f / (float)width;
            this.invHeight = 1f / (float)height;
            this.halfWidth = 0.5f * width;
            this.halfHeight = 0.5f * height;

            // Position et direction de la camera
            Vector3 position = new Vector3(0f, 0f, 0f);
            Vector3 direction = new Vector3(0f, 0f, 1f);
            // Position du coin bas gauche dans le repere de projection
            Vector3 up = new Vector3(0f, 1f, 0f);
            Vector3 U = Vector3.Cross(up, direction); // right de la camera
            Vector3 V = Vector3.Cross(direction, U); // up de la camera
            Vector3 bottomLeft = position - U * halfWidth - V * halfHeight;

            float radFov = (float)(Math.PI / 180f) * (fovY / 2f);
            float angle = (float)Math.Tan(radFov);

            Vector3 xInc = (U * (2f * invWidth)) * angle * aspectRatio;
            Vector3 yInc = (V * (2f * invHeight)) * angle;
            
            List<AbstractObject> obj = new List<AbstractObject>();
            obj.Add(new Sphere(new Vector3(0f, 0f, 10f), .5f, new Material(new Vector3(1, 0, 0))));
            //obj.Add(new Sphere(new Vector3(0f, 0f, 10f), .3f, new Material(new Vector3(0, 0, 1))));
            float nb = 2f;
            Material triangleMat = new Material(new Vector3(0, 1, 0), 0.2f, 1f, 20f);
            obj.Add(new Triangle(new Vector3(-nb*2, -2, 14f - nb), new Vector3(-nb * 2, -1, 14f + nb), new Vector3(nb * 2, -2, 14f - nb), triangleMat));
            obj.Add(new Triangle(new Vector3(-nb * 2, -1, 14f + nb), new Vector3(nb * 2, -1, 14f + nb), new Vector3(nb * 2, -2, 14f - nb), triangleMat));


            obj.Add(new Triangle(new Vector3(-nb * 2, -1, 14f + nb), new Vector3(-nb * 2, -1 + nb * 2, 14f + nb), new Vector3(nb * 2, -1, 14f + nb), triangleMat));
            obj.Add(new Triangle(new Vector3(-nb * 2, -1 + nb * 2, 14f + nb), new Vector3(nb * 2, -1 + nb * 2, 14f + nb), new Vector3(nb * 2, -1, 14f + nb), triangleMat));

            List<Light> lights = new List<Light>();
            lights.Add(new Light(new Vector3(10f, 5f, 0f), new Vector3(1f, 1f, 1f), 1, Light.LIGHT_MODE.PHONG));
            //lights.Add(new Light(new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 1f), 1, Light.LIGHT_MODE.PHONG));
            //lights.Add(new Light(new Vector3(0f, 0f, 0f), new Vector3(1f, 0f, 1f)));


            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float s = (2f * x * invWidth - 1f) * angle * aspectRatio;
                    float t = (2f * y * invHeight - 1f) * angle;

                    Vector3 origin = new Vector3(s, t, 0f);
                    Vector3 dir = origin - position;
                    dir.z += 1f;
                    dir.Normalize();
                    Ray ray = new Ray(origin, dir);

                    Vector3 color = TracePhong(ray, obj, lights);
                    int k = y * width * 4 + x * 4;
                    backBuffer[k + 0] = color.x;
                    backBuffer[k + 1] = color.y;
                    backBuffer[k + 2] = color.z;
                }
            }
        }
    }
}