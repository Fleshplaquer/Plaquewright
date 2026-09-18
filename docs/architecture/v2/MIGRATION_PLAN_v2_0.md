# Plaquewright – Migrationsplan 2.0

**Datum:** 18. September 2026 · **Status:** Angenommener Migrationsleitplan 2.0
**Historische Audit-Baseline:** `d39cb54`
**PW-S02-Nachweis:** `b04fcbe`
**PW-S03-Nachweis:** `f90517a`
**PW-S04-Nachweis:** `8bd4dd0`
**PW-S05-Nachweis:** `d8826a2` – vereinbarter in-memory Snapshot-/Replay-Umfang abgenommen; finaler S05-Testumfang 1248 Tests, anschließend `gcc`
**PW-S06-Nachweis:** `3ee638a` – zeitabhängiger Stats-/Derived-Query-Referenzumfang abgenommen; finaler S06-Testumfang 1267/1267, anschließend `gcc`
**Prinzip:** Verhalten erhalten, Grenzen testen, dann den kleinsten notwendigen Eingriff vornehmen.

Die frühere `MIGRATION_PLAN.md` wird in der Architekturübergabe genannt, ist im bereitgestellten Archiv aber nicht vorhanden. Dieses Dokument ersetzt deshalb keine ungelesene Detailentscheidung stillschweigend.

## 1. Dokumente zunächst nebeneinander

Die neuen Markdown-Dateien werden gemeinsam unter `docs/architecture/v2/` abgelegt. Ihre relativen Links setzen voraus, dass sie zusammenbleiben. Architektur 2.0 ist angenommen; ein späterer Code- oder Performance-Status wird dadurch nicht vorweggenommen.

Die drei alten Dokumente bleiben als historische Entscheidungsgrundlage erhalten. Ihre Produkt-/Roadmap-Aussagen werden gegenüber Architektur 2.0 als historisch markiert. Eine spätere Verschiebung nach `docs/legacy/idler/` erfolgt mit Linkprüfung; ein bloßes Suchen/Ersetzen von Idler durch Plaquewright reicht nicht.

Alte Gameplay-Regeln werden nicht gelöscht. Gemäß D-02 bleiben die bestehenden Combat-Regeln als ausdrücklich benanntes Referenz-Regelpaket erhalten; andere historische Produkt-/Roadmap-Aussagen bleiben Quellenmaterial.

## 2. Baseline nicht umdeuten

`d39cb54` bleibt der historische A/B-Auditpunkt. `b04fcbe` belegt PW-S02; `f90517a` belegt PW-S03; `8bd4dd0` belegt PW-S04; `d8826a2` belegt PW-S05; `3ee638a` belegt den abgeschlossenen PW-S06-Umfang. Diese Referenzen werden nicht miteinander vermischt. Dokumentationscommits erhalten wiederum eigene Hashes und ändern diese Nachweisrollen nicht.

Für PW-S05 sind außerdem die technischen Zwischenstände relevant: `653c1f5` für die Snapshot-/Restore-Grundlage, `789763c` für die explizite Pending-Combat-Work-Grenze und `c6b2327` für den Runtime-/Session-Fortsetzungs-Snapshot. `d8826a2` ist der abschließende Replay-Nachweis.

Für PW-S06 sind `238a24a` (zeitabhängiger State/Boundary), `0739c31` (Derived-Query-Cache), `982798e` (Domain-Snapshot/Restore) und `3ee638a` (Continuation-Replay mit pending Expiration) die technische Folge.

A/B-Findings werden nicht neu nummeriert, und fehlende ausführliche Finding-Texte werden nicht rekonstruiert, als wären sie vorhanden. Die ursprüngliche Framework-Übergabe mit 1023 Tests und „Transaction Boundary als nächster Schritt“ wird als älterer Technikstand gekennzeichnet.

## 3. Nach PW-S06 weiterhin keine Komplettreorganisation

