using System;
using System.Collections.Generic;
using System.Text;
using AspxFrameWork.DataHelper;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Reflection;
using Hyc.DbDriver.Interface;

namespace Hyc.DbDriver.SqlServer
{

  interface IMonth
  {
    void Test<T>() where T:class,new();
  }

  class SampleClass1 :IMonth
  {

    public void Test<T>() where T : class, new()
    {
      string tableName = new T().GetType().Name;
      throw new NotImplementedException();
    }
  }

  class Test
  {
    public void mm()
    {
      new SampleClass1().Test<Modelss>();
    }
  }
  public class Modelss
  {
    public Modelss(){

    }
    public int code { get; set; }
  }

  public class DbHelper : IDbHelper
  {
    /// <summary>
    /// 表前缀
    /// </summary>
    public String TablePrefix { get; set; }

    public DbHelper(string DBConnectString, string DbProvider, string TablePrefix)
    {
      this.TablePrefix = TablePrefix;

      SqlHelper.AspxConnection = DBConnectString;
      SqlHelper.AspxProvider = DbProvider;
    }

    public object CreateRecord<T>(Hashtable myhash) where T : class, new()
    {
      string tableName = getTableName<T>();
      DataTable table = new AttributeHelper<T>().getEntityTable();
      List<DbParameter> p = new List<DbParameter>();
      StringBuilder strSql = new StringBuilder();
      StringBuilder fieldBuilder = new StringBuilder();
      StringBuilder valueBuilder = new StringBuilder();
      Dictionary<Type, DbType> TypeToDb = getDbType();//new Dictionary<Type,DbType>();
      if (table.Rows.Count > 1)
      {
        Hashtable ht = new Hashtable(System.StringComparer.Create(System.Globalization.CultureInfo.CurrentCulture, true));
        foreach (DataRow dr in table.Rows)
        {
          String Name = dr["Name"].ToString();
          String ClassName = dr["ClassName"].ToString();
          Type Type = (Type)dr["Type"];
          Int32 Length = Convert.ToInt32(dr["Length"]);
          EnumFieldUsage Usage = (EnumFieldUsage)Convert.ToInt32(dr["Usage"]);
          object DefaultValue = dr["DefaultValue"];
          bool Null = Convert.ToBoolean(dr["Null"]);
          bool UrlEncode = Convert.ToBoolean(dr["UrlEncode"]);

          DbType dbtype = TypeToDb[Type];


          bool NameExists = myhash.Contains(Name.ToLower());
          bool ClassNameExists = myhash.Contains(ClassName.ToLower());

          String PvName = NameExists ? Name : ClassName;//如果传值不是表的字段名，则取类的字段名(用户获取传过来的值)

          if (Usage == EnumFieldUsage.BySystem || Usage == EnumFieldUsage.IncKey || Usage == EnumFieldUsage.IncPK)
            continue;

          #region 不允许为空值判断
          if (!Null)
          {
            if (!NameExists && !ClassNameExists)
            {
              return -2;
            }
          }
          #endregion

          #region 主键或者唯一键判断
          object KeyValue = ((NameExists || ClassNameExists) ? myhash[PvName.ToLower()] : (DefaultValue == null ? "" : DefaultValue));
          if (!Null)
          {//不允许为空值判断
            if (string.IsNullOrEmpty(KeyValue.ToString()))
              return -2;
          }
          if (Usage == EnumFieldUsage.PrimaryKey || Usage == EnumFieldUsage.UniqueKey)
          {//主键或者唯一键
            if (!string.IsNullOrEmpty(KeyValue.ToString()))
            {
              DbParameter[] p1 = new DbParameter[]{
                SqlHelper.MakeInParam(Prefix+Name,TypeToDb[Type],Length,KeyValue)
              };
              string sql = "select isnull(count(1),0) from [" + TablePrefix + tableName + "] where [" + Name + "]=" + Prefix + Name;
              if (Convert.ToInt32(SqlHelper.ExecuteScalar(CommandType.Text, sql, p1)) > 0)
              {
                return -1;
              }
            }
          }
          #endregion

          if (!string.IsNullOrEmpty(DefaultValue.ToString()))
          {
            if (Type == typeof(System.DateTime) && DefaultValue.ToString().ToLower() == "now")
              DefaultValue = DateTime.Now;
            DbParameter d = SqlHelper.MakeInParam(Prefix + Name, dbtype, Length, DefaultValue);
            //if (Type == typeof(System.String) && Length <= 200 && Length != 0)
            if (Type == typeof(System.String) && Length != 0 && UrlEncode)
            {
              d = SqlHelper.MakeInParam(Prefix + Name, TypeToDb[Type], Length, DefaultValue.ToString().Replace("&", "&#38;").Replace("<", "&#60;").Replace(">", "&#62;"));
            }
            p.Add(d);
            if (!ht.Contains(Name))
            {
              ht.Add(Name, d);
            }
          }

          object colValue = null;
          if (NameExists || ClassNameExists)
          {
            colValue = myhash[PvName.ToLower()];

            if (!Null)
            {//不允许为空值判断
              if (string.IsNullOrEmpty(colValue.ToString()))
                return -2;
            }

            if (ht.Contains(Name))
            {
              p.Remove((DbParameter)ht[Name]);
              ht.Remove(Name);
            }
            DbParameter d = SqlHelper.MakeInParam(Prefix + Name, dbtype, Length, colValue);
            //if (Type == typeof(System.String) && Length <= 200 && Length != 0)
            if (Type == typeof(System.String) && Length != 0 && UrlEncode)
            {
              d = SqlHelper.MakeInParam(Prefix + Name, TypeToDb[Type], Length, colValue.ToString().Replace("&", "&#38;").Replace("<", "&#60;").Replace(">", "&#62;"));
            }
            p.Add(d);
          }

          if (NameExists || ClassNameExists || !string.IsNullOrEmpty(DefaultValue.ToString()))
          {
            fieldBuilder.Append("[" + Name + "],");
            valueBuilder.Append(Prefix + Name + ",");
          }
        }

        strSql.Append("insert into [" + TablePrefix + tableName + "](");
        strSql.Append(fieldBuilder.Remove(fieldBuilder.Length - 1, 1).ToString());
        strSql.Append(") values(");
        strSql.Append(valueBuilder.Remove(valueBuilder.Length - 1, 1).ToString());
        strSql.Append(");SELECT @@identity");
        return SqlHelper.ExecuteScalar(CommandType.Text, strSql.ToString(), p.ToArray());
      }
      return 0;
    }

