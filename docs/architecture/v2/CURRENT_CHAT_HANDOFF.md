# Plaquewright – CURRENT CHAT HANDOFF

**Stand:** 18. September 2026
**Zweck:** Kurzlebige Arbeitsübergabe an den nächsten Chat. Diese Datei bei jedem Chatwechsel **ersetzen/aktualisieren**, nicht als historische Chronik aufblasen.
**Autorität:** `docs/architecture/v2/` definiert Architektur und Abnahmegrenzen. Diese Datei beschreibt nur den aktuellsten Arbeitsstand zwischen zwei Chats. Aktueller Source-Code und tatsächlich gezeigte Test-/Git-Ausgaben schlagen veraltete Statusangaben in dieser Datei.

## 1. Startanweisung für den nächsten Chat

Arbeite am Projekt **Plaquewright** weiter. Es ist ein engine-unabhängiges, deterministisches, modulares Gameplay-/Simulations-Framework in C#/.NET 8; Godot ist erster Host/Adapter, nicht Gameplay-Autorität. Lies zuerst `README.md`, dann diesen Handoff und bei Architekturfragen die passenden v2-Dokumente.

Nicht alte A/B-Findings wieder aufrollen, solange kein konkreter neuer Codebeleg dafür vorliegt. Keine große Generalisierung oder neue Serializer-/Savegame-Schicht ohne aktuellen Bedarf einführen. Änderungen in kleinen, testbaren Blöcken durchführen.

Arbeitsstil mit dem Nutzer:
- Deutsch, direkt, kompakt.
- Windows PowerShell-Befehle.
- Codeänderungen zuerst dateiweise im Chat, danach Test-/Git-Befehle.
- Kürzel: `g` = grün, `c` = committed, zweites `c` = clean, `gcc` = grün + committed + clean.
- Nicht automatisch ZIP/Patch für Code erzeugen, außer der Nutzer verlangt es.

## 2. Verbindliches Architektur-Kurzbild

> Kernel coordinates; Domains own State; Gameplay resolves rules; Transactions mutate state; Events describe results; Queries read state; Modules extend via contracts.

Kommunikation:
- Query = read-only.
- Transaction = autoritative Mutation/Atomizität.
- Domain Event = Fakt nach erfolgreichem Commit.
- Reaction = neue Arbeit nach Commit, niemals rückwirkende Änderung des alten Commits.
- Effect != Domain Change.
- Nicht jede Action ist eine Transaction.

Determinismusvertrag:

`Same authoritative start state + same ordered inputs/host facts + same rules/module versions + same relevant RNG/ID/scheduler state => same authoritative result.`

Keine unkontrollierte Wall-Clock-Zeit, Randomness, Render-FPS- oder ungeordnete Gameplay-Iteration. Engine-Physics bleibt extern; gameplayrelevante Resultate werden als Host Facts/Provider-Ergebnisse eingebracht.

## 3. Historische stabile Nachweise

- A01–A07 und B01–B10: historisch abgeschlossen; Baseline `d39cb54`.
- PW-S02: `b04fcbe` – Domain Event/Reaction, sichere Follow-up-Reservation, D-04 Input Closure, D-05 Publication/Fault, RunNext/RunToCompletion-Beweis.
- PW-S03: `f90517a` – Execution Plan, explizite Module/Contracts, `SimulationSession`, Headless + echter Godot-Smoke. 1210/1210 im S03-Referenzstand.
- PW-S04: `8bd4dd0` – Combat als Referenzruleset, `ApplyResolvedDamageAction`, `DamageCommittedEvent`, PreDefeat vor Commit, result-backed Damage-Commit, Paid Cost->Damage Pipeline. 1215/1215, `gcc`.

## 4. Aktueller PW-S05-Stand

PW-S05 = **Snapshot/Restore + deterministischer Replay-Beweis**. Noch nicht komplett abgenommen.

Aktuell bestätigt:
- **1239/1239 Tests grün**.
- Danach vom Nutzer **`gcc`** bestätigt.
- Der konkrete Commit-Hash dieses 1239er S05-Zwischenstands wurde im vorherigen Chat nach dem Commit nicht mehr festgehalten. Beim nächsten sinnvollen Git-Abgleich `git log -1 --oneline` erfassen und in die Doku nachtragen; nicht raten.

