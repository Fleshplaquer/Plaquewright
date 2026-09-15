# Idler - Gameplay- und Combat-Manifest

**Version:** 1.0  
**Stand:** 7. September 2026, nach Bestätigung von R01, R02, R03, H01 und den technischen/QA-Anforderungen der zweiten Architekturprüfung.  
**Status:** Konsolidierter, vom Nutzer bestätigter Entwurfsstand. Zur schrittweisen Implementierung freigegeben; keine Behauptung einer bereits verifizierten Produktionsimplementierung.

> Der Core soll starke, ungewöhnliche Builds ermöglichen. Ihre Stärke muss aus nachvollziehbaren Regeln entstehen - nicht aus mehrfach verwendeten Budgets, zufälliger Reihenfolge, verlorenen Zuständen oder irreführenden Statistiken.

## M00 - Geltung, Quellen und Entscheidungsstatus

Dieses Manifest konsolidiert die im Gespräch bestätigten Regeln. Es ist keine unveränderte Abschrift der ersten Zusammenfassung: Die ausdrücklich angenommenen Audit-Korrekturen ersetzen die widersprüchlichen älteren Beispiele. Es führt keine ungefragten neuen Pflichtmechaniken ein.

**Quellenhierarchie:** Letzte ausdrückliche Bestätigung im Gespräch geht einer älteren Working Decision vor. Auditbefunde dokumentieren Probleme; erst ihre bestätigten Lösungen werden zur aktuellen Regel. Referenzspiele liefern Inspiration und Risikofälle, keine automatisch importierten Spielregeln.

| Status | Bedeutung in diesem Dokument |
|---|---|
| Bestätigter Vertrag | Aktueller gemeinsamer Entwurfsstand, einschließlich angenommener Working Decisions. Änderungen erfordern eine dokumentierte Entscheidung und passende Regressionstests. |
| Konfigurierbare Policy | Mechanismus und Auswahlstelle sind beschlossen; konkrete Klassen-/Contentwerte können variieren. |
| Offen / DEFERRED | Bewusst noch nicht gewählte Zahl, technische Umsetzung oder optionale Funktion. Darf nicht stillschweigend als fertig angenommen werden. |
| Implementierungsvorschlag | Konkrete Bau- und Testreihenfolge im separaten Implementierungsplan; kein zusätzlich beschlossenes Gameplay. |

**Freigabegrenze:** Im so konsolidierten Regelstand ist derzeit kein bekannter blockierender semantischer Widerspruch offengeblieben. Das ist eine Entwurfsfreigabe, kein mathematischer Beweis für jedes denkbare Content-Set. Erlaubte Kombinationen müssen im Compiler validiert und am implementierten Core getestet werden. Eine noch nicht unterstützte Policy liefert `Unsupported`, nie ein plausibel aussehendes Ersatzresultat.

Die Berichte bleiben unverändert als historische Quellen erhalten: [Audit 1](sources/Idler_Architektur_Audit_1.md), [Audit 2](sources/Idler_Architektur_Audit_2.md). Deren damalige Offen-Markierungen sind durch die späteren Bestätigungen abgelöst, nicht nachträglich aus den Berichten entfernt.

**Begleitdokumente:** [Implementierungsreihenfolge](Idler_Implementation_Sequence_v1_0.md), [Freigabe und Testnachweise](Idler_Freigabe_und_Testnachweise_v1_0.md).

## M01 - Projektziel und Architekturphilosophie

Idler ist ein solo entwickeltes Idle-/Incremental-Spiel mit tiefem Build-Crafting: Skills und individuelle Skilltrees, Items/Affixe, Traits, Relics, Conversions, Statusketten und offensive wie defensive Regeltransformationen. Godot mit C# bleibt die gewählte Umgebung. Gute Debuggability, sehr schwache Zielhardware und eine geringe mentale Einstiegshürde sind zentrale Anforderungen.

**Core = reines C#. Godot = Darstellung, Eingabe, Audio und Host-Integration.** Der Core besitzt die gameplayrelevante Simulations- und Raumlogik. Ein Godot-Node entscheidet nicht nebenher über Damage, Ressourcenverlust oder Triggerberechtigung.

Wiederverwendung ist ein Ziel, aber kein Auftrag, vor Idler eine universelle RPG-Engine zu bauen. Allgemeine Mechanismen bleiben von Idler-Regeln und Content getrennt. Extraktion erfolgt bei konkreter Wiederverwendung oder wenn sie Idler selbst vereinfacht. Keine spekulative Vielzahl von Projekten, Managern oder Interfaces.

**Neue Kombination = Daten. Neue primitive Mechanik = gezielt neuer Code.** Nicht jede denkbare Mechanik muss sich ohne Code ausdrücken lassen. Derselbe Trigger-, Condition-, Modifier- und Behaviour-Unterbau steht Skills, Items, Klassen, Gegnern und Status zur Verfügung.

Starke Trigger-Loops, Walking-Simulator-Builds und explizites Derived-Double-Dipping sind zulässige Designrichtungen. Sie sind nicht automatisch gut gebalanced. Unbegrenzte synchrone Arbeit oder verdeckt mehrfach angewendete Regeln sind dagegen keine legitimen Build-Eigenschaften.

Die Entwicklung erfolgt nach der dokumentierten Reihenfolge in kleinen vertikalen Schritten. Im Arbeitsdialog wird jeweils ein konkreter Implementierungsschritt umgesetzt und geprüft, nicht der gesamte Plan gleichzeitig.

## M02 - Begriffe, Klassifikationen und Erweiterbarkeit

### Grundbegriffe

| Begriff | Verantwortung |
|---|---|
| Definition | Editierbare, referenzierbare Content-Beschreibung. |
| Compiled Plan / Compiled Data | Validierte, vorbereitete und unveränderliche Regeln bzw. Daten. |
| Runtime State | Veränderlicher Zustand: Ressourcen, Timer, Beiträge, Charges, Counter. |
| SkillInstance | Laufender Besitz eines Skills mit dessen Cooldown-/Charge-Zustand. |
| SkillExecution | Eine Aktivierung mit Requirements, Kosten, Timing, Commit und Abbruchregeln. |
| DamageExecution | Zusammengehörige offensive Ausführung; kann mehrere Deliveries und Hits tragen. |
| DeliveryInstance | Räumlich/zeitliche Übertragung, beispielsweise Projektil, MeleeArc oder AreaPulse. |
| HitExecution | Ein zugelassener Trefferversuch gegen ein bestimmtes Target; Ergebnis kann vermieden, pariert oder akzeptiert sein. |
| DamagePath | Ein semantisch unterscheidbarer Schadensanteil, kein eigener Treffer. |
| EffectInstance / StatusInstance | Persistente Wirkung; ein Status ist am Actor angebracht, eine Zone räumlich wirksam. |
| Policy | Nach welcher Regel ein Mechanismus entscheidet. |
| Scope | Auf welchen fachlichen Bereich ein Ergebnis, Limit oder Zustand bezogen ist. |
| Group | Welche Komponenten eine Entscheidung teilen. |
| Modifier | Verändert Werte, Parameter oder ausdrücklich eine Policy/Struktur. |
| Condition | Deklarative Abfrage auf eine benannte Sicht des Kontexts/Zustands. |
| Behaviour | Implementiert eine primitive Mechanik und ihren Lifecycle. |

### Klassifikationsachsen

DamageType, DamageForm, Action-/Delivery-Kontext, Mechanic, Ownership und Herkunft sind getrennte Achsen. Beispiele sind Physical/Fire/Cold, Hit/DoT, Attack/Spell, Projectile/Area/Melee und Ailment/Ground/Cloud. Konkrete zusätzliche Schadensarten sind Contententscheidungen; die Beispiele definieren keine abgeschlossene Liste.

Normale DamagePaths haben genau einen aktuellen konkreten DamageType und eine aktuelle DamageForm. `Elemental` ist eine Gruppe, kein zusätzlicher Schadenspool. Fire/Cold/Lightning können `Elemental` implizieren. `+100 % Elemental Damage` gibt jedem passenden Anteil volle +100 %, nicht je ein Drittel.

Ein einzelner Modifier-Beitrag gilt pro passendem Pfad und seiner vorgesehenen Scaling-Stufe höchstens einmal, unabhängig davon, wie viele Tags seine Bedingung erfüllen. Verschiedene Modifier-Beiträge dürfen addieren. Source und Child sind verschiedene Scaling-Knoten; die ausdrückliche ScaledSource-Semantik ist daher kein Verstoß gegen diese Regel.

**Tags klassifizieren; Stats quantifizieren; Behaviours handeln.** `Projectile` ist eine Klassifikation, Chain ein Behaviour/Capability, ChainCount eine numerische Eigenschaft bzw. laufender Delivery-State. Die UI darf alle drei gemeinsam erklären, ohne sie technisch gleichzusetzen.

Attack und Spell sind im normalen Skillmodell alternative primäre Action-Typen. Explizite Hybride sind möglich; ein unabhängiger Status muss nicht künstlich einen dieser Typen erhalten. Ein Spell darf Weapon Damage verwenden, ohne dadurch Attack oder Melee zu werden.

### Tag-DAG und dynamische Klassifikation

Authoring-IDs sind erweiterbar; der Compiler löst sie auf IDs und vorbereitete Mengen/Bitsets auf. Im Hotpath wird kein String-Tag-Graph durchlaufen. Zyklen im Implikationsgraphen sind ungültig.

`DirectTags` sind die ausdrücklichen Beiträge. `EffectiveTags` sind ihre konsistente Implikationsmenge. Beim Entfernen eines Beitrags werden die EffectiveTags neu abgeleitet. Ein verbleibendes `two_handed_sword` darf nicht durch Löschen eines abgeleiteten Bits aufhören, ein Sword zu sein.

`enemy` ist keine intrinsische Identität. Species, Rank und weitere Actor-Klassifikationen sind von Allegiance/Relationship und Role getrennt. Ein gecharmter Untoter bleibt ein Untoter. Burning ist aktiver Status; LowLife, Nearby und RecentlyHit sind Conditions, keine Ersatz-Identitätstags.

### Direkter Schaden und Child-Wirkungen

Projectile, Area und Melee beschreiben den konkreten direkten Schadensweg. Ein Skill mit Area-Mechanik macht nicht automatisch jeden seiner Paths zu Area Damage. Eine Projectile-Explosion ist standardmäßig ein Area-Pfad, kein vererbter Projectile-Pfad; ausdrückliche Hybride bleiben erlaubt.

Ein direktes Spell-DoT kann Spell Damage verwenden. Ein neu angewendetes Ailment oder ausgelöster anderer Skill erbt Parent-Tags nicht automatisch. Der neue Effekt besitzt seine eigene Definition. Der Parent bleibt als Herkunft abfragbar. `Damage with Area Skills` und `Area Damage` sind deshalb verschiedene Filter.

Waffenart, tatsächliche Weapon Contribution, Waffenrequirement, Melee-Delivery und Entfernung sind ebenfalls getrennt. `Nearby` ist kein Synonym für Melee.

## M03 - Definitionen, Compiler und Runtime

```text
Authoring Definitions + Build + versionierte Regeln
                         |
                      Compile
                         |
          Immutable Compiled Plans / Snapshots
                         |
          Runtime-State + konkrete Executions
```

Definitionen dürfen komfortabel sein: stabile IDs, Graphen, lesbare Bedingungen, Quellenverweise. Das endgültige Authoring-Dateiformat bleibt offen. Godot-Resources, JSON oder andere Adapter dürfen die Kernsemantik nicht bestimmen.

Der Compiler löst IDs, Implikationen, konstante Conditions, Modifier-Pools, Roll-Policies, Reihenfolgen und Abhängigkeiten auf. Er validiert nicht nur DamageConversion, sondern auch Stat-Abhängigkeiten, kombinierte Type-/Form-/Derived-Pläne, Resource-Finanzierung, Rollenverwendungen und Transformationskonflikte.

Unbekannte IDs, nicht unterstützte Policies und ungültige kombinierte Pläne bekommen Diagnosen mit Quellangaben. Ein zyklisches Paar wie `MaxMana aus MaxWard` und `MaxWard aus MaxMana` braucht eine ausdrückliche Snapshot- oder mathematische Lösungsregel; es wird nicht durch wiederholtes Auswerten zufällig gelöst.

`CompiledSkill` enthält weder aktuellen Cooldown noch Charges oder Restdauer. Temporäre Strukturänderungen erzeugen einen neuen EffectiveState aus Grundkonfiguration und Quellenbeiträgen. Alte Executions behalten ihren vereinbarten Snapshot. Ein `LiveScaling`-Behaviour muss ausdrücklich einen anderen Vertrag angeben.

Die Lebensdauer folgt der Verwendung: Auch eine logisch kurzlebige DamageExecution kann von lange fliegenden Projektilen oder Effects referenziert werden. Pooling darf diese Lebensdauer nicht verkürzen. Stabile persistente IDs, kompakte Runtime-IDs und generationengesicherte Handles haben unterschiedliche Aufgaben.

Snapshots werden bei Build-/Buff-Änderungen versioniert, nicht für jeden Hit komplett neu aufgebaut. Inkrementelle Rebuilds sind eine spätere Optimierung und werden gegen einen vollständigen Compile verglichen.

## M04 - Modifier, Conditions und Structural Transforms

### Numerische Modifier

Normales Increased/Reduced wird innerhalb seines vorgesehenen Pools addiert; More/Less multipliziert. Quelle, Ziel/Scope, Rechenoperation, Condition-Sicht und zeitliche Bindung gehören zum Vertrag eines Modifiers. Ein Skilltree-Modifier ist nicht allein wegen seiner Quelle schwächer oder stärker als derselbe Modifier auf einem Relic.

