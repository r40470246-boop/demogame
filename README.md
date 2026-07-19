# 🐛 Worms Zone Clone — Unity Setup Guide

## Step 1: Unity Project Create Karo

1. Unity Hub open karo
2. **New Project** → **2D** template select karo
3. Name: `WormsZoneClone`
4. Location: `/Users/rohit/Documents/`
5. **Create Project** click karo

---

## Step 2: Photon PUN 2 Import Karo (FREE)

### Option A — Unity Asset Store se
1. Unity mein: **Window → Asset Store**
2. Search: `Photon PUN 2 Free`
3. **Add to My Assets** → **Open in Unity** → **Import**

### Option B — Direct download
1. [photonengine.com](https://www.photonengine.com) pe jaao
2. **Sign Up** (free) karo
3. **Dashboard → Create New App → Photon PUN**
4. **App ID** copy karo
5. Unity mein: **Window → Photon Unity Networking → PUN Wizard**
6. App ID paste karo → **Setup Project**

---

## Step 3: Scripts Copy Karo

Ye folder apne Unity project ke `Assets/` folder mein copy karo:

```
WormsZoneClone/Assets/Scripts/  →  YourUnityProject/Assets/Scripts/
```

Ya individually:

```
Assets/Scripts/Core/
  ├── WormMovement.cs
  ├── WormBody.cs
  ├── WormHead.cs
  ├── FoodItem.cs
  ├── FoodSpawner.cs
  ├── WormSkinManager.cs
  └── GameManager.cs

Assets/Scripts/Multiplayer/
  ├── NetworkManager.cs
  ├── LeaderboardManager.cs
  └── (LobbyUI se connected)

Assets/Scripts/UI/
  ├── MainMenuUI.cs
  ├── LobbyUI.cs
  ├── GameHUD.cs
  └── GameOverUI.cs

Assets/Scripts/PowerUps/
  ├── PowerUpBase.cs
  ├── SpeedBoost.cs
  ├── FoodMagnet.cs
  ├── Shield.cs
  └── PowerUpSpawner.cs

Assets/Scripts/Utils/
  ├── JoystickController.cs
  └── CameraFollow.cs
```

---

## Step 4: LeanTween Import Karo (Animation ke liye)

1. Unity Asset Store mein search: `LeanTween`
2. **Free** hai — import karo

---

## Step 5: Scenes Banao

### Scene 1: MainMenu
1. **File → New Scene** → Save as `MainMenu`
2. **GameObject → UI → Canvas** banao
3. Canvas pe add karo:
   - `NetworkManager.cs`
   - `MainMenuUI.cs`
   - `WormSkinManager.cs`

### Scene 2: GameScene
1. **File → New Scene** → Save as `GameScene`
2. GameObject setup:

```
Hierarchy:
├── Main Camera          ← CameraFollow.cs attach karo
├── GameManager          ← GameManager.cs attach karo
├── FoodSpawner          ← FoodSpawner.cs attach karo
├── PowerUpSpawner       ← PowerUpSpawner.cs attach karo
├── LeaderboardManager   ← LeaderboardManager.cs attach karo
├── CameraShake          ← CameraShake.cs attach karo
└── Canvas
    ├── GameHUD          ← GameHUD.cs attach karo
    └── GameOverUI Panel ← GameOverUI.cs attach karo
```

---

## Step 6: Worm Prefab Banao

1. **GameObject → Create Empty** → naam: `WormPrefab`
2. Components add karo:
   - `Rigidbody2D` (Gravity Scale: 0)
   - `CircleCollider2D` (Is Trigger: ✅)
   - `SpriteRenderer`
   - `PhotonView`
   - `PhotonTransformView`
   - `WormMovement.cs`
   - `WormBody.cs`
   - `WormHead.cs`
3. Tag set karo: `Player`
4. **Prefab banao**: `Assets/Resources/` folder mein drag karo (naam exactly `WormPrefab` hona chahiye)

---

## Step 7: Tags Set Karo

**Edit → Project Settings → Tags & Layers** mein ye tags add karo:
- `Food`
- `WormBody`
- `PowerUp`
- `Boundary`
- `Player`

---

## Step 8: Android APK Build

1. **File → Build Settings**
2. **Android** select karo → **Switch Platform**
3. **Player Settings** mein:
   - Company Name: apna naam
   - Product Name: `Worms Zone`
   - Package Name: `com.yourname.wormszone`
   - Minimum API: Android 6.0 (API 23)
   - Target API: Latest
4. **Build & Run** click karo → folder choose karo → APK ready!

---

## Step 9: Joystick UI Setup

1. Canvas mein ek **Panel** banao (bottom-left pe)
2. Panel ke andar do circles banao:
   - Outer circle = Background (JoystickController.background)
   - Inner circle = Handle (JoystickController.handle)
3. Panel pe `JoystickController.cs` attach karo
4. Background aur Handle references assign karo

---

## Build Settings

```
Scenes in Build:
0. MainMenu
1. GameScene
2. Lobby (optional)
```

---

## ⚠️ Common Errors aur Fix

| Error | Fix |
|-------|-----|
| `PhotonView not found` | Worm prefab pe PhotonView component add karo |
| `Resources/WormPrefab not found` | Prefab exactly `Assets/Resources/WormPrefab.prefab` mein hona chahiye |
| `LeanTween not found` | LeanTween asset import karo ya FoodItem.cs se LeanTween lines remove karo |
| `TMPro not found` | Window → TextMeshPro → Import TMP Essentials |
| Build fail on Android | Android Build Support Unity Hub se install karo |

---

## 📞 Next Steps

Aage ye cheezein aur add kar sakte ho:
- 🎵 Background music aur sound effects
- 🗺️ Mini-map
- 💰 Coins system
- 🎁 Daily rewards
- 🏆 Global leaderboard (backend chahiye)
- 👥 Friends system
