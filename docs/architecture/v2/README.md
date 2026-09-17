# Plaquewright – Architektur 2.0

**Stand:** 17. September 2026  
**Historische Audit-Baseline:** `d39cb54`  
**Aktueller Funktionsnachweis:** PW-S02 auf `b04fcbe`, vom Nutzer als grün / committed / clean bestätigt  
**Dokumentstatus:** Architektur 2.0 angenommen; D-01, D-02, D-04 und D-05 beschlossen; PW-S02 abgenommen

## Was diese Fassung festlegt

Plaquewright ist ein engine-unabhängiges, deterministisches und modulares Gameplay-/Simulations-Framework für C#. Ein einzelnes Idle-Spiel und dessen alte P-Meilensteine bestimmen nicht mehr automatisch die Baufolge. Godot ist erster Referenzhost; autoritative Gameplay-Regeln bleiben im C#-Framework beziehungsweise in ausdrücklich komponierten Domains und Regelpaketen.

Die bestehende Implementierung und der A/B-Auditabschluss werden weiterverwendet. Insbesondere wird die bereits vorhandene externe Transaction-Grenze nicht erneut als fehlende Erstimplementierung eingeplant.

## Beschlossene Richtungsentscheidungen

**D-01 – Determinismus und Replay:** Plaquewright garantiert deterministische autoritative Gameplay-Simulation innerhalb eines kompatiblen Ruleset-/Modul-/Ausführungsprofils. Gameplayrelevante Ergebnisse externer Systeme wie Engine-Physics werden über definierte Provider oder geordnete Host Facts eingebracht und für vollständiges Replay soweit nötig aufgezeichnet. Cross-Version-, Cross-Plattform- und Cross-Engine-Bitgleichheit sind keine v2.0-Grundgarantie.

**D-02 – Combat:** Die bestehenden Combat-Regeln bleiben ein erstklassiges, austauschbares Referenz-Regelpaket und Belastungstest für komplexe Komposition. Combat ist keine Kernel-Abhängigkeit. Combat-spezifische Abstraktionen werden erst dann generalisiert, wenn mindestens ein weiterer unabhängiger Anwendungsfall denselben Vertrag benötigt.

**D-04 – Inputordnung:** Sobald die Verarbeitung eines Timestamps begonnen hat, werden keine neuen externen Inputs mehr für diesen oder einen früheren Timestamp angenommen. Bereits zugelassene Inputs sowie kausale Follow-ups bleiben deterministisch geordnet.

**D-05 – Commit/Event/Fault:** Autoritative Event-Kapazität wird vor dem zugehörigen Commit gesichert. Ein unerwarteter Reaction-Fehler rollt den bereits gültigen Commit nicht zurück, sondern beendet die weitere autoritative Ausführung dieser Runtime als Fault.

Snapshots sind für Replay an **quiescent boundaries** vorgesehen: kein aktiver Apply, kein halb veröffentlichter Commit, kein laufender interner Callback-Stack als Persistenzanforderung.

## Dokumente

| Datei | Zweck |
|---|---|
| [Manifest](Plaquewright_Manifest_v2_0.md) | Verbindlicher Architekturvertrag, Grenzen, Determinismus, Replay und Entscheidungen |
| [Implementierungsfolge](Plaquewright_Implementation_Sequence_v2_0.md) | Vertikale Schritte ab der bestätigten Baseline |
| [Freigabe und Testnachweise](Plaquewright_Freigabe_und_Testnachweise_v2_0.md) | Technischer Anschlussstand und künftige Abnahmen |
| [Architecture Map](ARCHITECTURE_MAP_v2_0.md) | Verantwortlichkeiten und Beispielabläufe |
| [Code Classification](CODE_CLASSIFICATION_v2_0.md) | Einordnung vorhandener Typen ohne pauschale Ordnergleichsetzung |
| [Migration Plan](MIGRATION_PLAN_v2_0.md) | Schrittweise Übernahme ohne Rewrite oder stillen Regelverlust |
| [Glossar](PLAQUEWRIGHT_ARCHITECTURE_GLOSSARY_v2_0.md) | Begriffe und Grenzen |
| [Source Evidence](SOURCE_EVIDENCE_v2_0.md) | Quellen, Snapshotgrenzen und Entscheidungsnachweise |

## Empfohlene Leserichtung

Zuerst Manifest PW-01 bis PW-10 und die Architecture Map. Danach die Baufolge PW-S01 bis PW-S05. Das Quellenregister trennt historische Dokumente, beobachteten Code, bestätigte Auditstände und neue Architekturentscheidungen.

## Nächster Entwicklungsstrang

PW-S02 ist abgeschlossen. Der nächste Schritt ist **PW-S03 – Modulkomposition und zwei Host-Betriebsarten**.

Die bislang im Referenztest manuell verbundenen Bausteine werden in eine kleine explizite Simulation-Komposition überführt. Zuerst muss Door/Alarm headless ohne Combat-Pflichtzustand funktionieren. Danach wird dieselbe Autorität über einen minimalen Godot-Host angesprochen.

Noch nicht Ziel dieses Schritts sind automatisches Plugin-Discovery, Runtime-Hotloading, ein universeller Service-Locator oder ein Umbau des Combat-Systems.

## Ablage und Übernahme

Die Dateien sind für eine gemeinsame Ablage unter `docs/architecture/v2/` gedacht. Alte Idler-Dokumente bleiben historische Entscheidungsgrundlage und werden nicht per Suchen/Ersetzen umgedeutet. Ein Repository-Commit dieser Dokumente ist ein Dokumentationscommit und keine Behauptung neuer Framework-Funktionalität.

D-03, D-06 und D-07 bleiben offene technische Detailgates.