Ein vorbereiteter Snapshot darf offene additive Pools nicht zerstören:

```text
100 Base
+100 % Fire Increased
+100 % Increased gegen Burning

Burning beim Hit:
100 * (1 + 1 + 1) = 300
nicht: vorab 200, danach nochmals *2 = 400
```

Der Compiler darf den konstanten Anteil vorberechnen, muss aber die fachliche Rechenstruktur erhalten, wenn später bedingte Beiträge hinzukommen. Target-bezogene offensiv wirkende Modifier bleiben Source-Scaling; sie sind nicht automatisch Target-DamageTaken-Modifier.

### Conditions und Zustandssichten

Conditions lesen benannte Sichten: `Before`/`BeforeHit`, `Projected`, `AfterCommit` oder einen ausdrücklich erlaubten Live-Kontext. OnHit gegen Burning liest standardmäßig den im Event gesicherten BeforeHit-Zustand. Der Hit kann sich nicht durch seinen eigenen neu angewendeten Burn qualifizieren. Der nächste Hit sieht den committed Burn.

Static Conditions werden nur unter den tatsächlich stabilen Annahmen wegoptimiert. Eine spätere Strukturänderung invalidiert davon abhängige Pläne. Target-/Delivery-bezogene Conditions werden beim fachlichen Wirkzeitpunkt ausgewertet; die vorhandene Modifier-Menge und ihre Source-Magnituden folgen dem Snapshotvertrag.

### Strukturbeiträge

Tags, Behaviours, Rollenbindungen, Roll-Contributors und Schutzreihenfolgen werden quellenbezogen verwaltet. Zwei Buffs können Area unabhängig gewähren. Das Ende eines Buffs entfernt nur dessen Beitrag.

`BindRole`, `UnbindRole`, `SelectResource`, `ReplaceResource` und explizite Suppression sind unterschiedliche Operationen. Ein globales Verbot von Projectiles muss auch die betroffenen Behaviours und Implikationen konsistent transformieren, nicht nur ein Bit löschen.

Komposition ist mechanikspezifisch: Tags vereinigen, Roll-Contributors blenden, Reihenfolgeconstraints topologisch ordnen, exklusive Ersetzungen validieren. Zusätzliche Abhängigkeiten durch Transformationen werden vorbereitet und validiert; kein unbeschränktes Add/Remove-Oszillieren.

Ein ungelöster Buildkonflikt lehnt die neue Konfiguration ab und erhält die letzte gültige. Für temporäre Transformationen muss eine Konfliktregel vorliegen; andernfalls scheitert die konfliktverursachende Transformation sichtbar. Keine verborgene Item-/Klassen-/Ladereihenfolge entscheidet.

## M05 - Ressourcen und mehrwertige Rollen

Actors besitzen ein dynamisches `ResourceSet`. Life, Mana, Rage, Heat, Integrity, Ward oder SoulCharges sind Definitionen, keine Pflichtfelder des Combat-Core. Kein Mana-Pool bedeutet tatsächlich fehlendes Mana, nicht ein versteckter Sonderfall mit MaxMana = 0.

Ressourcen definieren Bounds, optionales Maximum, Startwert, kontinuierliche oder diskrete Werte, Regeneration, Decay, Gain/Spend/Loss sowie Overflow-/Underflow-Verhalten. Uncapped und negative Ressourcen sind ausdrücklich möglich, aber keine automatisch angenommene Baseline.

Regeneration und Decay sind semantisch getrennt. Ein gemeinsames Vorzeichenfeld genügt nicht, wenn Modifier ihre Ursachen unterscheiden sollen. Charges, die ausschließlich einem Dodge-/Block-/Trigger-Behaviour gehören, sind zunächst dessen State, keine universelle Charakterressource.

### Rollen und Einheiten

`ResourceRoleId` ist erweiterbar. Eine Ressource hat 0..N Roles; eine Role findet 0..N Ressourcen. Vital, Primary, Utility1..N und weitere Rollen sind Daten. Die Auswahl dedupliziert konkrete Resource-Identitäten. Zwei Treffer über verschiedene Rollen sind keine zwei unabhängigen Budgets derselben Ressource.

Der Verbraucher legt Kardinalität und Auswahl fest: ExactlyOne, All, Split oder eine ausdrückliche geordnete Auswahl. Leere oder mehrdeutige Pflichtauswahlen sind Fehler, nicht zufällige Dictionary-Entscheidungen.

Eine Role legt keinen Wechselkurs fest. Kostenprofile oder explizite Umrechnungen bestimmen, ob ein Skill etwa 20 Mana, 8 Rage oder 1 SoulCharge kostet. Diese Zahlen sind ein Beispiel, kein globaler Kurs.

### Änderungen und Migration

Resource-Änderungen besitzen getrennte Dimensionen für Änderungsart, Ursache, Mechanismus und Quelle. Damage, SkillCost, Sacrifice, Drain, Decay, CapacityClamp, Transfer und Recovery bleiben abfragbar. Nicht alles gehört in ein einziges gegenseitig ausschließendes Cause-Enum.

Resource-Austausch ist atomar an einer Resolution-Grenze. Er legt Current/Max-Migration, Rollen und abhängige Pläne fest. Bestehende Beiträge bleiben standardmäßig an ihre konkrete Ressource gebunden. Wird sie entfernt, endet ihr Recovery-Beitrag mit `ResourceRemoved`; er springt nicht ungefragt zu einer Ersatzressource.

Offene betroffene Reservierungen werden aufgehoben und zugehörige Aktivierungen abgebrochen, sofern keine explizite Migration vorliegt. Bereits bezahlte Kosten folgen ihrer Refund-Policy. Max-Änderungen behalten zunächst Current absolut; ein kleineres Max begrenzt Current mit Ursache `CapacityClamp`, eine Erhöhung heilt nicht automatisch.

Diskrete Gains dürfen einen definierten Restakkumulator besitzen. Nutzbar werden nur ganze Einheiten. Volle Ressourcen folgen weiterhin ihrer OverflowPolicy; der Restakkumulator ist kein unbegrenzter versteckter Vorrat. Diskrete Kosten verlangen standardmäßig ganze Einheiten.

## M06 - Skill-Lifecycle, Kosten und Snapshots

Requirements prüfen beispielsweise Skill-/Actor-Zustand, Waffe, Target, Verfügbarkeit, Charges und Kosten. Mehrteilige Kosten werden gemeinsam prüfbar und reservierbar gemacht. Standardmäßig scheitert die Aktivierung ohne Teilzahlung, sobald eine notwendige Kostenkomponente nicht bezahlt werden kann.

`CostTimingPolicy` kann OnStart, OnCommit, PerChannelPulse oder OnCompletion verwenden. Der normale vorgeschlagene Startfall zahlt beim Start. Ein späterer Interrupt erstattet nicht automatisch; Refund ist eine eigene Operation. Cost oder Sacrifice ist kein Hit und durchläuft nicht Damage-Mitigationsphasen.

Start, Commit und Ende einer SkillExecution sind verschiedene Zeitpunkte. Vor Commit kann eine unterbrechbare Ausführung scheitern. Nach Commit können erzeugte Projectiles und Effects weiterleben, auch wenn der Cast oder Source-Actor endet. Cooldown-Start und Charge-Recharge sind explizite Policies; deren konkrete Skillkonfiguration muss dokumentiert sein.

Attack-/Cast-Speed verändert Ausführungszeit und ausdrücklich zugehörige Channel-Rhythmen, nicht automatisch laufende DoTs. Bereits gestartete Timing-Konfigurationen folgen dem Snapshotvertrag. Channeling ist eine SkillExecution mit kontinuierlichem Output oder fachlichen Pulsen, nicht automatisch eine neue Skillaktivierung pro technischer Aktualisierung.

Triggered Skills erzeugen eigene Executions. ManaCost, CastTime, Cooldown und Requirements werden nicht pauschal ignoriert. Der Trigger legt ausdrücklich erlaubte Ausnahmen fest.

Der Source-Snapshot bindet Grundkonfiguration, Waffen-/Flat-Beiträge, Transformationsregeln, Source-Modifier und Herkunft zur vereinbarten Start-/Erzeugungsphase. Zielzustände und Delivery-State werden am Hit bzw. fachlichen Wirkzeitpunkt gelesen. Ein später erworbener normaler Damage-Buff aktualisiert ein bereits fliegendes Projektil nicht automatisch. Explizites LiveScaling ist eine andere Policy.

## M07 - DamageExecution, Delivery und Hit-Berechtigung

Eine SkillExecution kann mehrere DamageExecutions erzeugen, beispielsweise Impact, eigenständige Explosion und Ground Effect. Eine DamageExecution kann mehrere direkte Paths und mehrere Deliveries tragen. Ein Volley mit acht Projektilen muss deshalb nicht acht getrennte DamageExecutions erzeugen.

Eine Delivery hält Position, Richtung, zurückgelegten Weg, Pierce-/Chain-/Bounce-Zähler und relevante Target-History. Diese Daten sind weder Actor-Buildzustand noch Ergebnis eines einzelnen Hits. Ein Modifier wie zusätzlicher Schaden pro Pierce liest ausdrücklich Delivery-State.

Ein Projektil, eine kurzlebige MeleeArc, eine Area-Abfrage und ein Beam verwenden dieselbe semantische Trennung. `Beam` entscheidet nicht, ob Schaden Hit oder DoT ist. Eine Zone kann ihren eigenen Lifecycle besitzen, ohne die ursprüngliche SkillExecution künstlich offen zu halten.

### Fachlicher Kontakt statt Callback-Zählung

`TargetHitPolicy` besitzt Scope, Limit und Zählpunkt. Standard für normale Projektile: einmal pro Target und Delivery; gezählt wird beim zugelassenen HitAttempt. Ein evadeter Versuch verbraucht dieses Limit ebenfalls. Verschiedene Projektile dürfen nach ihren jeweiligen Policies dasselbe Target treffen.

Eine Kontakt-Episode besteht aus Eintritt, anhaltendem Kontakt und Austritt. Mehrere technische Kollisionsmeldungen innerhalb desselben Kontakts erzeugen nicht mehrere HitAttempts. `CountAt = HitAccepted` ist eine mögliche Sonderpolicy, braucht aber weiterhin ausdrücklich erlaubte neue Versuche.

Scope kann Delivery, DamageExecution oder ausdrücklich SkillExecution sein; Limit kann 1, N oder Unlimited sein. AoE-Overlap, Return und Chain verwenden dieselbe Regelidee. Ein größerer Scope darf Impact und eigenständige Explosion nicht unbeabsichtigt gegenseitig ausschließen.

Eine abgelehnte Interaktion erzeugt keine HitExecution und keine Defense-/Damage-Rolls. Eine zugelassene, aber evadete HitExecution hat einen HitAttempt, jedoch kein `Hit`- oder `CriticalHit`-Ergebnis. Ob die Delivery weiterfliegt, zerfällt oder abprallt, ist separat konfiguriert.

Mehrere echte Hits werden nacheinander committed. Fünf Treffer zu je 100 sind nicht derselbe Vorgang wie ein Treffer mit 500: Armor-Budget, Rolls, OnHit, Statusversuche und Schutzverbrauch beziehen sich auf die wirklichen Hits.

## M08 - DamageRanges, Roll-Policies und Roll-Gruppen

### Ranges und Rechengrenzen

Vor der konkreten Auflösung werden logische DamageRanges geführt. Assembly, Transfers und normales Scaling erhalten Minimum und Maximum sowie die notwendigen Abhängigkeiten. Ein Range-Ergebnis alleine beschreibt keine beliebige komplexe Zufallsverteilung; die vereinbarte RollPolicy und RollGroup gehören dazu.

Minimum- und Maximum-Beiträge werden zunächst unabhängig als Kandidaten berechnet. Erst an der definierten logischen Range-Grenze gilt:

```text
wenn CandidateMin <= CandidateMax:
    Range = [CandidateMin, CandidateMax]
sonst, Default CollapseBetween:
    c = (CandidateMin + CandidateMax) / 2
    Range = [c, c]
```

Beispiel: Base 100..150, +100 Minimum und -100 Maximum ergibt Kandidaten 200..50 und damit 125..125. Nur +100 Minimum ergibt 175..175, nicht das ältere Clamp-Ergebnis 150..150.

Normalization ist eine nicht beliebig verschiebbare Berechnungsgrenze. Eigenständige Ranges A = 200..50 und B = 0..100 ergeben nach jeweiliger Normalisierung zusammen 125..225. Vorzeitiges Zusammenwerfen zu Kandidaten 200..150 und Collapse auf 175..175 ist keine zulässige Optimierung. Interne Fragmente derselben logischen Range müssen dagegen vor deren Normalisierung korrekt zusammengeführt werden.

### Roll-Begriffe und Default

| Begriff | Vertrag |
|---|---|
| Domain | Art der Zufallsentscheidung: Damage, Crit, Block, Status-Application, Trigger usw. |
| Policy | Random, Minimum, Average, Maximum, gewichtete Contributors oder Auswahlregel. Nicht jede Policy ist in jeder Domain sinnvoll. |
| Scope | Wann ein neuer Wert bestimmt wird. Damage standardmäßig pro HitExecution. |
| Group | Welche Paths dieselbe Entscheidung teilen. Direkte Paths eines Hits teilen standardmäßig eine Gruppe. |
| Provenance | Welche Policies und Quellen beigetragen haben. |
| Numerische Traits | Ob der tatsächliche Wert Minimum/Mittelpunkt/Maximum seiner aufgelösten Range ist. |

Die RollPosition `u` wird auf einen Pfad abgebildet als `Min + u * (Max - Min)`. Minimum liefert 0, Average den Mittelpunkt 0,5 und Maximum 1. Average bezeichnet nicht den Erwartungswert einer beliebig gewichteten oder Lucky-Verteilung.

