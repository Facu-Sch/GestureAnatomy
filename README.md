# GestureAnatomy 🧬👋

An interactive 3D anatomical and biological model viewer for Meta Quest, using hand microgestures for intuitive navigation and exploration. Built as an exploration of Meta's microgesture capabilities applied to bioengineering education.

<img width="1919" height="722" alt="image" src="https://github.com/user-attachments/assets/b6bc66e1-bbd8-4ab5-aee8-6732f771d63f" />


## 🎯 Overview

GestureAnatomy allows users to explore anatomical and biological 3D models in augmented reality using natural hand gestures. The application leverages Meta Quest's microgesture detection system to provide a touch-free, immersive visualization experience.

**Current Models:**
- Human Skull
- Brain
- DNA Molecule

## ✨ Features

- **Gesture-Based Rotation**: Rotate models in any axis using hand swipes
- **Model Switching**: Navigate between different anatomical models with conveyor-belt transition effect
- **Zoom Control**: Smoothly zoom in/out for detailed inspection
- **Dual-Hand Controls**: Independent gesture controls for right and left hands
- **AR Positioning**: Models automatically position in front of the camera view

## 🕹️ Gesture Controls

### Right Hand
| Gesture | Action |
|---------|--------|
| Swipe Left | Rotate model around Y-axis (counterclockwise) |
| Swipe Right | Rotate model around Y-axis (clockwise) |
| Swipe Forward | Rotate model around X-axis (up) |
| Swipe Backward | Rotate model around X-axis (down) |
| Thumb Tap | Stop rotation |

### Left Hand
| Gesture | Action |
|---------|--------|
| Swipe Left | Previous model |
| Swipe Right | Next model |
| Swipe Forward | Zoom in |
| Swipe Backward | Zoom out |
| Thumb Tap | Stop zoom |

## 🛠️ Technologies

- **Unity**: 6000.0.2f1
- **Meta XR SDK**: com.meta.xr.sdk.all
- **Platform**: Meta Quest 3
- **Language**: C#

## 📋 Requirements

- Unity 6000.0.2f1 or higher
- Meta XR All-in-One SDK
- Meta Quest 3 headset
- Hand tracking enabled

## 🚀 Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/Facu-Sch/GestureAnatomy.git
   cd GestureAnatomy
   ```

2. **Open in Unity**
   - Open Unity Hub
   - Add project from disk
   - Select the cloned folder
   - Ensure Unity 6000.0.2f1+ is installed

3. **Load the Scene**
   - In the Project window, navigate to `Assets/Scenes/`
   - Drag `SampleScene` to the Hierarchy window to replace the default `Untitled` scene
   - Or double-click `SampleScene` to open it

4. **Install Meta XR SDK** (if not already installed)
   - Open Package Manager (Window > Package Manager)
   - Add package from git URL: `com.meta.xr.sdk.all`

5. **Configure Build Settings**
   - File > Build Settings
   - Add `SampleScene` to build
   - Player Settings > XR Plug-in Management > Enable Oculus

6. **Test in Unity Editor**
   - Connect Meta Quest 3 via USB
   - Enable Developer Mode on headset
   - Press Play in the Unity environment


## 📂 Project Structure

```
Assets/
├── Scripts/
│   ├── XRMicrogesturePrefabController.cs  # Main gesture controller
│   └── XRPrefabController.cs              # Keyboard controller (testing)
├── Models/                                 # 3D anatomical models
├── Materials/                              # Model materials
└── Scenes/
    └── SampleScene.unity                   # Main application scene
```

## 🎓 Development Context

This project was developed as part of an exploration into microgesture capabilities for the Immersive Technologies Group (GTI) at FIUNER, focused on bioengineering applications. The goal was to evaluate how natural hand gestures could enhance anatomical education in XR environments.

## 🗺️ Roadmap

- [ ] Add gesture hint overlays when viewing hands
- [ ] Expand model library (heart, lungs, skeletal system, cells)
- [ ] Implement model annotations and labels
- [ ] Multi-user collaborative viewing

## 🤝 Contributing

Contributions, issues, and feature requests are welcome!


## 🙏 Credits

- 3D Models sourced from [Sketchfab](https://sketchfab.com/)
- Developed with Meta XR SDK

## 👨‍💻 Author

**Facundo Nahuel Schneider**
- LinkedIn: [linkedin.com/in/facundo-schneider-a6045631b](https://www.linkedin.com/in/facundo-schneider-a6045631b)
- Email: facundoschneider5@gmail.com
- University: FIUNER - Immersive Technologies Group

---

*Built with Unity and Meta Quest for exploring the intersection of XR and bioengineering education.*
