using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using IFC.Models;
using UnityEngine;

public class IFCFileParser
{
    private readonly Dictionary<string, Dictionary<string, string>> elementData = new();

    // Regular expressions for parsing
    private readonly Regex elementRegex = new(@"#(\d+)\s*=\s*IFC(\w+)\s*\((.*)\)\s*;", RegexOptions.Compiled);
    private readonly Regex paramRegex = new(@"\'([^\']*)\'\s*,|([^,]+),|\'([^\']*)\'\s*$|([^,]+)$", RegexOptions.Compiled);

    public List<IFCElement> ParseIFCData(byte[] data)
    {
        List<IFCElement> elements = new List<IFCElement>();
        
        using (MemoryStream stream = new MemoryStream(data))
        using (StreamReader reader = new StreamReader(stream))
        {
            while (reader.ReadLine() is { } line)
            {
                if (line.StartsWith("#"))
                {
                    ParseLine(line);
                }
            }
        }

        // Convert parsed data to simplified elements
        foreach (var entry in elementData)
        {
            var element = ConvertToSimplifiedElement(entry.Key, entry.Value);
            if (element != null)
            {
                elements.Add(element);
            }
        }

        return elements;
    }

    private void ParseLine(string line)
    {
        var match = elementRegex.Match(line);
        if (!match.Success) return;

        string id = match.Groups[1].Value;
        string type = match.Groups[2].Value;
        string parameters = match.Groups[3].Value;
        Debug.Log("ID: " + id + Environment.NewLine + " Type: " + type 
                  + Environment.NewLine + ", Parameters: " + parameters);

        var paramList = ExtractParameters(parameters);
        
        // Store element data
        var elementProperties = new Dictionary<string, string>
        {
            { "TYPE", type }
        };

        // Add parameters based on IFC type
        switch (type.ToUpper())
        {
            case "WALL":
            case "WALLSTANDARDCASE":
            case "SLAB":
            case "BEAM":
            case "COLUMN":
            case "WINDOW":
            case "DOOR":
            case "ROOF":
                ParseBuildingElement(elementProperties, paramList);
                break;

            case "CARTESIANPOINT":
                ParseCartesianPoint(elementProperties, paramList);
                break;

            case "LOCALPLACEMENT":
                ParseLocalPlacement(elementProperties, paramList);
                break;

            case "PROPERTYSETS":
                ParsePropertySet(elementProperties, paramList);
                break;
        }

        elementData[id] = elementProperties;
    }

    private List<string> ExtractParameters(string parameters)
    {
        var matches = paramRegex.Matches(parameters);
        return matches
                     .Select(m => m.Groups
                                  .First(g => g.Success && g.Value != "")
                                  .Value.Trim('\''))
                     .ToList();
    }

    private void ParseBuildingElement(Dictionary<string, string> properties, List<string> parameters)
    {
        if (parameters.Count > 0) properties["GlobalId"] = parameters[0];
        if (parameters.Count > 1) properties["Name"] = parameters[1];
        if (parameters.Count > 2) properties["Description"] = parameters[2];
        if (parameters.Count > 3) properties["ObjectType"] = parameters[3];
        if (parameters.Count > 4) properties["ObjectPlacement"] = parameters[4];
        if (parameters.Count > 5) properties["Representation"] = parameters[5];
    }

    private void ParseCartesianPoint(Dictionary<string, string> properties, List<string> parameters)
    {
        if (parameters.Count >= 3)
        {
            properties["X"] = parameters[0];
            properties["Y"] = parameters[1];
            properties["Z"] = parameters[2];
        }
    }

    private void ParseLocalPlacement(Dictionary<string, string> properties, List<string> parameters)
    {
        if (parameters.Count > 0) properties["PlacementRelTo"] = parameters[0];
        if (parameters.Count > 1) properties["RelativePlacement"] = parameters[1];
    }

    private void ParsePropertySet(Dictionary<string, string> properties, List<string> parameters)
    {
        if (parameters.Count > 1)
        {
            properties["PropertySetName"] = parameters[0];
            properties["PropertyValues"] = string.Join("|", parameters.Skip(1));
        }
    }

    private IFCElement ConvertToSimplifiedElement(string id, Dictionary<string, string> data)
    {
        if (!data.ContainsKey("TYPE")) return null;

        var element = new IFCElement
        {
            GlobalId = id,
            Name = data.ContainsKey("Name") ? data["Name"] : $"Element_{id}",
            Category = IFCElement.SetCategory(data["TYPE"]),
            Position = ExtractPosition(data),
            Rotation = ExtractRotation(data),
            Scale = ExtractScale(data)
        };

        // Add core properties
        foreach (var prop in data)
        {
            if (IsRelevantProperty(prop.Key))
            {
                element.CoreProperties[prop.Key] = prop.Value;
            }
        }

        return element;
    }
    

    private Vector3 ExtractPosition(Dictionary<string, string> data)
    {
        // Default position
        Vector3 position = Vector3.zero;

        // Try to find position data in the element's properties
        if (data.ContainsKey("X") && data.ContainsKey("Y") && data.ContainsKey("Z"))
        {
            float.TryParse(data["X"], out float x);
            float.TryParse(data["Y"], out float y);
            float.TryParse(data["Z"], out float z);
            position = new Vector3(x, y, z);
        }

        return position;
    }

    private Quaternion ExtractRotation(Dictionary<string, string> data)
    {
        // Default rotation
        return Quaternion.identity;
    }

    private Vector3 ExtractScale(Dictionary<string, string> data)
    {
        // Default scale - can be modified based on IFC type and properties
        Vector3 scale = Vector3.one;

        // Example: Adjust scale based on element type
        if (data.ContainsKey("TYPE"))
        {
            switch (data["TYPE"].ToUpper())
            {
                case "WALL":
                case "WALLSTANDARDCASE":
                    scale = new Vector3(0.3f, 3f, 5f); // Example wall dimensions
                    break;
                case "SLAB":
                    scale = new Vector3(5f, 0.3f, 5f); // Example floor dimensions
                    break;
                // Add more cases as needed
            }
        }

        return scale;
    }

    private bool IsRelevantProperty(string propertyName)
    {
        // List of properties we want to keep
        string[] relevantProperties = {
            "GlobalId",
            "Name",
            "Description",
            "ObjectType",
            "Level",
            "Material"
        };

        return relevantProperties.Contains(propertyName);
    }
}