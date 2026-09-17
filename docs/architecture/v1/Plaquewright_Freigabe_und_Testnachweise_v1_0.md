# Idler - Freigabe, Quellenstand und Referenznachweise

**Stand:** 7. September 2026  
**Gehört zu:** Manifest 1.0 und Implementation Sequence 1.0.

## 1. Freigabeentscheidung

Die Nutzerbestätigungen haben die Reparaturen aus Audit 1 sowie R01, R02, R03, H01 und die technischen/Telemetry-/QA-Anforderungen aus Audit 2 angenommen. Das Manifest konsolidiert diese Verträge. Ältere damit unvereinbare Chatbeispiele sind nicht mehr gleichzeitig gültige Regeln.

**Derzeit kein bekannter blockierender semantischer Widerspruch im konsolidierten Umfang.** Damit sind Manifest und die technische Implementierungsfolge als Arbeitsgrundlage freigegeben. Nicht freigegeben ist eine angeblich fertige, performant bewiesene oder vollständig getestete Produktionsimplementierung.

Offene konkrete Zahlen, Toolchain-/Numerik-Auswahl, Datenformate, Scheduler-Profilwerte und technische Messschwellen stehen sichtbar in M26 und den Umsetzungsgates. Der erste implementierte Mechanismus muss seine benötigte Konfiguration vollständig mitbringen. Unvollständige oder nicht unterstützte Pläne scheitern ausdrücklich, statt Defaultverhalten zu erfinden.

Eine Freigabe dieses Entwurfs ist weder ein Beweis für alle zukünftigen Datenkombinationen noch eine Garantie, dass bei der Implementierung keine weiteren Fehler gefunden werden. Genau dafür sind Referenzresolver, Validator und fortlaufende QA vorgesehen.

## 2. Verwendete Grundlagen

| Quelle | Verwendung und Grenze |
|---|---|
| Sichtbarer Projektchat | Hauptquelle für die Gameplay-Regeln und ausdrücklichen Annahmen/Korrekturen. |
| Ursprüngliche Projektzusammenfassung | Ziele, Solo-/Performance-/Godot-C#-Rahmen; spätere Entscheidungen gehen vor. Keine aktuelle Toolchain-Verifikation. |
| Audit 1 | Historische F01-F16-/K01-K09-Befunde und 20 Referenzrechnungen. |
| Bestätigte Antworten zu 1, 2, 8 und 11-23 | Lösung der ersten Befunde; Quellen- und Statusgrenzen werden beibehalten. |
| Audit 2 | Risiken aus Entwicklerquellen, 48 Referenzprüfungen und verbleibende Integrationsverträge. |
| Letzte Nutzerbestätigung | Annahme R01-R03/H01 sowie technischer Absicherung, Telemetrie/Statistik und QA. |

In diesem Konsolidierungsdurchgang wurde kein neuer Live-Vergleich der Referenzspiele behauptet. Die Primärquellen und Datierungen der vorherigen Recherche bleiben in [Audit 2](sources/Idler_Architektur_Audit_2.md) und [Quellenregister](sources/Audit_2_Quellen.json) dokumentiert. Das Manifest leitet daraus keine unbekannten Implementierungsdetails fremder Spiele ab.

## 3. Abschluss der Befunde aus Audit 1

