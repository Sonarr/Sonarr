//http://fastjson.codeplex.com/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using Exceptron.Driver.fastJSON;

namespace Exceptron.Driver.fastJSON
{
    internal delegate string Serialize(object data);
    internal delegate object Deserialize(string data);

    internal class JSON
    {
        public readonly static JSON Instance = new JSON();

        private JSON()
        {
        }
        public bool UseOptimizedDatasetSchema = true;
        public bool UseFastGuid = true;
        public bool UseSerializerExtension = true;
        public bool IndentOutput = false;
        public bool SerializeNullValues = true;
        public bool UseUTCDateTime = false;
        public bool ShowReadOnlyProperties = false;
        public bool UsingGlobalTypes = true;

        public string ToJSON(object obj)
        {
            return ToJSON(obj, UseSerializerExtension, UseFastGuid, UseOptimizedDatasetSchema, SerializeNullValues);
        }

        public string ToJSON(object obj,
                             bool enableSerializerExtensions)
        {
            return ToJSON(obj, enableSerializerExtensions, UseFastGuid, UseOptimizedDatasetSchema, SerializeNullValues);
        }

        public string ToJSON(object obj,
                             bool enableSerializerExtensions,
                             bool enableFastGuid)
        {
            return ToJSON(obj, enableSerializerExtensions, enableFastGuid, UseOptimizedDatasetSchema, SerializeNullValues);
        }

        public string ToJSON(object obj,
                             bool enableSerializerExtensions,
                             bool enableFastGuid,
                             bool enableOptimizedDatasetSchema,
                             bool serializeNullValues)
        {
            return new JSONSerializer(enableOptimizedDatasetSchema, enableFastGuid, enableSerializerExtensions, serializeNullValues, IndentOutput).ConvertToJSON(obj);
        }

        public object Parse(string json)
        {
            return new JsonParser(json).Decode();
        }

        public T ToObject<T>(string json)
        {
            return (T)ToObject(json, typeof(T));
        }

        public object ToObject(string json)
        {
            return ToObject(json, null);
        }

        public object ToObject(string json, Type type)
        {
            Dictionary<string, object> ht = new JsonParser(json).Decode() as Dictionary<string, object>;
            if (ht == null) return null;
            return ParseDictionary(ht, null, type);
        }

#if CUSTOMTYPE
        internal SafeDictionary<Type, Serialize> _customSerializer = new SafeDictionary<Type, Serialize>();
        internal SafeDictionary<Type, Deserialize> _customDeserializer = new SafeDictionary<Type, Deserialize>();

        public void RegisterCustomType(Type type, Serialize serializer, Deserialize deserializer)
        {
            if (type != null && serializer != null && deserializer != null)
            {
                _customSerializer.Add(type, serializer);
                _customDeserializer.Add(type, deserializer);
                // reset property cache
                _propertycache = new SafeDictionary<string, SafeDictionary<string, myPropInfo>>();
            }
        }

        internal bool IsTypeRegistered(Type t)
        {
            Serialize s;
            return _customSerializer.TryGetValue(t, out s);
        }
#endif

        #region [   PROPERTY GET SET CACHE   ]
        SafeDictionary<Type, string> _tyname = new SafeDictionary<Type, string>();
        internal string GetTypeAssemblyName(Type t)
        {
            string val = "";
            if (_tyname.TryGetValue(t, out val))
                return val;
            else
            {
                string s = t.AssemblyQualifiedName;
                _tyname.Add(t, s);
                return s;
            }
        }

        SafeDictionary<string, Type> _typecache = new SafeDictionary<string, Type>();
        private Type GetTypeFromCache(string typename)
        {
            Type val = null;
            if (_typecache.TryGetValue(typename, out val))
                return val;
            else
            {
                Type t = Type.GetType(typename);
                _typecache.Add(typename, t);
                return t;
            }
        }

