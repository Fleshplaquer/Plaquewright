# Plaquewright – Architecture Map 2.0

**Stand:** 18. September 2026 · **Status:** Architektur 2.0 freigegeben; PW-S02, PW-S03 und PW-S04 abgenommen · **Historische Audit-Baseline:** `d39cb54` · **PW-S02:** `b04fcbe` · **PW-S03:** `f90517a` · **PW-S04:** `8bd4dd0`
Die frühere `ARCHITECTURE_MAP.md` wird in der Architekturübergabe genannt, liegt im bereitgestellten Archiv aber nicht vor. Diese Fassung ist eine neue Rekonstruktion, kein behaupteter zeilenweiser Abgleich. [E1, E3]

## 1. Zielbild

Die Pfeile bezeichnen Abhängigkeit oder gezielte Kommunikation, nicht vorgeschriebene neue Assemblies.

```text
Host: Godot / Headless / spaeter weitere Hosts
    |
    v
Adapter: Eingaben, Darstellung, externe Ergebnisse
    |
    v
SimulationSession<TWorkItem>
    |
    +--> SimulationRunner / SimulationScheduler
    |
    v
SimulationComposition / ExecutionPlan
    |
    v
Spiel-Komposition / Regelpakete
    |                       |
    v                       v
Resources / Stats / Combat   externe Door-, Alarm-, Temperature-Module
    |                       |
    +----------+------------+
               v
Kernel-Vertraege
Zeit / Reihenfolge / IDs / RNG / Transactions / Folgearbeit
```

Der Kernel verweist nicht zurück auf Combat oder einen Türtyp. Eine Spielregel darf beide Domains kennen, die sie absichtlich verbindet.

## 2. Kontroll- und Datenfluss

| Zeitpunkt | Verantwortlicher | Ergebnis |
|---|---|---|
| Start | Komposition | Validierte Module, Definitionen, Regel- und Ausführungsprofile |
| Eingabe | Adapter und Zulassungsgrenze | Geordneter, zeitlich gültiger Intent |
| Lesen | Domain-Query | Passende veröffentlichte, projizierte oder historische Sicht |
| Auflösen | Gameplay-Regel und Domain-Resolver | Effects und konkrete vorbereitbare Änderungen |
| Vorbereiten | Participants und Coordinator | Gemeinsame Gültigkeit und benötigte Kapazität |
| Veröffentlichen | Vorgesehener Commit-Pfad | Neuer State und notwendige Buchungen/Commit-Ergebnisse |
| Danach | Dispatcher und Reactions | Ereignisse lesen und neue deterministische Arbeit erzeugen |
| Darstellen | Host | Kopien/Sichten auf Ergebnisse, keine direkten Fremdmutationen |

PW-S02 implementiert dafür bewusst kein universelles Commit-Ergebnispaket: Ein typisiertes Domain Event beschreibt den committed Sachverhalt, während `DomainEventTransactionCoordinator` die sichere Commit→Event-Übergabe koordiniert. Der aktuelle Stand ist in [Code Classification](CODE_CLASSIFICATION_v2_0.md) beschrieben.

## 3. Referenzszenario: bezahlte Tür mit Folgeaktion

**Schon vorhandener Beweis:** Resources und ein externes Door-Modul können im Gold-/Door-Test gemeinsam vorbereitet und committed werden. [E3, E4]

**PW-S02-Nachweis auf `b04fcbe`:**

```text
OpenPaidDoor
  -> Regel liest Gold und Door-State
  -> Resources bereitet Zahlung vor
  -> Door-Modul bereitet Oeffnung vor
  -> Coordinator committed beides
  -> DoorOpened wird nach Commit sichtbar
  -> Alarm-Regel erzeugt RaiseAlarm als neue Arbeit
  -> RaiseAlarmAction aktualisiert im Referenztest den Alarm-Zustand
  -> spaetere Schritte koennen daraus weitere Transactions oder zeitliche Arbeit ableiten
```

Scheitert die Öffnung beim Prepare, gibt es weder Zahlung noch `DoorOpened` noch Alarm. Eine unerwartete Reaction-Exception rollt die bereits erfolgreich geöffnete Tür nicht zurück; die Runtime faultet und verarbeitet keine weitere autoritative Arbeit. Der aktuelle Referenztest beweist die Reaction-/Follow-up-Grenze, noch keine zweite generische Alarm-Transaction.

Ein Profileffekt, der gemeinsam mit der Türöffnung zwingend atomar sein muss, wäre hingegen Teilnehmer der ersten Transaction, nicht eine spätere Reaction. Diese Unterscheidung entscheidet das Gameplay-Regelpaket.

## 4. Referenzszenario: Combat über dieselbe Runtime

PW-S04 führt Combat als zweites fachliches Referenzszenario über dieselbe Session-/Composition-/Scheduler-/Transaction-/Event-Grenze wie den Door-Fall. Der Kernel erhält dadurch keine Health-, Damage-, Mana- oder Armor-Fachbegriffe. [E9]

