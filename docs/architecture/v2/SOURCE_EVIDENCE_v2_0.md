# Plaquewright – Source Evidence 2.0

**Datum:** 18. September 2026 · **Status:** Quellen-/Evidenzregister zur freigegebenen Architektur 2.0
**Zweck:** Vision, alten Source-Snapshot, neue Nutzerbestätigungen und vorgeschlagene Architektur nicht miteinander verwechseln.

## 1. Quellenhierarchie nach Aussageart

Für das **Produktziel** gilt die neu eingebrachte Architekturübergabe E1: Framework statt einzelnes Idle-Spiel. Für die **historische A/B-Audit-Baseline** gilt E2 mit `d39cb54`; für den **PW-S02-Funktionsnachweis** gilt E7 mit `b04fcbe`; für den **PW-S03-Funktionsnachweis** gilt E8 mit `f90517a`; für PW-S04 gilt E9 mit `8bd4dd0`; für die PW-S05-Grundlage gilt E10 mit `653c1f5`; für den **abgeschlossenen PW-S05-Funktionsnachweis** gilt E11 mit `d8826a2`; für den **abgeschlossenen PW-S06-Funktionsnachweis** gilt E12 mit `3ee638a`. Für den vollständig direkt gelesenen älteren Quellcode gilt der bereitgestellte Archiv-Snapshot E3. Eine ältere Zusammenfassung wird nicht allein durch ihren späteren Upload zum neuesten Code.

Technische Entscheidungen werden nur dort als beschlossen ausgegeben, wo sie aus E1 oder den bestätigten Projektständen hervorgehen. D-03, D-06 und D-07 bleiben offene Detailgates. D-04 und D-05 wurden im PW-S02-Durchgang beschlossen und technisch nachgewiesen. PW-S03 belegt Headless/Godot als gemeinsame Host-Autoritätsgrenze, schließt D-07 als Release-/Distributionsentscheidung aber noch nicht. PW-S04 belegt Combat als zweites fachliches Referenzszenario. E10 und E11 belegen zusammen den vereinbarten PW-S05-in-memory Snapshot-/Replay-Scope. E12 belegt PW-S06 für den zeitabhängigen Stats-/Derived-Query-Referenzumfang einschließlich Cache-Invalidierung und domainlokaler Snapshot-/Continuation-Fortsetzung. Host-Fact-, Cross-Version-, Side-Effect-, Performance- und Plattformfreigaben folgen daraus nicht.

## 2. Projektquellen

### E1 – Neue Architekturübergabe

Datei: `Pasted markdown(1).md`, Überschrift „Plaquewright – Projektübergabe“.

Übernommen: Framework als Produkt, kleiner fachneutraler Kernel, Domain-State-Ownership, Queries/Transactions/Events, engineunabhängiger C#-Core, externe Module ohne Friend-Zugriff, Exact und Approximate, ungefähr 30 Sekunden Replay-/Analyseziel, ungefähr 500 Gegner als Lastziel und schrittweise Migration ohne Rewrite.

Historische Technik darin: 1023 Tests, externer Transaction Coordinator als nächster Schritt, Single-Owner-Defeat als damaliger Stand. Diese Angaben werden nicht ungeprüft über spätere Belege geschrieben.

### E2 – Bestätigter Gesprächsstand bis `d39cb54`

Quelle: Die in diesem Gespräch gezeigte Git-Ausgabe und die Erklärungen `g`, `done`, `gcc`.

Belegt durch Nutzerangaben: `main`, `origin/main` synchron, Working Tree clean; letzte Commits `d39cb54`, `e71fd4b`, `ab292d5`; Build und Tests grün. A01–A07/B01–B10 wurden im Gespräch als erledigt abgeschlossen.

Der B07-Teil besitzt eine explizite Ausgabe mit 1176 bestandenen Tests. Nachfolgende Ergänzungen wurden mit `g` bestätigt. 1189 war die erwartete Endzahl, nicht eine in der finalen Baseline-Ausgabe tatsächlich gezeigte Testsummary.

Nicht behauptet: eigener aktueller Repository-Fetch, eigene Ausführung des vollständigen `d39cb54`, rückwirkend rekonstruierte ausführliche A01–B06-Texte.

### E3 – Quellcodearchiv `zip.7z`

