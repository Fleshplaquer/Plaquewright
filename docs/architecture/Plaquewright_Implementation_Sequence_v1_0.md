# Idler - Implementation Sequence

**Version:** 1.0  
**Stand:** 7. September 2026  
**Grundlage:** [Gameplay- und Combat-Manifest 1.0](Idler_Manifest_v1_0.md) nach Bestätigung der Audit-Abschlussverträge.  
**Status:** Vorgeschlagene technische Baufolge. Kein bereits implementierter Code und keine neue implizite Gameplay-Entscheidung.

## Leitlinie

**Zuerst einen kleinen korrekten Durchstich bauen, danach Mechaniken schrittweise ergänzen, zuletzt gezielt optimieren.** Tests, Ergebnisbuchungen, Diagnostics und technische Sicherheitsgrenzen beginnen nicht erst am Ende.

Die Reihenfolge unterscheidet sich bewusst von der Diskussionsreihenfolge. Eine Damage-Formel braucht nicht schon einen universellen Trigger-Editor. Eine geteilte Schutzressource benötigt aber ein korrektes Transaktionsfundament, bevor mehrere Routing-Zweige darauf zugreifen.

Der vorhandene funktionierende Toy-Core und die Godot-Debug-Konfiguration werden zunächst als Referenz gesichert. Die neuen Module ersetzen ihn schrittweise. Dieses Dokument verändert kein Repository und behauptet keinen aktuellen Projektzustand außer den Angaben des Gesprächs.

### Fertig-Definition für jeden Schritt

Ein Schritt ist erst fertig, wenn sein fachlicher Vertrag dokumentiert ist, ein kleines Beispiel durchläuft, Fehlerfälle sichtbar diagnostiziert werden und die zugehörigen Tests grün sind. Neue Regeln besitzen definierte Einheiten, Scope, Zeit-/Snapshot-Bindung und Ergebnisfelder. Tests und ein kurzer nachvollziehbarer Trace sind Teil des Schritts, kein späterer Nachtrag.

Nicht unterstützte Features werden ausdrücklich abgelehnt. Leere oder Identity-Phasen sind im kleinen Durchstich erlaubt, solange der Content diese Mechaniken nicht verlangt. Ein nicht implementierter Modifier darf nicht kommentarlos ignoriert werden.

### Überblick

| Abschnitt | Schritte | Sichtbares Ergebnis |
|---|---|---|
| Fundament | P00-P04 | Dokumentierte Regeln, Tests, deterministische Zeit/RNG und atomare Ressourcenänderungen. |
| Erster spielbarer Kern | P05-P08 | Ein echter Headless-/Godot-Durchstich mit Damage, Conversions und verteilter Defense. |
| Zeit und Wechselwirkungen | P09-P12 | Status, Recovery, Stagger, Trigger, Lifecycle, Charm und Runtime-Transformationen. |
| Werkzeuge und Härtung | P13-P17 | Gemeinsame Raumlogik, Build-Analyse, sichere Persistence, Optimierung und Abnahme. |

Die P-Schritte sind Meilensteine, keine Aufforderung, jeweils alles auf einmal zu programmieren. Im Arbeitsdialog wird ein Meilenstein in kleine Aufgaben mit Test und Review zerlegt.

## P00 - Regelbaseline und Golden Fixtures sichern

**Ziel:** Den angenommenen Stand im Projekt verankern, bevor alter Chattext wieder zu konkurrierenden Regeln wird.

Manifest, Änderungsregister und Referenzprüfungen werden versioniert. Die Python-Prüfungen dienen als kleine unabhängige Orakel; sie werden nicht als fertiger Game-Core behandelt. Jeder Implementierungsbereich verweist auf seine Manifest-IDs und Q-Szenarien.

Ein minimales Feature-Register trennt Supported, Planned und Deferred. Beispielwerte erhalten den Zusatz Fixture/Testprofil, damit sie nicht unbemerkt als Balancewerte ins Spiel gelangen.

**Abnahme:** DamageTaken, R01, R02, R03 und H01 besitzen je ein kurzes eindeutiges Testbeispiel. Ältere widersprüchliche Beispiele sind als ersetzt markiert. Der ursprüngliche Toy-Core ist nachvollziehbar gesichert.

