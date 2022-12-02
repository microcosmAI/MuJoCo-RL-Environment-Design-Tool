using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

public class light
{
    [XmlAttribute]
    public string diffuse;
    [XmlAttribute]
    public string pos;
    [XmlAttribute]
    public string dir;
}
