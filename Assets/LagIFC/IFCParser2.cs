using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LagIFC
{
    public class IFCParser2 : MonoBehaviour
    {
        // Dictionary to store all IFC elements by their ID
        private Dictionary<string, IFCElement> elements = new Dictionary<string, IFCElement>();
        // Dictionary to store material mappings
        private Dictionary<string, Material> materialCache = new Dictionary<string, Material>();
    
        // Basic IFC element class to store properties
        public class IFCElement
        {
            public string GlobalId { get; set; }
            public string Type { get; set; }
            public string Name { get; set; }
            public Dictionary<string, object> Properties { get; set; }
            public List<Vector3> Vertices { get; set; }
            public List<int> Indices { get; set; }
            public string MaterialName { get; set; }
            public string ParentId { get; set; }
        
            public IFCElement()
            {
                Properties = new Dictionary<string, object>();
                Vertices = new List<Vector3>();
                Indices = new List<int>();
            }
        }

        public GameObject ParseIFCData(byte[] ifcData)
        {
            // Create root GameObject to hold the IFC model
            GameObject ifcRoot = new GameObject("IFC_Model");
        
            try
            {
                using (MemoryStream stream = new MemoryStream(ifcData))
                using (StreamReader reader = new StreamReader(stream))
                {
                    while (reader.ReadLine() is { } line)
                    {
                        ParseLine(line);
                    }
                }

                // Create GameObjects hierarchy
                CreateGameObjectHierarchy(ifcRoot);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing IFC data: {e.Message}");
                return null;
            }

            return ifcRoot;
        }

        private void ParseLine(string line)
        {
            if (string.IsNullOrEmpty(line) || !line.StartsWith("#")) return;

            // Basic IFC line parser
            // Format: #ID = IFCTYPE(param1, param2, ...)
            string[] parts = line.Split('=');
            if (parts.Length != 2) return;

            string id = parts[0].Trim().TrimStart('#');
            string content = parts[1].Trim();

            // Extract type and parameters
            int typeEndIndex = content.IndexOf('(');
            if (typeEndIndex == -1) return;

            string type = content.Substring(0, typeEndIndex);
            string parameters = content.Substring(typeEndIndex + 1).TrimEnd(')');

            ProcessIFCElement(id, type, parameters);
        }

        private void ProcessIFCElement(string id, string type, string parameters)
        {
            IFCElement element = new IFCElement
            {
                GlobalId = id,
                Type = type
            };

            // Parse parameters based on IFC type
            switch (type.ToUpper())
            {
                case "IFCWALL":
                case "IFCSLAB":
                case "IFCBEAM":
                case "IFCCOLUMN":
                    ParseBuildingElement(element, parameters);
                    break;
                case "IFCMATERIAL":
                    ParseMaterial(element, parameters);
                    break;
                // Add more cases for different IFC types
            }

            elements[id] = element;
        }

        private void ParseBuildingElement(IFCElement element, string parameters)
        {
            // Split parameters and process them
            string[] paramArray = SplitIFCParameters(parameters);
        
            if (paramArray.Length >= 2)
            {
                element.Name = CleanIFCString(paramArray[1]);
            }

            // Example geometry creation (simplified)
            // In a real implementation, you would parse actual geometry data from the IFC file
            CreateBasicGeometry(element);
        }

        private void ParseMaterial(IFCElement element, string parameters)
        {
            string[] paramArray = SplitIFCParameters(parameters);
            if (paramArray.Length >= 1)
            {
                element.MaterialName = CleanIFCString(paramArray[0]);
                CreateMaterial(element.MaterialName);
            }
        }

        private void CreateBasicGeometry(IFCElement element)
        {
            // Simplified geometry creation
            // In a real implementation, you would convert IFC geometry representation to Unity mesh data
            element.Vertices = new List<Vector3>
            {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
                // Add more vertices for a complete cube
            };

            element.Indices = new List<int>
            {
                0, 1, 2,
                2, 3, 0,
                // Add more indices for a complete cube
            };
        }

        private void CreateGameObjectHierarchy(GameObject root)
        {
            foreach (var element in elements.Values)
            {
                GameObject elementObj = new GameObject($"{element.Type}_{element.Name}");
                elementObj.transform.SetParent(root.transform);

                // Create mesh if geometry exists
                if (element.Vertices.Count > 0)
                {
                    MeshFilter meshFilter = elementObj.AddComponent<MeshFilter>();
                    MeshRenderer meshRenderer = elementObj.AddComponent<MeshRenderer>();

                    Mesh mesh = new Mesh
                    {
                        vertices = element.Vertices.ToArray(),
                        triangles = element.Indices.ToArray()
                    };
                    mesh.RecalculateNormals();
                    meshFilter.mesh = mesh;

                    // Assign material if exists
                    if (!string.IsNullOrEmpty(element.MaterialName) && materialCache.ContainsKey(element.MaterialName))
                    {
                        meshRenderer.material = materialCache[element.MaterialName];
                    }
                }

                // Add component to store IFC properties
                var propertyComponent = elementObj.AddComponent<IFCPropertyContainer>();
                propertyComponent.Properties = element.Properties;
            }
        }

        private string[] SplitIFCParameters(string parameters)
        {
            // Split parameters handling nested structures and quoted strings
            // This is a simplified version - real implementation needs more robust parsing
            return parameters.Split(',')
                .Select(p => p.Trim())
                .ToArray();
        }

        private string CleanIFCString(string input)
        {
            // Remove quotes and trim whitespace
            return input.Trim('\'', '"', ' ');
        }

        private void CreateMaterial(string materialName)
        {
            if (!materialCache.ContainsKey(materialName))
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.name = materialName;
                // Set material properties based on IFC data
                materialCache[materialName] = mat;
            }
        }
    }

// Component to store IFC properties on GameObjects
    public class IFCPropertyContainer : MonoBehaviour
    {
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }
}