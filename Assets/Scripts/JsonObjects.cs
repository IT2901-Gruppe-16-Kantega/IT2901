using System;
using System.Collections.Generic;

// ReSharper disable InconsistentNaming

[Serializable]
public class Type {
	public int id;
	public string navn;
}

[Serializable]
public class Enhet {
	public int id;
	public string kortnavn;
	public string navn;
}

[Serializable]
public class Egenskaper {
	public int datatype;
	public string datatype_tekst;
	public Enhet enhet;
	public int enum_id;
	public int id;
	public string navn;
	public string verdi;
}

[Serializable]
public class Geometri {
	public bool egengeometri;
	public int srid;
	public string wkt;
}

[Serializable]
public class Kontraktsområer {
	public string navn;
	public int nummer;
}

[Serializable]
public class Riksvegruter {
	public string navn;
	public string nummer;
	public string periode;
}

[Serializable]
public class Vegreferanser {
	public int fra_meter;
	public int fylke;
	public int hp;
	public string kategori;
	public int kommune;
	public string kortform;
	public int nummer;
	public string status;
	public int til_meter;
}

[Serializable]
public class Stedfestinger {
	public double fra_posisjon;
	public string kortform;
	public string retning;
	public string sideposisjon;
	public double til_posisjon;
	public int veglenkeid;
}

[Serializable]
public class Lokasjon {
	public List<int> fylker;
	public Geometri geometri;
	public List<int> kommuner;
	public List<Kontraktsområer> kontraktsområder;
	public List<int> regioner;
	public List<Riksvegruter> riksvegruter;
	public List<Stedfestinger> stedfestinger;
	public int strekningslengde;
	public List<int> vegavdelinger;
	public List<Vegreferanser> vegreferanser;
}

[Serializable]
public class Stedfesting {
	public double fra_posisjon;
	public string kortform;
	public string retning;
	public string sideposisjon;
	public double til_posisjon;
	public int veglenkeid;
}

[Serializable]
public class Vegreferanse {
	public int fra_meter;
	public int fylke;
	public int hp;
	public string kategori;
	public int kommune;
	public string kortform;
	public int nummer;
	public string status;
	public int til_meter;
}

[Serializable]
public class Vegsegmenter {
	public int fylke;
	public Geometri geometri;
	public int kommune;
	public int region;
	public Stedfesting stedfesting;
	public int strekningslengde;
	public int vegavdeling;
	public Vegreferanse vegreferanse;
}

[Serializable]
public class Barn {
	public Type type;
	public List<int> vegobjekter;
}

[Serializable]
public class Foreldre {
	public Type type;
	public List<int> vegobjekter;
}

[Serializable]
public class Relasjoner {
	public List<Barn> barn;
	public List<Foreldre> foreldre;
}

[Serializable]
public class Objekter {
	public List<Egenskaper> egenskaper;
	public Geometri geometri;
	public string href;
	public int id;
	public Lokasjon lokasjon;
	public Metadata metadata;
	public List<GpsManager.GpsLocation> parsedLocation;
	public Relasjoner relasjoner;
	public List<Vegsegmenter> vegsegmenter;
}

[Serializable]
public class Metadata {
	public double bearing;
	public double distance;
	public string notat;
	public Type type;
}

[Serializable]
public class NvdbObjekt {
	public List<Objekter> objekter;
// TODO create a metadata to say where it is from and who made the change?
}

[Serializable]
public class RoadSearchObject {
	public string date;
	public string description;
	public long key;
	public List<Objekter> report;
	public List<Objekter> roadObjects;
	public List<Objekter> roads;
}

[Serializable]
public class Report {
	public List<ReportObject> reportObjects;
}

[Serializable]
public class ReportObject {
	public List<ReportEgenskap> endringer;
	public int vegobjekt;
}

[Serializable]
public class ReportEgenskap {
	public string beskrivelse;
	public string dato;
	public ReportEgenskap2 egenskap;
	public string type;
}

[Serializable]
public class ReportEgenskap2 {
	public int datatype;
	public string datatype_tekst;
	public int id;
	public string navn;
	public string verdi;
}