Bereits umgesetzt:
1. `ResourceStateSnapshot`: Current/Maximum/Revision; Restore läuft unabhängig weiter. `ulong.MaxValue` als terminale Revision bleibt gültig.
2. `ResourceStateSetSnapshot`: IDs/State/Revisions, sortierter Restore, keine Constructor-Ambiguität.
3. `EntityRuntimeStateSnapshot` und `EntityRuntimeStateSetSnapshot`.
4. Snapshots/Restore für `EntityIdAllocator`, `ExecutionIdAllocator`, `HitExecutionIdAllocator`, `DamageExecutionIdAllocator`, inklusive ihrer unterschiedlichen Exhaustion-Semantik.
5. `SimulationRuntimeStateSnapshot`: RootSeed + Entities + Allocatorstände. Restore injiziert neue State-Objekte und **immer eine neue `SimulationRuntimeIdentity`**. Alte Runtime-Ownership darf nicht über Restore gültig werden.
6. `SimulationSchedulerSnapshot<TPayloadSnapshot>` + `ScheduledEventSnapshot<TPayloadSnapshot>`: Limits, Pending Events, originale `ScheduledEventKey`s, Sequence-Fortsetzung. Restore darf Pending Work **nicht** erneut via `Schedule(...)` einfügen, sonst ändern sich Sequences.
7. Scheduler Snapshot Boundary: Capture verboten bei `_activeEventKey != null` oder `_reservedQueueSlots != 0`. Prepared Follow-ups werden nicht serialisiert.
8. `SimulationRunnerSnapshot<TPayloadSnapshot>`: `CurrentTime`, `ProcessedEvents`, `MaxProcessedEvents`, `_externalInputsClosedThrough` + Scheduler-Snapshot. Faulted oder terminal-budgeted Runtime im ersten Profil nicht snapshot-fähig.
9. D-04 wird über Restore erhalten: bereits begonnenes T bleibt für neue externe Inputs geschlossen.
10. Persistenter RNG-State ist im aktuellen Core **nicht separat nötig**: es gibt keine langlebig gespeicherten RNG-Instanzen; Streams werden deterministisch aus Seed/Kontext erzeugt. Falls spätere Domains einen persistenten RNG halten, müssen sie ihn snapshotten.
11. `DamageResolutionQuantitiesSnapshot`, `DamageResolutionSnapshot`, `ApplyResolvedDamageActionSnapshot`: bereits resolved Damage-Mengen und stabile IDs werden beschrieben; Live-Kontexte werden gegen Runtime B neu gebunden. Keine erneute Combat-Resolution und keine erneute Execution-/Hit-/Damage-ID-Allokation.
12. Restore eines pending Damage-WorkItems validiert, dass referenzierte Entities in Runtime B existieren.
13. Integrationstest: pending `ApplyResolvedDamageAction` in Scheduler A -> Runtime+Runner Snapshot -> Runtime B + Runner Restore -> ausführen. Direkte Fortsetzung A und restored Fortsetzung B stimmen im geprüften Scope bei Scheduler-Key/Trace, Resource-Current/Revision und Ledger-Ergebnis/Provenienz überein.

Wichtige Leitentscheidung:

> Snapshot != Object Graph Clone. Snapshot = describable authoritative state from which a new runtime can be reconstructed.

`CompiledResourceRegistry` wird im ersten in-memory Proof als kompatible immutable Definition von außen an Restore übergeben. Noch kein Ruleset-/Definition-Migrationsvertrag.

## 5. Unmittelbar nächster Arbeitsschritt

Nicht wieder S05-Grundlagen bauen. Als Nächstes:

1. Pending-Work-Snapshot über den einzelnen `ApplyResolvedDamageActionSnapshot`-Fall hinaus generalisieren.
2. Dafür eine kleine explizite Codec/Envelope-Grenze für mehrere `ISimulationWorkItem`-Typen entwerfen; **kein** Reflection-Service-Locator und noch kein JSON-Serializer.
3. Darauf einen höheren Runtime-/Session-Snapshot setzen, der `SimulationRuntimeStateSnapshot` und `SimulationRunnerSnapshot<...>` kohärent zusammenführt.
4. Danach einen echten Replay-Beweis: Snapshot -> weitere geordnete Inputs/ggf. Host Facts -> Runtime A vs. restaurierte Runtime B -> State + relevante History/Trace vergleichen.
5. Erst wenn dieser Scope grün ist, PW-S05-Doku/Freigabe endgültig abschließen.

Vor Produktionscode zunächst die aktuellen Work-Item-Typen/Execution-Plan-Grenzen prüfen, damit die Codec-Abstraktion nicht vorschnell zu breit wird.

## 6. Dinge, die ausdrücklich nicht vorschnell gebaut werden

- kein JSON-/Binary-Saveformat als erster Schritt
- kein universeller Event Store
- kein Object-Graph-Serializer
- kein globaler `CombatService`
- kein Service Locator / Reflection-Discovery
- kein Cross-Version-Versprechen ohne Versionierungsnachweis
- keine Wiederverwendung alter `RuntimeIdentity` oder runtime-gebundener Objektinstanzen nach Restore
- Combat-Abstraktionen nicht in den Kernel verschieben, solange kein zweiter unabhängiger Domain-Fall denselben Vertrag beweist

## 7. Dokument-/Git-Pflege bei nächstem Chatwechsel

Diese Datei soll beim nächsten Chatwechsel wieder **überschrieben** werden. Mindestinhalt:

- aktueller HEAD-Hash und `git status`-Zustand
- letzte bestätigte Testzahl
- welcher PW-Schritt vollständig abgenommen / in Arbeit ist
- seit letzter v2-Doku neu entschiedene Architekturdetails
- aktuell implementierte Dateien/Verträge, die der nächste Chat kennen muss
- offener nächster kleiner Block
- bekannte Stolperfallen / bewusst nicht gebaute Dinge

Wenn der laufende Schritt abgeschlossen wurde, zusätzlich die stabilen v2-Dokumente aktualisieren. Der Handoff ersetzt diese Dokumente nicht.
