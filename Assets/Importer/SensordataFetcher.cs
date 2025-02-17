using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static Importer.Constants;

namespace Importer
{
    public class SensordataFetcher : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI sensorDataText; // Reference to the Text for displaying the data
        [SerializeField] private GameObject sensorCanvas; // Reference to the Sensor Canvas
        [SerializeField] private Button closeButton; // Button to close the Sensor Canvas
        [SerializeField] private GameObject buttonCanvas; // Reference to the Canvas which holds the button
        [SerializeField] private Button openButton; // Button to open the Sensor Canvas

        void Start()
        {
            // Start fetching data repeatedly
            StartCoroutine(FetchDataCoroutine());
            // Set initial Canvas state to be invisible
            if (sensorCanvas)
                sensorCanvas.SetActive(false);
            if (buttonCanvas)
                buttonCanvas.SetActive(true);

            // Button click events to open/close the canvas
            if (openButton)
                openButton.onClick.AddListener(OpenCanvas);

            if (closeButton)
                closeButton.onClick.AddListener(CloseCanvas);
        }

        IEnumerator FetchDataCoroutine()
        {
            while (true)
            {
                using (UnityWebRequest request = UnityWebRequest.Get(BaseUrl + "/sensordata"))
                {
                    // Send request and wait for response
                    yield return request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        
                        // Parse the received data
                        string data = request.downloadHandler.text;
                        // Parse the JSON data into an object
                        SensorData sensorData = JsonUtility.FromJson<SensorData>(data);

                        // Parse and log the received data
                        Debug.Log(data);
                        if (sensorDataText)
                        {
                            sensorDataText.text = $"Name: {sensorData.name}\nValue: {sensorData.value}";
                        }
                    }
                    else
                    {
                        Debug.LogError("Failed to fetch data: " + request.error);
                    }
                }
                yield return new WaitForSeconds(1); // Wait 1 second between requests
            }
        }
        // Show the Canvas
        private void OpenCanvas()
        {
            if (sensorCanvas)
                sensorCanvas.SetActive(true); // Make the Canvas visible
            if (buttonCanvas)
                buttonCanvas.SetActive(false);
        }

        // Hide the Canvas
        private void CloseCanvas()
        {
            if (sensorCanvas)
                sensorCanvas.SetActive(false); // Hide the Canvas
            if (buttonCanvas)
                buttonCanvas.SetActive(true);
        }
    }
    [System.Serializable]
    public class SensorData
    {
        public string name;
        public int value;
        public string timestamp;
    }
}