**Lethal Damage mit PreDefeat:**

```text
DamageResolutionContext
  -> ApplyResolvedDamageAction
  -> DamageResourceLossPlan(s)
  -> ResolvedDamageApplicationExecutor
       -> ResourceTransactionDraft
       -> optional PreDefeat pro Owner
       -> DefeatAwareResourceTransactionCommitter
  -> DamageCommittedEvent
  -> Reaction
  -> neue Work Item
```

Die PreDefeat-Regel darf den noch unveröffentlichten Draft verändern. Erst der Resource-Commit macht State, Revisionen und Ledger autoritativ sichtbar. `DamageCommittedEvent` wird danach aus dem tatsächlichen Commit-Ergebnis erzeugt; seine Reactions können nur neue Arbeit anfordern und schreiben den Commit nicht rückwirkend um.

**Bezahlter Angriff:**

```text
PayAttackCostAction
  -> Cost Transaction
  -> AttackCostCommittedEvent
  -> Reaction
  -> Damage Resolution
  -> ApplyResolvedDamageAction
  -> Damage Commit
  -> DamageCommittedEvent
  -> Reaction
```

Damit bleiben fachliche Operationen getrennt: Die Zahlung ist Cost, der Treffer ist Resource Loss/Damage. Beide nutzen gemeinsame generische Runtime-Primitiven, ohne dass Resources Combat kennen muss.

Resource-Routing bleibt bewusst Ruleset-/Kompositionswissen. `ApplyResolvedDamageAction` trägt eine bereits aufgelöste Damage-Resolution, aber keine fest verdrahtete „Health“-Resource. Der interne Damage-Application-Executor kann mehrere Loss-Pläne und Owner koordinieren; ein fehlender betroffener Owner oder ein Plan aus einer fremden Damage-Resolution wird vor autoritativer Mutation abgewiesen.

Die Actor-/Projectile-Darstellung kann weiterhin in Godot laufen. Autoritative Target-/Kontaktinformation benötigt eine explizite Host-/Spatial-Grenze; PW-S04 führt keine Physics-Autorität in Combat oder Kernel ein.

## 5. Abhängigkeitsregeln

PW-S03 konkretisiert die Startup-Komposition: Module deklarieren Required/Provided-Contracts; fehlende oder doppelte Provider werden vor dem Run abgewiesen. Ein gebauter Execution Plan ist eingefroren und routet über den exakten Work-Item-Typ. Die explizite Modulreihenfolge bleibt sichtbar; es gibt keine automatische Reflection-Discovery oder versteckte Topological Sort. [E8]

Ein unabhängiges Modul verändert den Kernel nicht, um registriert zu werden. Ein verbindendes Modul darf Resources und Inventory kennen, ohne daraus eine Rückabhängigkeit in Resources zu erzeugen.

Nicht jedes Zusammenspiel läuft durch einen globalen Event-Bus. Queries bleiben direkte Lesepfade; Transactions koordinieren gemeinsame Änderungen; Events beschreiben abgeschlossene Ergebnisse.

Die aktuelle `SimulationRuntimeState` ist eine vorhandene Gameplay-Komposition. Eine generische Runtime wird nur so weit getrennt, wie eine zweite modulare Komposition dies tatsächlich benötigt.

## 6. Nicht zu verwechseln

- Autoritativer State ist nicht der Debug-Trace.
- Ein Domain Event ist keine Anweisung.
- Eine Simulation-Work-Queue ist kein verteilter Message-Broker.
- Eine Modulgrenze ist keine Sandbox.
- Headless ist keine automatische Approximation.
- Engine-Unabhängigkeit ist noch kein Beweis für Physics-/Plattform-Replay.

## 7. Nächster Abgleich

PW-S04 ist abgeschlossen. Der finale Nachweisstand `8bd4dd0` wurde nach 1215/1215 bestandenen Tests mit `gcc` bestätigt. Damit tragen Door/Alarm, Resources/Door/Alarm und Combat dieselbe generische Runtime, während Combat-Fachbegriffe außerhalb des Kernels bleiben. [E9]

Der nächste Abgleich gehört zu **PW-S05**: Snapshot/Restore und deterministischer Replay-Beweis an einer quiescent boundary. Vor einer Implementierung wird konkret bestimmt, welche Domainzustände, ausstehenden Scheduler-/Reaction-Work-Items, RNG-/ID-Zustände, Regelversionen und autoritativen Inputs beziehungsweise Host Facts für den gewählten kleinen Referenzfall benötigt werden.

Ein globaler Serialization-, EventStore-, Registry-, Plugin-Discovery- oder Editor-Unterbau ist dafür weiterhin nicht vorab erforderlich.

**Quellen:** [E1–E4, E7–E9 und W1](SOURCE_EVIDENCE_v2_0.md).
**Verbindlichkeit:** [PW-00 und PW-20](Plaquewright_Manifest_v2_0.md).
