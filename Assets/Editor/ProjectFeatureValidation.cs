using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class ProjectFeatureValidation
{
    private const string StateKey = "ProjectFeatureValidation.";
    private const string PlayerPath = "Assets/Prefabs/Player/3rdPersonPlayer.prefab";
    private static readonly string[] IntPrefs = { "InvertX", "InvertY", "score", "highscore", "health", "lives" };
    private static readonly string[] FloatPrefs = { "HorizontalMouseSensitivity", "VerticalMouseSensitivity" };
    private static readonly Stack<IEnumerator> Steps = new Stack<IEnumerator>();
    private static Keyboard keyboard;
    private static Gamepad gamepad;
    private static int lastFrame = -1;
    private static string runtimeError;

    static ProjectFeatureValidation()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        EditorApplication.update += Tick;
    }

    // Run with -batchmode -nographics -executeMethod ProjectFeatureValidation.Run, without -quit.
    public static void Run()
    {
        if (!Application.isBatchMode)
            throw new InvalidOperationException("Run this validation in a separate batch-mode Unity process.");

        SessionState.SetBool(StateKey + "active", true);
        SessionState.SetBool(StateKey + "complete", false);
        SessionState.SetInt(StateKey + "count", 0);
        SessionState.SetString(StateKey + "results", "");
        SessionState.SetFloat(StateKey + "deadline", (float)EditorApplication.timeSinceStartup + 120f);
        foreach (string key in IntPrefs) SavePreference(key, false);
        foreach (string key in FloatPrefs) SavePreference(key, true);

        try
        {
            ValidateAssets();
            ValidateComboModel();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorApplication.isPlaying = true;
        }
        catch (Exception exception)
        {
            Finish(exception);
        }
    }

    private static void ValidateAssets()
    {
        foreach (string path in new[] { PlayerPath, "Assets/Prefabs/Player/3rdPersonPlayerOriginal.prefab" })
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Check(prefab != null, path + " loads");
            ThirdPersonCharacterController controller = prefab.GetComponentInChildren<ThirdPersonCharacterController>(true);
            Check(controller != null && controller.GetComponent<CharacterController>() != null, path + " has a working controller reference");
            Near(controller.sprintMultiplier, 1.75f, 0.001f, path + " sprint multiplier");
            Check(controller.dashSound != null, path + " dash sound resolves to an audio asset");
            Check(controller.dashSpeed > controller.moveSpeed * controller.sprintMultiplier && controller.dashDuration > 0f && controller.dashCooldown > controller.dashDuration,
                path + " dash tuning exceeds sprint speed and includes cooldown");
        }

        GameObject currentPlayer = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPath);
        CinemachineFreeLook camera = currentPlayer.GetComponentInChildren<CinemachineFreeLook>(true);
        Check(camera != null && camera.GetComponent<CinemachineCameraSettings>() != null, "Gameplay FreeLook has the settings adapter");

        foreach (string path in new[] { "Assets/Prefabs/UI/UIManagerMainMenu.prefab", "Assets/Prefabs/UI/UIManagerInGame.prefab" })
        {
            Settings settings = AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponentInChildren<Settings>(true);
            Check(settings != null && settings.invertX != null && settings.invertY != null, path + " inversion controls resolve");
            Check(HasListener(settings.invertX.onValueChanged, settings, "ChangeInvertX") && HasListener(settings.invertY.onValueChanged, settings, "ChangeInvertY"),
                path + " inversion events call the correct settings methods");
        }

        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) continue;
            var loaded = EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);
            int playerCount = 0;
            foreach (GameObject root in loaded.GetRootGameObjects())
            {
                foreach (ThirdPersonCharacterController controller in root.GetComponentsInChildren<ThirdPersonCharacterController>(true))
                {
                    playerCount++;
                    Check(controller.dashSound != null && Mathf.Approximately(controller.sprintMultiplier, 1.75f), scene.path + " player inherits sound and sprint settings");
                    Check(controller.transform.root.GetComponentInChildren<CinemachineCameraSettings>(true) != null,
                        scene.path + " contains the Cinemachine settings adapter");
                }
            }
            if (!scene.path.Contains("MainMenu")) Check(playerCount > 0, scene.path + " contains a player");
        }
    }

    private static bool HasListener(UnityEngine.Events.UnityEventBase evt, Object target, string method)
    {
        for (int i = 0; i < evt.GetPersistentEventCount(); i++)
            if (evt.GetPersistentTarget(i) == target && evt.GetPersistentMethodName(i) == method) return true;
        return false;
    }

    private static void ValidateComboModel()
    {
        var combo = new PickupCombo(4f, 5);
        int[] awards = { 10, 20, 30, 40, 50, 50 };
        for (int i = 0; i < awards.Length; i++)
            Check(combo.Collect(10, i * 0.1f) == awards[i], "Pickup " + (i + 1) + " awards " + awards[i]);
        combo.Tick(0.5f);
        Check(combo.Multiplier == 5 && combo.ChainCount == 6, "Combo caps multiplier while tracking the chain");
        combo.Tick(4.5f);
        Check(combo.ChainCount == 0 && combo.Collect(10, 4.5f) == 10, "Combo expires at its deadline and restarts at x1");
        combo.Reset();
        Check(combo.Collect(0, 6f) == 0 && combo.ChainCount == 0, "Zero-value pickup cannot begin a combo");
        Check(combo.Collect(10, 7f) == 10 && combo.Collect(10, 10.9f) == 20, "Collecting before expiry extends the chain");
        combo.Tick(14.8f);
        Check(combo.ChainCount == 2, "Extended combo remains available inside its new window");
        combo.Reset();
        Check(combo.ChainCount == 0, "Explicit combo reset clears the chain");
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (!SessionState.GetBool(StateKey + "active", false)) return;
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            Time.captureDeltaTime = 1f / 60f;
            Time.timeScale = 1f;
            runtimeError = null;
            Application.logMessageReceived += OnLog;
            InputSystem.onAfterUpdate += RuntimeTick;
            Steps.Clear();
            Steps.Push(ValidateRuntime());
            lastFrame = -1;
        }
        else if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(StateKey + "complete", false))
        {
            Exit();
        }
    }

    private static void Tick()
    {
        if (!SessionState.GetBool(StateKey + "active", false) || SessionState.GetBool(StateKey + "complete", false)) return;
        if (EditorApplication.timeSinceStartup > SessionState.GetFloat(StateKey + "deadline", 0f))
        {
            Finish(new TimeoutException("Feature validation exceeded 120 seconds."));
        }
    }

    private static void RuntimeTick()
    {
        if (InputState.currentUpdateType != InputUpdateType.Dynamic || SessionState.GetBool(StateKey + "complete", false)) return;
        if (!EditorApplication.isPlaying || Steps.Count == 0 || Time.frameCount == lastFrame) return;
        lastFrame = Time.frameCount;
        try
        {
            if (runtimeError != null) throw new InvalidOperationException(runtimeError);
            while (Steps.Count > 0)
            {
                IEnumerator current = Steps.Peek();
                if (!current.MoveNext()) { Steps.Pop(); continue; }
                if (current.Current is IEnumerator nested) { Steps.Push(nested); continue; }
                return;
            }
            Finish(null);
        }
        catch (Exception exception)
        {
            Finish(exception);
        }
    }

    private static IEnumerator ValidateRuntime()
    {
        InputSystem.settings = Object.Instantiate(InputSystem.settings);
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
        Application.runInBackground = true;
        keyboard = InputSystem.AddDevice<Keyboard>();
        gamepad = InputSystem.AddDevice<Gamepad>();
        var ground = new GameObject("Validation ground", typeof(BoxCollider));
        ground.transform.position = new Vector3(0f, -0.5f, 0f);
        ground.transform.localScale = new Vector3(200f, 1f, 200f);
        var player = new GameObject("Validation player");
        player.SetActive(false);
        var capsule = player.AddComponent<CharacterController>();
        capsule.height = 2f;
        capsule.center = Vector3.up;
        var health = player.AddComponent<Health>();
        health.defaultHealth = health.maximumHealth = health.currentHealth = 5;
        health.currentLives = 0;
        var controller = player.AddComponent<ThirdPersonCharacterController>();
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPath).GetComponentInChildren<ThirdPersonCharacterController>(true);
        controller.moveInput = source.moveInput.Clone();
        controller.jumpInput = source.jumpInput.Clone();
        controller.sprintInput = source.sprintInput.Clone();
        controller.dashInput = source.dashInput.Clone();
        controller.moveSpeed = source.moveSpeed;
        controller.sprintMultiplier = source.sprintMultiplier;
        controller.dashSpeed = source.dashSpeed;
        controller.dashDuration = source.dashDuration;
        controller.dashCooldown = source.dashCooldown;
        controller.followers = new List<FollowLikeChild>();
        player.SetActive(true);
        yield return Frames(10);

        Keys(Key.W);
        yield return Frames(3);
        Check(controller.moveInput.ReadValue<Vector2>().y > 0f,
            "Headless movement input: key=" + keyboard.wKey.ReadValue() + ", keyboardEnabled=" + keyboard.enabled +
            ", actionEnabled=" + controller.moveInput.enabled + ", controls=" + controller.moveInput.controls.Count +
            ", playerEnabled=" + controller.isActiveAndEnabled + ", position=" + player.transform.position);
        Near(HorizontalSpeed(capsule), controller.moveSpeed, 0.03f, "W moves at walking speed");
        Keys(Key.W, Key.LeftShift);
        yield return Frames(3);
        Near(HorizontalSpeed(capsule), controller.moveSpeed * 1.75f, 0.03f, "Shift sprint is exactly 1.75 times walking speed");
        Keys(Key.W, Key.D, Key.LeftShift);
        yield return Frames(3);
        Near(HorizontalSpeed(capsule), controller.moveSpeed * 1.75f, 0.03f, "Diagonal sprint is normalized");
        Keys(Key.W);
        yield return Frames(3);
        Near(HorizontalSpeed(capsule), controller.moveSpeed, 0.03f, "Releasing sprint returns to walking speed");
        Keys();
        InputState.Change(gamepad, new GamepadState { leftStick = Vector2.up }.WithButton(GamepadButton.LeftStick));
        yield return Frames(3);
        Near(HorizontalSpeed(capsule), controller.moveSpeed * 1.75f, 0.03f, "Gamepad stick press sprints");
        InputState.Change(gamepad, new GamepadState());
        yield return Frames(3);

        controller.MoveToPosition(new Vector3(0f, 5f, 0f));
        Keys(Key.W, Key.LeftShift);
        yield return Frames(3);
        Near(HorizontalSpeed(capsule), controller.moveSpeed * 1.75f, 0.03f, "Sprint works while airborne");
        Keys();
        yield return Frames(2);
        controller.MoveToPosition(Vector3.zero);
        yield return Frames(3);
        Vector3 dashStart = player.transform.position;
        Keys(Key.E);
        yield return Frames(2);
        Check(controller.IsDashing, "E starts a dash");
        Near(HorizontalSpeed(capsule), controller.dashSpeed, 0.03f, "Dash moves at configured speed");
        Vector3 pausedPosition = player.transform.position;
        float pausedCooldown = controller.DashCooldownRemaining;
        Time.timeScale = 0f;
        yield return Frames(6);
        Near(Vector3.Distance(player.transform.position, pausedPosition), 0f, 0.001f, "Pause freezes dash movement");
        Near(controller.DashCooldownRemaining, pausedCooldown, 0.001f, "Pause freezes dash cooldown");
        Time.timeScale = 1f;
        player.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        yield return Frames(20);
        Check(!controller.IsDashing, "Dash ends after its duration");
        Near(player.transform.position.z - dashStart.z, controller.dashSpeed * controller.dashDuration, 0.04f, "Dash travels its full configured distance");
        Near(player.transform.position.x - dashStart.x, 0f, 0.01f, "Dash direction stays fixed when facing changes");
        Keys();
        yield return Frames(2);
        Keys(Key.E);
        yield return Frames(2);
        Check(!controller.IsDashing, "Cooldown rejects another dash");
        yield return Frames(70);
        Near(player.transform.position.z - dashStart.z, controller.dashSpeed * controller.dashDuration, 0.04f, "Holding dash does not repeat automatically");

        Keys();
        yield return Frames(2);
        controller.MoveToPosition(Vector3.zero);
        player.transform.rotation = Quaternion.identity;
        var wall = new GameObject("Validation wall", typeof(BoxCollider));
        wall.transform.position = new Vector3(0f, 1.5f, 2f);
        wall.transform.localScale = new Vector3(8f, 3f, 0.2f);
        Physics.SyncTransforms();
        InputState.Change(gamepad, new GamepadState().WithButton(GamepadButton.East));
        yield return Frames(2);
        Check(controller.IsDashing, "Gamepad east button starts a dash");
        yield return Frames(15);
        Check(player.transform.position.z < 1.6f && !controller.IsDashing, "Wall blocks and cancels the dash");
        Object.Destroy(wall);
        InputState.Change(gamepad, new GamepadState());
        yield return Frames(2);
        controller.MoveToPosition(Vector3.zero);
        Keys(Key.E);
        yield return Frames(2);
        health.currentHealth = 0;
        yield return Frames(2);
        Check(!controller.IsDashing && controller.playerState == ThirdPersonCharacterController.PlayerState.Dead, "Death cancels an active dash");
        Near(HorizontalSpeed(capsule), 0f, 0.01f, "Dead player has no horizontal movement");
        health.currentHealth = 5;
        Keys();
        yield return Frames(2);
        controller.MoveToPosition(Vector3.zero);
        Near(controller.DashCooldownRemaining, 0f, 0.001f, "Teleport or respawn clears dash cooldown");
        Keys(Key.E);
        yield return Frames(2);
        controller.enabled = false;
        Check(!controller.IsDashing && !controller.sprintInput.enabled && !controller.dashInput.enabled, "Disabling player clears dash and disables new inputs");
        Keys();

        ValidateCameraSettings();
        yield return ValidateComboIntegration(player, health);
    }

    private static void ValidateCameraSettings()
    {
        PlayerPrefs.SetInt("InvertX", 0);
        PlayerPrefs.SetInt("InvertY", 0);
        PlayerPrefs.SetFloat("HorizontalMouseSensitivity", 1f);
        PlayerPrefs.SetFloat("VerticalMouseSensitivity", 1f);
        var settingsObject = new GameObject("Validation settings");
        settingsObject.SetActive(false);
        var settings = settingsObject.AddComponent<Settings>();
        settings.invertX = Child<Toggle>(settingsObject, "Invert X");
        settings.invertY = Child<Toggle>(settingsObject, "Invert Y");
        settings.horizontalMouseSensitivitySlider = Child<Slider>(settingsObject, "Horizontal sensitivity");
        settings.verticalMouseSensitivitySlider = Child<Slider>(settingsObject, "Vertical sensitivity");
        settings.horizontalMouseSensitivitySlider.maxValue = settings.verticalMouseSensitivitySlider.maxValue = 3f;
        settingsObject.SetActive(true);
        settings.invertX.SetIsOnWithoutNotify(true);
        settings.ChangeInvertX();
        Check(PlayerPrefs.GetInt("InvertX") == 1, "Main-menu settings save inversion without a gameplay camera");

        var cameraObject = new GameObject("Validation FreeLook");
        cameraObject.SetActive(false);
        var camera = cameraObject.AddComponent<CinemachineFreeLook>();
        camera.m_XAxis.m_InvertInput = false;
        camera.m_YAxis.m_InvertInput = true;
        camera.m_XAxis.m_MaxSpeed = 150f;
        camera.m_YAxis.m_MaxSpeed = 1f;
        var adapter = cameraObject.AddComponent<CinemachineCameraSettings>();
        cameraObject.SetActive(true);
        Check(camera.m_XAxis.m_InvertInput && camera.m_YAxis.m_InvertInput, "Camera loads saved X inversion and retains default Y direction");
        settings.invertY.SetIsOnWithoutNotify(true);
        settings.ChangeInvertY();
        Check(camera.m_XAxis.m_InvertInput && !camera.m_YAxis.m_InvertInput, "Y inversion toggles independently at runtime");
        settings.invertX.SetIsOnWithoutNotify(false);
        settings.ChangeInvertX();
        Check(!camera.m_XAxis.m_InvertInput && !camera.m_YAxis.m_InvertInput, "X can return to default while Y stays inverted");
        settings.horizontalMouseSensitivitySlider.SetValueWithoutNotify(2f);
        settings.ChangeHorizontalMouseSensitivity();
        settings.verticalMouseSensitivitySlider.SetValueWithoutNotify(0.5f);
        settings.ChangeVerticalMouseSensitivity();
        adapter.ApplySettings();
        adapter.ApplySettings();
        Near(camera.m_XAxis.m_MaxSpeed, 300f, 0.001f, "Horizontal sensitivity applies without compounding");
        Near(camera.m_YAxis.m_MaxSpeed, 0.5f, 0.001f, "Vertical sensitivity applies independently");
        settingsObject.SetActive(false);
        settingsObject.SetActive(true);
        Check(!settings.invertX.isOn && settings.invertY.isOn, "Reopened settings panel reflects persisted inversion");
        Object.Destroy(settingsObject);
        Object.Destroy(cameraObject);
    }

    private static IEnumerator ValidateComboIntegration(GameObject player, Health health)
    {
        PlayerPrefs.SetInt("score", 0);
        PlayerPrefs.SetInt("health", 5);
        PlayerPrefs.SetInt("lives", 3);
        var managerObject = new GameObject("Validation game manager");
        var manager = managerObject.AddComponent<GameManager>();
        manager.player = player;
        yield return Frames(2);
        GameManager.score = 0;
        manager.ResetPickupCombo();
        GameManager.AddPickupScore(10);
        GameManager.AddPickupScore(10);
        Check(GameManager.score == 30 && manager.Combo.ChainCount == 2, "Pickup scoring integrates with the game score");
        health.isInvincible = false;
        health.TakeDamage(1);
        Check(manager.Combo.ChainCount == 0, "Player damage resets the combo");
        GameManager.AddPickupScore(10);
        Check(GameManager.score == 40, "Pickup after damage restarts at base score");
        manager.enabled = false;
        var eventSystem = new GameObject("Validation event system", typeof(UnityEngine.EventSystems.EventSystem));
        GameObject uiObject = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/UIManagerInGame.prefab"));
        yield return Frames(2);
        manager.enabled = true;
        ComboDisplay.Create(manager);
        yield return Frames(2);
        ComboDisplay display = uiObject.GetComponentInChildren<ComboDisplay>(true);
        Check(display != null && display.isActiveAndEnabled, "Existing gameplay UI displays the combo HUD");
        Check(!display.GetComponent<CanvasGroup>().blocksRaycasts, "Combo HUD does not block gameplay or menu input");
        Time.timeScale = 0f;
        yield return Frames(2);
        float comboTime = manager.Combo.RemainingTime;
        yield return Frames(6);
        Near(manager.Combo.RemainingTime, comboTime, 0.001f, "Pause freezes the combo countdown");
        Time.timeScale = 1f;
        UIManager.instance.GoToPage(UIManager.instance.pausePageIndex);
        Check(!display.gameObject.activeInHierarchy, "Opening the pause page hides the combo HUD");
        Object.Destroy(uiObject);
        Object.Destroy(eventSystem);
        yield return Frames(2);
        UIManager.instance = null;
        manager.LevelCleared();
        Check(manager.Combo.ChainCount == 0, "Completing a level clears the combo");
        GameManager.AddPickupScore(10);
        manager.GameOver();
        Check(manager.Combo.ChainCount == 0, "Game over clears the combo");
        Object.Destroy(managerObject);
        yield return Frames(2);
        GameManager.instance = null;
    }

    private static T Child<T>(GameObject parent, string name) where T : Component
    {
        var child = new GameObject(name, typeof(RectTransform));
        child.transform.SetParent(parent.transform);
        return child.AddComponent<T>();
    }

    private static IEnumerator Frames(int count)
    {
        for (int i = 0; i < count; i++) yield return null;
    }

    private static void Keys(params Key[] keys) => InputState.Change(keyboard, new KeyboardState(keys));
    private static float HorizontalSpeed(CharacterController controller) => Vector3.ProjectOnPlane(controller.velocity, Vector3.up).magnitude;

    private static void Near(float actual, float expected, float tolerance, string message)
    {
        Check(Mathf.Abs(actual - expected) <= tolerance, message + " (expected " + expected + ", got " + actual + ")");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        SessionState.SetInt(StateKey + "count", SessionState.GetInt(StateKey + "count", 0) + 1);
        SessionState.SetString(StateKey + "results", SessionState.GetString(StateKey + "results", "") + "PASS " + message + "\n");
        Debug.Log("[Feature validation] PASS " + message);
    }

    private static void OnLog(string message, string stackTrace, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert)
            runtimeError = message + "\n" + stackTrace;
    }

    private static void SavePreference(string key, bool isFloat)
    {
        SessionState.SetBool(StateKey + key + ".exists", PlayerPrefs.HasKey(key));
        if (isFloat) SessionState.SetFloat(StateKey + key, PlayerPrefs.GetFloat(key));
        else SessionState.SetInt(StateKey + key, PlayerPrefs.GetInt(key));
    }

    private static void RestorePreference(string key, bool isFloat)
    {
        if (!SessionState.GetBool(StateKey + key + ".exists", false)) PlayerPrefs.DeleteKey(key);
        else if (isFloat) PlayerPrefs.SetFloat(key, SessionState.GetFloat(StateKey + key, 0f));
        else PlayerPrefs.SetInt(key, SessionState.GetInt(StateKey + key, 0));
    }

    private static void Finish(Exception failure)
    {
        Application.logMessageReceived -= OnLog;
        InputSystem.onAfterUpdate -= RuntimeTick;
        SessionState.SetBool(StateKey + "complete", true);
        SessionState.SetInt(StateKey + "exitCode", failure == null ? 0 : 1);
        if (failure != null)
        {
            SessionState.SetString(StateKey + "results", SessionState.GetString(StateKey + "results", "") + "FAIL " + failure + "\n");
            Debug.LogError("[Feature validation] " + failure);
        }
        if (EditorApplication.isPlaying)
        {
            Time.timeScale = 1f;
            Time.captureDeltaTime = 0f;
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            if (gamepad != null) InputSystem.RemoveDevice(gamepad);
            EditorApplication.isPlaying = false;
        }
        else Exit();
    }

    private static void Exit()
    {
        foreach (string key in IntPrefs) RestorePreference(key, false);
        foreach (string key in FloatPrefs) RestorePreference(key, true);
        PlayerPrefs.Save();
        int code = SessionState.GetInt(StateKey + "exitCode", 1);
        string summary = "FEATURE VALIDATION " + (code == 0 ? "PASSED" : "FAILED") + ": " + SessionState.GetInt(StateKey + "count", 0) + " checks";
        Directory.CreateDirectory("Logs");
        File.WriteAllText("Logs/feature-validation-results.txt", summary + "\n" + SessionState.GetString(StateKey + "results", ""));
        Debug.Log(summary);
        SessionState.SetBool(StateKey + "active", false);
        EditorApplication.Exit(code);
    }
}
