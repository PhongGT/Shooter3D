using System.Collections.Generic;
using Shooter3D;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Shooter3D.EditorTools
{
    public static class PortraitRailShooterBuilder
    {
        private const string ScenePath = "Assets/Scenes/TestScene.unity";
        private const string MaterialFolder = "Assets/_Game/Materials";
        private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();

        [MenuItem("Tools/Shooter3D/Build Portrait Rail Shooter Scene")]
        public static void BuildScene()
        {
            EnsureFolders();
            CreateMaterials();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "TestScene";

            ConfigureLighting();
            GameObject environment = BuildEnvironment();
            Camera camera = BuildCamera(out RailCameraController railCamera);
            BuildLighting(environment.transform);
            EnemyController[] targets = BuildEncounters(environment.transform);
            BuildGameplay(camera, railCamera, targets);
            ConfigurePlayerSettings();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Shooter3D] Portrait rail-shooter scene built successfully: " + ScenePath +
                      " | duration=50s | targets=" + targets.Length + " | orientation=Portrait");
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "_Game");
            EnsureFolder("Assets/_Game", "Scripts");
            EnsureFolder("Assets/_Game", "Editor");
            EnsureFolder("Assets/_Game", "Materials");
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static void CreateMaterials()
        {
            Materials.Clear();
            MakeMaterial("Road_Gray", new Color(0.22f, 0.25f, 0.29f), 0.05f, 0.38f);
            MakeMaterial("Sidewalk_BlueGray", new Color(0.34f, 0.43f, 0.49f), 0.02f, 0.28f);
            MakeMaterial("Building_Ochre", new Color(0.70f, 0.42f, 0.22f), 0.02f, 0.30f);
            MakeMaterial("Roof_Indigo", new Color(0.18f, 0.20f, 0.34f), 0.05f, 0.35f);
            MakeMaterial("Window_Teal", new Color(0.08f, 0.46f, 0.53f), 0.15f, 0.75f, new Color(0.02f, 0.10f, 0.12f));
            MakeMaterial("Cover_Green", new Color(0.20f, 0.43f, 0.31f), 0.05f, 0.30f);
            MakeMaterial("Crate_Brown", new Color(0.43f, 0.25f, 0.13f), 0.02f, 0.25f);
            MakeMaterial("Metal_Orange", new Color(0.86f, 0.31f, 0.11f), 0.25f, 0.45f);
            MakeMaterial("Lamp_Navy", new Color(0.08f, 0.12f, 0.21f), 0.45f, 0.55f);
            MakeMaterial("LampGlow_Yellow", new Color(1f, 0.63f, 0.12f), 0f, 0.35f, new Color(1.4f, 0.55f, 0.05f));
            MakeMaterial("Enemy_Crimson", new Color(0.72f, 0.08f, 0.10f), 0.02f, 0.32f);
            MakeMaterial("EnemyHead_Amber", new Color(1f, 0.42f, 0.10f), 0.02f, 0.30f);
            MakeMaterial("Civilian_Cyan", new Color(0.08f, 0.66f, 0.78f), 0.02f, 0.30f);
            MakeMaterial("Danger_Back", new Color(0.12f, 0.04f, 0.05f), 0f, 0.15f);
            MakeMaterial("Danger_Fill", new Color(1f, 0.06f, 0.04f), 0f, 0.25f, new Color(1.1f, 0.02f, 0.01f));
        }

        private static Material MakeMaterial(string name, Color color, float metallic, float smoothness, Color? emission = null)
        {
            string path = MaterialFolder + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = shader;
            }

            material.SetColor("_BaseColor", color);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            if (emission.HasValue)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission.Value);
            }
            else
            {
                material.DisableKeyword("_EMISSION");
            }
            EditorUtility.SetDirty(material);
            Materials[name] = material;
            return material;
        }

        private static void ConfigureLighting()
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.20f, 0.31f, 0.46f);
            RenderSettings.ambientEquatorColor = new Color(0.30f, 0.23f, 0.21f);
            RenderSettings.ambientGroundColor = new Color(0.08f, 0.09f, 0.12f);
            RenderSettings.ambientIntensity = 1.05f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.30f, 0.40f, 0.52f);
            RenderSettings.fogStartDistance = 45f;
            RenderSettings.fogEndDistance = 115f;
        }

        private static GameObject BuildEnvironment()
        {
            GameObject root = new GameObject("ENVIRONMENT_ColorCoded");
            GameObject roadRoot = Child("Road_Gray", root.transform);
            CreatePrimitive(PrimitiveType.Cube, "Main Road", new Vector3(0f, -0.12f, 36f), new Vector3(7f, 0.24f, 92f), Materials["Road_Gray"], roadRoot.transform);
            for (int z = -4; z <= 78; z += 6)
            {
                CreatePrimitive(PrimitiveType.Cube, "Lane Marker", new Vector3(0f, 0.015f, z), new Vector3(0.12f, 0.02f, 2.5f), Materials["Metal_Orange"], roadRoot.transform, false);
            }

            GameObject sidewalkRoot = Child("Sidewalk_BlueGray", root.transform);
            CreatePrimitive(PrimitiveType.Cube, "Left Sidewalk", new Vector3(-4.5f, 0.05f, 36f), new Vector3(2f, 0.28f, 92f), Materials["Sidewalk_BlueGray"], sidewalkRoot.transform);
            CreatePrimitive(PrimitiveType.Cube, "Right Sidewalk", new Vector3(4.5f, 0.05f, 36f), new Vector3(2f, 0.28f, 92f), Materials["Sidewalk_BlueGray"], sidewalkRoot.transform);

            GameObject buildings = Child("Buildings_Ochre", root.transform);
            GameObject roofs = Child("Roofs_Indigo", root.transform);
            GameObject windows = Child("Windows_Teal", root.transform);
            for (int i = 0; i < 7; i++)
            {
                float z = i * 14f + 1f;
                float leftHeight = 7f + (i % 3) * 1.7f;
                float rightHeight = 8f + ((i + 1) % 3) * 1.5f;
                CreatePrimitive(PrimitiveType.Cube, "Building L" + i, new Vector3(-7.3f, leftHeight * 0.5f, z), new Vector3(4f, leftHeight, 12f), Materials["Building_Ochre"], buildings.transform);
                CreatePrimitive(PrimitiveType.Cube, "Building R" + i, new Vector3(7.3f, rightHeight * 0.5f, z), new Vector3(4f, rightHeight, 12f), Materials["Building_Ochre"], buildings.transform);
                CreatePrimitive(PrimitiveType.Cube, "Roof L" + i, new Vector3(-7.3f, leftHeight + 0.2f, z), new Vector3(4.3f, 0.4f, 12.3f), Materials["Roof_Indigo"], roofs.transform);
                CreatePrimitive(PrimitiveType.Cube, "Roof R" + i, new Vector3(7.3f, rightHeight + 0.2f, z), new Vector3(4.3f, 0.4f, 12.3f), Materials["Roof_Indigo"], roofs.transform);

                for (int floor = 0; floor < 3; floor++)
                {
                    float y = 1.6f + floor * 2.1f;
                    CreatePrimitive(PrimitiveType.Cube, "Window L", new Vector3(-5.27f, y, z - 2.2f), new Vector3(0.10f, 1.05f, 1.45f), Materials["Window_Teal"], windows.transform, false);
                    CreatePrimitive(PrimitiveType.Cube, "Window L", new Vector3(-5.27f, y, z + 2.2f), new Vector3(0.10f, 1.05f, 1.45f), Materials["Window_Teal"], windows.transform, false);
                    CreatePrimitive(PrimitiveType.Cube, "Window R", new Vector3(5.27f, y, z - 2.2f), new Vector3(0.10f, 1.05f, 1.45f), Materials["Window_Teal"], windows.transform, false);
                    CreatePrimitive(PrimitiveType.Cube, "Window R", new Vector3(5.27f, y, z + 2.2f), new Vector3(0.10f, 1.05f, 1.45f), Materials["Window_Teal"], windows.transform, false);
                }
            }

            GameObject covers = Child("Cover_Green", root.transform);
            Vector3[] coverPositions =
            {
                new Vector3(-1.2f, 0.55f, 8f), new Vector3(1.7f, 0.55f, 14f),
                new Vector3(-1.9f, 0.55f, 27f), new Vector3(1.8f, 0.55f, 34f),
                new Vector3(-1.8f, 0.55f, 51f), new Vector3(1.8f, 0.55f, 59f),
                new Vector3(-1.2f, 0.55f, 73f)
            };
            foreach (Vector3 position in coverPositions)
            {
                CreatePrimitive(PrimitiveType.Cube, "Tactical Cover", position, new Vector3(1.6f, 1.1f, 0.65f), Materials["Cover_Green"], covers.transform);
            }

            GameObject catwalks = Child("Catwalks_Orange", root.transform);
            foreach (float z in new[] { 21f, 45f, 68f })
            {
                CreatePrimitive(PrimitiveType.Cube, "Elevated Catwalk", new Vector3(0f, 3.15f, z), new Vector3(7f, 0.28f, 2f), Materials["Metal_Orange"], catwalks.transform);
                CreatePrimitive(PrimitiveType.Cube, "Catwalk Support L", new Vector3(-3.1f, 1.55f, z), new Vector3(0.25f, 3.1f, 0.25f), Materials["Metal_Orange"], catwalks.transform);
                CreatePrimitive(PrimitiveType.Cube, "Catwalk Support R", new Vector3(3.1f, 1.55f, z), new Vector3(0.25f, 3.1f, 0.25f), Materials["Metal_Orange"], catwalks.transform);
            }

            BuildStreetProps(root.transform);
            return root;
        }

        private static void BuildStreetProps(Transform root)
        {
            GameObject lamps = Child("StreetLamps_Navy", root);
            for (int z = 1; z <= 75; z += 12)
            {
                foreach (float x in new[] { -3.35f, 3.35f })
                {
                    CreatePrimitive(PrimitiveType.Cylinder, "Lamp Post", new Vector3(x, 1.65f, z), new Vector3(0.11f, 1.65f, 0.11f), Materials["Lamp_Navy"], lamps.transform);
                    CreatePrimitive(PrimitiveType.Sphere, "Lamp Glow", new Vector3(x, 3.35f, z), Vector3.one * 0.32f, Materials["LampGlow_Yellow"], lamps.transform, false);
                }
            }

            GameObject crates = Child("Crates_Brown", root);
            foreach (Vector3 position in new[]
            {
                new Vector3(-4f, 0.45f, 12f), new Vector3(4f, 0.45f, 26f),
                new Vector3(-4.1f, 0.45f, 41f), new Vector3(4.1f, 0.45f, 57f),
                new Vector3(-4f, 0.45f, 70f)
            })
            {
                CreatePrimitive(PrimitiveType.Cube, "Supply Crate", position, Vector3.one * 0.9f, Materials["Crate_Brown"], crates.transform);
            }
        }

        private static Camera BuildCamera(out RailCameraController rail)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 60f;
            camera.nearClipPlane = 0.12f;
            camera.farClipPlane = 180f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.20f, 0.34f, 0.50f);
            camera.allowHDR = true;
            cameraObject.AddComponent<AudioListener>();
            UniversalAdditionalCameraData data = cameraObject.AddComponent<UniversalAdditionalCameraData>();
            data.renderPostProcessing = false;

            GameObject pathRoot = new GameObject("RAIL_PATH_50_SECONDS");
            Vector3[] positions =
            {
                new Vector3(0f, 1.8f, -5f), new Vector3(0.3f, 1.8f, 7f),
                new Vector3(-0.4f, 1.95f, 19f), new Vector3(0.5f, 1.8f, 32f),
                new Vector3(-0.3f, 2.0f, 46f), new Vector3(0f, 1.9f, 60f)
            };
            Vector3[] lookTargets =
            {
                new Vector3(0f, 2.1f, 10f), new Vector3(0.2f, 3.1f, 21f),
                new Vector3(0f, 3.6f, 32f), new Vector3(-0.3f, 2.2f, 47f),
                new Vector3(0.2f, 3.6f, 60f), new Vector3(0f, 3.1f, 76f)
            };

            Transform[] points = new Transform[positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                GameObject waypoint = new GameObject("Waypoint_" + i + "_T" + (i * 10));
                waypoint.transform.SetParent(pathRoot.transform);
                waypoint.transform.position = positions[i];
                waypoint.transform.rotation = Quaternion.LookRotation((lookTargets[i] - positions[i]).normalized, Vector3.up);
                points[i] = waypoint.transform;
            }

            cameraObject.transform.SetPositionAndRotation(points[0].position, points[0].rotation);
            rail = cameraObject.AddComponent<RailCameraController>();
            rail.Configure(points, new[] { 10f, 10f, 10f, 10f, 10f });
            return camera;
        }

        private static void BuildLighting(Transform parent)
        {
            GameObject sunObject = new GameObject("Directional Light - Warm");
            sunObject.transform.SetParent(parent);
            sunObject.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
            Light sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.82f, 0.65f);
            sun.intensity = 1.55f;
            sun.shadows = LightShadows.Soft;
            RenderSettings.sun = sun;

            GameObject fillObject = new GameObject("Directional Light - Cool Fill");
            fillObject.transform.SetParent(parent);
            fillObject.transform.rotation = Quaternion.Euler(35f, 145f, 0f);
            Light fill = fillObject.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = new Color(0.34f, 0.52f, 1f);
            fill.intensity = 0.38f;
            fill.shadows = LightShadows.None;
        }

        private static EnemyController[] BuildEncounters(Transform environmentRoot)
        {
            GameObject root = Child("ENCOUNTERS_PortraitSafeZone", environmentRoot);
            var specs = new[]
            {
                new TargetSpec(2f,  new Vector3(-0.55f, 0f, 8f),  5.8f, 3.0f, false),
                new TargetSpec(5f,  new Vector3(1.15f, 0f, 14f),   5.8f, 3.3f, false),
                new TargetSpec(9f,  new Vector3(0.55f, 3.31f, 21f), 6.0f, 3.1f, false),
                new TargetSpec(13f, new Vector3(-1.25f, 0f, 27f),  5.7f, 3.0f, false),
                new TargetSpec(17f, new Vector3(1.1f, 0f, 32f),    5.7f, 3.2f, true),
                new TargetSpec(20f, new Vector3(-0.45f, 0f, 35f),  5.8f, 2.8f, false),
                new TargetSpec(24f, new Vector3(1.25f, 0f, 40f),   5.8f, 3.1f, false),
                new TargetSpec(28f, new Vector3(-0.65f, 3.31f, 45f), 6.0f, 3.0f, false),
                new TargetSpec(32f, new Vector3(0.9f, 0f, 50f),    5.8f, 2.8f, false),
                new TargetSpec(36f, new Vector3(-1.2f, 0f, 55f),   5.8f, 3.2f, true),
                new TargetSpec(39f, new Vector3(0.55f, 0f, 59f),   5.8f, 2.8f, false),
                new TargetSpec(43f, new Vector3(-1.0f, 0f, 64f),   5.8f, 3.0f, false),
                new TargetSpec(46f, new Vector3(0.6f, 3.31f, 68f), 5.5f, 2.7f, false)
            };

            EnemyController[] targets = new EnemyController[specs.Length];
            for (int i = 0; i < specs.Length; i++)
            {
                targets[i] = BuildTarget(root.transform, specs[i], i + 1);
            }
            return targets;
        }

        private static EnemyController BuildTarget(Transform parent, TargetSpec spec, int index)
        {
            string prefix = spec.Civilian ? "Civilian" : "Enemy";
            GameObject root = new GameObject(prefix + "_" + index.ToString("00") + "_T" + spec.Time);
            root.transform.SetParent(parent);
            root.transform.position = spec.Position;
            EnemyController controller = root.AddComponent<EnemyController>();
            Material bodyMaterial = spec.Civilian ? Materials["Civilian_Cyan"] : Materials["Enemy_Crimson"];
            Material headMaterial = spec.Civilian ? Materials["Civilian_Cyan"] : Materials["EnemyHead_Amber"];

            GameObject body = CreatePrimitive(PrimitiveType.Capsule, "Body Hitbox", new Vector3(0f, 1.35f, 0f), new Vector3(0.48f, 0.58f, 0.34f), bodyMaterial, root.transform);
            body.GetComponent<EnemyHitbox>()?.Configure(controller, HitZone.Body);
            ConfigureHitbox(body, controller, HitZone.Body);
            GameObject head = CreatePrimitive(PrimitiveType.Sphere, "Head Hitbox", new Vector3(0f, 2.18f, 0f), Vector3.one * 0.38f, headMaterial, root.transform);
            ConfigureHitbox(head, controller, HitZone.Head);
            GameObject leftArm = CreatePrimitive(PrimitiveType.Capsule, "Left Arm Hitbox", new Vector3(-0.58f, 1.35f, 0f), new Vector3(0.20f, 0.50f, 0.20f), bodyMaterial, root.transform);
            leftArm.transform.localRotation = Quaternion.Euler(0f, 0f, -12f);
            ConfigureHitbox(leftArm, controller, HitZone.Arm);
            GameObject rightArm = CreatePrimitive(PrimitiveType.Capsule, "Right Arm Hitbox", new Vector3(0.58f, 1.35f, 0f), new Vector3(0.20f, 0.50f, 0.20f), bodyMaterial, root.transform);
            rightArm.transform.localRotation = Quaternion.Euler(0f, 0f, 12f);
            ConfigureHitbox(rightArm, controller, HitZone.Arm);
            GameObject leftLeg = CreatePrimitive(PrimitiveType.Capsule, "Left Leg Hitbox", new Vector3(-0.22f, 0.52f, 0f), new Vector3(0.22f, 0.52f, 0.22f), bodyMaterial, root.transform);
            ConfigureHitbox(leftLeg, controller, HitZone.Body);
            GameObject rightLeg = CreatePrimitive(PrimitiveType.Capsule, "Right Leg Hitbox", new Vector3(0.22f, 0.52f, 0f), new Vector3(0.22f, 0.52f, 0.22f), bodyMaterial, root.transform);
            ConfigureHitbox(rightLeg, controller, HitZone.Body);

            GameObject bar = new GameObject("Threat Timer");
            bar.transform.SetParent(root.transform, false);
            bar.transform.localPosition = new Vector3(0f, 2.78f, 0f);
            CreatePrimitive(PrimitiveType.Cube, "Background", Vector3.zero, new Vector3(1.15f, 0.11f, 0.08f), Materials["Danger_Back"], bar.transform, false);
            GameObject fill = CreatePrimitive(PrimitiveType.Cube, "Fill", new Vector3(0f, 0f, -0.05f), new Vector3(0.9f, 0.07f, 0.09f), Materials["Danger_Fill"], bar.transform, false);
            if (spec.Civilian)
            {
                bar.SetActive(false);
            }

            controller.Configure(spec.Time, spec.Duration, spec.ThreatDelay, 100, spec.Civilian, spec.Civilian ? null : fill.transform);
            return controller;
        }

        private static void ConfigureHitbox(GameObject target, EnemyController owner, HitZone zone)
        {
            EnemyHitbox hitbox = target.AddComponent<EnemyHitbox>();
            hitbox.Configure(owner, zone);
        }

        private static void BuildGameplay(Camera camera, RailCameraController rail, EnemyController[] targets)
        {
            GameObject gameplay = new GameObject("GAMEPLAY_SYSTEMS");
            GameManager manager = gameplay.AddComponent<GameManager>();

            Canvas canvas = BuildCanvas();
            RectTransform safeArea = CreateUIRect("Safe Area", canvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            safeArea.gameObject.AddComponent<SafeAreaController>();

            Color hudColor = new Color(1f, 0.82f, 0.22f);
            Text hp = CreateText("HP", safeArea, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(42f, -62f), new Vector2(330f, 76f), 38, TextAnchor.MiddleLeft, hudColor);
            Text score = CreateText("Score", safeArea, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-42f, -62f), new Vector2(380f, 76f), 38, TextAnchor.MiddleRight, hudColor);
            Text timer = CreateText("Timer", safeArea, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -65f), new Vector2(220f, 72f), 34, TextAnchor.MiddleCenter, new Color(0.82f, 0.93f, 1f));
            Text instruction = CreateText("Instruction", safeArea, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -135f), new Vector2(800f, 60f), 24, TextAnchor.MiddleCenter, new Color(0.75f, 0.90f, 1f));
            instruction.text = "DRAG TO AIM  •  TAP TO FIRE  •  R TO RELOAD";

            Text ammo = CreateText("Ammo", safeArea, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 105f), new Vector2(350f, 90f), 48, TextAnchor.MiddleCenter, hudColor);
            Text reload = CreateText("Reload", safeArea, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 180f), new Vector2(500f, 70f), 34, TextAnchor.MiddleCenter, new Color(1f, 0.42f, 0.15f));
            reload.gameObject.SetActive(false);
            Text feedback = CreateText("Feedback", safeArea, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -430f), new Vector2(850f, 90f), 34, TextAnchor.MiddleCenter, hudColor);
            feedback.gameObject.SetActive(false);

            RectTransform crosshairRect = CreateUIRect("CROSSHAIR_AimArea_10-90_10-85", canvas.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(112f, 112f));
            BuildCrosshair(crosshairRect);
            CrosshairController crosshair = gameplay.AddComponent<CrosshairController>();
            crosshair.Configure(crosshairRect);

            GameObject resultPanel = CreateUIBlock("Mission Result Panel", safeArea, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.04f, 0.06f, 0.10f, 0.88f));
            Text resultText = CreateText("Mission Result", resultPanel.transform, new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.65f), Vector2.zero, Vector2.zero, 56, TextAnchor.MiddleCenter, hudColor);
            resultPanel.SetActive(false);

            GunController gun = gameplay.AddComponent<GunController>();
            gun.Configure(camera, crosshair, ammo, reload);
            manager.Configure(targets, hp, score, timer, feedback, resultPanel, resultText);
        }

        private static Canvas BuildCanvas()
        {
            GameObject canvasObject = new GameObject("HUD_PORTRAIT_1080x1920");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static void BuildCrosshair(RectTransform root)
        {
            Color color = new Color(1f, 0.82f, 0.16f, 0.95f);
            CreateUIBlock("Top", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 35f), new Vector2(7f, 30f), color);
            CreateUIBlock("Bottom", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -35f), new Vector2(7f, 30f), color);
            CreateUIBlock("Left", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-35f, 0f), new Vector2(30f, 7f), color);
            CreateUIBlock("Right", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(35f, 0f), new Vector2(30f, 7f), color);
            CreateUIBlock("Dot", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(10f, 10f), color);
        }

        private static Text CreateText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment, Color color)
        {
            RectTransform rect = CreateUIRect(name, parent, anchorMin, anchorMax, position, size);
            Text text = rect.gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            Outline outline = rect.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.82f);
            outline.effectDistance = new Vector2(2f, -2f);
            return text;
        }

        private static GameObject CreateUIBlock(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size, Color color)
        {
            RectTransform rect = CreateUIRect(name, parent, anchorMin, anchorMax, position, size);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return rect.gameObject;
        }

        private static RectTransform CreateUIRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = (anchorMin + anchorMax) * 0.5f;
            rect.anchoredPosition = position;
            if (anchorMin == anchorMax)
            {
                rect.sizeDelta = size;
            }
            else
            {
                rect.offsetMin = position;
                rect.offsetMax = position;
            }
            return rect;
        }

        private static GameObject CreatePrimitive(PrimitiveType type, string name, Vector3 position, Vector3 scale, Material material, Transform parent, bool keepCollider = true)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            Renderer renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            if (!keepCollider)
            {
                Object.DestroyImmediate(go.GetComponent<Collider>());
            }
            return go;
        }

        private static GameObject Child(string name, Transform parent)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent);
            return child;
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.runInBackground = true;
        }

        private readonly struct TargetSpec
        {
            public readonly float Time;
            public readonly Vector3 Position;
            public readonly float Duration;
            public readonly float ThreatDelay;
            public readonly bool Civilian;

            public TargetSpec(float time, Vector3 position, float duration, float threatDelay, bool civilian)
            {
                Time = time;
                Position = position;
                Duration = duration;
                ThreatDelay = threatDelay;
                Civilian = civilian;
            }
        }
    }
}
