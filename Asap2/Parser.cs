using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Asap2
{
    [Serializable]
    public class ParserErrorException : Exception
    {
        public ParserErrorException()
        {
        }

        public ParserErrorException(string message) : base(message)
        {
        }

        public ParserErrorException(string message, Exception inner) : base(message, inner)
        {
        }

        public ParserErrorException(string format, params object[] args) : base(string.Format(format, args))
        {
        }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client. 
        protected ParserErrorException(SerializationInfo info,
            StreamingContext context)
        {
        }

        public override string ToString()
        {
            return base.Message;
        }
    }


    public class Parser
    {
        private readonly IErrorReporter errorHandler;

        public Parser(string fileName, IErrorReporter errorHandler)
        {
            this.fileName = fileName;
            this.errorHandler = errorHandler;
            indentType = "    ";
        }

        public string fileName { get; private set; }
        public string indentType { get; set; }

        /// <summary>
        ///     Parse the provided A2L file.
        /// </summary>
        /// <returns>true if all succeded with no fatal errors</returns>
        public Asap2File DoParse()
        {
            var status = false;
            Asap2Scanner scanner;
            Asap2Parser parser;

            using (var fs = File.Open(fileName, FileMode.Open))
            {
                using (var bs = new BufferedStream(fs))
                {
                    scanner = new Asap2Scanner(bs, errorHandler);
                    parser = new Asap2Parser(scanner, errorHandler);
                    try
                    {
                        status = parser.Parse();
                    }
                    catch (ParserErrorException e)
                    {
                        errorHandler.reportError(e.Message);
                        status = false;
                    }
                }
            }


            if (status)
                return parser.Asap2File;
            return null;
        }

        /// <summary>
        ///     Parse the provided A2L file.
        /// </summary>
        /// <param name="stream">Data stream to parse.</param>
        /// <returns>true if all succeded with no fatal errors</returns>
        public Asap2File DoParse(Stream stream)
        {
            var status = false;
            Asap2Scanner scanner;
            Asap2Parser parser;
            scanner = new Asap2Scanner(stream, errorHandler);
            parser = new Asap2Parser(scanner, errorHandler);
            try
            {
                status = parser.Parse();
            }
            catch (ParserErrorException e)
            {
                errorHandler.reportError(e.Message);
                status = false;
            }

            if (status)
                return parser.Asap2File;
            return null;
        }

        public bool Serialise(Asap2File tree, StreamWriter stream)
        {
            tree.elements.Sort((x, y) => x.OrderID.CompareTo(y.OrderID));
            foreach (var item in tree.elements)
                if (item.GetType() == typeof(FileComment))
                {
                    stream.Write(item.ToString());
                    stream.Write(Environment.NewLine);
                }
                else
                {
                    foreach (var data in SerialiseNode(item, 0)) stream.WriteAsync(data);
                }

            stream.Flush();
            return false;
        }

        private StringBuilder Indent(uint level)
        {
            var tmp = new StringBuilder((int) (indentType.Length * level));
            for (ulong i = 0; i < level; i++) tmp.Append(indentType);
            return tmp;
        }

        private IEnumerable<string> SerialiseNode(object tree, uint indentLevel, ElementAttribute nodeElemAtt = null)
        {
            var baseAtt = AttributeCache<BaseAttribute, MemberInfo>.Get(tree.GetType());

            string elementName = null;
            if (baseAtt != null)
            {
                var pI = SortedPropertyCache.Get(tree.GetType());
                var fI = SortedFieldsCache.Get(tree.GetType());

                for (var i = 0; i < pI.Length; i++)
                {
                    var elemAtt = AttributeCache<ElementAttribute, MemberInfo>.Get(pI[i]);
                    if (elemAtt != null)
                        if (elemAtt.IsName)
                            elementName = (string) pI[i].GetValue(tree);
                }

                if (elementName == null)
                    for (var i = 0; i < fI.Length; i++)
                    {
                        var elemAtt = AttributeCache<ElementAttribute, MemberInfo>.Get(fI[i]);
                        if (elemAtt != null)
                            if (elemAtt.IsName)
                                elementName = (string) fI[i].GetValue(tree);
                    }

                {
                    yield return Environment.NewLine;
                    if (elementName == null) elementName = tree.GetType().Name.ToUpper();

                    if (baseAtt.IsSimple)
                        yield return Indent(indentLevel).Append(elementName).ToString();
                    else
                        yield return Indent(indentLevel).Append("/begin " + elementName).ToString();
                }


                if (fI.Length > 0)
                    foreach (var resultData in SerialiseElement(tree, fI, pI, indentLevel + 1))
                        yield return resultData;

                if (tree.GetType() == typeof(MODULE))
                {
                    // Handle internal module data.
                    var elements = new SortedList<ulong, Asap2Base>();
                    var moduleObj = tree as MODULE;

                    if (moduleObj.elements != null)
                        foreach (var obj in moduleObj.elements)
                            elements.Add(obj.OrderID, obj);

                    if (moduleObj.AxisPtsCharacteristicMeasurement != null)
                        foreach (var obj in moduleObj.AxisPtsCharacteristicMeasurement.Values)
                            elements.Add(obj.orderID(), (Asap2Base) obj);

                    if (moduleObj.CompuTabCompuVtabCompuVtabRanges != null)
                        foreach (var obj in moduleObj.CompuTabCompuVtabCompuVtabRanges.Values)
                            elements.Add(obj.OrderID, obj);

                    if (moduleObj.CompuMethods != null)
                        foreach (var obj in moduleObj.CompuMethods.Values)
                            elements.Add(obj.OrderID, obj);

                    if (moduleObj.Frames != null)
                        foreach (var obj in moduleObj.Frames.Values)
                            elements.Add(obj.OrderID, obj);

                    if (moduleObj.Functions != null)
                        foreach (var obj in moduleObj.Functions.Values)
                            elements.Add(obj.OrderID, obj);

                    if (moduleObj.Groups != null)
                        foreach (var obj in moduleObj.Groups.Values)
                            elements.Add(obj.OrderID, obj);

                    if (moduleObj.Record_layouts != null)
                        foreach (var obj in moduleObj.Record_layouts.Values)
                            elements.Add(obj.OrderID, obj);

                    if (moduleObj.Units != null)
                        foreach (var obj in moduleObj.Units.Values)
                            elements.Add(obj.OrderID, obj);

                    if (moduleObj.User_rights != null)
                        foreach (var obj in moduleObj.User_rights.Values)
                            elements.Add(obj.OrderID, obj);

                    foreach (var element in elements.Values)
                    foreach (var dataNode in SerialiseNode(element, indentLevel + 1))
                        yield return dataNode;
                }

                if (!baseAtt.IsSimple)
                {
                    yield return Environment.NewLine;
                    yield return Indent(indentLevel).Append("/end " + elementName).ToString();
                }
            }
            else if (nodeElemAtt != null)
            {
                // Pure data node
                string data = data = Environment.NewLine + Indent(indentLevel);

                if (nodeElemAtt.IsString)
                {
                    var value = tree.ToString();
                    data += "\"" + value + "\"";
                    yield return Indent(indentLevel).Append(data).ToString();
                }
                else
                {
                    if (tree.GetType().IsEnum)
                    {
                        var value = Enum.GetName(tree.GetType(), tree);
                        data += value;
                        yield return data;
                    }
                    else if (nodeElemAtt.CodeAsHex)
                    {
                        var tmp = (ulong) tree;
                        var value = "0x" + tmp.ToString("X");
                        data += value;
                        yield return data;
                    }
                    else if (tree.GetType() == typeof(decimal))
                    {
                        var value = (decimal) tree;
                        data += value.ToString(CultureInfo.InvariantCulture);
                        yield return data;
                    }
                    else
                    {
                        data += tree.ToString();
                        yield return data;
                    }
                }
            }
        }

        private IEnumerable<string> SerialiseElement(object tree, FieldInfo[] fI, PropertyInfo[] pI, uint indentLevel)
        {
            foreach (var info in pI)
            {
                var att = AttributeCache<ElementAttribute, MemberInfo>.Get(info);
                if (att != null)
                {
                    var objData = info.GetValue(tree);
                    if (objData != null)
                        foreach (var serialiseAttribute in SerialiseAttributeData(objData, info.PropertyType, att,
                            indentLevel))
                            yield return serialiseAttribute;
                }
            }

            foreach (var info in fI)
            {
                var att = AttributeCache<ElementAttribute, MemberInfo>.Get(info);
                if (att != null)
                {
                    var objData = info.GetValue(tree);
                    if (objData != null)
                        foreach (var serialiseAttribute in SerialiseAttributeData(objData, info.FieldType, att,
                            indentLevel))
                            yield return serialiseAttribute;
                }
            }
        }

        private IEnumerable<string> SerialiseAttributeData(object objData, Type objType, ElementAttribute att,
            uint indentLevel)
        {
            if (att.IsComment)
            {
                yield return Environment.NewLine;
                var tmp = Indent(indentLevel);
                tmp.Append("/*");
                tmp.Append(objData);
                tmp.Append("*/");
                tmp.Append(Environment.NewLine);
                yield return tmp.ToString();
            }
            else if ((att.IsArgument || att.IsString) && !att.IsList)
            {
                var data = "";
                if (att.Comment != null)
                {
                    yield return Environment.NewLine;
                    var tmp = Indent(indentLevel);
                    tmp.Append("/*");
                    tmp.Append(att.Comment);
                    tmp.Append("*/ ");
                    yield return tmp.ToString();
                }

                if (att.Name != null && att.Name != "")
                {
                    data += Environment.NewLine;
                    data += Indent(indentLevel).Append(att.Name).Append(" ").ToString();
                }
                else if (att.ForceNewLine)
                {
                    data += Environment.NewLine;
                    data += Indent(indentLevel).ToString();
                }
                else if (att.Comment == null)
                {
                    data = " ";
                }

                if (att.IsString)
                {
                    var value = objData.ToString();
                    data += "\"" + value + "\"";
                    yield return data;
                }
                else
                {
                    if (objType.IsEnum)
                    {
                        var value = Enum.GetName(objType, objData);
                        data += value;
                        yield return data;
                    }
                    else if (att.CodeAsHex)
                    {
                        var tmp = (ulong) objData;
                        var value = "0x" + tmp.ToString("X");
                        data += value;
                        yield return data;
                    }
                    else if (objType == typeof(decimal))
                    {
                        var tmp = (decimal) objData;
                        data += tmp.ToString(CultureInfo.InvariantCulture);
                        yield return data;
                    }
                    else
                    {
                        yield return data + objData;
                    }
                }
            }
            else if (att.IsDictionary)
            {
                var dict = ToDict<string, object>(objData);

                if (dict.Count > 0)
                {
                    if (att.Comment != null)
                    {
                        yield return Environment.NewLine;
                        var tmp = Indent(indentLevel);
                        tmp.Append("/*");
                        tmp.Append(att.Comment);
                        tmp.Append("*/");
                        tmp.Append(Environment.NewLine);
                        yield return tmp.ToString();
                    }
                    else if (att.ForceNewLine)
                    {
                        yield return Environment.NewLine;
                    }

                    foreach (var elem in dict.Values)
                    foreach (var dicElement in SerialiseNode(elem, indentLevel))
                        yield return dicElement;
                }
            }
            else if (att.IsList)
            {
                if (objData is IList)
                {
                    var list = (IList) objData;
                    if (list.Count > 0)
                    {
                        if (att.Comment != null)
                        {
                            yield return Environment.NewLine;
                            var tmp = Indent(indentLevel);
                            tmp.Append("/*");
                            tmp.Append(att.Comment);
                            tmp.Append("*/");
                            yield return tmp.ToString();
                        }
                        else if (att.ForceNewLine)
                        {
                            yield return Environment.NewLine;
                        }

                        if (list[0].GetType().BaseType == typeof(Asap2Base))
                        {
                            /* If the is list is List<Asap2Base> sort the list and then iterate over the sorted list. */
                            IEnumerable<Asap2Base> tmp =
                                from Asap2Base item in list
                                orderby item.OrderID
                                select item;

                            foreach (var item in tmp)
                            foreach (var listElement in SerialiseNode(item, indentLevel, att))
                                yield return listElement;
                        }
                        else
                        {
                            /* Generic data elements. */
                            foreach (var item in list)
                            foreach (var listElement in SerialiseNode(item, indentLevel, att))
                                yield return listElement;
                        }
                    }
                }
            }
            else
            {
                if (objData != null)
                    foreach (var dataNode in SerialiseNode(objData, indentLevel))
                        yield return dataNode;
            }
        }

        private static Dictionary<TKey, TValue> ToDict<TKey, TValue>(object obj)
        {
            var stringDictionary = obj as Dictionary<TKey, TValue>;

            if (stringDictionary != null) return stringDictionary;
            var baseDictionary = obj as IDictionary;

            if (baseDictionary != null)
            {
                var dictionary = new Dictionary<TKey, TValue>();
                foreach (DictionaryEntry keyValue in baseDictionary)
                {
                    if (!(keyValue.Value is TValue))
                        // value is not TKey. perhaps throw an exception
                        return null;
                    if (!(keyValue.Key is TKey))
                        // value is not TValue. perhaps throw an exception
                        return null;

                    dictionary.Add((TKey) keyValue.Key, (TValue) keyValue.Value);
                }

                return dictionary;
            }

            // object is not a dictionary. perhaps throw an exception
            return null;
        }

        private static class SortedFieldsCache
        {
            private static readonly Dictionary<Type, FieldInfo[]> Value;

            static SortedFieldsCache()
            {
                Value = new Dictionary<Type, FieldInfo[]>();
            }

            public static FieldInfo[] Get(Type x)
            {
                FieldInfo[] v;
                if (Value.TryGetValue(x, out v))
                    return v;

                v = x.GetFields().OrderBy(f =>
                {
                    var data = AttributeCache<ElementAttribute, MemberInfo>.Get(f);
                    if (data == null)
                        return (uint) 999999; /* sort it last */
                    return data.SortOrder;
                }).ToArray();
                Value.Add(x, v);
                return v;
            }
        }

        private static class SortedPropertyCache
        {
            private static readonly Dictionary<Type, PropertyInfo[]> Value;

            static SortedPropertyCache()
            {
                Value = new Dictionary<Type, PropertyInfo[]>();
            }

            public static PropertyInfo[] Get(Type x)
            {
                PropertyInfo[] v;
                if (Value.TryGetValue(x, out v))
                    return v;

                v = x.GetProperties().OrderBy(f =>
                {
                    var data = AttributeCache<ElementAttribute, MemberInfo>.Get(f);
                    if (data == null)
                        return (uint) 999999; /* sort it last */
                    return data.SortOrder;
                }).ToArray();
                Value.Add(x, v);
                return v;
            }
        }

        private static class AttributeCache<T, L>
            where T : class
            where L : MemberInfo
        {
            public static readonly Dictionary<L, T> Value;

            static AttributeCache()
            {
                Value = new Dictionary<L, T>();
            }

            public static T Get(L x)
            {
                T v;
                if (Value.TryGetValue(x, out v))
                    return v;

                v = Attribute.GetCustomAttribute(x, typeof(T)) as T;
                Value.Add(x, v);
                return v;
            }
        }
    }
}