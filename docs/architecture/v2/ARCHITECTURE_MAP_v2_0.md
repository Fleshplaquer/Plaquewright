# Plaquewright – Architecture Map 2.0

**Stand:** 17. September 2026 · **Status:** Architektur 2.0 freigegeben · **Anschluss:** `d39cb54` laut Nutzer  
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

Ein vorgeschlagenes Commit-Ergebnispaket ist nicht gleichbedeutend mit einem bereits implementierten Typ. Der aktuelle Generic-Transaction-Vertrag ist in [Code Classification](CODE_CLASSIFICATION_v2_0.md) beschrieben.

## 3. Referenzszenario: bezahlte Tür mit Folgeaktion

**Schon vorhandener Beweis:** Resources und ein externes Door-Modul können im Gold-/Door-Test gemeinsam vorbereitet und committed werden. [E3, E4]

**Vorgeschlagener Ausbau:**

```text
OpenPaidDoor
  -> Regel liest Gold und Door-State
  -> Resources bereitet Zahlung vor
  -> Door-Modul bereitet Oeffnung vor
  -> Coordinator committed beides
  -> DoorOpened wird nach Commit sichtbar
  -> Alarm-Regel erzeugt RaiseAlarm als neue Arbeit
  -> Alarm-Modul committed eigenen Zustand
  -> optional: AutoClose-Arbeit zum spaeteren Simulationszeitpunkt
```

Scheitert die Öffnung beim Prepare, gibt es weder Zahlung noch `DoorOpened` noch Alarm. Scheitert später die Alarm-Action, bleibt die bereits erfolgreich geöffnete Tür geöffnet; das ist kein Rollback der ersten Transaction.

Ein Profileffekt, der gemeinsam mit der Türöffnung zwingend atomar sein muss, wäre hingegen Teilnehmer der ersten Transaction, nicht eine spätere Reaction. Diese Unterscheidung entscheidet das Gameplay-Regelpaket.

## 4. Referenzszenario: zeitlich getrennter Angriff

Ein Skill-Modul kann künftig bei Cast-Start Kosten und seinen eigenen Ausführungszustand committen. Ein späterer Treffer liest die vom Regelprofil festgelegten Werte, löst Damage auf und lässt Resources die konkrete Zustandsänderung veröffentlichen. Eine On-Hit-Heilung ist neue Folgearbeit, sofern das Profil sie nicht ausdrücklich der direkten Resolution zuordnet.

Eine Schadensverhinderung, die den Treffer ändern soll, gehört vor dessen Commit. Ein Event-Handler nach `DamageApplied` darf diesen Commit nicht umschreiben.

Die Actor-/Projectile-Darstellung kann in Godot laufen. Autoritative Target-/Kontaktinformation benötigt trotzdem eine explizite Host-/Spatial-Grenze. Das Diagramm setzt keine bereits fertige Spatial-Simulation voraus.

## 5. Abhängigkeitsregeln

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

Vor dem Ausbau werden konkrete Source- und Testkörper an der Baseline geprüft, besonders gemeinsam genutzte State-/Ledger-Ziele und gemischte Prepared-Lebensdauern. Danach folgt ein kleiner Post-Commit-/Reaction-Beweis. Ein globaler Registry-, Serialization- und Editor-Unterbau ist dafür nicht vorab erforderlich.

**Quellen:** [E1–E4 und W1](SOURCE_EVIDENCE_v2_0.md).  
**Verbindlichkeit:** [PW-00 und PW-20](Plaquewright_Manifest_v2_0.md).