Das Archiv wurde in diesem Dokumentationsdurchgang erneut gelesen. Es enthält die drei v1-Markdown-Dokumente und die geprüften Infrastruktur-/Domain-/Testquellen. Die vorausgegangene Archivprüfung im Gespräch ordnete diesen älteren Snapshot `b944dca` zu; die Byteidentität ist durch den Hash unten festgehalten.

Die später im Chat besprochenen Änderungen wurden für diese Dokumentationsprüfung nicht als vollständiger neuer Source-Snapshot rekonstruiert. Deshalb sind die nachfolgenden Zeilenanker ausdrücklich **Archiv-Zeilen**, nicht garantierte Zeilen von `d39cb54`.

### E4 – Hochgeladene Test-/Git-Auszüge

Die Testnamenlisten zeigen vorhandene Themen und spätere Ergänzungen. Die Datei `Pasted text(20260916-135035).txt` enthält den B08-Commit `ab292d5` und nennt den tatsächlichen Testpfad unter `Plaquewright.Core.Tests`.

Methodennamen sind Suchanker, keine hinreichende Begründung für jede Assertion. Wo konkrete Testkörper aus E3 gelesen wurden, werden sie gesondert genannt. Die spätere Prüfung auf neue API-Grenzen muss den tatsächlichen Nicht-Friend-Kontext benutzen.

### E5 – Historische Idler-Dokumente im Archiv

Dateinamen mit `Plaquewright_`, interne Titel weiterhin Idler:

- `Plaquewright_Manifest_v1_0.md`
- `Plaquewright_Implementation_Sequence_v1_0.md`
- `Plaquewright_Freigabe_und_Testnachweise_v1_0.md`

Diese Dateien wurden zur Ermittlung der abzulösenden Produkt-/Planannahmen und zum Erhalt historischer Fachentscheidungen gelesen. Sie bestimmen nicht die neue Framework-Baufolge.

### Fehlende frühere Architekturdateien

E1 nennt `ARCHITECTURE_MAP.md`, `CODE_CLASSIFICATION.md`, `MIGRATION_PLAN.md`, `PLAQUEWRIGHT_ARCHITECTURE_GLOSSARY.md` und `SOURCE_EVIDENCE.md`.

Unter diesen Namen beziehungsweise Namensbestandteilen wurden im Archiv keine Dateien gefunden. Deshalb sind die gleichnamigen v2-Fassungen hier **neue Rekonstruktionen**, keine behaupteten Updates ungelesener Originaldateien. Es wird nicht behauptet, diese Dateien existierten auf dem Rechner des Nutzers nicht.

## 3. Weitere Entscheidungs- und Primärquellen

Die externen Quellen bestimmen nicht das Projektziel und führen keine neue Engine-, Datenbank- oder Bibliotheksabhängigkeit ein.


**E6 – Projektgespräch, 17. September 2026: Annahme D-01 und D-02.**
Der Nutzer stimmte dem vorgeschlagenen Determinismus-/Replay-Vertrag und der Einordnung des bestehenden Combat als austauschbares Referenz-Regelpaket ausdrücklich zu. Daraus stammen die finalen Formulierungen in PW-10, PW-13, PW-15 und PW-20. Diese Zustimmung ist keine Behauptung, dass Snapshot/Replay oder Cross-Engine-Adapter bereits implementiert sind.

### E7 – PW-S02 Implementierung und Abnahme, 17. September 2026

Quelle: Projektgespräch, gezeigte Testausgaben, bereitgestellter aktueller `CrossModuleTransactionTests.cs`-Ausschnitt und Nutzerbestätigung des Repository-Stands `b04fcbe`.

Im PW-S02-Durchgang wurden die typisierte Domain-Event-/Reaction-Grenze, sichere Follow-up-Reservation vor Commit, explizit geordneter Reaction-Dispatch, Same-Timestamp-Input-Schluss sowie Fail-stop bei unerwarteten Reaction-Exceptions umgesetzt und durch positive und negative Tests geprüft. Der separate PW-QA-12-Nachtrag weist für das Door-/Alarm-Referenzszenario die Äquivalenz zwischen wiederholtem `RunNext` und `RunToCompletion` nach.

Der Nutzer bestätigte die relevanten Test-/Build-Durchgänge mit `g` und die abgeschlossenen Repository-Stände einschließlich des QA-12-Nachtrags mit `gcc`. Der zuletzt genannte HEAD nach diesem Nachtrag ist `b04fcbe`. Eine exakte finale Testanzahl wird nicht nachträglich erfunden.

