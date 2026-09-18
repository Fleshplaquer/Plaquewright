# Plaquewright – Implementierungsfolge 2.0

**Version:** 2.0 · **Datum:** 18. September 2026
**Ausgangspunkt:** Nutzerbestätigte Audit-Baseline `d39cb54`, nicht der alte Idler-P00-Start
**Status:** Freigegebene Architektur-Baufolge; PW-S02 abgenommen auf `b04fcbe`, PW-S03 auf `f90517a`, PW-S04 auf `8bd4dd0`; PW-S05 in Arbeit, aktueller bestätigter Zwischenstand 1239/1239 Tests und `gcc` (Commit-Hash hier nicht erfasst)

## 1. Ziel der Reihenfolge

Wir bauen das Zusammenspiel eines Frameworks, nicht zunächst ein vollständiges Idle-/ARPG-Regelwerk. Der Weg bleibt vertikal: Jeder Schritt schließt einen kleinen nutzbaren Ablauf über echte öffentliche Grenzen.

Die Architekturübergabe nennt die externe Transaction-Grenze noch als nächsten Schritt. Dieser technische Teil ist im inzwischen dokumentierten Anschlussstand bereits vorhanden. Er wird benutzt, nicht nochmals eingeführt. Die ältere Testzahl 1023 ist ebenfalls kein aktueller Baseline-Nachweis. [E1–E4]

Die neue Reihenfolge verwendet `PW-Sxx`, um weder alte P-Meilensteine noch abgeschlossene A/B-Findings umzunummerieren. Ein PW-Schritt ist eine neue geplante Fähigkeit, kein versteckt wieder eröffnetes Finding.

## 2. Überblick

| Schritt | Ergebnis | Warum jetzt? |
|---|---|---|
| PW-S00 | Framework-Ziel und Baseline nachvollziehbar dokumentiert | Verhindert weitere Arbeit nach falscher Produktvision |
| PW-S01 | Transaction-Komposition hat einen eindeutigen Konflikt-/Lebensdauervertrag | Ereignispublikation darf keine unklare Mutation verdecken |
| PW-S02 | Commit-Ergebnis → Event → deterministische Reaction **– abgenommen** | Schließt die zentrale modulübergreifende Ausführungskette |
| PW-S03 | Kleine explizite Modulkomposition und Headless-/Godot-Referenz **– abgenommen** | Beweist Benutzbarkeit außerhalb interner Tests |
| PW-S04 | Zweites fachliches Referenzszenario mit vorhandenen Combat-Bausteinen **– abgenommen** | Belegt, dass dieselbe Runtime auch result-backed Combat, PreDefeat und bezahlte Damage-Folgearbeit trägt |
| PW-S05 | Snapshot/Restore und deterministischer Replay-Beweis **– in Arbeit** | Prüft State-Ownership und ausstehende Arbeit früh |
| PW-S06 | Zeitabhängige Domain und abgeleitete Queries | Erweitert Status/Production und Cache-Invalidierung an realem Bedarf |
| PW-S07 | Definierte Lastprofile, Retention und gemessene Optimierung | Performance wird geprüft statt aus Architektur abgeleitet |
| PW-S08 | Exakte Zeitbeschleunigung, optionale Approximation und weitere Adapter | Baut auf belegten Semantik- und Replay-Grenzen auf |

Messungen beginnen bereits bei PW-S02/PW-S03; PW-S07 ist die systematische Lastabnahme, nicht der erste Blick auf Allokationen. Der genaue Inhalt späterer Schritte wird aus den vorangehenden Beweisen angepasst.

## 3. PW-S00 – Dokumentationsbaseline konsolidieren

**Lieferung:** Manifest, Baufolge, Abnahmematrix, Architekturkarte, Code-Klassifikation, Migrationsplan, Glossar und Quellenregister. D-01 und D-02 sind am 17. September 2026 beschlossen.

**Abnahme:** Jede Statusaussage unterscheidet Ziel, beobachteten Code, Testquellen und ausgeführte Tests. A01–A07/B01–B10 bleiben als bestätigte Historie dokumentiert. Ein späterer Dokumentationscommit ist nicht derselbe Commit wie `d39cb54`.

**Nicht enthalten:** Umbenennung der kompletten Quellstruktur, neue Engine-Adapter oder Änderung von Combat-Semantik.

## 4. PW-S01 – Vertrag für zusammengesetzte Änderungen präzisieren

