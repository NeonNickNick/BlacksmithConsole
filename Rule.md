# "Blacksmith" Core Rulebook v1.0

## 1. Basic Logical Architecture

### 1.1 Core Mechanics
* **Game Mode:** Two-player synchronous turn-based game.
* **Action Economy:** Each turn, every player can and must declare exactly one skill. All actions cost 1 turn of time.
* **Victory Conditions:** Both players start with 10 HP and 10 Max HP. At the end of any turn's resolution, a player with HP $\le 0$ is declared dead. If both players drop to 0 HP or below in the same turn, it results in a double-death draw.

### 1.2 Spatial Partitioning and Resolution Topology
The game space is logically divided into a symmetrical linear model. Attack resolutions travel from the attacker through the following defensive lines to reach the target:
**[Player A]** HP Zone $\leftarrow$ Damage Reduction Zone (Membrane) $\leftrightarrow$ Attack Prep Zone $\leftrightarrow$ **[Clash Zone]** $\leftrightarrow$ Attack Prep Zone $\leftrightarrow$ (Membrane) $\rightarrow$ Damage Reduction Zone $\rightarrow$ HP Zone **[Player B]**

### 1.3 Turn Resolution Sequence
Each turn strictly follows this order:
1.  **Turn Start:** Inherit states from the previous turn; some skills require retrieving values during this phase.
2.  **Action Declaration:** Both players synchronously declare skills and deduct the corresponding resources (or HP, etc.). Resources gained (Time, Space, Iron, gIron, Magic) receive a "pending confirmation" tag. Resolution is independent of attack skill tags.
3.  **Healing Resolution:** Played healing effects are applied to the HP zone.
4.  **Armor Resolution:** Played armor is applied to the Armor zone.
5.  **Damage Reduction (Def) Resolution:** Damage reduction values are loaded into the Defense Zone; the "Time Shield" skill is resolved here.
6.  **True Attack Resolution:** True damage attacks are resolved.
7.  **Attack Resolution:** Attack values generated this turn, plus values in the Attack Prep Zone scheduled for this turn, enter the Clash Zone as ordered vectors. Different damage types are sorted by priority; same-type damages are sorted by the order they entered.
8.  **Clash Resolution:** Attacks in the Clash Zone follow a 1:1 equivalent cancellation rule. The system ignores damage types and tags here, calculating the total attack power of both queues. The side with the lower total damage has its queue fully destroyed. The side with the higher total is the winner. The system uses the loser's total damage as a "depletion quota," subtracting from the front (head) of the winner's queue until the quota is exhausted. The surviving damage fragments in the winner's queue retain all original attributes/tags and proceed to the defender's Damage Reduction Zone. If totals are equal, both queues are destroyed simultaneously.
9.  **Damage Reduction Check:** Overflow attacks surviving the Clash enter the defender's Damage Reduction Zone. Each attack segment checks the single-turn defense values to modify the final attack value entering the HP Zone.
10. **Armor and Damage Resolution:** Damage breaking through defenses enters the HP Zone, deducting Armor first, then HP. Attack-attached tags are resolved.
11. **Resource Resolution:** After tag checks (if not interrupted or silenced), legal resources are officially granted to the players.
12. **Life/Death Resolution:** Turn ends. Players' HP is checked; $\le 0$ means death. If one dies and the other survives, the survivor wins; if both die, it is a draw.

---

## 2. Resource and Value System

### 2.1 Resource System
**Basic Resources:**
* **Iron:** The foundational currency unit; the minimum granularity is 0.5.
* **Time / Space:** High-tier universal resources obtainable without equipment.

**Advanced/Exclusive Resources (Requires Class Unlock):**
* **Magic:** Warlock-exclusive resource.
* **gIron (Gold-Iron):** Alchemy product. Has absolute equivalence to Iron in all checks except the "Touch of Midas" skill.

### 2.2 Damage Types
* **Standard Attack (ATK):** Follows the standard resolution flow (Clash $\rightarrow$ Reduction $\rightarrow$ Armor $\rightarrow$ HP).
* **Magic Attack (MATK):** Priority > ATK. Follows standard flow but is immune to the Spiked Shield (CDEF) recoil mechanism.
* **True Attack (RATK):** Priority > MATK. Ignores the Clash Zone, Damage Reduction Zone, and Armor Zone; directly deducts equivalent HP from the HP zone.

### 2.3 Damage Reduction Types (Single Turn Expiration)
Defense attributes only take effect in the current turn and reset to zero at the turn's end. The defense queue acts as a static wall; it is not altered by attacks, but attacks are modified by it. Priority order matches the order the walls were deployed.
* **Standard Defense (DEF):** Standard attack mitigation.
* **Spiked Shield Defense (CDEF):** Priority > DEF. After resolution, 50% of the damage mitigated by this shield (rounded up) is stored as MATK in the opponent's Attack Prep Zone, to be resolved against the opponent next turn.
* **True Defense (RDEF):** Priority > CDEF. Standard attack mitigation, but immune to penetration effects like "Armor Piercing".