E7 belegt für sich PW-S02 und die Entscheidungen D-04/D-05. Es belegt noch keinen Snapshot-/Replay-Support, keine Headless-/Godot-Grenze aus PW-S03, keine Combat-Migration aus PW-S04 und keine Performance-Freigabe.

### E8 – PW-S03 Implementierung und Abnahme, 18. September 2026

Quelle: Projektgespräch, vollständig gezeigte Test-/Build-Ausgabe während PW-S03, die gemeinsam erarbeiteten S03-Verträge und Tests, der vom Nutzer gemeldete echte Godot-Smoke-Run sowie die Bestätigung des Repository-Endstands `f90517a`.

PW-S03 führte `SimulationExecutionPlan`, explizite Required/Provided-Modulkomposition und `SimulationSession<TWorkItem>` als host-neutrale Autoritätsgrenze ein. ExternalTests belegen Door/Alarm sowie Resources/Door/Alarm headless ohne Combat-Pflichtzustand. Fehlende Required-Contracts, doppelte Provider und doppelte Handler werden vor beziehungsweise beim Build der Komposition abgewiesen; die gebaute Plan-/Composition-Sicht wird nicht durch spätere Builder-Registrierungen verändert.

Für den Headless-Stand wurde eine vollständige Summary mit **1210/1210** bestandenen Tests gezeigt. Nach Einbau des Godot-Adapters bestätigte der Nutzer Tests und Build erneut als grün. Die Godot-Hauptassembly referenziert `Plaquewright.Core`; `src/Presentation/Main.cs` erzeugt eine Composition/Session, gibt einen externen `GodotReadyInput` hinein und führt ihn über Core aus. Der echte Godot-Start meldete `Plaquewright Godot host ready. SimulationTime=0us, ProcessedEvents=1.`. Anschließend wurde `f90517a` als HEAD genannt und mit `gcc` bestätigt.

E8 belegt PW-S03 innerhalb des aktuellen Referenzumfangs: gemeinsame Core-Autorität für Headless und Godot, nicht engineübergreifende Physics-/Render-Parität. Es belegt noch keine aufgezeichneten autoritativen Host Facts, kein Snapshot-/Replay, keine Combat-Migration und keine Release-/Binärkompatibilitätsmatrix.

### E9 – PW-S04 Implementierung und Abnahme, 18. September 2026

Quelle: Projektgespräch, im S04-Durchgang direkt gezeigte aktuelle Combat-/Resource-Quellen, gemeinsam erarbeitete Produktionsverträge und Tests sowie die Nutzerbestätigung des Repository-Endstands `8bd4dd0`.

PW-S04 führte Combat als zweites fachliches Referenzszenario über die gemeinsame Runtime. `ISimulationWorkItem` markiert produktive autoritative Work Items; `IDomainEvent` ist ein solches Work Item, während die niedrigeren generischen Scheduler-/Runner-/Composition-Typen bewusst keinen entsprechenden Generic-Constraint erhielten. Die interne Prepared-Follow-up-Reservation wurde von einem vorab gebundenen Payload auf eine Reservation von Kapazität/Key/Order generalisiert, sodass der tatsächliche Event-Payload nach dem Commit aus dem echten Ergebnis erzeugt werden kann.

Combat erhielt `DamageCommittedEvent`, `ApplyResolvedDamageAction`, `DamageApplicationOwnerPlan`, `ResolvedDamageApplicationResult` und `ResolvedDamageApplicationExecutor`. Der Executor orchestriert bestehende `DamageResourceLossPlan`-/Staging-, PreDefeat- und Defeat-aware Commit-Bausteine, ohne Resource-Routing als Kernel- oder Action-Wissen festzuschreiben und ohne die bestehende Multi-Owner-Fähigkeit des Committers auf einen einzigen Health-Pool zu reduzieren.

Der lethal-Damage-Referenztest belegt: Projection und PreDefeat verändern vor dem Commit nur den Draft; der autoritative State wird erst im Commit sichtbar; `DamageCommittedEvent` wird aus dem tatsächlichen `DefeatAwareResourceTransactionCommitResult` erzeugt; die Reaction sieht committed State und erzeugt neue Arbeit. Separate Gameplay-Execution-IDs bleiben in Damage- und Recovery-Provenienz erhalten. Negative Tests weisen eine fremde Damage-Resolution, einen fehlenden Target-Owner sowie Resolution/CommitResult mit verschiedenen Target-Entities vor autoritativer Fehlpublikation ab.