        SafeDictionary<Type, CreateObject> _constrcache = new SafeDictionary<Type, CreateObject>();
        private delegate object CreateObject();
        private object FastCreateInstance(Type objtype)
        {
            try
            {
                CreateObject c = null;
                if (_constrcache.TryGetValue(objtype, out c))
                {
                    return c();
                }
                else
                {
                    DynamicMethod dynMethod = new DynamicMethod("_", objtype, null);
                    ILGenerator ilGen = dynMethod.GetILGenerator();

                    ilGen.Emit(OpCodes.Newobj, objtype.GetConstructor(Type.EmptyTypes));
                    ilGen.Emit(OpCodes.Ret);
                    c = (CreateObject)dynMethod.CreateDelegate(typeof(CreateObject));
                    _constrcache.Add(objtype, c);
                    return c();
                }
            }
            catch (Exception exc)
            {
                throw new Exception(string.Format("Failed to fast create instance for type '{0}' from assemebly '{1}'",
                    objtype.FullName, objtype.AssemblyQualifiedName), exc);
            }
        }

        private struct myPropInfo
        {
            public bool filled;
            public Type pt;
            public Type bt;
            public Type changeType;
            public bool isDictionary;
            public bool isValueType;
            public bool isGenericType;
            public bool isArray;
            public bool isByteArray;
            public bool isGuid;
#if !SILVERLIGHT
            public bool isDataSet;
            public bool isDataTable;
            public bool isHashtable;
#endif
            public GenericSetter setter;
            public bool isEnum;
            public bool isDateTime;
            public Type[] GenericTypes;
            public bool isInt;
            public bool isLong;
            public bool isString;
            public bool isBool;
            public bool isClass;
            public GenericGetter getter;
            public bool isStringDictionary;
            public string Name;
#if CUSTOMTYPE
            public bool isCustomType;
#endif
            public bool CanWrite;
        }

        SafeDictionary<string, SafeDictionary<string, myPropInfo>> _propertycache = new SafeDictionary<string, SafeDictionary<string, myPropInfo>>();
        private SafeDictionary<string, myPropInfo> Getproperties(Type type, string typename)
        {
            SafeDictionary<string, myPropInfo> sd = null;
            if (_propertycache.TryGetValue(typename, out sd))
            {
                return sd;
            }
            else
            {
                sd = new SafeDictionary<string, myPropInfo>();
                PropertyInfo[] pr = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (PropertyInfo p in pr)
                {
                    myPropInfo d = CreateMyProp(p.PropertyType, p.Name);
                    d.CanWrite = p.CanWrite;
                    d.setter = CreateSetMethod(p);
                    d.getter = CreateGetMethod(p);
                    sd.Add(p.Name, d);
                }
                FieldInfo[] fi = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (FieldInfo f in fi)
                {
                    myPropInfo d = CreateMyProp(f.FieldType, f.Name);
                    d.setter = CreateSetField(type, f);
                    d.getter = CreateGetField(type, f);
                    sd.Add(f.Name, d);
                }

                _propertycache.Add(typename, sd);
                return sd;
            }
        }

        private myPropInfo CreateMyProp(Type t, string name)
        {
            myPropInfo d = new myPropInfo();
            d.filled = true;
            d.CanWrite = true;
            d.pt = t;
            d.Name = name;
            d.isDictionary = t.Name.Contains("Dictionary");
            if (d.isDictionary)
                d.GenericTypes = t.GetGenericArguments();
            d.isValueType = t.IsValueType;
            d.isGenericType = t.IsGenericType;
            d.isArray = t.IsArray;
            if (d.isArray)
                d.bt = t.GetElementType();
            if (d.isGenericType)
                d.bt = t.GetGenericArguments()[0];
            d.isByteArray = t == typeof(byte[]);
            d.isGuid = (t == typeof(Guid) || t == typeof(Guid?));
#if !SILVERLIGHT
            d.isHashtable = t == typeof(Hashtable);
            d.isDataSet = t == typeof(DataSet);
            d.isDataTable = t == typeof(DataTable);
#endif

            d.changeType = GetChangeType(t);
            d.isEnum = t.IsEnum;
            d.isDateTime = t == typeof(DateTime) || t == typeof(DateTime?);
            d.isInt = t == typeof(int) || t == typeof(int?);
            d.isLong = t == typeof(long) || t == typeof(long?);
            d.isString = t == typeof(string);
            d.isBool = t == typeof(bool) || t == typeof(bool?);
            d.isClass = t.IsClass;

            if (d.isDictionary && d.GenericTypes.Length>0 && d.GenericTypes[0] == typeof(string))
                d.isStringDictionary = true;

#if CUSTOMTYPE
            if (IsTypeRegistered(t))
                d.isCustomType = true;
#endif
            return d;
        }

