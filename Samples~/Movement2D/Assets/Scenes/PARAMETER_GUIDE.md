# Movement2D Parameter Guide & State Cleanup Demo

## 🎛️ Parameter Effects on Movement Feel

### Transform Modes

#### TransformDirect
- **MoveSpeed**: Linear speed multiplier
  - 2-3 = Slow, methodical movement
  - 5 = Standard speed (default)
  - 8-10 = Fast, responsive movement
  - 15+ = Very fast, twitchy

#### TransformTranslate  
- **MoveSpeed**: Same as Direct
- **Space**: Movement reference frame
  - **World**: Always moves in world directions (up=world up, right=world right)
  - **Self**: Moves relative to object rotation (try rotating player 45° to see difference)

#### MoveTowards
- **MoveSpeed**: Target position distance
- **MaxDelta**: Step size per frame (most important!)
  - 1-2 = Very slow, deliberate movement
  - 5 = Standard speed (default)
  - 10-15 = Fast, snappy movement
  - 20+ = Very fast, may overshoot

#### LerpSmooth
- **MoveSpeed**: Target distance
- **LerpSpeed**: Smoothness factor (CRITICAL for feel)
  - 1-3 = Very floaty, dreamlike movement
  - 6 = Smooth but responsive (default)
  - 10-15 = Nearly instant, slight smoothing
  - 20+ = Almost direct movement
- **LerpTarget**: 
  - InputDirection = Move towards input direction
  - TargetTransform = Move towards object (click-to-move)

#### SmoothDamp
- **MoveSpeed**: Target distance
- **SmoothTime**: Spring-damper response time (MOST important!)
  - 0.05 = Tight, snappy, almost instant
  - 0.1 = Natural, organic feel (default)
  - 0.2 = Floaty, gentle movement
  - 0.5 = Very floaty, momentum-heavy
- **MaxSmoothSpeed**: Prevents overshoot
  - 5-10 = Normal range
  - 20+ = Allows fast movement but may overshoot

### Physics Modes

#### VelocityDirect
- **MoveSpeed**: Max velocity
- **HorizontalDeceleration**: Stop speed (CRITICAL for feel)
  - 0 = Instant stop (Super Meat Boy style)
  - 5 = Quick stop, slight slide
  - 8 = Natural deceleration (default)
  - 15+ = Long slide, ice-like
- **PreserveVertical**: Keep jump/gravity momentum
  - true = Keep Y velocity (platformers)
  - false = Reset Y to 0 (top-down games)

#### ForceAdditive
- **AccelerationForce**: How quickly full speed is reached
  - 5 = Very slow acceleration
  - 15 = Normal acceleration (default)
  - 30 = Quick acceleration
  - 50+ = Instant acceleration
- **MaxSpeed**: Speed cap
- **Drag**: Slide-to-stop behavior (VERY important!)
  - 0 = Pure ice, never stops
  - 2 = Light drag, slight slide (default)
  - 5 = Normal drag, quick stop
  - 10+ = Heavy drag, mud-like

#### ForceImpulse
- **ImpulseForce**: Push strength per input
  - 2 = Gentle pushes
  - 8 = Standard pushes (default)
  - 15+ = Strong pushes
- **ImpulseCooldown**: Delay between pushes
  - 0.05 = Rapid fire, shaky movement
  - 0.1 = Standard timing (default)
  - 0.3 = Deliberate, spaced pushes

#### KinematicMovePosition
- **MoveSpeed**: Linear speed
- **CollisionDetection**: Prevents tunnelling
  - Discrete = Faster, may pass through thin objects
  - Continuous = Slower but more accurate
- **InterpolationMode**: Visual smoothing
  - None = No smoothing, may appear jerky
  - Interpolate = Smooth between physics updates
  - Extrapolate = Predict movement, may jitter

## 🧪 State Cleanup Demo Setup

### Why State Cleanup Matters
Without cleanup, switching from ForceAdditive (which accumulates velocity) to TransformDirect (which has no velocity control) leaves the character sliding uncontrollably.

