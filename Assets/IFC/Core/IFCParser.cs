using System;
using System.Collections.Generic;
using IFC.Models;
using UnityEngine;

namespace IFC.Core
{
    public class IFCParser : MonoBehaviour
    {
       

        // Cache for mesh instances
        private readonly Dictionary<IFCCategory, Mesh> categoryMeshes = new();
        private readonly Dictionary<string, Material> materialCache = new();

        // Lists to store elements by category for batch processing
        private readonly Dictionary<IFCCategory, List<IFCElement>> categorizedElements = new();

        private IFCFileParser fileParser;

        private void Awake()
        {
            fileParser = new IFCFileParser();
        }

        public GameObject CreateOptimizedModel(byte[] ifcData)
        {
            var root = new GameObject("IFC_Model");

            try
            {
                var elements = fileParser.ParseIFCData(ifcData);
                CategorizeElements(elements);
                CreateBatchedGameObjects(root);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error creating model: {e.Message}");
                Destroy(root);
                return null;
            }

            return root;
        }

        private void CategorizeElements(List<IFCElement> elements)
        {
            categorizedElements.Clear();
            foreach (IFCCategory category in Enum.GetValues(typeof(IFCCategory)))
            {
                categorizedElements[category] = new List<IFCElement>();
            }

            foreach (var element in elements)
            {
                categorizedElements[element.Category].Add(element);
            }
        }

        private void CreateBatchedGameObjects(GameObject root)
        {
            foreach (var categoryGroup in categorizedElements)
            {
                if (categoryGroup.Value.Count == 0) continue;

                var categoryParent = new GameObject(categoryGroup.Key.ToString());
                categoryParent.transform.SetParent(root.transform);

                if (categoryGroup.Value.Count > 1)
                {
                    CreateInstancedCategory(categoryParent, categoryGroup.Key, categoryGroup.Value);
                }
                else
                {
                    CreateSingleElements(categoryParent, categoryGroup.Value);
                }
            }
        }

        private void CreateInstancedCategory(GameObject parent, IFCCategory category,
            List<IFCElement> elements)
        {
            // Create a single mesh renderer and filter for the category
            var meshFilter = parent.AddComponent<MeshFilter>();
            meshFilter.mesh = categoryMeshes[category];

            var meshRenderer = parent.AddComponent<MeshRenderer>();
            var instanceMaterial = GetMaterialForCategory(category);
            meshRenderer.material = instanceMaterial;

            // Set up instancing
            var matrices = new Matrix4x4[elements.Count];
            for (var i = 0; i < elements.Count; i++)
            {
                matrices[i] = Matrix4x4.TRS(
                    elements[i].Position,
                    elements[i].Rotation,
                    elements[i].Scale
                );
            }

            // Use Graphics.DrawMeshInstanced for rendering
            // This is much more efficient than creating individual GameObjects
            StartCoroutine(RenderInstances(meshFilter.mesh, instanceMaterial, matrices));
        }

        private System.Collections.IEnumerator RenderInstances(Mesh mesh, Material material, Matrix4x4[] matrices)
        {
            while (true)
            {
                // Draw all instances in batches of 1023 (Unity's limit)
                for (var i = 0; i < matrices.Length; i += 1023)
                {
                    var count = Mathf.Min(1023, matrices.Length - i);
                    var batch = new Matrix4x4[count];
                    Array.Copy(matrices, i, batch, 0, count);
                    Graphics.DrawMeshInstanced(mesh, 0, material, batch);
                }

                yield return new WaitForEndOfFrame();
            }
        }

        private void CreateSingleElements(GameObject parent, List<IFCElement> elements)
        {
            foreach (var element in elements)
            {
                var elementObj = new GameObject(element.Name);
                elementObj.transform.SetParent(parent.transform);
                elementObj.transform.SetPositionAndRotation(element.Position, element.Rotation);
                elementObj.transform.localScale = element.Scale;

                // Add minimal components
                var properties = elementObj.AddComponent<IFCElementProperties>();
                IFCElementProperties.Initialize(element);
            }
        }

        private Material GetMaterialForCategory(IFCCategory category)
        {
            var materialKey = category.ToString();
            if (materialCache.ContainsKey(materialKey)) return materialCache[materialKey];
            var mat = new Material(Shader.Find("Standard"))
            {
                enableInstancing = true // Important for instancing
            };
            materialCache[materialKey] = mat;

            return materialCache[materialKey];
        }
        
        

       
     
    }

// Lightweight component to store essential IFC properties
    public class IFCElementProperties : MonoBehaviour
    {
        public static void Initialize(IFCElement element)
        {
            new Dictionary<string, string>(element.CoreProperties);
        }
    }
}