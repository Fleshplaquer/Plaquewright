# Plaquewright – Source Evidence 2.0

**Datum:** 17. September 2026 · **Status:** Quellen-/Evidenzregister zur freigegebenen Architektur 2.0  
**Zweck:** Vision, alten Source-Snapshot, neue Nutzerbestätigungen und vorgeschlagene Architektur nicht miteinander verwechseln.

## 1. Quellenhierarchie nach Aussageart

Für das **Produktziel** gilt die neu eingebrachte Architekturübergabe E1: Framework statt einzelnes Idle-Spiel. Für den **jüngsten technischen Anschluss** gilt der Nutzerbeleg E2 mit `d39cb54`. Für **direkt gelesenen Quellcode** gilt der bereitgestellte Archiv-Snapshot E3. Eine ältere Zusammenfassung wird nicht allein durch ihren späteren Upload zum neuesten Code.

Technische Entscheidungen werden nur dort als beschlossen ausgegeben, wo sie aus E1, bestätigten Änderungen oder E6 hervorgehen. D-03 bis D-07 bleiben Detailgates und werden nicht rückwirkend als entschieden dargestellt.

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

## 3. Externe Primärquellen für zwei technische Präzisierungen

Die externen Quellen bestimmen nicht das Projektziel und führen keine neue Engine-, Datenbank- oder Bibliotheksabhängigkeit ein.


**E6 – Projektgespräch, 17. September 2026: Annahme D-01 und D-02.**
Der Nutzer stimmte dem vorgeschlagenen Determinismus-/Replay-Vertrag und der Einordnung des bestehenden Combat als austauschbares Referenz-Regelpaket ausdrücklich zu. Daraus stammen die finalen Formulierungen in PW-10, PW-13, PW-15 und PW-20. Diese Zustimmung ist keine Behauptung, dass Snapshot/Replay oder Cross-Engine-Adapter bereits implementiert sind.

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

Ausgeführt: Quelleninspektion, Entwurfsarbeit, Markdown-Dateierstellung und strukturelle Dokumentprüfung.

Nicht ausgeführt: neuer .NET-Testlauf, neuer Godot-Start, Replay-/Cross-Platform-Test, Benchmark, vollständiger Dependency-Graph-Audit des finalen Codes oder Änderung des Nutzerrepositories.

Aussagen über die neue Baufolge und künftige Fähigkeiten sind Vorschläge. Der Test-/Freigabekatalog bezeichnet geplante Nachweise ausdrücklich als geplant.

**Zu den Dokumenten:** [Einstieg](README.md), [Manifest](Plaquewright_Manifest_v2_0.md).