### Demo Setup Steps

#### Step 1: Create Test Scene
1. Create new Unity scene
2. Add Player with Movement2D_UMFOSS + SpriteRenderer + Rigidbody2D
3. Add UI Text to show current mode

#### Step 2: Add Debug Visualization
Add this script to Player to visualize state:
```csharp
using UnityEngine;
using TMPro;
using GameplayMechanicsUMFOSS.Movement;

public class StateCleanupDemo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI modeText;
    [SerializeField] private TextMeshProUGUI velocityText;
    [SerializeField] private Movement2D_UMFOSS movement;
    
    private void Update()
    {
        if (movement != null)
        {
            modeText.text = $"Mode: {movement.CurrentMode}";
            
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                velocityText.text = $"Velocity: {rb.linearVelocity:F2}";
            }
        }
    }
}
```

#### Step 3: Test State Cleanup

**Test 1: ForceAdditive → TransformDirect**
1. Start in **ForceAdditive** mode
2. Move around to build up velocity
3. While moving, switch to **TransformDirect**
4. **Expected**: Movement should instantly stop (no sliding)
5. **If broken**: Character continues sliding uncontrollably

**Test 2: VelocityDirect → SmoothDamp**
1. Start in **VelocityDirect** mode
2. Move at full speed
3. While moving, switch to **SmoothDamp**
4. **Expected**: Smooth transition, no velocity carryover
5. **If broken**: Jerky movement or velocity preservation

**Test 3: LerpSmooth → ForceImpulse**
1. Start in **LerpSmooth** mode
2. Move smoothly around
3. While moving, switch to **ForceImpulse**
4. **Expected**: Clean switch to staccato movement
5. **If broken**: Smooth movement continues or weird behavior

### Step 4: Parameter Adjustment Demo

**Create Parameter Test Buttons**
```csharp
using UnityEngine;
using UnityEngine.UI;
using GameplayMechanicsUMFOSS.Movement;

public class ParameterTestDemo : MonoBehaviour
{
    [SerializeField] private Movement2D_UMFOSS movement;
    
    public void TestSlowMovement() => movement.SetMode(MovementMode.LerpSmooth);
    public void TestFastMovement() => movement.SetMode(MovementMode.VelocityDirect);
    public void TestIcePhysics() => movement.SetMode(MovementMode.ForceAdditive);
    public void TestInstantMovement() => movement.SetMode(MovementMode.TransformDirect);
    public void TestFloatyMovement() => movement.SetMode(MovementMode.SmoothDamp);
}
```

**Button Setup:**
1. Create 5 UI Buttons
2. Link each to a test method above
3. Add parameter adjustment sliders for current mode

### Step 5: Live Parameter Testing

**For SmoothDamp Mode:**
```csharp
// Add these to your demo script
[Range(0.05f, 0.5f)] public float smoothTime = 0.1f;
private Movement2D_UMFOSS movement;

void Update()
{
    if (movement.CurrentMode == MovementMode.SmoothDamp)
    {
        // Use reflection or public setter to change smoothTime
        // Movement should feel different immediately
    }
}
```

## 🎯 Expected Results

### Proper State Cleanup:
- **Instant stop** when switching to transform modes
- **No velocity carryover** between different physics modes
- **Clean transitions** between all 9 modes
- **Predictable behavior** every time

### Parameter Effects:
- **Low values** = Slow, floaty, dreamlike
- **Medium values** = Natural, responsive movement
- **High values** = Fast, twitchy, responsive
- **Extreme values** = May cause instability

## 🔍 Debug Tips

1. **Watch Console** for cleanup errors
2. **Monitor Velocity** in Inspector during mode switches
3. **Test Edge Cases**: Switch modes while moving diagonally
4. **Check Rigidbody Settings**: Make sure they're appropriate for each mode

This demo will clearly show both the parameter effects and the importance of state cleanup!