Ein zweiter vertikaler Test führt einen bezahlten Angriff über `Cost Commit -> Event -> Reaction -> Damage Resolution -> ApplyResolvedDamageAction -> Damage Commit -> DamageCommittedEvent -> Reaction`. Cost und Damage bleiben getrennte Ledger-Operationen. Der Nutzer bestätigte den finalen Testlauf mit **1215/1215** bestandenen Tests und danach `gcc`; anschließend nannte er `8bd4dd0` als aktuellen HEAD.

E9 belegt PW-S04 innerhalb dieses Referenzumfangs. Es belegt noch keinen Snapshot-/Restore-Mechanismus, kein Replay, keine aufgezeichneten Physics-Host-Facts, keine Performance-/Retention-Freigabe und keine allgemeine Skill-/Combat-Service-Abstraktion.

**Im S04-Durchgang direkt gelesene aktuelle Source-Bausteine:** `DamageResolutionContext.cs`, `DamageResourceTargetContext.cs`, `DefeatAwareResourceTransactionCommitter.cs`, `DamageResourceTransactionStager.cs`, `DefeatAwareResourceTransactionOwnerCommitRequest.cs`, `PreDefeatRecoveryIntervention.cs` und `PreDefeatMinimumCurrentIntervention.cs`. Die neuen S04-Produktionsdateien wurden im Gespräch schrittweise erstellt und durch die bestätigten Test-/Build-Läufe nachgewiesen; diese Dokumentpflege ersetzt keinen vollständigen neuen Repository-Snapshot.

### E10 – PW-S05 Snapshot/Restore-Grundlage, 18. September 2026

Quelle: Projektgespräch, direkt gelesene Scheduler-/Runner-/Runtime-/Combat-Quellen, gemeinsam erarbeitete Snapshot-Typen und Tests sowie der im nachfolgenden Source-Export sichtbare Git-Stand `653c1f5 Add PW-S05 snapshot restore foundation and pending combat work proof`. Der Nutzer hatte diesen Stand mit **1239/1239** bestandenen Tests und anschließendem `gcc` bestätigt.

Der bestätigte Grundstand umfasst in-memory Snapshot/Restore für `ResourceState`, `ResourceStateSet`, `EntityRuntimeState`, `EntityRuntimeStateSet`, Entity-/Execution-/Hit-/Damage-ID-Allocator und `SimulationRuntimeState`. Restore erhält eine neue `SimulationRuntimeIdentity`; captured Runtime-Objektreferenzen werden nicht als Persistenzidentität weiterverwendet.

`SimulationSchedulerSnapshot<TPayloadSnapshot>` erhält Pending Events mit ihren ursprünglichen `ScheduledEventKey`s, Sequence-Fortsetzung und Scheduler-Limits. Capture wird bei aktivem Event oder offener `PreparedScheduledFollowUp`-Reservation abgewiesen. `SimulationRunnerSnapshot<TPayloadSnapshot>` erhält `CurrentTime`, `ProcessedEvents`, Runner-Limit und `_externalInputsClosedThrough`; faulted oder terminal-budgeted Runner werden im ersten fortsetzbaren Snapshot-Profil abgewiesen.

Für Pending Combat Work wurden `DamageResolutionQuantitiesSnapshot`, `DamageResolutionSnapshot` und `ApplyResolvedDamageActionSnapshot` eingeführt. Bereits resolved Damage-Mengen werden nicht erneut berechnet. Execution-/Hit-/Damage-IDs werden beim Rebind nicht neu alloziert; stattdessen werden neue runtime-gebundene Context-Objekte gegen die neue Runtime Identity konstruiert. Fehlende referenzierte Entities führen zum Restore-Fehler.

Ein Integrationstest beweist den ersten realen Scheduler-Fall: Ein pending `ApplyResolvedDamageAction` wird einmal in Runtime A normal fortgesetzt und einmal über `RuntimeSnapshot + RunnerSnapshot -> Runtime B -> Restore` rekonstruiert. Beide Fortsetzungen stimmen im geprüften Scope bei Scheduler-Key/Trace, Resource-Current/Revision sowie Resource-Ledger-Ergebnis und Provenienz überein.

