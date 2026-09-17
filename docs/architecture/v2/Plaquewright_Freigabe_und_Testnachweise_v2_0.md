# Plaquewright – Freigabe, Baseline und Testnachweise 2.0

**Version:** 2.0 · **Datum:** 17. September 2026  
**Dokumentstatus:** Architektur 2.0 angenommen; neue Funktions-/Release-Freigaben bleiben an konkrete Code- und Testnachweise gebunden  
**Technischer Referenzpunkt:** `d39cb54`, vom Nutzer als grün, committed und clean bestätigt

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
| Build / Tests | Laut finaler Nutzerbestätigung grün | In diesem Dokumentationsdurchgang nicht neu ausgeführt |
| Repository | Laut gezeigtem Git-Status clean und synchron | Kein eigener Live-Abruf des Remotes für diese Dokumentation |

Eine vollständig gezeigte frühere Testausgabe enthält **1176 bestandene Tests** nach dem Pre-Defeat-Teil. Spätere Ergänzungen wurden mit `g` bestätigt; 1189 war die zuletzt erwartete Zahl, aber es liegt keine vollständig gezeigte finale Summary mit dieser Zahl vor. Deshalb wird 1189 hier nicht als neu ausgezählter Baselinewert ausgegeben.

Die Architekturübergabe mit 1023 Tests beschreibt einen älteren technischen Stand. Sie ist die aktuelle Quelle der Produktvision, nicht der jüngere technische Nachweis. [E1, E2]

## 2. Was dieser Durchgang ausgeführt hat

Die bereitgestellte Architekturübergabe und die drei alten Markdown-Dokumente wurden gelesen. Das Archiv `zip.7z` wurde für Source-/Dokumentinspektion entpackt; ausgewählte Infrastruktur-, Domain- und Testquellen wurden statisch geprüft. Daraus wurden neue Markdown-Entwürfe erstellt.

Es wurden **keine Produktionsdateien verändert, keine C#-Tests ausgeführt, keine Benchmarks gemessen und kein neuer Codecommit erstellt**. Der verfügbare Archiv-Snapshot ist nicht der vollständige nach allen Chatänderungen aktualisierte Stand `d39cb54`. Einzelheiten stehen im [Quellenregister](SOURCE_EVIDENCE_v2_0.md).

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
| PW-QA-14 | Headless und Godot verwenden dieselbe Autorität | Gleiche definierte Eingaben liefern gleiche Core-Ergebnisse im gewählten Profil | PW-S03 |
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

## 5. Mindestinhalt eines Test-/Release-Belegs

Ein Beleg nennt Commit, betroffene Module und Regelversionen, Buildkonfiguration, Runtime und Plattform, tatsächlich ausgeführte Befehle, Ergebnis sowie bewusst nicht geprüfte Aspekte.

Für Referenzszenarien werden Ausgangszustand und geordnete Inputs festgehalten. Für Performance kommen Hardware, Warm-up, Messdauer, Ereignisraten, Verteilung und Speicherprofil hinzu. Ein kleiner Fixture-Test ist kein 500-Gegner-Benchmark.

Authoring-/Replay-Versionen und Migrationsverhalten werden erst als unterstützt bezeichnet, wenn ihr Roundtrip beziehungsweise Fehlerfall nachgewiesen ist.

## 6. Freigabe der Dokumentversion

D-01 und D-02 wurden am 17. September 2026 ausdrücklich angenommen. Detailgates D-03 bis D-07 können bis unmittelbar vor ihrem jeweiligen Implementierungsschritt offen bleiben.

**Architekturfreigabe 2.0:** erteilt am 17. September 2026.  
**D-01 Determinismus/Replay:** beschlossen.  
**D-02 Combat-Referenzpaket:** beschlossen.  
**Neue Funktionsfreigabe auf Basis dieser Dokumente:** nicht automatisch erteilt.  
**Historischer A/B-Abschluss `d39cb54`:** bleibt dokumentiert.

Eine Dokumentationsannahme ersetzt keinen Code-Nachweis; ein späterer Codefehler ersetzt umgekehrt nicht rückwirkend die festgehaltene damalige Testausgabe.