        private delegate void GenericSetter(object target, object value);

        private static GenericSetter CreateSetMethod(PropertyInfo propertyInfo)
        {
            MethodInfo setMethod = propertyInfo.GetSetMethod();
            if (setMethod == null)
                return null;

            Type[] arguments = new Type[2];
            arguments[0] = arguments[1] = typeof(object);

            DynamicMethod setter = new DynamicMethod("_", typeof(void), arguments);
            ILGenerator il = setter.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
            il.Emit(OpCodes.Ldarg_1);

            if (propertyInfo.PropertyType.IsClass)
                il.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
            else
                il.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);

            il.EmitCall(OpCodes.Callvirt, setMethod, null);
            il.Emit(OpCodes.Ret);

            return (GenericSetter)setter.CreateDelegate(typeof(GenericSetter));
        }

        internal delegate object GenericGetter(object obj);

        private static GenericGetter CreateGetField(Type type, FieldInfo fieldInfo)
        {
            DynamicMethod dynamicGet = new DynamicMethod("_", typeof(object), new Type[] { typeof(object) }, type, true);
            ILGenerator il = dynamicGet.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, fieldInfo);
            if (fieldInfo.FieldType.IsValueType)
                il.Emit(OpCodes.Box, fieldInfo.FieldType);
            il.Emit(OpCodes.Ret);

