# Tank Game Physics Demo – Mini Tanks on a Messy Table

![Tank 1](media/tank1.webp)
![Tank 2](media/tank2.webp)
![Tank 3](media/tank3.webp)

This project demonstrates a physics‑driven tank combat system set on a tabletop environment. It features fully functional player and AI‑controlled tanks with realistic movement mechanics, projectile trajectories, and a class‑based finite state machine for enemy behavior.

## Overview

Mini tanks battle on a table, kicking up chaos as they move. The focus is on **custom physics simulation** – from tread generation and friction calculations to projectile mathematics – all built from the ground up to achieve responsive, believable tank behavior.

## Key Features

**Tank Movement & Tread System**

- Simplified geometry – Tanks move on two "skids" (visual treads are procedural).
- Procedural treads – Generated on‑the‑fly, they follow a 4‑node path of straight segments. At corners, each tread link rotates around the node with a wheel‑radius offset, creating a continuous track animation.
- Friction from first principles – Because Unity's physics API doesn't expose friction directly, a custom Newtonian model is used:
  - The gravity vector `g` is decomposed into a component normal to the surface and a component parallel to it (static friction).
  - An opposing linear friction force is calculated.
  - Torque arises when the contact patch's center is offset from the center of mass on a slope; this is countered by a custom friction torque.
  - All forces are combined into a single `AddForce` and `AddTorque` per `FixedUpdate`, compensating for the gravity that Unity will apply automatically.
  - Treads use a physics material with `0` static and kinetic friction – all friction is handled by the custom script.
- Stability – Tanks can stand still, move, and rotate in place on any slope without sliding.

**Turret & Projectile Physics**

- Independent turret rotation – The turret and gun traverse at a fixed speed toward the aim point (more realistic than instant snapping).
- Automatic targeting – The turret continuously tracks the target point (e.g., mouse position or enemy tank).
- Perfect trajectory calculation – Based on the classic monkey‑and‑projectile problem (howitzer and falling monkey):
  1. Given the gun's fire point, the target point, and a point directly above the target, the time for the projectile to reach the target under gravity is solved.
  2. The required initial impulse is then calculated, producing a trajectory that **always hits the target** – no trial and error, no misses.
  3. The calculation compensates for Unity's physics update timing, ensuring accuracy every frame.

**Enemy AI**

- Class‑based Finite State Machine – Clean, modular states (e.g., `PatrolState`, `AttackState`) managed by an `EnemyTankStateMachine`.
- Navigation – Uses NavMesh for pathfinding; the AI can patrol waypoints and engage the player.
- Combat – Detects targets within a radius, aims the turret, and fires at a configurable rate. Line‑of‑sight checks via obstacle masks.
- Modular inputs – Both player and AI tanks consume `inputX` / `inputY` through dedicated controller scripts (`PlayerInput`, `AITurretController`, etc.), allowing any tank to be driven by any input source.

**Interfaces & Damage System**

- `IDamagable` – Simple interface for damageable objects (tanks, test boxes).
- `IDrivable` – Abstraction for tank movement controls.
- `ITurretControlable` – Abstraction for turret aiming and firing.
- Damage requests – Structured as `Damage.Request` structs, containing damage amount, type, and source for clean event handling.

**Technical Highlights**

- Custom friction without Unity's built‑in physics materials – The tank's stability and motion are entirely script‑driven.
- Monkey‑and‑projectile mathematics – A textbook physics problem applied to real‑time gameplay, guaranteeing perfect aim.
- Procedural tread animation – Links move along a path and rotate around corner nodes, creating a convincing track effect without complex rigging.
- Dynamic homepoint creation – AI tanks generate a stable "home" position above the ground on startup, using raycasts to find the surface.

## How It Works (Brief)

1. **Tank movement**  
   Each tank's custom `TankMovement` script calculates the desired forward/rotational speed from `inputX`/`inputY`. It then determines how far the treads must "move" to achieve that motion, computes the necessary impulse (force × time), and applies it – along with the pre‑calculated friction forces – to the `Rigidbody`.

2. **Turret aiming & firing**  
   The turret rotates toward the aim point at a fixed angular speed. When the fire command is given, the projectile's start position, target position, and a point above the target are used to solve for the required launch velocity using the monkey‑and‑projectile equations. The result is a force impulse that guarantees a hit.

3. **AI behavior**  
   The `EnemyTankStateMachine` switches between states based on conditions (e.g., player in range). Each state handles its own logic – patrolling moves the tank via `AINavigationController`, attacking drives the turret via `AITurretController` and triggers firing.

## Result

The tanks move convincingly over any surface, treads animate procedurally, and shots always land exactly where aimed – whether by player or AI. The system is modular, making it easy to add new tank types, AI states, or projectile behaviors.

---

## Future Optimizations & Proof of Concept