Random ist nur der Fallback, solange keine ausdrücklichen Contributors die Auflösung bestimmen. Random mischt nur dann mit, wenn es ausdrücklich als Contributor gewährt wird. Gleiche Contributor-Typen zählen standardmäßig einmal, während ihre gewährenden Quellen getrennt verfolgt werden. Zusätzliche Gewichte sind explizit.

```text
Average + Maximum:
(0,5 + 1) / 2 = 0,75

Average Gewicht 1 + Maximum Gewicht 2:
(0,5 + 2 * 1) / 3 = 5/6
```

Normales Random verwendet eine ausdrücklich definierte diskrete Menge normalisierter Positionen mit erreichbaren Endpunkten. Die Auflösung ist eine versionierte Ruleset-Konfiguration; 0..1.000 inklusive aus den Beispielen ist kein bereits festgelegter Produktionswert. Diese Definition ist keine späte Integer-Rundung des Damage.

Lucky/Unlucky kompositionieren standardmäßig über den Netto-Auswahlbonus: zusätzliche günstige Versuche minus zusätzliche ungünstige Versuche. Positiv bedeutet BestOf(1+n), negativ WorstOf(1+|n|), null einen normalen Versuch. Jeder Versuch erstellt einen vollständigen Contributor-Kandidaten. Beliebig verschachtelte Best-/Worst-Pläne sind nur explizite Sondermechaniken.

### Korrelation und Extremwert-Trigger

Conversion-Splits und normales As Extra behalten die RollGroup. 100..200 Physical plus 30..60 Extra Fire ergibt bei `u = 0,8` 180 Physical und 54 Fire vor abweichendem finalen Scaling. Different RollGroups dürfen vor dem Roll nicht zu einer einzigen uniformen Range zusammengezogen werden.

Eine abweichende Path-Policy wie Maximum nur für Fire braucht eine ausdrückliche Gruppengrenze. Derived-Child-Effects erhalten standardmäßig eine eigene Gruppe. Ein konkreter `RolledPreCrit`-Input ist dagegen bereits ein Wert; dessen Übernahme ist kein neuer zufälliger Parent-Roll.

`OnMaximumRollHit` prüft standardmäßig alle ausgewählten direkten Paths des akzeptierten Hits. Die Auswahl muss nichtleer sein; ein einzelner konstanter Nebenpfad macht einen gemischten Hit nicht insgesamt maximal. Ein spezifischer Filter oder `Any` ist eine andere explizite Mechanik.

Bei ausschließlich kollabierten ausgewählten Ranges sind Minimum, Average und Maximum gleichzeitig erfüllt. Das bleibt eine gewollte Build-Achse. Diese Eigenschaften beziehen sich auf den Pre-Crit-Roll und werden durch Crit oder Defense nicht rückwirkend geändert. Policy-Beteiligung ist davon getrennt: Maximum kann beteiligt sein, obwohl der Wert nur bei 75 % seiner Range liegt.

## M09 - Crit: geteilter Sample, konkretes Ergebnis

**R01 ersetzt den alten Shared-CritResult-Vertrag:** Eine CritGroup teilt standardmäßig ihren Zufallswert innerhalb einer DamageExecution. Jede HitExecution vergleicht diesen Wert mit ihrer anwendbaren Crit Chance. Ein gemeinsames boolesches Ergebnis wird nicht vorausgesetzt.

```text
CritSample = 0,30
Normaler Gegner, Chance 20 %: kein Crit
Boss, Chance 50 %: Crit
```

Mehrere Shotgun-Treffer unter gleichen Bedingungen critten weiterhin gemeinsam. Ändert ein früherer Hit den Target-Kontext, kann ein späterer Vergleich anders ausfallen. Der Sample bleibt derselbe. Triggered Child Skills und neue DamageExecutions bestimmen standardmäßig ihre eigenen Samples.

Chance und Policy sind getrennt. Ein unclamped CritChance-Stat darf Overcap besitzen; die normale Erfolgswahrscheinlichkeit wird auf 0..100 % begrenzt. Der Überschuss hat ohne ausdrückliche Mechanik keine zusätzliche Wirkung. GuaranteedSuccess ist nicht +100 % Chance; Forbidden ist nicht bloß 0 %. Das ausdrückliche Crit-Verbot verhindert standardmäßig auch garantierten Erfolg.

Die Chance-Domain muss 0 % sicher scheitern und 100 % sicher gelingen lassen. Die inklusive DamageRange-Verteilung wird nicht ungeprüft als Bernoulli-Verteilung übernommen. Lucky/Unlucky sind Auswahlregeln für Chance-Samples; Scope, Group und Anzahl der Versuche bleiben nachvollziehbar.

### Crit-Wirkung

Der konkrete Damage-Roll findet vor der Crit-Multiplikation statt. Die Chance kann bereits als Sample vorbereitet sein; Sample-Ziehzeit und Anwendung des Multiplikators sind verschiedene Schritte.

```text
CritMultiplier = max(0, (BaseMultiplier + additive Prozentpunkte)
                         * Produkt(zulässiger More-/Less-Faktoren))
```

150 % Basis plus 50 Prozentpunkte ergibt 200 %; anschließend 20 % more CritMultiplier ergibt 240 %. Die Basiszahl 150 % ist ein Beispiel, kein Pflichtwert jeder Klasse.

Multiplier und Crit-Wirkung dürfen pro Path differieren. Ein Path kann die Crit-Wirkung ausdrücklich ignorieren und bei Faktor 1 bleiben. Der semantisch kritische akzeptierte Hit kann weiterhin `CriticalHit` liefern. Faktoren unter 1 sind als ausdrückliche Mechanik möglich; negative Damage- oder Heilwerte entstehen daraus nicht automatisch.

`CriticalHit` entsteht höchstens einmal je akzeptierter HitExecution, nicht je DamageType oder RollGroup. Evade und vollständiger Parry verhindern diesen Event, auch wenn der vorbereitete Vergleich kritisch gewesen wäre. Crit-Mitigationsregeln am Target verändern ausdrücklich die Crit-Wirkung; generisches Less Damage Taken from Crits ist eine spätere andere Schicht.

DoTs critten standardmäßig nicht. Eine Sondermechanik braucht einen fachlichen Crit-Scope, beispielsweise eine Effect-Application oder einen definierten Pulse, und darf nicht pro frei gewähltem numerischen Integrationssegment rollen.

## M10 - Damage Assembly und Waffenquellen

Assembly beantwortet, welche Basisbeiträge eine DamageExecution besitzt. SkillBase, WeaponContribution und passende Flat-/Added-Contributions werden mit Herkunft und Range zusammengesetzt. Noch kein Target-Mitigation, Crit oder verstecktes globales Scaling.

### Waffenauflösung

```text
Weapon Base + lokale Flat-Boni
          -> lokale Waffenmodifier
          -> ResolvedWeaponDamage
          -> expliziter Skill-Weapon-Koeffizient
          -> Damage Assembly
```

Beispiel: `(80 Physical + 20 lokal) * 1,25 = 125` Waffenbasis; 150 % Weapon Contribution liefert 187,5. Ein globaler Physical-Increase kommt erst in der normalen Damage-Skalierung. Der bereits lokal eingerechnete Modifier wird dort nicht nochmals als derselbe Beitrag eingesammelt.

Weapon Requirement, Wahl der Waffenquelle und Weapon-Damage-Koeffizient sind getrennt. Mainhand ist der normale Ausgangsfall; Offhand, beide, alternierend oder Unarmed benötigen eine explizite Definition. Fehlt eine zulässige Quelle, greift ein definiertes Unarmed-Profil oder das Requirement scheitert.

Jede DamageExecution eines mehrteiligen Skills legt ihre Contribution fest. Impact plus Explosion verdoppelt Weapon Damage nicht automatisch, verteilt es aber auch nicht stillschweigend. Attack/Spell/Melee folgt nicht aus der Verwendung einer Waffe.

Added-Damage-Effectiveness betrifft die dafür bestimmten zusätzlichen Flat-Contributions. Sie skaliert nicht ungefragt SkillBase, WeaponContribution oder As Extra. Added Damage wird direkter Bestandteil des betroffenen Paths mit dessen Kontext; ein ausgelöster anderer Skill ist kein Flat Added Damage.

Kompatible Beiträge können nach ihren logischen Rechengrenzen aggregiert werden. Unterschiedliche Herkunft bleibt erhalten, soweit spätere Gameplay-Filter darauf zugreifen können. Diagnostische Herkunft kann separat kompakt gespeichert werden.

## M11 - Conversion, As Extra, Derived und Output-Pläne

### Damage-Type-Conversion

Conversion verteilt vorhandenen unskalierten Schaden. Pro eligible Source-Path werden passende ausgehende Anteile gesammelt. Gleiche Ziele werden nur bei kompatibler Semantik zusammengeführt. Bei Gesamtsumme `s <= 1` bleibt der Rest am Source-Typ; bei `s > 1` werden Anteile proportional durch `s` geteilt. Negative Anteile sind ungültig. Quelle Item/Klasse/Skilltree liefert keine automatische Priorität.

```text
100 Fire; 80 % -> Cold; 60 % -> Lightning
-> 400/7 Cold + 300/7 Lightning
-> insgesamt weiterhin 100
```

Ketten sind erlaubt, solange der aktive Conversion-Plan azyklisch ist. Kein willkürliches Gameplay-Limit von zehn Conversions. Technische Größenlimits und Diagnosen bleiben notwendig, denn ein DAG kann sehr breit werden.

Normales Scaling verwendet den finalen CurrentType des jeweiligen Scaling-Knotens. OriginType, Lineage und ConversionCount bleiben getrennt erhalten und wirken nur durch ausdrückliche Regeln. `NotConverted` bedeutet ConversionCount = 0, nicht bloß CurrentType = OriginType.

### As Extra

As Extra erzeugt zusätzlichen Schaden, ohne die Source-Menge zu reduzieren. Es gehört nicht in den 100-%-Conversion-Pool. Standardbasis ist der Raw-Assembly-Snapshot vor Extra, Conversion und normalem Scaling.

| Basis | Zahlenquelle |
|---|---|
| RawSource | Assembly-Beiträge vor Extra, Type-/Form-Transformation und normalem Scaling. |
| ConvertedSource | Explizit benannter transformierter Source-Stand vor normalem Scaling. |
| ScaledSource | Benannter Source-Stand nach normalem Source-Scaling; Crit nicht automatisch enthalten. |

Die letzteren Basen können durch Skills, Talente, Klassen, Uniques oder andere ausdrückliche Mechaniken verwendet werden. Standard-As-Extra-Regeln lesen eingefrorene Source-Snapshots ohne automatische Rückfütterung durch neu erzeugtes Extra. Bewusste mehrstufige Ketten benötigen einen eigenen validierten Ableitungsplan.

Ein ExtraFire-Path aus Physical entsteht als Fire: OriginType = Fire, ExtraFrom = Physical. Das ist nicht die Conversion-Lineage Physical -> Fire. Spätere eigene Conversion des Extras erweitert seine eigene Lineage.

### Derived und benannte Eingaben

Derived beschreibt numerische Abhängigkeit; Triggered beschreibt die Ursache einer Ausführung. Beide können gemeinsam vorkommen. Ein Derived-Pfad ist nicht automatisch ein neuer Skill-Cast oder Trigger-Proc.

Normale Damage-Derivation verwendet als bestätigten Default eine `ScaledPreCrit`-Range und eigene Child-Identität/RollGroup. Bei targetabhängigem offensiven Scaling wird diese Range erst im passenden Kontext vervollständigt. `RolledPreCrit` bezeichnet dagegen den konkreten Parent-Roll. Eine Basis nach Crit oder nach einer bestimmten Defense-Schicht muss genau diese Schicht benennen.

`PostMitigationDamage`, `PostTakenScalingDamage`, `DamageTaken` und `ActualResourceLoss` sind nicht austauschbar. Ein bloßes Wort wie FinalDamage ist keine ausreichende SourceBasis. ResourceLoss-Eingaben tragen zusätzlich Resource-Einheit, Ursache und gegebenenfalls einen Damage-Wechselkurs.

Der Child erbt Parent Attack/Spell/Projectile nicht automatisch. Er kann dieselben Klassifikationen aus seiner eigenen Definition besitzen. Owner bleibt standardmäßig erhalten. Bei ScaledSource darf ein Modifier Source und Child jeweils einmal skalieren, wenn beide eigenständig matchen. Dies ist das ausdrücklich erlaubte starke mehrstufige Scaling, kein allgemeines Verbot doppelt beteiligter Modifier.

### R02: SourceBasis und OutputPlan sind getrennt

Eine lineare Liste globaler Phasen ist nur eine Lesehilfe. Der Compiler braucht einen endlichen, typisierten Abhängigkeitsplan für späte Outputs.

```text
SourceBasis                       OutputPlan
Woher kommt der Zahlenwert?       Wie wird der neue Anteil verarbeitet?
            \                    /
             neuer definierter DamagePath
```

Spätes ExtraFire aus ConvertedCold durchläuft seinen eigenen vorgesehenen Conversion-/Scaling-Plan. Ist Fire -> Lightning aktiv, werden 30 ExtraFire zu 30 Lightning vor dessen Scaling. Der Parent-Snapshot wird nicht rückwirkend geändert. Die Parent-Weapon-/Flat-Assembly wird nicht nochmals hinzugefügt.

