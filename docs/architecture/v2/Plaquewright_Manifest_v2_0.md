# Plaquewright – Framework-Manifest 2.0

**Dokumentversion:** 2.0
**Datum:** 18. September 2026
**Status:** Architekturfreigabe 2.0; D-01, D-02, D-04 und D-05 beschlossen; PW-S02 bis PW-S06 abgenommen
**Historische Audit-Baseline:** `d39cb54`
**PW-S02-Nachweis:** `b04fcbe`, vom Nutzer als grün / committed / clean bestätigt
**PW-S03-Nachweis:** `f90517a`, vom Nutzer als grün / committed / clean bestätigt
**PW-S04-Nachweis:** `8bd4dd0`, vom Nutzer nach 1215/1215 Tests als grün / committed / clean bestätigt
**PW-S05-Nachweis:** `d8826a2`, vereinbarter in-memory Core-Snapshot-/Replay-Scope abgenommen; finaler S05-Testumfang 1248 Tests, vollständiger Regressionstest und Build grün, anschließend `gcc`
**PW-S06-Nachweis:** `3ee638a`, zeitabhängiger Stats-/Derived-Query-Referenzumfang abgenommen; finaler S06-Testumfang 1267/1267, vollständiger Regressionstest und Build grün, anschließend `gcc`
**Gegenstand:** Engine-unabhängiges, deterministisches, modulares Gameplay-/Simulations-Framework für C#

## PW-00 – Geltung, Quellen und Änderungsstatus

Dieses Manifest richtet Plaquewright am Framework-Ziel aus. Ein einzelnes Idle-Spiel ist nicht mehr das Produkt, aus dessen Featureliste alle anderen Anforderungen abgeleitet werden. Idle, ARPG, Tower Defense und Survival sind mögliche Anwendungen; Godot ist der erste Host, nicht die autoritative Gameplay-Implementierung. Grundlage ist die neu eingebrachte Architekturübergabe. [E1]

Die Fassung ersetzt nach ausdrücklicher Annahme die alte **Produkt- und Baufolgevorgabe** des Idler-Manifests. Sie verwirft nicht automatisch bestehende Combat-Regeln, Tests oder fachliche Erkenntnisse. Ein allgemeiner Kernel-Vertrag und die Regel „dieses Spiel berechnet Armor auf diese Weise“ sind unterschiedliche Arten von Entscheidungen.

Die Wörter „muss“, „darf nicht“ und „soll“ beschreiben den freigegebenen Architekturvertrag, nicht einen Beweis, dass der aktuelle Code ihn vollständig umsetzt. Die Framework-Ausrichtung sowie D-01, D-02, D-04 und D-05 sind beschlossen. Die technischen Detailgates D-03, D-06 und D-07 werden vor den jeweils betroffenen Implementierungsschritten konkretisiert.

Quellencode belegt den Iststand; Architekturentscheidungen definieren den Sollstand; Testausführungen belegen konkrete Szenarien auf konkreten Ständen. Keines ersetzt das andere. Der vom Nutzer bestätigte A/B-Auditabschluss bleibt auf `d39cb54` dokumentiert und wird nicht durch neue Dokumentnummern neu eröffnet. [E2–E5]

**Quellen und Grenzen:** [Quellenregister](SOURCE_EVIDENCE_v2_0.md).
**Umsetzung:** [Implementierungsfolge](Plaquewright_Implementation_Sequence_v2_0.md).
**Abnahme:** [Freigabe und Testnachweise](Plaquewright_Freigabe_und_Testnachweise_v2_0.md).

## PW-01 – Produkt und Erfolgskriterium

Plaquewright liefert einen kleinen Koordinationskern, wiederverwendbare fachliche Module und explizite Erweiterungsverträge. Ein Spiel kombiniert diese mit eigenen Regeln und einem Host.

Das zentrale Erfolgskriterium lautet:

> Ein externes Modul kann eigene Zustände, Regeln und Folgeaktionen einbringen, ohne dass der Kernel seine Fachbegriffe kennen oder ein unabhängiges Modul dafür verändert werden muss.

Eine absichtliche Abhängigkeit ist erlaubt: Eine Kaufregel darf Resources und Inventory kennen. Daraus folgt nicht, dass Resources selbst von Inventory abhängen muss. „Modular“ bedeutet nicht „alle Module sind voneinander unabhängig“, sondern „Abhängigkeiten sind erforderlich, gerichtet, benannt und prüfbar“.

Die erste nutzbare Framework-Version soll durch wenige vollständige Referenzszenarien entstehen, nicht durch die Vorabimplementierung aller denkbaren Genres. Der vorhandene Code wird weiterentwickelt, nicht neu geschrieben. [E1]

## PW-02 – Zuständigkeiten und Schichten

| Bereich | Verantwortung | Nicht seine Verantwortung |
|---|---|---|
| Kernel | Simulationszeit, Reihenfolge, allgemeine Identitäten, RNG-Infrastruktur, Transaction-Koordination, begrenzte Folgearbeit | Health, Mana, Armor, Inventory oder eine bestimmte Schadensformel |
| Domain-Modul | Eigener autoritativer Zustand, lokale Invarianten, Leseverträge, vorbereitbare Änderungen | Fremden Modulzustand direkt verändern |
| Gameplay-Regeln / Komposition | Intent auflösen, beteiligte Module verbinden, fachliche atomare Einheit bestimmen | Interne Publikationsrechte umgehen |
| Engine-Adapter | Engine-Daten in definierte Eingaben übersetzen; Ergebnisse darstellen | Verdeckte alternative Gameplay-Wahrheit erzeugen |
| Host | Ausführung anstoßen, Lebenszyklus und Integration betreiben, Darstellung und Plattformdienste | Renderzeit als unkontrollierte Simulationsregel verwenden |