            return (GenericGetter)dynamicGet.CreateDelegate(typeof(GenericGetter));
        }

        private static GenericSetter CreateSetField(Type type, FieldInfo fieldInfo)
        {
            Type[] arguments = new Type[2];
            arguments[0] = arguments[1] = typeof(object);

            DynamicMethod dynamicSet = new DynamicMethod("_", typeof(void), arguments, type, true);
            ILGenerator il = dynamicSet.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            if (fieldInfo.FieldType.IsValueType)
                il.Emit(OpCodes.Unbox_Any, fieldInfo.FieldType);
            il.Emit(OpCodes.Stfld, fieldInfo);
            il.Emit(OpCodes.Ret);

            return (GenericSetter)dynamicSet.CreateDelegate(typeof(GenericSetter));
        }

        private GenericGetter CreateGetMethod(PropertyInfo propertyInfo)
        {
            MethodInfo getMethod = propertyInfo.GetGetMethod();
            if (getMethod == null)
                return null;

            Type[] arguments = new Type[1];
            arguments[0] = typeof(object);

            DynamicMethod getter = new DynamicMethod("_", typeof(object), arguments);
            ILGenerator il = getter.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
            il.EmitCall(OpCodes.Callvirt, getMethod, null);

            if (!propertyInfo.PropertyType.IsClass)
                il.Emit(OpCodes.Box, propertyInfo.PropertyType);

            il.Emit(OpCodes.Ret);

            return (GenericGetter)getter.CreateDelegate(typeof(GenericGetter));
        }

        readonly SafeDictionary<Type, List<Getters>> _getterscache = new SafeDictionary<Type, List<Getters>>();
        internal List<Getters> GetGetters(Type type)
        {
            List<Getters> val = null;
            if (_getterscache.TryGetValue(type, out val))
                return val;

            PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            List<Getters> getters = new List<Getters>();
            foreach (PropertyInfo p in props)
            {
                if (!p.CanWrite && ShowReadOnlyProperties == false) continue;

                object[] att = p.GetCustomAttributes(typeof(System.Xml.Serialization.XmlIgnoreAttribute), false);
                if (att != null && att.Length > 0)
                    continue;

                JSON.GenericGetter g = CreateGetMethod(p);
                if (g != null)
                {
                    Getters gg = new Getters();
                    gg.Name = p.Name;
                    gg.Getter = g;
                    gg.propertyType = p.PropertyType;
                    getters.Add(gg);
                }
            }

            FieldInfo[] fi = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (var f in fi)
            {
                object[] att = f.GetCustomAttributes(typeof(System.Xml.Serialization.XmlIgnoreAttribute), false);
                if (att != null && att.Length > 0)
                    continue;

                JSON.GenericGetter g = CreateGetField(type, f);
                if (g != null)
                {
                    Getters gg = new Getters();
                    gg.Name = f.Name;
                    gg.Getter = g;
                    gg.propertyType = f.FieldType;
                    getters.Add(gg);
                }
            }

            _getterscache.Add(type, getters);
            return getters;
        }

        private object ChangeType(object value, Type conversionType)
        {
            if (conversionType == typeof(int))
                return (int)CreateLong((string)value);

            else if (conversionType == typeof(long))
                return CreateLong((string)value);

            else if (conversionType == typeof(string))
                return (string)value;

            else if (conversionType == typeof(Guid))
                return CreateGuid((string)value);

            else if (conversionType.IsEnum)
                return CreateEnum(conversionType, (string)value);

            return Convert.ChangeType(value, conversionType, CultureInfo.InvariantCulture);
        }
        #endregion


        private object ParseDictionary(Dictionary<string, object> d, Dictionary<string, object> globaltypes, Type type)
        {
            object tn = "";
            if (d.TryGetValue("$types", out tn))
            {
                UsingGlobalTypes = true;
                globaltypes = new Dictionary<string, object>();
                foreach (var kv in (Dictionary<string, object>)tn)
                {
                    globaltypes.Add((string)kv.Value, kv.Key);
                }
            }

            bool found = d.TryGetValue("$type", out tn);
#if !SILVERLIGHT
            if (found == false && type == typeof(System.Object))
            {
                return CreateDataset(d, globaltypes);
            }
#endif
            if (found)
            {
                if (UsingGlobalTypes)
                {
                    object tname = "";
                    if (globaltypes.TryGetValue((string)tn, out tname))
                        tn = tname;
                }
                type = GetTypeFromCache((string)tn);
            }

            if (type == null)
                throw new Exception("Cannot determine type");

            string typename = type.FullName;
            object o = FastCreateInstance(type);
            SafeDictionary<string, myPropInfo> props = Getproperties(type, typename);
            foreach (string name in d.Keys)
            {
                if (name == "$map")
                {
                    ProcessMap(o, props, (Dictionary<string, object>)d[name]);
                    continue;
                }
                myPropInfo pi;
                if (props.TryGetValue(name, out pi) == false)
                    continue;
                if (pi.filled == true)
                {
                    object v = d[name];

                    if (v != null)
                    {
                        object oset = null;

                        if (pi.isInt)
                            oset = (int)CreateLong((string)v);
#if CUSTOMTYPE
                        else if (pi.isCustomType)
                            oset = CreateCustom((string)v, pi.pt);
#endif
                        else if (pi.isLong)
                            oset = CreateLong((string)v);

                        else if (pi.isString)
                            oset = (string)v;

                        else if (pi.isBool)
                            oset = (bool)v;

                        else if (pi.isGenericType && pi.isValueType == false && pi.isDictionary == false)
#if SILVERLIGHT
                            oset = CreateGenericList((List<object>)v, pi.pt, pi.bt, globaltypes);
#else
                            oset = CreateGenericList((ArrayList)v, pi.pt, pi.bt, globaltypes);
#endif
                        else if (pi.isByteArray)
                            oset = Convert.FromBase64String((string)v);

                        else if (pi.isArray && pi.isValueType == false)
#if SILVERLIGHT
                            oset = CreateArray((List<object>)v, pi.pt, pi.bt, globaltypes);
#else
                            oset = CreateArray((ArrayList)v, pi.pt, pi.bt, globaltypes);
#endif
                        else if (pi.isGuid)
                            oset = CreateGuid((string)v);
#if !SILVERLIGHT
                        else if (pi.isDataSet)
                            oset = CreateDataset((Dictionary<string, object>)v, globaltypes);

                        else if (pi.isDataTable)
                            oset = this.CreateDataTable((Dictionary<string, object>)v, globaltypes);
#endif

                        else if (pi.isStringDictionary)
                            oset = CreateStringKeyDictionary((Dictionary<string, object>)v, pi.pt, pi.GenericTypes, globaltypes);

#if !SILVERLIGHT
                        else if (pi.isDictionary || pi.isHashtable)
                            oset = CreateDictionary((ArrayList)v, pi.pt, pi.GenericTypes, globaltypes);
#else 
                        else if (pi.isDictionary)
                            oset = CreateDictionary((List<object>)v, pi.pt, pi.GenericTypes, globaltypes);
#endif

                        else if (pi.isEnum)
                            oset = CreateEnum(pi.pt, (string)v);

                        else if (pi.isDateTime)
                            oset = CreateDateTime((string)v);

                        else if (pi.isClass && v is Dictionary<string, object>)
                            oset = ParseDictionary((Dictionary<string, object>)v, globaltypes, pi.pt);

                        else if (pi.isValueType)
                            oset = ChangeType(v, pi.changeType);

#if SILVERLIGHT
                        else if (v is List<object>)
                            oset = CreateArray((List<object>)v, pi.pt, typeof(object), globaltypes);
#else
                        else if (v is ArrayList)
                            oset = CreateArray((ArrayList)v, pi.pt, typeof(object), globaltypes);
#endif
                        else
                            oset = v;

                        if (pi.CanWrite)
                            pi.setter(o, oset);
                    }
                }
            }
            return o;
        }

