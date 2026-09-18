# Plaquewright – Architektur 2.0

**Stand:** 18. September 2026  
**Historische Audit-Baseline:** `d39cb54`  
**PW-S02-Nachweis:** `b04fcbe`, vom Nutzer als grün / committed / clean bestätigt  
**PW-S03-Nachweis:** `f90517a`, vom Nutzer als grün / committed / clean bestätigt  
**PW-S04-Nachweis:** `8bd4dd0`, vom Nutzer nach 1215/1215 Tests als grün / committed / clean bestätigt  
**PW-S05-Nachweis:** `d8826a2`, vom Nutzer nach vollständigem Regressionstest und Build als grün / committed / clean bestätigt; finaler S05-Testumfang 1248 Tests  
**PW-S06-Nachweis:** `3ee638a`, vom Nutzer nach 1267/1267 Tests und Build als grün / committed / clean bestätigt  
**Dokumentstatus:** Architektur 2.0 angenommen; D-01, D-02, D-04 und D-05 beschlossen; PW-S02 bis PW-S06 abgenommen

## Was diese Fassung festlegt

Plaquewright ist ein engine-unabhängiges, deterministisches und modulares Gameplay-/Simulations-Framework für C#. Ein einzelnes Idle-Spiel und dessen alte P-Meilensteine bestimmen nicht mehr automatisch die Baufolge. Godot ist erster Referenzhost; autoritative Gameplay-Regeln bleiben im C#-Framework beziehungsweise in ausdrücklich komponierten Domains und Regelpaketen.

Die bestehende Implementierung und der A/B-Auditabschluss werden weiterverwendet. Insbesondere wird die bereits vorhandene externe Transaction-Grenze nicht erneut als fehlende Erstimplementierung eingeplant.

## Beschlossene Richtungsentscheidungen

**D-01 – Determinismus und Replay:** Plaquewright garantiert deterministische autoritative Gameplay-Simulation innerhalb eines kompatiblen Ruleset-/Modul-/Ausführungsprofils. Gameplayrelevante Ergebnisse externer Systeme wie Engine-Physics werden über definierte Provider oder geordnete Host Facts eingebracht und für vollständiges Replay soweit nötig aufgezeichnet. Cross-Version-, Cross-Plattform- und Cross-Engine-Bitgleichheit sind keine v2.0-Grundgarantie.

**D-02 – Combat:** Die bestehenden Combat-Regeln bleiben ein erstklassiges, austauschbares Referenz-Regelpaket und Belastungstest für komplexe Komposition. Combat ist keine Kernel-Abhängigkeit. Combat-spezifische Abstraktionen werden erst dann generalisiert, wenn mindestens ein weiterer unabhängiger Anwendungsfall denselben Vertrag benötigt.

**D-04 – Inputordnung:** Sobald die Verarbeitung eines Timestamps begonnen hat, werden keine neuen externen Inputs mehr für diesen oder einen früheren Timestamp angenommen. Bereits zugelassene Inputs sowie kausale Follow-ups bleiben deterministisch geordnet.

**D-05 – Commit/Event/Fault:** Autoritative Event-Kapazität wird vor dem zugehörigen Commit gesichert. Ein unerwarteter Reaction-Fehler rollt den bereits gültigen Commit nicht zurück, sondern beendet die weitere autoritative Ausführung dieser Runtime als Fault.

Runtime-/Session-Fortsetzungssnapshots entstehen im aktuellen Profil nur an **quiescent boundaries**. Ein S05-Runtime-Restore rekonstruiert neue Runtime-Objekte und eine neue `SimulationRuntimeIdentity`; domainlokale Snapshots wie in S06 rekonstruieren ihren eigenen State unabhängig. Alte Live-Objekte oder alte Handlerbindungen werden nicht als Persistenzzustand übernommen.

## Dokumente

| Datei | Zweck |
|---|---|
| [Manifest](Plaquewright_Manifest_v2_0.md) | Verbindlicher Architekturvertrag, Grenzen, Determinismus, Replay und Entscheidungen |
| [Implementierungsfolge](Plaquewright_Implementation_Sequence_v2_0.md) | Vertikale Schritte ab der bestätigten Baseline |
| [Freigabe und Testnachweise](Plaquewright_Freigabe_und_Testnachweise_v2_0.md) | Technischer Anschlussstand und Abnahmen |
| [Architecture Map](ARCHITECTURE_MAP_v2_0.md) | Verantwortlichkeiten und Beispielabläufe |
| [Code Classification](CODE_CLASSIFICATION_v2_0.md) | Einordnung vorhandener Typen ohne pauschale Ordnergleichsetzung |
| [Migration Plan](MIGRATION_PLAN_v2_0.md) | Schrittweise Übernahme ohne Rewrite oder stillen Regelverlust |
| [Glossar](PLAQUEWRIGHT_ARCHITECTURE_GLOSSARY_v2_0.md) | Begriffe und Grenzen |
| [Source Evidence](SOURCE_EVIDENCE_v2_0.md) | Quellen, Snapshotgrenzen und Entscheidungsnachweise |
| [Current Chat Handoff](CURRENT_CHAT_HANDOFF.md) | Kurzlebige Arbeitsübergabe; bei jedem Chatwechsel ersetzen |

## PW-S05 – abgenommener Snapshot-/Replay-Umfang

PW-S05 wurde auf `d8826a2` im vereinbarten **in-memory Core-Profil** abgeschlossen. Der Ausbau verlief über `653c1f5` (Snapshot-/Restore-Grundlage), `789763c` (expliziter Combat-Pending-Work-Codec), `c6b2327` (Runtime-/Session-Fortsetzungs-Snapshot) und `d8826a2` (deterministischer Replay-Fortsetzungsbeweis).