„Core“ bezeichnet derzeit eine Assembly und einen Quellcodebereich, nicht automatisch den Kernel. Insbesondere ist `SimulationRuntimeState` im untersuchten Snapshot eine konkrete Kombination von Entities, Resources und Combat. Diese Komposition bleibt nutzbar, darf aber nicht als generischer Weltzustand zur Pflicht für alle Module werden. [E3]

Ein Modul darf intern spezialisierte Speicher und Fast Paths besitzen. Das Framework erzwingt weder eine universelle `Dictionary<string, object>`-Welt noch ein vollständiges ECS.

## PW-03 – Modulverträge und Aufbau einer Simulation

Eine Simulation soll ihre Module, Regelversionen und Abhängigkeiten ausdrücklich zusammenstellen. Vorgeschlagener erster Aufbau: Registrierung und Validierung beim Start, danach ein stabiler Ausführungsplan. Laufzeit-Hotloading ist nicht Teil dieser ersten Grenze.

Fehlende Pflichtverträge, doppelte Identitäten und ungültige Abhängigkeiten sollen vor dem ersten Gameplay-Schritt mit nachvollziehbaren Fehlern scheitern. Reihenfolge mit fachlicher Bedeutung wird nicht aus zufälliger Assembly-Erkennung oder ungeordneter Enumeration abgeleitet.

Module erhalten die benötigten schmalen Verträge. Ein allwissender globaler Service-Locator ist kein Ersatz für Abhängigkeiten. Der Kernel muss nicht jede Query oder jeden Rechenschritt dynamisch vermitteln.

**Erster Strukturbeweis:** Eine kleine Simulation mit externem Door-/Alarm-Modul läuft ohne Combat. Eine zweite Komposition verwendet Resources und ein Combat-Referenzszenario. Eine eigene Assembly pro Unterordner ist dafür keine Vorbedingung.

## PW-04 – Drei Kommunikationswege

### Query: lesen

Queries lesen die autoritative, veröffentlichte Sicht, sofern nicht ausdrücklich eine Projection- oder Snapshot-Sicht verlangt wird. Beispiel: „Wie viel Gold ist verfügbar?“ oder „Welcher Armor-Wert gilt für diesen Treffer?“

Eine Query darf keine Ressourcen ausgeben, keine autoritative Zufallsfolge voranschieben und keine Events oder Arbeit erzeugen. Ein rein abgeleiteter Cache darf intern aktualisiert werden, wenn dies die beobachtbare Gameplay-Semantik nicht ändert und seine Invalidierung definiert ist.

Direkte typisierte Leseaufrufe sind erlaubt. Nicht jede Query benötigt eine Nachricht oder eine Queue.

### Transaction / Domain Change: ändern

Autoritative Änderungen werden von ihrer besitzenden Domain vorbereitet und über den vorgesehenen Commit-Pfad veröffentlicht. Beispiel: Gold bezahlen und eine Tür öffnen.

Eine fachlich zusammengehörige Änderung mehrerer Domains benötigt eine gemeinsame atomare Grenze. Eine einzelne lokale Änderung darf einen entsprechend kleinen Commit-Pfad verwenden; nicht jede Setter-ähnliche Operation muss einen großen generischen Objektgraphen erzeugen.

### Event / Reaction: auf Ergebnisse reagieren

Ein Domain Event beschreibt einen erfolgreich committed Sachverhalt. Eine Reaction verarbeitet diesen Sachverhalt und kann neue Arbeit anfordern. Sie ändert den ursprünglichen Commit nicht rückwirkend.

„Möglicher Schaden“ ist ein Resolution-Ergebnis, noch kein `DamageApplied`. „Tür soll aufgehen“ ist ein Intent, noch kein `DoorOpened`. [E1]

## PW-05 – Action, Resolution, Effect und Domain Change

Der konzeptionelle Ablauf ist:

```text
Zugelassener Intent / Action
  -> passende Gameplay-Regel
  -> Queries und Resolution
  -> gewünschte Effects
  -> vorbereitete Domain Changes
  -> Commit
  -> Domain Events
  -> deterministisch eingeplante Reactions / neue Actions
```

Dies ist ein Verantwortungsmodell, keine Pflicht zu einer eigenen Klasse und Allocation für jeden Pfeil.

Ein Effect beschreibt eine gewünschte fachliche Wirkung: `DealDamage(100)`. Die Resolution kann daraus nach dem gewählten Regelprofil einen konkreten Ressourcenverlust von 63 ableiten. Der Ressourcen-Commit veröffentlicht diesen Verlust; erst sein Ergebnis rechtfertigt entsprechende Events.

Eine Action ist nicht automatisch genau eine Transaction. Cast-Start, späterer Einschlag und eine folgende Heilung können drei getrennte atomare Übergänge sein. Ein späterer Fehlschlag macht die früheren Commits nicht ungeschehen; Refund oder Kompensation ist eine neue ausdrückliche Regel.

## PW-06 – Transactions und Publikationsrechte

Die vorhandene Ressourcen-Mechanik mit Draft, Projektionen, Revisionen und vorbereitetem Ledger wird weiterverwendet. Der generische Coordinator kennt Participants, nicht Gold, Türen oder Damage. Die externe Gold-/Door-Grenze ist bereits Teil des Anschlussstands und nicht erneut als Erstimplementierung einzuplanen. [E2–E4]

### Zielvertrag

Prepare darf fachlich ablehnen. Vor dem ersten Apply müssen alle normalen Voraussetzungen für die gesamte atomare Einheit feststehen. Apply führt keine neuen fachlichen Entscheidungen aus und ruft keine beliebigen Gameplay-, UI- oder Reaction-Callbacks auf.

