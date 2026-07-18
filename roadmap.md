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
- Needs System — `NeedsComponent { float Satiety, Energy, Social }`, decay per second via `NeedsSystem` (configurable rates in `Globals`)
- Energy interrupt — `Energy < MinEnergyNeed` cancels current schedule, sends citizen home to sleep; sleep slows satiety decay (metabolism factor); `InterruptPathfinding()` helper cleanly detaches PathfindingComponent + resets schedule dispatch for immediate re-evaluation
- State effect system — `OnArriveEffects` (per-schedule-entry, fired once on pathfinding arrival) + `StateEffectRegistry` / `StateSystem` (declarative enter/exit effects keyed by `ActivityType` or `(from, to)` transition, fired on any `ActivityTypeComponent` change) — general-purpose infrastructure for milestones 2+
- Day/Night Cycle — `CanvasModulate` + `DayNightCycle` sample an Inspector-authored `Gradient`/`Curve` off `SimWorld`'s clock for ambient tint and a shared `DayBlend`; `PointLight2D` street lights and window glows (`LightCone.png` / `LightConeWide.png` light-cookie textures, standalone files rather than atlas regions since `Light2D` doesn't honour `AtlasTexture` cropping) ramp on/off it; `LightOccluder2D` on building prefabs (reusing their collision polygons) casts real shadows
- Interior scenes register as proper map levels with their own citizens + pathfinding — cross-level pathfinding correctly resolves and places citizens in their actual interior instance
- Citizens face the right direction on arrival — `FacingDirectionEffect` (`OnArriveEffects`) reads the destination `Location`'s `FacingDirection` and snaps the citizen to face it, e.g. toward a door
- Occupancy system — locations have a max capacity, citizens queue outside when full
- `QueueRegistry` — maps tile positions to queues; citizen moves to queue tile, waits turn
- Save / Load — JSON serialisation of all component state per citizen, `OccupancyRegistry` reconstructed from positions on load, world time saved and resumed
- Jobs — `JobComponent { string Employer, string Title }` + `EmployerRegistry` (wage/schedule lookup by employer+title); `ScheduleSystem` pulls `EmployerRegistry.GetSchedule(...)` into a citizen's schedule so they pathfind to work
- Wallets — `Wallet`/`WalletRegistry` (Guid-keyed, mirroring the `Inventory`/`InventoryRegistry` pattern); `InventorySystem` deducts balance on purchase and rejects insufficient funds
- Shops & Commerce loop (hunger→shop→buy→eat) — `HungerComponent` (armed/disarmed by `NeedsSystem` on the `Satiety` threshold) → `ConsumptionSystem` (branches on already-has-food-in-inventory vs. needs-to-shop, re-entrancy guarded via `Without<PathfindingComponent/BrowseShopComponent/ItemTransferRequestComponent>`) → `ShopSystem` (scores browse against `PreferenceComponent`, raises `ItemTransferRequestComponent` on a match) → `InventorySystem` (resolves the transfer against `WalletRegistry`) → `FactComponent`/`MemorySystem` (turns browse/transfer outcomes into `ShopQueryMemory` entries); `AttachComponentEffect`'s named-tuple matching JIT-fills `BrowseShopComponent.EntityID` with the arrived shop's `EntityID` on pathfinding arrival
- Memory-driven shop avoidance — `MemoryComponent.GetAvoidStringByNegativeShopQuery` feeds `LocationRegistry.Resolve`'s `!{EntityID},{EntityID}` avoid syntax from `ConsumptionSystem`, so citizens stop returning to shops that recently let them down
- Memory decay — `IMemory.Satisfaction` is a derived ease-out of `Age` over a `Lifespan` (`Globals.MemoryLifespanPerUnit`) that scales with `|OriginalSatisfaction|` (bigger deal → persists longer, near-zero memories fade fast)
- Eating removes the consumed item from inventory via `InventoryEffect` (signed: positive adds, negative removes) as one of the dining trip's `OnArriveEffects`
- Money is earned by checking the Wage property on `JobComponent` during `WalletSystem`'s update
- Save/Load now round-trips everything, not just citizen components — `Wallet`/`Inventory`/`OccupancyRegistry`/`QueueRegistry` are captured and restored as part of `SaveGameData.Registries`; citizen `Entity` IDs survive a load (`World.CreateEntity(Guid)` overload) instead of being randomised, so Guid-keyed registries actually reconnect; `LoadGame` re-runs `PersonPresenter.PreBootstrap()`/`Bootstrap()` per citizen so loaded citizens get their presenter state machine, `WalletComponent`, `GodotNodeComponent` and `HomeComponent` back instead of sitting visually inert
- `LocationKey` record (`Name`, `Map`) replaces raw tuple/struct dictionary keys in `OccupancyRegistry`/`QueueRegistry` — `Location`'s `string[] Tags` field broke record-generated equality when used as a dictionary key (arrays compare by reference), which would've silently fragmented queues/occupancy after a save/load round-trip
- `NeedsSystem` now disarms `HungerComponent`/`TiredComponent` once the need recovers (was arm-only — citizens would get stuck re-triggering shopping trips forever after eating)
- Headless test infrastructure — `SimWorld.Instance` settable internally (no scene tree needed), `MapRegistry.RegisterTestMap` injects raw walkable tiles/fence edges directly (real A* pathfinding, no `TileSet`/`TileMapLayer` authoring required), optional `movementLayer` param for tests that need real per-tile `MoveToComponent` stepping (e.g. interrupting a citizen mid-route) rather than single-tick arrival
- `Tests/HungerLoopTest.cs` — first full end-to-end integration test: a citizen crosses the hunger threshold, shops for a preferred item, buys it (wallet deducted, inventory transferred), walks to eat, and ends up measurably less hungry, exercising `NeedsSystem`/`ConsumptionSystem`/`ShopSystem`/`InventorySystem`/`PathfindingSystem`/`StateSystem`/`DelayedEffectSystem` together. Immediately caught several real bugs on first run: `ItemRegistry`'s dictionary initializer missing `new()` (crashed the moment anything touched it), `ActivityTypeEffect`'s constructor param not matching its property (broke JSON save/load for any effect using it), `ShopSystem` crashing on an empty browse result, `Inventory` having no way to price stock for sale
- Skills system — `Skill` enum, `SkillsComponent` (0–10 clamped `IncreaseSkill`, `WithSkill` fluent initializer for seeding), `SkillDeltaEffect` (used as an `ActivityTypeEffect.OnCompleteEffects` reward — e.g. shift-end Charisma/Dexterity gains for Cashiers, Charisma/Wisdom for Managers), `SkillHelpers.SatisfiedBy` for margin-tolerant requirement checks against a citizen's skill dictionary; round-trips through save/load
- Hiring loop (apply → interview → decision) — `JobSeekingComponent` / `JobApplicantComponent` / `JobApplicationResultComponent` + `JobSystem` drive the full pipeline: unemployed citizens (`CitizenComponent`-tagged) get marked job-seeking, `EmployerRegistry.GetQualifiedVacancies(...)` returns only unfilled + skill-qualified jobs per employer (not just "any job at an employer with at least one qualifying slot," which let citizens land on jobs they didn't actually qualify for), scored by wage minus commute distance; the interview itself is just another `ActivityTypeEffect(ActivityType.Interview, ...)` whose `OnCompleteEffects` calls `EmployerRegistry.MarkJobFilled` + attaches `JobComponent` on success, or logs an `AlreadyApplied` entry + a `ConfidenceMemory` (via `JobInterviewFact`/`MemorySystem`) on failure so citizens don't immediately reapply somewhere that just turned them down
- Interview location split — regular applicants interview at the employer's `ManagerVisitor` location; `Manager` applicants "phone/video interview" from home instead, resolved via `LocationRegistry.Resolve("/{homeMapID}/@admin")`. Since there's only ever one manager per building, the job title is just the bare `"Manager"` rather than the `{Title}_{EntityID}` scheme every other job uses — this is what unblocks hiring the very first manager at a brand-new building with no staff to interview them
- `ShopBuildingPresenter` sets up the Manager role alongside Cashiers — its own schedule, `ManagerLocation`/`ManagerVisitorLocation` child nodes, shift-end skill gains; will eventually get pulled up into a shared `WorkBuildingPresenter` base once `OfficeBuildingPresenter`/`RestaurantBuildingPresenter` etc. need the same staffing layer
- Bugs the hiring loop work shook loose: `StateSystem`'s `End`-based activity completion first never fired past the tick an activity started (an unrelated `continue` on unchanged `ActivityType` was skipping the check entirely), then — once reordered — fired every tick forever once triggered, because nothing cleared `End`/`OnCompleteEffects` afterward; both had to be fixed for any `durationHours` `ActivityTypeEffect` (Eat, Work, Interview) to resolve exactly once. `JobSystem`'s interview-dispatch loop was also redispatching an applicant to their interview every tick once `PathfindingComponent` cleared on arrival, endlessly resetting the interview timer. `LocationRegistry.Get`'s `@tag` lookup branch skipped `TryResolveOccupancy`, unlike its other two lookup branches — meaning tag-resolved locations (like the Manager's home `@admin` desk) bypassed occupancy/queue resolution entirely.
- `Tests/JobLoopTest.cs` — second full integration test: an unemployed citizen applies for the one vacancy in town, crosses a map warp to interview for it, and gets hired, exercising `JobSystem`/`PathfindingSystem`/`StateSystem`/`MemorySystem` together end-to-end
- Citizens pay rent on the 1st of each in-game month
- Eviction if unable to pay — citizen becomes homeless, sleep quality drops, energy decay accelerates
- Landlord entities collect rent, accumulate wealth
- Can be fired from a job for low performance
- Social interaction based on proximity