Der vorhandene externe Coordinator ist die Grundlage. Vor einer allgemeinen Event-/Reaction-Schicht prüfen wir die Fälle, die erst durch neue Zusammensetzung relevant werden.

**Kleine Beweisszenarien:**

| Fall | Notwendiger Vertrag |
|---|---|
| Zwei Kosten gegen denselben Ressourcenbestand | Gemeinsame Vorbereitung oder Ablehnung vor Apply; keine doppelte Nutzung derselben Ausgangssumme |
| Mehrere vorbereitete Appends gegen dasselbe Ledger | Gemeinsame Kapazitäts-/Publikationssicherheit |
| Frisches und bereits verwendetes Prepared-Objekt in einem Aufruf | Gesamte Verwendbarkeit vor dem ersten Apply prüfen |
| Zugriff auf fremde Runtime/Owner | Vor jeder Veröffentlichung abweisen |
| Neue Transaction während Apply | Explizit nicht zulassen oder separat definiertes Protokoll; kein zufälliges Reentranzverhalten |

**Vorgehen:** Konkrete aktuelle Source- und Testkörper lesen, vorhandene Nachweise wiederverwenden, nur unbelegte Szenarien ergänzen. Die Tabelle behauptet nicht, dass alle Fälle im aktuellen Code fehlen oder fehlschlagen.

**Vorgeschlagene erste Einschränkung:** Ein Writer je Simulation und nicht-reentrante Publikation. Mehrere Änderungen innerhalb einer Domain dürfen in einem Participant gesammelt werden. Ein allgemeiner Optimistic-Concurrency- oder Rollback-Mechanismus wird nicht vorausgesetzt.

**Abnahme:** Unterstützte Kombinationen sind dokumentiert und getestet; nicht unterstützte Kombinationen scheitern vor sichtbarer Mutation. Ein technischer Apply-Fehler wird nicht als normale fachliche Ablehnung ausgegeben.

**Nicht enthalten:** Neue Datenbank, verteilte Transactions, Umbau aller bestehenden lokalen Committer.

## 5. PW-S02 – Commit-Ergebnis und deterministische Folgearbeit

**Status:** Abgenommen am 17. September 2026. Technischer Nachweisstand: `b04fcbe`.

Der externe Door-/Alarm-Fall beweist die erste vollständige Nicht-Combat-Kette:

```text
OpenDoorIntent
  -> Gold + Door Transaction
  -> Commit
  -> DoorOpened
  -> geordnete Reaction
  -> RaiseAlarmAction
  -> neuer autoritativer Alarm-Zustand
```

Der Referenzfall beweist dabei die Follow-up-Grenze; die Alarm-Action ist in PW-S02 noch keine zweite generische Transaction.

**Umgesetzte Grenze:**

1. `IDomainEvent` markiert einen bereits committed fachlichen Sachverhalt.
2. `IDomainReaction<TEvent, TWorkItem>` verarbeitet ein solches Ergebnis als neue Arbeit.
3. `DomainReactionContext<TWorkItem>` stellt den schmalen Follow-up-Pfad bereit.
4. `DomainReactionDispatcher<TEvent, TWorkItem>` verwendet eine beim Aufbau eingefrorene, explizite Reaction-Reihenfolge.
5. `DomainEventTransactionCoordinator.TryCommitAndPublish(...)` reserviert die benötigte Event-Kapazität vor dem Commit.
6. Prepared Scheduler-Follow-ups bleiben interne Publikationsmechanik.
7. Ein begonnener Timestamp wird für nachträglich eingebrachte externe Inputs geschlossen.
8. Unerwartete Reaction-Fehler führen zu Fail-stop; bereits committed State wird nicht zurückgerollt.

**Abgenommen:** Prepare-Ablehnung erzeugt weder Mutation noch Event; erfolgreiche Reactions beobachten vollständig committed State; fehlende Queue-Kapazität verhindert den Commit vor sichtbarer Mutation; Same-Time-Folgearbeit bleibt kausal geordnet; mehrere Reactions behalten ihre Kompositionsreihenfolge; ein Reaction-Fault lässt den vorherigen Commit bestehen und faultet die Runtime; wiederholtes `RunNext` und `RunToCompletion` liefern für das Referenzszenario denselben autoritativen Trace und State.