Eine vorbereitete Änderung ist kein öffentlich nutzbarer „State jetzt veröffentlichen“-Befehl. Das Framework veröffentlicht über seine vorgesehenen Coordinator-/Domain-Grenzen. Die öffentliche Erweiterbarkeit von `PreparedTransactionChange` ist davon zu unterscheiden: Fremdmodule dürfen ihre eigene Mutation implementieren. `protected ApplyCore()` ist deshalb kein Sandboxmechanismus gegen bösartigen Fremdcode. [E3]

### Grenzen, die bei neuer Komposition ausdrücklich zu prüfen sind

Zwei einzeln gültige Participants sind nicht automatisch gemeinsam gültig. Zwei Kosten gegen denselben Goldbestand dürfen nicht beide dieselbe unreservierte Ausgangssumme ausgeben. Zwei Ledger-Appends dürfen ihre gemeinsame Kapazität nicht unabhängig überschätzen.

Ein Domain-Participant muss solche Änderungen gemeinsam vorbereiten oder die Überlappung muss vor jedem Apply abgewiesen werden. Die erste Framework-Ausbaustufe kann eine bewusst eingeschränkte Kombination unterstützen; sie darf keine allgemeine Konfliktauflösung vortäuschen.

Ebenso müssen die Verwendbarkeit **aller** Prepared-Objekte, Eigentümer-/Runtime-Zuordnung und Lebensdauer geklärt sein, bevor eine Batch-Publikation beginnt. Nicht nur die erneute Nutzung eines einzelnen Objekts, sondern eine Mischung aus frischen und bereits verbrauchten Objekten ist relevant.

### Fehler- und Ausführungsmodell

Vorgeschlagener anfänglicher Vertrag: ein autoritativer Writer je Simulation, keine verschachtelte Ausführung während Apply und keine extern beobachtbaren Zwischenstände. Parallelität unabhängiger Simulationen bleibt möglich; ein gemeinsamer nebenläufiger World-Commit ist damit nicht zugesagt.

Eine unerwartete Exception mitten in Apply ist ein Infrastruktur-/Vertragsfehler, kein normaler „Kauf abgelehnt“-Fall. Ohne tatsächlich implementiertes Recovery darf das System weder vollständigen Rollback behaupten noch einfach weitersimulieren. Erwartete Ablehnung, Budgetende und technische Faults bleiben unterscheidbar.

## PW-07 – Commit-Ergebnis, Ereignisse und Folgearbeit

**PW-S02 ist umgesetzt und abgenommen.** Ein erfolgreich committed Sachverhalt kann als typisiertes `IDomainEvent` in deterministische Folgearbeit übergehen. Dafür existieren `IDomainReaction<TEvent, TWorkItem>`, `DomainReactionContext<TWorkItem>` und eine explizit komponierte `DomainReactionDispatcher<TEvent, TWorkItem>`.

`DomainEventTransactionCoordinator.TryCommitAndPublish(...)` sichert die benötigte Follow-up-Kapazität vor dem Transaction-Commit. Kann die autoritative Event-Publikation nicht garantiert werden, beginnt der Commit nicht. Bei fachlicher Prepare-Ablehnung wird die Reservation verworfen. Für Fälle, in denen der Event-Payload erst aus dem tatsächlichen Commit-Ergebnis entstehen kann, reserviert die interne Prepared-Follow-up-Grenze vorab nur Kapazität, Scheduler-Key und Reihenfolge; nach erfolgreichem Commit wird der konkrete Event-Payload erzeugt und über die Reservation publiziert.

Die Scheduler-Reservation bleibt intern. Fremde Module erhalten kein allgemeines Recht, Queue-Kapazität oder vorbereitete Scheduler-Einträge selbst zu publizieren.

PW-S02 führte bewusst kein universelles `CommitResult`-Objekt ein. PW-S04 bestätigt diese Entscheidung: Combat verwendet sein vorhandenes fachliches `DefeatAwareResourceTransactionCommitResult`, um danach ein typisiertes `DamageCommittedEvent` zu erzeugen. Daraus entsteht weiterhin kein Kernel-weites Commit-Result-Modell.

Mehrere Reactions werden in expliziter, beim Aufbau eingefrorener Kompositionsreihenfolge ausgeführt. Daraus geplante gleichzeitige Follow-ups behalten über Scheduler-Wave und Sequence eine reproduzierbare Reihenfolge. Es gibt keine Reflection-Discovery, dynamische Prioritätsregistrierung oder Mutation der Reaction-Liste während des Runs.

Eine unerwartete Reaction-Exception macht einen bereits erfolgreichen Commit nicht rückgängig. Die autoritative Runtime geht in den Faulted-Zustand und verarbeitet danach keine weitere Gameplay-Arbeit. Technische Diagnosefehler dürfen später separat behandelt werden, ändern diesen Gameplay-Vertrag aber nicht.

Die Unterscheidung zwischen dem Sammeln und dem späteren Dispatch von Events ist auch in der ergänzend geprüften Fachquelle beschrieben. Plaquewrights Wahl „Reactions nach Commit“ stammt jedoch aus der Projektübergabe und der bestätigten PW-S02-Umsetzung, nicht aus einer übernommenen Microservice-Architektur. [E1, E7, W2]

## PW-08 – Pre-Commit-Regeln sind keine Post-Commit-Reactions

Eine Verteidigung, ein Kostenersatz oder eine Intervention gegen eine bevorstehende Niederlage muss bei der Resolution beziehungsweise Vorbereitung wirken, wenn sie den ursprünglichen Commit ändern soll.

Beispiel: Eine Regel „dieser Treffer darf den letzten Lebenspunkt nicht verbrauchen“ ist nicht als Reaktion auf bereits veröffentlichten Tod umzusetzen. Eine Wiederbelebung nach dem Tod wäre eine andere Regel mit anderer Ereignisgeschichte.