Der aktuelle Core hält keine persistenten RNG-Instanzen. RNG-Streams werden aus Root Seed und Ausführungskontext deterministisch erzeugt; deshalb enthält dieser Scope keinen zusätzlichen langlebigen RNG-Snapshot. Falls spätere Module einen persistenten RNG halten, gehört dessen Zustand in ihren Domain-Snapshot. `CompiledResourceRegistry` wird als kompatible unveränderliche Definition von außen bereitgestellt.

E10 belegt die S05-Grundlage, noch nicht die spätere Mehrtyp-Codec-, Runtime-/Session-Envelope- oder Replay-Fortsetzungsabnahme.

### E11 – PW-S05 Abschluss: Pending-Work-Codec, Runtime/Session-Snapshot und Replay, 18. September 2026

Quelle: Projektgespräch „Code 3“, die gemeinsam erarbeiteten Produktions-/Teständerungen, Nutzerbestätigungen der Test-/Build-Läufe sowie der abschließend gezeigte Git-Stand:

```text
d8826a2 (HEAD -> main, origin/main) Prove PW-S05 deterministic replay continuation
c6b2327 Add PW-S05 runtime session continuation snapshot
789763c Generalize PW-S05 pending combat work snapshots
3afc2e6 new docs
## main...origin/main
```

Der Ausbau erfolgte in drei weiteren kleinen Blöcken:

1. **`789763c` – Pending Combat Work verallgemeinert.** `DamageCommittedEventSnapshot`, `CombatWorkItemSnapshot` und `CombatWorkItemSnapshotCodec` bilden eine explizite Snapshot-Envelope für `ApplyResolvedDamageAction` und `DamageCommittedEvent`. Der committed Event wird aus seinen Fakten rekonstruiert, ohne Damage oder Commit erneut auszuführen. Unbekannte Work Items werden abgewiesen; es gibt keine Reflection-Discovery oder Referenzkopie. Die neuen Codec-Tests liefen 6/6 grün, der vollständige Lauf danach 1245/1245.
2. **`c6b2327` – höherer Runtime-/Session-Fortsetzungs-Snapshot.** `SimulationRuntimeSessionSnapshot<TPayloadSnapshot>` bündelt `SimulationRuntimeStateSnapshot` und `SimulationRunnerSnapshot<TPayloadSnapshot>`. `SimulationSession<TWorkItem>` kann intern aus einem restaurierten Runner und einer neu bereitgestellten Composition rekonstruiert werden. Die Composition wird absichtlich nicht gesnapshottet; der Test baut sie gegen Runtime B neu und weist damit stale Handler-/Runtime-Bindungen nach. Nach diesem Block bestätigte der Nutzer 1247 grüne Tests und anschließend `gcc`.
3. **`d8826a2` – deterministischer Replay-Fortsetzungsbeweis.** Der Test pausiert nach einem bereits committed Damage, während dessen `DamageCommittedEvent` und weiteres resolved Damage pending sind. Nach dem Snapshot werden in Runtime A und Runtime B identische geordnete neue `ReplayDamageInput`-Inputs eingebracht. Runtime A wird zuerst vollständig beendet; anschließend läuft Runtime B. Verglichen werden Runner-Ergebnis, kompletter Fortsetzungs-Trace, Event-Fakten einschließlich Gameplay-/Damage-/Hit-IDs, Resource-Current/Revision sowie die nach der Snapshot-Grenze neu entstandenen Resource-Loss-Ledger-Einträge und Provenienz. Der Nutzer bestätigte den gezielten Test, anschließend den vollständigen Regressionstest und Build als grün und danach `gcc`. Der finale S05-Testumfang beträgt 1248 Tests.

Die Ledger-History vor dem Snapshot wird im aktuellen Profil nicht persistiert; der Replay-Beweis vergleicht die ab der Snapshot-Grenze neu entstehende History. Der Snapshot enthält außerdem keine `SimulationComposition`; kompatible Regeln/Definitionen und `CompiledResourceRegistry` werden von außen bereitgestellt.

E11 belegt PW-S05 für den vereinbarten **in-memory Core-Snapshot-/Replay-Scope**. Es belegt nicht: universelle polymorphe Persistenz aller `ISimulationWorkItem`-Typen, JSON-/Binary-Saveformat, Cross-Version-Migration, aufgezeichnete Physics-/Collision-Host-Facts, reale Netzwerk-/Kauf-Side-Effect-Suppression, Cross-Platform-/Cross-Engine-Bitgleichheit, 30-Sekunden-Retention oder Performancefreigabe.