    public object UpdateRecord<T>(Hashtable myhash) where T : class, new()
    {
      DataTable table = new AttributeHelper<T1>().getEntityTable();
      List<DbParameter> p = new List<DbParameter>();
      StringBuilder strSql = new StringBuilder();
      StringBuilder fieldBuilder = new StringBuilder();
      Dictionary<Type, DbType> TypeToDb = getDbType();//new Dictionary<Type,DbType>();
      if (table.Rows.Count > 1)
      {
        string PkName = ""; Type PkType = typeof(System.Int32); Int32 PkLength = 0; object PkValue = null;
        foreach (DataRow dr in table.Rows)
        {
          String Name = dr["Name"].ToString();
          String ClassName = dr["ClassName"].ToString();
          Type Type = (Type)dr["Type"];
          Int32 Length = Convert.ToInt32(dr["Length"]);
          bool UrlEncode = Convert.ToBoolean(dr["UrlEncode"]);
          EnumFieldUsage Usage = (EnumFieldUsage)Convert.ToInt32(dr["Usage"]);
          if (Usage == EnumFieldUsage.IncKey || Usage == EnumFieldUsage.IncPK || Usage == EnumFieldUsage.PrimaryKey)
          {
            bool NameExists = myhash.Contains(Name.ToLower());
            bool ClassNameExists = myhash.Contains(ClassName.ToLower());
            if (NameExists || ClassNameExists)
            {
              PkName = NameExists ? Name : ClassName;
              PkValue = Convert.ChangeType(myhash[PkName.ToLower()], Type);
              PkName = Name;
              PkType = Type;
              PkLength = Length;

              DbParameter d = SqlHelper.MakeInParam(Prefix + PkName, TypeToDb[PkType], PkLength, PkValue);
              //if (Type == typeof(System.String) && Length <= 200 && Length != 0)
              if (Type == typeof(System.String) && Length != 0 && UrlEncode)
              {
                d = SqlHelper.MakeInParam(Prefix + PkName, TypeToDb[PkType], PkLength, PkValue.ToString().Replace("&", "&#38;").Replace("<", "&#60;").Replace(">", "&#62;"));
              }
              p.Add(d);

              break;
            }
          }
        }

        foreach (DataRow dr in table.Rows)
        {
          String Name = dr["Name"].ToString();
          String ClassName = dr["ClassName"].ToString();
          Type Type = (Type)dr["Type"];
          Int32 Length = Convert.ToInt32(dr["Length"]);
          EnumFieldUsage Usage = (EnumFieldUsage)Convert.ToInt32(dr["Usage"]);
          bool Null = Convert.ToBoolean(dr["Null"]);
          bool UrlEncode = Convert.ToBoolean(dr["UrlEncode"]);

          if (Usage == EnumFieldUsage.BySystem || Usage == EnumFieldUsage.IncKey || Usage == EnumFieldUsage.IncPK)
            continue;

          object colValue = null;
          bool NameExists = myhash.Contains(Name.ToLower());
          bool ClassNameExists = myhash.Contains(ClassName.ToLower());


          #region 不允许为空值判断
          if (!Null)
          {
            if (!NameExists && !ClassNameExists)
            {
              return -2;
            }
          }
          #endregion

          if (NameExists || ClassNameExists)
          {
            String ValueName = NameExists ? Name : ClassName;

            if (PkName.ToLower() == Name.ToLower())
              continue;
            colValue = Convert.ChangeType(myhash[ValueName.ToLower()], Type);//myhash[Name.ToLower()];

            if (!Null)
            {//不允许为空值判断
              if (string.IsNullOrEmpty(colValue.ToString()))
                return -2;
            }

            if (Usage == EnumFieldUsage.PrimaryKey || Usage == EnumFieldUsage.UniqueKey)
            {//主键或者唯一键
              if (!string.IsNullOrEmpty(colValue.ToString()))
              {
                DbParameter[] p1 = new DbParameter[]{
                  SqlHelper.MakeInParam(Prefix+Name,TypeToDb[Type],Length,colValue),
                  SqlHelper.MakeInParam(Prefix+PkName,TypeToDb[PkType],PkLength,PkValue)
                };
                string sql = "select isnull(count(1),0) from [" + TablePrefix + tableName + "] where [" + Name + "]=" + Prefix + Name + " and [" + PkName + "]<>" + Prefix + PkName;
                if (Convert.ToInt32(SqlHelper.ExecuteScalar(CommandType.Text, sql, p1)) > 0)
                {
                  return -1;
                }
              }
            }

            DbParameter d = SqlHelper.MakeInParam(Prefix + Name, TypeToDb[Type], Length, colValue);
            if (Type == typeof(System.String) && Length != 0 && UrlEncode)
            {
              d = SqlHelper.MakeInParam(Prefix + Name, TypeToDb[Type], Length, colValue.ToString().Replace("&", "&#38;").Replace("<", "&#60;").Replace(">", "&#62;"));
            }
            p.Add(d);

            fieldBuilder.Append("[" + Name + "] = " + Prefix + "" + Name + ",");
          }
        }

        strSql.Append("update [" + TablePrefix + tableName + "] set ");
        strSql.Append(fieldBuilder.Remove(fieldBuilder.Length - 1, 1).ToString());
        strSql.Append(" where [" + PkName + "]=" + Prefix + PkName);
        SqlHelper.ExecuteNonQuery(strSql.ToString(), p.ToArray());
        return PkValue;
      }
      return 0;
    }