**Noch nicht:** Weitere Mechanikfamilien, Content-Editor oder universelle Bibliothek entwerfen.

**Bezug:** M00, Anhang A/C. **Voraussetzung:** keine.

## P01 - Kleine Projekt-/Teststruktur und technische Baseline

**Ziel:** Produktionscode und Tests können ohne Godot-Szene laufen; F5-/Godot-Workflow bleibt erhalten.

Die vorhandene Lösung wird zuerst tatsächlich inspiziert. Als Ausgangspunkt genügen eine enginefreie Core-Bibliothek, ein Testprojekt und das vorhandene Godot-Projekt. Ein separater Headless-Host kommt erst hinzu, wenn P05 ihn braucht; weitere Packages/Projekte nicht auf Vorrat.

Toolchain und Testframework werden in den tatsächlich unterstützten Versionen gewählt und festgehalten. Der Produktionszahlentyp, die erste Zeitrepräsentation und die erwartete Incremental-Größenordnung werden vor breiter API-Festlegung entschieden. Eine kleine Domänengrenze für Damage/Resource-Mengen ist sinnvoll; keine abstrakte Zahlenbibliothek nur für hypothetische Fälle.

**Abnahme:** Wiederholbarer Build, ein Referenzrechentest, strukturierter Testfehler, dokumentierter Numerikvertrag. Core hat keine Godot-Abhängigkeit. Exact-Replay-Erwartung und numerische Vergleichstoleranz sind getrennt.

**Noch nicht:** Alle Typen des Manifests als leere Klassen anlegen oder SDK-Versionen aus altem Chat ungeprüft übernehmen.

**Bezug:** M01, M03, M21. **Voraussetzung:** P00.

## P02 - IDs, Tags, Stats und Conditions als lesbares Referenzmodell

**Ziel:** Eine Definition kann korrekt klassifiziert und ihre wenigen benötigten Stats berechnet werden.

Stabile Content-IDs von Runtime-Handles trennen. Registry, Tag-DAG, Direct/EffectiveTags und quellenbezogene Beiträge umsetzen. Danach wenige primitive Stats und Condition-Operatoren mit benannter Zustandssicht. Flat, Increased/Reduced und More/Less erhalten eigene Operationen und Einheiten.

Der Compiler startet als einfacher Validator und Resolver. Eine Rule-Quelle und die Begründung für Match/NoMatch sind von Beginn an diagnostizierbar. Komfortable Authoring-Strings werden einmal aufgelöst; Bitset-Mikrooptimierung ist noch keine Voraussetzung.

**Abnahme:** Unbekannte IDs und Zyklen werden mit Quelle gemeldet. Zwei Area-Buffs bleiben unabhängig. Ein Modifier greift nicht mehrfach aufgrund von Implikationen. Bedingtes Increased ergibt 300 statt 400 im Referenzbeispiel. Reihenfolge ungeordneter Beiträge ändert das Ergebnis nicht.

**Noch nicht:** Beliebige Script-Ausführung, komplexer Editor oder komplette Behaviour-Bibliothek.

**Bezug:** M02-M04, Q22. **Voraussetzung:** P01.

## P03 - Deterministische Zeit, RNG-Domains und Scheduler-Grundgerüst

**Ziel:** Fachliche Arbeit lässt sich reproduzierbar und begrenzt ausführen.

SimulationTime, stabile Event-Identitäten, Scope-/Domain-Schlüssel, Generationsschutz und eine Queue für atomare Einheiten umsetzen. Das erste Scheduler-Profil dokumentiert die vollständige Gleichstandsordnung. Direkte lokale Arbeit und nachgelagerte Zero-Delay-Folgen werden getrennt.

Damage-Support inklusive Endpunkten und Bernoulli-Chance werden getrennt behandelt. Das erste RNG-Verfahren und seine Version werden dokumentiert. Bereits jetzt gibt es maximale deterministische Arbeit/Queuegröße mit einem ehrlichen Incomplete-Resultat, nicht erst beim Performancepass.