Ein durch Hit-to-DoT erzeugter neuer DoT darf nicht stillschweigend eine allgemein geltende DoT-Type-Conversion verpassen. Sein OutputPlan wertet die für diese neue Form vorgesehenen Regeln aus. Der zusammengesetzte Type-/Form-/Ableitungsplan muss azyklisch und eindeutig sein; jede bereits aufgelöste Operation auf derselben fachlichen Stufe wird nicht unkontrolliert wiederholt.

Content beschreibt Source und Output semantisch, nicht als `goto stage 4`. Unterstützt eine Compiler-Version einen erlaubten OutputPlan noch nicht, weist sie ihn sichtbar zurück. Ein falsches Vereinfachen ist keine zulässige Fallback-Policy.

### Hit-to-DoT und As Extra DoT

Normaler offensiver Form-Transfer verteilt den pre-scaling Amount: 100 Hit mit 50 % Transfer ergibt 50 Hit plus 50 **gesamten** Base-DoT. Die Hit- und DoT-Pfade skalieren anschließend nach ihrer jeweiligen aktuellen Identität. Der direkte Action-Kontext kann dabei erhalten bleiben; ein neu erzeugtes Ailment ist davon getrennt.

100 Hit plus 50 % As Extra DoT ergibt dagegen 100 Hit plus 50 zusätzlichen gesamten DoT. Eine explicit ScaledSource- oder Critical-Ableitung kann stärker sein und braucht ihre eigenen benannten Regeln.

Der DamageType bleibt bei reinem Form-Transfer zunächst erhalten. Ein anderer Zieltyp ist eine zusätzliche definierte Transformation. Transfer ist kein defensiver Stagger. Allgemeines permanentes DoT-to-Hit bleibt deferred; ausdrückliches Konsumieren verbleibender Wirkung und Erzeugen eines Hits bleibt ein möglicher konkreter Use Case.

### Aggregation

Paths dürfen nur zusammengelegt werden, wenn alle verbleibenden erlaubten Gameplay-Operationen dieselben Ergebnisse erhalten. Gleicher CurrentType genügt nicht. Origin, Lineage, Form, RollGroup, Output-Basis, Klassifikation, Policies und Target-Filter können trennen.

Information wird nicht entfernt, nur weil der aktuelle Player-Build sie nicht abfragt: Ein späteres Target kann sie benötigen. Der Referenzmodus rechnet konservativ ohne aggressive Aggregation. Optimierungen müssen denselben Vertrag differentiell erfüllen.

## M12 - Eindeutige Schadensmengen und fachlicher Damage-Flow

### Die verbindlichen Mengen

| Name | Definition |
|---|---|
| `IncomingDamage` | Konkreter offensiver Schaden vor Zielverteidigung. |
| `PostMitigationDamage` | Nach den passenden HitTransforms, Armor und Resistance. |
| `PostTakenScalingDamage` | Zusätzlich nach dem normalen Target-Taken-Scaling, vor Stagger/Routing/Protection. |
| `DamageTaken` | Dem konkreten Empfänger nach seiner Protection zugestellter Schaden, vor ResourceProtection. |
| `CandidateResourceLoss` | Angefragter Verlust in Einheiten einer bestimmten Ressource. |
| `ActualResourceLoss` | Akzeptierter tatsächlicher Bruttoverlust dieser Ressource, nicht bloß Nettodifferenz. |
| `VitalLossEquivalent` | Qualifizierter tatsächlicher vitaler Verlust in Damage-Einheiten; Herkunft, Verhältnis und Auswahl bleiben nachvollziehbar. |

**Ablösung:** Die ältere Definition `DamageTaken = vor Protection` gilt nicht mehr. Deren Menge heißt jetzt `PostTakenScalingDamage`. Auch das Ende der Taken-Modifier-Phase wird nicht automatisch als `OnDamageTaken` ausgegeben.

Beispiel bei 1:1-Life-Verlust:

```text
Incoming                           1.000
Resistance 50 %                      500  PostMitigation
Taken Increased 20 %                 600  PostTakenScaling
Barrier absorbiert 400               200  DamageTaken
Fortification verhindert 50          150  ActualLifeLoss
```

Vollständige Barrier-Absorption lässt einen akzeptierten Hit und dessen normale OnHit-Applications bestehen, erzeugt aber kein positives DamageTaken. Fortification kann tatsächlichen ResourceLoss verhindern, obwohl positives DamageTaken vorliegt. Bei vitalem Ward können damageverursachte Absorptionskosten dennoch für eine explizit resourceverlustbasierte Leech-Auswahl relevant sein; verschiedene Messgrößen werden nicht künstlich gleichgesetzt.

Mengen behalten ihre benötigten DamagePath-Metadaten. Summen sind Hilfswerte, keine Erlaubnis, Physical, Fire, Routing-Ziele oder Resource-Einheiten bedeutungslos zusammenzuwerfen.

### Hit-Flow

Der offensive Plan stellt Ranges, passende Source-Modifier und SourceBases bereit. Delivery erzeugt einen zugelassenen HitAttempt. Im konkreten Hitkontext werden offene Conditions vervollständigt, der Damage-Roll und das zielabhängige CritResult bestimmt und Crit auf den konkreten Wert angewendet. Bereits bereitgestellte Samples dürfen beim Preview oder durch Telemetrie nicht nochmals gezogen werden.

```text
HitAttempt
-> Active Dodge -> Evasion -> Parry
-> Hit akzeptiert
-> Glancing / passende HitTransforms -> Block
-> gemeinsames Armor-Budget -> Resistance
-> DamageTaken-Modifier
-> PostTakenScalingDamage
-> Stagger-Aufteilung -> Routing
-> Protection des Empfängers
-> DamageTaken / CandidateResourceLoss
-> ResourceProtection / Shortfall / finanzierte Kosten
-> lokale direkte Applications, projizierte Defeat-Interventionen
-> gemeinsamer Commit und Post-Reaktionen
```

Dies ist eine fachliche Übersicht, kein Aufruf, SourceBasis oder Derived-Outputs in eine starre globale Schleife zu zwingen. Neue Outputs besitzen ihre eigenen Pläne. Pure Vorbereitung kann früh erfolgen; gameplayrelevante Zustandsänderungen folgen den festgelegten Phasen.

### Nicht-Hit-Wirkungen

DoT verwendet denselben Type-/Scaling-/Attribution-Unterbau, aber standardmäßig keine Evasion, Dodge, Parry, Glancing, Block, Armor oder Crit. Es durchläuft passende Resistance/Taken-, Routing-, Protection- und Resource-Schritte. Sondereligibility benötigt eine definierte Semantik auch für Limits und zeitliche Auslösung.

StaggerRelease ist eine bereits mitigierte Settlement-Menge und beginnt später bei seinem Release-/Routing-/Protection-Vertrag. Er ist weder neuer Hit noch normaler DoT und durchläuft keine bereits erledigte Mitigation erneut.

## M13 - Defensive Mechaniken und gemeinsame Budgets

### Evasion, Dodge, Parry, Glancing und Block

| Mechanik | Standardidentität und Scope |
|---|---|
| Evasion | Passiver Contest; ein Roll pro zugelassenem HitAttempt. Vermeidung erzeugt keinen akzeptierten Hit. |
| Active Dodge | Aktiviertes Fenster mit eigener Verfügbarkeit; eligible Hits werden standardmäßig garantiert vermieden. Vor Evasion. |
| Parry | Reaktive Interception nach Dodge/Evasion, vor Acceptance. Cancel, Partial, Deflect oder weitere explizite Resultate. |
| Glancing | Akzeptierter Hit wird proportional abgeschwächt; Roll standardmäßig pro Hit. |
| Block | Akzeptierter Hit wird durch Interception proportional abgeschwächt; eigener Roll pro Hit. |

DamageType allein bestimmt keine Avoidance. Ein Radiation-Projektil kann evadebar sein, ein Radiation-Field-DoT hat gar keinen Hit zum Evaden. Delivery-/Behaviour-Eligibility regelt jede Mechanik separat. Aus einer logischen Möglichkeit, einer Zone räumlich auszuweichen, folgt nicht automatisch ein passiver Evasion-Roll.

Evasion verwendet ein Rating und eine versionierte Chance-Policy. Accuracy wurde nach früherem Deferred-Status später als mögliche Angreifer-Gegenstat bestätigt: kein zusätzlicher normaler Accuracy-Erfolgswurf, sondern ein gemeinsamer Contest zur Bestimmung der EvadeChance. Die konkrete Rating-/Accuracy-Formel bleibt offen. `CannotBeEvaded` ist ein ausdrückliches Überspringen, nicht unendlich viel Accuracy. Andere Dodge-/Parry-Eligibility bleibt getrennt.

Kein willkürlicher allgemeiner Evasion-Cap unter 100 % ist Architekturpflicht. Die eigentliche Wahrscheinlichkeit bleibt 0..100 %. Garantierte Mechaniken und asymptotische Ratingkurven sind getrennte Möglichkeiten. Nicht-evadebare Quellen liefern Gegenprofile, keine Garantie guten Balancings.

Block trennt Eligibility, Availability, Roll und Effectiveness. Default ist independent/ready ohne allgemeinen Recovery-Cooldown. Recovery oder Charges sind optionale Behaviour-Policies; bei ihrer Verwendung ändert ein erfolgreicher Block die Verfügbarkeit für den nächsten Hit. Ein voller Block bleibt Hit plus Block, nicht Evade. Glancing und Crit können gleichzeitig gelten.

Glancing, Block und ähnliche proportionale Mechaniken teilen technische HitTransform-Infrastruktur, behalten aber eigene semantische Identität, Events, Scope und Availability. Modifier innerhalb derselben Mechanik werden zuerst aufgelöst; unterschiedliche erfolgreiche proportionale Mechaniken multiplizieren ihre verbleibenden Faktoren auf alle jeweils eligible Paths. Ereignisse und Budgets dürfen dabei nicht wegoptimiert werden.

### Armor

Armor besitzt Capacity und GuardReduction. **Capacity ist ein gemeinsames Budget pro fachlichem Hit**, nicht pro DamagePath und nicht pro interner Datenfragmentierung.

Bei identischer GuardReduction `g`, eligible Gesamtmenge `D` und Capacity `C` gilt:

```text
Guarded = min(C, D)
Prevented = Guarded * g
Remaining = D - Prevented
```

1000 Damage, Capacity 600 und Guard 50 % ergibt 700, auch wenn der Hit intern in zwei Paths zu je 500 zerlegt ist. Fünf echte Treffer zu je 100 gegen Capacity 100/Guard 50 % ergeben dagegen 250; ein einzelner 500er Treffer ergibt 450.

Für mehrere eligible Paths verwendet der Referenzfall proportional zu ihren eligible Amounts zugeteilte Guard-Capacity. Unterschiedliche Type-Wirksamkeiten werden danach auf deren geschützte Anteile angewendet. Typgewichtungen, Penetration und Sonderallocation müssen explizit im ArmorPlan stehen. Pathanzahl erzeugt nie zusätzliche Capacity.

Glancing/Block kommen vor Armor und dürfen dessen Wirksamkeit gegen große Hits verbessern. Armor kommt vor Resistance. Lokale Trefferzonen bleiben deferred; globale Armor ist die erste Policy, nicht die einzige theoretisch mögliche.

### Resistance und Taken-Scaling

Resistance verwendet den CurrentType und kann Hits und DoTs betreffen. Reduction verändert Target-State; Penetration die für diesen Path aufgelöste effektive Resistance. Negative Resistance ist möglich. Grenzen und Caps sind versionierte Ruleset-Parameter; die rohe lineare Formel darf nicht bei >100 % ungefragt negative Damage/Heilung erzeugen.

Normales Taken-Scaling ist eine spätere, separate Schicht:

```text
TakenFactor = max(0, 1 + Sum(IncreasedTaken) - Sum(ReducedTaken))
              * Produkt(zulässiger More-/Less-Taken-Faktoren)
```

Vulnerability kann einen solchen Modifier liefern, ist keine zweite Sonderformel. Die Outputs heißen PostTakenScalingDamage, nicht bereits DamageTaken. Armor-, Block-, Resistance- und Taken-Anteile werden im Trace getrennt erklärt; die ausgewiesene Reihenfolge bestimmt deren marginale Beiträge.

### Allgemeiner Bypass

Bypass überspringt einen ausdrücklich unterstützten Mechanismus für einen Anteil. Er ist weder Stat-Reduction noch Penetration. Der Split sitzt am betroffenen Mechanismus, nicht an einem einzigen globalen Bypass-Schritt.

Bei 1000 Damage, 25 % BlockBypass und erfolgreichem 40-%-Block gehen `250 + 750 * 0,6 = 700` weiter. BarrierBypass bedeutet nicht WardBypass oder SkipAllDefense. Eligibility und Bypass bleiben getrennt.

## M14 - Stagger-Eintritt, Routing, Shortfall und Protection

### Routing-Vertrag

Routing verteilt bereits mitigierte Payloads auf Actor/Resource/Role-Ziele. Es erzeugt ohne ausdrückliche Extra-/Duplikationsmechanik keinen zusätzlichen Schaden. Nach dem initialen Stagger-Split folgt aktuelles Routing für die sofortige Menge.

Passende Anteile werden pro Path gesammelt. Bis 100 % geht der Rest an das definierte Default-Ziel; darüber erfolgt proportionale Normalisierung. Identische Ziele werden nur bei kompatiblen Wechselkursen, Bedingungen, Fallback- und weiteren relevanten Policies zusammengefasst. Zielgleichheit allein darf unterschiedliche Verträge nicht vernichten.

Routing zu einem anderen Actor beginnt bei dessen Schutz-/Settlement-Vertrag, nicht erneut bei dessen Armor/Resistance. Bereits abgeschlossene Mitigation wird nicht erneut ausgeführt. **HitRedirection** vor der eigentlichen Zielverteidigung ist dagegen ein anderer Mechanismus und kann eine vollständige Resolution am neuen Hit-Target auslösen.

