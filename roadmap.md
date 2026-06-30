# City Sim — Roadmap

Full autonomy simulation. No player character. You build the city, set the conditions, watch society emerge.

---

## ✅ Complete

- ECS core (World, Entity, EntityQuery, IUpdateSystem)
- PresenterNode / state machine pattern
- Camera — zoom (cursor-anchored) + pan
- TileMap registration, walkability grid, fence edges
- Cross-level pathfinding (overworld ↔ interiors)
- Warp system (bidirectional tile pairs)
- Building prefab — door animation, interior popup (SubViewport)
- Character prefab — layered sprites (body/hair/clothing/face), auto-built SpriteFrames
- PathfindingSystem (async Task.Run), MoveToSystem (facing + animation)
- Tile-center coordinate convention
- Schedule system — `ScheduleComponent` / `ScheduleSystem`, world clock, three destination resolution modes (`/MapID/TileId`, `*LocationType`, `@tag`), `LocationRegistry`

---

## 🔨 Milestone 1 — Citizens Feel Alive

**Needs System**
- `NeedsComponent { float Hunger, float Energy, float Social }` — all decay over time
- `NeedsDecaySystem` — reduces each stat per second (configurable rates)
- `NeedsInterruptSystem` — monitors thresholds, interrupts current schedule when critical:
  - `Energy < 0.2` → go home, sleep
  - `Hunger < 0.2` → go to nearest restaurant/kitchen
  - `Social < 0.3` → go to nearest park/leisure spot
- Sleep slows hunger decay (metabolism factor)
- `InterruptPathfinding()` helper — cleanly detaches PathfindingComponent + MoveToComponent, resets schedule dispatch so the system re-evaluates immediately

---

## 🔨 Milestone 2 — World Has a Pulse

**Day/Night Cycle**
- `WorldClock` — in-game time advancing at configurable speed (TimeSpeed on SimWorld)
- Ambient light driven by hour — full day, twilight lerp, night dark-blue
- Street light entities emit a soft warm-orange radial glow on the light map
- Light map multiply-blended over the scene after all sprites are drawn

**Building Interiors Feel Real**
- Occupancy system — locations have a max capacity, citizens queue outside when full
- `QueueRegistry` — maps tile positions to queues; citizen moves to queue tile, waits turn
- Citizens face the right direction on arrival (snap facing to door direction)
- Interior scenes register as proper map levels with their own citizens + pathfinding

**Multiple Citizens**
- Spawn 10–20 citizens from `CitizenConfig` at scene load
- Varied body/hair/clothing/face variants
- Staggered schedules (different wake/work/eat/sleep times)
- Names, randomised personality traits influencing decay rates

---

## 🔨 Milestone 3 — Economy Emerges

**Jobs**
- `JobComponent { string Employer, float Wage, Schedule WorkSchedule }`
- Citizens pathfind to their workplace at work time, earn money on shift completion
- Buildings have a `StaffingComponent` — job slots, hiring/firing logic
- Unemployment: citizens without jobs have more free time, lower income

**Money**
- `WalletComponent { float Balance }`
- Shops/restaurants deduct balance on visit, reject if insufficient funds
- Wages deposited on shift end

**Shops & Commerce**
- Shops have `InventoryComponent` — stock levels, restock triggers
- Citizens spend money based on needs (hungry → restaurant, low mood → park/bar)
- Price elasticity seed: expensive shops drain wallets faster, citizens seek cheaper alternatives

**Property & Rent**
- Residential buildings have units with `RentComponent { float MonthlyRent, Entity? Tenant }`
- Citizens pay rent on the 1st of each in-game month
- Eviction if unable to pay — citizen becomes homeless, sleep quality drops, energy decay accelerates
- Landlord entities collect rent, accumulate wealth

---

## 🔨 Milestone 4 — Society Complexity

**Mood & Wellbeing**
- `MoodComponent` — composite score from needs + recent events
- Low mood affects productivity (work output reduced), social willingness, spending habits
- High mood unlocks leisure activities (park visits, socialising)

**Social Proximity**
- Idle citizens within N tiles of each other face each other and slowly restore Social
- `SocialInteractionComponent(Entity Other, TimeSpan Until)` tracks the pair
- On end: detach component, resume normal schedule

**Relationships**
- Citizens who interact frequently build `RelationshipComponent` links
- Friends visit each other's homes, sit together at restaurants
- Relationships decay without contact

**Reputation & Businesses**
- Businesses accumulate a reputation score based on customer satisfaction
- High reputation → more customers → more revenue → owner wealth grows
- Low reputation → fewer customers → eventual closure

**Crime & Poverty Loop**
- Citizens below a wealth threshold have a rising chance of petty crime
- Crime reduces area property values, increases stress on nearby citizens
- Police/law enforcement as a city service (budget-funded)

---

## 💡 Future Ideas

**Vehicles**
- Road-only navigation layer separate from pedestrian grid
- Citizens board/exit vehicles — handoff between foot and vehicle pathfinding
- Traffic queueing at intersections
- `EntityType.Vehicle` already anticipated in archive's navigation bit-mask system

**Weather System**
- `WeatherState` advances through clear → overcast → rain → storm
- Visual: scrolling noise shader for rain/snow, fog overlay, wind UV-offset on foliage
- Citizens pathfind indoors faster in rain, umbrella accessory layer shown
- Lightning: full-screen bloom spike + white flash at random storm intervals
- Puddles accumulate on ground layer during rain, ripple shader

**Political System**
- City council with elected officials
- Citizens vote based on mood, wealth, and direct policy impact
- Policies affect tax rates, zoning, services — feed back into economy

**Save / Load**
- JSON serialisation of all component state per citizen
- `OccupancyRegistry` reconstructed from positions on load
- World time saved and resumed

**Generational Play**
- Citizens age, retire, die
- Offspring inherit some traits, start with parents' wealth
- City evolves over decades without any player intervention

---

## Architecture Principles (keep these in mind)

- ECS is pure data/logic — no Godot types in components or systems except `GodotNodeComponent`
- Presenters are Godot nodes that bridge ECS state to visuals — they poll, never push
- All positions in tile grid coords (`Vector2I`) — world positions derived at the last moment via `MapToGlobal` extension method
- Systems registered in `SimWorld.Bootstrap` — order matters (Pathfinding before MoveTo)
- Schedules and needs are authoritative in ECS — UI only reads, never writes