**Abnahme:** Gleiches Inputskript erzeugt denselben Trace/State-Hash. Ein veralteter Timer trifft keine neue Instanz im wiederverwendeten Slot. Endliche Zero-Delay-Kette läuft durch; unendliche und stark verzweigende Spielzeugketten enden sichtbar am Sicherheitsbudget.

**Noch nicht:** Alle Continuous-/Spatial-Integratoren, Netzwerk-Replay oder plattformübergreifende Bitgleichheit versprechen.

**Bezug:** M08-M09, M20-M21, Q16/Q18. **Voraussetzung:** P02.

## P04 - ResourceSet, Roles und Resolution-Transaktion

**Ziel:** Ein Vorgang kann mehrere Pools korrekt prüfen, reservieren und gemeinsam verändern.

ResourceDefinition/-Instance, mehrwertige Roles und explizite Auswahl implementieren. Ein minimaler Pending State mit Budgetreservierungen und Bruttoledger bildet Gain, Loss und mehrteilige Kosten ab. Cause, Mechanism und ChangeKind bleiben getrennt. DefeatConditions werden im projizierten State vor Commit geprüft.

Noch ohne komplizierte Damage-Layer werden die kritischen Operationen getestet: SharedReserve, Shortfall, RemainAtOne, echte Restoration, Cost-Failure und CapacityClamp. UI/Observer lesen erst akzeptierte Ergebnisse.

**Abnahme:** 300 Reserve werden nicht zu 600; Loss 40 plus Restore 20 liefert NetDelta -20 und trotzdem GrossLoss 40. RemainAtOne committed 100 -> 1 ohne fiktive Heilung. Fehlende zweite Skillkostenkomponente bezahlt nicht die erste. Mehrere Rollen duplizieren kein Budget.

**Noch nicht:** Gesamtes Resource-Replacement oder beliebig viele Spezialwährungen. Zwei bis drei benannte Testpools reichen.

**Bezug:** M05, M15, Q08-Q11/Q21. **Voraussetzung:** P02-P03.

## P05 - Erster vertikaler Durchstich: ein Skill, ein Target, echte Ergebnisse

**Ziel:** Einen kleinen durchgängigen Kampf tatsächlich ausführen, nicht weiter nur Infrastruktur bauen.

Eine SkillInstance aktiviert eine SkillExecution, bezahlt einfache Kosten, erzeugt eine DamageExecution und eine bewusst einfache direkte Delivery gegen ein Testtarget. Eine HitExecution verwendet DamageRange/Policy, per-hit RollGroup, geteilten CritSample und targetbezogenes CritResult. Noch nicht aktive Defense-Phasen sind ausdrücklich Identity, nicht halb implementierte Platzhalter.

Ein minimaler Result-Ledger, Counter-Modus und lesbarer Trace begleiten den Ablauf. Der Headless-Host und eine sehr kleine Godot-Debugansicht benutzen dieselbe Core-Funktion. Die Godot-Ansicht kann State, Buttons und Ergebnisse anzeigen; sie implementiert keine eigene Schadensformel oder Kollision.

**Abnahme:** Fünf konfigurierte HitAttempts derselben DamageExecution rollen eigene Damagewerte, teilen ihren CritSample und liefern korrekte einzelne Resourceänderungen. Mehrere Paths liefern einen Hit-Event. Off/Counters/Trace verändern keinen Endstate.

**Noch nicht:** Echte Projektilphysik, Inventar-UI, viele Gegner oder spektakuläre Effekte. Dieses Szenario ist ein direkter, räumlich trivialer Durchstich, kein fertiger Projectile-Test.

**Bezug:** M06-M10, M12/M22, Q03-Q04/Q19. **Voraussetzung:** P01-P04.

## P06 - Damage Assembly, Conversion und typisierte Output-Pläne

**Ziel:** Die zentralen offensiven Transformationsverträge umsetzen, bevor Contentkombinationen wachsen.

Lokale Weapon-Assembly, SkillBase, Flat/Added-Effectiveness und pro DamageExecution definierte Contributions ergänzen. Type-Conversion normalisiert und erhält eigene Mengen. LogicalRanges respektieren CollapseBetween-Grenzen.