Der zusammengesetzte Routing-/Finanzierungsplan ist innerhalb einer Resolution endlich und validiert. Fallback ist eine ausdrückliche Kante mit definiertem Einstiegspunkt, kein beliebiges Zurückspringen in denselben Routing-Knoten.

### Einheiten und Shortfall

Damage-Einheiten und Resource-Einheiten bleiben getrennt. 100 Mana bei Kosten von 2 Mana pro Damage begleichen 50 Damage, nicht 100. Jede Verrechnung kennt ihr Verhältnis.

```text
CandidateResourceLoss
= ActualResourceLoss
+ explizit PreventedResourceLoss
+ zunächst noch unaufgelöster Shortfall
```

Ein Minimum-Clamp der Ressource verhindert den Rest nicht automatisch. Die Routing-/Schutzmechanik muss ihn durch SpillToFallback, ausdrückliche Verhinderung, erlaubte Schulden oder eine andere definierte Operation auflösen. Eine ungelöste Pflichtschuld ist kein erfolgreicher stiller Commit. Ein fachlich abgeschlossener Overkill wird separat ausgewiesen; er gilt weder als bezahlter ResourceLoss noch als Leech-Basis. Die konkrete erste Mana-Shield-Definition muss ihren Shortfall-Vertrag enthalten.

Eine fehlende Minion-Destination oder entfernte Ressource verwendet ebenfalls einen ausdrücklichen Fallback. Route-Discovery und Claims werden gegen den vorgesehenen Snapshot/Pending-State gestellt, nicht abhängig von zufälliger Pathiteration.

### ProtectionChain

Jeder Actor besitzt seine effektive geordnete Schutzkonfiguration. Ward -> Barrier und Barrier -> Ward sind beide zulässig. Klasse, Skilltree und temporäre Effects dürfen sie strukturell ändern. Die Reihenfolge wird bei Änderungen neu aufgelöst, nicht pro Hit neu sortiert. Widersprüchliche Before/After-Constraints brauchen eine explizite Konfliktregel.

Resource und ProtectionLayer sind verschieden. Ein Layer nutzt eine Ressource oder anderen begrenzten Behaviour-State. Seine Eligibility, geschützte Destination, Availability, ConsumptionRatio, Allocation, Limits und Bypass-Schnittstelle sind explizit. Ward als vitale Ressource darf nicht allein wegen seines Namens doppelt als Absorber und finales Verlustziel behandelt werden.

Default-Layers absorbieren partiell. Verhinderter Damage und verbrauchte Resource müssen nicht 1:1 sein. Für eligible Mengen `d_i`, Resource-Kosten pro Damage `c_i > 0` und Budget `B` gilt im proportionalen Referenzfall:

```text
FullCost = Sum(d_i * c_i)
lambda = min(1, B / FullCost)          // bei positivem FullCost
Absorbed_i = lambda * d_i
ResourceConsumed = Sum(Absorbed_i * c_i)
```

300 Physical bei Kosten 1 und 300 Fire bei Kosten 2 benötigen vollständig 900 Resource. Budget 300 absorbiert je 100 Damage und verbraucht genau 300 Resource. Shared Budgets gelten auch über verschiedene Routing-Zweige derselben Transaktion hinweg.

Per-Hit- oder Per-Pulse-Limits beziehen sich auf fachliche Einheiten. **Kein Limit pro frei gewähltem Continuous-DoT-Integrationssegment.** Kontinuierliche Throughput-Grenzen benötigen einen Zeit-/Budgetvertrag. Fünf echte Shotgun-Hits dürfen fünf PerHit-Limits verwenden, nicht fünf Kopien des aktuellen Resource-Bestands.

Barrier ist der vorgesehene stabile Absorptionspool, Ward der Pool mit ausdrücklichem Decay/Retention-Verhalten. Konkrete Formeln und Caps sind Content-/Ruleset-Entscheidungen. Ein vorhandener Decay beweist nicht, dass ein Ward-Build kontrollierbar skaliert.

## M15 - Resolution-Transaktion, ResourceProtection und Defeat

### Atomare fachliche Einheit

Eine zusammengehörige Resolution plant ihre Änderungen zunächst im Pending State. Sie kann mehrere betroffene Actors und Ressourcen umfassen. Verschiedene echte Shotgun-Hits bleiben getrennte Transaktionen.

```text
Before-State lesen
-> Damage/Claims/Applications im Pending State planen
-> gemeinsame Budgets reservieren und Shortfall auflösen
-> ResourceProtection und projizierte DefeatConditions prüfen
-> erlaubte Pre-Defeat-Interventionen einplanen
-> Mengen, Kosten, Abhängigkeiten und finalen State validieren
-> State UND akzeptierte kausale Operationen gemeinsam committen
-> Post-Events publizieren und Child-Arbeit einreihen
```

Kein fremder Hit unterbricht diese lokale Resolution zwischen Armor und Resistance oder zwischen Reservierung und Zahlung. Ein Budget wird durch Actor plus konkrete Resource-Identität eindeutig adressiert. Zwei Fortification-Claims zu je 300 gegen denselben Bestand 300 erhalten nicht je 300. Proportionale Allokation gleicher Claims ergibt je 150; eine andere Priorität ist explizite Gameplay-Konfiguration.

### Fortification und ResourceProtection

Fortification ist ein allgemeiner Reserve-Pool plus ResourceProtection-Behaviour, keine fest eingebaute Life-Heilung. Er kann Life, Mana, mehrere Ressourcen oder eine ausgewählte Role schützen. Threshold, Loss-Ursache, Verbrauchsverhältnis und gemeinsames Budget sind explizit.

CurrentState-, ProjectedState- und EventMagnitude-Conditions sind verschieden. Ein Mana-Floor von 20 % schützt nur den Anteil unter dieser Grenze, soweit Reserve und Policy dies erlauben. Zusätzliche allgemeine Protection-Cost-Finanzierung ist nicht automatisch eine kostenlose Verhinderung von Kosten.

R03 verlangt echte Finanzierung: 80 absorbierter Damage bei 1:1-Kosten, zur Hälfte aus Ward und zur Hälfte aus Fortification, verbraucht 40 Ward plus 40 tatsächlich reservierte Fortification. Schutzwirkung und Kostenersatz werden gemeinsam geplant. Keine zyklische Selbstfinanzierung und kein pauschaler nachträglicher Kostenwegfall nach bereits zugesagter Wirkung.

SkillCost, Sacrifice, Drain, Decay und damagebedingter Loss werden nicht ungefragt gleich geschützt. Eine Ersatzfinanzierung von Kosten oder erlaubter Schutz gegen Sacrifice muss ausdrücklich definiert sein.

### Bruttoledger statt bloßem NetDelta

```text
Life vorher                100
akzeptierter Damage-Loss    40
explizite Restoration       20
Life nachher                80

GrossLoss = 40; GrossRecovery = 20; NetDelta = -20
```

Recoup auf qualifizierten Loss verwendet 40, nicht 20. Echte Recovery bleibt ein positiver Vorgang. Verhinderung reduziert den geplanten Loss, statt eine fiktive Heilung zu erzeugen. R03 ersetzt damit jede Interpretation, die aus Atomic Commit nur einen Nettowert ableitet.

ChangeKind, Cause, Mechanism, SourcePayload, zahlender Actor und geschützter Actor bleiben getrennt. Ein damageverursachter Vital-Ward-Verbrauch durch Absorption bleibt als damagebezogener vitaler Verlust selektierbar. Die gleiche physische Zahlung wird jedoch nicht durch mehrere Labels oder Rollen doppelt gezählt.

### Defeat und Thresholds

VitalResource ist eine Role; DefeatCondition ist eine eigene Regel. Life <= 0, Ward <= 0, Heat >= X oder kombinierte Body-/Soul-Bedingungen sind möglich. Bounds und DefeatThreshold sind nicht dasselbe.

`WouldBeDefeated` prüft den projizierten Zustand vor sichtbarem Commit. Eine zugelassene Intervention wie RemainAtOne bei Life 100 und CandidateLoss 150 verhindert 51 und committed Loss 99. Es gibt weder sichtbares Life 0 noch eine erfundene anschließende Heilung.

Pre-Defeat-Interventionen besitzen ausdrückliche Verfügbarkeit und Wiederholung, standardmäßig einmal je konkrete Intervention und Resolution. Kosten werden ebenfalls reserviert. Nach Änderungen werden die betroffenen Bedingungen erneut geprüft, ohne rekursiv die gesamte Hit-Pipeline neu zu starten.

State-Transition-Events verwenden die akzeptierten Zustandsübergänge. `IsLowLife` ist keine ständig feuernde Transition. GrossLoss-/Recovery-Events beziehen sich auf akzeptierte Einzeloperationen. Benötigte Zwischenphänomene werden ausdrücklich als Operationsergebnis benannt und nicht aus beliebigen Pending-Zwischenständen abgeleitet.

Direkte OnHit-Applications werden nach der abgeschlossenen Berechnung ihres Source-Hits lokal eingeplant, bevor der finale DeathSnapshot entsteht. Sie verändern den Source-Hit nicht rückwirkend. Dadurch kann ein Poison des tödlichen Hits im DeathSnapshot vorhanden sein und sich durch eine OnDeath-Regel verbreiten. Ein später ausgelöster Child-Cast ist keine solche direkte Application.

## M16 - Status, Ailments, Zones und zeitliche Wirkung

### Status-Vertrag

StatusDefinition legt Application, Stack-Gruppe, Storage/Policy, Lifetime, Overflow, Behaviours und Entfernung fest. Nicht jeder Status ist DoT oder Ailment; nicht jedes Ailment muss Damage besitzen. Ein Mark kann Modifier liefern, ein Chill Utility verändern und eine Bomb erst beim Ende Damage erzeugen.

StatusApplicationAttempt, erfolgreicher StatusApplied, StackAdded, Refresh, Consume, Expire und allgemeines Removed sind getrennte Vorgänge. Immunity ist eine ausdrückliche Regel, nicht bloß eine hohe Avoidance-Zahl. Application-Chance und Target-Avoidance sind getrennt von Damage-Resistance.

Damage-Resistance kontrolliert nicht automatisch Application, Lifetime oder Utility-Stärke. Die vorgesehenen Ailment-Defense-Achsen sind Avoidance und explizite EffectReduction; Dauerverkürzung, Cleanse, Immunity, StackCaps, Consume und Transform bleiben separate Mechaniken.

### Stacking und Herkunft

Default-Stackgruppe: Target + StatusDefinition + Owner. Gemeinsame ownerübergreifende Stacks sind eine ausdrückliche Policy. Eine Gesamtanzahl auf dem Target kann weiterhin über Gruppen aggregiert werden.

Independent, CappedIndependent, Aggregate, Counter und Replace bestimmen die Gameplay-Semantik. Runtime-Buckets sind davon getrennt. ReplaceWeakest braucht eine StrengthMetric: standardmäßig offensiver Snapshot-DPS für damaging Status, definierte Magnitude für Utility. Restschaden und Restdauer sind andere auswählbare Metriken; Gleichstände verwenden eine stabile Application-Reihenfolge.

Refresh verändert standardmäßig nur Lifetime, nicht Snapshot/Owner oder Pulse-Phase. Bei TotalAmountBased erzeugt Refresh keinen neuen Schadensvorrat. Eine Umverteilung des Restbetrags oder zusätzliche Menge ist explizit. Bei einem erreichten Stacklimit gilt im einfachen Default RejectNew; ReplaceOldest, ReplaceWeakest, Refresh oder ConsumeAndTrigger sind ausdrückliche Varianten.

Transfer erhält standardmäßig Snapshot und Restlaufzeit. Copy/Spread mit Duplikation ist eine andere Wirkung. Cleanse, Consume, Transfer, Transformation, Expire und Defeat besitzen unterschiedliche Removal-Causes. Transfer oder Cleanse darf nicht versehentlich OnExpire auslösen.

### Zeitmodell

EffectTime ist die interne Zeit einer persistenten Wirkung. EffectRate bestimmt ihren Fortschritt relativ zur Simulationszeit. Technischer Integrationsschritt, UI-Aktualisierung, fachlicher Pulse und EffectRate sind unterschiedliche Größen.

| Mengenmodell | Wirkung der source-seitig gewählten Basisdauer |
|---|---|
| RateBased | Rate ist primär. Längere Basisdauer liefert normalerweise mehr nominelle Gesamtwirkung. |
| TotalAmountBased | Gesamtbetrag ist primär. Basisdauer verteilt diesen Betrag. Normaler Hit-to-DoT-Transfer verwendet dieses Modell. |

Beide Mengenmodelle können Fixed-, Derived- oder Hybrid-Basis verwenden. Das sind unabhängige Achsen.

100 Gesamtwirkung über zehn Sekunden ergibt Rate 10/s. Eine **defensive Lifetime-Verkürzung** auf fünf Sekunden entfernt nicht ausgezahlte Wirkung: 50 insgesamt. EffectRate x2 arbeitet dagegen denselben Vorrat in fünf realen Sekunden ab: 100 insgesamt. `RedistributeRemainingAmount` ist wiederum eine ausdrückliche Umverteilung.

Source-Magnitude und Basisdauer werden normalerweise beim Erzeugen gesnapshottet. Aktuelle Target-Defense, Routing/Protection und ausdrückliche dynamische Effect-Modifikatoren werden bei der Wirkung berücksichtigt. Spätere normale Owner-Damage-Buffs skalieren einen alten Poison nicht rückwirkend.

### Pulse- und Zone-Regeln

