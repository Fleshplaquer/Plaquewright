# Plaquewright – Code Classification 2.0

**Datum:** 17. September 2026 · **Status:** Freigegebene v2.0-Einordnung / statische Momentaufnahme  
**Historische Audit-Baseline:** `d39cb54`; **aktueller PW-S02-Nachweis:** `b04fcbe` laut Nutzer; **vollständig direkt untersuchte Source-Basis:** älterer Archiv-Snapshot `zip.7z` plus im PW-S02-Durchgang gezeigte/erarbeitete betroffene Test- und Vertragsteile

Die in der Architekturübergabe genannte alte `CODE_CLASSIFICATION.md` ist im bereitgestellten Archiv nicht enthalten. Diese neue Klassifikation verwendet tatsächliche Quellpfade, behauptet aber keinen vollständigen aktuellen Dependency-Audit.

## 1. Einordnung nach Verantwortung, nicht nach Verzeichnis

| Vorhandener Bereich / Typ | Einordnung im Zielbild | Konsequenz für die nächste Arbeit |
|---|---|---|
| `SimulationTime`, `SimulationDuration` | Generische Simulationszeit | Beibehalten |
| `SimulationScheduler<TPayload>`, `SimulationRunner<TPayload>`, Keys, Waves, Budgets | Generische Ablauf-Infrastruktur | Beibehalten; PW-S02 ergänzt reservierbare Follow-ups und die geschlossene Same-Timestamp-Input-Grenze |
| `DeterministicRng`, Factory, Domains, StableHash64 | Generische deterministische Infrastruktur | Beibehalten; unterstützte Versions-/Umgebungsmatrix getrennt dokumentieren |
| `ExecutionId`, `ExecutionIdAllocator` | Allgemeine kausale Identität als Kernel-Kandidat | Keine neue parallele Skill-ID-Hierarchie ohne Bedarf |
| `EntityId` | Allgemeine Identität als Kernel-Kandidat | Der Standort im Entities-Ordner beweist keine notwendige Resource-Kopplung |
| `GameplayExecutionContext` | Allgemeiner Ausführungskontext mit konkretem Source-Entity-Bezug | Für entitylose Actions nicht blind zur universellen Pflicht machen |
| `HitExecutionIdAllocator`, `DamageExecutionIdAllocator` | Combat-spezifische Identitätsinfrastruktur | Nicht allein wegen `Simulation/` in den Kernel einordnen |
| `SimulationRuntimeState` | Vorhandene Gameplay-Komposition aus Entities, Resources und Combat | Als konkrete Komposition erhalten; generische Module nicht davon abhängig machen |
| `ITransactionParticipant`, `PreparedTransactionChange`, `TransactionCoordinator` | Generische Cross-Domain-Grenze | Weiterverwenden, neue gemeinsame Konflikte/Lebensdauern gezielt belegen |
| `IDomainEvent`, `IDomainReaction<,>`, `DomainReactionContext<>`, `DomainReactionDispatcher<,>` | Kleine generische Post-Commit-Folgearbeitsgrenze | PW-S02 abgenommen; explizite Komposition statt globalem EventBus beibehalten |
| `DomainEventTransactionCoordinator` | Komposition aus sicher reserviertem Follow-up und bestehendem Transaction-Commit | Für Commit→Event-Fälle nutzen; `TransactionCoordinator` selbst eventfrei halten |
| `PreparedScheduledFollowUp<TPayload>` | Interne Scheduler-Publikationsreservation | Intern halten; keine allgemeine externe Queue-Reservation-API daraus machen |
| `EntityRuntimeState`, `EntityRuntimeStateSet` | Bestehende Entity-/Resource-Komposition | Nicht sofort durch ECS ersetzen; Nutzung ohne Resources später konkret nachweisen |
| `Resources/*` | Domain und ihre eigenen Mutations-/Buchungsmechanismen | Draft und Commit-Interna nicht pauschal veröffentlichen |
| `ResourceCostTransactionParticipant` | Öffentlicher Resources-Adapter zur generischen Transaction | Bestehende Cost-Grenze nutzen; nicht als universellen Resource-Participant ausgeben |
| `Combat/*` | Gameplay-Domain mit konkreten fachlichen Regeln | Weiterverwenden; Kernel und künftig wählbare Regelprofile unterscheiden |
| `Stats/ModifierMath`, `ModifierAccumulator` | Rechenbausteine der Stats-/Modifier-Domain | Kein Beleg eines bereits vollständigen Stat-Graph-/Cache-Systems |
| `Tags/*`, `Conditions/*` | Klassifikation und Auswertung, mit bestehender Kopplung | Nur bei einer benötigten Grenze trennen, nicht aus Ordnerästhetik |
| `Numerics/NumericComparison` | Geteilte numerische Hilfsfunktion | Nicht mit einem universellen plattformübergreifenden Numerikvertrag verwechseln |
| `src/Presentation` | Host-/Präsentationsbereich | Kein autoritativer Neben-Core |

