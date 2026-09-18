# Plaquewright – Source Evidence 2.0

**Datum:** 18. September 2026 · **Status:** Quellen-/Evidenzregister zur freigegebenen Architektur 2.0
**Zweck:** Vision, alten Source-Snapshot, neue Nutzerbestätigungen und vorgeschlagene Architektur nicht miteinander verwechseln.

## 1. Quellenhierarchie nach Aussageart

Für das **Produktziel** gilt die neu eingebrachte Architekturübergabe E1: Framework statt einzelnes Idle-Spiel. Für die **historische A/B-Audit-Baseline** gilt E2 mit `d39cb54`; für den **PW-S02-Funktionsnachweis** gilt E7 mit `b04fcbe`; für den **PW-S03-Funktionsnachweis** gilt E8 mit `f90517a`; für den **aktuellen PW-S04-Funktionsnachweis** gilt E9 mit `8bd4dd0`. Für den vollständig direkt gelesenen älteren Quellcode gilt der bereitgestellte Archiv-Snapshot E3. Eine ältere Zusammenfassung wird nicht allein durch ihren späteren Upload zum neuesten Code.

Technische Entscheidungen werden nur dort als beschlossen ausgegeben, wo sie aus E1 oder den bestätigten Projektständen E6–E9 hervorgehen. D-03, D-06 und D-07 bleiben offene Detailgates. D-04 und D-05 wurden im PW-S02-Durchgang beschlossen und technisch nachgewiesen. PW-S03 belegt Headless/Godot als gemeinsame Host-Autoritätsgrenze, schließt D-07 als Release-/Distributionsentscheidung aber noch nicht. PW-S04 belegt Combat als zweites fachliches Referenzszenario auf derselben Runtime-Grenze; D-03, D-06 und D-07 bleiben dadurch unverändert offen.

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

Ausgeführt in der ursprünglichen Dokumenterstellung: Quelleninspektion, Entwurfsarbeit, Markdown-Dateierstellung und strukturelle Dokumentprüfung. Im anschließenden Projektverlauf wurden PW-S02, PW-S03 und PW-S04 durch den Nutzer im Repository umgesetzt und getestet; E7, E8 und E9 dokumentieren diese bestätigte Entwicklung.

Diese aktuelle Dokumentaktualisierung führt selbst keinen neuen .NET-Testlauf, Godot-Start, Replay-/Cross-Platform-Test, Benchmark oder vollständigen Dependency-Graph-Audit aus. Sie rekonstruiert auch keinen vollständigen Source-Snapshot von `8bd4dd0`; Aussagen über PW-S02/PW-S03/PW-S04 beschränken sich auf die im Gespräch aufgebauten/benannten Verträge, direkt gezeigten relevanten Sources, gezeigten beziehungsweise bestätigten Test-/Build-Ausgaben, den gemeldeten Godot-Smoke-Run und die Nutzerbestätigungen.

PW-S04 ist damit als Referenzschritt abgeschlossen. PW-S05 und spätere Fähigkeiten bleiben geplant. Snapshot/Replay, Host-Fact-Replay, Physics-Parität und Performance werden durch die bisherigen Nachweise nicht vorweggenommen.
