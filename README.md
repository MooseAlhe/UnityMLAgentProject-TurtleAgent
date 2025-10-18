# 🐢 Unity ML-Agents – Turtle Agent

An educational Unity **ML-Agents** project where a “Turtle” learns to **navigate toward a goal** while **avoiding static and moving obstacles**. Built to showcase incremental RL curriculum design—from simple goal-seeking to dynamic obstacle avoidance.

---

## 🎯 What This Project Demonstrates

- **Navigation in continuous space:** forward motion and turning with continuous actions.
- **Shaped rewards:** distance-to-goal improvements, proximity bonuses, time/energy penalties, success/failure terminals.
- **Curriculum-style progression:** four agent variants (`TurtleAgent1` → `TurtleAgent4`) that introduce obstacles and dynamics step-by-step.
- **Dynamic obstacles:** a vertically oscillating **MovingWall** forcing path timing and planning.
- **ML-Agents PPO training** with a reproducible YAML config (`config/turtle.yaml`).

---

## 🧠 Agent Variants & Behaviors

### 1) `TurtleAgent1` – Basic Goal-Seeking
- **Task:** Drive toward a visible goal on a flat plane.
- **Actions:** 2D continuous (turn rate, forward speed).
- **Observations:** Relative goal position (and/or local heading), basic state.
- **Rewards (typical):** Positive for reducing distance; small close-range bonus; mild penalty for excessive action magnitude; +1 on success, −1 on failure.
- **Visual Cue:** Floor tint indicates success/failure at end of episode.

### 2) `TurtleAgent2` – Randomized Starts
- **Task:** Same as #1 but with randomized start and goal spawn points each episode.
- **Purpose:** Generalization across the arena; robust heading alignment and approach behavior.

### 3) `TurtleAgent3` – Static Wall Obstacle
- **Task:** Reach the goal while **avoiding a static wall** placed between random start/goal locations.
- **Observations (added):** Wall position.
- **Rewards:** Distance shaping + penalties for wall collisions; success ends the episode.
- **Takeaway:** Develops path deviation and basic obstacle avoidance.

### 4) `TurtleAgent4` – Moving Wall Obstacle
- **Task:** Reach the goal with a **moving wall** (oscillating on one axis).
- **Observations (added):** Wall position and, implicitly via deltas, wall motion cues.
- **Rewards:** As above; stricter penalties for hitting the wall; optional shaping for timing windows.
- **Outcome:** The agent learns to time its crossing and plan a safer path.

### Support Script – `MovingWall`
- **Behavior:** Oscillates between two bounds (adjustable `speed` and amplitude).
- **Use:** Drop into the scene as an obstacle for `TurtleAgent4` training/evaluation.

---

## 🏗️ Scene & Project Structure

```
Assets/
 ├─ Scripts/
 │   ├─ TurtleAgent1.cs
 │   ├─ TurtleAgent2.cs
 │   ├─ TurtleAgent3.cs
 │   ├─ TurtleAgent4.cs
 │   └─ MovingWall.cs
 ├─ Scenes/
 │   └─ (training & demo scenes)
config/
 └─ turtle.yaml
results/
```

---

## ⚙️ Training Setup

- **Unity:** 2021 LTS or newer recommended
- **Packages:** `com.unity.ml-agents`, `com.unity.barracuda`
- **Python:** 3.9–3.11
- **PPO Config:** `config/turtle.yaml` (batch size 1024, buffer 10240, lr=3e-4 (linear schedule), γ=0.99, λ=0.95, time_horizon=64, etc.)

### Example (excerpt) – `config/turtle.yaml`
```yaml
behaviors:
  Turtle:
    trainer_type: ppo
    hyperparameters:
      batch_size: 1024
      buffer_size: 10240
      learning_rate: 0.0003
      beta: 0.005
      epsilon: 0.2
      lambd: 0.95
      num_epoch: 3
    max_steps: 200000000
    time_horizon: 64
    summary_freq: 50000
```

---

## 🚀 Quick Start

### 1) Open in Unity
- Clone or download the project and open with **Unity Hub**.
- In **Package Manager**, install **ML-Agents** and **Barracuda**.

### 2) Python Environment
```bash
python -m venv .venv
.venv\Scripts activate      # (Windows)
# or
source .venv/bin/activate   # (macOS/Linux)
pip install --upgrade pip
pip install mlagents mlagents-envs tensorboard
```

### 3) Train
```bash
mlagents-learn config/turtle.yaml --run-id turtle_run --time-scale 20
```
Press **Play** in the Unity Editor when prompted.

### 4) Monitor
```bash
tensorboard --logdir results
```

---

## 📊 Results & Observations

- `TurtleAgent1/2`: Rapid improvement in heading control and straight-line approach.
- `TurtleAgent3`: Emergent path selection to avoid static wall collisions.
- `TurtleAgent4`: Learns **timing** around a moving barrier; fewer collisions and smoother trajectories over time.
- Reward curves stabilize with adequate exploration (1–2M+ steps).

*(Add your own graphs, screenshots, or GIFs here.)*

---

## 🔍 Tips for Better Learning

- **Reward shaping:** Start dense (distance deltas + proximity bonuses), then anneal if overfitting to shaping.
- **Time scale:** Use higher `--time-scale` for speed, drop to `1` for visual demos.
- **Spawn diversity:** Randomize start/goal to improve generalization.
- **Obstacle pacing:** Increase wall speed gradually as the policy improves.

---

## 🧠 Extensions & Next Steps

- Add multiple moving obstacles with different phases.
- Partial observability: limit direct goal/wall positions, add raycasts or camera sensors.
- Domain randomization: vary friction, size, or wall amplitude.
- Curriculum learning pipelines: promote from Agent1 → 4 automatically.
- Evaluate with noise injection and stochastic resets.

## 👤 Author

**MooseAlhe** – Exploring continuous-control navigation and curriculum learning with Unity ML-Agents.
