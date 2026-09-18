# Plaquewright – CURRENT CHAT HANDOFF

**Stand:** 18. September 2026
**Aktueller HEAD:** `d8826a2` – `Prove PW-S05 deterministic replay continuation`
**Repository:** `main` synchron mit `origin/main`, Working Tree clean (`gcc`)
**Letzter bestätigter Teststand:** 1248 Tests; vollständiger Regressionstest und Build grün
**Status:** PW-S02, PW-S03, PW-S04 und **PW-S05 abgenommen**; nächster regulärer Strang PW-S06

**Zweck:** Kurzlebige Arbeitsübergabe an den nächsten Chat. Diese Datei bei jedem Chatwechsel **ersetzen/aktualisieren**, nicht als historische Chronik aufblasen.
**Autorität:** `docs/architecture/v2/` definiert Architektur und Abnahmegrenzen. Aktueller Source-Code und tatsächlich gezeigte Test-/Git-Ausgaben schlagen veraltete Statusangaben in dieser Datei.

## 1. Startanweisung für den nächsten Chat

Arbeite am Projekt **Plaquewright** weiter. Es ist ein engine-unabhängiges, deterministisches, modulares Gameplay-/Simulations-Framework in C#/.NET 8; Godot ist erster Host/Adapter, nicht Gameplay-Autorität. Lies zuerst `README.md`, dann diesen Handoff und bei Architekturfragen die passenden v2-Dokumente.

Nicht alte A/B-Findings oder PW-S05-Grundlagen wieder aufrollen, solange kein konkreter neuer Codebeleg dafür vorliegt. Keine große Generalisierung, Serializer-/Savegame-Schicht, EventStore- oder Cache-Plattform ohne aktuellen Bedarf einführen. Änderungen in kleinen, testbaren Blöcken durchführen.

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

## 3. Stabile Nachweise

- A01–A07 und B01–B10: historisch abgeschlossen; Baseline `d39cb54`.
- PW-S02: `b04fcbe` – Domain Event/Reaction, sichere Follow-up-Reservation, D-04 Input Closure, D-05 Publication/Fault, RunNext/RunToCompletion-Beweis.
- PW-S03: `f90517a` – Execution Plan, explizite Module/Contracts, `SimulationSession`, Headless + echter Godot-Smoke. 1210/1210 im S03-Referenzstand.
- PW-S04: `8bd4dd0` – Combat als Referenzruleset, `ApplyResolvedDamageAction`, `DamageCommittedEvent`, PreDefeat vor Commit, result-backed Damage-Commit, Paid Cost→Damage Pipeline. 1215/1215, `gcc`.
- PW-S05: `d8826a2` – in-memory Snapshot/Restore + expliziter Pending-Work-Codec + Runtime-/Session-Fortsetzung + deterministischer Replay-Beweis. Final 1248 Tests, vollständiger Build grün, `gcc`.

## 4. PW-S05 – endgültiger technischer Stand

Technische Folge:

```text
653c1f5  Add PW-S05 snapshot restore foundation and pending combat work proof
789763c  Generalize PW-S05 pending combat work snapshots
c6b2327  Add PW-S05 runtime session continuation snapshot
d8826a2  Prove PW-S05 deterministic replay continuation
```

Abgenommen ist der vereinbarte **in-memory Core-Scope**:

