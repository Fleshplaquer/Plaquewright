# Plaquewright – CURRENT CHAT HANDOFF

**Stand:** 18. September 2026  
**Aktueller Funktionsnachweis:** `3ee638a` – `Prove PW-S06 timed modifier continuation replay`  
**Repository vor dieser Dokumentaktualisierung:** `main` synchron mit `origin/main`, Working Tree clean (`gcc`)  
**Letzter bestätigter Teststand:** 1267/1267 Tests; vollständiger Regressionstest und Build grün  
**Status:** PW-S02 bis **PW-S06 abgenommen**; nächster regulärer Strang PW-S07

**Zweck:** Kurzlebige Arbeitsübergabe an den nächsten Chat. Diese Datei bei jedem Chatwechsel **ersetzen/aktualisieren**, nicht als historische Chronik aufblasen.  
**Autorität:** `docs/architecture/v2/` definiert Architektur und Abnahmegrenzen. Der Dokumentationscommit nach `3ee638a` darf einen eigenen HEAD-Hash erhalten; `3ee638a` bleibt der Funktionsnachweis für PW-S06.

## 1. Startanweisung für den nächsten Chat

Arbeite am Projekt **Plaquewright** weiter. Es ist ein engine-unabhängiges, deterministisches, modulares Gameplay-/Simulations-Framework in C#/.NET 8; Godot ist erster Host/Adapter, nicht Gameplay-Autorität. Lies zuerst `README.md`, dann diesen Handoff und bei Architekturfragen die passenden v2-Dokumente.

Nicht alte A/B-Findings oder bereits abgenommene S02–S06-Grundlagen wieder aufrollen, solange kein konkreter neuer Codebeleg dafür vorliegt. Keine große Generalisierung, Serializer-/Savegame-Schicht, EventStore-, Timer-, Status- oder Cache-Plattform ohne aktuellen Bedarf einführen. Änderungen in kleinen, testbaren Blöcken durchführen.

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
- PW-S05: `d8826a2` – in-memory Snapshot/Restore + expliziter Pending-Work-Codec + Runtime-/Session-Fortsetzung + deterministischer Replay-Beweis. 1248 Tests, `gcc`.
- PW-S06: `3ee638a` – zeitabhängiger Stats-State, StateBoundary-Ablauf, Derived Query, revision-basierter Cache, Domain-Snapshot/Restore und pending-Expiration-Fortsetzung. 1267/1267, `gcc`.

## 4. PW-S06 – endgültiger technischer Stand

Technische Folge:

```text
238a24a  Add PW-S06 timed modifier state and boundary proof
0739c31  Add PW-S06 derived query cache invalidation
982798e  Add PW-S06 timed modifier snapshot restore
3ee638a  Prove PW-S06 timed modifier continuation replay
```

Abgenommen ist der kleine **zeitabhängige Stats-/Derived-Query-Referenzumfang**:

1. `ModifierKind` bündelt die bereits vorhandenen Modifier-Familien, ohne eine zweite Rechenlogik einzuführen.
2. `TimedModifierStateSet` besitzt den autoritativen zeitabhängigen Zustand. `Revision` ändert sich nur bei echter State-Mutation.
3. `TimedModifierKey` identifiziert einen Slot; dessen `Generation` identifiziert die aktuelle Lebensdauer. Refresh erhöht die Generation; Cancel lässt den inaktiven Slot samt Generation erhalten.
4. `TimedModifierExpiration` trägt Key, Generation und `ExpiresAt`. Bereits geplante alte Expiration-Arbeit darf im Scheduler verbleiben und wird domainseitig als stale erkannt.
5. Ablauf wird für den Referenzfall bei `ExpiresAt` in `SchedulerPhase.StateBoundary` eingeplant. Damit ist der Modifier vor `Execution` desselben Timestamps entfernt.
6. Zero-duration wird im ersten Profil abgewiesen; damit wird kein Same-Time-Child als scheinbare Vorgrenze missverstanden.
7. `TimedModifierValueQuery` ist read-only und verwendet die vorhandene `ModifierAccumulator`-/`ModifierMath`-Semantik als Referenzrechnung.
8. `TimedModifierValueQueryCache` cached nur für dieselbe State-Instanz, dieselbe Revision und denselben Query-Input. Apply/Refresh/Cancel/Expire invalidieren durch Revision; stale Expiration tut dies nicht.
9. `TimedModifierStateSetSnapshot` erhält aktive **und inaktive** Slots, Generationen, Revision, Modifierdaten und Expiration-Zeit. Dadurch kollidiert eine alte Expiration nach Restore nicht mit einer neu gestarteten Generation.
10. Restore rekonstruiert den State direkt und ruft nicht `ApplyOrRefresh(...)` nach; Revision-/Generation-Exhaustion bleibt erhalten.
11. Der Abschlussbeweis kombiniert den Domain-Snapshot mit `SimulationRunnerSnapshot<TPayloadSnapshot>` und einer **test-only** expliziten Work-Item-Snapshotgrenze. Es wurde kein universeller Serializer eingeführt.
12. Der Restore baut eine frische `SimulationComposition` gegen den restaurierten Stats-State. Runtime A wird vor B fortgesetzt, damit versehentliche alte State-/Handlerbindungen auffallen.
13. Ein Refresh-Fall hält zwei pending Expirations im Scheduler: Generation 1 wird bei t=110 stale ohne Revisionserhöhung; Generation 2 läuft bei t=150 am `StateBoundary` ab; Same-Time-Execution sieht danach den Basiswert.
14. PW-QA-20 ist für diesen S06-Referenzumfang abgenommen.

