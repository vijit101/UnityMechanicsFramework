====================================================
SETUP INSTRUCTIONS — Modular Interaction System
====================================================

PREREQUISITES:
- Unity 2021.3 LTS or later (2022.3 LTS recommended)
- TextMeshPro package (usually included by default in Unity)

====================================================
STEP 1: Create the Interactable Layer
====================================================

1. Go to Edit > Project Settings > Tags and Layers
2. In the "Layers" section, find an empty User Layer slot (e.g. Layer 6)
3. Name it "Interactable"
4. Remember this layer — every interactable object must be on it

====================================================
STEP 2: Set Up the Player
====================================================

1. Create an empty GameObject: GameObject > Create Empty
2. Name it "Player"
3. Add components:
   - SpriteRenderer (assign any sprite — square works fine)
   - Rigidbody2D (set Gravity Scale to 0 for top-down, or leave for platformer)
   - BoxCollider2D
   - InteractionController_UMFOSS (from Runtime/Interaction/)

4. Configure InteractionController_UMFOSS in Inspector:
   - Detection Mode: OverlapCircle
   - Interaction Radius: 2.5
   - Interactable Layer: select "Interactable"
   - Selection Mode: Nearest
   - Require Hold: false (we will test instant interactions first)
   - Interact Key: E

5. (Optional) Add a simple movement script so you can walk around:
   Create a script called DemoPlayerMovement.cs:

   using UnityEngine;
   public class DemoPlayerMovement : MonoBehaviour
   {
       [SerializeField] private float speed = 5f;
       private Rigidbody2D rb;
       private void Awake() { rb = GetComponent<Rigidbody2D>(); }
       private void FixedUpdate()
       {
           float h = Input.GetAxisRaw("Horizontal");
           float v = Input.GetAxisRaw("Vertical");
           rb.velocity = new Vector2(h, v).normalized * speed;
       }
   }

====================================================
STEP 3: Set Up the Interaction Prompt UI
====================================================

1. Create a Canvas: GameObject > UI > Canvas
2. Set Canvas Scaler to "Scale With Screen Size", 1920x1080

3. Create the prompt panel:
   - Right-click Canvas > UI > Panel
   - Name it "PromptPanel"
   - Anchor to bottom-center
   - Set size to ~300x80
   - Set Image alpha to ~200 for slight transparency

4. Inside PromptPanel, create:
   - Right-click PromptPanel > UI > Text - TextMeshPro
   - Name it "PromptLabel"
   - Set text to "Press E to interact"
   - Center align, white color, font size 24

5. Inside PromptPanel, create a progress bar:
   - Right-click PromptPanel > UI > Slider
   - Name it "HoldProgressBar"
   - Remove the Handle Slide Area child
   - Set Min: 0, Max: 1, Value: 0
   - Set Interactable: false (we control it via script)

6. Add InteractionPrompt_UMFOSS component to PromptPanel:
   - Prompt Panel: drag PromptPanel itself
   - Prompt Label: drag PromptLabel
   - Hold Progress Bar: drag HoldProgressBar

====================================================
STEP 4: Create Interactable Objects
====================================================

OBJECT 1 — DOOR (instant, single use)

1. Create a square sprite GameObject, name it "Door"
2. Set layer to "Interactable"
3. Add BoxCollider2D
4. Add DemoInteractableDoor component
5. Set Sprite Renderer reference (drag the SpriteRenderer)
6. Position at (3, 0, 0)

OBJECT 2 — NPC (instant, repeatable)

1. Create a sprite GameObject, name it "NPC"
2. Set layer to "Interactable"
3. Add BoxCollider2D
4. Add DemoInteractableNPC component
5. (Optional) Create a child TextMeshPro - Text for name tag
   - Set the Name Tag Object field to this child
6. Position at (-3, 0, 0)

OBJECT 3 — ITEM PICKUP (instant, single use)

1. Create a small sprite GameObject, name it "HealthPotion"
2. Set layer to "Interactable"
3. Add BoxCollider2D or CircleCollider2D
4. Add DemoInteractablePickup component
5. Set Item Name to "Health Potion"
6. Position at (0, 3, 0)

OBJECT 4 — GENERATOR (hold to interact)

1. Create a sprite GameObject, name it "Generator"
2. Set layer to "Interactable"
3. Add BoxCollider2D
4. Add DemoInteractableGenerator component
5. Set Sprite Renderer reference
6. Position at (0, -3, 0)
7. IMPORTANT: On the Player's InteractionController, set:
   - Require Hold: true
   - Hold Duration: 2.0
   NOTE: For a demo with both instant and hold objects, you can
   either use two players or test them separately by toggling
   Require Hold on the controller.

====================================================
STEP 5: Add Demo UI Display
====================================================

1. On the Canvas, add two TextMeshPro - Text objects:
   - "InventoryLabel" — anchored top-left
   - "DetectionModeLabel" — anchored top-right

2. Create an empty GameObject "DemoManager"
3. Add DemoInventoryDisplay component:
   - Inventory Label: drag InventoryLabel
   - Detection Mode Label: drag DetectionModeLabel
   - Controller: drag the Player's InteractionController

4. Add DemoDetectionModeSwitcher component:
   - Controller: drag the Player's InteractionController
   - Switch Key: Tab
   - Mode Label: drag DetectionModeLabel

====================================================
STEP 6: Test
====================================================

1. Press Play
2. Move the player with WASD/arrow keys
3. Walk toward each object:
   - Door: should highlight, show "Press E to open", press E to open
   - NPC: should show name tag, "Press E to talk", press E for dialogue log
   - Pickup: should float, show item name, press E to collect
   - Generator: set Require Hold = true, hold E to fill progress bar
4. Press Tab to switch detection modes
5. Check Console for debug logs confirming interactions

====================================================
CONTROLS SUMMARY
====================================================

WASD / Arrow Keys  — Move player
E                  — Interact / Hold to interact
Tab                — Switch detection mode (Trigger / OverlapCircle / Raycast)
