using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Importer
{
    public class XMLDataModel : MonoBehaviour {

        public string IFCClass;
        public string Name;
        public string Description;
        public string Id;
        public string Index;
        public string Layer;
        
        public List<IFCPropertySet> propertySets;
        public List<IFCPropertySet> quantitySets;
    }

    [System.Serializable]
    public class IFCPropertySet
    {
        public string setName = "";
        public string setId = "";

        public List<IFCProperty> properties;
    }

    [System.Serializable]
    public class IFCProperty
    {
        public string name = "";
        public string value = "";
    }
}
