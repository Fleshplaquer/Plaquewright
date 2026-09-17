# Plaquewright – Migrationsplan 2.0

**Datum:** 17. September 2026 · **Status:** Angenommener Migrationsleitplan 2.0  
**Ausgangspunkt:** Audit-Baseline `d39cb54`  
**Prinzip:** Verhalten erhalten, Grenzen testen, dann den kleinsten notwendigen Eingriff vornehmen.

Die frühere `MIGRATION_PLAN.md` wird in der Architekturübergabe genannt, ist im bereitgestellten Archiv aber nicht vorhanden. Dieses Dokument ersetzt deshalb keine ungelesene Detailentscheidung stillschweigend.

## 1. Dokumente zunächst nebeneinander

Die neuen Markdown-Dateien werden gemeinsam unter `docs/architecture/v2/` abgelegt. Ihre relativen Links setzen voraus, dass sie zusammenbleiben. Architektur 2.0 ist angenommen; ein späterer Code- oder Performance-Status wird dadurch nicht vorweggenommen.

Die drei alten Dokumente bleiben als historische Entscheidungsgrundlage erhalten. Ihre Produkt-/Roadmap-Aussagen werden gegenüber Architektur 2.0 als historisch markiert. Eine spätere Verschiebung nach `docs/legacy/idler/` erfolgt mit Linkprüfung; ein bloßes Suchen/Ersetzen von Idler durch Plaquewright reicht nicht.

Alte Gameplay-Regeln werden nicht gelöscht. Gemäß D-02 bleiben die bestehenden Combat-Regeln als ausdrücklich benanntes Referenz-Regelpaket erhalten; andere historische Produkt-/Roadmap-Aussagen bleiben Quellenmaterial.

## 2. Baseline nicht umdeuten

`d39cb54` bleibt der technische Auditpunkt. Ein Commit, der diese Dokumente hinzufügt, erhält einen anderen Hash und wird als Dokumentationsänderung benannt.

A/B-Findings werden nicht neu nummeriert, und fehlende ausführliche Finding-Texte werden nicht rekonstruiert, als wären sie vorhanden. Die ursprüngliche Framework-Übergabe mit 1023 Tests und „Transaction Boundary als nächster Schritt“ wird als älterer Technikstand gekennzeichnet.

## 3. Vor dem nächsten Code keine Komplettreorganisation

Zuerst die aktuellen betroffenen Source-/Testkörper für den nächsten Referenzfall abgleichen. Nur wenn die neue Grenze einen bestehenden Typ tatsächlich blockiert, wird refaktoriert.

Der erste funktionale Ausbau ist die sichere Verbindung von Commit-Ergebnis, Event und Folgearbeit. Dazu kann ein kleiner gemeinsamer Vertrag nötig werden. Daraus folgt keine Freigabe aller Resource-Interna und keine neue universelle State-Schnittstelle.

## 4. Generische Runtime von Gameplay-Komposition trennen

`SimulationRuntimeState` wird nicht allein wegen seines Namens in `KernelWorld` umbenannt. Zunächst muss der Door-/Alarm-Fall zeigen, welche generischen Dienste er braucht, ohne Combat oder Resource-Entity-Zustand vorauszusetzen.

Erst diese Dienste erhalten eine schmale Kompositionsgrenze. Der bestehende Typ kann weiterhin eine bequeme Zusammenstellung für das vorhandene Gameplay bleiben.

**Schutz:** Alle bisherigen Ownership-/Runtime-Tests bleiben bestehen. Ein formaler Dependency-Split darf keine fremden Kontexte plötzlich gültig machen.

## 5. Resources und Transactions bewahren

Resource-Draft, Projektionen, Operationen, Ledger und Prepared-Mechanik bleiben die Implementierungsgrundlage. Ein neuer allgemeiner Workflow darf nicht parallel eine zweite unverbundene Ressourcenbuchhaltung eröffnen.

Die erste neue Kombination prüft gemeinsame State-/Ledger-Ziele und Prepared-Lebensdauern. Reicht ein bestehender Participant nicht, wird ein passender Domain-Adapter ergänzt oder die Kombination kontrolliert begrenzt.

Nachträgliche Events sind kein Ersatz für atomare State-Änderung. Alarm nach einer Türöffnung darf Folgearbeit sein; eine zwingend gemeinsame Zahlung/Öffnung darf nicht in zwei lose Events zerfallen.

## 6. Regeln vom Kernel fernhalten, nicht alles abstrahieren

Vorhandene Combat-Rechner bleiben zunächst konkrete Referenzimplementierungen. Eine zweite tatsächlich benötigte Variante bestimmt, wo ein austauschbarer Regelvertrag sinnvoll ist.

Nicht jeder private Rechenschritt wird zu einem öffentlichen Interface. Domain-spezifische Mengen wie `DamageTaken` bleiben nützlich, auch wenn der Kernel sie nicht kennt.

Die Enum-Frage wird punktuell behandelt: Offene Inhalte/Klassifikationen können registrierte IDs benötigen; geschlossene Operationen behalten überprüfbare Semantik. Kein pauschaler Umbau aller Enums in Strings.

## 7. Snapshots und zeitliche Arbeit nicht nachträglich vergessen

Neue geplante Arbeit erhält beschreibbare Identität, Payload und Versions-/Lifetime-Regeln. Für persistierbare Arbeit werden versteckte Closure-Zustände und Engine-Objektreferenzen vermieden.

Das bedeutet noch nicht, dass ein Speicherformat jetzt feststeht. Vor dem ersten Replay muss aber klar sein, welche neuen Zustände und Queues zum Snapshot gehören.

## 8. Tests und Kompatibilität

Jeder Eingriff erhält einen Verhaltenstest vor oder zusammen mit der Änderung. Bestehende Assertions werden nicht entfernt, nur damit ein anderer interner Ablauf grün wird; geänderte Fachsemantik braucht eine ausdrückliche Entscheidung.

Eine neue öffentliche Grenze bekommt zusätzlich einen Test aus `Core.ExternalTests`. Ein Namespace in der Datei macht eine Core.Tests-Datei nicht zur externen Assembly.

Bei API-Änderungen werden vorhandene Aufrufer migriert oder bewusst kompatible Adapter erhalten. Eine externe API wird nicht allein aus internem Implementierungskomfort erweitert.

## 9. Was in diesem Durchgang nicht geschieht

Keine Quellcodeverschiebung, keine Dateilöschung im Nutzerrepository, keine neue Bibliotheksversion, kein Git-Commit oder Push. Erstellt wird ein neuer Dokumentationsentwurf. Seine Einordnung wird nicht als bereits ausgeführte Migration ausgegeben.

Als nächster Repository-Schritt kann ein reiner Dokumentationscommit entstehen. Produktionsänderungen folgen anschließend in getrennten, getesteten Commits entlang der [Baufolge](Plaquewright_Implementation_Sequence_v2_0.md).

**Quellen:** [E1–E5](SOURCE_EVIDENCE_v2_0.md).