### E12 – PW-S06 Abschluss: zeitabhängiger Stats-State, Derived Query, Cache und Continuation, 18. September 2026

Quelle: Projektgespräch, der vor PW-S06 erzeugte Source-Export `Plaquewright_S06_Source.txt`, die im Gespräch gemeinsam erarbeiteten S06-Produktions-/Teständerungen, die vom Nutzer bestätigten vollständigen Test-/Build-Läufe und die abschließende Git-Ausgabe. Der Source-Export zeigt den sauberen Ausgangspunkt `6ba6ae9 Document PW-S05 acceptance` und die relevanten vorhandenen `Stats`, `SimulationTime`, `SimulationDuration`, `SchedulerPhase`, `SimulationEventContext`, `SimulationRunner`, Composition- und Entity-Grenzen. Er ist **kein** vollständiger Post-S06-Repository-Snapshot.

Der Nutzer bestätigte die vier S06-Blöcke jeweils nach vollständigem Testlauf und anschließendem `gcc`. Maßgeblich ist die danach vom Nutzer gezeigte tatsächliche Git-Historie:

```text
3ee638a (HEAD -> main, origin/main) Prove PW-S06 timed modifier continuation replay
982798e Add PW-S06 timed modifier snapshot restore
0739c31 Add PW-S06 derived query cache invalidation
238a24a Add PW-S06 timed modifier state and boundary proof
6ba6ae9 Document PW-S05 acceptance
## main...origin/main
```

Die bestätigten Teststände im Projektgespräch waren: S06-A **1256/1256**, S06-B **1260/1260**, S06-C **1265/1265**, S06-D **1267/1267**. Nach dem finalen S06-D-Lauf wurde zusätzlich `gcc` bestätigt; der vollständige Build war grün.

1. **`238a24a` – zeitabhängiger State und Boundary-Beweis.** `ModifierKind`, `TimedModifierKey`, `TimedModifierExpiration`, `TimedModifierStateSet` und `TimedModifierValueQuery` bilden den kleinen Stats-Referenzfall. Generationen unterscheiden aufeinanderfolgende Lebensdauern desselben Keys; Revisionen ändern sich nur bei echter autoritativer Mutation. Der externe Session-Test plant Expiration bei `ExpiresAt` in `SchedulerPhase.StateBoundary` und zeigt, dass eine `Execution` am selben Timestamp bereits den abgelaufenen State liest. Zero-duration wird im ersten Profil abgewiesen.
2. **`0739c31` – Derived-Query-Cache.** `TimedModifierValueQueryCache` bindet Gültigkeit an State-Identität, Revision und Query-Input. Die Tests vergleichen den Cache gegen die ungecachte Referenzrechnung. Apply, Refresh, Cancel und erfolgreiche Expiration invalidieren durch Revision; eine stale Expiration mutiert nicht und lässt den Cache gültig.
3. **`982798e` – Domain-Snapshot/Restore.** `TimedModifierActiveSnapshot`, `TimedModifierSlotSnapshot` und `TimedModifierStateSetSnapshot` erhalten aktive und inaktive Slots, Generation, Revision, Modifierdaten und Ablaufzeit. Inaktive Slots sind notwendig, damit nach Cancel/Restore eine neue Lebensdauer mit `Generation + 1` fortsetzt und alte Expiration-Tokens stale bleiben. Restore reproduziert Revision-/Generation-Exhaustion und führt die Änderungs-API nicht erneut aus.
4. **`3ee638a` – pending Expiration über Restore.** Der Abschlussbeweis kombiniert `TimedModifierStateSetSnapshot` mit einem `SimulationRunnerSnapshot<TestWorkItemSnapshot>`. Die Work-Item-Snapshotformen sind test-only und bilden keinen neuen Produktionsserializer. Eine frische `SimulationComposition` bindet gegen den restaurierten State. Im Refresh-Fall bleibt die alte Generation bei t=110 stale, ohne Revision zu erhöhen; die aktuelle Generation läuft bei t=150 am StateBoundary ab; die Same-Time-Query sieht den Basiswert. Original und Restore stimmen bei Trace, Query-Ergebnissen, Expiration-Ergebnissen und Revisionen überein.

