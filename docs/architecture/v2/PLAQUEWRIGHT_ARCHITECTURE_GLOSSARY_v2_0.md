# Plaquewright – Architecture Glossary 2.0

**Datum:** 18. September 2026 · **Status:** Lebendes Architekturglossar 2.0  
Die alte gleichnamige Glossardatei ist in der Übergabe erwähnt, nicht als vollständiger Inhalt verfügbar. Diese Fassung definiert die Begriffe für den neuen Entwurf; sie ist kein Katalog bereits existierender C#-Typen.

| Begriff | Bedeutung im Framework | Wichtige Abgrenzung |
|---|---|---|
| Kernel | Allgemeine Koordination von Zeit, Reihenfolge, Identität, RNG, Transactions und Folgearbeit | Kein Combat- oder Ressourcenregelwerk |
| Core | Aktuelle Assembly-/Quellcodebezeichnung | Nicht automatisch identisch mit Kernel |
| Domain | Fachlicher Bereich mit eigenen Zuständen und Invarianten | Resources, Inventory, Combat oder externe Temperature-Domain |
| Modul | Integrierbarer Anbieter von Domain-Funktionen oder Infrastruktur | Muss nicht sofort eine eigene Assembly sein |
| Komposition | Auswahl und Verbindung der Module und Regelverträge für eine Simulation | Darf konkrete Domains kennen, anders als Kernel |
| Module Contract | Explizit bereitgestellte oder benötigte Fähigkeit einer Startup-Komposition | Marker für Verfügbarkeit, kein Service Locator und kein automatischer Instanzresolver |
| Execution Plan | Beim Build eingefrorene Zuordnung konkreter Work-Item-Typen zu Ausführungshandlern | Exaktes Typ-Routing; keine Reflection-Discovery oder polymorphe Mehrdeutigkeit |
| Simulation Composition | Validierte Menge expliziter Module, Contracts und Execution-Handler | Required/Provided prüft Verfügbarkeit; sortiert Module nicht heimlich neu |
| Simulation Session | Host-neutrale Fassade über Composition, Runner und Scheduler | Gemeinsame autoritative Laufzeit für Headless/Godot; kein zweiter Host-Scheduler |
| Regelprofil / Regelpaket | Versionierte fachliche Auswahl von Regeln und Policies | D-02: bestehendes Combat bleibt benanntes austauschbares Referenz-Regelpaket |
| Host | Ausführende Anwendung, etwa Godot oder Headless | Bringt Inputs/Host Facts ein und treibt eine `SimulationSession`; nicht automatisch autoritative Gameplay-Logik |
| Adapter | Übersetzung zwischen Host/Engine und Framework-Verträgen | Darf eine `SimulationSession` besitzen/treiben; keine unkontrollierten Engine-Typen im Core |
| Autoritativer Zustand | Zustand, aus dem die Simulation ihre verbindlichen Ergebnisse ableitet | Nicht eine UI-Anzeige oder Debug-Kopie |
| Query | Lesen einer benannten Zustandssicht | Keine versteckte autoritative Mutation |
| Projection | Noch nicht veröffentlichte Sicht auf geplante Änderungen | Kein bereits committed Sachverhalt |
| Intent / Command | Gewünschte Aktion mit noch offenem Erfolg | `OpenDoor`, nicht `DoorOpened` |
| Action | Fachlicher Ausführungsvorgang | Kann mehrere zeitlich getrennte Transactions umfassen |
| Resolution | Ermittlung einer Wirkung aus Regeln, Inputs und passenden Zustandssichten | Veröffentlicht nicht allein dadurch State |
| Effect | Gewünschte fachliche Wirkung | `DealDamage(100)` ist nicht zwingend `Health -= 100` |
| Domain Change | Konkrete vorbereitbare Zustandsänderung der besitzenden Domain | Nicht bloß eine abstrakte Wirkung |
| Transaction | Atomare fachliche Publikation einer oder mehrerer Änderungen | Kein universeller Action-Lifecycle und keine verteilte ACID-Datenbank |
| Participant | Domain-Adapter, der eine Änderung für die Transaction vorbereitet | Coordinator muss seine Fachbegriffe nicht kennen |
| Prepare | Prüfung und Vorbereitung vor sichtbarer Änderung | Normale Ablehnung ist hier möglich |
| Prepared Change | Vorbereitete Änderung mit begrenzter Verwendbarkeit | Kein öffentliches frei aufrufbares Apply-Recht |
| Apply / Publication | Veröffentlichung bereits validierter Änderungen | Keine neuen normalen Gameplay-Entscheidungen oder beliebigen Observer-Callbacks |
| Domain Event | Beschreibender Fakt nach erfolgreichem Commit | Nicht „bitte führe dies aus“ |
| Reaction | Regel als Antwort auf ein committed Ereignis | Erzeugt neue Arbeit; schreibt Vergangenheit nicht um |
| Reaction Dispatcher | Explizit komponierte, beim Aufbau eingefrorene Reihenfolge von Reactions für einen Eventtyp | Kein globaler Reflection-EventBus und keine dynamische Priority-Registry |
| Prepared Follow-up | Intern reservierte Scheduler-Kapazität samt geordnetem Schlüssel vor der späteren Publikation | Noch nicht ausführbare Arbeit; wird publiziert oder verworfen |
| External Input Closure | Grenze, ab der für einen begonnenen Timestamp keine neuen externen Inputs mehr zugelassen werden | Bereits vorher zugelassene Inputs und kausale Follow-ups bleiben gültig |
| Pre-Commit-Regel | Beeinflusst die noch unveröffentlichte Resolution oder Vorbereitung | Nicht mit einer späteren Event-Reaction verwechseln |
| Work Item | Geordnete ausführbare Simulationseinheit | Persistierbare Arbeit benötigt beschreibbaren Zustand statt beliebiger Closure |
| Scheduler | Ordnet und begrenzt Work nach dem gewählten Ausführungsprofil | Bestimmt nicht eigenmächtig sämtliche Genre-Regeln |
| Wave | Vorhandenes Hilfsmittel zur Ordnung gleicher Timestamps und kausaler Generationen | Kein Ersatz für einen vollständigen Input-/Event-Vertrag |
| ExecutionId | Vorhandene Identität eines Gameplay-Ausführungskontexts | Noch kein vollständiger Causality-Graph oder automatisch globale ID |
| Provenienz | Ursprung und fachliche Ursache eines Ergebnisses/einer Operation | B07 ist ein Teil davon |
| Causality | Verknüpfung von Ursache, Ausführung, Änderung und Folgeereignis | Nicht gleichbedeutend mit Volltracing |
| Ledger | Buchung fachlicher Operationen, etwa Resource Loss/Cost/Recovery | Nicht automatisch Snapshot, universeller Event Store oder unbegrenzt aufzubewahren |
| Snapshot | Konsistente Aufnahme des für Fortsetzung erforderlichen Zustands | Darf nicht einen halben modulübergreifenden Apply konservieren |
| Replay | Reproduzierte Fortsetzung innerhalb eines angegebenen Vertrags | Kein automatisches engineübergreifendes Physics-Replay |
| Host Fact | Geordnetes autoritatives Ergebnis eines externen Systems, das Plaquewright nicht selbst reproduziert | z. B. aufgezeichneter ProjectileImpact aus Engine-Physics |
| Quiescent Boundary | Sicherer Snapshot-Punkt ohne aktiven Apply oder halb veröffentlichten Commit | Prepared-Objekte müssen nicht serialisiert werden |
| Exact Simulation | Semantisch gleiches Ergebnis und relevante History im definierten Profil | Muss nicht jeden Renderframe nachspielen |
| Approximate Simulation | Explizit erlaubter Verlust an Simulationsdetail | Kein stiller Notfallmodus bei Budgetende |
| Fast Path | Spezialisierter schnellerer Ausführungspfad | Muss relevante Referenzsemantik bewahren |
| Definition | Authoring-/Regeldaten | Nicht der veränderliche Zustand einer Instanz |
| Runtime Instance | Konkrete Existenz und Zustand im Simulationslauf | Nicht bloß dieselbe globale Definition |
| Revision | Version eines veränderlichen Zustands für Gültigkeitsprüfungen | Nicht die Software-/Definition-Version |
| Runtime Identity | Abgrenzung einer konkreten laufenden Simulation | In-process Referenzprüfung ist nicht automatisch Persistenzidentität |
| Handle / Generation | Kontrollierter Zugriff mit optionaler Wiederverwendungsabsicherung | Generationsmodell nur dort behaupten, wo es implementiert ist |
| Reentranz | Erneuter Eintritt in laufende Ausführung/Publikation | Für das erste Ausbauprofil nicht einfach nebenläufig zulassen |
| Fault | Technischer Fehler mit definiertem Stopp-/Recovery-Verhalten | D-05: unerwarteter Reaction-Fault rollt vorherigen Commit nicht zurück und stoppt die weitere autoritative Ausführung der Runtime |
| Budgetende | Explizite Arbeits-/Speichergrenze | Keine Meldung einer vollständig berechneten Simulation |
| gcc | Nutzerkürzel: grün, committed, clean | Aussage über gemeldeten Arbeitsstand, nicht automatisch neue Testanzahl |

Begriffe werden nur gemeinsam mit ihren Verträgen geändert. Der Name eines Types oder Tests ist kein eigenständiger Nachweis seiner Semantik.

**Grundlage:** [Manifest](Plaquewright_Manifest_v2_0.md) und [Quellenregister](SOURCE_EVIDENCE_v2_0.md).
