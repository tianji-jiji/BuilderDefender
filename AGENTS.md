# AGENTS.md

## Overall Goal

The overall goal is to write clean and well-organized code.

The main principles are readability and extensibility.

All code should prioritize:

* Readability.
* Extensibility.
* Clear responsibility boundaries.
* Low coupling between systems.
* Safe refactoring as the project grows.

## Git Operation Rules

* Do not perform any Git state-changing operation without explicit permission from the user.
* Git state-changing operations include, but are not limited to: creating branches, switching branches, deleting branches, staging files, committing, merging, rebasing, resetting, stashing, pushing, opening pull requests, and modifying Git history.
* Before performing any Git state-changing operation, ask the user and wait for clear approval or a clear direct instruction.
* Read-only Git inspection commands are allowed only when they are necessary for the current task.

## Project Context

The project is a 2D tower defense / RTS-style game. Core systems include building placement, resource collection, enemy waves, towers, projectiles, rewards, card effects, UI tooltips, floating popups, object pooling, and game progression.

## Unity Version and Compatibility Requirements

* Target Unity version: Unity 6.3 LTS.
* Unity API usage must match the current project Editor version; do not use obsolete or deprecated Unity APIs.

## C# Naming Conventions

* Event names must consistently use the `OnXXX` format to indicate actions that have already happened, such as `OnHealthChanged` and `OnItemSelected`.
* Types, methods, properties, and events must use PascalCase.
* Constants must use UPPER_SNAKE_CASE.
* All static variables, whether private or public, must use PascalCase.
* Local variables and method parameters must use camelCase.
* Private non-serialized fields must start with an underscore followed by camelCase, using the `_camelCase` format.
* Private serialized fields and public readonly fields must use camelCase.
* Private readonly fields must start with an underscore followed by camelCase, using the `_camelCase` format.
* Collection types, including `List`, `Dictionary`, and their generic variants, must use variable names ending with `List` or `Dic`.
* When the object type is clear from the left side, use target-typed `new()`.

## C# Code Structure and Design Requirements

* Fields that need to be configured in the Unity Inspector should preferably use `[SerializeField] private` instead of public fields.
* Each `MonoBehaviour` script must have a single responsibility.
* Do not write large methods with mixed responsibilities.
* Each method should be responsible for one clearly defined function.
* Try to keep each method within 50 lines.
* The nesting level inside a method should be no more than two levels.
* UI, gameplay logic, visual effects, audio, and data calculation should be decoupled from each other.
* Manager classes should only coordinate different systems and should not take over all detailed logic.
* Prefer composition over inheritance.
* Do not use magic numbers.
* In Unity scripts, lifecycle methods should be placed before custom methods, and their preferred order is `Awake`, `OnEnable`, `OnDisable`, `Start`, `Update`, `FixedUpdate`.

## Refactoring Rules

* Do not keep unused compatibility wrappers, fallback code, or legacy indirection unless there is a confirmed active dependency.
* When obsolete production code is only referenced by tests, update the tests to the current API and remove the obsolete production code.
* Avoid preserving code only because it might be useful in the future.
* If compatibility code must be kept, clearly explain the active dependency and the condition for removing it later.

## Chinese Comments and Documentation Standards

* Add Chinese XML documentation comments using `///` at the top of C# scripts to describe the class function and purpose.
* Add Chinese XML documentation comments using `///` for enum types to describe the enum meaning.
* Add a single-line Chinese comment using `//` for all custom methods to describe the method purpose.
* Unity lifecycle methods do not need extra comments.
* Add Chinese comments for event and delegate definitions to explain their purpose.
* Chinese comments must use UTF-8 encoding to avoid garbled text.

## Unity Folder Rules

* Runtime code must be placed under the `Assets/Scripts` directory.
* Editor-only scripts must be placed inside an `Editor` folder.
* Material files must be placed under `Assets/Materials`.
* Prefabs must be placed under `Assets/Prefabs`.
* Scene files must be placed under `Assets/Scenes`.
* When the project becomes larger, organize folders by feature module.
* Do not put everything into broad folders such as managers, systems, or utilities.

## Unity Component Reference Rules

- Do not call expensive methods such as `GetComponentInChildren`, `GetComponent`, `FindObjectOfType`, or `GameObject.Find` inside `Update`, `FixedUpdate`, `LateUpdate`, high-frequency event callbacks, damage detection, pathfinding, enemy spawning, or other high-frequency logic.
- The correct approach is to cache component references in `Awake`, or assign references directly in the Inspector.