### 2.4 Armor Types (Permanent Retention)
Armor resides in the HP zone, does not clear at the turn's end, and acts as an external buffer for HP. Priority is based on the order damage is taken.
* **Standard Armor (AMR):** Standard buffer; takes damage before HP.
* **True Armor (RAMR):** Priority > AMR. Same as above, but immune to "Armor Piercing" effects.
* **Stone Armor (SAMR):** Priority > RAMR. (Note: Can absorb overflow damage once broken; see Cauldron rules).

---

## 3. Skill Resolution List

### 3.1 Basic Common Skill Tree
Skills that require no prerequisites.

| Skill Name | Resource Cost | Output / Effect |
| :--- | :--- | :--- |
| Forge | 0 | 1 Iron |
| Pierce | 0.5 Iron | 1 ATK |
| Drill | 1.5 Iron | 3 ATK |
| Cut | 2.5 Iron | 5 ATK |
| Shield (n) | 0 + 0.5n Iron | 2 + n DEF |
| Spiked Shield (n) | 1 + 0.5n Iron | 4 + n CDEF |
| Heal (n) | n Iron | Restore 2n HP |
| Time | 3 Iron | 1 Time |
| Space | 3 Iron | 1 Space |
| Tear | 1 Space | 8 ATK |

### 3.2 Class System (Equipment)
* **Mechanic:** Advanced operations require purchasing a class. This counts as your valid action for the turn.
* **Restrictions:** Limited to one purchase per game. Once bound, it cannot be changed or unequipped. *Upon purchasing any class, the other primary class options permanently disappear from the player's available choices.* Some classes have secondary sub-equipment.

| Class Name | Unlock Cost | Gained Effect |
| :--- | :--- | :--- |
| Cannon | 4 Iron | Gains Cannon class traits + 3 DEF |
| Driver | 3 Iron | Gains Driver class traits |
| Warlock (Staff) | 1 Iron | Gains Warlock class traits |

### 3.3 Class-Exclusive Skill Libraries

#### A. Cannon
| Skill Name | Resource Cost | Output / Effect |
| :--- | :--- | :--- |
| Bombardment | 1 Iron | 4 ATK |
| Double Strike | 2 Iron | 7 ATK |
| Triple Strike | 3 Iron | 11 ATK |
| Gun Barrel | 0 | 1 ATK + 2 DEF |
| Armor-Piercing Shell | 1 Iron | 2 ATK. Includes special effects: <br>1. **Interrupt:** If this attack breaks through to the target's HP zone and deals $\ge 1$ damage, the opponent's "Forge" or "Accumulate Magic" action this turn becomes invalid.<br>2. **Triple Piercing:** When encountering a non-true defense/armor layer, the attack value temporarily swells to 3x its remaining original value (starts at 6 ATK) for deduction. For each layer it successfully penetrates, the remaining value is restored by dividing by 3 (rounded up). |

#### B. Driver
* **Passive Effect - Time Shield:** Every turn, without requiring an action, automatically treated as using a skill granting 1 RDEF. If the player holds $n$ Time resources at the start of the turn, they gain an additional $2n$ RDEF. The maximum RDEF generated by this passive is 5.

| Skill Name | Resource Cost | Output / Effect |
| :--- | :--- | :--- |
| Space Shock (n) | n Space | 12n ATK |
| Space-Time Shift | 1 Space | 1 Time + 3 RDEF |
| Time-Space Shift | 1 Time | 1 Space + 3 RDEF |
| Space Barrier (n) | n Iron | Yields $-0.5n^2 + 5.5n$ RDEF (where $n \le 5$) |

#### C. Warlock
| Skill Name | Resource Cost | Output / Effect |
| :--- | :--- | :--- |
| Accumulate Magic | 1 Iron | 1 Magic |
| Magic (n) | n Magic | Deals $2n$ MATK per turn. This damage is suspended in the Attack Prep Zone and triggers for 3 continuous turns starting from the current turn. |
| Silence | 0 | Opponent's Time/Space generation is invalid this turn |
| Sacrifice | 1 HP + 1 Max HP | 7 RDEF + 2 Iron |

**Warlock Secondary Equipment:**
| Equipment Name | Resource Cost | Output / Effect |
| :--- | :--- | :--- |
| Alchemy | 2 Iron | Unlocks "Touch of Midas" |
| Touch of Midas | 1 Iron | Yields 5 gIron |