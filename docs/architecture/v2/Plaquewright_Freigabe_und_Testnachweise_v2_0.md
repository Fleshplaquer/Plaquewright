# Plaquewright – Freigabe, Baseline und Testnachweise 2.0

**Version:** 2.0 · **Datum:** 18. September 2026  
**Dokumentstatus:** Architektur 2.0 angenommen; neue Funktions-/Release-Freigaben bleiben an konkrete Code- und Testnachweise gebunden  
**Historische Audit-Baseline:** `d39cb54`, vom Nutzer als grün, committed und clean bestätigt  
**PW-S02-Nachweisstand:** `b04fcbe`, vom Nutzer als grün, committed und clean bestätigt  
**PW-S03-Nachweisstand:** `f90517a`, vom Nutzer als grün, committed und clean bestätigt

## 1. Was tatsächlich bestätigt ist

Der Nutzer zeigte folgenden Git-Stand:

```text
d39cb54 (HEAD -> main, origin/main) Enforce prepared publication API boundary
e71fd4b Align invariant tests with architecture contracts
ab292d5 Enforce scheduler external input boundary
```

Dazu: Branch `main`, mit `origin/main` synchron, Working Tree clean; `gcc` wurde als grün, committed und clean erklärt. Der Auditanschluss wurde im Gespräch mit A01–A07 und B01–B10 abgeschlossen. [E2]

| Bereich | Anschlussstatus | Nachweisgrenze |
|---|---|---|
| A01–A07 | Im Projektverlauf als erledigt bestätigt | Die ausführlichen einzelnen Finding-Texte liegen diesem Dokument nicht vollständig vor |
| B01–B06 | Im Projektverlauf als erledigt bestätigt | Keine nachträglich erfundenen Bezeichnungen oder Einzelabnahmen |
| B07 | Ledger causal execution linkage abgeschlossen | Besprochene Änderungen und Testquellen plus Nutzerbestätigung |
| B08 | Scheduler input boundary abgeschlossen | Kein automatischer Nachweis eines vollständigen neuen Zeitordnungsprofils |
| B09 | Test-Invariant-Audit im vereinbarten Umfang abgeschlossen | Testnamenlisten sind kein erschöpfender Beweis aller Kombinationen |
| B10 | Prepared publication rights / API boundary abgeschlossen | Keine Sandbox- oder universelle Rollback-Garantie |
| PW-S02 | Abgenommen | Commit → Domain Event → deterministische Reaction einschließlich Queue-Reservation, Input-Grenze, Reaction-Ordering, Fail-stop und RunNext/RunToCompletion-Äquivalenz |
| PW-S03 | Abgenommen | Expliziter Execution Plan, Required/Provided-Modulkomposition, host-neutrale `SimulationSession`, zwei Headless-Konfigurationen und echter Godot-Smoke-Run über dieselbe Core-Autorität |
| Build / Tests | Laut finaler Nutzerbestätigung grün | In diesem Dokumentationsdurchgang nicht neu ausgeführt |
| Repository | Laut gezeigtem Git-Status clean und synchron | Kein eigener Live-Abruf des Remotes für diese Dokumentation |

Eine vollständig gezeigte frühere Testausgabe enthält **1176 bestandene Tests** nach dem Pre-Defeat-Teil. Im PW-S02-Durchgang wurde zwischenzeitlich eine vollständige Summary mit 1196 Tests gezeigt, davon zunächst ein erwartungsbedingt fehlschlagender neuer Reservation-Test; nach Korrektur der Runner-Erwartung sowie den weiteren Ordering-/Fault-/Äquivalenztests bestätigte der Nutzer die jeweiligen vollständigen Läufe mit `g` und den abschließenden Repository-Stand mit `gcc`.

Für PW-S03 wurde anschließend eine vollständige Summary mit **1210 bestandenen Tests, 0 fehlgeschlagenen Tests** gezeigt. Nach Einbau des Godot-Adapters bestätigte der Nutzer den erneuten Test-/Build-Durchgang als grün und führte zusätzlich den Godot-Host aus. Dessen Output bestätigte `SimulationTime=0us` und `ProcessedEvents=1`. Der abschließende Stand `f90517a` wurde mit `gcc` bestätigt. Da nach der gezeigten 1210-Summary keine weiteren Testdateien hinzukamen, wird 1210 als bestätigter S03-Testumfang geführt; dies ist keine Performance-, Replay- oder Cross-Platform-Freigabe.

Die Architekturübergabe mit 1023 Tests beschreibt einen älteren technischen Stand. Sie ist die aktuelle Quelle der Produktvision, nicht der jüngere technische Nachweis. [E1, E2]

## 2. Was diese Dokumentation selbst ausgeführt hat

