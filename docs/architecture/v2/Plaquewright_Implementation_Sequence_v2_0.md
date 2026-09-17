# Plaquewright – Implementierungsfolge 2.0

**Version:** 2.0 · **Datum:** 17. September 2026  
**Ausgangspunkt:** Nutzerbestätigte Audit-Baseline `d39cb54`, nicht der alte Idler-P00-Start  
**Status:** Freigegebene Architektur-Baufolge; einzelne Implementierungsschritte werden weiterhin separat durchgeführt und abgenommen

## 1. Ziel der Reihenfolge

Wir bauen das Zusammenspiel eines Frameworks, nicht zunächst ein vollständiges Idle-/ARPG-Regelwerk. Der Weg bleibt vertikal: Jeder Schritt schließt einen kleinen nutzbaren Ablauf über echte öffentliche Grenzen.

Die Architekturübergabe nennt die externe Transaction-Grenze noch als nächsten Schritt. Dieser technische Teil ist im inzwischen dokumentierten Anschlussstand bereits vorhanden. Er wird benutzt, nicht nochmals eingeführt. Die ältere Testzahl 1023 ist ebenfalls kein aktueller Baseline-Nachweis. [E1–E4]

Die neue Reihenfolge verwendet `PW-Sxx`, um weder alte P-Meilensteine noch abgeschlossene A/B-Findings umzunummerieren. Ein PW-Schritt ist eine neue geplante Fähigkeit, kein versteckt wieder eröffnetes Finding.

## 2. Überblick

| Schritt | Ergebnis | Warum jetzt? |
|---|---|---|
| PW-S00 | Framework-Ziel und Baseline nachvollziehbar dokumentiert | Verhindert weitere Arbeit nach falscher Produktvision |
| PW-S01 | Transaction-Komposition hat einen eindeutigen Konflikt-/Lebensdauervertrag | Ereignispublikation darf keine unklare Mutation verdecken |
| PW-S02 | Commit-Ergebnis → Event → deterministische Reaction | Schließt die zentrale modulübergreifende Ausführungskette |
| PW-S03 | Kleine explizite Modulkomposition und Headless-/Godot-Referenz | Beweist Benutzbarkeit außerhalb interner Tests |
| PW-S04 | Zweites fachliches Referenzszenario mit vorhandenen Combat-Bausteinen | Prüft, ob die Grenze mehr als den Door-Fall trägt |
| PW-S05 | Snapshot/Restore und deterministischer Replay-Beweis | Prüft State-Ownership und ausstehende Arbeit früh |
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

Dies ist der vorgeschlagene erste neue **Funktionsblock** nach der Vertragsprüfung.

**Referenzfall:** Die bereits vorhandene Zahlung/Öffnung wird um `DoorOpened` und eine separate Alarm-Reaction erweitert. Das Beispielmodul bleibt außerhalb der Friend-Assembly; der Kernel kennt keine Tür.

**Lieferung in kleinen Teilstücken:**

1. Ein kleines, typisiertes Commit-Ergebnis mit den benötigten kausalen Identitäten.
2. Vorbereitung beziehungsweise sichere Übernahme autoritativer Ereignisdaten ohne Reactions im Apply.
3. Geordneter Dispatch nach vollständigem Commit.
4. Reaction erzeugt neue Work statt rekursiv eine weitere Transaction in den laufenden Apply zu drücken.
5. Ein begrenztes, reproduzierbares Ausführungsprofil für dieses Szenario.

Die Namen sind konzeptionell. Bestehende passende Typen werden vor dem Anlegen neuer Typen geprüft. Es wird kein universelles Nachrichtenformat für sämtliche zukünftigen Domains erzwungen.

**Abnahme:** Keine Zahlung/Öffnung/Event bei Prepare-Ablehnung; beim Erfolg beobachten Reactions beide committed Zustände. Dieselbe Folge entsteht bei wiederholtem RunNext und RunToCompletion. Eine volle autoritative Ergebnisqueue wird vor Commit erkannt oder durch eine bereits gesicherte Pending-Übergabe abgefangen.

Gleicher Timestamp, nächste Wave, spätere externe Eingabe und Handler-Fehler sind eigene Tests. Kein pauschales „nach Commit ist alles unfehlbar“ und kein stilles Weglassen von Gameplay.

**Nicht enthalten:** Vollständiges Status-/Skill-System, globales Reflection-Routing, Netzwerkbus, persistenter Event Store.

## 6. PW-S03 – Modulkomposition und zwei Host-Betriebsarten

Die Funktionen aus PW-S02 werden nicht dauerhaft nur als Test-Helfer verbunden.