#if CUSTOMTYPE
        private object CreateCustom(string v, Type type)
        {
            Deserialize d;
            _customDeserializer.TryGetValue(type, out d);
            return d(v);
        }
#endif

        private void ProcessMap(object obj, SafeDictionary<string, JSON.myPropInfo> props, Dictionary<string, object> dic)
        {
            foreach (KeyValuePair<string, object> kv in dic)
            {
                myPropInfo p = props[kv.Key];
                object o = p.getter(obj);
                Type t = Type.GetType((string)kv.Value);
                if (t == typeof(Guid))
                    p.setter(obj, CreateGuid((string)o));
            }
        }

        private long CreateLong(string s)
        {
            long num = 0;
            bool neg = false;
            foreach (char cc in s)
            {
                if (cc == '-')
                    neg = true;
                else if (cc == '+')
                    neg = false;
                else
                {
                    num *= 10;
                    num += (int)(cc - '0');
                }
            }

            return neg ? -num : num;
        }

        private object CreateEnum(Type pt, string v)
        {
            // TODO : optimize create enum
#if !SILVERLIGHT
            return Enum.Parse(pt, v);
#else
            return Enum.Parse(pt, v, true);
#endif
        }

        private Guid CreateGuid(string s)
        {
            if (s.Length > 30)
                return new Guid(s);
            else
                return new Guid(Convert.FromBase64String(s));
        }

        private DateTime CreateDateTime(string value)
        {
            bool utc = false;
            //                   0123456789012345678
            // datetime format = yyyy-MM-dd HH:mm:ss
            int year = (int)CreateLong(value.Substring(0, 4));
            int month = (int)CreateLong(value.Substring(5, 2));
            int day = (int)CreateLong(value.Substring(8, 2));
            int hour = (int)CreateLong(value.Substring(11, 2));
            int min = (int)CreateLong(value.Substring(14, 2));
            int sec = (int)CreateLong(value.Substring(17, 2));

            if (value.EndsWith("Z"))
                utc = true;

            if (UseUTCDateTime == false && utc == false)
                return new DateTime(year, month, day, hour, min, sec);
            else
                return new DateTime(year, month, day, hour, min, sec, DateTimeKind.Utc).ToLocalTime();
        }