    public void DeleteRecord<T>(object Code) where T : class, new()
    {
      throw new NotImplementedException();
    }

    public void DeleteRecord<T>(string strwhere) where T : class, new()
    {
      throw new NotImplementedException();
    }

    public bool Exists<T>(string strwhere) where T : class, new()
    {
      throw new NotImplementedException();
    }

    public bool Exists<T>(string strwhere, Hashtable myhash) where T : class, new()
    {
      throw new NotImplementedException();
    }

    public object GetColObject<T>(string Col, string strWhere) where T : class, new()
    {
      throw new NotImplementedException();
    }

    public int GetTableTotal<T>(string strwhere) where T : class, new()
    {
      throw new NotImplementedException();
    }

    public int GetTableSum<T>(string strWhere) where T : class, new()
    {
      throw new NotImplementedException();
    }

    public void UpdateByCol<T>(string Col, string Value, string CodeCol, int code) where T : class, new()
    {
      throw new NotImplementedException();
    }

    public void UpdateByCol<T>(string[] Col, string[] Value, string CodeCol, int code) where T : class, new()
    {
      throw new NotImplementedException();
    }

    public void UpdateByCol<T>(string Col, string Value, string CodeCol, string code) where T : class, new()
    {
      throw new NotImplementedException();
    }

