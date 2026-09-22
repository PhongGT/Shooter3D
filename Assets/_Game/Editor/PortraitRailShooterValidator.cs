using System;
using System.Collections.Generic;
using Shooter3D;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Shooter3D.EditorTools
{
    public static class PortraitRailShooterValidator
    {
        [MenuItem("Tools/Shooter3D/Validate Portrait Rail Shooter Scene")]
        public static void ValidateScene()
        {
            var errors = new List<string>();
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/TestScene.unity", OpenSceneMode.Single);

            GameManager manager = UnityEngine.Object.FindFirstObjectByType<GameManager>();
            RailCameraController rail = UnityEngine.Object.FindFirstObjectByType<RailCameraController>();
            CrosshairController crosshair = UnityEngine.Object.FindFirstObjectByType<CrosshairController>();
            GunController gun = UnityEngine.Object.FindFirstObjectByType<GunController>();
            EnemyController[] targets = UnityEngine.Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
            CanvasScaler scaler = UnityEngine.Object.FindFirstObjectByType<CanvasScaler>();

            if (manager == null) errors.Add("GameManager is missing");
            if (Camera.main == null) errors.Add("Main Camera is missing or untagged");
            if (rail == null) errors.Add("RailCameraController is missing");
            if (crosshair == null || gun == null) errors.Add("Aim/shoot controllers are missing");
            if (targets.Length != 13) errors.Add("Expected 13 encounter targets, found " + targets.Length);
            if (scaler == null) errors.Add("CanvasScaler is missing");
            else
            {
                if (scaler.referenceResolution != new Vector2(1080f, 1920f)) errors.Add("Canvas reference resolution is not 1080x1920");
                if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize) errors.Add("Canvas scale mode is incorrect");
                if (Mathf.Abs(scaler.matchWidthOrHeight - 0.5f) > 0.001f) errors.Add("Canvas match value is not 0.5");
            }

            if (PlayerSettings.defaultInterfaceOrientation != UIOrientation.Portrait) errors.Add("Player orientation is not Portrait");
            if (PlayerSettings.allowedAutorotateToLandscapeLeft || PlayerSettings.allowedAutorotateToLandscapeRight) errors.Add("Landscape autorotation is still enabled");
            if (EditorBuildSettings.scenes.Length != 1 || EditorBuildSettings.scenes[0].path != "Assets/Scenes/TestScene.unity") errors.Add("Build Settings does not contain only TestScene");

            GameObject environment = GameObject.Find("ENVIRONMENT_ColorCoded");
            if (environment == null)
            {
                errors.Add("Environment root is missing");
            }
            else
            {
                foreach (Renderer renderer in environment.GetComponentsInChildren<Renderer>(true))
                {
                    Material material = renderer.sharedMaterial;
                    if (material == null)
                    {
                        errors.Add("Environment renderer has no material: " + renderer.name);
                        continue;
                    }
                    Color color = material.HasProperty("_BaseColor") ? material.GetColor("_BaseColor") : material.color;
                    if (color.r > 0.92f && color.g > 0.92f && color.b > 0.92f)
                    {
                        errors.Add("White environment material detected: " + material.name);
                    }
                }
            }

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
                {
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(item.gameObject) > 0)
                    {
                        errors.Add("Missing script on " + item.name);
                    }
                }
            }

            if (errors.Count > 0)
            {
                throw new BuildFailedException("[Shooter3D Validation] " + string.Join(" | ", errors));
            }

            Debug.Log("[Shooter3D Validation] PASS | TestScene | 50s rail | 13 targets | Portrait 1080x1920 | colored environment | no missing scripts");
        }
    }
}