| Befund | Jetzt geltender Vertrag | Manifest / Nachweis |
|---|---|---|
| F01: DamageTaken wechselte Bedeutung | Nach Protection, vor ResourceProtection; vorgelagerte Mengen benannt. | M12; V10/V11. |
| F02: Commit versus WouldBeDefeated | Projektion und Intervention vor sichtbarem Commit. | M15; C09/V12. |
| F03: Globale Phasen versus lokale Applications | Lokale Hittransaktion, BeforeHit-Snapshots, kausale Folgearbeit. | M04/M15/M20; C14-C16. |
| F04: Armor pro Fragment | Gemeinsames Budget pro fachlichem Hit. | M13; V04/V05. |
| F05: Geteilte Ressourcen doppelt ausgegeben | Transaktionsweite Reservierungen und Allokation. | M14/M15; C10/C11/V06/V07. |
| F06: Shortfall ging verloren | Bilanz plus ausdrückliche Auflösung/Units. | M14; C13/V08/V09. |
| F07: Caps auf technischen Segmenten | Fachliche Hit-/Pulse-/Zeitbudgets, keine Segment-Procs. | M14/M16/M17; V34/V35 als Vergleich/Gegenbeispiel. |
| F08: Neue Grenzereignisse beim Fast-forward | Integrator meldet Depletion/Threshold/Kontaktgrenzen. | M21; C18/V36. |
| F09: Vorgebackenes Scaling zerstört additive Pools | Vorbereiteter Berechnungsplan erhält bedingte Poolstruktur. | M04; V27. |
| F10: SharedCritResult gegen verschiedene Chancen | R01: geteilter CritSample, targetbezogener Erfolg. | M09; C01-C03/V26. |
| F11: Späte Pfade verpassen Transformation | R02: SourceBasis getrennt vom typisierten OutputPlan. | M11; C04-C07. |
| F12: Zu aggressive Aggregation | Logische Grenzen und künftig benötigte Semantik erhalten. | M08/M11; C23/V20. |
| F13: Offene Roll-Randregeln | Fallback, Deduplikation, Gewichte, Endpunkte, Auswahlgruppen. | M08/M09; V21-V25/C24. |
| F14: Average ist nicht ExpectedOutput | Diagnose-/Monte-Carlo-/exakte Teilmodelle unterscheiden. | M23; V45. |
| F15: Zeitfortschritt/DAG garantiert nicht wenig Arbeit | Technische Budgets und sichtbarer Incomplete-Status. | M20/M25; C19/V38/V39. |
| F16: Structural-/Role-Konflikte | Quellenbeiträge, mehrwertige Rollen, explizite Auswahl/Abhängigkeiten. | M04/M05; V17/V18. |

**"Abgeschlossen" bedeutet hier: eine vom Nutzer angenommene Regel ersetzt die widersprüchliche Auslegung. Es bedeutet nicht, dass der echte C#-Resolver diese Regel bereits nachweislich umsetzt.**

## 4. Zusätzliche Präzisierungen K01-K09 und Rechenkorrektur

| Punkt | Bestätigter Abschluss | Manifest / geplante Tests |
|---|---|---|
| K01 / Nutzerpunkt 13 | Owner, Stats, Parent, Provider und NumericSource getrennt; gezielter Reflect/Deflect. | M19; Q10/Q12/Q16. |
| K02 / 14 | Target-Limit standardmäßig beim zugelassenen Versuch; Kontakt-Episode separat. | M07; Q05. |
| K03 / 15 | LifetimeCut != EffectRate; Terminal-Pulse und Restintervalle definiert. | M16; V30/V31/Q13. |
| K04 / 16 | Quellenbezogene Stackgruppen, explizite StrengthMetric; tödliche direkte Application im DeathSnapshot. | M15/M16; C15/V16/Q12. |
| K05 / 17 | Overkillfreie Leech-Basis mit Units; Recoup auf GrossLoss; Caps/volle Pools verschwenden ohne Refresh. | M18; C08/C13/V32/V33/Q15. |
| K06 / 18 | CostProfiles, atomare Zahlung, Migration, CapacityClamp und diskrete Reste. | M05/M06; V13/Q21. |
| K07 / 19 | Lokale Weapon-Auflösung von globalem Scaling getrennt; pro Execution explizite Contributions. | M10; V46/Q21. |
| K08 / 20-21 | Versionierter Numerik-/Replay-/Save-Vertrag und gemeinsame räumliche Wahrheit. | M21; C17/V37/V44 sowie echte Q16/Q17/Q20 noch umzusetzen. |
| K09 / 22 | Einmalige Buchung, verschiedene Berichtssichten, Diagnose gegen kausalen Vergleich getrennt. | M22/M23; C20-C22/V40-V43/Q19. |
| T17 / 23 | 200 preCrit *2 *0,25 *0,20 = 20. | V47; alte Werte 50/10 nicht weiterverwenden. |

Die Nutzerpunkte 11 und 12 entsprechen F12 und F16 und stehen bereits in der vorigen Tabelle.

## 5. Abschlussverträge aus Audit 2

