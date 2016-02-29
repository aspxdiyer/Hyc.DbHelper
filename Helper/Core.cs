﻿using AspxFrameWork.DataHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Hyc.DbDriver
{

  public class HycParameter
  {
    private Hashtable table;
    public HycParameter()
    {
      table = new Hashtable(System.StringComparer.Create(System.Globalization.CultureInfo.CurrentCulture, true));
    }
    public void SetParameter(string key, object value_ren)
    {
      table.Add(key, value_ren);
    }

    public Hashtable Parameters
    {
      get { return table; }
    }
  }

  internal class Core
  {
    internal List<DbParameter> getDbParameter<T>(Hashtable myhash, string Prefix) where T : class,new()
    {
      DataTable table = new AttributeHelper<T>().getEntityTable();
      List<DbParameter> p = new List<DbParameter>();
      Dictionary<Type, DbType> TypeToDb = getDbType();
      foreach (DictionaryEntry de in myhash)
      {
        string keys = de.Key.ToString();
        object colum_Value = de.Value;
        var Rows = table.AsEnumerable().Where(n => (n.Field<string>("Name").ToLower() == keys.ToLower() || n.Field<string>("ClassName").ToLower() == keys.ToLower())).Select(n => new { Name = n.Field<string>("Name"), ClassName = n.Field<string>("ClassName"), Explain = n.Field<string>("Explain"), Type = n.Field<System.Type>("Type"), Length = n.Field<int>("Length"), Usage = n.Field<int>("Usage"), DefaultValue = n.Field<object>("DefaultValue"), Null = n.Field<bool>("Null"), UrlEncode = n.Field<bool>("UrlEncode") }).ToList();
        //Rows.Count
        if (Rows.Count > 0)
        {
          //p.Add()
          var Row = Rows[0];
          //var colum_Value = (myhash.Contains(Row.Name.ToLower()) ? myhash[Row.Name] : myhash[Row.ClassName]);
          DbParameter d = SqlHelper.MakeInParam(Prefix + Row.Name, TypeToDb[Row.Type], Row.Length, colum_Value);
          if (Row.Type == typeof(System.String) && Row.Length != 0 && Row.UrlEncode)
          {
            d = SqlHelper.MakeInParam(Prefix + Row.Name, TypeToDb[Row.Type], Row.Length, colum_Value.ToString().Replace("&", "&#38;").Replace("<", "&#60;").Replace(">", "&#62;"));
          }
          p.Add(d);
          //fieldBuilder.Add("[" + Row.Name + "] = " + Prefix + "" + Row.Name);
        }
      }

      //if (table.Rows.Count > 0)
      //{
      //  foreach (DataRow dr in table.Rows)
      //  {
      //    String Name = dr["Name"].ToString();
      //    String ClassName = dr["ClassName"].ToString();
      //    Type Type = (Type)dr["Type"];
      //    Int32 Length = Convert.ToInt32(dr["Length"]);
      //    EnumFieldUsage Usage = (EnumFieldUsage)Convert.ToInt32(dr["Usage"]);
      //    bool Null = Convert.ToBoolean(dr["Null"]);
      //    bool UrlEncode = Convert.ToBoolean(dr["UrlEncode"]);

      //    object colValue = null;
      //    bool NameExists = myhash.Contains(Name.ToLower());
      //    bool ClassNameExists = myhash.Contains(ClassName.ToLower());

      //    if (NameExists || ClassNameExists)
      //    {
      //      String ValueName = NameExists ? Name : ClassName;
      //      colValue = Convert.ChangeType(myhash[ValueName.ToLower()], Type);
      //      DbParameter d = SqlHelper.MakeInParam(Prefix + Name, TypeToDb[Type], Length, colValue);

      //      if (Type == typeof(System.String) && Length != 0 && UrlEncode)
      //      {
      //        d = SqlHelper.MakeInParam(Prefix + Name, TypeToDb[Type], Length, colValue.ToString().Replace("&", "&#38;").Replace("<", "&#60;").Replace(">", "&#62;"));
      //      }

      //      p.Add(d);
      //    }
      //  }
      //}
      return p;
    }

    internal Dictionary<Type, DbType> getDbType()
    {
      Dictionary<Type, DbType> typeMap = new Dictionary<Type, DbType>();
      typeMap[typeof(byte)] = System.Data.DbType.Byte;
      typeMap[typeof(sbyte)] = System.Data.DbType.SByte;
      typeMap[typeof(short)] = System.Data.DbType.Int16;
      typeMap[typeof(ushort)] = System.Data.DbType.UInt16;
      typeMap[typeof(int)] = System.Data.DbType.Int32;
      typeMap[typeof(uint)] = System.Data.DbType.UInt32;
      typeMap[typeof(long)] = System.Data.DbType.Int64;
      typeMap[typeof(ulong)] = System.Data.DbType.UInt64;
      typeMap[typeof(float)] = System.Data.DbType.Single;
      typeMap[typeof(double)] = System.Data.DbType.Double;
      typeMap[typeof(decimal)] = System.Data.DbType.Decimal;
      typeMap[typeof(bool)] = System.Data.DbType.Boolean;
      typeMap[typeof(string)] = System.Data.DbType.String;
      typeMap[typeof(char)] = System.Data.DbType.StringFixedLength;
      typeMap[typeof(Guid)] = System.Data.DbType.Guid;
      typeMap[typeof(DateTime)] = System.Data.DbType.DateTime;
      //typeMap[typeof(DateTimeOffset)] = System.Data.DbType.DateTimeOffset;
      typeMap[typeof(byte[])] = System.Data.DbType.Binary;
      typeMap[typeof(byte?)] = System.Data.DbType.Byte;
      typeMap[typeof(sbyte?)] = System.Data.DbType.SByte;
      typeMap[typeof(short?)] = System.Data.DbType.Int16;
      typeMap[typeof(ushort?)] = System.Data.DbType.UInt16;
      typeMap[typeof(int?)] = System.Data.DbType.Int32;
      typeMap[typeof(uint?)] = System.Data.DbType.UInt32;
      typeMap[typeof(long?)] = System.Data.DbType.Int64;
      typeMap[typeof(ulong?)] = System.Data.DbType.UInt64;
      typeMap[typeof(float?)] = System.Data.DbType.Single;
      typeMap[typeof(double?)] = System.Data.DbType.Double;
      typeMap[typeof(decimal?)] = System.Data.DbType.Decimal;
      typeMap[typeof(bool?)] = System.Data.DbType.Boolean;
      typeMap[typeof(char?)] = System.Data.DbType.StringFixedLength;
      typeMap[typeof(Guid?)] = System.Data.DbType.Guid;
      typeMap[typeof(DateTime?)] = System.Data.DbType.DateTime;
      //typeMap[typeof(DateTimeOffset?)] = System.Data.DbType.DateTimeOffset;
      //typeMap[typeof(System.Data.Linq.Binary)] = DbType.Binary;
      return typeMap;//[type];
    }

    /// <summary>
    /// 返回实体
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dt"></param>
    /// <returns></returns>
    internal T GetModel<T>(DataTable dt,int i=0) where T : class,new()
    {
      T _t = Activator.CreateInstance<T>();
      //获取对象所有属性
      PropertyInfo[] propertyInfo = _t.GetType().GetProperties();
      for (int j = 0; j < dt.Columns.Count; j++)
      {
        foreach (PropertyInfo info in propertyInfo)
        {
          object[] attrs = info.GetCustomAttributes(typeof(FieldAttribute), true);
          string pn = "";
          if (attrs.Length == 1)
          {
            FieldAttribute attr = (FieldAttribute)attrs[0];
            if (!string.IsNullOrEmpty(attr.FieldName.Trim()))
            {
              pn = attr.FieldName.Trim().ToUpper();
            }
          }
          //属性名称(或者Attribute里面的fileName)和列名相同时赋值
          pn = string.IsNullOrEmpty(pn) ? info.Name.ToUpper() : pn;

          if (dt.Columns[j].ColumnName.ToUpper().Equals(pn))
          {
            if (dt.Rows[i][j] != DBNull.Value)
            {
              info.SetValue(_t, dt.Rows[i][j], null);
            }
            else
            {
              info.SetValue(_t, null, null);
            }
            break;
          }
        }
      }
      return _t;
    }


  }
}