R02 wird konkret: Raw/Converted/Scaled-Reader und typisierter OutputPlan werden getrennt. Spätes Extra und neu erzeugte DoT-Seeds erhalten ihre eigene Conversion/Scaling-Verarbeitung ohne Parent-Assembly oder automatisches Feedback. Der erste Compiler darf einen begrenzten unterstützten Satz an Operatoren haben und andere Pläne klar ablehnen.

**Abnahme:** Q01-Q03/Q21. 30 ConvertedCold-ExtraFire werden durch ihren Plan 30 Lightning; Parent bleibt unverändert. Source-/Child-Scaling ist einmal je Knoten. Unaggregierte Referenz und erste einfache Aggregation stimmen überein.

**Noch nicht:** Freies Stage-Scripting, aggressive Optimierung oder zufällige Dynamic-Goto-Pipeline.

**Bezug:** M08/M10-M11. **Voraussetzung:** P05.

## P07 - Hit-Defense und gemeinsame Armor-Budgets

**Ziel:** Einen konkreten Treffer durch die bestätigten defensiven Hitphasen auflösen.

Eligibility, Availability und Resolution von Evasion, Dodge/Parry, Glancing und Block schrittweise ergänzen. Für den ersten Test dürfen Fenster/Charges ausdrücklich im Fixture gesetzt sein; vollständige Aktivierung folgt beim Lifecycle.

Armor erhält ein Budget pro Hit, anschließend CurrentType-Resistance und getrenntes Taken-Scaling. Bypass wirkt an seinem Mechanismus. Rating-/Accuracy-Formel und Resistenzgrenzen werden als erstes dokumentiertes Test-/Ruleset-Profil gewählt; sie sind keine aus dem Manifest abgeleiteten Zahlen.

**Abnahme:** Q06. Hit 1000 gegen Capacity 600/Guard 50 % liefert 700, unabhängig von interner Fragmentzahl. Fünf echte Hits unterscheiden sich korrekt von einem großen. Crit/Block/Glancing-Events bleiben Hit-weit, Wirkung Path-abhängig.

**Noch nicht:** Mehrere redundant benannte Defense-Systeme, lokale Hit-Locations oder komplettes Balancing aller Klassen.

**Bezug:** M12-M13. **Voraussetzung:** P05-P06.

## P08 - Routing, Protection und finanzierte Resource Guards integrieren

**Ziel:** Die schwierige Grenze zwischen Damage und mehreren gemeinsam genutzten Pools schließen.

Routing pro Path, Normalisierung, Destinations/Role-Bindings, Shortfall- und Fallback-Pläne umsetzen. Danach actor-spezifische ProtectionChain mit Bypass, proportionaler Allocation und unterschiedlichen Kostenverhältnissen. Geteilte Budgets verwenden die Transaktion aus P04.

Fortification finanziert qualifizierte ResourceProtection; bei Ersatz von Ward-Absorptionskosten wird die Ersatzreserve tatsächlich reserviert. Ein eigener Testactor bildet die Fairy mit vitalem Ward ab. Reihenfolge ist konfigurierbar, nicht nach Namen sortiert.

**Abnahme:** Q08-Q11. PostTakenScaling 600, Barrier 400, ResourceGuard 50 ergibt DamageTaken 200 und LifeLoss 150. Zwei Claims verbrauchen keine doppelte Reserve. 80 Absorption mit Kostenersatz benötigt die vollen 40 Ward plus 40 Reserve. Kein unresolved Shortfall wird still verworfen.

**Noch nicht:** Alle exotischen Redirect-/Reserveklassen. Das erste Mana-Shield-Fixture benennt seine Fallback-Policy ausdrücklich.

**Bezug:** M12/M14-M15. **Voraussetzung:** P04/P07.

## P09 - Persistente Effects, Status und lokale Application-Reihenfolge

**Ziel:** Ein echter Hit kann einen eigenen Status erzeugen, der korrekt zeitlich weiterlebt.

EffectTime, Lifetime, Pulse-/Continuous-Behaviour und StatusInstances umsetzen. Mit Independent und einem einfachen begrenzten Stackmodell anfangen; zusätzliche bestätigte StackPolicies einzeln mit eigenen Tests ergänzen.