E12 belegt PW-S06 und PW-QA-20 für diesen konkreten Referenzumfang. Es belegt **nicht**: allgemeines Status-/Buff-System, globalen Timer-/Cancellation-Service, universellen Derived-Value-Graph, universellen Work-Item-Serializer, Cross-Version-Persistenz, reale Host-Fact-/Side-Effect-Replayfälle oder Performance-/Retention-Ziele.

**W1 – Godot, Physics introduction.** Gelesen am 17. September 2026. Die offizielle Dokumentation warnt, dass Physics nicht deterministisch garantiert ist. Verwendet nur zur Abgrenzung von Engine-Unabhängigkeit und autoritativem räumlichem Replay.

URL: `https://docs.godotengine.org/en/stable/tutorials/physics/physics_introduction.html`

**W2 – Microsoft, Domain events: Design and implementation.** Gelesen am 17. September 2026. Beschreibt die Trennung von gesammelten Domain Events und deren späterem Dispatch sowie die Bedeutung der Commit-Grenze. Verwendet als Hintergrund; keine Übernahme von Microservices, EF Core oder Datenbanktransaktionen in Plaquewright.

URL: `https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/domain-events-design-implementation`

**W3 – Microsoft, Double / Floating-point numeric types.** Gelesen am 17. September 2026. Verwendet für den Hinweis auf begrenzte numerische Präzision. Kein Nachweis, dass Plaquewright auf konkreten Plattformen divergiert, und keine Empfehlung, die vorhandene net8.0-Toolchain zu ändern.

URL: `https://learn.microsoft.com/en-us/dotnet/api/system.double`
URL: `https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/floating-point-numeric-types`

## 4. Konkrete Source-Anker im untersuchten Archiv

| Pfad im Archiv | Zeile | Symbol / Suchanker |
|---|---|---|
| `src/Core/Transactions/TransactionCoordinator.cs` | 3 | `public static class TransactionCoordinator` |
| `src/Core/Transactions/TransactionCoordinator.cs` | 79 | `new HashSet<PreparedTransactionChange>` |
| `src/Core/Transactions/TransactionCoordinator.cs` | 104 | `preparedChanges[index].Apply();` |
| `src/Core/Transactions/PreparedTransactionChange.cs` | 12 | `public abstract class PreparedTransactionChange` |
| `src/Core/Transactions/PreparedTransactionChange.cs` | 20 | `internal void Apply()` |
| `src/Core/Transactions/PreparedTransactionChange.cs` | 33 | `protected abstract void ApplyCore();` |
| `src/Core/Resources/ResourceCostTransactionParticipant.cs` | 6 | `public sealed class ResourceCostTransactionParticipant` |
| `src/Core/Resources/ResourceCostTransactionParticipant.cs` | 44 | `public bool TryPrepare(` |
| `src/Core/Resources/ResourceCostTransactionParticipant.cs` | 91 | `private sealed class PreparedResourceCostChange` |
| `src/Core/Resources/ResourceTransactionCommitter.cs` | 3 | `internal static class ResourceTransactionCommitter` |
| `src/Core/Resources/ResourceTransactionCommitter.cs` | 18 | `internal static PreparedResourceTransactionCommit Prepare(` |
| `src/Core/Resources/ResourceTransactionCommitter.cs` | 51 | `internal static void ApplyPrepared(` |
| `src/Core/Simulation/SimulationRuntimeState.cs` | 7 | `public sealed class SimulationRuntimeState` |
| `tests/Plaquewright.Core.ExternalTests/PublicApi/PublicApiBoundaryTests.cs` | 10 | `public void ExternalTestAssembly_IsNotCoreFriendAssembly` |
| `tests/Plaquewright.Core.ExternalTests/Transactions/CrossModuleTransactionTests.cs` | 11 | `public void PayGoldAndOpenDoor_WhenBothCanPrepare_CommitsBoth` |
| `tests/Plaquewright.Core.ExternalTests/Transactions/CrossModuleTransactionTests.cs` | 73 | `public void PayGoldAndOpenDoor_WhenDoorRejects_CommitsNeither` |
| `tests/Plaquewright.Core.Tests/Combat/DefeatAwareResourceTransactionCommitterTests.cs` | 698 | `public void MultiOwnerDraft_WithoutDefeatTransitions_CommitsAllOwnersAtomically` |

