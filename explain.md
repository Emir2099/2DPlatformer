# Unity 6 Simple 2D Platformer — Full Implementation Guide

This document explains the end-to-end implementation of the 2D Platformer contained in this project. It covers project setup, scene composition, physics and input, animations, and a line-by-line breakdown of each script in `Assets/Scripts`. It also includes practical notes, pitfalls, and troubleshooting tips that are commonly tested in exams. It now includes Tile Palette creation, Cinemachine setup, alternative animation creation via code, and a detailed breakdown of the damage flicker.

The examples and references below are tailored to this repo:
- Scene/UI elements like a win screen (`Flag`) and restart logic (`RestartGame`)
- A player character with movement, coyote-like extra jump, damage/knockback, and health UI
- Moving platforms and simple patrolling enemies
- Coins collectible count


## 1) Project Setup and Core Concepts

### Unity Version
- Project targets Unity 6 (2023+). Scripts here use the built-in (old) Input Manager APIs (`Input.GetAxis`, `Input.GetKeyDown`). This works in Unity 6 if the old input backend is enabled (it is by default in many templates).

### Packages
- No custom packages are required beyond Unity’s defaults for 2D projects. Ensure 2D packages (2D Sprite, 2D Tilemap) are installed if you’re authoring content.

### Layers & Tags
- Player GameObject should be tagged as `Player`.
- Ground Tilemap or platforms should be on a layer included in the Player’s `groundLayer` mask (set in the `Player` Inspector).
- Damaging entities or hazards should be tagged as `Damage` (as used by `Player`’s collision logic).

### Physics 2D Settings
- Confirm the Layer Collision Matrix allows collisions between the Player layer and the Ground/Enemy layers.
- Rigidbody2D on the Player should be Dynamic, with gravity scale > 0 and no “Freeze Position Y” constraint.
- All colliding surfaces need a 2D Collider (e.g., BoxCollider2D, CapsuleCollider2D, TilemapCollider2D).


## 2) Scene Composition Overview

- Player: Sprite, Animator, Rigidbody2D, Collider2D, and the `Player` script.
- Ground: Tilemap with colliders on a layer included by `Player.groundLayer`.
- Moving Platforms: Objects with `MovingPlatform` script and Collider2D, move between two points.
- Enemy: A simple patrol AI moving between `points`.
- Coins: Trigger colliders; increment player’s `coins` when collected.
- Flag (goal): Trigger collider. Shows a Win UI and pauses time when reached.
- Win UI: A Canvas with `winUI` GameObject set inactive by default; activated on win.
- Restart Button: UI button invoking `RestartGame.LoadCurrentScene`.


## 3) Input and Movement Model

- Horizontal movement uses `Input.GetAxis("Horizontal")` and applies directly to `Rigidbody2D.velocity.x`.
- Jump uses `KeyCode.Space` and sets the `Rigidbody2D.velocity.y` to `jumpForce` when grounded (or consumes an extra jump in-air).
- Grounded state is computed in `FixedUpdate()` using `Physics2D.OverlapCircle` at `groundCheck` with `groundCheckRadius`/`groundLayer`.
- Animations are driven by script via `Animator.Play` choosing Idle/Run/Jump/Fall (see also Section 9 for code-based animation alternative).


## 4) Script-by-Script Deep Dive

All scripts are in `Assets/Scripts`.

### 4.1 Player (`Assets/Scripts/Player.cs`)

Responsibilities:
- Reads input and applies horizontal movement and jumping to `Rigidbody2D`.
- Tracks grounded state via overlap circle.
- Supports “double jump” (configurable `extraJumpValues`).
- Health system with hit reaction and a short red flash (coroutine).
- Simple animation state control using `Animator.Play`.
- Health UI fill via a `UnityEngine.UI.Image`.

Important fields:
- `public int coins`, `public int health = 100`, `public float jumpForce = 10f`, `public float moveSpeed = 5f`.
- `public Transform groundCheck`, `public float groundCheckRadius = 0.2f`, `public LayerMask groundLayer`.
- `public Image healthImage` for a filled health bar.
- `public int extraJumpValues = 1` for double jump.

Flow summary:
- Update: read `Horizontal`, set `rb.velocity.x`. On Space: jump if grounded else use remaining `extraJumps` and decrement. Reset `extraJumps` when grounded. Update health UI and drive animations.
- FixedUpdate: compute `isGrounded = Physics2D.OverlapCircle(...)` using `groundCheck` and layer mask.
- Damage on collision with tag `Damage`: reduce health, small vertical knock, start `FlashRed()` coroutine, call `Die()` if health <= 0.

`FlashRed()` coroutine (damage feedback):
```csharp
private IEnumerator FlashRed()
{
    spriteRenderer.color = Color.red;
    yield return new WaitForSeconds(0.1f);
    spriteRenderer.color = Color.white;
}
```

