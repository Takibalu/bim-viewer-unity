using UnityEngine;

namespace IFC.Utils
{
    public static class MeshGenerator
    {
        public static Mesh CreateBoxMesh(float width, float height, float depth)
        {
            var mesh = new Mesh();
            
            // Vertices
            var vertices = new Vector3[]
            {
                new(-width/2, -height/2, -depth/2),
                new(width/2, -height/2, -depth/2),
                new(width/2, height/2, -depth/2),
                new(-width/2, height/2, -depth/2),
                new(-width/2, -height/2, depth/2),
                new(width/2, -height/2, depth/2),
                new(width/2, height/2, depth/2),
                new(-width/2, height/2, depth/2)
            };

            // Triangles
            var triangles = new[]
            {
                0, 2, 1, //face front
                0, 3, 2,
                2, 3, 6, //face top
                3, 7, 6,
                1, 2, 5, //face right
                2, 6, 5,
                0, 7, 3, //face left
                0, 4, 7,
                5, 6, 7, //face back
                5, 7, 4,
                0, 1, 5, //face bottom
                0, 5, 4
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            
            return mesh;
        }

        public static Mesh CreatePlaneMesh(float width, float depth)
        {
            var mesh = new Mesh();
            
            // Vertices
            var vertices = new Vector3[]
            {
                new(-width/2, 0, -depth/2),
                new(width/2, 0, -depth/2),
                new(width/2, 0, depth/2),
                new(-width/2, 0, depth/2)
            };

            // Triangles
            var triangles = new int[6]
            {
                0, 2, 1,
                0, 3, 2
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            
            return mesh;
        }
    }
}