**Bewusst nicht eingeführt:** globaler EventBus, Reflection-Routing, dynamische Reaction-Prioritäten, Runtime-Hotregistration, universelles `CommitResult`, vollständiges Status-/Skill-System oder persistenter Event Store.

## 6. PW-S03 – Modulkomposition und zwei Host-Betriebsarten

**Status:** Abgenommen am 18. September 2026. Technischer Endstand: `f90517a`.

PW-S03 führt die in PW-S02 bewiesenen Primitiven in eine kleine host-neutrale Runtime-Grenze über.

**Umgesetzte Bausteine:**

1. `SimulationExecutionPlanBuilder<TWorkItem>` und `SimulationExecutionPlan<TWorkItem>` bilden einen beim Build eingefrorenen Dispatchplan. Routing erfolgt über den exakten Runtime-Typ; doppelte Handler werden bei der Komposition abgewiesen.
2. `ISimulationModuleContract`, `SimulationModuleBuilder<TWorkItem>`, `SimulationCompositionBuilder<TWorkItem>` und `SimulationComposition<TWorkItem>` beschreiben explizite Module mit Required/Provided-Contracts. Fehlende Provider und doppelte Provider scheitern vor dem ersten Run. Die Modulreihenfolge bleibt explizit; Dependencies führen nicht heimlich zu Reflection-Discovery oder automatischer Topological Sort.
3. `SimulationSession<TWorkItem>` kapselt Composition, Runner und Scheduler als host-neutrale Fassade. Sie delegiert externe Inputs, `RunNext` und `RunToCompletion` an dieselbe autoritative Core-Laufzeit und öffnet keinen zweiten Scheduler-Zugriff.
4. Headless wurden sowohl Door/Alarm als auch Resources/Door/Alarm mit bezahlter, atomarer Öffnung ausgeführt. Combat ist für beide Konfigurationen keine Pflichtabhängigkeit.
5. Die Godot-Hauptassembly referenziert `Plaquewright.Core` per `ProjectReference`. `src/Presentation/Main.cs` erzeugt eine Composition und `SimulationSession`, übergibt einen externen Host-Input und führt ihn über die Core-Laufzeit aus. `main.tscn` benötigt dafür keinen eigenen Gameplay-Pfad.

**Ausführungsnachweis:** Während PW-S03 wurde eine vollständige Testsummary mit **1210/1210** bestandenen Tests gezeigt. Nach dem Godot-Adapter bestätigte der Nutzer erneut Tests und Build als grün. Zusätzlich wurde ein echter Godot-Start ausgeführt; die Ausgabe lautete `Plaquewright Godot host ready. SimulationTime=0us, ProcessedEvents=1.`. Der abschließende Repository-Stand `f90517a` wurde mit `gcc` bestätigt.

**Abgenommen:** Das externe Modul benötigt keine neuen Kernel-Fachbegriffe und kein `InternalsVisibleTo`; fehlende oder doppelte Contracts werden vor Run abgewiesen; die Session bewahrt die D-04-Inputgrenze; Headless und Godot benutzen dieselbe `SimulationSession`-/Core-Autorität statt zwei Implementierungen. Der Godot-Nachweis ist ein Host-Smoke-Test, kein Physics-/Replay-Paritätstest.

**Migrationsergebnis:** `SimulationRuntimeState` musste für PW-S03 nicht vorsorglich umgebaut werden. Die neue generische Host-/Composition-Grenze entstand daneben als schmale Infrastruktur; die bestehende Gameplay-Komposition kann bis zu einem realen Blocker erhalten bleiben.

**Nicht enthalten:** Automatisches Plugin-Discovery, Runtime-Hotloading, Service Locator, dynamische Contract-Prioritäten, Assembly-Split jedes Verzeichnisses, vollständiges ECS, Host-Fact-Replay oder eine Cross-Engine-Kompatibilitätsmatrix.

## 7. PW-S04 – Zweites Referenzszenario: vorhandenes Combat nutzen

**Status:** Abgenommen am 18. September 2026. Technischer Endstand: `8bd4dd0`.

PW-S04 führt vorhandene Combat-Bausteine über die in PW-S02/PW-S03 etablierte Work-/Transaction-/Event-/Reaction-Grenze, ohne Combat-Fachbegriffe in den Kernel zu verschieben.

**Umgesetzte beziehungsweise bestätigte Bausteine:**

