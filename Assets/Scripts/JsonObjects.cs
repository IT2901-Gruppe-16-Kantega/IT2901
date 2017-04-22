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
	public string navn;
	public string kortnavn;
}

[Serializable]
public class Egenskaper {
	public int id;
	public string navn;
	public int datatype;
	public string datatype_tekst;
	public string verdi;
	public int enum_id;
	public Enhet enhet;
}

[Serializable]
public class Geometri {
	public string wkt;
	public int srid;
	public bool egengeometri;
}

[Serializable]
public class Kontraktsområer {
	public int nummer;
	public string navn;
}

[Serializable]
public class Riksvegruter {
	public string nummer;
	public string navn;
	public string periode;
}

[Serializable]
public class Vegreferanser {
	public int fylke;
	public int kommune;
	public string kategori;
	public string status;
	public int nummer;
	public int hp;
	public int fra_meter;
	public int til_meter;
	public string kortform;
}

[Serializable]
public class Stedfestinger {
	public int veglenkeid;
	public double fra_posisjon;
	public double til_posisjon;
	public string kortform;
	public string retning;
	public string sideposisjon;
}

[Serializable]
public class Lokasjon {
	public List<int> kommuner;
	public List<int> fylker;
	public List<int> regioner;
	public List<int> vegavdelinger;
	public List<Kontraktsområer> kontraktsområder;
	public List<Riksvegruter> riksvegruter;
	public List<Vegreferanser> vegreferanser;
	public List<Stedfestinger> stedfestinger;
	public Geometri geometri;
	public int strekningslengde;
}

[Serializable]
public class Stedfesting {
	public int veglenkeid;
	public double fra_posisjon;
	public double til_posisjon;
	public string kortform;
	public string retning;
	public string sideposisjon;
}

[Serializable]
public class Vegreferanse {
	public int fylke;
	public int kommune;
	public string kategori;
	public string status;
	public int nummer;
	public int hp;
	public int fra_meter;
	public int til_meter;
	public string kortform;
}

[Serializable]
public class Vegsegmenter {
	public Stedfesting stedfesting;
	public Geometri geometri;
	public int kommune;
	public int fylke;
	public int region;
	public int vegavdeling;
	public Vegreferanse vegreferanse;
	public int strekningslengde;
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
	public int id;
	public string href;
	public List<Egenskaper> egenskaper;
	public Geometri geometri;
	public Lokasjon lokasjon;
	public List<Vegsegmenter> vegsegmenter;
	public List<GpsManager.GpsLocation> parsedLocation;
	public Relasjoner relasjoner;
	public Metadata metadata;
    public bool markert = false;
}

[Serializable]
public class Metadata {
	public Type type;
	public double distance;
	public double bearing;
	public string notat;
}

[Serializable]
public class NvdbObjekt {
	public List<Objekter> objekter;
// TODO create a metadata to say where it is from and who made the change?
}