#if SILVERLIGHT
        private object CreateArray(List<object> data, Type pt, Type bt, Dictionary<string, object> globalTypes)
        {
            Array col = Array.CreateInstance(bt, data.Count);
            // create an array of objects
            for (int i = 0; i < data.Count; i++)// each (object ob in data)
            {
                object ob = data[i];
                if (ob is IDictionary)
                    col.SetValue(ParseDictionary((Dictionary<string, object>)ob, globalTypes, bt), i);
                else
                    col.SetValue(ChangeType(ob, bt), i);
            }

            return col;
        }
#else
        private object CreateArray(ArrayList data, Type pt, Type bt, Dictionary<string, object> globalTypes)
        {
            ArrayList col = new ArrayList();
            // create an array of objects
            foreach (object ob in data)
            {
                if (ob is IDictionary)
                    col.Add(ParseDictionary((Dictionary<string, object>)ob, globalTypes, bt));
                else
                    col.Add(ChangeType(ob, bt));
            }
            return col.ToArray(bt);
        }
#endif


#if SILVERLIGHT
        private object CreateGenericList(List<object> data, Type pt, Type bt, Dictionary<string, object> globalTypes)
#else
        private object CreateGenericList(ArrayList data, Type pt, Type bt, Dictionary<string, object> globalTypes)
#endif
        {
            IList col = (IList)FastCreateInstance(pt);
            // create an array of objects
            foreach (object ob in data)
            {
                if (ob is IDictionary)
                    col.Add(ParseDictionary((Dictionary<string, object>)ob, globalTypes, bt));
#if SILVERLIGHT
                else if (ob is List<object>)
                    col.Add(((List<object>)ob).ToArray());
#else
                else if (ob is ArrayList)
                    col.Add(((ArrayList)ob).ToArray());
#endif
                else
                    col.Add(ChangeType(ob, bt));
            }
            return col;
        }

        private object CreateStringKeyDictionary(Dictionary<string, object> reader, Type pt, Type[] types, Dictionary<string,object> globalTypes)
        {
            var col = (IDictionary)FastCreateInstance(pt);
            Type t1 = null;
            Type t2 = null;
            if (types != null)
            {
                t1 = types[0];
                t2 = types[1];
            }

            foreach (KeyValuePair<string, object> values in reader)
            {
                var key = values.Key;//ChangeType(values.Key, t1);
                object val = null;
                if (values.Value is Dictionary<string, object>)
                    val = ParseDictionary((Dictionary<string, object>)values.Value, globalTypes, t2);
                else
                    val = ChangeType(values.Value, t2);
                col.Add(key, val);
            }

            return col;
        }

#if SILVERLIGHT
        private object CreateDictionary(List<object> reader, Type pt, Type[] types, Dictionary<string, object> globalTypes)
#else
        private object CreateDictionary(ArrayList reader, Type pt, Type[] types, Dictionary<string, object> globalTypes)
#endif
        {
            IDictionary col = (IDictionary)FastCreateInstance(pt);
            Type t1 = null;
            Type t2 = null;
            if (types != null)
            {
                t1 = types[0];
                t2 = types[1];
            }

            foreach (Dictionary<string, object> values in reader)
            {
                object key = values["k"];
                object val = values["v"];

                if (key is Dictionary<string, object>)
                    key = ParseDictionary((Dictionary<string, object>)key, globalTypes, t1);
                else
                    key = ChangeType(key, t1);

                if (val is Dictionary<string, object>)
                    val = ParseDictionary((Dictionary<string, object>)val, globalTypes, t2);
                else
                    val = ChangeType(val, t2);

                col.Add(key, val);
            }

            return col;
        }

        private Type GetChangeType(Type conversionType)
        {
            if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                return conversionType.GetGenericArguments()[0];

            return conversionType;
        }