Der ursprüngliche v2-Dokumentationsdurchgang las die bereitgestellten Quellen und erzeugte die Architekturdateien ohne Produktionsänderung. **Seitdem** wurden PW-S02 und PW-S03 im Nutzerrepository implementiert und getestet. Der Nutzer bestätigte die S02-Codeänderungen einschließlich PW-QA-12 auf `b04fcbe` sowie den abgeschlossenen S03-Stand `f90517a` mit `gcc`.

Diese aktuelle Dokumentaktualisierung führt selbst **keinen neuen .NET-Testlauf, Godot-Start oder Benchmark** aus und verändert das Nutzerrepository nicht. Sie übernimmt die im Projektgespräch gezeigten/bestätigten S03-Nachweise, während `d39cb54` die historische A/B-Audit-Baseline und `b04fcbe` der S02-Zwischenstand bleiben. Einzelheiten stehen im [Quellenregister](SOURCE_EVIDENCE_v2_0.md).

## 3. Statussprache für künftige Nachweise

| Status | Bedeutung |
|---|---|
| Geplant | Vertrag/Testfall beschrieben, keine Implementierung behauptet |
| Implementiert | Passender Code liegt vor, Ausführungsnachweis noch separat |
| Testquelle geprüft | Setup, Assertions und Scope gelesen |
| Ausgeführt | Ergebnis für konkreten Commit und konkrete Umgebung vorhanden |
| Abgenommen | Vereinbarter Scope erfüllt; verbleibende Grenzen dokumentiert |

Bestandene Beispieltests beweisen ihren Fall. Property-/Permutationstests erweitern die geprüfte Menge, machen daraus aber keine unbegrenzte Vollständigkeitsgarantie. Cross-Platform-, Performance- und Langzeitbehauptungen benötigen eigene Nachweise.

## 4. Neuer Abnahmekatalog

Die folgenden `PW-QA`-IDs sind **künftige Abnahmen der freigegebenen Architektur**, keine neue Liste bereits fehlgeschlagener A/B-Findings.

| ID | Invariante / Szenario | Erwarteter Nachweis | Zuordnung |
|---|---|---|---|
| PW-QA-01 | Externes Modul ohne Friend-Zugriff | Modul kompiliert und arbeitet nur über öffentliche Verträge; bestehende ExternalTests wiederverwenden | PW-S01–S03 |
| PW-QA-02 | Modulare Komposition ohne Combat | Door/Alarm läuft ohne Combat-Pflichtzustand | PW-S03 |
| PW-QA-03 | Prepare-Ablehnung bleibt unsichtbar | State, Revisionen, Ledger und neu hinzugefügte Events unverändert | PW-S01–S02 |
| PW-QA-04 | Zusammengesetzte Kosten teilen einen Bestand | Gemeinsame Reservierung oder Ablehnung vor irgendeinem Apply | PW-S01 |
| PW-QA-05 | Gemeinsames Ledger mehrerer Participants | Kein verlorener Append und kein normaler Kapazitätsfehler nach erster Mutation | PW-S01 |
| PW-QA-06 | Gemischte Prepared-Lebensdauern | Frisch + verbraucht in beiden Reihenfolgen vor erster Veröffentlichung kontrolliert | PW-S01 |
| PW-QA-07 | Commit-Reentranz und Fremdzuordnung | Kein geschachtelter Apply und kein fremder Runtime-/Owner-Zugriff | PW-S01 |
| PW-QA-08 | Ereignis nach vollständigem Commit | Observer sieht alle beteiligten neuen Zustände, nicht einen Zwischenstand | PW-S02 |
| PW-QA-09 | Reaction ist neue Arbeit | Kein rekursiver Commit im laufenden Apply; eindeutiger Ursachenbezug | PW-S02 |
| PW-QA-10 | Begrenzte autoritative Eventübernahme | Ablehnung vor Mutation oder gesichertes Pending-Ergebnis; kein stiller Verlust | PW-S02 |
| PW-QA-11 | Zeitordnung bei gleicher Zeit | Späte externe Eingabe und Zero-delay-Child springen nicht vor abgeschlossene Arbeit/Ursachen | PW-S02 |
| PW-QA-12 | Wiederholtes RunNext entspricht Gesamtaufruf | Identischer autoritativer Trace und State im gleichen Profil | PW-S02–S03 |
| PW-QA-13 | Reaction-Fault ist kein rückwirkender Abort | Vorheriger Commit bleibt als committed erkennbar; definierter Fault-/Fortsetzungsstatus | PW-S02 |
| PW-QA-14 | Headless und Godot verwenden dieselbe Autorität | Beide Hosts treiben dieselbe `SimulationSession`-/Core-Laufzeit; bei hostabhängigen Fakten folgt die strengere Ergebnisparität mit aufgezeichneten gleichen Inputs/Host Facts | PW-S03–S05 |
| PW-QA-15 | Pre-Commit-Regel versus Post-Commit-Reaction | Prevention verändert Plan; spätere Reaction verändert nur neue Arbeit | PW-S04 |
| PW-QA-16 | Unterschiedliche Regelkompositionen | Zweites fachliches Szenario benötigt keine Kernel-Fachbegriffe | PW-S04 |
| PW-QA-17 | Snapshot enthält ausstehende Arbeit | Restore erhält Events, Scheduler, RNG, IDs und Domainzustände im zugesagten Scope | PW-S05 |
| PW-QA-18 | Replay-Verlauf entspricht Fortsetzung | Nicht nur Endsaldo, sondern relevante Ereignisse und Ursachebezüge stimmen | PW-S05 |
| PW-QA-19 | Analyse wiederholt keine realen Side-Effects | Keine erneuten realen Host-/Netzwerk-/Kaufaktionen beim Replay | PW-S05 |
| PW-QA-20 | Cache-/Zeitgrenzen bleiben semantisch korrekt | Referenzrechnung gegen invalidierte Queries, Ticks, Ablauf und Versionen | PW-S06 |
| PW-QA-21 | Trace an/aus verändert keine Simulation | State, Gameplay-IDs und RNG-Verwendung unabhängig vom Debug-Level | PW-S02–S07 |
| PW-QA-22 | Ressourcenbilanz und Provenienz im Ausbau | Vorhandene B07-Fälle erweitern, keine neue globale „letzte Execution“-Zuordnung | PW-S04 |
| PW-QA-23 | Referenz-/Batch-/Fast-Path stimmen überein | Mengen, Reihenfolge, Ownership und weitere beobachtbare Regeln bleiben äquivalent | PW-S07–S08 |
| PW-QA-24 | Dauerbetrieb hat begrenzten Speicher | Retention und Queue-Wachstum mit explizitem Profil messen | PW-S07 |
| PW-QA-25 | Exact und Approximate bleiben unterscheidbar | Modus und Grenzen sichtbar; Budgetende ändert nicht heimlich Fidelity | PW-S08 |
| PW-QA-26 | Version-/Plattformversprechen ist ehrlich | Unterstützte Matrix nennen und ausführen; nicht geprüfte Kombinationen nicht freigeben | PW-S05–S08 |
| PW-QA-27 | Autoritative Host Facts sind replaybar | Externe gameplayrelevante Ergebnisse werden geordnet erfasst oder über einen deterministischen Provider reproduziert; Core-Verlauf bleibt gleich | PW-S03–S05 |
| PW-QA-28 | Snapshot nur an quiescent boundary | Kein Snapshot mitten in Apply/halbem Commit; Restore benötigt keine Prepared-Objekte oder laufenden Callback-Stack | PW-S05 |

