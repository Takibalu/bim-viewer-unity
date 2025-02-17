using System.IO;
using UnityEngine;
using Vuforia;

namespace Importer
{
    public class BlueprintLoader : MonoBehaviour 
    {
        public ImageTargetBehaviour LoadBlueprint(string fileName, float targetSize, GameObject currentModel = null) 
        {
            // Check if file exists
            if (!File.Exists(fileName))
            {
                Debug.LogError($"File not found: {fileName}");
                return null;
            }
            Debug.Log("Filename: " + fileName);
            // Load texture from file
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.LoadImage(File.ReadAllBytes(fileName));
            
            int targetSizePx = 1024; // Adjust this size value if necessary for picture
            Texture2D textureFile = ResizeTexture(texture, targetSizePx, targetSizePx);

            
            textureFile.Apply(true, false); // Ensure the texture is applied

            // Validate that Vuforia is initialized
            if (!VuforiaBehaviour.Instance)
            {
                Debug.LogError("Vuforia is not initialized.");
                return null;
            }

            // Create the ImageTarget
            var mTarget = VuforiaBehaviour.Instance.ObserverFactory.CreateImageTarget(
                textureFile,
                targetSize,
                Path.GetFileNameWithoutExtension(fileName)
            );

            if (!mTarget)
            {
                Debug.LogError("Failed to create ImageTargetObserver.");
                return null;
            }

            if (currentModel)
            {
                currentModel.transform.parent = mTarget.transform;
                currentModel.transform.localPosition = Vector3.zero;
                currentModel.transform.localRotation = Quaternion.Euler(-90, 0, 0);
            }
            mTarget.gameObject.AddComponent<DefaultObserverEventHandler>();
        
            Debug.Log("Instant Image Target created " + mTarget.TargetName);
        
            return mTarget;
        }
        private Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
        {
            RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
            RenderTexture.active = rt;
            Graphics.Blit(source, rt);

            Texture2D result = new Texture2D(newWidth, newHeight, TextureFormat.ARGB32, false);
            result.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
            result.Apply();

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);
            return result;
        }
    }
}