Wichtige Grenze: Die Stats-Domain wurde **nicht** vorsorglich in `SimulationRuntimeState` eingebaut. Domain-State und Composition bleiben getrennt; der S06-Beweis zeigt, dass ein unabhängiger Domain-Snapshot gemeinsam mit Runner-Pending-Work fortgesetzt werden kann.

## 5. Bewusst weiterhin offen

- kein JSON-/Binary-Saveformat
- kein universeller Event Store oder Object-Graph-Serializer
- keine universelle polymorphe Persistenz aller `ISimulationWorkItem`-Typen
- keine Cross-Version-/Ruleset-Migration
- keine vollständige Replay-History-/Ledger-Retention vor einem Snapshot
- noch kein Replay echter Physics-/Collision-/anderer Host Facts
- noch kein Nachweis der Unterdrückung realer Netzwerk-/Kauf-/Host-Side-Effects im Replay
- keine Cross-Platform-/Cross-Engine-Bitgleichheitsfreigabe
- kein allgemeines Status-/Buff-/Debuff-System
- keine globale Timer- oder Scheduler-Cancellation-Registry
- kein allgemeiner Query-/Cache-Graph
- kein globaler `CombatService`
- kein Service Locator / Reflection-Discovery
- noch kein abgenommenes Performance-/Retention-Profil

PW-QA-17, PW-QA-18 und PW-QA-28 bleiben für den S05-Core-Scope abgenommen. PW-QA-20 ist für S06 abgenommen. PW-QA-19 und PW-QA-27 bleiben für reale Side-Effects beziehungsweise echte externe Host Facts offen.

## 6. Unmittelbar nächster Arbeitsschritt – PW-S07

PW-S07 = **definierte Lastprofile, Retention und gemessene Optimierung**.

Nicht zuerst optimieren. Zuerst einen reproduzierbaren Referenzpfad und Messvertrag festlegen:

1. konkrete Lastform auswählen, z. B. viele unabhängige Entity-/Status-/Scheduler-Aktivitäten oder ein vorhandenes Combat-AoE-Profil;
2. Hardware, .NET-Runtime, Buildmodus, Seed, Definitionen und Aktivitätsraten dokumentieren;
3. Referenzsemantik und beobachtbare Ergebnisse festhalten, bevor ein Fast Path entsteht;
4. Laufzeitverteilung, Allokationen/GC sowie Queue-/Ledger-/Trace-Wachstum messen;
5. Retention-Grenzen explizit benennen statt unbegrenzte History vorauszusetzen;
6. erst danach einen konkreten Hot Path optimieren und gegen den Referenzpfad vergleichen.

Vor dem ersten S07-Code aktuellen Benchmark-/Testbestand und vorhandene Trace-/Ledger-/Queue-Zugänge lesen. Kein Benchmark-Framework oder Cache-/Batch-Unterbau vorsorglich einführen, wenn ein kleiner vorhandener Testharness genügt.

## 7. Dokument-/Git-Pflege beim nächsten Chatwechsel

Diese Datei wieder **überschreiben**. Mindestinhalt:

- aktueller Funktions-HEAD und `git status`
- letzte bestätigte Testzahl
- letzter vollständig abgenommener PW-Schritt
- aktuell laufender kleiner Block
- neu eingeführte Verträge/Dateien
- bekannte Scope-Grenzen/Stolperfallen

Die stabilen v2-Dokumente wurden nach PW-S06 auf den Funktionsstand `3ee638a` aktualisiert. Ein danach entstehender reiner Dokumentationscommit ändert diesen Funktionsnachweis nicht.
