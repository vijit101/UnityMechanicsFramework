# Movement2D Complete Setup Guide

## 🚀 START FROM SCRATCH - Complete Step-by-Step Guide

### Step 1: Create New Unity Project
1. Open Unity Hub
2. Click "New Project"
3. Select **2D** template
4. Name it anything (e.g., "Movement2DTest")
5. Click "Create Project"
6. Wait for Unity to load the new project

### Step 2: Add the Movement Package
1. In Unity, go to **Window → Package Manager**
2. Click the **"+"** icon in top-left corner
3. Select **"Add package from disk..."**
4. Navigate to: `/Users/kumarkartikay/Desktop/Notes/Game Dev/UnityMechanicsFramework`
5. Select the **package.json** file
6. Click **Open**
7. Wait for Unity to import the package

### Step 3: Verify Package Import
1. In Package Manager, you should see "Unity Mechanics Framework"
2. Open your **Project** window (usually at bottom)
3. Look under **Packages** → You should see the framework files
4. Check these folders exist:
   - `Packages/Unity Mechanics Framework/Runtime/Movement/`
   - `Packages/Unity Mechanics Framework/Runtime/Utils/Adapters/`

### Step 4: Check the Core Files
Verify these files exist and can be opened:
1. **Movement Script**: `Packages/Unity Mechanics Framework/Runtime/Movement/Movement2D_UMFOSS.cs`
2. **Adapters**: `Packages/Unity Mechanics Framework/Runtime/Utils/Adapters/IAdapters.cs`
3. **Unity Adapters**: `Packages/Unity Mechanics Framework/Runtime/Utils/Adapters/UnityAdapters.cs`
4. **Demo Script**: `Packages/Unity Mechanics Framework/Runtime/Movement/Movement2DDemoController.cs`

### Step 5: Create Test Scene
1. In Unity, go to **File → New Scene**
2. Select **2D Template** (or Basic if 2D not available)
3. Save the scene: **File → Save As**
4. Name it `MovementTest` and save in `Assets/Scenes/`

### Step 6: Create Player GameObject
1. In Hierarchy, right-click → **Create Empty**
2. Name it **"Player"**
3. With Player selected, add components:
   - **Add Component → SpriteRenderer**
   - **Add Component → Rigidbody2D**
   - **Add Component → Movement2D_UMFOSS** (search for this)
4. Configure SpriteRenderer:
   - Click the Sprite field → Select Unity's default square sprite
   - Set Color to something visible (e.g., red)

### Step 7: Configure Rigidbody2D
1. Select the Player GameObject
2. In Rigidbody2D component:
   - **Gravity Scale**: Set to 0 (for 2D top-down movement)
   - **Freeze Rotation Z**: Check this box

### Step 8: Configure Movement2D_UMFOSS
1. In the Movement2D_UMFOSS component:
   - **Movement Mode**: Start with "TransformDirect"
   - **Move Speed**: Set to 5
   - **Face Direction**: Check this box
2. Leave other settings at default for now

### Step 9: Setup Camera
1. Select **Main Camera** in Hierarchy
2. In Camera component:
   - **Projection**: Set to **Orthographic**
   - **Size**: Set to **10**
   - **Position**: Set to (0, 0, -10)

### Step 10: Basic Test (No UI Required)
1. Press **Play** button
2. Use **Arrow Keys** or **WASD** to move the player
3. The red square should move instantly (TransformDirect mode)
4. If this works, the basic system is functioning!

### Step 11: Test Different Movement Modes
While still in Play mode:
1. Select the Player GameObject
2. In Movement2D_UMFOSS component, change **Movement Mode**:
   - **TransformDirect**: Instant, pixel-perfect movement
   - **TransformTranslate**: Similar to Direct
   - **MoveTowards**: Linear, constant speed
   - **LerpSmooth**: Smooth, floaty movement
   - **SmoothDamp**: Springy, organic feel
   - **VelocityDirect**: Standard platformer feel
   - **ForceAdditive**: Slippery/ice physics
   - **ForceImpulse**: Staccato, push-based
   - **KinematicMovePosition**: Collision-aware

### Step 12: Test Mode Switching
1. While moving, switch between modes
2. Each mode should feel distinctly different
3. No velocity should "bleed" between modes (proper cleanup)
4. Try switching from physics modes to transform modes

### Step 13: Test Face Direction
1. Move left and right
2. The sprite should flip horizontally when changing direction
3. This should work across all movement modes

### Step 14: (Optional) Add UI for Better Testing
If you want the full demo experience:
1. Right-click Hierarchy → **UI → Canvas**
2. Right-click Canvas → **UI → Text - TextMeshPro**
3. Name it "ModeDisplay"
4. Create a simple script to show current mode (or use SimpleMovementDemo if available)

### Step 15: Verify All Features Work
Check these key features:
- ✅ Basic movement with Arrow Keys
- ✅ All 9 movement modes feel different
- ✅ Mode switching works without velocity bleeding
- ✅ Face Direction works correctly
- ✅ No errors in Console
- ✅ Physics modes require Rigidbody2D, Transform modes don't

## 🎯 Expected Results

### Transform Modes (1-5):
- **TransformDirect**: Instant start/stop, pixel-perfect
- **TransformTranslate**: Same as Direct (try rotating object to test Space.Self)
- **MoveTowards**: Linear movement, constant speed
- **LerpSmooth**: Smooth, floaty, exponential ease
- **SmoothDamp**: Springy, slight overshoot, organic

### Physics Modes (6-9):
- **VelocityDirect**: Responsive, slight slide on stop
- **ForceAdditive**: Builds up speed, slippery, takes time to stop
- **ForceImpulse**: Discrete pushes per input
- **KinematicMovePosition**: Solid, collision-aware

## 🔧 Troubleshooting

### If Movement Doesn't Work:
1. Check Console for errors
2. Verify Rigidbody2D is configured correctly
3. Make sure Movement2D_UMFOSS component is added
4. Check that sprite is visible (not hidden behind camera)

### If Package Import Fails:
1. Make sure you selected package.json file
2. Try restarting Unity
3. Check that the package path is correct

### If Scripts Have Errors:
1. Open the script files in Unity's code editor
2. Check for missing using statements
3. Verify all references are correct

## 🎮 Success Indicators

You know it's working when:
- Arrow Keys move the player
- Each mode feels distinctly different
- No errors in Console
- Mode switching is instant and clean
- Face Direction flips correctly

This complete guide should get you from a fresh Unity project to a fully working Movement2D test!
