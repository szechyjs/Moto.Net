using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Moto.Net.Mototrbo.FXP;

public class FeatureList
{
  List<Feature> features;

  public FeatureList(List<Feature> features)
  {
    this.features = features;
  }

  public Feature GetFeature(string name)
  {
    return features.Find(f => f.Name == name);
  }

  public static FeatureList LoadFile(string fileName)
  {
    using var file = File.OpenText(fileName);
    var feats = JsonConvert.DeserializeObject<List<Feature>>(file.ReadToEnd());
    return new FeatureList(feats);
  }
}
