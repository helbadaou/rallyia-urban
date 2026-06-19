# 🏙️ Unity Top-Down City Game

C# scripts for a top-down city driving game with road-following traffic.

## Scripts

| Script | Purpose |
|--------|---------|
| `TopDownCamera.cs` | Smooth orthographic camera that follows the player car from above |
| `CarController.cs` | WASD car controller with Rigidbody physics and road detection |
| `RoadDetector.cs` | Raycast-based detection for whether the car is on a road surface |
| `TrafficManager.cs` | Spawns and manages AI cars driving along waypoint paths |

## Setup Instructions

### 1. Create a New Unity Project
- Open Unity Hub → **New Project** → **3D Core** (Unity 2022+)
- Name it `CityDrivingGame`

### 2. Import Scripts
- Copy the `Assets/Scripts/` folder into your Unity project's `Assets/` folder

### 3. Setup the Scene

#### Camera
1. Select **Main Camera**
2. Add the `TopDownCamera` component
3. Set **Target** to your car (or leave empty to auto-find "Player" tagged object)
4. Set **Height** to `50` and **Orthographic Size** to `30`

#### Player Car
1. Create a 3D cube or import a car model
2. Add **Rigidbody** component
3. Add `CarController` component
4. Set **Tag** to `Player`
5. Add a **Box Collider** (or mesh collider)

#### Road Detection
- The `RoadDetector` component is auto-added by `CarController`
- **Option A (Recommended):** Tag your road meshes with the `Road` tag
- **Option B:** Set the road GameObjects to a `Road` layer
- **Option C:** Name your road materials with "road", "asphalt", "street", etc.

#### Traffic Manager
1. Create an empty GameObject → name it `TrafficManager`
2. Add the `TrafficManager` component
3. Create car prefabs (simple cubes work for testing)
4. Assign prefabs to the **Car Prefabs** array

#### Waypoint Paths
1. Create empty GameObjects along your roads → name them `WP_0`, `WP_1`, etc.
2. Position them in order along the road
3. In the Traffic Manager, create a **Road Path** entry
4. Drag the waypoints into the **Waypoints** array in order
5. Repeat for each road/street

### 4. Tag & Layer Setup
Go to **Edit → Project Settings → Tags and Layers**:
- Add tag: `Road`
- Add layer: `Road`

### 5. Press Play
- WASD or Arrow keys to drive
- Camera follows the car smoothly
- Traffic cars drive autonomously on their paths

## Controls

| Input | Action |
|-------|--------|
| W / ↑ | Accelerate forward |
| S / ↓ | Brake / Reverse |
| A / ← | Turn left |
| D / → | Turn right |

## Features

- ✅ Orthographic top-down camera with smooth follow
- ✅ Rigidbody-based car physics
- ✅ Road detection (slows down off-road)
- ✅ AI traffic that loops on waypoint paths
- ✅ Staggered car speeds for natural traffic look
- ✅ Camera bounds to stay within city limits

## Customization

### Adjust Camera
```csharp
// In TopDownCamera.cs
height = 60f;           // Higher = more zoomed out
smoothTime = 0.1f;      // Lower = snappier follow
orthographicSize = 40f; // Wider view
```

### Adjust Car Feel
```csharp
// In CarController.cs
moveSpeed = 20f;        // Faster car
turnSpeed = 10f;        // Quicker turning
offRoadSpeedMultiplier = 0.1f; // More penalty off-road
```

### Add More Traffic
```csharp
// In TrafficManager.cs
carCount = 20;          // More cars
minSpeed = 5f;          // Slower minimum
maxSpeed = 12f;         // Slower maximum
```
