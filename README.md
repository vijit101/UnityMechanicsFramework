# UnityMechanicsFramework

A modular, open-source collection of plug-and-play gameplay mechanics built for Unity.

Instead of rewriting commonly used systems across projects, this repository centralizes reusable and scalable gameplay mechanics designed with clean architecture and extensibility in mind. The goal is to provide production-ready systems that developers can integrate directly into their 2D games.

---

## Mechanics Library

> Contributors may add new mechanics to this list along with their name.

---

### 1. MonoSingleton Generic  
**Author:** Shubham B  

A reusable generic singleton base class for `MonoBehaviour`.

Convert any script into a singleton by inheriting:

```csharp
public class GameManager : MonoSingletonGeneric<GameManager>
{
}
