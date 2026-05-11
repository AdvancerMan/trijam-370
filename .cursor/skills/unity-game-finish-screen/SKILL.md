---
name: unity-game-finish-screen
description: >-
  Builds Unity finish/game-over screens with shared gameplay background, session
  statistics (TMP), semi-transparent stat panels, and a button that loads the
  main or next level scene via SceneManager. Use when adding or changing level
  complete, game over, results, summary, restart, next level UI, or finish
  scenes in Unity (UGUI, TextMeshPro, scene flow).
disable-model-invocation: true
---

# Unity game finish screen

## Required behavior (verbatim)

* it should be opened when player failed or finished the level
* main background for the finish scene should be the same as for the main scene
* finish screen should show some statistics in text on how the game went (if it is unclear from the user prompt - ask), e.g. time spent / actions done
* statistics text should have slightly gray-faded background - it can be accomplished using 0.5 alpha colored gray square sprite
* finish screen should have a button that returns player to the game (either "next level" button or "restart the game" button) - this button should lead to the main scene (or next level scene)

## When to apply

Use this skill whenever implementing or adjusting a **finish / game over / level complete** flow in Unity: new scene, UI layout, wiring, or code that passes stats into that scene.

## Workflow checklist

- [ ] **Trigger**: From gameplay (win or lose), call a single entry point (e.g. manager `TriggerGameOver` / `TriggerLevelComplete`) that sets session data then `SceneManager.LoadScene(finishSceneName)`.
- [ ] **Session data**: Before loading the finish scene, write all stats the UI will show into a **static** or **ScriptableObject** carrier the finish screen reads in `Start` / `OnEnable`. Avoid relying on destroyed scene objects.
- [ ] **Statistics**: If the user or design doc does not specify which metrics to show (time, score, actions, deaths, etc.), **ask** which fields to display and where they are produced in gameplay.
- [ ] **Background**: Reuse the same world sprites, materials, camera clear color, and/or skybox as the main scene so the finish scene visually matches. In this repo, see `Assets/Scenes/GameOver.unity`: world `SpriteRenderer` layers behind UI (`DirtVisual`, `Background`) plus `Main Camera` aligned with gameplay.
- [ ] **Stat panel**: Add one grouped panel behind stat lines as a **gray square** with **alpha ~0.5** — either a UI `Image` (Unity default white sprite, color RGB gray, A 0.5) stretched behind `TextMeshProUGUI`, or a world-space `SpriteRenderer` square with the same color/alpha.
- [ ] **Canvas**: Use **Canvas** + **CanvasScaler** + **GraphicRaycaster**; assign the same **Render Camera** as gameplay if using World Space canvas. Include **EventSystem** (and Input System UI module if the project uses the new Input System).
- [ ] **Primary action button**: One button whose onClick loads **either** the stored gameplay scene name (restart / retry) **or** the next level scene name (progression). Expose a `[SerializeField]` fallback scene name if the stored name is empty.
- [ ] **Build Settings**: Add the finish scene and all target scenes to **File > Build Settings** so `LoadScene` resolves by name.

## Code pattern (reference)

Reference implementation:

- `Assets/Scripts/GameSessionData.cs` — static holder for values set at run end.
- `Assets/Scripts/WorldManager.cs` — main game logic, `TriggerGameOver()` fills session data then loads `"GameOver"`.
- `Assets/Scripts/GameOverScreen.cs` — `Start` assigns `TMP_Text` from session data; public method loads `GameplaySceneName` or fallback.

Match project rules: `[SerializeField]` for inspector wiring, null checks on optional `TMP_Text`, no hardcoded scene names except as fallback default.

## Scene hierarchy (typical)

1. **Camera** — same orthographic/size or perspective as gameplay if the background is world-aligned.
2. **Background roots** — duplicated or shared art from the main scene (sorting order behind UI).
3. **Canvas (World Space)** — child order: optional full-screen dimmer → **gray alpha panels** → title → stat texts → **Button** (with child TMP label).
4. **EventSystem** — required for buttons.
5. **Controller GameObject** — `MonoBehaviour` with serialized `TMP_Text` references and `LoadScene` method for the button.

## Button wiring

In the Inspector: **Button > On Click ()** → target the finish-screen component → method that calls `SceneManager.LoadScene(...)`. Keep scene name resolution in code (session data + fallback), not duplicated in multiple buttons.

## Next level vs restart

- **Restart / same level**: Store current gameplay scene name in session data before loading finish (as in `GameSessionData.GameplaySceneName`).
- **Next level**: Store the **next** scene name when the player wins, or compute from a level index list; the same button handler can branch on win vs lose if both use one finish scene.

## Additional resources

- Concrete scene object layout and components: `Assets/Scenes/GameOver.unity`
- Screen logic: `Assets/Scripts/GameOverScreen.cs`
