using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

public class body
{
    [XmlAttribute]
    public string pos;
    [XmlAttribute]
    public string name;
    public joint joint;
    public geom geom;
}