Direkte OnHit-Applications werden in der lokalen Hittransaktion nach Source-Damageberechnung und vor DeathSnapshot vorgenommen. H01 sichert BeforeHit/AfterCommit-Sichten. Defeat-/Cleanup-Reihenfolge, Removal-Causes und Snapshot-Lebensdauer sind Teil dieses Schritts.

Der erste Integrator behandelt konstante Raten und explizite Ratewechsel exakt innerhalb seines Zahlenvertrags. Er entdeckt Depletion/Thresholds vor bereits geplanten späteren Events. Nicht unterstützte nichtlineare Integratoren werden nicht simuliert, als seien sie exakt.

**Abnahme:** Q07/Q12/Q13. Hit 1 kann sich nicht durch seinen eigenen Burn qualifizieren; Hit 2 sieht ihn. Letzter Pulse erfolgt vor Expire. LifetimeCut und EffectRate unterscheiden sich. Tödlicher Hit kann Poison in den DeathSnapshot schreiben.

**Noch nicht:** Tausende Status optimieren oder alle Zone-Shapes implementieren.

**Bezug:** M15-M16/M21. **Voraussetzung:** P03/P06/P08.

## P10 - Recovery und Stagger mit individuellen Zeitbeiträgen

**Ziel:** Zeitliche Schulden und Recovery teilen Infrastruktur, ohne ihre Semantik zu vermischen.

Eine kleine Contribution-/Schedule-Infrastruktur hält eigene Start-/Endpunkte und unterstützt aggregierte konstante Raten. Daraus entstehen separat Leech, Recoup und Stagger-Pläne. Nicht ein gemeinsamer globaler Timer.

Recovery bekommt overkillfreie Basis in passenden Einheiten, CapWaste, ContinueAndWaste, Overflow und getrennte Speed-/Amount-Regeln. Stagger speichert bereits mitigierte Payloads und beginnt beim Release bei aktuellem Routing/Protection. Sein gespeicherter Anteil wird im Ledger nicht als zusätzlicher Damage gebucht.

**Abnahme:** Q14-Q15. Die drei Referenz-Leech-Beiträge zahlen 420 mit ihren eigenen Endzeiten aus. Stagger wendet Resistance nicht nochmals an. Aktuelle Barrier kann einen späteren Release absorbieren. Feinere Integration erzeugt weder mehr Gesamtwirkung noch mehr Procs.

**Noch nicht:** Jede denkbare Pause-/Restzeit-Policy oder live veränderte nichtlineare Schedule vorweg generalisieren.

**Bezug:** M17-M18/M22. **Voraussetzung:** P08-P09.

## P11 - Trigger und vollständigen Skill-Lifecycle ergänzen

**Ziel:** Bestehende Primitive zu echten automatisierten Buildketten verbinden.

Der Trigger-Resolver prüft Filter, Availability, AttemptQuota, gecachten RollScope und Erfolgsquota. Er unterscheidet Trigger zugesagt, Skill gestartet, committed und abgebrochen. ICD/Charges besitzen ihren eigenen State.

Den minimalen Skill-Lifecycle aus P05 um Cast-/Attack-Time, Commit, Interrupt, Refund, Cooldownstart, Charges und Channeling erweitern. Jede neue Policy wird an einer kleinen Skilldefinition getestet. Triggered Skills dürfen nur ausdrücklich konfigurierte Requirements umgehen.

Dodge-/Parry-Aktivierung kann jetzt von automatischen Commands oder manuellen Testeingaben kommen. Eine späte ResourceLoss-Prognose wird nur als abgesicherte rein lesende Vorschau genutzt, nicht durch zweimalige Auflösung derselben Kosten/Rolls.

**Abnahme:** Ein Versuch je DamageExecution bleibt 30 % statt der Fünfversuchswahrscheinlichkeit. Finite Zero-Delay- und zeitlich wiederkehrende Ketten laufen korrekt; Sicherheitsbudget meldet Incomplete. Kosten, Quoten und Cooldowns werden nicht doppelt verbraucht. Post-Trigger manipulieren keinen fertigen Source-Hit.

**Noch nicht:** Umfangreiche Sandbox-/Macro-UI oder komplette Autoplay-KI. Ein kleines reproduzierbares Command-Skript reicht.

**Bezug:** M06/M20, Q07/Q18/Q21. **Voraussetzung:** P09-P10.