    public void UpdateByCol<T>(string[] Col, string[] Value, string CodeCol, string code) where T : class, new()
    {
      throw new NotImplementedException();
    }

    public void Inc<T>(string ColName, int IncValue, string CodeCol, string code) where T : class, new()
    {
      throw new NotImplementedException();
    }

    public void Inc<T>(string ColName, int IncValue, string strwhere) where T : class, new()
    {
      throw new NotImplementedException();
    }

    public DataSet FindList<T>(string strWhere, string order) where T : class, new()
    {
      throw new NotImplementedException();
    }

    public DataSet FindList<T>(string Column, string strWhere, string order) where T : class, new()
    {
      throw new NotImplementedException();
    }

    public DataSet FindByPage<T>(string FieldKey, int PageSize, int PageIndex, string strWhere, string Column, string FieldOrder) where T : class, new()
    {
      throw new NotImplementedException();
    }

    public DataSet FindByPage<T>(string FieldKey, int PageSize, int PageIndex, string strWhere, string Column, string FieldOrder, ref int RecordCount) where T : class, new()
    {
      throw new NotImplementedException();
    }

    public T TryFind<T>(string column, string where) where T : class, new()
    {
      throw new NotImplementedException();
    }

    public T TryFind<T>(string where) where T : class, new()
    {
      throw new NotImplementedException();
    }

    public T TryFind<T>(string column, string where, Hashtable table) where T : class, new()
    {
      throw new NotImplementedException();
    }

    public T TryFind<T>(string where, Hashtable table) where T : class, new()
    {
      throw new NotImplementedException();
    }

    public List<T> ToList<T>(DataSet table) where T : class, new()
    {
      throw new NotImplementedException();
    }

    public object FindMaxColumn<T>(string Column, string where) where T : class, new()
    {
      throw new NotImplementedException();
    }

    public int Procedure(string ProcName, DbParameter[] p)
    {
      throw new NotImplementedException();
    }

    public int ProcedureByErr(string ProcName, DbParameter[] p, ref string ErrString)
    {
      throw new NotImplementedException();
    }

    public DataSet ProDataSet(string ProcName, DbParameter[] p)
    {
      throw new NotImplementedException();
    }

    public DataSet Query(string sql)
    {
      throw new NotImplementedException();
    }

    public DataSet Query(string sql, DbParameter[] p)
    {
      throw new NotImplementedException();
    }

    public int ExecuteNonQuery(string sql, DbParameter[] p)
    {
      throw new NotImplementedException();
    }

    public int ExecuteNonQuery(string sql, Hashtable table)
    {
      throw new NotImplementedException();
    }

    public DataSet FindSetBySql(string tableName, string ColName, string OrderString, string strWhere, DbParameter[] para, int page, int PageSize)
    {
      throw new NotImplementedException();
    }

    public DataSet FindSetBySql(string tableName, string ColName, string OrderString, string strWhere, Hashtable table, int page, int PageSize)
    {
      throw new NotImplementedException();
    }

    private string Prefix
    {
      get
      {
        switch (TablePrefix.ToLower())
        {
          case "mysql":
          case "oracle":
            return "?";
          default:
            return "@";
        }
      }
    }

    private string getTableName<T>() where T : class,new()
    {
      string tableName = new T().GetType().Name;
      string tmpTable = TableAttributeHelper<T>.TableName();
      if (!string.IsNullOrEmpty(tmpTable))
        tableName = tmpTable;
      return tableName;
    }

    private Dictionary<Type, DbType> getDbType()
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
  }
}