Normaler erster Pulse: nach einem Intervall, nicht automatisch beim Apply. Weitere Pulse folgen ihrer benannten EffectTime- oder SimulationTime-Uhr. Ein geplanter Pulse exakt am natürlichen Endzeitpunkt findet vor Expired statt. Angebrochene Restintervalle erzeugen keinen Teil-Pulse; Continuous Damage integriert deren Restzeit.

Ein Pulse kann Hit, DoT, ApplyStatus, Restore oder andere definierte Wirkungen erzeugen. Die Tatsache, dass etwas periodisch passiert, macht es nicht automatisch zum Hit. Neue fachliche DamageExecutions eines Pulses verwenden ihre definierten Crit-/Roll-Regeln und den Snapshotvertrag des Effects.

Ground, Cloud, Aura und Field sind erweiterbare Zone-Klassifikationen. Attachment, Bewegung, Shape, TargetFilter und Follow sind Behaviours/Policies, nicht allein aus dem Namen abgeleitete Pflichtregeln. Zone bedeutet nicht automatisch Damage. Overlap kann Independent, Strongest, Merge oder begrenzt sein; die konkrete Definition legt es fest.

Ziele werden zum fachlichen Wirkzeitpunkt aus der gemeinsamen Raumlogik bestimmt. Kontinuierliche Zone-Wirkung braucht auch Eintritts-/Austrittsgrenzen. Ownership und Source-Snapshot bleiben stabil, soweit keine explizite Transformation sie ändert.

## M17 - Stagger als zeitversetztes Settlement

Stagger nimmt einen Anteil von PostTakenScalingDamage auf. Er erzeugt zunächst keine zusätzliche Schadensreduktion. Typ, Herkunft und bereits erledigte Verarbeitung bleiben erhalten. Ein 40-%-Stagger auf 1000 ergibt 600 sofort und 400 später fällig.

Beiträge behalten ihre eigenen Release-Horizonte; neue Beiträge refreshen alte nicht standardmäßig. Schnellere oder langsamere Auszahlung ändert den zeitlichen Verlauf, nicht automatisch den verbleibenden Betrag. Explizite StaggerReduction, Consume, Cancel oder Transformation werden separat bilanziert.

Bei Release gelten standardmäßig aktuelles Routing, aktuelle eligible Protection und ResourceProtection. Armor, Resistance, normales Taken-Scaling, Crit und Hit-Avoidance laufen nicht erneut. Keine neue Hit-/OnCrit-Semantik und kein normaler DoT. Ein SnapshotRouting ist eine ausdrückliche Sonderpolicy.

Gespeicherter, freigesetzter, verworfener und noch verbleibender Stagger werden getrennt erfasst. Ursprünglich gespeicherte Menge ist nicht zusätzlich zum späteren DamageTaken zu zählen. DamageTaken entsteht für den späten Anteil erst nach seiner dann aktuellen Protection, mit Release-Herkunft.

OnStaggerRelease-Procs benötigen einen definierten fachlichen Pulse, Beitrag oder Mengen-Threshold. Ein numerisches Integrationssegment ist kein Proc-Anlass. PerHit-Protection wird nicht durch frei gewählte Release-Teilungen beliebig neu verfügbar.

Beim Defeat endet der verbleibende Pool standardmäßig mit dem Actor. Eine OnDefeat-Explosion aus Rest-Stagger ist ein ausdrücklicher Output vor Cleanup. Er darf keinen Actor-/Source-Verweis nach dessen ungültiger Wiederverwendung lesen.

## M18 - Recovery und zeitliche Beiträge

Regeneration ist autonome kontinuierliche Wiederherstellung, Restore ein unmittelbarer Gain durch einen Vorgang, Leech eine Ableitung aus offensiver Wirkung und Recoup eine Wiederherstellung auf Basis qualifizierten eigenen Verlusts. Life/Mana/Ward sind auswählbare Resource-Ziele, keine getrennten Recovery-Engines.

LifeOnHit und OnKill-Restore sind kein Leech: Ihr Wert muss nicht vom verursachten Damage abhängen. Ihre Events behalten die Hit-/Kill-Semantik und explizite Quoten/Cooldowns.

### Basis und Einheiten

Default-Leech verwendet den tatsächlich verbrauchten damagequalifizierten Anteil ausgewählter Vital-Ressourcen des Targets, ausgedrückt in Damage-Einheiten. Es enthält keinen Overkill. 1000 möglicher Schaden gegen 120 vorhandenes Life bei 1:1 liefert 120 Basis, bei 10 % also 12 Recovery.

Damageverursachter Verbrauch vitalen Wards durch Protection bleibt auswählbar. Selection dedupliziert konkrete Buchungen und Ressourcen. Wechselkurse rechnen Resource-Einheiten auf die passende Damage-Menge zurück; mehrere Finanzierungsschichten werden nicht mehrfach als dieselbe vitale Leech-Basis. Eine alternative Leech-Basis muss ausdrücklich benannt sein.

Recoup verwendet tatsächlichen Bruttoverlust, standardmäßig damagebedingt, nicht NetDelta und nicht SkillCost/Sacrifice. Durch Fortification verhinderter Verlust gehört nicht hinein.

### Zeit und Limits

Leech-/Recoup-Beiträge behalten Start, Zeitvertrag, Betrag, Herkunft und eigenes Ende. Bei konstanten Raten kann eine aggregierte Rate plus geordnete Rate-Changes ausreichen. Unterschiedliche Laufzeiten werden nicht durch einen gemeinsamen Refresh ersetzt. Pause/LiveRate/Caps können mehr State erfordern; das einfache Schedule ist keine universelle Abkürzung.

Default bei voller Ressource: ContinueAndWaste. Default bei Recovery-Cap: Zeit läuft weiter, nicht ausgezahlte Menge wird verworfen, nicht heimlich verlängert. 100 über fünf Sekunden mit Cap 10/s ergibt 50 Auszahlung und 50 CapWaste.

RecoverySpeed ändert ausdrücklich die zeitliche Abarbeitung eines Vorrats. RecoveryAmount/OutputReduction ändert angebotene oder wirksame Mengen. Diese Semantiken dürfen nicht unter einem unklaren Rate-Stat vermischt werden.

Offered, CapWaste, Prevented, AttemptedGain, AppliedGain und Overflow werden getrennt bilanziert. Bereits durch ein Cap verworfene Recovery wird nicht automatisch ResourceOverflow. Erst der Gain-Versuch am Resource-Maximum erzeugt Overflow. Umleitung oder Speicherung davon braucht eine explizite Policy.

Kontinuierliche Recovery erzeugt keine Gameplay-Procs je technischem Tick. Fachliche Events wie LeechCreated oder Mengenregeln wie je 100 tatsächlich regenerierte Einheiten sind zulässig. Beiträge dürfen nur bei gleicher relevanter Semantik aggregiert werden.

## M19 - Ownership, Attribution, Charm und Reflect

| Information | Bedeutung |
|---|---|
| CurrentEffect | Aktuell wirkende Skill-/Effect-Definition bzw. Instanz. |
| CausalParent | Unmittelbar auslösender fachlicher Vorgang. |
| Provider | Item, Talent, Klasse oder andere Contentquelle der Mechanik. |
| NumericSource | Snapshot/Beitrag, aus dem ein Zahlenwert abgeleitet wurde. |
| Owner | Mechanischer Eigentümer der Wirkung. |
| StatsSnapshot | Tatsächlich verwendete offensive Werte und Regeln. |
| RootSource | Ursprung der übergeordneten Wirkungskette. |
| Controller / Allegiance | Aktuelle Kontrolle und Beziehung, nicht identisch mit Owner oder Stats. |

Ein Minion kann Damage-Owner sein und einen Player als RootOwner besitzen. Player-/Minion-Skalierung wird explizit gebunden; sie folgt nicht automatisch daraus, dass der Controller ein Player ist. KillCreditRecipient und LeechRecipient sind separate Zielrollen, standardmäßig vom mechanischen Owner abgeleitet, explizit umlenkbar.

Triggered/Derived-Wirkungen behalten normalerweise den Owner des vorgesehenen ausführenden Kontexts. Wenn ein Trigger einen anderen Actor handeln lässt, wird dessen Execution-Kontext ausdrücklich gebunden; das Trigger-Event macht ihn nicht automatisch zum Owner seiner Source. Provider ist nicht automatisch Damage-Owner.

Charm verändert Controller, Allegiance und Role, aber keine bestehende Poison-Instanz rückwirkend. Sie behält Owner, Snapshot, Restdauer und Stacks. Charm-Skilltrees dürfen ausdrücklich Cleanse, Pause, Ownership-Transfer, Transform oder neue Outputs definieren.

Neue Projectile-Kontakte verwenden standardmäßig gespeicherte Source-Allegiance gegen die aktuelle Target-Allegiance. Eine bereits legitim angewendete Wirkung wird nicht durch eine erneute pauschale FriendlyFire-Prüfung automatisch abgeschaltet. Ausdrückliche Targeting-/Allegiance-Policies können anders funktionieren.

Deflect ändert standardmäßig Bahn/Ziel, nicht Owner und Scaling. Reflect kann Owner und Source-Allegiance wechseln, behält standardmäßig den vorhandenen offensiven Snapshot und skaliert nicht nochmals mit allen Stats des Reflektierenden. Nur die betroffene Delivery erhält den neuen Kontext, nicht ihre Geschwisterprojektile.

Verschwundene Source-Actors bleiben diagnostisch über gesicherte IDs/Snapshots erklärbar. Recovery an einen nicht mehr existierenden Empfänger scheitert ausdrücklich; sie springt nicht still zum RootOwner. Lange Trigger-Loops benötigen keine unbegrenzt gespeicherten Vorfahrenobjekte. Gameplayrelevante Historie und begrenzter Detailtrace sind getrennt.

Spectre-/Capture-artige Begleiter erzeugen neue Instanzen aus Monsterdefinition, Skills/Behaviours und explizitem Minion-Scaling-Profil. Sie kopieren nicht pauschal Encounter-Endgame-Zahlen. Charm derselben existierenden Einheit ist ein anderer Vorgang. Der Capture-/Spectre-Inhaltsumfang bleibt deferred, der Architekturtestfall erhalten.

## M20 - Trigger, Events und Scheduler

### Trigger-Vertrag

Ein Trigger kombiniert fachliches Event, Filter/Conditions, Availability, Attempt-Regeln, RNG-Policy/Scope, Erfolgs-/Wiederholungsquota und Output. RollScope und RepeatPolicy sind getrennt. Ein Versuch je DamageExecution mit 30 % bleibt 30 %; fünf unabhängige Versuche mit maximal einem Erfolg ergeben dagegen `1 - 0,7^5`.

Bekannte bereits verbrauchte Quoten verhindern neue Versuche vor unnötigem RNG-Verbrauch. Gemeinsame RollScopes cachen die vorgesehene Entscheidung. Charges/ICD/Recharge gehören zum Behaviour-State. Ob ein erfolgreicher Trigger eine Skillaktivierung zugesagt oder diese tatsächlich committed hat, bleibt getrennt.

Ein Trigger kann neue Skill-/DamageExecutions, Applications, Ressourcenoperationen oder andere Effects erzeugen. Derived bestimmt gegebenenfalls deren Zahleneingabe; Trigger allein bedeutet keine numerische Vererbung. Neue Executions haben ihre eigene Klassifikation, Rollkonfiguration und ihren vorgesehenen Snapshot.

### Event-Semantik

Das Vokabular ist offen erweiterbar. Bestehende Bedeutungen werden nicht zur Unterstützung einer neuen Mechanik heimlich umdefiniert. Ein interner Scheduler-Eintrag ist nicht automatisch ein triggerbarer Gameplay-Event.

Wichtige Familien sind SkillActivation/Commit/Interrupt; HitAttempt/Dodge/Evade/Parry/Hit/CriticalHit/Glancing/Block; DamageResolution/Absorption/Routing; StatusApplication/Refresh/Consume/Expire/Remove; ResourceChange/Gain/Spend/Loss/Overflow/Depletion; ThresholdTransition/PreDefeat/Defeated/Kill.

Ein Hit mit mehreren DamagePaths erzeugt standardmäßig ein Hit-/CriticalHit-Event. Shotgun erzeugt mehrere echte Hits. DamageTaken wird nach Protection gemessen; positive fachliche DamageResolution kann ein entsprechendes Event liefern. Kontinuierliche Integration erzeugt nicht allein aufgrund ihrer Schrittzahl Procs.

Post-Events reagieren auf abgeschlossene Ergebnisse. Sie ändern den verursachenden Hit nicht rückwirkend. Direkte Applications und finanzierte Pre-Commit-Eingriffe sind ausdrückliche lokale Operationen, keine zufällig früh ausgeführten EventBus-Listener.

H01 bindet Event-Conditions an gesicherte Zustandssichten. Spätes Dispatch von Hit 1 darf dessen BeforeHit nicht mit dem durch Hit 2 geänderten Live-Actor verwechseln. AfterCommit ist eine explizite andere Auswahl.

### Zeitordnung und begrenzte Ausführung

Scheduler ordnet fachliche Einheiten nach Simulationszeit und einer versionierten vollständigen Gleichstandsordnung. Innerhalb einer Einheit gibt es keine willkürliche Interleaving-Mutation. Lokale direkte Applications werden vor der nächsten Hit-Resolution committed; globale Phasen dürfen nicht alle Hits vor alle Applications schieben.

Zero-Delay-Folgearbeit wird kausal nach ihrer Source eingereiht; die vorgesehene nachgelagerte kausale Welle lässt ein Child nicht vor seinem Parent erscheinen. Konkrete Gleichstände zwischen externem Input, Expiration und anderen Eventarten werden im versionierten Scheduler-Profil explizit festgelegt und getestet. Keine Collection-Reihenfolge dient als versteckte Regel.