Ebenso sind Damage, Cost, Recovery, Prevention und finale Ressourcenwerte nicht austauschbar. Welche speziellen Mechaniken ein Spiel daraus bildet, liegt im Domain-/Regelprofil, nicht im Kernel.

Die im Code vorhandene Recovery-basierte MinimumCurrent-Intervention bleibt genau das. Ihre Tests werden nicht allein wegen der Framework-Neuausrichtung in Prevention umgedeutet. [E2, E3]

PW-S04 belegt diese Grenze nun in einem vollständigen Combat-Pfad: Eine PreDefeat-Recovery verändert den noch nicht committed Resource-Draft und kann dadurch einen neuen Defeat-Übergang verhindern. Erst nach dem Defeat-aware Commit entsteht `DamageCommittedEvent`; seine Reaction beobachtet den committed State und plant neue Arbeit. Damit ist Pre-Commit versus Post-Commit nicht nur Zielvertrag, sondern für den S04-Referenzfall ausgeführt. [E9]

## PW-09 – Zeit, Scheduler und Eingabegrenze

Der Host übersetzt Tastatur, AI-Entscheidungen oder externe Dienste in zugelassene Simulationseingaben. Geräte-Input bleibt außerhalb des Kernels; die zeitlich geordnete Aufnahme von Commands ist dagegen ein generischer Simulationsvertrag.

Die B08-Grenze über `ScheduleExternalInput` wird beibehalten. Sie ist keine vollständige Freigabe aller späteren Regeln für Events mit gleichem Timestamp. Ein Ausführungsprofil muss insbesondere festlegen, wann externe Eingaben, Ablaufereignisse und kausale Folgearbeit zugelassen und geordnet werden. [E2]

Neue Arbeit darf nicht vor ihre Ursache springen. Das gilt auch bei gleichem Timestamp und bereits fortgeschrittener Wave. Eine Pause zwischen zwei `RunNext`-Aufrufen darf keinen geheimen Weg schaffen, abgeschlossene Ordnungspositionen erneut zu besetzen.

Gleiche Eingaben brauchen neben der Zeit eine reproduzierbare Reihenfolge. „Gleiche Menge von Inputs“ genügt nicht, wenn ihr Ablauf fachlich verschieden ist.

Budgets für Queue, Arbeit und kausale Ketten sind explizit. Ein unterbrochener oder begrenzter Lauf ist nicht dasselbe wie eine abgeschlossene Simulation. Ein Renderframe-Zeitbudget darf lediglich die weitere Ausführung verschieben, nicht autoritative Ereignisse still verwerfen.

**D-04 ist beschlossen:** Externe Inputs für Simulationszeit `T` dürfen nur aufgenommen werden, solange die autoritative Verarbeitung von `T` noch nicht begonnen hat. Sobald das erste Event bei `T` aus der Queue übernommen wurde, ist die externe Aufnahme für `T` und frühere Zeiten geschlossen. Bereits vorher eingeplante Inputs für `T` bleiben gültig; neue externe Inputs müssen eine spätere Simulationszeit besitzen. Kausale Same-Time-Folgearbeit wird über spätere Scheduler-Waves eingeordnet und darf nicht vor ihre Ursache springen.

PW-S06 konkretisiert eine zweite Zeitgrenze: Ein zeitabhängiger Domain-State kann eine fachliche Zustandsgrenze explizit in `SchedulerPhase.StateBoundary` bei `ExpiresAt` planen. Im Referenzfall wird der Modifier bei `T` entfernt, bevor `Execution` bei demselben `T` liest. Refresh oder Cancel müssen alte Scheduler-Einträge nicht physisch entfernen; deren Gültigkeit wird über den aktuellen Domain-State und die Generation entschieden. Zero-duration wird im ersten Profil abgewiesen, damit ein Same-Time-Child nicht fälschlich als vorgezogene Boundary behandelt wird. [E12]

## PW-10 – Determinismus und RNG

Übernommenes Ziel:

> Gleicher autoritativer Startzustand, gleiche geordnete Inputs, gleiche Regeln und gleicher relevanter RNG-Zustand erzeugen dasselbe autoritative Ergebnis. [E1]

Dazu gehören künftig auch Modul-/Definition-Versionen, Scheduler-Profil, ID-Allokatorstände, ausstehende Arbeit und externe Ergebnisse, soweit sie autoritativ werden.

Deterministische RNG-Kontexte werden nach fachlicher Verwendung getrennt. Neue Loot-Ziehungen sollen nicht unbeabsichtigt Combat-Ziehungen verschieben. Diagnose darf keine Gameplay-Zufallsfolge verbrauchen.

**Beschlossene Entscheidung D-01 – Determinismus-/Replay-Vertrag:** Plaquewright garantiert deterministische autoritative Gameplay-Simulation innerhalb eines ausdrücklich kompatiblen Ruleset-/Modul- und Ausführungsprofils. Gleicher autoritativer Snapshot, gleiche geordnete Gameplay-Inputs, gleiche Ruleset-/Modulversionen, gleiche relevanten RNG-/ID-/Scheduler-Zustände und gleiche autoritative Host Facts müssen zum gleichen autoritativen Gameplay-Verlauf führen.

Rendering, Engine-Physics, Collision Detection und andere externe Systeme gehören nicht automatisch zu diesem Determinismusversprechen. Werden ihre Ergebnisse gameplayautoritativ, müssen sie über einen definierten Provider oder als geordnete Host Facts in die Simulation eingehen; für vollständiges Replay werden diese Ergebnisse soweit nötig aufgezeichnet. Cross-Version-, Cross-Plattform- und Cross-Engine-Bitgleichheit sind spätere Kompatibilitätsziele und keine 2.0-Grundgarantie.