1. `ResourceStateSnapshot`, `ResourceStateSetSnapshot`, `EntityRuntimeStateSnapshot`, `EntityRuntimeStateSetSnapshot` erhalten autoritativen Resource-/Entity-State samt Revisionen.
2. Entity-/Execution-/Hit-/Damage-ID-Allocator werden mit ihren jeweiligen Fortsetzungs-/Exhaustion-Semantiken restauriert.
3. `SimulationRuntimeStateSnapshot` erhält RootSeed + Domain-State + Allocatorstände. Restore erzeugt immer eine neue `SimulationRuntimeIdentity`.
4. `SimulationSchedulerSnapshot<TPayloadSnapshot>` / `ScheduledEventSnapshot<TPayloadSnapshot>` erhalten Limits, Pending Events, originale `ScheduledEventKey`s und Sequence-Fortsetzung. Restore schedult Pending Work nicht neu.
5. Snapshot-Capture ist quiescent: verboten bei aktivem Scheduler-Event oder offener Prepared-Follow-up-Reservation.
6. `SimulationRunnerSnapshot<TPayloadSnapshot>` erhält `CurrentTime`, `ProcessedEvents`, `MaxProcessedEvents`, `_externalInputsClosedThrough` und Scheduler-Snapshot. D-04 bleibt nach Restore erhalten.
7. Faulted oder terminal-budgeted Runner gehören nicht zum ersten fortsetzbaren Snapshotprofil.
8. Der aktuelle Core besitzt keinen langlebigen RNG-State. Falls spätere Domains persistenten RNG halten, müssen sie ihn domainseitig snapshotten.
9. `DamageResolutionQuantitiesSnapshot`, `DamageResolutionSnapshot`, `ApplyResolvedDamageActionSnapshot` erhalten bereits resolved Damage und IDs; Restore bindet neue Live-Kontexte an Runtime B, ohne Rule-Resolution oder ID-Allokation zu wiederholen.
10. `DamageCommittedEventSnapshot` beschreibt einen bereits committed Fakt. Restore wiederholt den historischen Damage-/Resource-Commit nicht.
11. `CombatWorkItemSnapshot` + `CombatWorkItemSnapshotCodec` unterstützen im Referenzprofil `ApplyResolvedDamageAction` und `DamageCommittedEvent`. Unbekannte Work Items werden abgewiesen. Kein Reflection-Service-Locator, kein universeller Serializer.
12. `SimulationRuntimeSessionSnapshot<TPayloadSnapshot>` bündelt Runtime- und Runner-Snapshot. Die `SimulationComposition` wird **nicht** gesnapshottet.
13. `SimulationSession<TWorkItem>.Restore(...)` erhält einen restaurierten Runner und eine **neu gegen Runtime B gebaute Composition**. Alte Handler-Closures dürfen nicht weiterverwendet werden.
14. Der finale Replay-Test pausiert nach einem Damage-Commit mit pending `DamageCommittedEvent` und weiterem pending Damage, nimmt einen Snapshot und bringt danach in A und B identische weitere geordnete Inputs ein.
15. Der A/B-Vergleich prüft Runner-Ergebnis, Scheduler-/Trace-Reihenfolge, Event-Fakten inklusive Gameplay-/Damage-/Hit-IDs, Resource-Current/Revision sowie die nach dem Snapshot neu entstehenden Ledger-Einträge/Provenienz.
16. Runtime A wird vor Runtime B vollständig beendet; stale Runtime-/Handler-Bindungen werden dadurch sichtbar.
17. Die Ledger-History **vor** der Snapshot-Grenze ist im aktuellen Profil nicht enthalten. Für den Replay-Nachweis wird die neue History ab der Grenze verglichen.
18. `CompiledResourceRegistry` und kompatible Regeln/Definitionen werden von außen an Restore gegeben. Kein Cross-Version-Migrationsvertrag.

Leitentscheidung:

> Snapshot != Object Graph Clone. Snapshot = describable authoritative state from which a new runtime can be reconstructed.

## 5. Bewusst weiterhin offen

- kein JSON-/Binary-Saveformat
- kein universeller Event Store
- kein Object-Graph-Serializer
- keine universelle polymorphe Persistenz aller `ISimulationWorkItem`-Typen
- keine Cross-Version-/Ruleset-Migration
- keine vollständige Replay-History-/Ledger-Retention vor dem Snapshot
- noch kein Replay echter Physics-/Collision-/anderer Host Facts
- noch kein Nachweis der Unterdrückung realer Netzwerk-/Kauf-/Host-Side-Effects im Replay
- keine Cross-Platform-/Cross-Engine-Bitgleichheitsfreigabe
- kein 30-Sekunden-Analyse-Scrubber
- kein Performance-/Retention-Profil
- kein globaler `CombatService`
- kein Service Locator / Reflection-Discovery

PW-QA-17, PW-QA-18 und PW-QA-28 sind für den aktuellen S05-Core-Scope abgenommen. PW-QA-19 und PW-QA-27 bleiben für reale Side-Effects beziehungsweise echte externe Host Facts offen.

## 6. Unmittelbar nächster Arbeitsschritt – PW-S06

PW-S06 = **zeitabhängige Domain und abgeleitete Queries**.

Nicht sofort ein generisches Status-/Timer-/Cache-System bauen. Zuerst aktuellen Source-Bestand zu Stats/Modifiers/Conditions sowie Scheduler-Zeitverträgen prüfen und dann **ein** kleines Referenzszenario auswählen, das folgende Grenzen real benötigt:

1. Domain-eigener zeitabhängiger autoritativer Zustand.
2. Explizite Ablauf-/Tick-/Zeitgrenze über `SimulationTime`/Scheduler.
3. Typisierte read-only Query beziehungsweise abgeleiteter Wert.
4. Korrekte Invalidierung bei State- und Zeitänderung.
5. Same-Timestamp-/Ablaufordnung ohne Full-World-Scan oder rückwirkende Observer-Reparatur.
6. Referenzvergleich zwischen direkter Berechnung und gegebenenfalls gecachter Sicht.

Vor Produktionscode die vorhandenen Stats-/Modifier-/Condition-Typen und ihre Tests lesen. Keine neue allgemeine Abstraktion einführen, bevor der konkrete Fall sie erzwingt.

## 7. Dokument-/Git-Pflege beim nächsten Chatwechsel

Diese Datei wieder **überschreiben**. Mindestinhalt:

- aktueller HEAD und `git status`
- letzte bestätigte Testzahl
- letzter vollständig abgenommener PW-Schritt
- aktuell laufender kleiner Block
- neu eingeführte Verträge/Dateien
- bekannte Scope-Grenzen/Stolperfallen

Die stabilen v2-Dokumente wurden nach PW-S05 auf den Endstand `d8826a2` aktualisiert. Dieser Handoff ersetzt sie nicht.