## 2. Sichtbare Transaction-Rechte im Archiv

`PreparedTransactionChange` ist öffentlich und abstrakt; `Apply()` ist internal; `ApplyCore()` ist protected abstract. Ein externer Participant kann daher eine vorbereitete eigene Änderung liefern, aber das Framework-Apply nicht regulär direkt aufrufen.

`ResourceTransactionCommitter` und `PreparedResourceTransactionCommit` sind internal. Der konkrete Prepared-Typ innerhalb `ResourceCostTransactionParticipant` ist private. Der Participant verwendet die bestehende Resource-Prepare-/Apply-Mechanik. [E3]

Diese Grenzen sichern die vorgesehene API, nicht beliebigen Drittcode. Sie beweisen insbesondere nicht allein, dass zwei Participants mit überlappenden Schreibzielen, wiederverwendeten Objekten oder später veränderten Previews in jeder Zusammensetzung sicher sind.

## 3. Neuere Änderungen aus dem Gespräch

B07 hat die explizite Execution-Provenienz für die besprochenen Pfade ergänzt; B08 den externen Scheduler-Zugang beschränkt; B09 zwei Testverträge präzisiert; B10 External-API-Nachweise ergänzt. Diese Änderungen sind durch Gespräch und Nutzerbestätigungen Teil des Anschlussstands, aber nicht alle in dem älteren Archiv enthalten. [E2]

Der gezeigte B08-Commit legte `SimulationSchedulerInputBoundaryTests.cs` unter `Plaquewright.Core.Tests` an, obwohl der Vorschlag ursprünglich `ExternalTests` genannt hatte. Der Reflection-Test auf öffentliche Methoden kann dort weiterhin sinnvoll sein; daraus wird aber kein Nicht-Friend-Kompilierungsnachweis abgeleitet. Eine neue externe Integrationsabnahme muss die tatsächliche Test-Assembly verwenden. [E4]

PW-S02 ergänzt auf dem vom Nutzer bestätigten Stand `b04fcbe` die typisierte Domain-Event-/Reaction-Grenze, sichere Prepared-Follow-up-Reservation vor Commit, explizite Reaction-Reihenfolge, Same-Timestamp-Input-Schluss und Fail-stop bei unerwarteten Reaction-Fehlern. Der Door-/Alarm-Fall bleibt in `Core.ExternalTests` der öffentliche Nicht-Combat-Nachweis. [E7]

## 4. Abgenommener kleiner Ausbau, weiterhin bewusst begrenzt

PW-S02 belegt eine **kleine typisierte Domain-Event-/Reaction-Pipeline**, aber keinen universellen EventBus oder ein fertiges Modul-Lifecycle-System. Ebenso sind generische Snapshot-/Restore-Infrastruktur und Replay-Scrubber weiterhin nicht nachgewiesen.

Aus dem S02-Nachweis folgt insbesondere noch keine automatische Reaction-Discovery, keine dynamische Prioritätsregistrierung, kein Hotloading, keine zweite generische Alarm-Transaction und keine Aussage über Combat-Migration. Diese Grenzen werden erst bei PW-S03/PW-S04 aus realem Bedarf erweitert.

Vor neuen Typen wird weiterhin der konkrete aktuelle Stand geprüft.

## 5. Umgang mit Tests

`Core.Tests` darf für interne Invarianten Friend-Zugriff besitzen. `Core.ExternalTests` soll Erweiterbarkeit und öffentliche Grenzen ohne diesen Zugriff prüfen. Beide Testarten sind nötig und ersetzen einander nicht.

Methodennamen wie `...Preserves...` oder `...IsRejected...` geben Hinweise, beweisen aber ohne Testkörper nicht, was genau unverändert bleibt. Neue Matrixeinträge nennen deshalb Setup, Assertion und nachgewiesene Grenze.

**Quellen / Snapshotgrenzen:** [Quellenregister](SOURCE_EVIDENCE_v2_0.md).  
**Nächste Eingriffe:** [Migrationsplan](MIGRATION_PLAN_v2_0.md).
