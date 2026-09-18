# Plaquewright – Architecture Map 2.0

**Stand:** 18. September 2026 · **Status:** Architektur 2.0 freigegeben; PW-S02 bis PW-S06 abgenommen · **Historische Audit-Baseline:** `d39cb54` · **PW-S02:** `b04fcbe` · **PW-S03:** `f90517a` · **PW-S04:** `8bd4dd0` · **PW-S05:** `d8826a2` · **PW-S06:** `3ee638a` · finaler S06-Testumfang 1267/1267 + `gcc`
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

## 7. Abgenommener PW-S05-Abgleich

PW-S05 ist auf `d8826a2` für den vereinbarten in-memory Core-Snapshot-/Replay-Scope abgenommen. Die technische Folge lautet `653c1f5` → `789763c` → `c6b2327` → `d8826a2`. [E10, E11]

Der Restore-Pfad trennt Snapshot-Daten von Live-Objekten und Live-Komposition:

```text
Runtime A
  Domain State + Revisions
  ID Allocators
  Runner / Scheduler
  pending DamageCommittedEvent
  pending ApplyResolvedDamageAction
            |
            v
        Snapshot
            |
            v
Runtime B
  neue RuntimeIdentity
  restaurierter Domain State
  gleiche Allocatorpositionen
  gleiche Scheduler Keys/Sequence
  rehydrierter committed Event
  neu gebundener DamageResolutionContext
            |
            +--> neue Composition fuer Runtime B
            |
            v
       Fortsetzung
```

Die Snapshot-Grenze ist quiescent: kein aktives Scheduler-Event und keine offene Prepared-Follow-up-Reservation. `ExternalInputsClosedThrough` wird mit restauriert, damit D-04 nach Restore nicht aufgeweicht wird. Pending Combat Work speichert keine alte Runtime-Referenz.

`CombatWorkItemSnapshotCodec` bildet im Referenzprofil genau `ApplyResolvedDamageAction` und `DamageCommittedEvent` ab. Der Action-Snapshot erhält IDs und bereits resolved Damage-Mengen und bindet neue Kontexte gegen Runtime B. Der Event-Snapshot erhält ausschließlich die Fakten eines bereits committed Events; der alte Damage-Commit wird beim Restore nicht wiederholt. Unbekannte Work Items werden kontrolliert abgewiesen.

`SimulationRuntimeSessionSnapshot<TPayloadSnapshot>` bündelt Runtime- und Runner-Fortsetzungszustand. `SimulationComposition` gehört bewusst nicht zum Snapshot: beim Restore wird eine neue Composition mit Handlern gebaut, die Runtime B referenzieren. Damit bleibt Konfiguration/Ruleset-Wissen getrennt vom autoritativen Laufzeitzustand.

Der abschließende Replay-Beweis geht über einen Endsaldo hinaus:

```text
Snapshot an T=100
  -> bereits committed Damage-Event noch pending
  -> weiteres resolved Damage pending
  -> gleiche neue Inputs nach Snapshot in A und B
  -> A vollstaendig fortsetzen
  -> B vollstaendig fortsetzen
  -> Runner/Scheduler/Trace vergleichen
  -> Event-Fakten + IDs vergleichen
  -> Resource-State/Revision vergleichen
  -> neue Ledger-History/Provenienz vergleichen
```

Die Ledger-History **vor** dem Snapshot ist im aktuellen Profil nicht enthalten. Ebenso bleiben echte Host Facts, reale Side-Effect-Unterdrückung, Cross-Version-Migration, universelle Work-Item-Persistenz, Savegame-Format und Cross-Platform-/Physics-Parität außerhalb der S05-Abnahme.

## 8. Abgenommener PW-S06-Abgleich

PW-S06 ist auf `3ee638a` für den zeitabhängigen Stats-/Derived-Query-Referenzumfang abgenommen. Die technische Folge lautet `238a24a` → `0739c31` → `982798e` → `3ee638a`. [E12]

Der neue Kontrollfluss bleibt domain-owned:

```text
Apply/Refresh Modifier
  -> TimedModifierStateSet mutiert
  -> Revision + Generation fortsetzen
  -> Expiration bei ExpiresAt / StateBoundary planen
            |
            +--> Query liest autoritativen Stats-State
            |      -> optionaler Cache: State identity + Revision + Input
            |
            v
Expiration Work
  -> Generation aktuell? -- nein --> stale, keine Mutation
            |
           ja
            v
  -> State am Boundary entfernen
  -> Revision erhöhen
  -> Same-Time Execution liest neuen State
```

Refresh und Cancel erfordern kein physisches Scheduler-Cancel. Die Scheduler-Arbeit beschreibt nur geordnete Arbeit; die Stats-Domain entscheidet anhand des aktuellen Slots und der Generation, ob diese Arbeit noch gültig ist. Dadurch bleibt alte Expiration-Arbeit deterministisch und billig als stale Pfad behandelbar.

Snapshot/Restore folgt demselben S05-Grundsatz, aber domainlokal: aktive **und inaktive** Slots, Generationen, Revision und Ablaufdaten werden beschrieben. Der Abschlussbeweis kombiniert diesen Domain-Snapshot mit einem Runner-Snapshot und einer test-only Work-Item-Rehydrierung. `SimulationComposition` wird neu gegen den restaurierten Stats-State gebaut; es wurde weder ein universeller Serializer noch eine Pflichtkopplung an `SimulationRuntimeState` eingeführt.

PW-QA-20 ist für diesen Referenzumfang abgenommen. Der nächste Architekturstrang ist PW-S07: definierte Last-/Retention-Profile und gemessene Optimierung. Ein Fast Path wird erst nach einer reproduzierbaren Referenzmessung eingeführt.

**Quellen:** [E1–E4, E7–E12 und W1](SOURCE_EVIDENCE_v2_0.md).
**Verbindlichkeit:** [PW-00 und PW-20](Plaquewright_Manifest_v2_0.md).
