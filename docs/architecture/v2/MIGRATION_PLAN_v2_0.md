# Plaquewright – Migrationsplan 2.0

**Datum:** 18. September 2026 · **Status:** Angenommener Migrationsleitplan 2.0
**Historische Audit-Baseline:** `d39cb54`
**PW-S02-Nachweis:** `b04fcbe`
**PW-S03-Nachweis:** `f90517a`
**Aktueller Funktionsnachweis:** PW-S04 auf `8bd4dd0`
**Prinzip:** Verhalten erhalten, Grenzen testen, dann den kleinsten notwendigen Eingriff vornehmen.

Die frühere `MIGRATION_PLAN.md` wird in der Architekturübergabe genannt, ist im bereitgestellten Archiv aber nicht vorhanden. Dieses Dokument ersetzt deshalb keine ungelesene Detailentscheidung stillschweigend.

## 1. Dokumente zunächst nebeneinander

Die neuen Markdown-Dateien werden gemeinsam unter `docs/architecture/v2/` abgelegt. Ihre relativen Links setzen voraus, dass sie zusammenbleiben. Architektur 2.0 ist angenommen; ein späterer Code- oder Performance-Status wird dadurch nicht vorweggenommen.

Die drei alten Dokumente bleiben als historische Entscheidungsgrundlage erhalten. Ihre Produkt-/Roadmap-Aussagen werden gegenüber Architektur 2.0 als historisch markiert. Eine spätere Verschiebung nach `docs/legacy/idler/` erfolgt mit Linkprüfung; ein bloßes Suchen/Ersetzen von Idler durch Plaquewright reicht nicht.

Alte Gameplay-Regeln werden nicht gelöscht. Gemäß D-02 bleiben die bestehenden Combat-Regeln als ausdrücklich benanntes Referenz-Regelpaket erhalten; andere historische Produkt-/Roadmap-Aussagen bleiben Quellenmaterial.

## 2. Baseline nicht umdeuten

`d39cb54` bleibt der historische A/B-Auditpunkt. `b04fcbe` belegt PW-S02; `f90517a` belegt PW-S03; `8bd4dd0` belegt den abgeschlossenen PW-S04-Endstand. Diese Referenzen werden nicht miteinander vermischt. Dokumentationscommits erhalten wiederum eigene Hashes und ändern diese Nachweisrollen nicht.

A/B-Findings werden nicht neu nummeriert, und fehlende ausführliche Finding-Texte werden nicht rekonstruiert, als wären sie vorhanden. Die ursprüngliche Framework-Übergabe mit 1023 Tests und „Transaction Boundary als nächster Schritt“ wird als älterer Technikstand gekennzeichnet.

## 3. Nach PW-S04 weiterhin keine Komplettreorganisation

PW-S02 hat gezeigt, dass bestehender Transaction-Coordinator und Scheduler mit wenigen schmalen Verträgen die benötigte Commit→Event→Reaction-Semantik tragen. PW-S03 hat darauf einen eingefrorenen Execution Plan, explizite Required/Provided-Komposition und die host-neutrale `SimulationSession` gesetzt. PW-S04 hat anschließend vorhandene Combat-Bausteine über dieselbe Runtime geführt, ohne einen globalen Combat-Service oder Kernel-Fachbegriffe einzuführen.

Die dabei notwendige Generalisierung blieb klein: produktive Work Items erhielten `ISimulationWorkItem`, die interne Follow-up-Reservation wurde payload-spät nutzbar, und Combat bekam einen result-backed Event sowie einen kleinen Application-Executor. Der nächste Codeblock ist PW-S05: Snapshot/Restore und Replay an einer quiescent boundary. Auch dort wird nur die tatsächlich benötigte Persistenz-/Restore-Grenze eingeführt.

## 4. Generische Runtime von Gameplay-Komposition trennen

`SimulationRuntimeState` wird nicht allein wegen seines Namens in `KernelWorld` umbenannt. PW-S03 hat bereits gezeigt, dass Door/Alarm sowie Resources/Door/Alarm über `SimulationComposition` und `SimulationSession` laufen können, ohne `SimulationRuntimeState` oder Combat als Pflichtzustand zu verwenden.