Die externen Cross-Module-Tests prüfen den Gold-/Door-Fall. Im Defeat-Committer-Test existiert bereits ein Multi-Owner-Szenario; die ältere pauschale Single-Owner-Beschreibung ist daher keine vollständige aktuelle Fähigkeitsbeschreibung.

Die Coordinator- und Prepared-Quellen zeigen die Prepare-/Apply-Struktur und Sichtbarkeiten. Sie sind kein alleiniger Beweis für beliebige Kombinationen gemeinsam genutzten States, Ledgers oder wiederverwendeter Prepared-Objekte.

## 5. Integrität der bereitgestellten Grundlagen

SHA-256, berechnet in diesem Dokumentationsdurchgang:

| ID | Datei | SHA-256 |
|---|---|---|
| E1 | `Pasted markdown(1).md` | `3cdf9f9dc76e2087aca7ec3884d144d9f0328eb41453683782ca4704fd8c60c1` |
| E3 | `zip.7z` | `3d645e35d1e660cab1b94fb3742d019e76eee4f785c94dbf02956a768f9d5cde` |
| E4a | `Pasted text (2)(6).txt` | `390debd2127476ef6d98498f6acbfc8402b3a25b49d755942c1ce92cba15c0c1` |
| E4b | `Pasted text(20260916-135237).txt` | `a8d799cf0e99f9a8c59d44a60a5a5e7dd94d9cbb979141a46e38f813f48f96c6` |
| E4c | `Pasted text(20260916-135035).txt` | `37924593fabc040d2f68ebf101db494b8a400167cb9f6c9b512a7eed69825d01` |
| E5a | `Plaquewright_Manifest_v1_0.md` | `d26d1f6648c97b17fa39218cb35bac392e0b78c41c0f555654f271509d252d77` |
| E5b | `Plaquewright_Implementation_Sequence_v1_0.md` | `7d21ec37c8d346202a82c890c7f5c7346901d52430c0ee7939377bc266380928` |
| E5c | `Plaquewright_Freigabe_und_Testnachweise_v1_0.md` | `b03485de10126b0ac5f8793b177fac080ee66e6ae9f59133289f9a0ddca663f1` |

E2 ist eine sichtbare Gesprächsangabe und wird nicht als künstliche heruntergeladene Git-Log-Datei ausgegeben.

## 6. Grenzen dieser Bearbeitung

Ausgeführt in der ursprünglichen Dokumenterstellung: Quelleninspektion, Entwurfsarbeit, Markdown-Dateierstellung und strukturelle Dokumentprüfung. Im anschließenden Projektverlauf wurden PW-S02, PW-S03 und PW-S04 durch den Nutzer im Repository umgesetzt und getestet; E7, E8 und E9 dokumentieren diese bestätigte Entwicklung. PW-S05 wurde von der auf `653c1f5` bestätigten Grundlage E10 bis zum Abschluss `d8826a2` in E11 fortgeführt. PW-S06 wurde anschließend vom dokumentierten Ausgangspunkt `6ba6ae9` bis zum Funktionsabschluss `3ee638a` in E12 nachgewiesen.

Diese aktuelle Dokumentaktualisierung führt selbst keinen neuen .NET-Testlauf, Godot-Start, Cross-Platform-Test, Benchmark oder vollständigen Dependency-Graph-Audit aus. Sie rekonstruiert keinen vollständigen Post-S06-Source-Snapshot von `3ee638a`; Aussagen über PW-S02 bis PW-S06 beschränken sich auf die im Gespräch aufgebauten/benannten Verträge, direkt gezeigten relevanten Sources, gezeigten beziehungsweise bestätigten Test-/Build-Ausgaben, den gemeldeten Godot-Smoke-Run, die Git-Ausgaben und die Nutzerbestätigungen.

PW-S04 ist als Referenzschritt abgeschlossen. PW-S05 ist auf `d8826a2` für den ausdrücklich beschriebenen in-memory Core-Snapshot-/Replay-Scope abgeschlossen. PW-S06 ist auf `3ee638a` für den zeitabhängigen Stats-/Derived-Query-Referenzumfang abgeschlossen. Host-Fact-Replay, reale Side-Effect-Suppression, universelle Work-Item-Persistenz, Cross-Version-/Physics-/Plattform-Parität, History-Retention und Performance werden dadurch nicht vorweggenommen.