1. `ISimulationWorkItem` markiert produktive autoritative Work Items. `IDomainEvent` ist ein solches Work Item. Die niedrigeren generischen Scheduler-/Runner-/Composition-Typen bleiben bewusst ohne `ISimulationWorkItem`-Generic-Constraint, damit ihre Infrastruktur generisch und separat testbar bleibt.
2. Die interne Prepared-Follow-up-Grenze reserviert vor dem Commit nur Scheduler-Kapazität, Schlüssel und Reihenfolge. Der tatsächliche Payload kann nach dem Commit aus dem echten Ergebnis erzeugt und anschließend in die Reservation publiziert werden.
3. `DamageCommittedEvent` ist ein produktiver Combat-Fakt nach erfolgreichem Resource-Commit. Er trägt stabile Damage-/Gameplay-Execution-, Source-/Target-, Hit- und Zeitinformation sowie das tatsächliche Defeat-aware Commit-Outcome, aber keine beliebige Resource- oder PreDefeat-Implementierungsstruktur.
4. `ApplyResolvedDamageAction` bildet die produktive Grenze „Damage ist fachlich resolved und soll nun autoritativ angewendet werden“. Resource-Routing bleibt Regel-/Kompositionswissen.
5. `ResolvedDamageApplicationExecutor` orchestriert vorhandene `DamageResourceLossPlan`-Bausteine, optionale PreDefeat-Interventionen pro Owner und den bestehenden Defeat-aware Resource-Commit. Er unterstützt mehrere Loss-Pläne und Owner, statt Combat auf genau einen Health-Pool zu reduzieren.
6. `DamageApplicationOwnerPlan` und `ResolvedDamageApplicationResult` bleiben Combat-interne Orchestrierungsverträge; daraus wurde kein allgemeiner Kernel-Service oder globaler Combat-Service gebaut.

**Vertikaler Nachweis A – lethal Damage mit PreDefeat:**

```text
DamageResolution
  -> ApplyResolvedDamageAction
  -> Resource-Loss-Projektion
  -> PreDefeatRecovery verändert nur den Draft
  -> Defeat-aware Commit
  -> DamageCommittedEvent aus dem tatsächlichen Commit-Ergebnis
  -> Reaction
  -> ObserveCommittedDamageAction
```

Der Test belegt unter anderem: autoritativer Life-State bleibt während der Projection unverändert; PreDefeat kann den noch uncommitteten Draft von lethal auf überlebt verändern; der Event wird erst nach sichtbar gewordenem Commit verarbeitet; Damage- und Recovery-Ledger-Einträge behalten getrennte Gameplay-Provenienz. Ein `DamageCommittedEvent` kann nicht aus einer Resolution und einem Commit-Ergebnis verschiedener Target-Entities gebildet werden.

**Negative Executor-Grenzen:** Ein Resource-Loss-Plan aus einer anderen Damage-Resolution wird vor Mutation abgewiesen. Fehlt der Owner der tatsächlich betroffenen Target-Entity, verhindert die bestehende Defeat-aware Ownership-Prüfung den Commit; State, Revision und Ledger bleiben unverändert.

**Vertikaler Nachweis B – bezahlter Angriff:**

```text
PayAttackCostAction
  -> Cost Transaction
  -> AttackCostCommittedEvent
  -> Reaction
  -> ResolvePaidAttackDamageAction
  -> ApplyResolvedDamageAction
  -> Damage Resource Commit
  -> DamageCommittedEvent
  -> Reaction
```

Der Test belegt ausdrücklich, dass die Kosten vor der Damage-Auflösung committed sind und fachlich getrennt bleiben: Cost erzeugt einen `ResourceCostLedgerEntry`, Damage einen `ResourceLossLedgerEntry`. Die kausale Folgearbeit läuft bei gleichem Timestamp über fortschreitende Waves.

**Abgenommen:** PW-QA-15, PW-QA-16 und PW-QA-22 für den beschriebenen S04-Referenzumfang. Der Kernel bleibt fachneutral; Pre-Commit und Post-Commit sind getrennt; Cost und Damage bleiben verschiedene Operationen; dieselbe Architektur trägt Door und Combat.

**Ausführungsnachweis:** Nach den inkrementellen S04-Schritten bestätigte der Nutzer jeweils grüne Test-/Build-Läufe. Der finale Stand `8bd4dd0` wurde nach **1215/1215** bestandenen Tests mit `gcc` bestätigt.

