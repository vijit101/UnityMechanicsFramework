# UnityMechanicsFramework

A modular, open-source collection of plug-and-play gameplay mechanics built for Unity.

Instead of rewriting commonly used systems across projects, this repository centralizes reusable and scalable gameplay mechanics designed with clean architecture and extensibility in mind. The goal is to provide production-ready systems that developers can integrate directly into their 2D games.

## 📂 Folder Structure

UnityMechanicsFramework/
│
├── package.json # UPM Manifest
├── README.md
├── CHANGELOG.md
├── LICENSE
│
├── Runtime/
│ ├── Core/
│ │ ├── MonoSingleton.cs
│ │ ├── EventBus.cs
│ │ └── StateMachine/
│ │
│ ├── Physics/
│ │ ├── IPhysicsAdapter.cs
│ │ ├── Physics2DAdapter.cs
│ │ └── Physics3DAdapter.cs
│ │
│ ├── Movement/
│ │ ├── ModularJumpSystem.cs
│ │ ├── DashSystem.cs
│ │ └── WallSlideSystem.cs
│ │
│ ├── Dialogue/
│ │ ├── DialogueSystem.cs
│ │ ├── DialogueNode.cs
│ │ └── DialogueDatabase.cs
│ │
│ ├── Input/
│ │ └── InputAdapter.cs
│ │
│ └── Utils/
│ └── TimerUtility.cs
│
├── Editor/ # Editor-only tools
│ ├── CustomInspectors/
│ └── PropertyDrawers/
│
├── Samples~/ # Example scenes & usage demos
│ ├── JumpExample/
│ └── DialogueExample/
│
└── Tests/
├── Runtime/
└── Editor/

---

## Mechanics Library

> Contributors may add new mechanics to this list along with their name.

---

### 1. MonoSingleton Generic  
**Author:** Shubham B  

A reusable generic singleton base class for `MonoBehaviour`.

Convert any script into a singleton by inheriting:

public class GameManager : MonoSingletonGeneric<GameManager>
{
}

### 2. Generic & Scalable Dialogue System  
**Author:** [Mayur](https://github.com/M-dev-acc)  

A ScriptableObject-based dialogue framework for building flexible and branching conversations in Unity.

**Highlights:**
- Data-driven architecture  
- Supports branching dialogue  
- Clean separation of data and logic  
- Easily extendable  
- Scalable for large narrative systems  

Designed to allow rapid iteration and safe expansion of dialogue trees without modifying the core system logic.  
New conversations can be added seamlessly while maintaining structural integrity.