Trigger-Zyklen sind erlaubt. Synchrone Berechnungs-/Transformations-/Finanzierungspläne brauchen dagegen endliche validierte Abhängigkeiten. Auch positive Delays und DAGs garantieren keine tragbare Arbeitsmenge. Queue, lebende Instanzen, erzeugte Paths, Trace und deterministische Arbeitsmenge bekommen technische Budgets.

Budgetüberschreitung endet sichtbar als unvollständiger Run, beispielsweise `BudgetExceeded`, nicht als heimlich verkürzte Procfolge oder vollständiger DPS-Wert. Der Host darf zwischen committed Einheiten Arbeit aufteilen, ohne die Simulationszeit oder Spielregeln zu ändern.

RNG-Domains, Scopes und stabile Schlüssel verhindern, dass ein kosmetischer Zusatzroll die gesamte Combat-RNG-Sequenz verschiebt. Konkretes RNG-Verfahren und Schlüsselversion werden dokumentiert. Ein geänderter kausaler Spielverlauf kann natürlich andere Events erzeugen; identische Seeds garantieren keine identischen Zweige verschiedener Builds.

## M21 - Kontinuierliche Zeit, Raum, Versionen und Persistence

### Integrationsvertrag

Jeder unterstützte kontinuierliche Mechanismus erklärt seine Rate-/Zustandsfortschreibung und relevante Grenzen. Der nächste Simulationsschritt endet am frühesten geplanten oder neu entstehenden Ereignis: Depletion, Threshold, Ratewechsel, Effect-Ende, Kontakt oder andere Zustandsgrenze.

```text
Life 100, konstanter DoT 100/s,
nächstes bereits geplantes Event bei t=10:
Defeat trotzdem bei t=1.
```

Einfache stückweise konstante Raten können exakt integriert werden. Nichtlineare oder räumlich gekoppelte Mechaniken benötigen eine definierte numerische Strategie und einen Fehler-/Zeitrastervertrag. Nicht unterstütztes Fast-forward wird abgelehnt oder als Näherung gekennzeichnet.

EffectRate 0, Duration 0, instantane Outputs, negative Raten und nichtendliche Zahlen benötigen explizite Validierung. Negative EffectRate bedeutet nicht automatisch rückwärts laufende Geschichte. Auto-Dodge-Prognosen sind rein lesende Previews, ohne zweite RNG-Ziehung, reservierte Charges oder publizierte Events. Ist keine exakte Vorschau definiert, verwendet der Content eine benannte einfachere Inputgröße statt angeblich finalem LifeLoss.

### Gemeinsame räumliche Wahrheit

Idler-Spiel und Headless-Simulator benutzen dieselben Core-Regeln für Positionen, TargetQueries, Kontaktzeitpunkte und deren Reihenfolge. Godot visualisiert und liefert Eingaben. Ein Render-Callback darf keine zusätzliche Trefferwahrheit schaffen.

Zunächst werden nur konkret benötigte Shapes, Reichweiten und Bewegungen umgesetzt. Analytisch einfache Kontakte und konservative fortgeschriebene Bewegung müssen denselben vereinbarten Treffervertrag erfüllen. Spatial-Strategie und Konfiguration gehören zum Replay-Vertrag. Ein vereinfachtes anderes Simulationsmodell wird als Näherung benannt.

### Versionen und Lebensdauer

Neue Build-/Content-Konfigurationen erzeugen neue immutable Pläne. Alte Executions referenzieren ihre gültige Version, neue verwenden den neuen Stand. LiveScaling ist ausdrücklich anders.

Timer, Pulse und Expiration-Einträge erhalten eine Invalidierungs-/Generationserkennung. Nach EffectRate-Änderung darf ein alter Ablauf-Event den Effekt nicht zur früheren Zeit entfernen. Wiederverwendete Actor-/Effect-Slots werden ebenfalls generationsgesichert adressiert.

### Numerik und Reproduzierbarkeit

Der Vertrag ist zunächst Reproduzierbarkeit innerhalb einer dokumentierten unterstützten Runtime-/Ruleset-Konfiguration. Plattform- und Versionswechsel sind nicht automatisch bitgleich. IDs, Ereignisordnung und deterministische State-Hashes werden exakt geprüft; numerische Vergleichsgenauigkeit folgt dem gewählten Zahlensystem.

Keine zusätzliche Gameplay-Zwischenrundung. Anzeigeformatierung ändert keine Ressourcen. Continuous Resources erlauben Bruchteile. Endliche Präzision ist kein Versprechen exakter reeller Mathematik. Der Produktionszahlentyp, Wertebereich für Incremental-Zahlen, Zeitauflösung und tolerierte numerische Fehler werden vor den betroffenen Produktions-APIs entschieden.

### Save und Replay

Replays enthalten Initialzustand, Build/Ruleset/Content-Version, RNG-Verfahren/-Version/Zustand, Inputs sowie numerischen, zeitlichen und räumlichen Modus. Ein Seed allein reicht nicht.

Savegames referenzieren stabile Definition-IDs. Runtime-Handles dürfen kontrolliert neu aufgebaut werden. Mid-Combat-Saves erfolgen an Commit-Grenzen und speichern erforderliche Queue-, Effect-, Contribution-, Snapshot- und RNG-Zustände. Eine Save-Migration garantiert kein historisch identisches Replay unter neuen Regeln.

Persistenz braucht ihren eigenen Integritätsvertrag: neue Generation vollständig schreiben, validieren und erst dann als gültig auswählen; letzte gültige Generation bei Fehlern erhalten. Konkrete Dateisystem-Atomicität/Durability wird implementiert und mit Fault Injection geprüft, nicht allein durch eine Gameplay-Transaktion behauptet.

## M22 - Telemetrie, Combat Insights und Explainability

### Drei getrennte Ebenen

| Ebene | Muss leisten |
|---|---|
| Gameplay-Ergebnis | Akzeptierte kausale Buchungen und Daten für Regeln; unabhängig davon, ob Statistik angezeigt wird. |
| Combat Insights | Begrenzte Aggregation nach Kampf, Zeitfenster, Owner, Skill/Effect und benannter Messgröße. |
| Detailtrace / Profiler | Optionaler begrenzter Verlauf einzelner Operationen und technischer Kosten. |

Ein Gameplay-Trigger ist kein optionaler Telemetrie-Listener. Ein Statistik-Observer verändert keinen State, zieht kein RNG, verschiebt keine Queue und beeinflusst keine Eligibility. Off, Counters und FullTrace müssen dieselben Gameplay-Ergebnisse liefern. Ein Observerfehler darf einen bereits committeten Zustand nicht zurücknehmen; Diagnose und gegebenenfalls isoliertes Abschalten erfolgen außerhalb des Gameplay-Vertrags.

### Mengen und Zählregeln

Eine kausale Buchung hat eine eindeutige Identität. CurrentEffect-, RootSkill-, Provider- und Owner-Sicht sind alternative Aufschlüsselungen, keine zusätzlich addierbaren Schadensmengen. 800 Cloud-Damage mit Root-Beiträgen 500 Bolt und 300 Slash bleibt 800.

DPS nennt die Basis: IncomingDamage, DamageTaken oder qualifizierter tatsächlicher Vital-Verlust. Damage, Life-Punkte, Mana-Kosten und Ward-Punkte werden nicht ohne definierten Wechselkurs addiert. Verhinderung, Absorption, Routing, Stagger und Recovery bleiben getrennte Konten.

Stored/Released/Cancelled/Remaining-Stagger beschreiben zeitliche Weitergabe. Sie werden nicht zusätzlich zum später zugestellten Schaden gezählt. GrossLoss und GrossRecovery bleiben sichtbar, auch wenn NetDelta klein ist.

### Proc-Kette mit Nennern

Zu unterscheiden sind ObservedEvents, FilterEligible, Available, Attempted, RollSucceeded, RepeatAllowed, EffectCommitted und gegebenenfalls SkillCommitted/Cancelled. Nicht jede Mechanik braucht alle Counter permanent, aber ihre Semantik muss abfragbar sein.

```text
100 beobachtete Events
40 eligible
20 tatsächliche Rollversuche
10 Erfolge

50 % Erfolg pro Versuch
10 % Procs pro beobachtetem Event
```

Diese Zahlen werden nicht beide als dieselbe ProcChance beschriftet. Ein SharedRoll wird einmal als RNG-Versuch gezählt, obwohl mehrere Hits sein Ergebnis nutzen können. Quoten-/Cooldown-Ablehnungen sind keine fehlgeschlagenen RNG-Rolls.

### Datenqualität und Speicher

Die Reports führen getrennt `TotalsComplete`, `BreakdownComplete` und `TraceComplete`. Ein erschöpftes Diagnosebudget begrenzt die Beobachtung, nicht die Anzahl der Gameplay-Ereignisse. Observerarbeit zählt nicht als zusätzlicher Gameplay-Workverbrauch, der denselben Run nur bei aktiviertem Trace vorzeitig beendet. Bei begrenzten Aufschlüsselungen bleiben nicht mehr zuordenbare Anteile in Other/Unresolved mit erhaltener Summe. Eine volle Detail-Queue darf keine Gameplay-Events entfernen.

ExecutionId, TargetId und vollständige Lineage werden nicht automatisch zu unbeschränkten dauerhaften Metric-Dimensionen. Normale Meter verwenden begrenzte Dimensionen und Zeitfenster. Detaillierte Exporte sind ausdrückliche Aktionen; lokale Combat Insights erfordern keinen Netzwerkdienst.

Trace-Daten besitzen ihren Inhalt oder einen gültigen Lebensdauervertrag. Ein später gelesener Trace darf nicht auf einen inzwischen wiederverwendeten Payload-Puffer zeigen. Laufende Loops dürfen ohne ausdrücklichen Voll-Export nicht unbegrenzte Vorfahrenbäume ansammeln.

### Erklärung und kausaler Item-Beitrag

Explain-Trace beantwortet: Welche Regeln, Quellen, Snapshots und Phasen erzeugten diese Zahl? Compiler-Trace erklärt hinzugefügte Tags, Policy-Contributors, Resource-Bindings, Conditions und Konflikte.

Die Frage nach dem Nutzen eines Items ist ein Vergleich zweier definierter Builds unter vergleichbaren Bedingungen. Mehrere Item-Differenzen sind wegen Wechselwirkungen nicht allgemein addierbar. Herkunftsanteile in einem Trace sind nicht automatisch kontrafaktische Prozentbeiträge zur Gesamtstärke.

## M23 - Simulator und statistische Aussagekraft

Ein gemeinsamer Combat-Core versorgt Spielhost, Tests, Headless-Simulation und Build-Analyse. Unterschiedliche Auswertungsstrategien werden benannt, nicht als identische Ergebnisse verkauft.

| Modus | Aussage |
|---|---|
| ExactGameplay innerhalb des Vertrags | Dieselben Regeln, RNG-Domains, Zeit-/Spatial-Konfigurationen wie der entsprechende Spielhost. |
| Seeded Monte Carlo | Mehrere Gameplay-Runs zur Auswertung von Streuung, Quantilen und Überlebenswahrscheinlichkeiten. |
| AverageRoll-Diagnose | Bewusst determinisierte Eingangsrolls; bei Nichtlinearität kein allgemeiner Erwartungswert. |
| Minimum-/Maximum-Diagnose | Gegenproben bestimmter Rollwerte, nicht automatisch globale Best-/Worst-Cases aller Trigger-Builds. |
| Approximated Fast-forward | Ausdrücklich dokumentierte Näherung mit ihrem Zeit-/Fehlervertrag. |

Gameplay-Maximum- oder Average-Policies einer Klasse werden im echten Simulator nicht durch ein verborgenes Override gelöscht. Ein diagnostischer Override wird separat ausgewiesen.

Nichtlinearität ist relevant: Für diskret gleichverteilten Eingang 0..200, ArmorCapacity 100 und 50 % Guard ist der erwartete Ausgang `12575/201`, etwa 62,56. Armor auf dem mittleren Eingang 100 ergibt nur 50. Das ist keine universelle ExpectedValue-Strategie.

Reports nennen mindestens Build-/Ruleset-/Content-Version, Encounter, Messbasis, Warm-up, Simulationshorizont, Messfenster, Seedplan, Numerik-/Spatial-Modus und CompletionStatus. DPS verwendet Simulationszeit; Wall-clock misst Performance. Burst, kampfweiter Durchschnitt und SteadyState sind unterschiedliche Kennzahlen.

Survival wird gegen einen benannten Horizont und ein Encounter-Modell gemessen. Gemeinsame CritSamples erzeugen korrelierte Hits, keine unabhängigen Crit-Stichproben pro Projektil. Gescheiterte Runs werden nicht herausgefiltert. BudgetExceeded/abgebrochene Runs sind weder Siege noch gewöhnliche Tode und dürfen keine scheinbar vollständige Erfolgsquote erzeugen.

Gepaarte Seedpläne helfen bei Buildvergleichen, erzwingen aber bei veränderten Ketten keine identischen kausalen Ereignisse. Ein stationärer endlos heilender Dummy beantwortet eine andere Frage als ein tatsächliches Kampf-/Überlebensszenario.

## M24 - QA und Referenzszenarien

QA beginnt mit dem ersten implementierten Baustein. Der einfache lesbare Referenzresolver geht aggressiver Optimierung voraus. Compiled Plans, Aggregation, Pooling, Dirty-Rebuilds und Fast-forward werden gegen ihn verglichen. Zwei Modelle mit derselben ungeprüften fehlerhaften Kernformel liefern keinen starken unabhängigen Nachweis.