Nicht jede ID erfordert eine neue Datei. Bestehende Tests werden zuerst zugeordnet. Ein neuer Test benötigt Assertionen über die relevante Zustandsgrenze, nicht nur den erwarteten Exception-Typ.

### 4.1 Abgenommene PW-S02-Nachweise

Für PW-S02 sind folgende Abnahmen auf dem bestätigten Stand `b04fcbe` erfüllt:

| ID | Status | Wesentliche Testquelle |
|---|---|---|
| PW-QA-03 | Abgenommen für den Door-/Resource-Fall | `RejectedDoorTransaction_ProducesNoDomainEventOrReaction` |
| PW-QA-08 | Abgenommen | `CommittedDoorTransaction_ProducesDeterministicFollowUpReaction` |
| PW-QA-09 | Abgenommen | `DoorOpened` erzeugt über eine Reaction neue Scheduler-Arbeit statt rekursiver Mutation im Commit |
| PW-QA-10 | Abgenommen | `DomainEventReservationFailure_PreventsTransactionCommit` sowie `PreparedScheduledFollowUpTests` |
| PW-QA-11 | Abgenommen für das aktuelle Scheduler-Profil | `ExternalInput_AtStartedTimestamp_IsRejectedWithoutFaultingRunner` und Same-Time-Wave-Nachweise |
| PW-QA-12 | Abgenommen für das Door-/Alarm-Referenzprofil | `DoorEventPipeline_RunNextAndRunToCompletionProduceSameAuthoritativeResult` |
| PW-QA-13 | Abgenommen | `ReactionFailure_FaultsRunnerWithoutRollingBackCommittedTransaction` |

`PW-QA-21` bleibt als späterer, schrittübergreifender Tracing-Nachweis offen. PW-S02 führt noch kein vollständiges konfigurierbares Gameplay-Tracing ein.

### 4.2 Abgenommene PW-S03-Nachweise

Für PW-S03 sind auf dem bestätigten Endstand `f90517a` folgende Abnahmen erfüllt:

