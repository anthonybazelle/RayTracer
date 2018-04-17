using System;

namespace RayTracer
{
    public class Vector3
    {
        public float x, y, z;
        public float length => (float)Math.Sqrt(x * x + y * y + z * z);
        public Vector3(float a, float b, float c) { this.x = a; this.y = b; this.z = c; }
        public void Normalize() { x /= length; y /= length; z /= length; }
        public static float Dot(Vector3 u, Vector3 v)
        {
            return u.x * v.x + u.y * v.y + u.z * v.z;
        }
        public static Vector3 Cross(Vector3 u, Vector3 v)
        {
            return new Vector3(u.y * v.z - u.z * v.y,
                            u.z * v.x - u.x * v.z,
                            u.x * v.y - u.y * v.x);
        }

        public static Vector3 operator+(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x + b.y, a.y + b.y, a.z + b.z);
        }

        public static Vector3 operator-(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x - b.y, a.y - b.y, a.z - b.z);
        }

        public static Vector3 operator*(Vector3 a, float b)
        {
            return new Vector3(a.x * b, a.y * b, a.z * b);
        }

        public static Vector3 operator *(float b, Vector3 a)
        {
            return new Vector3(a.x * b, a.y * b, a.z * b);
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

    public class Sphere
    {
        public Vector3 center;
        public float radius;

        public Sphere(Vector3 vector3, float v)
        {
            this.center = vector3;
            this.radius = v;
        }

        public bool Intersect(Ray ray, ref Hit hit, float tmin = 0f, float tmax = float.MaxValue)
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

    public class RayCaster
    {
        private float fovY;
        private float aspectRatio;
        private float invWidth;
        private float invHeight;
        private float halfWidth;
        private float halfHeight;

        private Vector3 lightPosition = new Vector3(200f, 100f, -50f);
        private Vector3 lightPosition2 = new Vector3(50f, -50f, 0f);
        public Vector3 DiffuseLighting(Vector3 N, Vector3 L)
        {
            return new Vector3(0f, 1f, 0f) * Math.Max(0f,Vector3.Dot(N, L));
        }

        public Vector3 DiffuseLighting2(Vector3 N, Vector3 L)
        {
            return new Vector3(1f, 0f, 1f) * Math.Max(0f, Vector3.Dot(N, L));
        }

        public Vector3 Trace(Ray ray, Sphere sphere)
        {
            Vector3 ambient = new Vector3(0f, 0f, 1f);
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

        public RayCaster(ref float[] backBuffer, float fovY, int width, int height, ref int totalRayCount)
        {
            this.fovY = fovY;
            this.aspectRatio = (float)width / (float)height;
            this.invWidth = 1f / (float)width;
            this.invHeight = 1f / (float)height;
            this.halfWidth = 0.5f * width;
            this.halfHeight = 0.5f * height;

            // Position et direction de la camera
            Vector3 position = new Vector3(0f, 0f, -0.2f);
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

            Sphere sphere = new Sphere(new Vector3(0f, 0f, 10f), 1f);

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

                    Vector3 color = Trace(ray, sphere);
                    int k = y * width * 4 + x * 4;
                    backBuffer[k + 0] = color.x;
                    backBuffer[k + 1] = color.y;
                    backBuffer[k + 2] = color.z;
                }
            }
        }
    }
}