| Testfamilie | Idler-Anforderung |
|---|---|
| Unit/Beispiel | Eindeutige Formeln, Endpunkte, Scope, Eligibility, Shares, Kostenverhältnisse. |
| Property/Invariant | Erhaltung innerhalb der vorgesehenen Phase/Einheit; einmalige Budgetverwendung; gültige Bereiche. |
| Metamorphic | Ungeordnete Modifier permutieren; gleiche interne Fragmente aufteilen; Telemetrie an/aus. |
| Differential | Referenz gegen optimiert; FullCompile gegen inkrementell; Headless gegen Godot-Host. |
| Zustandsmodell | Apply, Refresh, Charm, ReplaceResource, AdvanceTime, SaveLoad in erzeugten Reihenfolgen. |
| Mutation | Plus statt Mal, falscher Scope, verdoppeltes Budget, falscher Owner, ignorierter Snapshot werden erkannt. |
| Fault Injection | Fehlerhafter Observer, ungültige Daten, fehlende Dateien, zerrissener Save, abgebrochener Export. |
| Last/Soak | Breite/tiefe Ketten, viele Beiträge, lange Horizonte, begrenzter Speicher und sichtbarer Abbruchstatus. |
| Statistik/Balance | Gegnerdichte, Trigger-Korrelationen, ScaledSource-Sensitivität, Survivor-Bias und Alternativprofile. |

### Verbindliche Referenzszenarien

| ID | Szenario und Kernprüfung |
|---|---|
| Q01 | Conversion >100 %, gleiche Ziele, Reste und Ketten: Erhaltung vor normalem Scaling. |
| Q02 | Spätes Extra und neue DoT-Form: SourceBasis separat von OutputPlan, keine neue Parent-Assembly/Feedback. |
| Q03 | Ranges: CollapseBetween, Contributor-Deduplikation/Gewichte, Endpunkte, logische Aggregationsgrenzen. |
| Q04 | Crit: geteilter Sample, unterschiedliche Targets, gleiche Shotgun-Bedingungen, Guaranteed/Forbidden. |
| Q05 | HitEligibility: Kontakt-Episode, Versuchszählung, Evade und Return ohne Callback-Mehrfachhits. |
| Q06 | Shotgun: einzelne Block-/Evasion-Rolls, echte Armorbudgets, gemeinsamer Barrierbestand. |
| Q07 | H01: Hit 1 sieht BeforeHit; Hit 2 sieht direkte Applications; verzögerter Dispatch ändert Hit-1-Sicht nicht. |
| Q08 | SharedReserve: Life/Mana-Claims, Protectionkosten und Fortification werden einmalig finanziert. |
| Q09 | R03: Loss 40 plus Restore 20 bleibt Brutto 40/20; RemainAtOne erzeugt keine fiktive Heilung. |
| Q10 | Fairy: Ward vital, konfigurierbare Protection, PreDefeat vor Commit, Vital-Ward-Leech in richtigen Einheiten. |
| Q11 | Shortfall: fehlende/erschöpfte Route, Wechselkurs, Fallback, keine lautlose Damage-Löschung. |
| Q12 | Poison -> Charm -> Death -> Explosion/Spread: Owner bleibt, tödliche direkte Application im DeathSnapshot. |
| Q13 | DoT: LifetimeCut versus EffectRate, finaler Pulse, keine extra Procs/Caps pro technischem Segment. |
| Q14 | Stagger: individuelle Termine, einmalige Mitigation, aktuelle Protection beim Release, keine Doppelbuchung. |
| Q15 | Leech/Recoup: Laufzeiten, volle Ressourcen, RateCap-Waste, Bruttoverlust, kein Overkill. |
| Q16 | Rebuild/Handles: altes Projektil behält Snapshot; veralteter Timer und wiederverwendeter Slot bleiben sicher. |
| Q17 | Zeit/Spatial: ungeplante Depletion vor Queue-Event, Kontakt vor Zeitsprung, gleiche Host-Ergebnisse. |
| Q18 | Budgets: ZeroTime-Loop und positive verzögerte Verzweigung ergeben sichtbaren unvollständigen Run. |
| Q19 | Telemetrie: Off/Counters/FullTrace gleicher State, korrekte Nenner, Other-Summe und Owned-Trace-Daten. |
| Q20 | Save/Replay: Commit-Grenze, Versionen, sichere alte Generation bei Fehler, Roundtrip des State-/Eventvertrags. |
| Q21 | Weapon/Costs: lokaler Bonus einmalig, pro Execution definierte Contribution, atomare mehrteilige Zahlung. |
| Q22 | Structural: doppelte Quellenbeiträge, mehrwertige Roles, keine oszillierende Transformation. |

### Grenzen der Invarianten

Conversion erhält ihre Menge; As Extra und More dürfen danach erhöhen. Routing erhält seinen Payload; ein expliziter Prevent-Effekt darf ihn anschließend verringern. Ein echter Hit darf nicht mit einem anderen vertauscht werden, wenn seine Applications den Folgehit beeinflussen. Mehr eines beliebigen Stats ist bei Min-/Max-/Policy-Builds nicht automatisch monoton besser.

Die mitgelieferten Python-Referenzprüfungen sind Rechen- und kleine Protokollmodelle. Sie ersetzen keine C#-Unit-/Integrationstests, keine Godot- oder Dateisystemtests und keine Benchmarks. Tatsächlicher Prüfumfang steht im separaten Nachweisdokument.

## M25 - Authoring, Diagnosen und technische Abnahme

Content-IDs sind stabile Identitäten, nicht Displaynamen. Umbenennen einer persistenten ID benötigt Alias oder Migration. Skill, Status, Affix, Relic, Klasse und Trait komponieren gemeinsame Primitive. ItemInstance speichert gerollte Werte und Referenzen, nicht eine eigene zweite Modifier-Engine.

Jede implementierte Primitive braucht Schema, Einheiten, Eingangsvalidierung, Snapshot-/Zeitvertrag, Scope, Ergebnis-/Eventvertrag und wenigstens ein erklärbares Referenzbeispiel. Neue Mechaniken dürfen gezielten C#-Code benötigen; das ist kein Scheitern des datengetriebenen Systems.

Ungültige Gewichte, negative Transferanteile, nichtpositive Verbrauchsverhältnisse, nichtganzzahlige Versuchszahlen, nichtendliche Werte und ungültige zusammengesetzte Abhängigkeiten werden früh gemeldet. Implizite Sondermechanik durch Division durch null ist ausgeschlossen.

Performance ist ein zu messendes Ziel, keine aus dem Datenmodell ableitbare Garantie. Gemessen werden unter anderem Allokationen, live/peak Speicher, Queuegröße, aktive Statusbeiträge, Pathanzahl vor/nach Aggregation, Compile-Zeit, Ereignisse pro Simulations-/Wall-clock-Zeit und Observerkosten.

Ein vereinbarter Zielhardware-/Encounter-Katalog und eine Messkonfiguration gehen numerischen Abnahmeschwellen voraus. Kein absolutes events/s-Ziel wird ausgedacht. Sicherheitsbudgets wirken bereits im Referenzhost; spätere Optimierung darf sie nicht als verdeckte Gameplay-Caps missbrauchen.

## M26 - Offen, deferred und bewusst nicht festgeschrieben

Die folgenden Punkte sind keine ungelösten Widersprüche zwischen zwei gleichzeitig geltenden Regeln. Es sind explizite Konfigurationen, Technikentscheidungen oder spätere Features. Vor Nutzung im jeweiligen Implementierungsschritt müssen sie ausgewählt, dokumentiert und getestet sein. Beispielzahlen im Manifest sind keine stillen Produktionsdefaults.

| Punkt | Entscheidung vor produktiver Nutzung |
|---|---|
| Produktionsnumerik und Incremental-Größenordnung | Zahlentyp, Range, endliche Präzision, Vergleichsvertrag und Fehlerbehandlung. |
| Toolchain | Tatsächliche Godot/.NET/Test-Versionen erfassen und fixieren; alte Chat-Versionen sind kein aktueller Nachweis. |
| Zeit/RNG | Zeitauflösung, stabiles RNG-Verfahren, Sampling-Auflösung und Schlüsselversion. |
| Scheduler-Gleichstände | Vollständige getestete Reihenfolge für externe Inputs, fachliche Enden und andere gleichzeitige Einheiten. Lokaler Terminal-Pulse-vor-Expire-Vertrag bleibt verbindlich. |
| Defense-Zahlen | Armor-Profile, Resistance-Grenzen, Rating-/Accuracy-Kurve, Chance-/Effectiveness-Werte. |
| Ward/Fortification | Konkrete Decay-, Erzeugungs-, Threshold- und class-spezifische Konfigurationen. |
| Routing und Zahlung | Explizite Fallback-/Shortfall-Pläne je Mechanik, Wechselkurse und fremde Resource-Auswahl. |
| Skill-Lifecycle | Exakte Cost-/Cooldown-/Charge-/Interrupt-Profile je Skill. |
| Nichtlineare Continuous/Spatial-Fälle | Unterstützte Integrationsverfahren, Fehlertoleranzen und sichtbare Unsupported-/Approximation-Resultate. |
| Budgets und Zielhardware | Repräsentative Lastprofile und messbare technische Abnahmeschwellen. |
| Authoring-Format/Tools | Zunächst ein einfacher Adapter; eigener Editor erst bei nachgewiesenem Bedarf. |
| Optionale Mechaniken | Alternatives Lineage-Ruleset, lokale Armor, vollwertige Spectre/Capture-Inhalte, allgemeines DoT-to-Hit, exotische LiveScaling-/Rollgruppenvarianten. |

Die Architektur darf aktive manuelle Dodge-/Parry-Inputs und weitere Spiele unterstützen. Idler v1 muss diese nicht alle als spielbare Inhalte anbieten. Ebenso entsteht kein Pflichtumfang aus jedem im Gespräch genannten Unique-Beispiel.

## Anhang A - Bewusst ersetzte frühere Aussagen

| Frühere Formulierung | Jetzt maßgeblich |
|---|---|
| DamageTaken liegt vor Barrier/Ward | DamageTaken nach Protection; davor PostTakenScalingDamage. |
| Commit, dann WouldBeDefeated | Projected PreDefeat vor sichtbarem gemeinsamen Commit. |
| Atomic Commit bedeutet nur einen NetDelta | Finaler State plus akzeptiertes Bruttoledger. |
| Armor Capacity je DamagePath | Ein gemeinsames fachliches Hitbudget. |
| Alle Hits teilen boolesches CritResult | R01: Sample geteilt, Erfolg target-/hitbezogen. |
| Spätes Child hat globale Conversion verpasst | R02: eigener typisierter OutputPlan. |
| Kein Modifier darf Source und Child skalieren | Je Knoten einmal; ausdrückliches ScaledSource-Double-Dipping erlaubt. |
| Derived verwendet stets den konkreten Parent-Roll | Default ScaledPreCrit-Range; RolledPreCrit separat. |
| Min/Max nach jedem Modifier clampen | Kandidaten unabhängig, logische CollapseBetween-Grenze. |
| Ganze Hit-Gruppe maximal, sobald ein Path konstant ist | Nichtleere ausgewählte Pathmenge muss die Bedingung insgesamt erfüllen. |
| Genau eine Resource pro Role | Mehrwertige Role; Kardinalität wird vom Verbraucher verlangt. |
| Enemy ist intrinsischer Actor-Tag | Identität getrennt von Allegiance/Relationship. |
| Fortification ist automatische Heilung | Allgemeiner finanzierter Resource-Schutz; RecoveryReserve ist andere Mechanik. |
| Damage-/Leech-/Stagger-Tick = technischer Schritt | Nur explizite fachliche Pulse oder Mengen-/Zeitgrenzen erzeugen Gameplay-Semantik. |
| Average-Roll liefert erwarteten Kampfwert | Nur in passenden Modellen; sonst benannte Näherung oder Gameplay-Simulation. |
| DAG oder Delay verhindert Work-Explosion | Zusätzliche technische Budgets und ehrlicher CompletionStatus. |
| Telemetry off darf alle Events entfernen | Gameplay-Ereignisse bleiben; nur Beobachtung ist optional. |

## Anhang B - Referenzspiele und belegter Review-Umfang

PoE 1/2, Grim Dawn, Last Epoch, Titan Quest II, Soulstone Survivors, Ghostlore, Siralim Ultimate, Chronicon, Diablo IV und Torchlight: Infinite dienten dem vorherigen risikoorientierten Vergleich. Die im Audit genannten Patchstände und Entwicklerbegründungen sind historische Belege, keine hier neu geprüfte vollständige Live-Dokumentation dieser Spiele.

Die konkreten Quellen, Datierungen und Grenzen stehen in [Audit 2, Abschnitte 3 und 10](sources/Idler_Architektur_Audit_2.md) und [dessen Quellenregister](sources/Audit_2_Quellen.json). Das Manifest übernimmt unsere bestätigten Schlussfolgerungen, nicht automatisch fremde Formeln. Besonders wichtig bleiben die Risiken Scope-Leakage, rekursiver Arbeitsmenge, ungesicherter Datenlebensdauer, irreführender Statistik und Teil-Saves.

## Anhang C - Änderungsverfahren ab Version 1.0

Eine neue Regeländerung nennt die betroffenen Manifest-IDs, ersetzte Bedeutung, Zahlen-/State-Beispiele, Testfolgen und gegebenenfalls Save-/Replay-Migration. Bestätigte Defaults werden nicht durch ein beiläufiges späteres Beispiel umdefiniert.

Die nächste Arbeitsphase ist Implementierung mit Referenztests. Taucht dabei ein echter Widerspruch auf, wird genau dieser Vertrag erneut geöffnet; er wird weder still kaschiert noch zum Anlass genommen, ohne Befund das gesamte Konzept wieder von vorn zu entwerfen.