| ID | Status | Wesentliche Test-/Laufquelle |
|---|---|---|
| PW-QA-01 | Abgenommen für die S03-Kompositionsgrenze | `Core.ExternalTests` baut und verwendet Execution Plan, Modulkomposition und Session ausschließlich über öffentliche Verträge |
| PW-QA-02 | Abgenommen | Headless Door/Alarm sowie Resources/Door/Alarm laufen ohne Combat-Pflichtzustand |
| PW-QA-12 | Weiterhin erfüllt | `SimulationSession.RunNext()` verwendet dieselbe eingefrorene Composition über mehrere Schritte; die strengere RunNext/RunToCompletion-Äquivalenz wurde bereits in PW-S02 für das Door-/Alarm-Profil nachgewiesen |
| PW-QA-14 | Abgenommen für die aktuelle Host-Autoritätsgrenze | Headless und Godot verwenden denselben `SimulationSession`-/Core-Pfad; echter Godot-Smoke-Run bestätigt Assembly-Referenz und Runtime-Ausführung |

Zusätzlich belegen die S03-Tests, dass doppelte Handler, fehlende Required-Contracts und doppelte Provider bereits bei der Komposition abgewiesen werden und dass eine gebaute Execution-Plan-/Composition-Sicht durch spätere Builder-Änderungen nicht rückwirkend verändert wird.

`PW-QA-27` bleibt offen: Der Godot-Smoke-Run bringt einen einfachen externen Host-Input ein, aber noch keine gameplayrelevanten Physics-/Collision-Host-Facts mit Replay-Aufzeichnung oder deterministischem Provider. Ebenso ist PW-QA-14 keine Behauptung einer engineübergreifenden Physics- oder Render-Parität.

PW-S02 bleibt auf `b04fcbe` als eigener Zwischenstand dokumentiert. Für PW-S03 liegt dagegen eine explizit gezeigte Testsummary mit 1210/1210 sowie die nachfolgende grüne Godot-Adapter-/Build-Bestätigung vor; beide Nachweisstufen werden nicht miteinander vermischt.

## 5. Mindestinhalt eines Test-/Release-Belegs

Ein Beleg nennt Commit, betroffene Module und Regelversionen, Buildkonfiguration, Runtime und Plattform, tatsächlich ausgeführte Befehle, Ergebnis sowie bewusst nicht geprüfte Aspekte.

Für Referenzszenarien werden Ausgangszustand und geordnete Inputs festgehalten. Für Performance kommen Hardware, Warm-up, Messdauer, Ereignisraten, Verteilung und Speicherprofil hinzu. Ein kleiner Fixture-Test ist kein 500-Gegner-Benchmark.

Authoring-/Replay-Versionen und Migrationsverhalten werden erst als unterstützt bezeichnet, wenn ihr Roundtrip beziehungsweise Fehlerfall nachgewiesen ist.

## 6. Freigabe der Dokumentversion

D-01 und D-02 wurden am 17. September 2026 ausdrücklich angenommen. D-04 und D-05 wurden anschließend im PW-S02-Durchgang entschieden und nachgewiesen. PW-S03 belegt Headless und Godot als Referenz-Betriebsarten über dieselbe Core-Autorität; D-07 bleibt dennoch offen, weil Paket-/Distributionsumfang, unterstützte Host-Versionen und spätere Adapter noch nicht als Releasevertrag entschieden sind. D-03, D-06 und D-07 bleiben offene technische Detailgates.

**Architekturfreigabe 2.0:** erteilt am 17. September 2026.  
**D-01 Determinismus/Replay:** beschlossen.  
**D-02 Combat-Referenzpaket:** beschlossen.  
**D-04 Same-Timestamp-Input-Grenze:** beschlossen und in PW-S02 nachgewiesen.  
**D-05 Event-Publikation und Reaction-Fault-Politik:** beschlossen und in PW-S02 nachgewiesen.  
**PW-S02:** abgenommen auf `b04fcbe`.  
**PW-S03:** abgenommen auf `f90517a`; Headless-Komposition und echter Godot-Smoke-Run bestätigt.  
**Bestätigter S03-Testumfang:** 1210/1210; danach Godot-Adapter-Test/Build erneut grün bestätigt.  
**Offene Detailgates:** D-03, D-06 und D-07.  
**Historischer A/B-Abschluss `d39cb54`:** bleibt unverändert dokumentiert.

Eine Dokumentationsannahme ersetzt keinen Code-Nachweis; die hier genannten PW-S02-/PW-S03-Status beruhen auf den vom Nutzer bestätigten Code-, Test- und Host-Läufen. Snapshot/Replay, aufgezeichnete Host-Fact-Parität, Cross-Platform-/Physics-Äquivalenz und Performance bleiben eigene spätere Freigaben.
