using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MayazucMediaPlayer.Subtitles.OnlineAPIs.OpenSubtitles
{
    public static class OSDBExtensions
    {
        public static async Task<string> ComputeOSDBHash(this FileInfo file)
        {
            using (var stream = file.OpenRead())
            {
                var fileLength = stream.Length;
                long fileHash;

                fileHash = fileLength;
                var longSize = sizeof(long);
                long i = 0;
                byte[] buffer = new byte[longSize];
                var hashStreamLimit = 65536 / longSize;

                while (i < hashStreamLimit && (stream.Read(buffer, 0, longSize) > 0))
                {
                    i++;
                    fileHash += BitConverter.ToInt64(buffer, 0);
                }

                stream.Position = Math.Max(0, fileLength - 65536);
                i = 0;

                while (i < hashStreamLimit && (stream.Read(buffer, 0, longSize) > 0))
                {
                    i++;
                    fileHash += BitConverter.ToInt64(buffer, 0);
                }

                var returnValue = BitConverter.GetBytes(fileHash);
                return (returnValue.Reverse().ToHexString());
            }
        }

        public static XElement SerializeProperties(this Dictionary<string, object> values)
        {

            //XElement rootValue = new XElement("value");
            XElement rootstruct = new XElement("struct");


            foreach (var kvp in values)
            {
                var rootMemberElement = new XElement("member");
                var nameElement = new XElement("name");
                nameElement.Value = kvp.Key;
                var valueElement = new XElement("value");
                XElement valueRoot = null;

                if (kvp.Value != null)
                {
                    if (kvp.Value is string)
                    {
                        valueRoot = new XElement("string");
                    }
                    else if (kvp.Value is double)
                    {
                        valueRoot = new XElement("double");
                    }
                    valueRoot.Value = kvp.Value.ToString();
                }
                else
                {
                    valueRoot.Value = "nil";
                }
                valueElement.Add(valueRoot);

                rootMemberElement.Add(nameElement);
                rootMemberElement.Add(valueElement);
                rootstruct.Add(rootMemberElement);
            }

            //rootValue.Add(rootstruct);

            return rootstruct;
        }

        public static XElement SerializeArray<T>(this IList<T> values)
        {
            var rootArrayElement = new XElement("array");
            var dataElement = new XElement("data");
            foreach (var obj in values)
            {
                XElement rootValueElement = new XElement("value");
                XElement valueElement = null;
                if (obj is string)
                {
                    valueElement = new XElement("string");
                }
                else if (obj is double)
                {
                    valueElement = new XElement("double");
                }
                valueElement.Value = obj.ToString();

                rootValueElement.Add(valueElement);
                dataElement.Add(rootValueElement);
            }
            rootArrayElement.Add(dataElement);
            return rootArrayElement;
        }


        public static XElement SerializeProperties<T>(this IList<T> values)
        {
            var rootArrayElement = new XElement("params");
            foreach (var obj in values)
            {
                var parameterNode = new XElement("param");
                var parameterValueNode = new XElement("value");
                XElement valueNode = null;
                if (obj is string)
                {
                    valueNode = new XElement("string");
                    valueNode.Value = obj.ToString();

                }
                else if (obj is double)
                {
                    valueNode = new XElement("double");
                    valueNode.Value = obj.ToString();

                }
                else if (obj is List<object>)
                {
                    valueNode = (obj as List<object>).SerializeProperties();
                }
                else if (obj is Dictionary<string, object>)
                {
                    valueNode = (obj as Dictionary<string, object>).SerializeProperties();
                }

                parameterValueNode.Add(valueNode);
                parameterNode.Add(parameterValueNode);
                rootArrayElement.Add(parameterNode);
            }



            return rootArrayElement;
        }
    }
}
