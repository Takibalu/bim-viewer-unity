using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assembly_CSharp_Editor
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Networking;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.Interaction.Toolkit;

    public class IFCImporter : MonoBehaviour
    {
        public GameObject ifcPrefab;  // Prefab to instantiate
        private string fileUrl = "http://127.0.0.1:8000/download/your_file.ifc";  // URL of the IFC file
        private bool isFileDownloaded = false;

        // Update is called once per frame
        void Update()
        {
            // Check for keyboard input (Spacebar for example)
            if (Input.GetKeyDown(KeyCode.Space) && !isFileDownloaded)
            {
                StartCoroutine(DownloadAndInstantiate());
            }
        }

        // Coroutine to download the IFC file and instantiate it
        private IEnumerator DownloadAndInstantiate()
        {
            // Start the download of the IFC file
            UnityWebRequest request = UnityWebRequest.Get(fileUrl);
            yield return request.SendWebRequest();

            // Check if there was an error
            if (request.result == UnityWebRequest.Result.Success)
            {
                byte[] downloadedData = request.downloadHandler.data;

                // Instantiate the IFC prefab and apply the downloaded data (for now, just log it)
                ifcPrefab = new GameObject("IFC Object");
                ifcPrefab.AddComponent<MeshRenderer>(); // You could replace this with specific rendering logic

                // Log that the file was successfully downloaded
                Debug.Log("IFC file downloaded successfully!");

                // Set a flag that the file is downloaded so we don't trigger the action again
                isFileDownloaded = true;

            }
            else
            {
                Debug.LogError("Failed to download the file: " + request.error);
            }
        }
    }

}
