using Speckle.Core.Models;
using Speckle.Core.Api;
using UnityEngine;
using Speckle.ConnectorUnity.Wrappers;
using System.Collections.Generic;
using System.Text;

public class Parameters : MonoBehaviour
{
    public GameObject targetObject;

    void Start()
    {
        if (targetObject == null)
        {
            Debug.LogError("Target object is not assigned!");
            return;
        }

        SpeckleProperties props = targetObject.GetComponent<SpeckleProperties>();

        if (props != null && props.Data != null)
        {
            StringBuilder logMessage = new StringBuilder("Data properties:\n");

            foreach (KeyValuePair<string, object> entry in props.Data)
            {
                logMessage.AppendLine($"Key: {entry.Key}, Value: {entry.Value}");
            }

            Debug.Log(logMessage.ToString());
        }
        else
        {
            Debug.LogError("SpeckleProperties component or Data dictionary is missing.");
        }
    }
}