| ID | Angenommene Regel | Umsetzungspflicht |
|---|---|---|
| R01 | Geteilter Sample statt pauschal gemeinsamem booleschem CritResult. | SameTarget-/MixedTarget-/Debuff-/Forbidden-/Overcap-Tests. |
| R02 | SourceBasis und OutputPlan getrennt, keine neue Parent-Assembly oder automatisches Extra-Feedback. | Zusammengesetzten Plan validieren; Unsupported statt falschem Teilresultat. |
| R03 | Atomar sichtbarer State plus Bruttoledger; Ursachen und Mechanismen getrennt; Protection-Kosten real finanziert. | SharedReserve, VitalWard, Kostenersatz, Loss/Restore und Recoup prüfen. |
| H01 | BeforeHit wird gesichert; direkter Application-Commit vor Folgehit; Trigger als neue kausale Arbeit. | Kein Live-State-Leak bei spätem Dispatch; lokale Phasen nicht global vertauschen. |
| H02 | Immutable Versionen, Invalidierung und generationengesicherte Handles. | Alter Projectile-Snapshot, veraltete Pulse/Expiration, Full-/Incremental-Rebuild. |
| H03 | Kontinuierliche Mechanismen brauchen Integrations-/Grenzverträge. | Neue Depletion/Threshold/Spatial-Ereignisse; Approximation sichtbar. |
| H04 | Arbeit, Speicher, Queue und Diagnose sind begrenzt. | Kein stilles Weglassen von Gameplay; separater Incomplete-Status. |
| H05 | Parameter-/Stat-/Finanzierungs-Abhängigkeiten validieren; Previews rein lesend. | Ungültige Werte, Zyklen und doppelte Preview-Nebeneffekte ablehnen. |

## 6. In diesem Durchgang tatsächlich ausgeführt

| Suite | Ergebnis | Bedeutung |
|---|---|---|
| Audit 1 erneut | 20 benannte Referenzrechnungen bestanden. | Kleine arithmetische Beispiele und Gegenbeispiele. |
| Audit 2 erneut | 48 benannte Prüfungen, 2.941 ausgezählte Fälle bestanden. | Exakte Zahlenraster, Gegenbeispiele und kleine Protokollmodelle. |
| Abschluss-Suite v3 | 24 benannte Prüfungen mit 73 ausgewerteten Assertions bestanden. | Angenommene Abschlussverträge an kleinen Fixtures/Modellen, einschließlich R01-R03/H01. |

Die Suiten überlappen. Die Zahlen werden nicht als unabhängige vollständige Combat-Szenarien addiert. Ein bestandener Gegenbeispieltest bestätigt gerade die Ungleichheit zweier Auslegungen, nicht die Korrektheit eines Game-Cores.

In den unveränderten alten Ergebnissen stehen historische Labels wie `open_counterexample` oder `proposed_hardening`. Diese Labels dokumentieren den damaligen Audit-Zustand; der heutige Annahmestatus folgt den obigen Tabellen und dem Manifest. Sie werden nicht nachträglich gefälscht.

Die neue Suite verwendet bewusst standardbibliotheksbasierte rationale Rechnungen und kleine Modellzustände. Ihre R02-Checks prüfen konkrete Source-/Child-Beispiele; sie implementieren und verifizieren keinen allgemeinen Compiler. Ihre Budget-/Queue-/Ledger-Checks sind ebenfalls keine Produktionsengine.

### Ausführen

```sh
python validation/previous/checks_v1.py
python validation/previous/checks_v2.py
python validation/checks_v3.py
```

Die Programme schreiben ihre JSON-Ergebnisse neben die jeweilige Datei. Python ist hier ein Referenzwerkzeug, keine Änderung der Entscheidung für einen C#-Core. Nicht mit Python-Optimierungsflags ausführen, die Assertions deaktivieren.

### Ergebnisdateien

- [Audit-1-Neulauf](validation/previous/results.json)
- [Audit-2-Neulauf](validation/previous/results_v2.json)
- [Abschlussprüfung v3](validation/results_v3.json)
- [Abschlussprüfung: Quellcode](validation/checks_v3.py)

## 7. Ausdrücklich noch nicht nachgewiesen

Keine aktuelle C#-Spielimplementierung lag diesem Konsolidierungsschritt vor. Es wurden keine Godot-Kämpfe, Produktions-RNGs, Compiler-Optimierungen, echten Physics-Adapter, Save-Crashs, Cloud-Dienste, Benchmarks oder Hardware-/Plattformmatrizen ausgeführt.

Q01-Q22 im Manifest sind verbindliche zukünftige Abnahmeszenarien, keine bereits grüne C#-Testsuite. Kartoffel-PC-Tauglichkeit, dauerhafte Speicherbegrenzung und Save-Durability sind erst nach Implementierung mit dokumentierten Last-/Fehlerprofilen bewertbar.

## 8. Ergebnis

**Manifest 1.0 und Implementation Sequence 1.0 können jetzt als konsolidierte Arbeitsgrundlage verwendet werden.** Weitere Architekturarbeit erfolgt an konkreten neuen Befunden, nicht durch Wiederholung bereits angenommener Grundregeln. Die erste Umsetzung beginnt klein und bringt QA, Ergebnisledger und Trace von Anfang an mit.