`Die()` reloads the current scene via `SceneManager`.

Inspector setup for Player:
- Assign `groundCheck` (child at feet), `groundLayer`, and `healthImage`.
- Animator needs states named exactly: `Player_Idle`, `Player_Run`, `Player_Jump`, `Player_Fall`.

### 4.2 Moving Platform (`Assets/Scripts/MovingPlatform.cs`)
- Moves between `pointA` and `pointB` using `Vector3.MoveTowards` at `moveSpeed`.
- Parents Player on collision to prevent slipping; unparents on exit.
- Ensure Collider2D on platform; a Kinematic Rigidbody2D is recommended for consistent 2D collisions.

### 4.3 Enemy (`Assets/Scripts/Enemy.cs`)
- Patrols among `points` at `speed`. Flips sprite based on direction.
- Note fix: movement should be `speed * Time.deltaTime`, not `speed = Time.deltaTime`.

### 4.4 Coin (`Assets/Scripts/Coin.cs`)
- Trigger pickup: increments `Player.coins` and destroys itself.

### 4.5 Flag (`Assets/Scripts/Flag.cs`)
- On trigger with Player: `winUI.SetActive(true)` and `Time.timeScale = 0f`.

### 4.6 Restart (`Assets/Scripts/RestartGame.cs`)
- `LoadCurrentScene()` reloads the active scene and resumes time (`Time.timeScale = 1f`). Ideal for a UI Button.


## 5) Animation Setup (Animator-based)

- Create animation clips: `Player_Idle`, `Player_Run`, `Player_Jump`, `Player_Fall` using Window > Animation.
- Animator Controller: states named exactly like above. Use direct `Animator.Play` (as in code) or parameterize with `speed`, `isGrounded`, `verticalSpeed` for smoother transitions.


## 6) UI and Health Bar

- The `healthImage` is an Image with Fill Method (e.g., Horizontal). Script sets `fillAmount = health / 100f`.
- Consider clamping health to [0, 100].


## 7) Tile Palette and Tilemap Setup (step-by-step)

These steps align with the screenshot you shared and explain creating ground tiles the Player can walk on.

1) Create Grid and Tilemap
- GameObject > 2D Object > Tilemap > Rectangular.
- Unity creates a `Grid` parent with a `Tilemap` child. Rename the Tilemap to `Ground`.
- Add components to `Ground`:
  - `Tilemap Renderer`
  - `Tilemap Collider 2D` (adds colliders to painted tiles)
  - Optional: `Composite Collider 2D` + set `Rigidbody2D` (on Tilemap) to `Static` and enable “Used By Composite” on `Tilemap Collider 2D` to merge collider edges cleanly.

2) Create a Tile Palette
- Window > 2D > Tile Palette.
- Create a new palette (name: `Ground`).
- Drag your ground sprites or a spritesheet into the palette to create tile assets.
- With the Brush selected, paint onto the `Ground` Tilemap in the Scene.

3) Layers & Physics
- Put the `Ground` Tilemap on a dedicated layer (e.g., `Ground`).
- In the Player Inspector, set `groundLayer` mask to include this layer.
- Ensure the Layer Collision Matrix allows Player ↔ Ground.

4) Ground Check Placement
- On the Player, create an empty child Transform named `groundCheck` and position it slightly below the feet.
- Set `groundCheckRadius` ~ 0.2–0.3 so the overlap circle touches the tiles reliably.


## 8) Cinemachine Camera Setup

Smooth camera follow is a commonly graded item.

1) Install Cinemachine
- Window > Package Manager > Unity Registry > install Cinemachine.

2) Create a Cinemachine 2D Camera
- GameObject > Cinemachine > 2D Camera (Virtual Camera).
- Select the new `CM vcam`. In the Inspector:
  - Follow: drag the Player into the Follow field.
  - Lens: set Orthographic Size to match your design.
  - Framing/Dead Zone: tweak soft zones for less jitter.
  - Damping: set small values (e.g., 0.3–0.7) for smooth follow.

3) Confiner (Optional but recommended)
- Add `CinemachineConfiner2D` to the `CM vcam`.
- Create a polygon collider that outlines the play area (e.g., on an empty `Bounds` object with `PolygonCollider2D` set to “Is Trigger”).
- Assign the collider shape to the Confiner 2D and set `Confine Mode` to `Confine2D`.


## 9) Alternative: Creating Animations via Code (Sprite Swapping)

If you aren’t using an Animator (for quick prototypes or exam demos), you can animate by swapping sprites each frame:

```csharp
// Attach to the player if not using Animator
public class SimpleSpriteAnimator : MonoBehaviour
{
    public Sprite[] runFrames;
    public float fps = 10f;
    private SpriteRenderer sr;
    private float t;
    private int frame;

    void Awake() => sr = GetComponent<SpriteRenderer>();
    void Update()
    {
        t += Time.deltaTime * fps;
        frame = (int)t % runFrames.Length;
        sr.sprite = runFrames[frame];
    }
}
```

Pros: trivial and explicit. Cons: lacks transitions/blending—Animator is preferred for polish.


## 10) Damage Flicker — Implementation and Variations

Already implemented in `Player.cs` using a coroutine that tints the sprite red for 0.1s, then restores white. To make it more visible or repeated:

```csharp
private IEnumerator FlashRedRepeated(int times, float interval)
{
    for (int i = 0; i < times; i++)
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(interval);
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(interval);
    }
}
```

Alternate techniques for exams:
- Animate material color/emission via an Animation Clip or a DOTween/Tween.
- Swap to a dedicated “hit” material for a brief time.

Inspector requirements:
- Player must have a `SpriteRenderer` (cached in `Start()`).
- Enemies/hazards must be tagged `Damage` to trigger the logic.


## 11) Common Issues and Troubleshooting

Ground detection not working:
- `groundCheck` is null → assign a Transform (child at feet).
- `groundLayer` doesn’t include the ground → set mask appropriately.
- `groundCheckRadius` too small or feet point inside the player’s collider → tweak radius/position.

Jump not working:
- Using wrong property (must set `Rigidbody2D.velocity`, not `linearVelocity`).
- `isGrounded` stays false due to shadowed local variable.
- Rigidbody2D constraints freeze Y or Gravity Scale is 0.

Enemy doesn’t move:
- Fix the assignment typo in `Enemy.Update()` to `speed * Time.deltaTime`.

Player slides off moving platform:
- Parenting in `OnCollisionEnter2D/Exit2D` helps. Ensure proper colliders and non-static Rigidbody2D.

Win UI never appears:
- Ensure Flag’s collider is a Trigger and `winUI` reference is assigned.

Scene doesn’t restart:
- Hook the UI button to `RestartGame.LoadCurrentScene` and ensure the script is on a scene object.


## 12) End-to-End Implementation Checklist (what you do in the editor)

1) World
- Create `Grid` + `Tilemap (Ground)`. Build tiles via Tile Palette.
- Add `Tilemap Collider 2D` (+ `Composite Collider 2D` optional). Place on `Ground` layer.

2) Player
- Add `SpriteRenderer`, `Animator`, `Rigidbody2D (Dynamic)`, and a `Collider2D`.
- Add the `Player` script; assign `groundCheck`, `groundLayer`, and `healthImage`.
- Animator states: `Player_Idle`, `Player_Run`, `Player_Jump`, `Player_Fall`.

3) Enemies/Hazards
- Add `Enemy` with waypoints in `points`; fix Movement line as noted.
- Tag hazards/enemies `Damage`.

4) Moving Platforms
- Add `MovingPlatform` with `pointA` and `pointB`.

5) Coins
- Add `Coin` with Collider2D set to `Is Trigger`.

6) Flag & Win UI
- Add `Flag` with Trigger Collider2D and reference `winUI`. Win UI canvas starts inactive.

7) Cinemachine
- Install; create Virtual Camera; follow Player; tune damping and dead zones; add `Confiner2D` if needed.

8) Restart Button
- Add `RestartGame` script to a GameObject, hook up `LoadCurrentScene()` in UI Button OnClick.

9) Physics & Layers
- Check Layer Collision Matrix; ensure Player ↔ Ground/Enemy. Gravity Scale > 0; no Y freeze.

10) Playtest
- Verify jumps, extra jumps, damage flicker, coin pickup, platforms, enemy patrol, win UI, and restart.


## 13) File Index and Responsibilities

- `Assets/Scripts/Player.cs` — Player control, health, jump, animation, UI.
- `Assets/Scripts/MovingPlatform.cs` — Two-point platform movement and parenting.
- `Assets/Scripts/Enemy.cs` — Waypoint patrol and sprite flipping.
- `Assets/Scripts/Coin.cs` — Collectible trigger; increments coins.
- `Assets/Scripts/Flag.cs` — Goal trigger; shows Win UI and pauses.
- `Assets/Scripts/RestartGame.cs` — Scene reload helper (UI button hook).


## 14) Acceptance Criteria (for exam answers)

- Player can move left/right, jump, and perform configured extra jumps.
- Ground detection accurately toggles `isGrounded`.
- Enemy patrols between waypoints and faces movement direction.
- Coins increment player’s coin count when collected.
- Reaching the flag shows Win UI and pauses the game.
- Restart button reloads the scene and resumes time.
- No runtime exceptions when all public fields are set.
