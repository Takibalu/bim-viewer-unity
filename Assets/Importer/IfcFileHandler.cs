using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using static Importer.Constants;

namespace Importer
{
    public class IfcFileHandler : MonoBehaviour
    {
        private string localDownloadPath;
        private readonly string downloadPath = "Resources/DownloadedFiles/";
        public string folderName = "basic";
        private bool isFileDownloaded;
        private GameObject currentModel;
        private GameObject imageGameObject;
        private ObjectInjector objectInjector;
        private BlueprintLoader blueprintLoader;
        public float imgSize = 20f;
        public int timeLimit = 15;
        public DownloadChoice downloadChoice = DownloadChoice.Replace;
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) && !isFileDownloaded)
            {
                StartCoroutine(DownloadAndConvert(folderName));
            }
        }

        void Awake()
        {
            objectInjector = gameObject.AddComponent<ObjectInjector>();
            blueprintLoader = gameObject.AddComponent<BlueprintLoader>();
            localDownloadPath = Path.Combine(Application.dataPath, downloadPath);
            imageGameObject = new GameObject("ImageTarget");
            // Ensure the folder exists
            if (!Directory.Exists(localDownloadPath))
            {
                Directory.CreateDirectory(localDownloadPath);
                Debug.Log($"Created folder: {localDownloadPath}");
            }
        }
        
        // Check file existence and get user choice
        private DownloadChoice CheckFileExistence(string downloadPath)
        {
            // If the folder or zip already exists
            if (Directory.Exists(downloadPath) || File.Exists(downloadPath + ".zip"))
            {
                return downloadChoice;
            }
            
            return DownloadChoice.Replace; // Default to download if no existing files
        }
        
        IEnumerator DownloadAndConvert(string foldername)
        {
            var dir = Path.Combine(localDownloadPath, foldername);
            if (CheckFileExistence(dir) == DownloadChoice.Skip)
            {
                // Code for skipped download   
                string filePath = Directory.GetFiles(dir, "*.ifc")[0];
                string imageFilePath = Directory.GetFiles(dir, "*.jpg")[0];
                //Load model
                currentModel = objectInjector.Load(dir, Path.GetFileName(filePath), currentModel);
                //Load picture
                blueprintLoader.LoadBlueprint(imageFilePath, imgSize, currentModel).transform.parent = imageGameObject.transform;
                
                yield break;
            }

            if (CheckFileExistence(dir) == DownloadChoice.Replace)
            {
                if (File.Exists(dir + ".zip"))
                    File.Delete(dir + ".zip");
                if (Directory.Exists(dir))
                    Directory.Delete(dir, true);
            }
                
            // Step 1: Download folder
            Task<string> downloadTask = DownloadFolderAsync(foldername);
            yield return new WaitUntil(() => downloadTask.IsCompleted); // Wait for the async task to finish

            string downloadedZipPath = downloadTask.Result;
            if (!string.IsNullOrEmpty(downloadedZipPath))
            {
                isFileDownloaded = true;
                Debug.Log($"Folder downloaded to: {downloadedZipPath}");
                
                // Step 2: Unzip folder
                string unzippedFolderPath = UnzipFolder(downloadedZipPath);

                if (!string.IsNullOrEmpty(unzippedFolderPath))
                {
                    string ifcFilePath = Directory.GetFiles(unzippedFolderPath, "*.ifc")[0];
                    string jpgFilePath = Directory.GetFiles(unzippedFolderPath, "*.jpg")[0];
                    // Step 3: Convert file
                    Task<bool> conversionTask = ConvertFileAsync(ifcFilePath);
                    yield return new WaitUntil(() => conversionTask.IsCompleted); // Wait for the async task to finish

                    if (conversionTask.Result)
                    {
                        Debug.Log("File successfully converted to OBJ and XML.");
                        // Step 4: Load model
                        currentModel = objectInjector.Load(unzippedFolderPath, Path.GetFileName(ifcFilePath), currentModel);
                        // Step 5: Load picture 
                        blueprintLoader.LoadBlueprint(jpgFilePath, imgSize, currentModel).transform.parent = imageGameObject.transform;
                    }
                    else
                    {
                        Debug.LogError("Conversion failed.");
                    }
                }
            }
            else
            {
                Debug.LogError("Failed to download file.");
            }
        }

        // Function to download a folder
        private async Task<string> DownloadFolderAsync(string foldername)
        {
            string url = $"{BaseUrl}/download/{foldername}";
            string savePath = Path.Combine(localDownloadPath,  $"{foldername}.zip");

            try
            {
                using HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    byte[] fileData = await response.Content.ReadAsByteArrayAsync();

                    // Save the file locally
                    await File.WriteAllBytesAsync(savePath, fileData);

                    return savePath;
                }

                Debug.LogError($"Failed to download file: {response.ReasonPhrase}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception during download: {ex.Message}");
                return null;
            }
        }
        // Function to unzip the downloaded folder
        private string UnzipFolder(string zipPath)
        {
            try
            {
                string extractPath = Path.Combine(localDownloadPath, Path.GetFileNameWithoutExtension(zipPath));
                
                // Unzip the file
                ZipFile.ExtractToDirectory(zipPath, extractPath);
                
                return extractPath;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception during unzipping: {ex.Message}");
                return null;
            }
        }

        // Function to trigger the file conversion
        private async Task<bool> ConvertFileAsync(string filename)
        {
            string url = $"{BaseUrl}/convert";
            string location = Path.Combine(localDownloadPath, filename);
            Debug.Log($"Converting file: {filename}");
            Debug.Log($"URL: {url}");
            try
            {
                using HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromMinutes(timeLimit); 
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("filename", filename),
                    new KeyValuePair<string, string>("location", location) 
                });
                
                Debug.Log($"Content:{content}");
                
                //Conversion start, wait for response
                HttpResponseMessage response = await client.PostAsync(url, content); 
                Debug.Log($"Response status code: {response.StatusCode}");
                Debug.Log($"Response content: {response.Content.ReadAsStringAsync().Result}");
                if (response.IsSuccessStatusCode)
                {
                    Debug.Log("Conversion triggered successfully.");
                    return true;
                }

                Debug.LogError($"Failed to trigger conversion: {response.ReasonPhrase}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception during conversion: {ex.Message}");
                return false;
            }
        }
    }
}
