using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ExtraFunctions
{
    public static class XmlWork
    {
        public static T DeserializeXML<T>(this XElement element)
        {
            var serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(element.CreateReader());
        }

        public static XElement SerializeXML<T>(this T obj)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, obj);
                stream.Position = 0;
                using (XmlReader reader = XmlReader.Create(stream))
                {
                    var xml = XElement.Load(reader);
                    xml.Attributes().Where(f => f.IsNamespaceDeclaration).Remove();

                    return xml;
                }
            }
        }

        public static XElement FindElement(this IEnumerable<XElement> elements, string key, string value)
        {
            foreach (var item in elements)
            {
                var element = item.Element(key);
                if (element != null && (string)element == value)
                    return item;
            }
            return null;
        }
        /// <summary>
        /// Получает елемент в parent или если его нет создает
        /// </summary>
        /// <param name="parent">Родительский элемент</param>
        /// <param name="name">Название элеметна</param>
        /// <returns>Новый элемент</returns>
        public static XElement GetOrCreateElement(this XElement parent, string name)
        {
            var element = parent.Element(name);
            if (element == null)
            {
                element = new XElement(name);
                parent.Add(element);
            }
            return element;
        }
        /// <summary>
        /// Получает атрибут в parent или если его нет создает
        /// </summary>
        /// <param name="parent">Родительский элемент</param>
        /// <param name="name">Название атрибута</param>
        /// <param name="defaultValue">Значение при создании атрибута</param>
        /// <returns>Новй атрибут</returns>
        public static XAttribute GetOrCreateAttribute(this XElement parent, string name, object defaultValue = null)
        {
            var attr = parent.Attribute(name);
            if (attr == null)
            {
                attr = new XAttribute(name, defaultValue);
                parent.Add(attr);
            }
            return attr;
        }
    }
}
