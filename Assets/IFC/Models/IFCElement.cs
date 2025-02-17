using System.Collections.Generic;
using UnityEngine;

namespace IFC.Models
{
    // Essential IFC categories we care about
    public enum IFCCategory
    {
        Wall,
        Slab,
        Floor,
        Column,
        Beam,
        Door,
        Window,
        Roof,
        Other
    }

    // Simplified element class focusing on essential data
    public class IFCElement
    {
        public string GlobalId { get; set; }
        public string Name { get; set; }
        public IFCCategory Category { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Scale { get; set; }
        public Quaternion Rotation { get; set; }
        public Dictionary<string, string> CoreProperties { get; } = new();
        
        public static IFCCategory SetCategory(string category)
        {
            return category.ToUpper() switch
            {
                "WALL" => IFCCategory.Wall,
                "WALLSTANDARDCASE" => IFCCategory.Wall,
                "SLAB" => IFCCategory.Slab,
                "FLOOR" => IFCCategory.Floor,
                "BEAM" => IFCCategory.Beam,
                "COLUMN" => IFCCategory.Column,
                "WINDOW" => IFCCategory.Window,
                "DOOR" => IFCCategory.Door,
                "ROOF" => IFCCategory.Roof,
                _ => IFCCategory.Other // Default to "OTHER" if no match
            };
        }
    }

    
}