using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace LagIFC
{
    public class IFCDownload : MonoBehaviour
    {
        private string serverUrl = "http://127.0.0.1:8000/download/";
        public string fileName = "basic.ifc";
        private bool isFileDownloaded;
        private IFCParser2 parser;
        private GameObject currentModel;

        void Start()
        {
            parser = gameObject.AddComponent<IFCParser2>();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) && !isFileDownloaded)
            {
                StartCoroutine(DownloadIFCFile(fileName));
            }
        }

        IEnumerator DownloadIFCFile(string fileName)
        {
            string downloadUrl = serverUrl + fileName;
            UnityWebRequest request = UnityWebRequest.Get(downloadUrl);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                byte[] downloadedData = request.downloadHandler.data;
            
                // Clean up previous model if it exists
                if (currentModel)
                {
                    Destroy(currentModel);
                }

                // Parse and create the IFC model
                currentModel = parser.ParseIFCData(downloadedData);
            
                if (currentModel)
                {
                    Debug.Log("IFC file parsed and visualized successfully!");
                    isFileDownloaded = true;
                }
                else
                {
                    Debug.LogError("Failed to parse IFC file!");
                }
            }
            else
            {
                Debug.LogError("Failed to download the file: " + request.error);
            }
        }
    }
}