using System.Collections;
using IFC.Core;
using UnityEngine;
using UnityEngine.Networking;

namespace IFC.Download
{
    public class IFCImporter : MonoBehaviour
    {
        private const string ServerUrl = "http://127.0.0.1:8000/download/";
        public string fileName = "basic.ifc";
        private bool isFileDownloaded;
        private IFCParser parser;
        private GameObject currentModel;

        private void Start()
        {
            parser = gameObject.AddComponent<IFCParser>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) && !isFileDownloaded)
            {
                StartCoroutine(DownloadIFCFile(fileName));
            }
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private IEnumerator DownloadIFCFile(string nameOfFile)
        {
            var downloadUrl = ServerUrl + nameOfFile;
            using var request = UnityWebRequest.Get(downloadUrl);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                if (currentModel)
                    Destroy(currentModel);

                currentModel = parser.CreateOptimizedModel(request.downloadHandler.data);

                if (!currentModel) yield break;
                Debug.Log("IFC file parsed and visualized successfully!");
                isFileDownloaded = true;
            }
            else
            {
                Debug.LogError("Failed to download the file: " + request.error);
            }
        }
    }
}