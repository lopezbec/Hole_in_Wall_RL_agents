using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

[XmlRoot("ProfileDataCollection")]
public class ProfileDataContainer
{

    public ProfileDataContainer() { }

    [XmlArray("Profiles"), XmlArrayItem("Profile")]
    public List<ProfileData> Profiles = new List<ProfileData>();



    public void Save(string path)
    {
        var serializer = new XmlSerializer(typeof(ProfileDataContainer));
        using (var stream = new FileStream(path, FileMode.Create))
        {
            serializer.Serialize(stream, this);
        }
    }

    public static ProfileDataContainer Load(string path)
    {
        var serializer = new XmlSerializer(typeof(ProfileDataContainer));
        using (var stream = new FileStream(path, FileMode.Open))
        {
            return serializer.Deserialize(stream) as ProfileDataContainer;
        }
    }
}

