# SkyDash

A 2D endless runner for Android. You fly a ship upward through space, dodge
asteroids, collect stars, and fight off an alien that drops missiles. The
longer you survive, the faster the screen scrolls.

## Requirements

- Unity **6000.4.6f1** (Unity 6). Older editors will not open the project.
- Android Build Support with the SDK, NDK and OpenJDK modules.

## Opening the project

1. Add the folder in Unity Hub and open it with Unity 6000.4.6f1.
2. Open `Assets/Scenes/Menu.unity` and press Play.

The first import takes a while because the texture and audio assets are
recompressed.

## Scenes

Build order matters, because the menu buttons load scenes by index.

| Index | Scene | What it is |
|-------|-------|------------|
| 0 | `Menu.unity` | Title screen with Play and Settings |
| 1 | `SpaceDash.unity` | The game itself |
| 2 | `Settings.unity` | Touch sensitivity slider |

Scene names used from code live in `Assets/Scripts/UI/SceneNames.cs`.

## How the game is put together

Everything that has to travel with the player is parented under the
`GameManager` object, which `CameraMovement` pushes upward. That is why the
spawners, the border and the camera all keep their positions relative to the
player without any extra code.

- `Assets/Scripts/Core` - startup settings and the border cleanup that
  destroys anything falling out of the play area.
- `Assets/Scripts/Spawners` - `FallingItemSpawner` is the shared base for the
  star, heart, shield, magnet and ammo spawners.
- `Assets/Scripts/Obstacles` - `DamagingHazard` is the shared base for
  asteroids and enemy missiles.
- `Assets/Scripts/PowerUps` - the power-ups announce their own duration, and
  the UI bars listen for that, so the two can never disagree.
- `Assets/Scripts/Background` - `ScrollingBackground` recycles the planet
  strips so the sky loops forever.

Difficulty comes from one formula in `ScoreManager`: score is the distance
travelled, a level is one fixed stretch of distance, and each level multiplies
the base scroll speed.

## Checking the scripts compile

`check_scripts.sh` builds the C# against the Unity 6 assemblies without
opening the editor:

```sh
sh check_scripts.sh        # errors only
sh check_scripts.sh -v     # include warnings
```

It compiles the runtime scripts and the Editor folder separately, so
editor-only API used by mistake in gameplay code is caught.

## Android build settings

The values are stored in `ProjectSettings`, and the menu item
**SkyDash > Apply Android Build Settings** puts them back if they are ever
lost:

- IL2CPP, targeting ARMv7 and ARM64. Google Play requires a 64-bit build.
- Portrait only. The UI is authored for a 1080x1920 canvas.
- Bundle id `com.thescalingstudio.skydash`, minimum SDK 25.

Before publishing you still need to set a keystore in
**Player Settings > Publishing Settings**, and raise the bundle version code
for each upload.

## Third party assets

`Assets/ParticlePack` and `Assets/2D Space Kit` are asset store packs. The
game uses five files between them, so most of that content is unreferenced.
Unreferenced assets are not included in the build, so they cost repository
size and import time but not download size.