Die bisher verwendeten numerischen Typen werden nicht pauschal ersetzt. Numerische Gültigkeit, Rundung, Vergleich und branchrelevante Entscheidungen benötigen pro unterstütztem Profil nachvollziehbare Regeln. Begrenzte Float-Präzision ist kein Beweis gegen deterministische Simulation, aber auch keine erledigte Plattformmatrix. [W3]

## PW-11 – Host, räumliche Wahrheit und Headless

Der Core enthält keine `Godot.Node`, Unity-Objekte oder Engine-Timer. Adapter können solche Objekte besitzen und in Handles, Commands, Query-Ergebnisse und Präsentationsdaten übersetzen. [E1]

PW-S03 konkretisiert diese Grenze erstmals im laufenden Code: `SimulationSession<TWorkItem>` ist die host-neutrale Fassade über Composition, Runner und Scheduler. Headless-Tests verwenden sie direkt. Die Godot-Hauptassembly referenziert `Plaquewright.Core`, während `Main.cs` nur Composition/Session erstellt, einen externen Host-Input einbringt und die Core-Laufzeit ausführt. Damit existiert kein zweiter Godot-eigener Scheduler oder autoritativer Gameplay-Pfad. [E8]

Eine Engine-Kollision ist nicht automatisch ein portabel reproduzierbarer Gameplay-Fakt. Für autoritativ verwendete räumliche Ergebnisse braucht die jeweilige Komposition einen Vertrag: deterministischer austauschbarer Provider oder aufgezeichnete externe Ergebnisse. Godots Dokumentation garantiert für seine Physics keinen deterministischen Ablauf; ein fixer Takt allein beseitigt diese Grenze nicht. [W1]

Der Kernel implementiert deshalb nicht vorsorglich eine universelle Physik. Die bisherigen Headless-Szenarien verwenden explizite Inputs/Ziele und der Godot-Smoke-Run enthält noch keine autoritative Physics-/Collision-Integration.

Engine-Unabhängigkeit, Headless-Fähigkeit und Engine-übergreifendes Physics-Replay sind getrennte Eigenschaften. PW-S03 belegt die gemeinsame Host-Autoritätsgrenze für Headless und Godot, nicht Physics-/Render-Parität. Unity-/Unreal-Adapter bleiben spätere Integrationsvorhaben, keine in diesem Stand bestätigten Lieferbestandteile.

## PW-12 – Domain-State, Identität und Lebensdauer

Autoritative Zustände gehören ihren Domains. Ein Entity-Identifier kann verbinden, ohne dass jede Entity zwingend Resources, Combat und Inventory besitzen muss.

Runtime-gebundene Objektidentität und persistente Identität sind zu unterscheiden. Eine in-process Ownership-Prüfung wird beim Speichern nicht einfach als serialisierte Referenz übernommen. Wiederhergestellte Handles müssen gegen die neue Runtime und gültige Generationen/Versionen gebunden werden, sobald Wiederverwendung unterstützt wird.

Caches, Projektionen und Prepared-Objekte haben benannte Lebensdauern. Alte Arbeit darf nach Entitätsentfernung, Statuswechsel oder Definitionstausch nicht versehentlich auf einen inzwischen anderen Zustand wirken.

PW-S05 belegt diesen Vertrag für den vereinbarten ersten Core-Scope: Resource-/Entity-State und relevante ID-Allocator werden in-memory beschrieben; Restore erzeugt eine neue `SimulationRuntimeIdentity`. Pending `ApplyResolvedDamageAction` wird aus IDs und bereits resolved Werten gegen die neue Runtime rekonstruiert; ein pending `DamageCommittedEvent` wird als bereits committed Fakt rehydriert, ohne den historischen Commit erneut auszuführen. Der höhere Runtime-/Session-Snapshot restauriert Runner und Runtime gemeinsam, während die Composition bewusst neu gegen Runtime B gebaut wird. Das bleibt eine interne Fortsetzungsgrenze und keine generische Storage-/Savegame-Schicht. [E10, E11]

PW-S06 ergänzt einen unabhängigen Stats-Fall für Lebensdauer und Cache-Gültigkeit. `TimedModifierStateSet` besitzt aktive und inaktive Slots, `Generation` unterscheidet aufeinanderfolgende Lebensdauern eines Keys und `Revision` verändert sich nur bei echter autoritativer Mutation. Stale Expiration-Arbeit ist erwartete Arbeit ohne Mutation. Der Derived-Query-Cache ist nur für dieselbe State-Instanz, dieselbe Revision und denselben Query-Input gültig. Damit bleibt Cache-Gültigkeit an Domain-State gebunden statt an Observer- oder Scheduler-Aktivität. [E12]

## PW-13 – Bestehende Domains und erweiterbare Fachregeln

Resources mit `Current` und `Maximum` bleiben die Grundlage für passende Pools; eine parallele Pools-Domain entsteht nicht ohne unterschiedliche Semantik. Stats beschreiben Werte, Modifier Beiträge zu Werten. Tags klassifizieren; Conditions werten Voraussetzungen aus. [E1]

Eine offene Fachklassifikation kann später registrierte Keys/IDs benötigen. Ein geschlossenes Protokoll wie „diese Operation ist Cost, Loss oder Recovery“ muss dagegen nicht nur der Erweiterbarkeit wegen in freie Strings zerlegt werden.

**Beschlossene Entscheidung D-02 – Combat-Referenzpaket:** Die bestehenden Combat-Regeln bleiben als erstklassiges, austauschbares Plaquewright-Referenz-Regelpaket erhalten. Sie dienen als reales Beispiel und Belastungstest für komplexe Framework-Komposition, sind aber keine Kernel-Abhängigkeit.

