using System.Text;
using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.UI;

namespace Importer
{
    public class ClickEventHandler : MonoBehaviour
    {
        [SerializeField] public TextMeshProUGUI outputText; //Reference to the Text 
        [SerializeField] public GameObject canvas; // Reference to the Canvas (with Text and Panel)
        [SerializeField] public Button hideButton; // Reference to the Button for hiding the Canvas

        public Camera camera1;

        private void Start()
        {
            camera1 = Camera.main;
            // Make sure the canvas is initially invisible
            if (canvas)
                canvas.SetActive(false);

            // Assign button click event to hide the Canvas
            if (hideButton)
                hideButton.onClick.AddListener(HideCanvas);
        }

        void Update()
        {
            // Check for mouse click
            if (Input.GetMouseButtonDown(0))
            {
                // Create a ray from the mouse position
                if (camera1)
                {
                    Ray ray = camera1.ScreenPointToRay(Input.mousePosition);

                    // Perform raycast to detect object with MeshCollider
                    if (Physics.Raycast(ray, out var hit))
                    {
                        // Check if the hit object has a MeshCollider
                        MeshCollider meshCollider = hit.collider as MeshCollider;
                        if (meshCollider)
                        {
                            // Try to get the XMLDataModel component
                            XMLDataModel xmlDataModel = hit.collider.GetComponent<XMLDataModel>();
                    
                            if (xmlDataModel)
                            {
                                // Clear previous text
                                outputText.text = "";

                                // Create a StringBuilder to build the output
                                StringBuilder sb = new StringBuilder();
                                
                                // Add basic IFC data
                                sb.AppendLine("<b>IFC Basic Information:</b>");

                                if (!string.IsNullOrEmpty(xmlDataModel.IFCClass))
                                    sb.AppendLine($"IFC Class: {xmlDataModel.IFCClass}");

                                if (!string.IsNullOrEmpty(xmlDataModel.Name))
                                    sb.AppendLine($"Name: {xmlDataModel.Name}");

                                if (!string.IsNullOrEmpty(xmlDataModel.Id))
                                    sb.AppendLine($"STEP ID: {xmlDataModel.Id}");

                                if (xmlDataModel.Index != null) // Assuming Index is a nullable type
                                    sb.AppendLine($"STEP Index: {xmlDataModel.Index}");

                                if (!string.IsNullOrEmpty(xmlDataModel.Description))
                                    sb.AppendLine($"Description: {xmlDataModel.Description}");

                                if (!string.IsNullOrEmpty(xmlDataModel.Layer))
                                    sb.AppendLine($"Layer: {xmlDataModel.Layer}\n");


                                // Add Property Sets
                                if (xmlDataModel.propertySets is { Count: > 0 })
                                {
                                    sb.AppendLine("<b>Property Sets:</b>");
                                    foreach (var propSet in xmlDataModel.propertySets)
                                    {
                                        sb.AppendLine($"Property Set Name: {propSet.setName}");
                                        sb.AppendLine($"Property Set ID: {propSet.setId}");
                                
                                        if (propSet.properties != null)
                                        {
                                            foreach (var prop in propSet.properties)
                                            {
                                                sb.AppendLine($"  - {prop.name}: {prop.value}");
                                            }
                                        }
                                        sb.AppendLine(); // Extra line between property sets
                                    }
                                }

                                // Add Quantity Sets
                                if (xmlDataModel.quantitySets is { Count: > 0 })
                                {
                                    sb.AppendLine("<b>Quantity Sets:</b>");
                                    foreach (var quantitySet in xmlDataModel.quantitySets)
                                    {
                                        sb.AppendLine($"Quantity Set Name: {quantitySet.setName}");
                                        sb.AppendLine($"Quantity Set ID: {quantitySet.setId}");
                                
                                        if (quantitySet.properties != null)
                                        {
                                            foreach (var prop in quantitySet.properties)
                                            {
                                                sb.AppendLine($"  - {prop.name}: {prop.value}");
                                            }
                                        }
                                        sb.AppendLine(); // Extra line between quantity sets
                                    }
                                }

                                // Set the final text
                                outputText.text = sb.ToString();
                                Debug.Log(outputText.text);
                                // Make the canvas visible when a model is clicked
                                if (canvas)
                                    canvas.SetActive(true);
                            }
                            else
                            {
                                outputText.text = "No IFC data found on this object.";
                            }
                        }
                    }
                }
            }
        }
        // Handler to hide the Canvas
        private void HideCanvas()
        {
            if (canvas)
                canvas.SetActive(false);
        }
    }
}