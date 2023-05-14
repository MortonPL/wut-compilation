using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using VerboseCLI.Entities;

namespace VerboseCLI
{
    public static class XmlParserLocale
    {
        public static bool Parse(Stream stream, out Locale locale)
        {
            var reader = new XmlTextReader(stream);
            string name = "";
            string unknown = "";
            var dict = new Translator<LTU, string>("");
            locale = new();

            // must start with <locale name="something">
            reader.Read();
            if (reader.NodeType != XmlNodeType.Element || reader.Name != "locale") return false;
            while (reader.MoveToNextAttribute())
            {
                if (reader.Name == "name")
                {
                    name = reader.Value;
                    break;
                }
            }
            if (name.Length == 0) return false;

            // loop till the end
            bool stop = false;
            while (!stop && reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        var kvp = new KeyValuePair<string, string>();
                        switch (reader.Name)
                        {
                            case "UnknownLTU":
                                unknown = ReadUnknownLTU(reader);
                                break;
                            case "LTU":
                                if (ReadLTU(reader, ref kvp))
                                    dict.Add((LTU)Enum.Parse(typeof(LTU), kvp.Key), kvp.Value);
                                break;
                        };
                        break;
                    case XmlNodeType.EndElement:
                        stop = true;
                        break;
                    default:
                        break;
                }
            }
            locale = new Locale(name.ToLower(), new Translator<string>(unknown, dict));
            return true;
        }

        private static string ReadUnknownLTU(XmlTextReader reader)
        {
            reader.Read();
            string str = (reader.NodeType == XmlNodeType.Text) ? reader.Value : "";
            reader.Read();
            return str;
        }

        private static bool ReadLTU(XmlTextReader reader, ref KeyValuePair<string, string> kvp)
        {
            while (reader.MoveToNextAttribute())
            {
                if (reader.Name == "name")
                {
                    string key = reader.Value;
                    string value;
                    reader.Read();
                    if (reader.NodeType != XmlNodeType.Text || reader.Value.Length == 0)
                        return false;
                    value = reader.Value;
                    reader.Read();
                    kvp = new KeyValuePair<string, string>(key, value);
                    return true;
                }
            }
            return false;
        }
    }
}