**Bewusst nicht eingeführt:** globaler `CombatService`, universelles Intervention-Interface, automatische Resource-Routing-Regel, vollständiges Skill-/Cooldown-/Channel-System, Combat-Abhängigkeit im Kernel oder eine neue allgemeine EventBus-Schicht.

## 8. PW-S05 – Früher Snapshot-/Replay-Beweis

**Status:** In Arbeit. Aktueller bestätigter Zwischenstand: **1239/1239** Tests grün, anschließend `gcc`. Der konkrete Commit-Hash dieses S05-Blocks wurde in der vorliegenden Dokumentpflege nicht festgehalten.

PW-S05 beginnt bewusst mit einem in-memory Restore-Modell statt mit JSON, Savegame-Format oder Cross-Version-Migration. Der Leitgedanke lautet:

> Snapshot ist kein Object-Graph-Clone, sondern beschreibbarer autoritativer Zustand, aus dem eine neue Runtime rekonstruiert werden kann.

**Bereits umgesetzt und getestet:**

1. `ResourceState` und `ResourceStateSet` snapshotten Current/Maximum/Revision und restaurieren unabhängig weiterlaufende Instanzen.
2. `EntityRuntimeState` und `EntityRuntimeStateSet` erhalten Entity-IDs, Resource-State und deterministische Reihenfolge.
3. Entity-, Execution-, Hit- und Damage-ID-Allocator erhalten ihre Fortsetzungs- beziehungsweise Exhaustion-Positionen.
4. `SimulationRuntimeStateSnapshot` bündelt Seed, Entity-/Resource-State und Allocatorstände. Restore erhält eine **neue** `SimulationRuntimeIdentity`, statt die alte Referenzidentität zu konservieren.
5. `SimulationSchedulerSnapshot<TPayloadSnapshot>` erhält Limits, ursprüngliche `ScheduledEventKey`s, Sequenzposition und Pending Work. Capture ist nur quiescent erlaubt: kein aktives Event und keine offene Prepared-Follow-up-Reservation. Restore benutzt nicht erneut `Schedule(...)`, weil dadurch neue Sequenzen entstehen würden.
6. `SimulationRunnerSnapshot<TPayloadSnapshot>` erhält `CurrentTime`, `ProcessedEvents`, Runner-Limit und `ExternalInputsClosedThrough`, sodass D-04 nach Restore nicht wieder geöffnet wird. Faulted- oder terminal-budgeted Runtimes werden im ersten Profil nicht als fortsetzbare Snapshots akzeptiert.
7. Der aktuelle Core speichert keine langlebigen RNG-Instanzen. `GameplayExecutionContext` erzeugt deterministische Streams aus Root Seed und Kontext; deshalb gibt es im aktuellen S05-Scope keinen separaten persistenten RNG-Snapshot. Ein späterer Domain-eigener langlebiger RNG müsste seinen Zustand selbst snapshotten.
8. `DamageResolutionSnapshot` und `ApplyResolvedDamageActionSnapshot` bilden den ersten realen pending Combat-Work-Fall. IDs und bereits resolved Damage-Mengen werden beschrieben; runtime-gebundene Context-Objekte werden gegen die neue Runtime rekonstruiert, ohne IDs neu zu allozieren oder Combat-Regeln erneut aufzulösen.
9. Ein Integrationstest führt denselben pending `ApplyResolvedDamageAction` einmal in Runtime A direkt und einmal über `Snapshot -> Runtime B -> Restore` fort. Scheduler-Key, Resource-State/Revision und Ledger-Ergebnis/Provenienz stimmen überein.

**Damit bereits teilweise belegt:** PW-QA-17, PW-QA-18 und die aktuelle Scheduler-Ausprägung von PW-QA-28. PW-S05 als Ganzes bleibt offen.

**Nächster kleiner Block:** Die Pending-Work-Snapshot-Grenze über den einzelnen Combat-Work-Item-Typ hinaus generalisieren, ohne Reflection-Service-Locator oder vorschnellen Serializer. Danach einen höheren Runtime-/Session-Snapshot bilden und einen vollständigen Replay-Beweis mit weiteren geordneten Inputs beziehungsweise Host Facts und relevantem Trace durchführen.

**Weiterhin nicht enthalten:** fertiges Savegame-Format, JSON-Vertrag, allgemeiner Event Store, Cross-Version-Migration, Cloud-Saves, 30-Sekunden-Scrubber oder automatische Wiederholung realer Host-Side-Effects.