Combat-spezifische Begriffe und Abstraktionen bleiben im Combat-Bereich. Eine Regel oder Abstraktion wird erst dann in allgemeinere Framework-Infrastruktur gehoben, wenn mindestens ein weiterer unabhängiger Anwendungsfall denselben Vertrag tatsächlich benötigt. Alternative Spiele dürfen andere Combat-Regelprofile oder gar kein Combat verwenden.

PW-S04 folgt diesem Vertrag: `ApplyResolvedDamageAction`, `DamageCommittedEvent` und `ResolvedDamageApplicationExecutor` bleiben Combat-spezifisch. Allgemeinisiert wurden nur der kleine `ISimulationWorkItem`-Marker und die payload-späte interne Follow-up-Reservation, weil diese keine Combat-Fachsemantik tragen. [E9]

PW-S06 folgt demselben Prinzip im Stats-Bereich: `TimedModifierStateSet`, `TimedModifierValueQuery` und der kleine Query-Cache bleiben fachlich bei Stats. Der Kernel erhält weder einen Statusbegriff noch eine globale Timer-, Cancellation- oder Cache-Registry. Die vorhandene Modifier-Rechenlogik bleibt die Referenzsemantik. [E12]

Kernel-Verträge bleiben für alle Profile gültig. Fachregeln eines Combat-Profils gelten nur innerhalb dieses Profils. Beispielsweise ist eine definierte Schadensbilanz ein Domain-Vertrag; die konkrete Armor-Formel ist keine allgemeine Kernel-Invariante.

## PW-14 – Definitionen, Kompilierung und Authoring

Definitionen beschreiben Inhalte, Runtime-Instanzen deren konkrete Existenz und veränderliche Werte. Authoring darf Strings und komfortable Strukturen verwenden; vorbereitete Laufzeitdaten sollen validierte IDs, stabile Referenzen und effiziente Pläne nutzen. [E1]

Konfiguration und wiederverwendbare Regeln werden beim Laden geprüft, soweit möglich. Laufzeitbedingungen werden an ihrer vorgesehenen Resolution-Grenze gelesen; nicht alle Bedingungen dürfen vorab eingefroren werden.

Definitionstausch muss benennen, welche laufenden Wirkungen ihre alte Version behalten und welche neu auswerten. Ein generischer Hot-Reload-Compiler ist kein erster Implementierungsschritt.

C# ist zunächst die Erweiterungssprache. Datengetriebene Zusammensetzung vorhandener Primitive und neue Primitive in C# widersprechen sich nicht. Ein universeller Visual-Scripting-Interpreter ist nicht erforderlich.

## PW-15 – Replay, Kausalität und Diagnose

Langfristiges Ziel ist ein ungefähr 30 Sekunden umfassendes analysierbares Gameplay-Fenster. Es besteht nicht bloß aus einer Liste von Damage-Zahlen. Snapshots und die seitdem relevanten Eingaben/Zustandsübergänge müssen die gewünschte Wiederherstellung tragen. [E1]

Ein konsistenter Snapshot benötigt Domain-Zustände, Simulationszeit, Scheduler-/Reaction-Zustand, ID-/RNG-Zustände, Versionen und noch nicht verarbeitete autoritative externe Ergebnisse. Die erste Implementierung darf kleiner beginnen, muss ihren Umfang aber benennen.

PW-S05 hat diese erste Implementierung auf `d8826a2` abgeschlossen: Resource-/Entity-State samt Revisionen, aktuelle ID-Allocator, Scheduler-Key/Sequence/Pending Work, Runner-Zeit/ProcessedEvents/D-04-Closure sowie die im Combat-Referenzprofil benötigten pending Typen werden in-memory erfasst. `CombatWorkItemSnapshotCodec` bildet `ApplyResolvedDamageAction` und `DamageCommittedEvent` explizit ab und weist unbekannte Typen ab. `SimulationRuntimeSessionSnapshot<TPayloadSnapshot>` bündelt Runtime und Runner; die Composition wird nicht gesnapshottet, sondern neu für Runtime B aufgebaut. Der derzeitige Core besitzt keine persistenten RNG-Objekte; deterministische Streams werden aus Seed und Kontext erzeugt, sodass im aktuellen Scope kein separater langlebiger RNG-Zustand anfällt. Regeln/Definitionen werden als kompatible externe Definitionen bereitgestellt; ein Cross-Version-Vertrag ist nicht zugesagt. [E10, E11]

Snapshots entstehen nur an **quiescent boundaries**: kein Participant-Apply ist aktiv, kein Commit ist halb veröffentlicht und der aktuelle autoritative Ausführungsschritt ist abgeschlossen. Konkret weist der aktuelle Scheduler Snapshot-Capture bei aktivem Event und bei offener Prepared-Follow-up-Reservation zurück. Prepared-Objekte und laufende Callback-Stacks sind dadurch kein Snapshot-Bestandteil. Replay läuft in einer gesonderten Runtime oder einer anderweitig klar getrennten Analyseinstanz und darf keine realen Käufe, Netzwerkaufrufe oder sonstigen Host-Side-Effects erneut auslösen.

Der abschließende S05-Replay-Beweis pausiert nach einem bereits ausgeführten Damage-Commit, während dessen committed Event und weiteres resolved Damage noch pending sind. Nach dem Snapshot werden in Runtime A und Runtime B identische geordnete neue Inputs eingebracht. Verglichen werden Runner-/Scheduler-Ergebnis, Trace-Reihenfolge, Event-Fakten einschließlich der generierten Gameplay-/Damage-/Hit-IDs, Resource-State/Revision sowie die nach der Snapshot-Grenze neu entstehende Ledger-History und Provenienz. Die Ledger-History vor dem Snapshot ist im aktuellen Profil nicht Teil des Snapshots. [E11]