---

## 🔨 Milestone 1 — Citizens Feel Alive

**Needs System (remaining)**
- Social interrupt moved to Milestone 4 (tied to social proximity/relationships)

Milestone 1 closed out otherwise — see ✅ Complete.

---

## 🔨 Milestone 2 — World Has a Pulse

Milestone 2 closed out — see ✅ Complete.

---

## 🔨 Milestone 3 — Economy Emerges

**Shops & Commerce**
- Marker components doubling as UI status icons (hungry / seeking food / eating, etc.) — no UI yet, deferred
- Shops have `InventoryComponent` — stock levels, restock triggers
- Citizens spend money based on needs beyond hunger (low mood → park/bar) — waits on Mood (Milestone 4)
- Price elasticity seed: expensive shops drain wallets faster, citizens seek cheaper alternatives

---

## 🔨 Milestone 4 — Society Complexity

**Multiple Citizens**
- Spawn 10–20 citizens from `CitizenConfig` at scene load — or reconstructed from a save file, via the Save/Load system above
- Varied body/hair/clothing/face variants
- Staggered schedules (different wake/work/eat/sleep times)
- Names, randomised personality traits influencing decay rates

**Mood & Wellbeing**
- `MoodComponent` — composite score from needs + recent events
- Low mood affects productivity (work output reduced), social willingness, spending habits
- High mood unlocks leisure activities (park visits, socialising)