## 9. PW-S06 – Zeitabhängige Domains und abgeleitete Werte

Jetzt ergänzt ein konkreter Mechanismus die bisherigen Punkt-Events: beispielsweise eine Produktionsphase oder ein Status mit Tick und Ablauf. Das gewählte erste Szenario wird nach PW-S04 bestimmt.

**Lieferung:** Domain-eigener Zustand, typisierte Queries, definierte Zeitgrenzen sowie Invalidierung abgeleiteter Werte. Ein Status, der Resistance beeinflusst, wird bei seiner eigenen Zustandsänderung relevant; ein Combat-Resolver liest anschließend die passende Sicht.

**Abnahme:** Kein Full-World-Scan ohne Bedarf, kein Observer als rückwirkende Reparatur von State. Ablauf, gleiche Timestamps, Cancellation/Versionierung und Cache-/Referenzvergleich sind explizit geprüft.

**Zusammenführung:** Weitere Skills, Inventory, Equipment und neue Regelprofile kommen als eigenständige vertikale Fälle hinzu. Dieses Dokument schreibt dafür keine universell richtige Genre-Reihenfolge vor.

## 10. PW-S07 – Lastprofile und Optimierung

Die Architekturübergabe nennt ungefähr 500 individuell simulierte Gegner sowie große AoEs als Ziel-/Lastfälle. Das wird in ausführbare Profile übersetzt, sobald die benötigten Mechanismen existieren.

**Profile:** Ruhende Welt, häufige kleine Änderungen, viele gleichzeitige Kontakte, gemeinsam finanzierte Effekte, tiefe Reaction-Kette, lange Laufzeit mit begrenzter History. Jede Messung nennt Hardware, Runtime, Buildmodus, Seed, Definitionen und Aktivitätsraten.

**Abnahme:** Median und hohe Perzentile, Allokationen, GC, Queue-/Ledger-/Trace-Speicher und autoritative Durchsatzgrenzen sind dokumentiert. Erst danach werden konkrete Hot Paths optimiert.

**Semantikprüfung:** Referenzpfad gegen Batch-/Cache-/Fast-Path vergleichen. Ein schnellerer Endsaldo allein reicht nicht, wenn Zwischenereignisse, Ownership oder RNG-Nutzung verschieden sind.

**Nicht enthalten:** Eine erfundene pauschale Garantie „500 Gegner auf jeder Hardware“ oder obligatorische Horde-Aggregation.

## 11. PW-S08 – Zeitbeschleunigung und weitere Integrationen

Exact Fast-forward verwendet nachgewiesene modulare Grenz-/Integrationsverträge. Approximation ist ein separates Profil mit dokumentiertem Fidelity-Verlust. Fehlt ein geeigneter Provider, wird der nicht unterstützte Fall benannt.

Weitere Engines werden am bereits laufenden Headless-Vertrag integriert. Physics/Collision bleiben explizite Adapter-/Providerfragen; ein neuer Host ist nicht automatisch dieselbe räumliche Simulation.

Visual Tooling, Modding-Distribution, große Content-Pipelines und Multiplayer folgen tatsächlichem Bedarf und eigenen Abnahmen. C#-Module als Erweiterung sind nicht mit einer sicheren Runtime-Mod-Sandbox gleichzusetzen.

## 12. Definition of Done für jeden Schritt

Ein Schritt ist abgeschlossen, wenn sein zugesagter Anwendungsfall durch die vorgesehene öffentliche Grenze funktioniert, passende positive und negative Tests bestanden sind, Regressionstests grün sind und Soll-/Ist-Grenzen dokumentiert wurden.

Bei Codeänderungen bleiben `dotnet test`, `dotnet build`, Diff-Prüfung und ein klarer Commit Teil des Arbeitsablaufs. Ein reiner Dokumentationsentwurf wird nicht als neuer bestandener C#-Testlauf ausgegeben.

Die Arbeit endet an einem nutzbaren Zwischenstand. Ein offen gebliebenes Folgefeature wird weder versteckt noch durch eine neue Generalschicht ersetzt.

**Quellen:** [E1–E5, E7–E10](SOURCE_EVIDENCE_v2_0.md).
**Detailverträge:** [Manifest](Plaquewright_Manifest_v2_0.md).
**Test-IDs:** [Abnahmekatalog](Plaquewright_Freigabe_und_Testnachweise_v2_0.md).