PW-S06 zeigt, dass die Fortsetzungsgrenze auch einen neuen domainlokalen Zustand tragen kann: `TimedModifierStateSetSnapshot` erhält aktive und inaktive Slots, Generation, Revision, Modifierdaten und Ablaufzeit. Der Abschlussbeweis restauriert zusätzlich pending Expiration-Arbeit aus einem Runner-Snapshot und baut die Composition neu gegen den restaurierten Stats-State. Eine alte Generation bleibt stale; die aktuelle Generation läuft am vorgesehenen StateBoundary ab. Die hierfür verwendeten Work-Item-Snapshottypen sind test-only und erweitern den S05-Combat-Codec nicht zu einem universellen Serializer. [E12]

B07 transportiert eine kausale Gameplay-Execution in Ressourcenprovenienz. Das ist ein Baustein, noch kein vollständiger Graph aus Action, Effect, Rule, Transaction und Event. [E2]

Vorgeschlagene Trennung: immer benötigte kompakte Identitäten; autoritative History nur soweit für die zugesagte Funktion nötig; optionales Detailtracing in den Stufen Off, Minimal, Gameplay und Full Debug. Tracing an/aus darf das Gameplay-Ergebnis nicht ändern.

Ledger, Replay-History und Debug-Trace benötigen jeweils eine eigene Aufbewahrungsstrategie. Ein Resource-Ledger ist weder automatisch ein universeller Event Store noch ein dauerhaft vollständiger Spielstand.

## PW-16 – Exact und Approximate Simulation

Beide Strategien bleiben ausdrücklich Teil der Vision. Exact bedeutet semantische Gleichheit im definierten Profil, nicht das Nachspielen jedes Renderframes. Approximate erlaubt bewusst benannten Fidelity-Verlust. [E1]

Ein exakter Zeitsprung darf relevante Thresholds, Ablaufereignisse, Reihenfolgen, RNG-Verwendung und beobachtbare Folgeeffekte nicht überspringen. Gleicher Endbestand allein genügt nicht, wenn zwischendurch ein Kill, Proc oder Kauf möglich gewesen wäre.

Der S06-Referenzfall liefert dafür erstmals eine konkrete nicht-Combat Ablaufgrenze: `ExpiresAt` plus `StateBoundary` ist semantisch relevant, weil eine Query bei demselben Timestamp vor oder nach dem Ablauf unterschiedliche Werte liest. Ein späterer Fast-forward darf solche Boundaries nicht überspringen oder nur aus Endwerten rekonstruieren. [E12]

Für kontinuierliche Vorgänge meldet das verantwortliche Modul Integrationsmöglichkeiten und die nächste relevante Grenze. Fehlt ein belegter exakter Fast Path, verwendet es den Referenzpfad oder meldet die nicht unterstützte Fähigkeit.

Approximation ist ein gewähltes Profil mit Herkunft und Gültigkeitsbereich. Ein Lauf wird nicht heimlich approximiert, weil sein Arbeitsbudget erschöpft ist.

## PW-17 – Performance und begrenzte Kosten

Das Ziel von ungefähr 500 individuell simulierten Gegnern ist ein künftiges Lastszenario, kein bereits bestandener Benchmark. Es benötigt definierte Aktivitätsraten, Targets, Status, Hardware, Laufzeiten und Speichergrenzen. [E1]

Gemessen werden mindestens Laufzeitverteilung, Arbeit pro Simulationssekunde, Allokationen, GC-Verhalten sowie Queue-, Ledger- und Trace-Wachstum. Numerische Zielbudgets werden im jeweiligen Benchmarkprofil beschlossen; dieses Manifest erfindet keine Messwerte.

Batching, kompakte Handles, domainnahe Speicher, Cache-Invalidierung und spezialisierte Ausführungspfade sind erlaubt. Sie müssen gegen den Referenzpfad die für das Profil relevante Semantik erhalten.

PW-S06 hat einen kleinen Cache bereits semantisch abgesichert, aber **nicht** als Performancegewinn abgenommen: `TimedModifierValueQueryCache` muss gegen die direkte Query denselben Wert liefern und invalidiert an State-Identität/Revision/Input. Ob dieser oder andere Caches unter Last sinnvoll sind, ist Gegenstand von PW-S07 und D-06. [E12]

Ein AoE gegen 300 Ziele braucht nicht zwingend 300 schwergewichtige generische Transactions. Die atomare fachliche Grenze und die Ergebnishistorie dürfen aber auch nicht allein zugunsten eines schnelleren Loops verändert werden.

Keine pauschale Null-Allocation-Garantie: Stattdessen bekannte Budgets und gemessene Hot Paths. Reflection in API-Tests oder einmaliger Registrierung ist von Reflection in hochfrequenter Gameplay-Ausführung zu unterscheiden.

## PW-18 – Was bewusst nicht gebaut wird

Kein universelles Rendering, Audio, Pathfinding, Scene-Management, Asset-Loading, Geräte-Input, Physics-Kernel, vollständiges ECS, BigNumber-System, Multiplayer-Netcode, Runtime-Mod-Sandbox oder großer visueller Editor als Vorbedingung des Frameworks. [E1]

Optional notwendige Adapterverträge können entstehen, bevor ihre vollständigen Systeme implementiert werden. Dabei darf „später erweiterbar“ nicht als bereits unterstützte Funktion beworben werden.

## PW-19 – Entwicklungsweg und Prüfprinzip

Die Baufolge schließt konkrete Zusammenspielpfade, nicht Positionen einer alten Combat-Featureliste. Die ersten beiden neuen Funktionsblöcke sind inzwischen abgeschlossen:

