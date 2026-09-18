# Robot Invasion — added gameplay features

Open this project with **Unity 2022.3.62f3**. Start from `Assets/_Scenes/MainMenu.unity`, or open `Level1` / `Level2` to try the gameplay directly. The project uses the Unity Input System and Cinemachine 2.10.7.

On a fresh clone, run `git lfs pull` before opening Unity so the audio, models, and textures are present.

## Controls

| Action | Keyboard / mouse | Gamepad |
| --- | --- | --- |
| Move | WASD | Left stick |
| Look | Mouse | Right stick |
| Jump / double jump | Space | South button (A / Cross) |
| Sprint | Hold Left Shift | Hold left stick click |
| Dash | E | East button (B / Circle) |
| Pause / settings | Escape | — |

## Features added beyond the tutorial

1. **Directional dash.** A short burst in the movement direction, or the model's facing direction when standing still. It works on the ground and in the air, uses the existing `Assets/Audio/SFX/Player/Teleport.wav` sound, and needs no new animation. Defaults are 25 units/second for 0.2 seconds, with one second between starts. Direction stays fixed during a dash; gravity, collisions, and damage still apply. Sprint does not multiply dash speed. Walls and jump-pad bounces cancel the dash, and respawning clears movement and cooldown state.
2. **Sprint.** Holding sprint multiplies walking speed by exactly **1.75**. The player prefabs walk at 10 units/second and sprint at 17.5. Releasing the button restores walking speed. Diagonal input is capped so it cannot exceed those speeds.
3. **Timed pickup combos (additional programming feature).** Collect score pickups within four seconds of each other to build a multiplier from x1 up to x5. Each pickup refreshes the countdown. The game tracks the chain, awards the multiplied score, and shows the multiplier and time remaining in a HUD. Taking damage, dying, or starting another scene breaks the chain. Pause freezes its timer. Health, lives, keys, and goals keep their original behavior. This adds a separate gameplay state model, scoring integration, reset rules, and live UI beyond the movement changes.

**Camera settings fix:** the existing Invert X / Invert Y toggles now independently reverse the Cinemachine FreeLook camera. Mouse sensitivity sliders also affect Cinemachine. Settings apply immediately and are restored when entering a level, without needing to reopen the settings panel. The original camera defaults and legacy camera support are retained.

## Where to tune the features

- On the `Player` child in either player prefab: sprint multiplier, dash speed, duration, cooldown, clip, volume, and input bindings.
- On `GameManager`: the pickup combo window and maximum multiplier.
- On the Cinemachine FreeLook object: `CinemachineCameraSettings` applies preferences relative to the camera's configured default axis directions and speeds.

Both gameplay scenes inherit the new abilities from `Assets/Prefabs/Player/3rdPersonPlayer.prefab`.

## Manual play checks

- Hold sprint while moving, release it, and try diagonal movement and a gamepad.
- Dash while moving and standing still, then try it in the air and against a wall. Repeated presses during cooldown should not start another dash or sound.
- Pause during a dash or combo and resume. Movement and timers should remain frozen while paused.
- Die during a dash and respawn at a checkpoint. The player should not retain dash or falling momentum.
- Toggle each camera axis separately in settings. Close and reopen settings, then enter another level and verify the choices remain applied.
- Collect score pickups in quick succession, let the combo expire, and take damage during a chain. Check the awarded score, multiplier cap, and HUD countdown.

## Automated checks

`Assets/Editor/ProjectFeatureValidation.cs` provides a batch-mode check of the real player prefabs, scene inheritance, settings UI callbacks, and runtime behavior with simulated keyboard/gamepad input. It also checks combo scoring and the runtime HUD, and restores saved player preferences afterward.

Run from PowerShell in the project directory while this worktree is closed in Unity:

```powershell
& 'C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Unity.exe' `
  -batchmode -nographics -projectPath "$PWD" `
  -executeMethod ProjectFeatureValidation.Run `
  -logFile 'Logs/feature-validation.log'
```

Do not add `-quit`: the validator enters Play Mode and exits when its checks finish. It writes the results to `Logs/feature-validation-results.txt` and exits with code 0 on success.