Diese schmale Kompositionsgrenze bleibt bestehen. Der vorhandene `SimulationRuntimeState` kann weiterhin eine bequeme Zusammenstellung für das vorhandene Gameplay bleiben, bis PW-S04 oder ein späterer realer Fall eine konkrete Entkopplung verlangt.

**Schutz:** Alle bisherigen Ownership-/Runtime-Tests bleiben bestehen. Ein formaler Dependency-Split darf keine fremden Kontexte plötzlich gültig machen.

## 5. Resources und Transactions bewahren

Resource-Draft, Projektionen, Operationen, Ledger und Prepared-Mechanik bleiben die Implementierungsgrundlage. Ein neuer allgemeiner Workflow darf nicht parallel eine zweite unverbundene Ressourcenbuchhaltung eröffnen.

Der PW-S02-Fall verwendet die vorhandene Gold-/Door-Transaction weiter und ergänzt die sichere Event-Publikation. Weitere gemeinsame State-/Ledger-Ziele und Prepared-Lebensdauern werden weiterhin nur dann erweitert, wenn ein konkreter nächster Referenzfall sie benötigt.

Nachträgliche Events sind kein Ersatz für atomare State-Änderung. Alarm nach einer Türöffnung darf Folgearbeit sein; eine zwingend gemeinsame Zahlung/Öffnung darf nicht in zwei lose Events zerfallen.

## 6. Regeln vom Kernel fernhalten, nicht alles abstrahieren

Vorhandene Combat-Rechner bleiben konkrete Referenzimplementierungen. PW-S04 hat bestätigt, dass selbst der neue Damage-Application-Executor Combat-intern bleiben kann. Eine zweite tatsächlich unabhängige Variante bestimmt weiterhin, wo ein austauschbarer allgemeiner Regelvertrag sinnvoll ist.

Nicht jeder private Rechenschritt wird zu einem öffentlichen Interface. Domain-spezifische Mengen wie `DamageTaken` bleiben nützlich, auch wenn der Kernel sie nicht kennt.

Die Enum-Frage wird punktuell behandelt: Offene Inhalte/Klassifikationen können registrierte IDs benötigen; geschlossene Operationen behalten überprüfbare Semantik. Kein pauschaler Umbau aller Enums in Strings.

## 7. Snapshots und zeitliche Arbeit nicht nachträglich vergessen

Neue geplante Arbeit erhält beschreibbare Identität, Payload und Versions-/Lifetime-Regeln. Für persistierbare Arbeit werden versteckte Closure-Zustände und Engine-Objektreferenzen vermieden.

Das bedeutet noch nicht, dass ein Speicherformat jetzt feststeht. Vor dem ersten Replay muss aber klar sein, welche neuen Zustände und Queues zum Snapshot gehören.

## 8. Tests und Kompatibilität

Jeder Eingriff erhält einen Verhaltenstest vor oder zusammen mit der Änderung. Bestehende Assertions werden nicht entfernt, nur damit ein anderer interner Ablauf grün wird; geänderte Fachsemantik braucht eine ausdrückliche Entscheidung.

Eine neue öffentliche Grenze bekommt zusätzlich einen Test aus `Core.ExternalTests`. Ein Namespace in der Datei macht eine Core.Tests-Datei nicht zur externen Assembly.

Bei API-Änderungen werden vorhandene Aufrufer migriert oder bewusst kompatible Adapter erhalten. Eine externe API wird nicht allein aus internem Implementierungskomfort erweitert.

## 9. Nächster Repository-Schritt nach dieser Dokumentaktualisierung

Diese Dokumentaktualisierung verändert selbst keinen Produktionscode und führt keinen zusätzlichen Testlauf aus. Sie synchronisiert den Architekturstand mit dem vom Nutzer bestätigten PW-S04-Endstand `8bd4dd0` und dem finalen Testumfang 1215/1215.

Nach dem Dokumentationscommit folgt PW-S05 in einem getrennten, getesteten Codeblock. Zuerst werden Snapshot-Inhalt, quiescent boundary, Restore-Zielruntime sowie Pending Scheduler-/Reaction-, RNG-, ID- und Domainzustände konkret am aktuellen Codebestand geprüft. Keine allgemeine Serializer-, Savegame-, EventStore- oder Cross-Version-Schicht wird allein wegen der Dokumentpflege vorgezogen.

**Quellen:** [E1–E5, E7–E9](SOURCE_EVIDENCE_v2_0.md).