```text
vorhandene Transaction-Grenze
  -> Commit-/Ereignisvertrag                 [PW-S02 abgenommen]
  -> deterministische Reaction               [PW-S02 abgenommen]
  -> kleine explizite Modulkomposition       [PW-S03 abgenommen]
  -> Headless/Godot über SimulationSession   [PW-S03 abgenommen]
  -> Combat als zweiter Referenzfall         [PW-S04 abgenommen]
  -> Snapshot-/Restore-Grundlage              [PW-S05 abgenommen]
  -> expliziter Pending-Work-Codec             [PW-S05 abgenommen]
  -> Runtime-/Session-Fortsetzung              [PW-S05 abgenommen]
  -> deterministischer Replay-Beweis           [PW-S05 abgenommen: d8826a2]
  -> zeitabhängiger Domain-State                [PW-S06 abgenommen]
  -> Derived Query + revisionsbasierter Cache  [PW-S06 abgenommen]
  -> Domain-Snapshot + pending Expiration       [PW-S06 abgenommen: 3ee638a]
  -> definierte Lastprofile und Lastmessung     [PW-S07]
```

Jeder Schritt bringt den kleinsten nützlichen Referenzfall, negative Grenzfälle und eine dokumentierte Änderung mit. Fehlende Verträge werden dort präzisiert, wo sie das Szenario benötigt. Es gibt weder einen Komplettumbau noch einen jahrelangen „erst generischen Kernel fertigbauen“-Vorlauf.

Testnamen sind ein Inhaltsverzeichnis, kein Nachweis der Assertion-Qualität. API-Grenzen werden zusätzlich aus einer Assembly ohne `InternalsVisibleTo` geprüft. Neue Abschlussaussagen nennen Commit, ausgeführten Testumfang und Grenzen. [E3, E4]

## PW-20 – Entscheidungen und Annahme

| ID | Entscheidung | Vorschlag / aktueller Status |
|---|---|---|
| D-01 | Reichweite des Determinismus-/Replay-Versprechens | **Beschlossen:** deterministische autoritative Gameplay-Simulation im kompatiblen Ruleset-/Modul-/Ausführungsprofil; externe autoritative Ergebnisse als Provider/Host Facts; keine allgemeine Cross-Engine-Bitgleichheitsgarantie. |
| D-02 | Rolle der vorhandenen Combat-Regeln | **Beschlossen:** austauschbares erstklassiges Referenz-Regelpaket; Combat bleibt außerhalb des Kernels; Generalisierung erst bei zweitem unabhängigem Bedarf. |
| D-03 | Erster Ausführungs-/Konfliktvertrag | Ein Writer, nicht-reentrant; Überlappungen zusammen vorbereiten oder vor Apply ablehnen. Vorschlag, vor neuer Transaction-Komposition festzulegen. |
| D-04 | Gleichzeitige externe Eingaben und Folgearbeit | **Beschlossen:** Sobald die Verarbeitung von `T` begonnen hat, ist die externe Aufnahme für `T` geschlossen. Bereits zugelassene Inputs bleiben geordnet; kausale Same-Time-Folgearbeit läuft über spätere Waves. |
| D-05 | Ereignispuffer und technische Fehlerpolitik | **Beschlossen:** Benötigte autoritative Event-Kapazität wird vor Commit gesichert. Scheitert die Sicherung, findet kein Commit statt. Eine unerwartete Reaction-Exception rollt einen vorherigen Commit nicht zurück, sondern faultet die autoritative Runtime. |
| D-06 | Performance-/Retention-Profil | Hardware, Last, Grenzen und längste nötige History erst durch Szenarien und Messungen festlegen. |
| D-07 | Adapter-/Distributionsumfang der ersten Freigabe | **Teilweise technisch belegt, als Releasevertrag offen:** Headless und Godot nutzen inzwischen dieselbe Core-Autorität; Paket-/Distributionsform, unterstützte Host-Versionen, weitere Engines und Binärkompatibilität sind noch nicht festgelegt. |

D-03, D-06 und D-07 bleiben technische Detailgates. D-04 und D-05 wurden im Rahmen von PW-S02 entschieden und durch die zugehörigen Tests konkretisiert. PW-S03 belegt inzwischen Headless und Godot als gemeinsame Referenz-Betriebsarten über `SimulationSession`; D-07 bleibt offen, bis Distributionsumfang, unterstützte Host-Versionen und weitere Adapter als Releasevertrag entschieden werden.

**Annahmeprotokoll:** Architektur 2.0 am 17. September 2026 im Projektgespräch angenommen; D-01 und D-02 ausdrücklich bestätigt. D-04 und D-05 wurden anschließend im PW-S02-Durchgang beschlossen und auf `b04fcbe` als Teil des abgenommenen Referenzprofils dokumentiert. PW-S03 wurde am 18. September 2026 auf `f90517a` mit Headless-Nachweisen und echtem Godot-Smoke-Run abgenommen. PW-S04 wurde am 18. September 2026 auf `8bd4dd0` nach 1215/1215 bestandenen Tests als Combat-Referenzszenario abgenommen. PW-S05 wurde über `653c1f5`, `789763c` und `c6b2327` bis zum abschließenden Replay-Nachweis `d8826a2` geführt; der finale S05-Testumfang beträgt 1248 Tests. PW-S06 wurde anschließend über `238a24a`, `0739c31` und `982798e` bis `3ee638a` geführt. Der Nutzer bestätigte final 1267/1267 Tests, vollständigen Build und danach `gcc`. Damit sind der vereinbarte S05-in-memory Core-Snapshot-/Replay-Scope sowie der S06-Zeit-/Derived-Query-Referenzumfang abgenommen. Host-Fact-Replay, reale Side-Effect-Suppression, universelle Work-Item-Persistenz, Cross-Version-, Cross-Platform-, Physics-Paritäts-, Performance- und Retention-Freigaben bleiben separat.