PW-S02 hat gezeigt, dass bestehender Transaction-Coordinator und Scheduler mit wenigen schmalen Verträgen die benötigte Commit→Event→Reaction-Semantik tragen. PW-S03 hat darauf einen eingefrorenen Execution Plan, explizite Required/Provided-Komposition und die host-neutrale `SimulationSession` gesetzt. PW-S04 hat vorhandene Combat-Bausteine über dieselbe Runtime geführt, ohne einen globalen Combat-Service oder Kernel-Fachbegriffe einzuführen.

PW-S05 folgt demselben Muster. Die notwendige Persistenz-/Fortsetzungsgrenze blieb klein und explizit: in-memory Snapshots für autoritativen Domain-/Runtime-State, Scheduler/Runner, typspezifisch beschreibbares pending Combat Work und ein höherer Runtime-/Session-Envelope. Es wurde weder ein Object-Graph-Serializer noch ein universeller Event Store oder Savegame-Format eingeführt.

PW-S06 setzt dieses Muster fort: Statt eines allgemeinen Status-/Timer-/Cache-Unterbaus wurde ein kleiner Stats-eigener `TimedModifierStateSet` eingeführt. Generationen entwerten alte Expiration-Arbeit, Revisionen invalidieren die abgeleitete Cache-Sicht, und der domainlokale Snapshot erhält auch inaktive Slots. Der Abschlussbeweis nutzt eine test-only Work-Item-Snapshotgrenze; daraus wurde kein neuer universeller Persistenzvertrag abgeleitet.

Die Composition selbst wird nicht gesnapshottet. Nach Restore wird sie gegen die neue Runtime neu aufgebaut. Dadurch werden Handler-Closures und andere Live-Bindungen aus Runtime A nicht als Persistenzzustand fortgeschleppt.

## 4. Generische Runtime von Gameplay-Komposition trennen

`SimulationRuntimeState` wird nicht allein wegen seines Namens in `KernelWorld` umbenannt. PW-S03 hat gezeigt, dass Door/Alarm sowie Resources/Door/Alarm über `SimulationComposition` und `SimulationSession` laufen können, ohne `SimulationRuntimeState` oder Combat als Pflichtzustand zu verwenden.

PW-S05 bestätigt zusätzlich, dass ein höherer Fortsetzungs-Snapshot Runtime-State und Runner-State kohärent bündeln kann, ohne `SimulationComposition` selbst zu serialisieren. Die Komposition bleibt Build-/Ruleset-Wissen und wird beim Restore explizit neu gebunden.

**Schutz:** Alle bisherigen Ownership-/Runtime-Tests bleiben bestehen. Ein formaler Dependency-Split darf keine fremden Kontexte plötzlich gültig machen.

## 5. Resources und Transactions bewahren

Resource-Draft, Projektionen, Operationen, Ledger und Prepared-Mechanik bleiben die Implementierungsgrundlage. Ein neuer allgemeiner Workflow darf nicht parallel eine zweite unverbundene Ressourcenbuchhaltung eröffnen.

Der PW-S02-Fall verwendet die vorhandene Gold-/Door-Transaction weiter und ergänzt die sichere Event-Publikation. PW-S04 verwendet dieselben Grundgrenzen für Cost und Damage. PW-S05 restauriert committed Resource-State und vergleicht die **nach** der Snapshot-Grenze neu entstehende Ledger-History; die ältere Ledger-History vor dem Snapshot ist im aktuellen Profil nicht Bestandteil des Snapshots.

Nachträgliche Events sind kein Ersatz für atomare State-Änderung. Ebenso ist Replay kein erneuter Commit historischer Fakten: ein restaurierter `DamageCommittedEvent` beschreibt einen bereits committed Sachverhalt und führt den alten Damage-Commit nicht erneut aus.

## 6. Regeln vom Kernel fernhalten, nicht alles abstrahieren