Der abgenommene Umfang umfasst:

- Resource-/Entity-State samt Revisionen und relevante deterministische ID-Allocator.
- `SimulationRuntimeState`, Scheduler und Runner einschließlich `ScheduledEventKey`, Sequence-Fortsetzung, Zeit, `ProcessedEvents`, Limits und D-04-Input-Closure.
- Quiescent Snapshot-Capture: kein aktives Event und keine offene Prepared-Follow-up-Reservation.
- `DamageResolutionSnapshot` / `ApplyResolvedDamageActionSnapshot` als Rebind eines bereits resolved Combat-Vorgangs gegen Runtime B, ohne erneute Rule-Resolution oder ID-Allokation.
- `CombatWorkItemSnapshotCodec` als explizite, geschlossene Snapshot-Grenze für `ApplyResolvedDamageAction` und `DamageCommittedEvent`; unbekannte Work Items werden abgewiesen.
- `DamageCommittedEventSnapshot` rekonstruiert einen bereits committed Fakt, ohne Commit oder Damage erneut auszuführen.
- `SimulationRuntimeSessionSnapshot<TPayloadSnapshot>` bündelt Runtime- und Runner-Fortsetzungszustand. Die Composition wird bewusst **nicht** gesnapshottet, sondern für Runtime B neu aufgebaut.
- Ein Replay-Vergleich setzt nach der Snapshot-Grenze weitere identische geordnete Inputs in Runtime A und B ein und vergleicht Scheduler-/Trace-Verlauf, Event-Fakten und generierte IDs, Resource-State/Revision sowie die nach der Snapshot-Grenze neu entstehende Ledger-History/Provenienz.

Die alte Ledger-History vor der Snapshot-Grenze ist im aktuellen Profil **nicht** Teil des Snapshots. `CompiledResourceRegistry` und kompatible Regeln/Definitionen werden von außen bereitgestellt. Der Core hält derzeit keinen langlebigen RNG-State, der separat restauriert werden müsste.

## Bewusst nicht mit PW-S05 freigegeben

PW-S05 ist keine Freigabe für JSON-/Binary-Savegames, universelle polymorphe Persistenz aller `ISimulationWorkItem`-Typen, Cross-Version-Migration, automatische Ruleset-Migration, vollständige Replay-History-Retention, echte Host-/Netzwerk-Side-Effect-Unterdrückung, aufgezeichnete Physics-Host-Facts, Cross-Platform-/Cross-Engine-Bitgleichheit oder einen 30-Sekunden-Analyse-Scrubber.

## PW-S06 – abgenommener Zeit-/Query-Umfang

PW-S06 wurde auf `3ee638a` abgeschlossen. Die technische Folge lautet `238a24a` → `0739c31` → `982798e` → `3ee638a`; der finale bestätigte Teststand beträgt **1267/1267**, Build grün, anschließend `gcc`.

Der abgenommene Umfang umfasst:

- `TimedModifierStateSet` als domain-eigenen autoritativen Zustand mit `Revision`, pro Key fortgesetzter `Generation`, aktiven/inaktiven Slots und explizitem Ablaufzeitpunkt.
- `TimedModifierExpiration` als Gültigkeitstoken. Refresh und Cancel benötigen kein physisches Entfernen bereits geplanter Scheduler-Arbeit; alte Expirations werden über die Generation kontrolliert stale.
- Ablauf über `SchedulerPhase.StateBoundary`, sodass ein Ablauf bei `T` vor `Execution` desselben Timestamps sichtbar wird. Zero-duration wird im ersten Profil bewusst abgewiesen.
- `TimedModifierValueQuery` als read-only Referenzrechnung über die vorhandene `ModifierAccumulator`-/`ModifierMath`-Semantik.
- `TimedModifierValueQueryCache` mit Cache-Key aus State-Identität, `Revision` und Query-Input. Nur echte autoritative Mutation invalidiert; stale Expiration verändert die Revision nicht.
- `TimedModifierStateSetSnapshot` samt aktiven und inaktiven Slots, Generationen, Revision und Expiration-State. Restore rekonstruiert unabhängig und ruft nicht `ApplyOrRefresh(...)` nach.
- Einen Fortsetzungsbeweis, der Domain-Snapshot und pending Scheduler-Arbeit gemeinsam restauriert. Eine alte Expiration bleibt nach Restore stale, die aktuelle Generation läuft am korrekten `StateBoundary` ab, und Query/Cache liefern in beiden Branches dieselbe Fortsetzung.

PW-S06 führt **kein** allgemeines Status-/Buff-System, keine globale Timer-Registry, keinen Scheduler-Cancel-Service, keinen universellen Work-Item-Serializer und keine Pflichtintegration der Stats-Domain in `SimulationRuntimeState` ein. PW-QA-20 ist für diesen Referenzumfang abgenommen.

## Nächster Entwicklungsstrang

Der nächste reguläre Architekturbaustein ist **PW-S07 – definierte Lastprofile, Retention und gemessene Optimierung**. Zuerst wird ein reproduzierbares Referenzprofil mit Hardware-/Runtime-/Buildangaben und semantischer Referenzmessung festgelegt; erst danach werden Hot Paths oder Retention optimiert. D-06 bleibt bis zu solchen Messungen offen.

D-03, D-06 und D-07 bleiben offene technische Detailgates.