## P12 - Runtime-Transformationen, Ownership und Resource-Migration

**Ziel:** Die gewünschte Build-Flexibilität am laufenden Spielzustand beweisen.

Quellenbezogene Overlays und neue immutable Konfigurationsversionen ergänzen. Zunächst FullRebuild statt komplizierter Invalidierung. Mehrwertige Resource Roles, Add/Remove/Replace, Schutzreihenfolge und Policy-Contributors dürfen sich kontrolliert ändern.

Charm erhält bestehende Status-Herkunft; Reflect/Deflect betreffen gezielt eine Delivery, nicht den gesamten Volley. Owner, StatsSnapshot, Provider, Parent, NumericSource und Credit-/Leech-Empfänger bleiben getrennt. Noch nicht existierende Empfänger und veraltete Handles werden sichtbar behandelt.

**Abnahme:** Q10/Q12/Q16/Q22. Poison -> AutoCharm -> MinionDeath -> Explosion funktioniert als fachliches Szenario. Waffenwechsel aktualisiert kein altes Projektil. Entfernte Resource beendet gebundene Recovery und klärt Reservierungen. Zwei Sources eines Tags überleben das Entfernen einer Source korrekt.

**Noch nicht:** Vollwertiges Capture-/Spectre-Inventar oder alle Monster-Sondermechaniken. Ein adversariales Testmonster genügt zunächst.

**Bezug:** M03-M05/M19/M21. **Voraussetzung:** P11.

## P13 - Gemeinsame räumliche Simulation und echte Deliveries

**Ziel:** Headless und Godot erzielen dieselben gameplayrelevanten Target-/Kontaktresultate.

Mit wenigen benötigten Formen und Bewegungsmodellen anfangen, beispielsweise kreisförmige Zonen und gerade Projectile-Bahnen. Die konkreten Verfahren werden dokumentiert. Zeitliche Kontakte, Eintritt/Austritt, TargetFilter, HitHistory und Limit-Scopes laufen im Core.

Pierce, Chain, Return und bewegte Zonen werden nacheinander ergänzt. Der Godot-Host zeichnet die Ergebnisse und liefert Commands, steuert aber nicht über Rendercallbacks eine zweite Hit-Pipeline. Die direkte Delivery aus P05 bleibt als Referenzadapter bestehen.

**Abnahme:** Q05/Q17. Derselbe Seed-/Commandplan trifft unter verschiedenen Render-Refresh-Raten dieselben Targets in derselben fachlichen Reihenfolge. Eine langsame oder ausgelassene Darstellung verliert keinen Core-Kontakt. Fast-forward überspringt keine relevante Zonegrenze.

**Noch nicht:** Allgemeine Physikengine oder plattformübergreifendes deterministisches 3D. Ein abweichender vereinfachter Simulator wird als anderer Modus gekennzeichnet.

**Bezug:** M07/M21. **Voraussetzung:** P03/P09/P12.

## P14 - Combat Insights und Simulator als benutzbare Werkzeuge

**Ziel:** Die schon vorhandenen Ledger/Counter werden zu brauchbarer Build-Analyse.

Reports nach CurrentEffect, RootSkill, Owner und ausgewähltem Zeitfenster anbieten. Messbasis, Units, CompletionStatus und die getrennte Vollständigkeit von Totals/Breakdown/Trace sichtbar machen. Begrenzte Dimensionsanzahl mit Other/Unresolved statt stiller Datenverluste.

Proc-Nenner, Absorption versus Kosten, Stagger-Speicherung versus Zustellung, Brutto-Recovery und Todesanalyse integrieren. Ein Compiler-Inspector erklärt Matches, Quellenbeiträge, Policies und Output-Pläne.

Der Simulator führt zunächst echte deterministische Gameplay-Runs aus. Danach Seedserien, Streuung und Survivor-/Abbruchbehandlung. Average/Min/Max sind klar benannte Diagnosemodi, keine automatisch richtigen erwarteten oder extremen Gesamtresultate.

**Abnahme:** Q19. 800 Cloud-Damage bleibt über alle Sichten 800. Off/Counters/FullTrace und ein fehlerhafter Observer verändern keinen Core-Endstate. Geteilte CritSamples werden nicht als unabhängige Projektilstichproben ausgegeben. Trace-Puffer bleiben nach Wiederverwendung korrekt.