Vorhandene Combat-Rechner bleiben konkrete Referenzimplementierungen. PW-S04 hat bestätigt, dass selbst der Damage-Application-Executor Combat-intern bleiben kann. PW-S05 setzt darauf einen **Combat-spezifischen** `CombatWorkItemSnapshotCodec` für die im Referenzprofil tatsächlich benötigten Typen.

Dieser Codec ist ausdrücklich kein universeller Serializer aller `ISimulationWorkItem`-Typen. Er kennt `ApplyResolvedDamageAction` und `DamageCommittedEvent`; unbekannte Work Items werden kontrolliert abgewiesen. PW-S06 liefert inzwischen einen zweiten unabhängigen Domain-Fall, verwendet für dessen Abschlussbeweis aber bewusst nur eine test-only Work-Item-Snapshotgrenze. Generalisiert wird daher weiterhin kein produktiver Universalcodec; gemeinsam ist lediglich der generische Scheduler-/Runner-Payload-Snapshotvertrag.

Nicht jeder private Rechenschritt wird zu einem öffentlichen Interface. Domain-spezifische Mengen wie `DamageTaken` bleiben nützlich, auch wenn der Kernel sie nicht kennt.

Die Enum-Frage wird punktuell behandelt: Offene Inhalte/Klassifikationen können registrierte IDs benötigen; geschlossene Operationen behalten überprüfbare Semantik. Kein pauschaler Umbau aller Enums in Strings.

## 7. Snapshots und zeitliche Arbeit nicht nachträglich vergessen

PW-S05 konkretisiert den Snapshot-Vertrag jetzt vollständig für den vereinbarten ersten Core-Scope. Snapshot bedeutet **nicht** Object-Graph-Clone, sondern beschreibbarer autoritativer Zustand, aus dem eine neue Runtime rekonstruiert werden kann.

Erfasst werden Resource-/Entity-State samt Revisionen, relevante deterministische ID-Allocator, Runner-Zeit/`ProcessedEvents`/D-04-Input-Closure sowie Scheduler-Limits, Pending Work, originale `ScheduledEventKey`s und Sequence-Fortsetzung. Restore erzeugt eine neue `SimulationRuntimeIdentity`.

Runtime-gebundene pending Arbeit wird nicht als alte Objektinstanz konserviert. `DamageResolutionSnapshot` und `ApplyResolvedDamageActionSnapshot` erhalten stabile IDs und bereits resolved Damage-Mengen und binden neue Live-Kontexte gegen Runtime B. `DamageCommittedEventSnapshot` erhält dagegen nur die Fakten des bereits committed Events und benötigt beim Restore keinen erneuten Commit.

`SimulationRuntimeSessionSnapshot<TPayloadSnapshot>` führt Runtime- und Runner-Snapshot kohärent zusammen. Die Composition wird danach explizit neu für Runtime B erzeugt. Das ist eine Fortsetzungsgrenze, kein Savegame- oder Modul-Serialisierungsvertrag.

Die Snapshot-Grenze ist quiescent: kein aktives Scheduler-Event und keine offene Prepared-Follow-up-Reservation. Prepared-Objekte und laufende Callback-Stacks werden nicht serialisiert. `CompiledResourceRegistry` sowie kompatible Regeln/Definitionen werden im ersten Profil von außen bereitgestellt.

Der aktuelle Core hält keine persistenten RNG-Instanzen. Sollte ein Modul später einen langlebigen RNG halten, gehört dessen Zustand in dessen Domain-Snapshot.

PW-S06 bestätigt denselben Grundsatz für neue zeitabhängige Domain-Zustände: `TimedModifierStateSetSnapshot` beschreibt aktive und inaktive Slots, Generationen, Revision und Ablaufdaten. Restore rekonstruiert direkt und darf weder `ApplyOrRefresh` noch Scheduler-`Schedule` als Ersatz für Rehydration benutzen. Pending Expiration bleibt im Runner-Snapshot; ihre Gültigkeit wird nach Restore ausschließlich vom restaurierten Domain-State entschieden.

## 8. Replay-Beweis und seine Grenze