**Social Proximity**
- Social interrupt — `Social < MinSocialNeed` cancels current schedule, sends citizen to nearest park/leisure spot to seek out the proximity interactions above

**Relationships**
- Citizens who interact frequently build `RelationshipComponent` links
- Friends visit each other's homes, sit together at restaurants
- Relationships decay without contact
- Quitting — broader than firing's shift-end skill/mood check (Milestone 3); needs a citizen's financial trend (wallet trajectory, not just balance), manager sentiment (requires a manager-as-entity relationship link), and longer-term goals/aspirations to weigh against staying. Waits on `RelationshipComponent`/manager links above plus `MoodComponent` (this milestone) existing first

**Reputation & Businesses**
- Businesses accumulate a reputation score based on customer satisfaction
- High reputation → more customers → more revenue → owner wealth grows
- Low reputation → fewer customers → eventual closure

**Crime & Poverty Loop**
- Citizens below a wealth threshold have a rising chance of petty crime
- Crime reduces area property values, increases stress on nearby citizens
- Police/law enforcement as a city service (budget-funded)

---

## 🔨 Milestone 5 — Connected City

**Internet / Telephones**
- General "do things remotely" abstraction — a citizen at home (desk/sofa) can trigger actions that would otherwise require physically travelling somewhere
- First concrete use case has shipped narrowly, not generally: Manager job interviews resolve via `LocationRegistry.Resolve("/{homeMapID}/@admin")` straight from `JobSystem` (see ✅ Complete — hiring loop) rather than through any shared "remote action" abstraction
- Likely later use cases once a second one justifies generalising: ordering food/goods for delivery, checking a shop's stock remotely before travelling there, talking with friends/family without a `SocialInteractionComponent` proximity requirement

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
