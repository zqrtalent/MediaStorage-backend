using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MediaStorage.Common.Serialization.MfcSerialize
{
    // Variable type definitions.
    enum VariableType
    {
        Bool = 0,
        Int8,
        Int16,
        Int32,
        Int64,
        String,
        DateTime,
        Date,
        Time,
        Float,
        Double,
        Binary,
        Rect,
        Point,
        Size,
        Image,
        Font,
        Color32,
        Enumerable,
        UInt64,
        VoidPtr,
        Chips,
        None = 100,
        Array
    };

    public static class MFCSerializable
    {
        public delegate bool WriteSerializedInBuffer_Delegate(object objPropertyOwner, PropertyInfo property, ref BinaryWriter buffer);
        public delegate bool ReadFromSerializedBuffer_Delegate(object objPropertyOwner, PropertyInfo property, ref BinaryReader buffer);

        private static readonly Dictionary<string, WriteSerializedInBuffer_Delegate> _writeInBufferDelegatesByType = new Dictionary<string, WriteSerializedInBuffer_Delegate>()
        {
                {typeof(byte).Name, new WriteSerializedInBuffer_Delegate(WriteSerializedBuffer_Int8) },
                {typeof(short).Name, new WriteSerializedInBuffer_Delegate(WriteSerializedBuffer_Int16) },
                {typeof(int).Name, new WriteSerializedInBuffer_Delegate(WriteSerializedBuffer_Int32) },
                {typeof(long).Name, new WriteSerializedInBuffer_Delegate(WriteSerializedBuffer_Int64) },
                {typeof(UInt64).Name, new WriteSerializedInBuffer_Delegate(WriteSerializedBuffer_UInt64) },
                {typeof(bool).Name, new WriteSerializedInBuffer_Delegate(WriteSerializedBuffer_Bool) },
                {typeof(double).Name, new WriteSerializedInBuffer_Delegate(WriteSerializedBuffer_Double) },
                {typeof(float).Name, new WriteSerializedInBuffer_Delegate(WriteSerializedBuffer_Float) },
                {typeof(DateTime).Name, new WriteSerializedInBuffer_Delegate(WriteSerializedBuffer_DateTime) },
                {typeof(string).Name, new WriteSerializedInBuffer_Delegate(WriteSerializedBuffer_String) },
                {typeof(decimal).Name, new WriteSerializedInBuffer_Delegate(WriteSerializedBuffer_Chips) },
                {typeof(byte[]).Name, new WriteSerializedInBuffer_Delegate(WriteSerializedBuffer_Binary) },
        };

        private static readonly Dictionary<string, ReadFromSerializedBuffer_Delegate> _readFromSerializedBufferDelegatesByType = new Dictionary<string, ReadFromSerializedBuffer_Delegate>()
        {
                {typeof(byte).Name, new ReadFromSerializedBuffer_Delegate(ReadFromSerializedBuffer_Int8) },
                {typeof(short).Name, new ReadFromSerializedBuffer_Delegate(ReadFromSerializedBuffer_Int16) },
                {typeof(int).Name, new ReadFromSerializedBuffer_Delegate(ReadFromSerializedBuffer_Int32) },
                {typeof(long).Name, new ReadFromSerializedBuffer_Delegate(ReadFromSerializedBuffer_Int64) },
                {typeof(UInt64).Name, new ReadFromSerializedBuffer_Delegate(ReadFromSerializedBuffer_UInt64) },
                {typeof(bool).Name, new ReadFromSerializedBuffer_Delegate(ReadFromSerializedBuffer_Bool) },
                {typeof(double).Name, new ReadFromSerializedBuffer_Delegate(ReadFromSerializedBuffer_Double) },
                {typeof(float).Name, new ReadFromSerializedBuffer_Delegate(ReadFromSerializedBuffer_Float) },
                {typeof(DateTime).Name, new ReadFromSerializedBuffer_Delegate(ReadFromSerializedBuffer_DateTime) },
                {typeof(string).Name, new ReadFromSerializedBuffer_Delegate(ReadFromSerializedBuffer_String) },
                {typeof(decimal).Name, new ReadFromSerializedBuffer_Delegate(ReadFromSerializedBuffer_Chips) },
                {typeof(byte[]).Name, new ReadFromSerializedBuffer_Delegate(ReadFromSerializedBuffer_Binary) },
        };

        public static object GetPropertyValue(object obj, Type objType, string propertyName)
        {
            MemberInfo[] arrProperties = objType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < arrProperties.Length; i++)
            {
                PropertyInfo p = (PropertyInfo)arrProperties[i];
                Type propType = p.PropertyType;
                if (p.Name == propertyName)
                {
                    if (propType.GetTypeInfo().IsClass || !p.CanRead)
                        return null;
                    //object v = p.GetValue(obj);
                    return p.GetValue(obj);
                }
            }

            return null;
        }

        public static IList<Type> GetBaseTypes(this object thisObject)
        {
            IList<Type> listTypes = new List<Type>();
            Type t = thisObject.GetType();
            while (t != null)
            {
                //if (t == typeof(MFCSerializable))
                if (t == typeof(object))
                      break;
                listTypes.Insert(0, t);
                t = t.GetTypeInfo().BaseType;
            }
            return listTypes;
        }

        public static bool Serialize(this object thisObject, ref BinaryWriter buffer)
        {
            IList<Type> listTypes = thisObject.GetBaseTypes();
            // Skip properties of base class which are included in derived class properties.
            int propertiesSkip = 0;
            foreach (Type type in listTypes)
            {
                MemberInfo[] arrProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                for (int i = 0; i < (arrProperties.Length - propertiesSkip); i++)
                {
                    PropertyInfo p = (PropertyInfo)arrProperties[i];
                    Type propType = p.PropertyType;
                    TypeInfo propTypeInfo = propType.GetTypeInfo();
                    
                    if (propTypeInfo.IsGenericType) // Performance Issue???
                    {
                        IEnumerable<object> listObjects = (IEnumerable<object>)p.GetValue(thisObject);
                        if (listObjects == null || !listObjects.Any())
                        {
                            // Write variable type.
                            buffer.Write((byte)VariableType.Array);
                            // Write item count.
                            buffer.Write(0);
                        }
                        else
                        {
                            int ct = listObjects.Count();
                            if (ct == 0)
                            {
                                // Write variable type with defaul behaviuor.
                                buffer.Write((((byte)VariableType.Array) & 0x80));
                            }
                            else
                            {
                                // Write variable type.
                                buffer.Write((byte)VariableType.Array);
                                // Write item count.
                                buffer.Write(ct);
                            }

                            foreach (var obj in listObjects)
                            {
                                if (obj == null)
                                {
                                    // Write null element.
                                    buffer.Write((byte)VariableType.None);
                                }
                                else
                                    if (!obj.Serialize(ref buffer))
                                        return false;
                            }
                        }

                        continue;
                    }

                    if (_writeInBufferDelegatesByType.ContainsKey(propType.Name))
                    {
                        WriteSerializedInBuffer_Delegate method = _writeInBufferDelegatesByType[propType.Name];
                        if (method != null)
                        {
                            if (!method.Invoke(thisObject, p, ref buffer))
                                return false;
                        }
                        else
                            return false;
                    }
                    else
                    {
                        if (propTypeInfo.IsClass)
                        {
                            var obj = p.GetValue(thisObject);
                            if (obj == null || !obj.Serialize(ref buffer))
                                return false;
                        }
                        else
                            return false;
                    }
                }

                propertiesSkip = arrProperties.Length;
            }

            return true;
        }

        public static bool Deserialize(this object thisObject, byte[] buffer, int index, int count)
        {
            bool ret = false;
            using (var mem = new MemoryStream(buffer, index, count))
            {
                BinaryReader br = new BinaryReader(mem);
                ret = thisObject.Deserialize(br);
                br.Dispose();
                mem.Dispose();
            }
            return ret;
        }
        public static bool Deserialize(this object thisObject, BinaryReader buffer)
        {
            IList<Type> listTypes = thisObject.GetBaseTypes();

            // Skip properties of base class which are included in derived class properties.
            int propertiesSkip = 0;

            foreach (Type type in listTypes)
            {
                MemberInfo[] arrProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                for (int i = 0; i < (arrProperties.Length - propertiesSkip); i++)
                {
                    PropertyInfo p = (PropertyInfo)arrProperties[i];
                    Type propType = p.PropertyType;
                    TypeInfo propTypeInfo = propType.GetTypeInfo();

                    if (propTypeInfo.IsGenericType)
                    {
                        Type genericTypeArg = propType.GenericTypeArguments[0];
                        IList listObjects = null;
                        List<string> ff = (List<string>)Activator.CreateInstance(typeof(List<>).MakeGenericType(typeof(string)));
                        if (propTypeInfo.IsClass)
                            listObjects = (IList)Activator.CreateInstance(propType);
                        else
                            listObjects = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(propType.GenericTypeArguments));

                        byte cType = buffer.ReadByte(); // Read variable type.
                        if ((cType & 0x7f) != (byte)VariableType.Array)
                            return false;
                        int nLen = (cType & 0x80) == 0 ? buffer.ReadInt32() : 0; // Read array length.

                        for (int j = 0; j < nLen; j++)
                        {
                            // Check for null pointer.
                            if (buffer.PeekChar() == (byte)VariableType.None)
                            {
                                listObjects.Add(null);
                                buffer.ReadByte(); // Skip null pointer indicator byte.
                            }
                            else
                            {
                                object objNew = Activator.CreateInstance(genericTypeArg);
                                if (!objNew.Deserialize(buffer))
                                    return false;
                                listObjects.Add(objNew);
                            }
                        }
                        p.SetValue(thisObject, listObjects);
                        continue;
                    }

                    if (_readFromSerializedBufferDelegatesByType.ContainsKey(propType.Name))
                    {
                        ReadFromSerializedBuffer_Delegate method = _readFromSerializedBufferDelegatesByType[propType.Name];
                        if (method != null)
                        {
                            if (!method.Invoke(thisObject, p, ref buffer))
                                return false;
                        }
                        else
                            return false;
                    }
                    else
                    {
                        if(propTypeInfo.IsClass)
                        {
                            object objNew = Activator.CreateInstance(propType);
                            if (!objNew.Deserialize(buffer))
                                return false;
                            p.SetValue(thisObject, objNew);
                        }
                        else
                            return false;
                    }
                }
                propertiesSkip = arrProperties.Length;
            }
            return true;
        }

        public static void ZeroInit(this object obj)
        {
        }

        // Read from buffer and write in property.
        public static bool ReadFromSerializedBuffer_Int8(object objPropertyOwner, PropertyInfo property, ref BinaryReader buffer)
        {
            byte cType = buffer.ReadByte(); // Type
            byte val;
            if ((cType & ((byte)0x7f)) != (char)VariableType.Int8)
                return false;
            if ((cType & ((byte)0x80)) != 0)
                val = 0;
            else
                val = buffer.ReadByte();

            property.SetValue(objPropertyOwner, val);
            return true;
        }

        public static bool ReadFromSerializedBuffer_Int16(object objPropertyOwner, PropertyInfo property, ref BinaryReader buffer)
        {
            byte cType = buffer.ReadByte(); // Type
            short val;
            if ((cType & ((byte)0x7f)) != (char)VariableType.Int16)
                return false;
            if ((cType & ((byte)0x80)) != 0)
                val = 0;
            else
                val = buffer.ReadInt16();

            property.SetValue(objPropertyOwner, val);
            return true;
        }

        public static bool ReadFromSerializedBuffer_Int32(object objPropertyOwner, PropertyInfo property, ref BinaryReader buffer)
        {
            byte cType = buffer.ReadByte(); // Type
            int val;
            if ((cType & ((byte)0x7f)) != (char)VariableType.Int32)
                return false;
            if ((cType & ((byte)0x80)) != 0)
                val = 0;
            else
                val = buffer.ReadInt32();

            property.SetValue(objPropertyOwner, val);
            return true;
        }

        public static bool ReadFromSerializedBuffer_Int64(object objPropertyOwner, PropertyInfo property, ref BinaryReader buffer)
        {
            byte cType = buffer.ReadByte(); // Type
            bool bDefault = (cType & ((byte)0x80)) > 0 ? true : false;
            cType = (byte)(cType & 0x7f);
            long val;

            if (cType == (char)VariableType.Int64)
            {

                if (bDefault)
                    val = 0;
                else
                    val = buffer.ReadInt64();
                property.SetValue(objPropertyOwner, val);
                return true;
            }

            return false;
        }

        public static bool ReadFromSerializedBuffer_UInt64(object objPropertyOwner, PropertyInfo property, ref BinaryReader buffer)
        {
            byte cType = buffer.ReadByte(); // Type
            bool bDefault = (cType & ((byte)0x80)) > 0 ? true : false;
            cType = (byte)(cType & 0x7f);
            UInt64 val;

            if (cType == (char)VariableType.UInt64)
            {
                if (bDefault)
                    val = 0;
                else
                    val = buffer.ReadUInt64();
                property.SetValue(objPropertyOwner, val);
                return true;
            }

            return false;
        }

        public static bool ReadFromSerializedBuffer_Chips(object objPropertyOwner, PropertyInfo property, ref BinaryReader buffer)
        {
            byte cType = buffer.ReadByte(); // Type
            bool bDefault = (cType & ((byte)0x80)) > 0 ? true : false;
            cType = (byte)(cType & 0x7f);
            long val;

            if (cType == (char)VariableType.Chips)
            {
                if (bDefault)
                    val = 0;
                else
                { // Read 5 bytes.
                    //byte[] btt = buffer.ReadBytes(5);

                    val = buffer.ReadInt32();
                    val |= ((((long)buffer.ReadByte()) << 32) & 0x000000FF00000000);
                }

                decimal dVal = (decimal)(val / 100.0m);
                property.SetValue(objPropertyOwner, dVal);
                return true;
            }

            return false;
        }

        public static bool ReadFromSerializedBuffer_Bool(object objPropertyOwner, PropertyInfo property, ref BinaryReader buffer)
        {
            byte cType = buffer.ReadByte(); // Type
            bool val;
            if ((cType & ((byte)0x7f)) != (char)VariableType.Bool)
                return false;
            if ((cType & ((byte)0x80)) != 0)
                val = false;
            else
                val = buffer.ReadBoolean();

            property.SetValue(objPropertyOwner, val);
            return true;
        }

        public static bool ReadFromSerializedBuffer_Double(object objPropertyOwner, PropertyInfo property, ref BinaryReader buffer)
        {
            byte cType = buffer.ReadByte(); // Type
            double val;
            if ((cType & ((byte)0x7f)) != (char)VariableType.Double)
                return false;
            if ((cType & ((byte)0x80)) != 0)
                val = 0.0;
            else
                val = buffer.ReadDouble();

            property.SetValue(objPropertyOwner, val);
            return true;
        }

        public static bool ReadFromSerializedBuffer_Float(object objPropertyOwner, PropertyInfo property, ref BinaryReader buffer)
        {
            byte cType = buffer.ReadByte(); // Type
            float val;
            if ((cType & ((byte)0x7f)) != (char)VariableType.Float)
                return false;
            if ((cType & ((byte)0x80)) != 0)
                val = 0.0f;
            else
                val = buffer.ReadSingle();

            property.SetValue(objPropertyOwner, val);
            return true;
        }

        public static bool ReadFromSerializedBuffer_Date(object objPropertyOwner, PropertyInfo property, ref BinaryReader buffer)
        {
            byte cType = buffer.ReadByte(); // Type
            DateTime val;
            if ((cType & ((byte)0x7f)) != (char)VariableType.Date)
                return false;
            if ((cType & ((byte)0x80)) != 0)
                val = new DateTime();
            else
            {
                int year = (int)buffer.ReadInt16();
                int month = (int)buffer.ReadByte();
                int day = (int)buffer.ReadByte();
                int hour = (int)buffer.ReadByte();
                int minute = (int)buffer.ReadByte();
                int second = (int)buffer.ReadByte();
                int reserved = (int)buffer.ReadByte();
                val = new DateTime(year, month, day, hour, minute, second);
            }
            property.SetValue(objPropertyOwner, val);
            return true;
        }

        public static bool ReadFromSerializedBuffer_DateTime(object objPropertyOwner, PropertyInfo property, ref BinaryReader buffer)
        {
            byte cType = buffer.ReadByte(); // Type
            DateTime val;
            if ((cType & ((byte)0x7f)) != (char)VariableType.DateTime)
                return false;
            if ((cType & ((byte)0x80)) != 0)
                val = new DateTime();
            else
            {
                int year = (int)buffer.ReadInt16();
                int month = (int)buffer.ReadByte();
                int day = (int)buffer.ReadByte();
                int hour = (int)buffer.ReadByte();
                int minute = (int)buffer.ReadByte();
                int second = (int)buffer.ReadByte();
                int reserved = (int)buffer.ReadByte();
                val = new DateTime(year, month, day, hour, minute, second);
            }
            property.SetValue(objPropertyOwner, val);
            return true;
        }

        public static bool ReadFromSerializedBuffer_Time(object objPropertyOwner, PropertyInfo property, ref BinaryReader buffer)
        {
            byte cType = buffer.ReadByte(); // Type
            DateTime val;
            if ((cType & ((byte)0x7f)) != (char)VariableType.Time)
                return false;
            if ((cType & ((byte)0x80)) != 0)
                val = new DateTime();
            else
            {
                int year = (int)buffer.ReadInt16();
                int month = (int)buffer.ReadByte();
                int day = (int)buffer.ReadByte();
                int hour = (int)buffer.ReadByte();
                int minute = (int)buffer.ReadByte();
                int second = (int)buffer.ReadByte();
                int reserved = (int)buffer.ReadByte();
                val = new DateTime(year, month, day, hour, minute, second);
            }
            property.SetValue(objPropertyOwner, val);
            return true;
        }

        public static bool ReadFromSerializedBuffer_String(object objPropertyOwner, PropertyInfo property, ref BinaryReader buffer)
        {
            byte cType = buffer.ReadByte(); // Type
            string val;
            if ((cType & ((byte)0x7f)) != (char)VariableType.String)
                return false;
            if ((cType & ((byte)0x80)) != 0)
                val = "";
            else
            {
                short len = buffer.ReadInt16();
                if (len > 0)
                {
                    byte[] stringBytes = buffer.ReadBytes((int)len);
                    Encoding enc = new System.Text.UTF8Encoding();
                    val = enc.GetString(stringBytes);
                }
                else
                    val = "";
            }
            property.SetValue(objPropertyOwner, val);
            return true;
        }

        public static bool ReadFromSerializedBuffer_Binary(object objPropertyOwner, PropertyInfo property, ref BinaryReader buffer)
        {
            byte cType = buffer.ReadByte(); // Type
            byte[] val;
            if ((cType & ((byte)0x7f)) != (char)VariableType.Binary)
                return false;
            if ((cType & ((byte)0x80)) != 0)
                val = null;
            else
            {
                int len = buffer.ReadInt32();
                if (len > 0)
                    val = buffer.ReadBytes((int)len);
                else
                    val = null;
            }
            property.SetValue(objPropertyOwner, val);
            return true;
        }

        // Write property value into buffer.
        public static bool WriteSerializedBuffer_Int8(object objPropertyOwner, PropertyInfo property, ref BinaryWriter buffer)
        {
            byte val = (byte)property.GetValue(objPropertyOwner);
            if (val == 0)
            {
                // Write variable type + default value type indicator.
                buffer.Write((byte)(((byte)VariableType.Int8) | ((byte)0x80)));
                return true;
            }

            // Write variable type.
            buffer.Write((byte)VariableType.Int8);
            // Write variable value.
            buffer.Write(val);
            return true;
        }

        public static bool WriteSerializedBuffer_Int16(object objPropertyOwner, PropertyInfo property, ref BinaryWriter buffer)
        {
            short val = (short)property.GetValue(objPropertyOwner);
            if (val == 0)
            {
                // Write variable type + default value type indicator.
                buffer.Write((byte)(((byte)VariableType.Int16) | ((byte)0x80)));
                return true;
            }
            // Write variable type.
            buffer.Write((byte)VariableType.Int16);
            // Write variable value.
            buffer.Write(val);
            return true;
        }

        public static bool WriteSerializedBuffer_Int32(object objPropertyOwner, PropertyInfo property, ref BinaryWriter buffer)
        {
            int val = (int)property.GetValue(objPropertyOwner);
            if (val == 0)
            {
                // Write variable type + default value type indicator.
                buffer.Write((byte)(((byte)VariableType.Int32) | ((byte)0x80)));
                return true;
            }

            // Write variable type.
            buffer.Write((byte)VariableType.Int32);
            // Write variable value.
            buffer.Write(val);
            return true;
        }

        public static bool WriteSerializedBuffer_Int64(object objPropertyOwner, PropertyInfo property, ref BinaryWriter buffer)
        {
            long val = (long)property.GetValue(objPropertyOwner);
            if (val == 0)
            {
                // Write variable type + default value type indicator.
                buffer.Write((byte)(((byte)VariableType.Int64) | ((byte)0x80)));
                return true;
            }

            // Write variable type.
            buffer.Write((byte)VariableType.Int64);
            // Write variable value.
            buffer.Write(val);
            return true;
        }

        public static bool WriteSerializedBuffer_UInt64(object objPropertyOwner, PropertyInfo property, ref BinaryWriter buffer)
        {
            UInt64 val = (UInt64)property.GetValue(objPropertyOwner);
            if (val == 0)
            {
                // Write variable type + default value type indicator.
                buffer.Write((byte)(((byte)VariableType.UInt64) | ((byte)0x80)));
                return true;
            }

            // Write variable type.
            buffer.Write((byte)VariableType.UInt64);
            // Write variable value.
            buffer.Write(val);
            return true;
        }

        public static bool WriteSerializedBuffer_Chips(object objPropertyOwner, PropertyInfo property, ref BinaryWriter buffer)
        {
            decimal val = (decimal)property.GetValue(objPropertyOwner);
            if (val == 0)
            {
                // Write variable type + default value type indicator.
                buffer.Write((byte)(((byte)VariableType.Chips) | ((byte)0x80)));
                return true;
            }

            long lval = (long)(val * 100);
            // Write variable type.
            buffer.Write((byte)VariableType.Chips);
            // Write variable value.
            buffer.Write((UInt32)val);
            lval >>= 32;
            buffer.Write((byte)(lval & 0xFF));
            return true;
        }

        public static bool WriteSerializedBuffer_Bool(object objPropertyOwner, PropertyInfo property, ref BinaryWriter buffer)
        {
            bool val = (bool)property.GetValue(objPropertyOwner);
            if (val == false)
            {
                // Write variable type + default value type indicator.
                buffer.Write((byte)(((byte)VariableType.Bool) | ((byte)0x80)));
                return true;
            }

            // Write variable type.
            buffer.Write((byte)VariableType.Bool);
            // Write variable value.
            buffer.Write(val);
            return true;
        }

        public static bool WriteSerializedBuffer_Double(object objPropertyOwner, PropertyInfo property, ref BinaryWriter buffer)
        {
            double val = (double)property.GetValue(objPropertyOwner);
            if (val == 0.0)
            {
                // Write variable type + default value type indicator.
                buffer.Write((byte)(((byte)VariableType.Double) | ((byte)0x80)));
                return true;
            }

            // Write variable type.
            buffer.Write((byte)VariableType.Double);
            // Write variable value.
            buffer.Write(val);
            return true;
        }

        public static bool WriteSerializedBuffer_Float(object objPropertyOwner, PropertyInfo property, ref BinaryWriter buffer)
        {
            float val = (float)property.GetValue(objPropertyOwner);
            if (val == 0.0f)
            {
                // Write variable type + default value type indicator.
                buffer.Write((byte)(((byte)VariableType.Float) | ((byte)0x80)));
                return true;
            }

            // Write variable type.
            buffer.Write((byte)VariableType.Float);
            // Write variable value.
            buffer.Write(val);
            return true;
        }

        public static bool WriteSerializedBuffer_Date(object objPropertyOwner, PropertyInfo property, ref BinaryWriter buffer)
        {
            DateTime value = (DateTime)property.GetValue(objPropertyOwner);
            if (value != null)
            {
                // Write variable type.
                buffer.Write((byte)VariableType.Date);

                // Write variable value.
                buffer.Write((short)value.Year);
                buffer.Write((byte)value.Month);
                buffer.Write((byte)value.Day);
                buffer.Write((byte)value.Hour);
                buffer.Write((byte)value.Minute);
                buffer.Write((byte)value.Second);
                buffer.Write((byte)0); // Reserved
            }
            else
            {
                // Write variable type + default value type indicator.
                buffer.Write((byte)(((byte)VariableType.Date) | ((byte)0x80)));
            }
            return true;
        }

        public static bool WriteSerializedBuffer_Time(object objPropertyOwner, PropertyInfo property, ref BinaryWriter buffer)
        {
            DateTime value = (DateTime)property.GetValue(objPropertyOwner);
            if (value != null)
            {
                // Write variable type.
                buffer.Write((byte)VariableType.Time);

                // Write variable value.
                buffer.Write((short)value.Year);
                buffer.Write((byte)value.Month);
                buffer.Write((byte)value.Day);
                buffer.Write((byte)value.Hour);
                buffer.Write((byte)value.Minute);
                buffer.Write((byte)value.Second);
                buffer.Write((byte)0); // Reserved
            }
            else
            {
                // Write variable type + default value type indicator.
                buffer.Write((byte)(((byte)VariableType.Time) | ((byte)0x80)));
            }
            return true;
        }

        public static bool WriteSerializedBuffer_DateTime(object objPropertyOwner, PropertyInfo property, ref BinaryWriter buffer)
        {
            DateTime value = (DateTime)property.GetValue(objPropertyOwner);
            if (value != null)
            {
                // Write variable type.
                buffer.Write((byte)VariableType.DateTime);

                // Write variable value.
                buffer.Write((short)value.Year);
                buffer.Write((byte)value.Month);
                buffer.Write((byte)value.Day);
                buffer.Write((byte)value.Hour);
                buffer.Write((byte)value.Minute);
                buffer.Write((byte)value.Second);
                buffer.Write((byte)0); // Reserved
            }
            else
            {
                // Write variable type + default value type indicator.
                buffer.Write((byte)(((byte)VariableType.DateTime) | ((byte)0x80)));
            }
            return true;
        }

        public static bool WriteSerializedBuffer_String(object objPropertyOwner, PropertyInfo property, ref BinaryWriter buffer)
        {
            string value = (String)property.GetValue(objPropertyOwner);
            short len = (short)(string.IsNullOrEmpty(value) ? 0 : value.Length);

            if (len > 0)
            {
                // Write variable type.
                buffer.Write((byte)VariableType.String);

                // Write variable value.
                byte[] stringBytes = System.Text.UTF8Encoding.UTF8.GetBytes(value);

                len = (stringBytes != null) ? (short)stringBytes.Length : (short)0;

                // Write string len.
                buffer.Write(len);

                buffer.Write(stringBytes);
            }
            else
            {
                // Write variable type + default value type indicator.
                buffer.Write((byte)(((byte)VariableType.String) | ((byte)0x80)));
            }

            return true;
        }

        public static bool WriteSerializedBuffer_Binary(object objPropertyOwner, PropertyInfo property, ref BinaryWriter buffer)
        {
            byte[] value = (byte[])property.GetValue(objPropertyOwner);
            int len = (value == null ? 0 : value.Length);

            if (len > 0)
            {
                // Write variable type.
                buffer.Write((byte)VariableType.Binary);
                // Write string len.
                buffer.Write(len);
                buffer.Write(value);
            }
            else
            {
                // Write variable type + default value type indicator.
                buffer.Write((byte)(((byte)VariableType.Binary) | ((byte)0x80)));
            }
            return true;
        }
    }
}