Der abschließende PW-S05-Test erzeugt einen Snapshot nach einem bereits ausgeführten Damage-Commit, während dessen `DamageCommittedEvent` sowie weiteres resolved Damage noch pending sind. Danach werden in Runtime A und der restaurierten Runtime B identische, geordnete neue Inputs eingebracht.

Der Vergleich umfasst den vollständigen Fortsetzungsstatus des Referenzprofils: Runner-Ergebnis, Scheduler-/Trace-Reihenfolge, Event-Fakten einschließlich generierter Gameplay-/Damage-/Hit-IDs, Resource-Current/Revision sowie die nach der Snapshot-Grenze neu entstandenen Ledger-Einträge und ihre Provenienz. Runtime A wird vollständig beendet, bevor Runtime B weiterläuft, damit versehentliche alte Handler-/Runtime-Bindungen sichtbar würden.

Dieser Beweis umfasst keine realen Physics-/Netzwerk-/Kauf-Side-Effects und keine aufgezeichneten Host Facts. Solche externen autoritativen Ergebnisse benötigen später einen eigenen Provider-/Recording-Vertrag.

PW-S06 ergänzt einen zweiten, unabhängigen Fortsetzungsfall: Der Domain-Snapshot wird mit einem Runner-Snapshot kombiniert, während Expiration-Arbeit pending ist. Nach Restore bleibt eine alte Generation stale und die aktuelle Generation läuft am vorgesehenen `StateBoundary` ab. Eine frische Composition bindet gegen den restaurierten Stats-State. Damit ist die S05-Fortsetzungsgrenze nicht auf Combat-Objekte beschränkt, ohne dass daraus bereits ein universeller Produktionscodec folgt.

## 9. Tests und Kompatibilität

Jeder Eingriff erhält einen Verhaltenstest vor oder zusammen mit der Änderung. Bestehende Assertions werden nicht entfernt, nur damit ein anderer interner Ablauf grün wird; geänderte Fachsemantik braucht eine ausdrückliche Entscheidung.

PW-S05 endete auf `d8826a2` mit 1248 Tests. PW-S06 endete auf `3ee638a`; der Nutzer bestätigte 1267/1267 Tests, vollständigen Build und anschließend Repository-Status `main...origin/main` ohne Änderungen (`gcc`).

Eine neue **öffentliche** Grenze bekommt weiterhin zusätzlich einen Test aus `Core.ExternalTests`. Die S05-Snapshot-Verträge sind aktuell intern/in-memory; ihre Tests liegen deshalb in `Core.Tests`.

Cross-Version-, Cross-Platform-, Cross-Engine- und Serializer-Kompatibilität werden nicht aus diesen In-process-Tests abgeleitet.

## 10. Nächster Repository-Schritt nach dieser Dokumentaktualisierung

Diese Dokumentaktualisierung verändert selbst keinen Produktionscode und führt keinen zusätzlichen .NET-Testlauf aus. Sie synchronisiert die Dokumente mit dem vom Nutzer bestätigten PW-S06-Funktionsstand `3ee638a`. Ein anschließender Dokumentationscommit erhält einen eigenen Hash und ersetzt den Funktionsnachweis nicht.

Der nächste reguläre Entwicklungsstrang ist **PW-S07 – definierte Lastprofile, Retention und gemessene Optimierung**. Vor Optimierung wird ein konkretes reproduzierbares Lastprofil mit Referenzsemantik, Hardware, Runtime, Buildmodus, Seed und Aktivitätsraten festgelegt. Erst gemessene Hot Paths rechtfertigen Batch-/Cache-/Fast-Path-Ausbau.

Weiterhin nicht vorgezogen werden JSON-/Binary-Saveformat, universeller Event Store, Object-Graph-Serializer, Cross-Version-Migration, Cloud-Saves oder ein 30-Sekunden-Scrubber.

**Quellen:** [E1–E5, E7–E12](SOURCE_EVIDENCE_v2_0.md).
