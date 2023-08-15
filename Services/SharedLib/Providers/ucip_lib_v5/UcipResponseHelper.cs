using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace SharedLib.Providers.ucip_lib_v5
{
    public class UcipResponseHelper
    {
        public static readonly UcipResponseHelper Instance = new UcipResponseHelper();

        private UcipResponseHelper()
        {
        }

        public string Extract(string response, string parameterName, string parameterType)
        {
            int num = response.IndexOf(">" + parameterName + "<", 20, StringComparison.CurrentCultureIgnoreCase);
            int num2 = response.IndexOf(parameterType, num + 1, StringComparison.CurrentCultureIgnoreCase);
            int num3 = num2 + parameterType.Length;
            int num4 = response.IndexOf("</", num3 + 1, StringComparison.CurrentCultureIgnoreCase);
            return (num4 - num3 >= 0) ? response.Substring(num3, num4 - num3) : "";
        }

        public string ExtractArray(string response, string parameterName, string parameterType)
        {
            int num = response.IndexOf(">" + parameterName + "<", 20, StringComparison.CurrentCultureIgnoreCase);
            int num2 = response.IndexOf(parameterType, num + 1, StringComparison.CurrentCultureIgnoreCase);
            int num3 = num2 + parameterType.Length;
            int num4 = response.IndexOf("</array", num3 + 1, StringComparison.CurrentCultureIgnoreCase);
            return (num4 - num3 >= 0) ? response.Substring(num3, num4 - num3) : "";
        }

        public string ExtractResponseCode(string response)
        {
            int num = response.IndexOf(">responseCode<", 20, StringComparison.CurrentCultureIgnoreCase);
            int num2 = response.IndexOf("<i4>", num + 1, StringComparison.CurrentCultureIgnoreCase);
            int num3 = num2 + 4;
            int num4 = response.IndexOf("</", num3 + 1, StringComparison.CurrentCultureIgnoreCase);
            return (num4 - num3 >= 0) ? response.Substring(num3, num4 - num3) : "";
        }

        public List<string> ExtractAll(string response, string parameterName, string parameterType)
        {
            List<string> list = new List<string>();
            int num = 20;
            do
            {
                num = response.IndexOf(">" + parameterName + "<", num + 1, StringComparison.CurrentCultureIgnoreCase);
                int num2 = response.IndexOf(parameterType, num + 1, StringComparison.CurrentCultureIgnoreCase);
                int num3 = num2 + parameterType.Length;
                int num4 = response.IndexOf("</", num3 + 1, StringComparison.CurrentCultureIgnoreCase);
                string item = ((num4 - num3 >= 0) ? response.Substring(num3, num4 - num3) : "");
                list.Add(item);
            }
            while (num > 0);
            return list;
        }

        public List<string> ExtractAll(string response, string parameterName)
        {
            List<string> list = new List<string>();
            int num = response.IndexOf(">" + parameterName + "<", 20, StringComparison.CurrentCultureIgnoreCase);
            int num2 = response.IndexOf("</array", num + 1, StringComparison.CurrentCultureIgnoreCase);
            do
            {
                int num3 = response.IndexOf("<struct>", num + 1, StringComparison.CurrentCultureIgnoreCase);
                if (num3 < 0 || num3 >= num2)
                {
                    break;
                }
                int num4 = num3 + 8;
                int num5 = response.IndexOf("</struct>", num4 + 1, StringComparison.CurrentCultureIgnoreCase);
                string item = ((num5 - num4 >= 0) ? response.Substring(num4, num5 - num4) : "");
                num = num5;
                list.Add(item);
            }
            while (num > 0);
            return list;
        }

        public List<string[][]> ExtractAll(string response, string parameterName, string[][] structure)
        {
            List<string[][]> list = new List<string[][]>();
            int num = response.IndexOf(">" + parameterName + "<", 20, StringComparison.CurrentCultureIgnoreCase);
            int num2 = response.IndexOf("</array", num + 1, StringComparison.CurrentCultureIgnoreCase);
            do
            {
                int num3 = response.IndexOf("<struct>", num + 1, StringComparison.CurrentCultureIgnoreCase);
                if (num3 < 0 || num3 >= num2)
                {
                    break;
                }
                int num4 = num3 + 8;
                int num5 = response.IndexOf("</struct>", num4 + 1, StringComparison.CurrentCultureIgnoreCase);
                string response2 = ((num5 - num4 >= 0) ? response.Substring(num4, num5 - num4) : "");
                num = num5;
                string[][] array = new string[structure.Length][];
                for (int i = 0; i < structure.Length; i++)
                {
                    array[i] = new string[2];
                    array[i][0] = structure[i][0];
                    array[i][1] = Extract(response2, structure[i][0], structure[i][1]);
                }
                list.Add(array);
            }
            while (num > 0);
            return list;
        }

        public List<string[][]> ExtractAllNested(string response, string parameterName, string[][] structure)
        {
            List<string[][]> list = new List<string[][]>();
            string xml = response.Substring(response.IndexOf("<?xml"));
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);
            XmlNode xmlNode = xmlDocument.SelectSingleNode("/methodResponse/params/param/value/struct/member[name='" + parameterName + "']");
            foreach (XmlNode item in xmlNode.SelectNodes("value/array/data/value/struct"))
            {
                string[][] array = new string[structure.Length][];
                int num = 0;
                foreach (XmlNode item2 in item.SelectNodes("member"))
                {
                    string name = item2.SelectSingleNode("name").InnerXml;
                    string innerXml = item2.SelectSingleNode("value").ChildNodes[0].InnerXml;
                    if (structure.Any((string[] p) => p[0] == name))
                    {
                        array[num] = new string[2];
                        array[num][0] = name;
                        array[num++][1] = innerXml;
                    }
                }
                array = array.Where((string[] p) => p != null).ToArray();
                list.Add(array);
            }
            return list;
        }

        public List<string[][]> ExtractAllNestedSub(string response, string parameterName, string[][] structure)
        {
            List<string[][]> list = new List<string[][]>();
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(response);
            foreach (XmlNode item in xmlDocument.SelectNodes("data/value/struct"))
            {
                string[][] array = new string[structure.Length][];
                int num = 0;
                foreach (XmlNode item2 in item.SelectNodes("member"))
                {
                    string name = item2.SelectSingleNode("name").InnerXml;
                    string innerXml = item2.SelectSingleNode("value").ChildNodes[0].InnerXml;
                    if (structure.Any((string[] p) => p[0] == name))
                    {
                        array[num] = new string[2];
                        array[num][0] = name;
                        array[num++][1] = innerXml;
                    }
                }
                array = array.Where((string[] p) => p != null).ToArray();
                list.Add(array);
            }
            return list;
        }

        public List<int> AllIndexesOf(string str, string value, int loc)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("the string to find may not be empty", "value");
            }
            List<int> list = new List<int>();
            int startIndex = loc + 1;
            while (true)
            {
                startIndex = str.IndexOf(value, startIndex);
                if (startIndex == -1)
                {
                    break;
                }
                list.Add(startIndex);
                startIndex += value.Length;
            }
            return list;
        }
    }

}
