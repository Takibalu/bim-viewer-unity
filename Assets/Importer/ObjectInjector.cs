using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;
using Dummiesman;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Importer
{
    public class ObjectInjector : MonoBehaviour
    {
        private GameObject loadedObj;
        private XmlDocument loadedXML;
        private string fileName;

        // Loads a model and its associated XML file to create a complete hierarchy.
        public GameObject Load(string localDownloadPath, string fileName, GameObject currentModel)
        {
            this.fileName = Path.GetFileNameWithoutExtension(fileName); // Store the file name without extension
            string objFilePath = Path.Combine(localDownloadPath, this.fileName + ".obj");
            string mtlFilePath = Path.Combine(localDownloadPath, this.fileName + ".mtl");
            string xmlFilePath = Path.Combine(localDownloadPath, this.fileName + ".xml");

            if (File.Exists(objFilePath))
            {
                // Load the OBJ model
                currentModel = LoadObj(objFilePath, mtlFilePath);
                Debug.Log($"OBJ and MTL model loaded successfully: {currentModel.name}");
            }
            else
            {
                Debug.LogError($"OBJ file not found at path: {objFilePath}");
                return null; // Return early if OBJ file is missing
            }

            if (File.Exists(xmlFilePath))
            {
                // Load the XML structure and attach it to the currentModel
                LoadXML(xmlFilePath, currentModel);
                Debug.Log($"XML model loaded successfully: {currentModel.name}");
            }
            else
            {
                Debug.LogWarning($"XML file not found at path: {xmlFilePath}");
            }

            return currentModel;
        }

        // Loads an OBJ file, optionally with an MTL file, and returns the GameObject.
        private GameObject LoadObj(string path, string mtlPath)
        {
            var objLoader = new OBJLoader();

            // Check if the MTL file exists
            if (!string.IsNullOrEmpty(mtlPath) && File.Exists(mtlPath))
            {
                loadedObj = objLoader.Load(path, mtlPath);
            }
            else
            {
                loadedObj = objLoader.Load(path);
            }

            if (!loadedObj)
            {
                Debug.LogError("Failed to load OBJ file.");
            }
            return loadedObj;
        }

        // Loads an XML file for IFC structure and sets its parent to the provided parent GameObject.
        private void LoadXML(string file, GameObject parentModel)
        {
            if (!File.Exists(file))
            {
                Debug.LogError($"XML file not found at path: {file}");
                return;
            }

            loadedXML = new XmlDocument();
            try
            {
                loadedXML.Load(file);
                Debug.Log("XML file loaded successfully.");

                string basePath = "//ifc/decomposition";

                // Process the XML nodes and attach them to the parent model
                foreach (XmlNode node in loadedXML.SelectNodes(basePath + "/IfcProject"))
                {
                    AddElements(node, parentModel);
                }
            }
            catch (XmlException ex)
            {
                Debug.LogError($"Error loading XML file: {ex.Message}");
            }
        }

        // Recursively adds elements from the XML to the GameObject hierarchy.
        private void AddElements(XmlNode node, GameObject parent)
        {
            if (node.Attributes == null )
            {
                Debug.LogWarning("Node skipped: Missing attributes.");
                return;
            }

            string nameOfElement = node.Attributes["Name"]?.Value ?? "Unnamed";
            GameObject goElement;
            //For debug purpose
            
            // string allAttributes = string.Join(", ", 
            //     node.Attributes.Cast<XmlAttribute>()
            //         .Select(attr => $"{attr.Name}=\"{attr.Value}\""));
            //
            // Debug.Log($"Node Attributes: {allAttributes}");
            
            if (node.Attributes["id"] != null)
            {
                string id = node.Attributes["id"].Value;
                
                // Check if a GameObject with the same name exists
                goElement = GameObject.Find(fileName + "/" + id);
                // If no GameObject found, create a new one
                if (!goElement)
                {
                    goElement = new GameObject(nameOfElement);
                }
            }
            else
            {
                goElement = new GameObject(nameOfElement);
            }
            // Set the parent
            if (goElement)
            {
                // Link the object to the parent we received
                if (parent)
                    goElement.transform.SetParent(parent.transform);
                AddProperties(node, goElement);
                // Go through children (recursively)
                foreach (XmlNode child in node.ChildNodes)
                    AddElements(child, goElement);
            }
        }
        // Add properties to the element
        private void AddProperties(XmlNode node, GameObject go)
        {
            XMLDataModel xmlDataModel = go.AddComponent(typeof(XMLDataModel)) as XMLDataModel;

            if (xmlDataModel)
            {
                xmlDataModel.IFCClass = node.Name;
                if (node.Attributes != null)
                {
                    if (node.Attributes["id"] != null)
                        xmlDataModel.Id = node.Attributes["id"].Value;
                    if (node.Attributes["Name"] != null) 
                        xmlDataModel.Name = node.Attributes["Name"].Value;
                    if (node.Attributes["Description"] != null)
                        xmlDataModel.Description = node.Attributes["Description"].Value;
                }
                xmlDataModel.propertySets ??= new List<IFCPropertySet>();
                xmlDataModel.quantitySets ??= new List<IFCPropertySet>();
            }

            // Go through child nodes
            foreach (XmlNode child in node.ChildNodes)
            {
                switch (child.Name)
                {
                    case "IfcPropertySet":
                    case "IfcElementQuantity":
                        if (child.Attributes != null)
                        {
                            var link = child.Attributes["xlink:href"].Value.TrimStart('#');
                            var path = $"//ifc/properties/IfcPropertySet[@id='{link}']";
                            if (child.Name == "IfcElementQuantity")
                                path = $"//ifc/quantities/IfcElementQuantity[@id='{link}']";
                            var propertySet = loadedXML.SelectSingleNode(path);
                            if (propertySet is { Attributes: not null })
                            {
                                if (propertySet.Attributes["Name"] != null)
                                    Debug.Log($"PropertySet = {propertySet.Attributes["Name"].Value}");

                                // initialize this propertyset (Name, Id)
                                var myPropertySet = new IFCPropertySet();
                                if (propertySet.Attributes["Name"] != null) 
                                    myPropertySet.setName = TransformPropSetNameString(propertySet.Attributes["Name"].Value);
                                if (propertySet.Attributes["id"] != null) 
                                    myPropertySet.setId = propertySet.Attributes["id"].Value;
                                myPropertySet.properties ??= new List<IFCProperty>();

                                // run through property values
                                foreach (XmlNode property in propertySet.ChildNodes)
                                {
                                    string propName = "", propValue = "";
                                    IFCProperty myProp = new IFCProperty();
                                    if (property.Attributes["Name"] != null) 
                                        propName = property.Attributes["Name"].Value;
                                    if (property.Name != null)
                                    {
                                        if (property.Name == "IfcPropertySingleValue" &&
                                            property.Attributes["NominalValue"] != null)
                                        {
                                            // The name is Reference, so the propertyset name should be the name 
                                            if (property.Attributes["Name"] != null &&
                                                property.Attributes["Name"].Value == "Reference")
                                            {
                                                propName = property.Attributes["NominalValue"].Value;
                                                propValue = null;
                                                // Volume case
                                                if (IsMatchVolume(propName))
                                                {
                                                    propValue = propName;
                                                    propName = "Volume";
                                                }
                                            }
                                            else
                                                propValue = property.Attributes["NominalValue"].Value;
                                        }

                                        if (property.Name == "IfcQuantityLength" &&
                                            property.Attributes["LengthValue"] != null)
                                            propValue = property.Attributes["LengthValue"].Value;
                                        if (property.Name == "IfcQuantityArea" &&
                                            property.Attributes["AreaValue"] != null)
                                            propValue = property.Attributes["AreaValue"].Value;
                                        if (property.Name == "IfcQuantityVolume" &&
                                            property.Attributes["VolumeValue"] != null)
                                            propValue = property.Attributes["VolumeValue"].Value;
                                    }

                                    myProp.name = propName;
                                    myProp.value = propValue;
                                    myPropertySet.properties.Add(myProp);
                                }
                                
                                // add propertyset and quantityset to XMLDataModel
                                if (child.Name == "IfcPropertySet")
                                    if (xmlDataModel)
                                        xmlDataModel.propertySets.Add(myPropertySet);
                                if (child.Name == "IfcElementQuantity")
                                    if (xmlDataModel)
                                        xmlDataModel.quantitySets.Add(myPropertySet);
                            }
                        }

                        break;
                } 
            }
        }
        // Transform Property set name
        static string TransformPropSetNameString(string input)
        {
            // Remove the "Pset_" prefix
            string withoutPrefix = input.Replace("Pset_", "");

            // Insert spaces before each capital letter, except the first one
            string withSpaces = Regex.Replace(withoutPrefix, "(?<!^)([A-Z])", " $1");

            return withSpaces;
        }
        // Check for volume like data
        static bool IsMatchVolume(string input)
        {
            string pattern = @"^\d+\s*x\s*\d+\s*x\s*\d+\s*mm$";
            return Regex.IsMatch(input, pattern);
        }
    }
}