**Lieferung:** Eine kleine explizite Komposition, die Module und Regeladapter beim Start verbindet, benötigte Verträge validiert und einen festen Ausführungsplan erzeugt. Ein einfacher Headless-Host führt das Szenario aus; ein minimaler Godot-Host kann dieselben Eingaben übermitteln und dieselben Ergebnisse anzeigen.

**Zwei Konfigurationen:** Door/Alarm ohne Combat; Resources/Door/Alarm mit bezahlter Öffnung. Damit wird auch geprüft, ob unnötige Pflichtabhängigkeiten wie globale Resource-Entities in die generische Runtime geraten.

**Abnahme:** Das externe Modul benötigt keine neuen Kernel-Fachbegriffe und kein `InternalsVisibleTo`. Fehlende Abhängigkeiten werden vor Run abgewiesen. Host-Darstellung schreibt keinen Modulzustand direkt. Gleiche definierte Eingaben erzeugen im selben unterstützten Profil dieselben autoritativen Ergebnisse.

**Migration:** Nur die tatsächlich blockierende Kopplung aus `SimulationRuntimeState` herauslösen. Die bestehende Gameplay-Komposition kann als Convenience-Schicht erhalten bleiben.

**Nicht enthalten:** Automatisches Plugin-Discovery, Runtime-Hotloading, Assembly-Split jedes Verzeichnisses, vollständiges ECS.

## 7. PW-S04 – Zweites Referenzszenario: vorhandenes Combat nutzen

Nach dem fachfremden Türbeispiel prüfen wir die Grenze an dem Bereich, in den bereits die meiste Arbeit geflossen ist.

**Kleiner Ablauf:** Eine Action mit Kosten erzeugt später eine Damage-Auflösung gegen ein explizites Ziel. Resources committed den tatsächlichen Verlust. Ein gewähltes Ergebnis löst eine separate Folgeaktion aus. Bestehende Execution-/Hit-/Damage-Identitäten, Protection, Pre-Defeat und Provenienz werden soweit benötigt benutzt.

Die erste Action kann noch ohne vollständiges `SkillDefinition`-/Channel-/Cooldown-System implementiert sein. Ein Skill-Modul folgt dann, wenn es echte wiederkehrende Lifecycle-Anforderungen trägt, nicht weil „alles ein Skill sein muss“.

**Abnahme:** Der Kernel bleibt fachlich unverändert. Eine Regel vor Commit kann den geplanten Schaden beeinflussen; eine Reaction nach Commit nur neue Arbeit erzeugen. Kosten bleiben Kosten, Damage bleibt Damage. Dieselbe Architektur trägt den Door- und den Combat-Fall.

**D-02 ist beschlossen:** Das bestehende Combat bleibt ein benanntes Referenz-Regelpaket. Dieser Schritt soll zeigen, welche Verträge tatsächlich zwischen Combat und Framework geteilt werden; Combat-spezifische Typen werden nicht vorsorglich generalisiert.

## 8. PW-S05 – Früher Snapshot-/Replay-Beweis

Zunächst ein kurzer Headless-Fall, nicht sofort ein 30-Sekunden-Scrubber.

**Lieferung:** Snapshot an einer sicheren Grenze zwischen vollständigen Ausführungsschritten; Restore in einer neuen Runtime; Replay mit den seitdem nötigen Eingaben. Pending Reactions, geplante Zukunftsarbeit, RNG-/ID-Zustände und Regelversionen gehören zum definierten Umfang.

**Abnahme:** Fortsetzen ohne Snapshot und Restore+Replay liefern denselben autoritativen Verlauf und Zustand. Zwei Snapshots an verschiedenen zulässigen Grenzen können zu demselben Prüfzeitpunkt vorgespult werden. Stale Handles werden nicht an neue falsche Zustände gebunden. Host-Side-Effects werden beim Replay nicht real wiederholt.

**D-01 ist beschlossen:** Abgenommen wird deterministische autoritative Gameplay-Simulation innerhalb des für den Test angegebenen kompatiblen Profils. Autoritative externe Ergebnisse werden als Provider-Ergebnisse oder geordnete Host Facts Teil des Replay-Inputs. Engine-übergreifende Physics-/Bitgleichheit gehört nicht zur Grundabnahme.

**Nicht enthalten:** Vollständiger Savegame-Migrationsdienst, Cloud-Saves, Videos/GIFs oder ein fertiger Timeline-Editor.

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

**Quellen:** [E1–E5](SOURCE_EVIDENCE_v2_0.md).  
**Detailverträge:** [Manifest](Plaquewright_Manifest_v2_0.md).  
**Test-IDs:** [Abnahmekatalog](Plaquewright_Freigabe_und_Testnachweise_v2_0.md).
