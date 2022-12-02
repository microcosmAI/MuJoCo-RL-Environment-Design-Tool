using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

public class worldbody{
    [XmlElement]
    public light[] light;
    [XmlElement]
    public geom[] geom;
    [XmlElement]
    public body[] body;
}
