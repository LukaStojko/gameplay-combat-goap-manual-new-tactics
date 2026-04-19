# GOAP AI System (Unity)

This project implements a **Goal-Oriented Action Planning (GOAP)** system in Unity.  
AI agents dynamically plan and execute actions based on the current world state using a flexible architecture.

Features:
- GOAP implementation with A\*-based planning  
- Modular design (Beliefs, Actions, Sensors, Goals)  
- Tactical waypoint integration  
- Emergent group behavior

---

## Core Architecture

The system is built around a **GoapPlanner** that uses an **A\*** algorithm to find the optimal sequence of actions.

### Components

- **Beliefs**  
  Represent the AI’s knowledge about the world (e.g., "enemyVisible", "inCover")

- **Actions**  
  Define behaviors with:
  - Preconditions
  - Effects
  - Cost (used by A\*)

- **Strategies (Goals)**  
  High-level objectives such as survival or attacking enemies

- **Sensors**  
  Update beliefs from the environment (vision, distance checks, cover detection, ally presence)

---

## Planning System

The planner evaluates action sequences using A\*:

- Nodes represent world states  
- Edges represent actions  
- Cost is based on action costs  
- Heuristic estimates distance to goal  

Process:
1. Update beliefs  
2. Select a goal  
3. Generate a plan  
4. Execute actions sequentially  

---

## Example Actions

- Idle  
- Patrol  
- Attack  
- Go To Cover  
- Form Attack Plan

---

## Coordinated Behavior

Agents coordinate with nearby allies by:

- Avoiding the same cover positions  
- Spreading across tactical areas  
- Reacting to shared environmental context  

Coordination emerges from shared beliefs and perception rather than explicit communication.

---

## Tactical Waypoints

The system includes **tactical waypoints** that define:

- Cover positions  
- Attack positions  

### Perception

AI evaluates waypoints based on:

- Distance  
- Exposure to enemies  
- Occupancy by allies  
- Tactical value  

These inputs influence planning decisions, allowing agents to choose effective positions dynamically.