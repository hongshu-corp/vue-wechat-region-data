using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace process_region
{
  class Program
  {
    static void Main(string[] args)
    {
      var filePath = args[0];
      if (!File.Exists(filePath)) Console.WriteLine("不存在");

      var lines = File.ReadAllLines(filePath);
      var provinces = ProcessLines(lines);

      Save(provinces);
    }

    static void Save(List<Province> provinces)
    {
      var target = Convert(provinces);

      var ms = new MemoryStream();
      var setting = new DataContractJsonSerializerSettings();
      setting.UseSimpleDictionaryFormat = true;
      var ser = new DataContractJsonSerializer(typeof(Dictionary<string, Dictionary<string, string>>), setting);

      ser.WriteObject(ms, target);
      byte[] json = ms.ToArray();
      ms.Close();
      var content = Encoding.UTF8.GetString(json, 0, json.Length);

      using (var sw = new StreamWriter("/your-path/test.json", false))
      {
        sw.WriteLine(content);
      }
    }

    static Dictionary<string, Dictionary<string, string>> Convert(List<Province> provinces)
    {
      var result = new Dictionary<string, Dictionary<string, string>>();

      var provinceDic = new Dictionary<string, string>();
      result.Add("86", provinceDic);

      foreach (var province in provinces)
      {
        AddEntry(province, provinceDic);

        var cityDic = new Dictionary<string, string>();
        result.Add(province.id, cityDic);

        foreach (var city in province.Cites)
        {
          AddEntry(city, cityDic);

          var countyDic = new Dictionary<string, string>();
          result.Add(city.id, countyDic);

          foreach (var county in city.Counties)
          {
            AddEntry(county, countyDic);
          }
        }
      }
      return result;
    }

    static void AddEntry(Base model, Dictionary<string, string> dic)
    {
      if (!dic.ContainsKey(model.id))
      {
        dic.Add(model.id, model.name);
      }
      else
      {
        Console.WriteLine(model.id);
      }
    }
    static List<Province> ProcessLines(String[] lines)
    {
      var provinces = new List<Province>();
      var province = default(Province);
      var city = default(City);
      foreach (var line in lines)
      {
        var id = default(string);
        var parts = line.Split("\t", StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2)
        {
          id = parts[0];
          var right = parts[1];
          var nameAndCode = right.Trim('　').Split(" ", StringSplitOptions.None);
          var name = nameAndCode[nameAndCode.Length - 2];
          var code = nameAndCode[nameAndCode.Length - 1];

          if (!right.StartsWith("　"))
          {
            province = new Province() { id = id, name = name, code = code };
            provinces.Add(province);
          }
          else
          {
            // county
            if (right.StartsWith("　　　"))
            {
              var county = new County() { id = id, name = name, code = code };
              city.Add(county);

            }
            else if (right.StartsWith("　　"))
            { // city
              city = new City() { id = id, name = name, code = code };
              province.Add(city);
            }
          }
        }
      }
      return provinces;
    }
  }

  class Province : Base
  {
    public List<City> Cites { get; set; }

    public Province()
    {
      this.Cites = new List<City>();
    }

    public void Add(City city)
    {
      this.Cites.Add(city);
    }
  }

  class City : Base
  {
    public List<County> Counties { get; set; }

    public City()
    {
      this.Counties = new List<County>();
    }

    public void Add(County county)
    {
      this.Counties.Add(county);
    }
  }

  class County : Base
  {
  }

  class Base
  {
    public string name { get; set; }
    public string code { get; set; }
    public string id { get; set; }
  }
}