**Noch nicht:** Ein einzelner magischer Tankiness-Score oder exakt additive Item-Beitragsprozentwerte versprechen. Lokale Reports brauchen keinen externen Telemetriedienst.

**Bezug:** M22-M23. **Voraussetzung:** P10-P13; Grundlagen bereits seit P04/P05.

## P15 - Authoring-Adapter, Save/Replay und reproduzierbare Bugpakete

**Ziel:** Repräsentativen Content ohne neue Mechanikklassen anlegen und Laufzustände kontrolliert erhalten.

Ein einfaches reales Authoring-Format auswählen und über einen Adapter in die bereits vorhandenen Definitionen übersetzen. Stabile IDs, Schema-/Content-Version, Quellenpfade und Migrationen sind Pflicht. Kein neuer Spezialrechner für Item-/Affix-/Relic-Daten.

Save an Commit-Grenzen, Queue-/RNG-/Snapshot-/Effect-Zustand, gültige Generationen und fehlerhafte Teilschreibvorgänge behandeln. Der konkrete Dateisystemvertrag wird auf unterstützten Umgebungen getestet. Bugpakete enthalten nur ausgewählte benötigte Daten und deklarierte Versionen.

**Abnahme:** Q20. SaveLoad setzt einen komplexen Kampf innerhalb des vereinbarten Vertrags reproduzierbar fort. Unterbrochenes Schreiben zerstört nicht die letzte gültige Generation. Doppelte Displaynamen kollidieren nicht als IDs. Falsche Version oder fehlende ID wird klar diagnostiziert.

**Noch nicht:** Eigenen visuellen Editor, Cloudsave oder beliebige historische Replay-Kompatibilität versprechen.

**Bezug:** M03/M21/M25. **Voraussetzung:** P12-P14.

## P16 - Gemessene Optimierung und technische Härtung

**Ziel:** Performance verbessern, ohne Regeln oder Statistikqualität zu verändern.

Zielhardware und Lastprofile vereinbaren und Baselines aufnehmen. Danach nur nach Messung optimieren: kompakte IDs/Bitsets, geeignete Arrays/Structs, Buffer-Pooling, Contribution-Timelines, erlaubte Path-Aggregation, Listener-gerechte Datenmaterialisierung und zuletzt inkrementelle Rebuilds.

Jede Optimierung wird gegen den Referenzmodus getestet. Rechengrenzen, logische DamageRanges, gemeinsame Budgets und RNG-Gruppen bleiben erhalten. Ein schnellerer anderer Spielmodus ist keine Optimierung desselben Rulesets.

**Abnahme:** Differentialtests Q01-Q22 im unterstützten Featureumfang; State-/Trace-Gleichheit im vereinbarten Vertrag. Soak-Tests zeigen kein mit der Gesamtereigniszahl unbegrenzt wachsendes normales Meter. Breite Ketten, Observerfehler, ungueltige Daten und Exportabbrüche enden kontrolliert. Allocation-/Speicher-/Durchsatzwerte sind mit Messkonfiguration dokumentiert.

**Noch nicht:** Refactoring nur nach gefühlter Eleganz oder ungeprüfte Parallelisierung der Mutation. Konkrete Performanceversprechen folgen Messungen, nicht der Anzahl benutzter Structs.

**Bezug:** M03/M11/M20-M25. **Voraussetzung:** P14-P15, laufende Messungen schon vorher.

## P17 - Integrationsabnahme und erster Content-Zyklus

**Ziel:** Den Kern an wenigen echten Builds akzeptieren, statt weitere Architektur ohne Verbraucher zu entwickeln.

Mindestens ein direkter Skill, ein Shotgun-/MultiPath-Skill, ein Conversion-/Extra-/Derived-Build, ein Status-/Charm-Build und ein Resource-/Stagger-/Recovery-Build werden durchgehend im Headless- und Godot-Host getestet. Die Fairy dient als bewusster Gegenfall zum hardcodierten Life-Modell.