#if !SILVERLIGHT
        private DataSet CreateDataset(Dictionary<string, object> reader, Dictionary<string, object> globalTypes)
        {
            DataSet ds = new DataSet();
            ds.EnforceConstraints = false;
            ds.BeginInit();

            // read dataset schema here
            ReadSchema(reader, ds, globalTypes);

            foreach (KeyValuePair<string, object> pair in reader)
            {
                if (pair.Key == "$type" || pair.Key == "$schema") continue;

                ArrayList rows = (ArrayList)pair.Value;
                if (rows == null) continue;

                DataTable dt = ds.Tables[pair.Key];
                ReadDataTable(rows, dt);
            }

            ds.EndInit();

            return ds;
        }

        private void ReadSchema(Dictionary<string, object> reader, DataSet ds, Dictionary<string, object> globalTypes)
        {
            var schema = reader["$schema"];

            if (schema is string)
            {
                TextReader tr = new StringReader((string)schema);
                ds.ReadXmlSchema(tr);
            }
            else
            {
                DatasetSchema ms = (DatasetSchema)ParseDictionary((Dictionary<string, object>)schema, globalTypes, typeof(DatasetSchema));
                ds.DataSetName = ms.Name;
                for (int i = 0; i < ms.Info.Count; i += 3)
                {
                    if (ds.Tables.Contains(ms.Info[i]) == false)
                        ds.Tables.Add(ms.Info[i]);
                    ds.Tables[ms.Info[i]].Columns.Add(ms.Info[i + 1], Type.GetType(ms.Info[i + 2]));
                }
            }
        }

        private void ReadDataTable(ArrayList rows, DataTable dt)
        {
            dt.BeginInit();
            dt.BeginLoadData();
            List<int> guidcols = new List<int>();
            List<int> datecol = new List<int>();

            foreach (DataColumn c in dt.Columns)
            {
                if (c.DataType == typeof(Guid) || c.DataType == typeof(Guid?))
                    guidcols.Add(c.Ordinal);
                if (UseUTCDateTime && (c.DataType == typeof(DateTime) || c.DataType == typeof(DateTime?)))
                    datecol.Add(c.Ordinal);
            }

            foreach (ArrayList row in rows)
            {
                object[] v = new object[row.Count];
                row.CopyTo(v, 0);
                foreach (int i in guidcols)
                {
                    string s = (string)v[i];
                    if (s != null && s.Length < 36)
                        v[i] = new Guid(Convert.FromBase64String(s));
                }
                if (UseUTCDateTime)
                {
                    foreach (int i in datecol)
                    {
                        string s = (string)v[i];
                        if (s != null)
                            v[i] = CreateDateTime(s);
                    }
                }
                dt.Rows.Add(v);
            }

            dt.EndLoadData();
            dt.EndInit();
        }

        DataTable CreateDataTable(Dictionary<string, object> reader, Dictionary<string, object> globalTypes)
        {
            var dt = new DataTable();

            // read dataset schema here
            var schema = reader["$schema"];

            if (schema is string)
            {
                TextReader tr = new StringReader((string)schema);
                dt.ReadXmlSchema(tr);
            }
            else
            {
                var ms = (DatasetSchema)this.ParseDictionary((Dictionary<string, object>)schema, globalTypes, typeof(DatasetSchema));
                dt.TableName = ms.Info[0];
                for (int i = 0; i < ms.Info.Count; i += 3)
                {
                    dt.Columns.Add(ms.Info[i + 1], Type.GetType(ms.Info[i + 2]));
                }
            }

            foreach (var pair in reader)
            {
                if (pair.Key == "$type" || pair.Key == "$schema")
                    continue;

                var rows = (ArrayList)pair.Value;
                if (rows == null)
                    continue;

                if (!dt.TableName.Equals(pair.Key, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                ReadDataTable(rows, dt);
            }

            return dt;
        }
#endif
    }
}