# Robot Invasion — added gameplay features

## Controls

MOVE -> WASD or Left Stick  
Look -> Mouse or Right Stick  
Jump / Double Jump -> Space or A on controller  
Sprint -> Hold left Shift or Hold left stick click  
Dash -> E on keyboard or B on controller  
Pause -> Escape or Start/Back  

## Features added beyond the tutorial

1. **Directional dash.** A short burst in the movement direction, or the model's facing direction when standing still. It works on the ground and in the air, uses the existing `Assets/Audio/SFX/Player/Teleport.wav` sound, and needs no new animation. Defaults are 25 units/second for 0.2 seconds, with one second between starts. Direction stays fixed during a dash; gravity, collisions, and damage still apply. Sprint does not multiply dash speed. Walls and jump-pad bounces cancel the dash, and respawning clears movement and cooldown state.
2. **Sprint.** Holding sprint multiplies walking speed by exactly **1.75**. The player prefabs walk at 10 units/second and sprint at 17.5. Releasing the button restores walking speed. Diagonal input is capped so it cannot exceed those speeds.
3. **Timed pickup combos (additional programming feature).** Collect score pickups within four seconds of each other to build a multiplier from x1 up to x5. Each pickup refreshes the countdown. The game tracks the chain, awards the multiplied score, and shows the multiplier and time remaining in a HUD. Taking damage, dying, or starting another scene breaks the chain. Pause freezes its timer. Health, lives, keys, and goals keep their original behavior. This adds a separate gameplay state model, scoring integration, reset rules, and live UI beyond the movement changes.

**Camera settings fix:** the existing Invert X / Invert Y toggles now independently reverse the Cinemachine FreeLook camera. Mouse sensitivity sliders also affect Cinemachine. Settings apply immediately and are restored when entering a level, without needing to reopen the settings panel. The original camera defaults and legacy camera support are retained.