Jede angekündigte Mechanik ist entweder implementiert und getestet oder im Feature-Register sichtbar nicht unterstützt. Ein bekannter TODO-Pfad darf keinen scheinbar korrekten Build-Wert liefern.

**Abnahme:** Q-Matrix für den ersten Scope, dokumentierte offene Balancewerte, nachvollziehbare Combat-/Compiler-Traces, Save/Replay-Test und repräsentativer Performancenachweis. Mutationstests treffen besonders Scope-, Einheiten-, Budget- und Snapshot-Fehler; fehlgeschlagene Mutationen werden bewertet statt nur auf eine Prozentzahl reduziert.

Danach beginnt Contentproduktion mit demselben Ablauf: vorhandene Primitive kombinieren; nur bei einer echten neuen Mechanik Code ergänzen. Architekturänderungen werden am konkreten Gegenbeispiel dokumentiert.

**Bezug:** M01/M24-M26. **Voraussetzung:** P00-P16 im gewählten Anfangsumfang.

## Entscheidungsgates statt neuer Architekturschleife

| Vor Schritt | Noch festzulegen | Warum kein stiller Default |
|---|---|---|
| P01 | Tatsächliche Toolchain, Testframework, erster Numerik-/Zeitvertrag und Wertebereich | Keine erfundene aktuelle SDK-Version oder falsche BigNumber-Garantie. |
| P03 | RNG-Verfahren, Samplingauflösung, erste Timestamp-Gleichstandsordnung | Endpunktwahrscheinlichkeiten und Reihenfolge sind beobachtbar. |
| P06 | Unterstütztes erstes Operator-Set des OutputPlan-Compilers | Ein partieller Compiler muss Unsupported melden. |
| P07 | Erstes explizites Defense-Testprofil | Ratingkurven, Resist-Caps und Guard-Werte sind keine Universalzahlen. |
| P08 | Fallback-/Shortfall-Profile und Verbrauchskurse der Testmechaniken | Unbezahlter Rest ist keine automatische Verhinderung. |
| P09 | Unterstützte Integratoren und Grenzbehandlung | Nicht jede denkbare Formel ist exakt vorwärts integrierbar. |
| P11 | Konkrete Cost-/Cooldown-/Charge-Profile der Beispielskills | Der generische Lifecycle entscheidet nicht die Balance eines Skills. |
| P13 | Erster Spatial-Vertrag und Bewegungsumfang | Gleiche Damageformel bedeutet noch keine gleichen Treffer. |
| P15 | Authoring-Transport, Saveformat und Dateisystem-Durability | Gameplay-Atomicität ist keine Save-Crashsicherheit. |
| P16 | Zielhardware, Lastszenarien und Messschwellen | Kartoffel-PC-Tauglichkeit muss nachgewiesen werden. |

Diese Gates verlangen kleine konkrete Implementierungsentscheidungen. Sie sind kein Auftrag, die bestätigten Gameplay-Verträge erneut vollständig zu diskutieren.

## Was im ersten Scope bewusst nicht erzwungen wird

Ein vollwertiger Content-Editor, alternative Lineage-Spielmodi, allgemeine DoT-to-Hit-Conversion, lokale Armor, komplettes Spectre/Capture-System, netzwerkfähiger Multiplayer und eine separat veröffentlichte universelle Gameplay-Bibliothek sind keine Voraussetzung des ersten Kerns. Sie bleiben nur dort Erweiterungspunkte, wo unsere konkreten Verträge sie bereits vorsehen.

Auch parallele Mutation mehrerer Hits ist keine Startanforderung. Reine Vorberechnung kann später parallelisiert werden, wenn deterministische Commit-/Queue-Semantik und Messergebnisse das tragen.

## Nächster tatsächlicher Implementierungsschritt

**P00 abschließen und P01 beginnen:** Manifest und Prüfnachweise im Projekt versionieren, anschließend die tatsächliche vorhandene Lösung/Projektdateien ansehen und einen enginefreien Core-Test aus dem vorhandenen Workflow ausführbar machen.

Noch keine dreißig Systeme als leere Klassen anlegen. Der erste technische Nachweis ist klein: Ein dokumentierter Regeltest läuft ohne Godot, während der vorhandene Godot-Debugstart weiter funktioniert.
