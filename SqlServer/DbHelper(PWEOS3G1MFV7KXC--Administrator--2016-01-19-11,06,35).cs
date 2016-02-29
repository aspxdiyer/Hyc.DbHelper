using System;
using System.Collections.Generic;
using System.Text;
using AspxFrameWork.DataHelper;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Reflection;
using Hyc.DbDriver.Interface;
using System.Linq;
using System.Linq.Expressions;

namespace Hyc.DbDriver.SqlServer
{

  //interface IMonth
  //{
  //  void Test<T>() where T : class,new();
  //}

  //class SampleClass1 : IMonth
  //{

  //  public void Test<T>() where T : class, new()
  //  {
  //    string tableName = new T().GetType().Name;
  //    throw new NotImplementedException();
  //  }
  //}

  //class Test
  //{
  //  public void mm()
  //  {
  //    new SampleClass1().Test<Modelss>();
  //  }
  //}
  //public class Modelss
  //{
  //  public Modelss()
  //  {

  //  }
  //  public int code { get; set; }
  //}

  public class DbHelper : IDbHelper
  {
    /// <summary>
    /// 表前缀
    /// </summary>
    public String TablePrefix { get; set; }

    /// <summary>
    /// 表名
    /// </summary>
    private string tableName { get; set; }

    public DbHelper(string DBConnectString, string tableName, string TablePrefix)
    {
      this.TablePrefix = TablePrefix;

      this.tableName = tableName;
      SqlHelper.AspxConnection = DBConnectString;
      SqlHelper.AspxProvider = "MsSql";
    }

    public object CreateRecord<T>(T model) where T : class, new()
    {

      DataTable table = new AttributeHelper<T>().getEntityTable();
      List<DbParameter> p = new List<DbParameter>();
      StringBuilder strSql = new StringBuilder();
      Dictionary<Type, DbType> TypeToDb = new Core().getDbType();//new Dictionary<Type,DbType>();

      //入参
      PropertyInfo[] properties = model.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

      if (properties.Length <= 0)
      {
        return 0;
      }

      List<string> fieldList = new List<string>();
      List<string> valueList = new List<string>();
      if (table.Rows.Count > 1)
      {
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

          //BySystem，IncKey，IncPK不取，数据库自动处理
          if (Usage == EnumFieldUsage.BySystem || Usage == EnumFieldUsage.IncKey || Usage == EnumFieldUsage.IncPK)
            continue;


          string pv_name = "";
          object pv_value = null;

          foreach (PropertyInfo item in properties)
          {
            if (ClassName == item.Name)
            {
              //item.PropertyType.IsValueType ? Activator.CreateInstance(targetType) : null
              //判断是否是默认值
              if (item.PropertyType.IsValueType)
              {
                if (item.PropertyType == typeof(DateTime))
                  break;
                if (Usage == EnumFieldUsage.BySystem || Usage == EnumFieldUsage.IncKey || Usage == EnumFieldUsage.IncPK || Usage == EnumFieldUsage.PrimaryKey)
                  break;
                if (!string.IsNullOrEmpty(DefaultValue.ToString()))
                {
                  pv_name = Name;
                  pv_value = DefaultValue;
                  break;
                }
              }
              pv_name = Name;
              pv_value = item.GetValue(model, null);
              break;
            }
            //object value = item.GetValue(model, null);  //值
          }
          //if (myhash.Contains(Name.ToLower()))
          //{
          //  pv_name = Name;
          //}
          //if (myhash.Contains(ClassName.ToLower()))
          //{
          //  pv_name = Name;
          //}
          //if (!string.IsNullOrEmpty(pv_name))
          //{
          //  pv_value = myhash[pv_name];
          //}

          //Hashtable不存在
          if (string.IsNullOrEmpty(pv_name) && !string.IsNullOrEmpty(DefaultValue.ToString()))
          {
            pv_name = Name;
            if (Type == typeof(System.DateTime) && DefaultValue.ToString().ToLower() == "now")
              pv_value = DateTime.Now;
            else
              pv_value = DefaultValue;
          }


          #region 不允许为空值判断
          if (!Null)
          {
            if (string.IsNullOrEmpty(pv_name) || string.IsNullOrEmpty(pv_value.ToString()))
            {
              return -2;
            }
          }
          #endregion
          if (string.IsNullOrEmpty(pv_name))
          {
            continue;
          }

          DbParameter d = SqlHelper.MakeInParam(Prefix + Name, dbtype, Length, pv_value);
          if (Type == typeof(System.String) && Length != 0 && UrlEncode)
          {
            if (pv_value == null)
              pv_value = "";
            d = SqlHelper.MakeInParam(Prefix + Name, TypeToDb[Type], Length, pv_value.ToString().Replace("&", "&#38;").Replace("<", "&#60;").Replace(">", "&#62;"));
          }
          p.Add(d);

          fieldList.Add("[" + pv_name + "]");
          valueList.Add(Prefix + pv_name + "");
        }

        strSql.Append("insert into [" + TablePrefix + tableName + "] (");
        //strSql.Append(fieldBuilder.Remove(fieldBuilder.Length - 1, 1).ToString());
        strSql.Append(string.Join(" , ", fieldList.ToArray()));//.Remove(fieldBuilder.Length - 1, 1).ToString());
        strSql.Append(") values(");
        strSql.Append(string.Join(" , ", valueList.ToArray()));//strSql.Append(valueBuilder.Remove(valueBuilder.Length - 1, 1).ToString());
        strSql.Append(");SELECT @@identity");
        //System.Web.HttpContext.Current.Response.Write(strSql.ToString());
        return SqlHelper.ExecuteScalar(CommandType.Text, strSql.ToString(), p.ToArray());
      }
      return 0;
    }

    public object CreateRecord<T>(Hashtable myhash) where T : class, new()
    {

      DataTable table = new AttributeHelper<T>().getEntityTable();
      List<DbParameter> p = new List<DbParameter>();
      StringBuilder strSql = new StringBuilder();
      Dictionary<Type, DbType> TypeToDb = new Core().getDbType();//new Dictionary<Type,DbType>();

      List<string> fieldList = new List<string>();
      List<string> valueList = new List<string>();
      if (table.Rows.Count > 1)
      {
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

          //BySystem，IncKey，IncPK不取，数据库自动处理
          if (Usage == EnumFieldUsage.BySystem || Usage == EnumFieldUsage.IncKey || Usage == EnumFieldUsage.IncPK)
            continue;

          string pv_name = "";
          object pv_value = null;
          if (myhash.Contains(Name.ToLower()))
          {
            pv_name = Name;
          }
          if (myhash.Contains(ClassName.ToLower()))
          {
            pv_name = Name;
          }
          if (!string.IsNullOrEmpty(pv_name))
          {
            pv_value = myhash[pv_name];
          }

          //Hashtable不存在
          if (string.IsNullOrEmpty(pv_name) && !string.IsNullOrEmpty(DefaultValue.ToString()))
          {
            pv_name = Name;
            if (Type == typeof(System.DateTime) && DefaultValue.ToString().ToLower() == "now")
              pv_value = DateTime.Now;
            else
              pv_value = DefaultValue;
          }


          #region 不允许为空值判断
          if (!Null)
          {
            if (string.IsNullOrEmpty(pv_name) || string.IsNullOrEmpty(pv_value.ToString()))
            {
              return -2;
            }
          }
          #endregion
          if (string.IsNullOrEmpty(pv_name))
          {
            continue;
          }

          DbParameter d = SqlHelper.MakeInParam(Prefix + Name, dbtype, Length, pv_value);
          if (Type == typeof(System.String) && Length != 0 && UrlEncode)
          {
            d = SqlHelper.MakeInParam(Prefix + Name, TypeToDb[Type], Length, pv_value.ToString().Replace("&", "&#38;").Replace("<", "&#60;").Replace(">", "&#62;"));
          }
          p.Add(d);

          fieldList.Add("[" + pv_name + "]");
          valueList.Add(Prefix + pv_name + "");
        }

        strSql.Append("insert into [" + TablePrefix + tableName + "] (");
        //strSql.Append(fieldBuilder.Remove(fieldBuilder.Length - 1, 1).ToString());
        strSql.Append(string.Join(" , ", fieldList.ToArray()));//.Remove(fieldBuilder.Length - 1, 1).ToString());
        strSql.Append(") values(");
        strSql.Append(string.Join(" , ", valueList.ToArray()));//strSql.Append(valueBuilder.Remove(valueBuilder.Length - 1, 1).ToString());
        strSql.Append(");SELECT @@identity");
        //System.Web.HttpContext.Current.Response.Write(strSql.ToString());
        return SqlHelper.ExecuteScalar(CommandType.Text, strSql.ToString(), p.ToArray());
      }
      return 0;

      #region 旧代码废弃
      ////string tableName = getTableName<T>();
      //DataTable table = new AttributeHelper<T>().getEntityTable();
      //List<DbParameter> p = new List<DbParameter>();
      //StringBuilder strSql = new StringBuilder();
      //StringBuilder fieldBuilder = new StringBuilder();
      //StringBuilder valueBuilder = new StringBuilder();
      //Dictionary<Type, DbType> TypeToDb = new Core().getDbType();//new Dictionary<Type,DbType>();
      //if (table.Rows.Count > 1)
      //{
      //  Hashtable ht = new Hashtable(System.StringComparer.Create(System.Globalization.CultureInfo.CurrentCulture, true));
      //  foreach (DataRow dr in table.Rows)
      //  {
      //    String Name = dr["Name"].ToString();
      //    String ClassName = dr["ClassName"].ToString();
      //    Type Type = (Type)dr["Type"];
      //    Int32 Length = Convert.ToInt32(dr["Length"]);
      //    EnumFieldUsage Usage = (EnumFieldUsage)Convert.ToInt32(dr["Usage"]);
      //    object DefaultValue = dr["DefaultValue"];
      //    bool Null = Convert.ToBoolean(dr["Null"]);
      //    bool UrlEncode = Convert.ToBoolean(dr["UrlEncode"]);

      //    DbType dbtype = TypeToDb[Type];


      //    bool NameExists = myhash.Contains(Name.ToLower());
      //    bool ClassNameExists = myhash.Contains(ClassName.ToLower());

      //    String PvName = NameExists ? Name : ClassName;//如果传值不是表的字段名，则取类的字段名(用户获取传过来的值)

      //    if (Usage == EnumFieldUsage.BySystem || Usage == EnumFieldUsage.IncKey || Usage == EnumFieldUsage.IncPK)
      //      continue;


      //    #region 不允许为空值判断
      //    if (!Null)
      //    {
      //      if (!NameExists && !ClassNameExists)
      //      {
      //        return -2;
      //      }
      //    }
      //    #endregion

      //    #region 主键或者唯一键判断
      //    //object KeyValue = ((NameExists || ClassNameExists) ? myhash[PvName.ToLower()] : (DefaultValue == null ? "" : DefaultValue));
      //    //if (!Null)
      //    //{//不允许为空值判断
      //    //  if (string.IsNullOrEmpty(KeyValue.ToString()))
      //    //    return -2;
      //    //}
      //    //if (Usage == EnumFieldUsage.PrimaryKey || Usage == EnumFieldUsage.UniqueKey)
      //    //{//主键或者唯一键
      //    //  if (!string.IsNullOrEmpty(KeyValue.ToString()))
      //    //  {
      //    //    DbParameter[] p1 = new DbParameter[]{
      //    //      SqlHelper.MakeInParam(Prefix+Name,TypeToDb[Type],Length,KeyValue)
      //    //    };
      //    //    string sql = "select isnull(count(1),0) from [" + TablePrefix + tableName + "] where [" + Name + "]=" + Prefix + Name;
      //    //    if (Convert.ToInt32(SqlHelper.ExecuteScalar(CommandType.Text, sql, p1)) > 0)
      //    //    {
      //    //      return -1;
      //    //    }
      //    //  }
      //    //}
      //    #endregion

      //    if (!string.IsNullOrEmpty(DefaultValue.ToString()))
      //    {
      //      if (Type == typeof(System.DateTime) && DefaultValue.ToString().ToLower() == "now")
      //        DefaultValue = DateTime.Now;
      //      DbParameter d = SqlHelper.MakeInParam(Prefix + Name, dbtype, Length, DefaultValue);
      //      //if (Type == typeof(System.String) && Length <= 200 && Length != 0)
      //      if (Type == typeof(System.String) && Length != 0 && UrlEncode)
      //      {
      //        d = SqlHelper.MakeInParam(Prefix + Name, TypeToDb[Type], Length, DefaultValue.ToString().Replace("&", "&#38;").Replace("<", "&#60;").Replace(">", "&#62;"));
      //      }
      //      p.Add(d);
      //      if (!ht.Contains(Name))
      //      {
      //        ht.Add(Name, d);
      //      }
      //    }

      //    object colValue = null;
      //    if (NameExists || ClassNameExists)
      //    {
      //      colValue = myhash[PvName.ToLower()];

      //      //主键为空或者为0，调整
      //      if (Usage == EnumFieldUsage.PrimaryKey)
      //      {
      //        if (string.IsNullOrEmpty(colValue.ToString()) || colValue.ToString() == "0")
      //          continue;
      //      }


      //      if (!Null)
      //      {//不允许为空值判断
      //        if (string.IsNullOrEmpty(colValue.ToString()))
      //          return -2;
      //      }

      //      if (ht.Contains(Name))
      //      {
      //        p.Remove((DbParameter)ht[Name]);
      //        ht.Remove(Name);
      //      }
      //      DbParameter d = SqlHelper.MakeInParam(Prefix + Name, dbtype, Length, colValue);
      //      //if (Type == typeof(System.String) && Length <= 200 && Length != 0)
      //      if (Type == typeof(System.String) && Length != 0 && UrlEncode)
      //      {
      //        d = SqlHelper.MakeInParam(Prefix + Name, TypeToDb[Type], Length, colValue.ToString().Replace("&", "&#38;").Replace("<", "&#60;").Replace(">", "&#62;"));
      //      }
      //      p.Add(d);
      //    }

      //    if (NameExists || ClassNameExists || !string.IsNullOrEmpty(DefaultValue.ToString()))
      //    {
      //      fieldBuilder.Append("[" + Name + "],");
      //      valueBuilder.Append(Prefix + Name + ",");
      //    }
      //  }

      //  strSql.Append("insert into [" + TablePrefix + tableName + "](");
      //  strSql.Append(fieldBuilder.Remove(fieldBuilder.Length - 1, 1).ToString());
      //  strSql.Append(") values(");
      //  strSql.Append(valueBuilder.Remove(valueBuilder.Length - 1, 1).ToString());
      //  strSql.Append(");SELECT @@identity");
      //  return SqlHelper.ExecuteScalar(CommandType.Text, strSql.ToString(), p.ToArray());
      //}
      //return 0;
      #endregion
    }


    public object UpdateRecord<T>(Hashtable myhash) where T : class, new()
    {

      DataTable table = new AttributeHelper<T>().getEntityTable();
      List<DbParameter> p = new List<DbParameter>();
      StringBuilder strSql = new StringBuilder();
      //StringBuilder fieldBuilder = new StringBuilder();
      Dictionary<Type, DbType> TypeToDb = new Core().getDbType();//new Dictionary<Type,DbType>();

      List<string> fieldList = new List<string>();
      List<string> updateList = new List<string>();

      object result = null;
      foreach (DictionaryEntry de in myhash)
      {
        string keys = de.Key.ToString();
        object colum_Value = de.Value;
        var Rows = table.AsEnumerable().Where(n => (n.Field<string>("Name").ToLower() == keys.ToLower() || n.Field<string>("ClassName").ToLower() == keys.ToLower())).Select(n => new { Name = n.Field<string>("Name"), ClassName = n.Field<string>("ClassName"), Explain = n.Field<string>("Explain"), Type = n.Field<System.Type>("Type"), Length = n.Field<int>("Length"), Usage = n.Field<int>("Usage"), DefaultValue = n.Field<object>("DefaultValue"), Null = n.Field<bool>("Null"), UrlEncode = n.Field<bool>("UrlEncode") }).ToList();

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
          if (Row.Usage == (int)EnumFieldUsage.IncKey || Row.Usage == (int)EnumFieldUsage.IncPK || Row.Usage == (int)EnumFieldUsage.PrimaryKey)
          {
            updateList.Add("[" + Row.Name + "] = " + Prefix + "" + Row.Name);
            result = colum_Value;
          }
          else
            fieldList.Add("[" + Row.Name + "] = " + Prefix + "" + Row.Name);
          p.Add(d);
        }
      }
      if (fieldList.Count > 0)
      {
        strSql.Append("update [" + TablePrefix + tableName + "] set ");
        strSql.Append(string.Join(" , ", fieldList.ToArray()));
        if (updateList.Count > 0)
        {
          strSql.Append(" where " + string.Join(" and ", updateList.ToArray()));
        }
        SqlHelper.ExecuteNonQuery(strSql.ToString(), p.ToArray());
      }
      return result;

      #region 抛弃旧方法
      ////string tableName = getTableName<T>();
      //if (table.Rows.Count > 1)
      //{
      //  string PkName = ""; Type PkType = typeof(System.Int32); Int32 PkLength = 0; object PkValue = null;
      //  foreach (DataRow dr in table.Rows)
      //  {
      //    String Name = dr["Name"].ToString();
      //    String ClassName = dr["ClassName"].ToString();
      //    Type Type = (Type)dr["Type"];
      //    Int32 Length = Convert.ToInt32(dr["Length"]);
      //    bool UrlEncode = Convert.ToBoolean(dr["UrlEncode"]);
      //    EnumFieldUsage Usage = (EnumFieldUsage)Convert.ToInt32(dr["Usage"]);
      //    if (Usage == EnumFieldUsage.IncKey || Usage == EnumFieldUsage.IncPK || Usage == EnumFieldUsage.PrimaryKey)
      //    {
      //      bool NameExists = myhash.Contains(Name.ToLower());
      //      bool ClassNameExists = myhash.Contains(ClassName.ToLower());
      //      if (NameExists || ClassNameExists)
      //      {
      //        PkName = NameExists ? Name : ClassName;
      //        PkValue = Convert.ChangeType(myhash[PkName.ToLower()], Type);
      //        PkName = Name;
      //        PkType = Type;
      //        PkLength = Length;

      //        DbParameter d = SqlHelper.MakeInParam(Prefix + PkName, TypeToDb[PkType], PkLength, PkValue);
      //        //if (Type == typeof(System.String) && Length <= 200 && Length != 0)
      //        if (Type == typeof(System.String) && Length != 0 && UrlEncode)
      //        {
      //          d = SqlHelper.MakeInParam(Prefix + PkName, TypeToDb[PkType], PkLength, PkValue.ToString().Replace("&", "&#38;").Replace("<", "&#60;").Replace(">", "&#62;"));
      //        }
      //        p.Add(d);

      //        break;
      //      }
      //    }
      //  }

      //  foreach (DataRow dr in table.Rows)
      //  {
      //    String Name = dr["Name"].ToString();
      //    String ClassName = dr["ClassName"].ToString();
      //    Type Type = (Type)dr["Type"];
      //    Int32 Length = Convert.ToInt32(dr["Length"]);
      //    EnumFieldUsage Usage = (EnumFieldUsage)Convert.ToInt32(dr["Usage"]);
      //    bool Null = Convert.ToBoolean(dr["Null"]);
      //    bool UrlEncode = Convert.ToBoolean(dr["UrlEncode"]);

      //    if (Usage == EnumFieldUsage.BySystem || Usage == EnumFieldUsage.IncKey || Usage == EnumFieldUsage.IncPK)
      //      continue;

      //    object colValue = null;
      //    bool NameExists = myhash.Contains(Name.ToLower());
      //    bool ClassNameExists = myhash.Contains(ClassName.ToLower());


      //    #region 不允许为空值判断
      //    if (!Null)
      //    {
      //      if (!NameExists && !ClassNameExists)
      //      {
      //        return -2;
      //      }
      //    }
      //    #endregion

      //    if (NameExists || ClassNameExists)
      //    {
      //      String ValueName = NameExists ? Name : ClassName;

      //      if (PkName.ToLower() == Name.ToLower())
      //        continue;
      //      colValue = Convert.ChangeType(myhash[ValueName.ToLower()], Type);//myhash[Name.ToLower()];

      //      if (!Null)
      //      {//不允许为空值判断
      //        if (string.IsNullOrEmpty(colValue.ToString()))
      //          return -2;
      //      }

      //      #region 唯一键判断
      //      //if (Usage == EnumFieldUsage.PrimaryKey || Usage == EnumFieldUsage.UniqueKey)
      //      //{//主键或者唯一键
      //      //  if (!string.IsNullOrEmpty(colValue.ToString()))
      //      //  {
      //      //    DbParameter[] p1 = new DbParameter[]{
      //      //      SqlHelper.MakeInParam(Prefix+Name,TypeToDb[Type],Length,colValue),
      //      //      SqlHelper.MakeInParam(Prefix+PkName,TypeToDb[PkType],PkLength,PkValue)
      //      //    };
      //      //    string sql = "select isnull(count(1),0) from [" + TablePrefix + tableName + "] where [" + Name + "]=" + Prefix + Name + " and [" + PkName + "]<>" + Prefix + PkName;
      //      //    if (Convert.ToInt32(SqlHelper.ExecuteScalar(CommandType.Text, sql, p1)) > 0)
      //      //    {
      //      //      return -1;
      //      //    }
      //      //  }
      //      //}
      //      #endregion

      //      DbParameter d = SqlHelper.MakeInParam(Prefix + Name, TypeToDb[Type], Length, colValue);
      //      if (Type == typeof(System.String) && Length != 0 && UrlEncode)
      //      {
      //        d = SqlHelper.MakeInParam(Prefix + Name, TypeToDb[Type], Length, colValue.ToString().Replace("&", "&#38;").Replace("<", "&#60;").Replace(">", "&#62;"));
      //      }
      //      p.Add(d);

      //      fieldBuilder.Append("[" + Name + "] = " + Prefix + "" + Name + ",");
      //    }
      //  }

      //  strSql.Append("update [" + TablePrefix + tableName + "] set ");
      //  strSql.Append(fieldBuilder.Remove(fieldBuilder.Length - 1, 1).ToString());
      //  strSql.Append(" where [" + PkName + "]=" + Prefix + PkName);
      //  SqlHelper.ExecuteNonQuery(strSql.ToString(), p.ToArray());
      //  return PkValue;
      //}
      //return 0;
      #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="myhash"></param>
    /// <param name="update_hash"></param>
    /// <returns></returns>
    public void UpdateRecord<T>(Hashtable myhash, Hashtable update_hash) where T : class,new()
    {
      DataTable table = new AttributeHelper<T>().getEntityTable();
      List<DbParameter> p = new List<DbParameter>();
      StringBuilder strSql = new StringBuilder();
      List<string> fieldBuilder = new List<string>();
      Dictionary<Type, DbType> TypeToDb = new Core().getDbType();
      List<string> UpdateList = new List<string>();

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
          fieldBuilder.Add("[" + Row.Name + "] = " + Prefix + "" + Row.Name);
        }
      }

      if (update_hash.Count > 0)
      {
        foreach (DictionaryEntry de in update_hash)
        {
          string keys = de.Key.ToString();
          object colum_Value = de.Value;
          var Rows = table.AsEnumerable().Where(n => (n.Field<string>("Name").ToLower() == keys.ToLower() || n.Field<string>("ClassName").ToLower() == keys.ToLower())).Select(n => new { Name = n.Field<string>("Name"), ClassName = n.Field<string>("ClassName"), Explain = n.Field<string>("Explain"), Type = n.Field<System.Type>("Type"), Length = n.Field<int>("Length"), Usage = n.Field<int>("Usage"), DefaultValue = n.Field<object>("DefaultValue"), Null = n.Field<bool>("Null"), UrlEncode = n.Field<bool>("UrlEncode") }).ToList();
          if (Rows.Count > 0)
          {
            var Row = Rows[0];
            //var colum_Value = (myhash.Contains(Row.Name.ToLower()) ? myhash[Row.Name] : myhash[Row.ClassName]);
            DbParameter d = SqlHelper.MakeInParam(Prefix + Row.Name, TypeToDb[Row.Type], Row.Length, colum_Value);
            if (Row.Type == typeof(System.String) && Row.Length != 0 && Row.UrlEncode)
            {
              d = SqlHelper.MakeInParam(Prefix + Row.Name, TypeToDb[Row.Type], Row.Length, colum_Value.ToString().Replace("&", "&#38;").Replace("<", "&#60;").Replace(">", "&#62;"));
            }
            p.Add(d);
            UpdateList.Add("[" + Row.Name + "] = " + Prefix + "" + Row.Name);
          }
        }
      }

      if (fieldBuilder.Count > 0)
      {
        strSql.Append("update [" + TablePrefix + tableName + "] set ");
        strSql.Append(string.Join(" , ", fieldBuilder.ToArray()));
        if (UpdateList.Count > 0)
        {
          strSql.Append(" where " + string.Join(" and ", UpdateList.ToArray()));
        }
        SqlHelper.ExecuteNonQuery(strSql.ToString(), p.ToArray());
      }

    }

    public void DeleteRecord<T>(object Code) where T : class, new()
    {
      //string tableName = getTableName<T>();
      DataTable table = new AttributeHelper<T>().getEntityTable();
      Dictionary<Type, DbType> TypeToDb = new Core().getDbType();
      List<DbParameter> p = new List<DbParameter>();
      if (table.Rows.Count > 1)
      {
        string PkName = ""; Type PkType = typeof(System.Int32); Int32 PkLength = 0; object PkValue = null;
        foreach (DataRow dr in table.Rows)
        {
          String Name = dr["Name"].ToString();
          Type Type = (Type)dr["Type"];
          Int32 Length = Convert.ToInt32(dr["Length"]);
          EnumFieldUsage Usage = (EnumFieldUsage)Convert.ToInt32(dr["Usage"]);
          bool UrlEncode = Convert.ToBoolean(dr["UrlEncode"]);
          if (Usage == EnumFieldUsage.IncKey || Usage == EnumFieldUsage.IncPK)
          {
            PkValue = Code;
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
        StringBuilder strSql = new StringBuilder();
        strSql.Append("delete from " + TablePrefix + tableName + " ");
        strSql.Append(" where [" + PkName + "]=" + Prefix + PkName);
        SqlHelper.ExecuteNonQuery(strSql.ToString(), p.ToArray());
      }
    }

    public void DeleteRecord<T>(Expression<Func<T, bool>> where) where T : class, new()
    {
      string strwhere = (new T()).Where(where);
      if (!string.IsNullOrEmpty(strwhere))
      {
        //string tableName = getTableName<T>();
        StringBuilder strSql = new StringBuilder();
        strSql.Append("delete from " + TablePrefix + tableName + " ");
        strSql.Append(" where " + strwhere);
        SqlHelper.ExecuteNonQuery(strSql.ToString());
      }
    }

    public void DeleteRecord<T>(string strwhere) where T : class, new()
    {
      if (!string.IsNullOrEmpty(strwhere))
      {
        //string tableName = getTableName<T>();
        StringBuilder strSql = new StringBuilder();
        strSql.Append("delete from " + TablePrefix + tableName + " ");
        strSql.Append(" where " + strwhere);
        SqlHelper.ExecuteNonQuery(strSql.ToString());
      }
    }

    public void DeleteRecord<T>(string strwhere, Hashtable myhash) where T : class, new()
    {
      try
      {
        if (myhash == null)
          DeleteRecord<T>(strwhere);
        string mysql = "delete from {0}";
        if (strwhere.Trim() != "")
          mysql += " where " + strwhere;
        string sqlString = string.Format(mysql, TablePrefix + tableName);
        List<DbParameter> p = new Core().getDbParameter<T>(myhash, Prefix);
        if (p.Count == 0)
          DeleteRecord<T>(strwhere);
        SqlHelper.ExecuteNonQuery(sqlString, p.ToArray());
      }
      catch
      {
      }
    }

    public bool Exists<T>(string strwhere) where T : class, new()
    {
      try
      {
        //string tableName = getTableName<T>();
        string mysql = "select count(1) from {0}";
        if (strwhere.Trim() != "")
          mysql += " where " + strwhere;
        object total = SqlHelper.ExecuteScalar(string.Format(mysql, TablePrefix + tableName));
        return (Convert.ToInt32(total) != 0);
      }
      catch
      {
        return false;
      }
    }

    public bool Exists<T>(string strwhere, Hashtable myhash) where T : class, new()
    {
      try
      {
        //string tableName = getTableName<T>();
        string mysql = "select count(1) from {0}";
        if (strwhere.Trim() != "")
          mysql += " where " + strwhere;
        string sqlString = string.Format(mysql, TablePrefix + tableName);
        List<DbParameter> p = new Core().getDbParameter<T>(myhash, Prefix);
        if (p.Count == 0)
          return Exists<T>(strwhere);
        object total = SqlHelper.ExecuteScalar(sqlString, p.ToArray());
        return (Convert.ToInt32(total) != 0);
      }
      catch
      {
        return false;
      }
    }

    public object GetColObject<T>(string Col, string strWhere) where T : class, new()
    {
      try
      {
        //string tableName = getTableName<T>();
        string mysql = "select " + Col + " from " + TablePrefix + tableName + "";
        if (strWhere != "")
          mysql += " where " + strWhere;
        return SqlHelper.ExecuteScalar(mysql);
      }
      catch
      {
        return (object)"";
      }
    }

    public int GetTableTotal<T>(string strwhere) where T : class, new()
    {
      try
      {
        //string tableName = getTableName<T>();
        string mysql = "select isnull(count(1),0) from " + TablePrefix + tableName + "";
        if (strwhere != "")
          mysql += " where " + strwhere;
        return Convert.ToInt32(SqlHelper.ExecuteScalar(mysql));
      }
      catch
      {
        return 0;
      }
    }

    public int GetTableSum<T>(string col, string strWhere) where T : class, new()
    {
      try
      {
        //string tableName = getTableName<T>();
        string mysql = "select isnull(sum([" + col + "]),0) from " + TablePrefix + tableName + "";
        if (strWhere != "")
          mysql += " where " + strWhere;
        return Convert.ToInt32(SqlHelper.ExecuteScalar(mysql));
      }
      catch
      {
        return 0;
      }
    }

    public void UpdateByCol<T>(string Col, string Value, string CodeCol, int code) where T : class, new()
    {
      //string tableName = getTableName<T>();
      StringBuilder strSql = new StringBuilder();
      strSql.Append("update " + TablePrefix + tableName + " set ");
      strSql.Append("[" + Col + "]='" + Value + "'");
      strSql.Append(" where [" + CodeCol + "]=" + code);
      SqlHelper.ExecuteNonQuery(strSql.ToString());
    }

    public void UpdateByCol<T>(string[] Col, string[] Value, string CodeCol, int code) where T : class, new()
    {
      //string tableName = getTableName<T>();
      StringBuilder strSql = new StringBuilder();
      strSql.Append("update " + TablePrefix + tableName + " set ");
      int length = Math.Min(Col.Length, Value.Length);
      for (int i = 0; i < length - 1; i++)
      {
        strSql.Append("[" + Col[i] + "]='" + Value[i] + "',");
      }
      strSql.Append("[" + Col[length - 1] + "]='" + Value[length - 1] + "'");
      strSql.Append(" where [" + CodeCol + "]=" + code);
      SqlHelper.ExecuteNonQuery(strSql.ToString());
    }

    public void UpdateByCol<T>(string Col, string Value, string CodeCol, string code) where T : class, new()
    {
      //string tableName = getTableName<T>();
      StringBuilder strSql = new StringBuilder();
      strSql.Append("update " + TablePrefix + tableName + " set ");
      strSql.Append("[" + Col + "]='" + Value + "'");
      strSql.Append(" where [" + CodeCol + "] in(" + code + ")");
      SqlHelper.ExecuteNonQuery(strSql.ToString());
    }

    public void UpdateByCol<T>(string[] Col, string[] Value, string CodeCol, string code) where T : class, new()
    {
      //string tableName = getTableName<T>();
      StringBuilder strSql = new StringBuilder();
      strSql.Append("update " + TablePrefix + tableName + " set ");
      int length = Math.Min(Col.Length, Value.Length);
      for (int i = 0; i < length - 1; i++)
      {
        strSql.Append("[" + Col[i] + "]='" + Value[i] + "',");
      }
      strSql.Append("[" + Col[length - 1] + "]='" + Value[length - 1] + "'");
      strSql.Append(" where [" + CodeCol + "] in(" + code + ")");
      SqlHelper.ExecuteNonQuery(strSql.ToString());
    }

    public void Inc<T>(string ColName, int IncValue, string CodeCol, string code) where T : class, new()
    {
      //string tableName = getTableName<T>();
      StringBuilder strSql = new StringBuilder();
      strSql.Append("update " + TablePrefix + tableName + " set ");
      strSql.Append("[" + ColName + "]=isnull([" + ColName + "],0)+" + IncValue);
      strSql.Append(" where [" + CodeCol + "] in(" + code + ")");
      SqlHelper.ExecuteNonQuery(strSql.ToString());
    }

    public void Inc<T>(string ColName, int IncValue, string strwhere) where T : class, new()
    {
      //string tableName = getTableName<T>();
      StringBuilder strSql = new StringBuilder();
      strSql.Append("update " + TablePrefix + tableName + " set ");
      strSql.Append("[" + ColName + "]=isnull([" + ColName + "],0)+" + IncValue);
      if (string.IsNullOrEmpty(strwhere))
        strSql.Append(" where " + strwhere);
      SqlHelper.ExecuteNonQuery(strSql.ToString());
    }

    public DataSet FindList<T>(string strWhere, string order) where T : class, new()
    {
      //string tableName = getTableName<T>();
      StringBuilder strSql = new StringBuilder();
      strSql.Append("select * from " + TablePrefix + tableName);
      if (strWhere.Trim() != "")
      {
        strSql.Append(" where " + strWhere);
      }
      if (order.Trim() != "")
        strSql.Append(" order by " + order);
      DataSet result = SqlHelper.ExecuteDataset(strSql.ToString());
      return result;
    }

    public DataSet FindList<T>(string Column, string strWhere, string order) where T : class, new()
    {
      //string tableName = getTableName<T>(); 
      StringBuilder strSql = new StringBuilder();
      strSql.Append("select " + (Column == "" ? "*" : Column) + " from " + TablePrefix + tableName);
      if (strWhere.Trim() != "")
      {
        strSql.Append(" where " + strWhere);
      }
      if (order.Trim() != "")
        strSql.Append(" order by " + order);
      DataSet result = SqlHelper.ExecuteDataset(strSql.ToString());
      return result;
    }

    public DataSet FindByPage<T>(string FieldKey, int PageSize, int PageIndex, string strWhere, string Column, string FieldOrder) where T : class, new()
    {
      //string tableName = getTableName<T>(); 
      DataSet ds = new DataSet();
      DbParameter[] p ={
                    SqlHelper.MakeInParam("@tbname",DbType.String,255,TablePrefix + tableName),
                    SqlHelper.MakeInParam("@FieldKey",DbType.String,255,(FieldKey==""?"Code":FieldKey)),
                    SqlHelper.MakeInParam("@PageCurrent",DbType.Int32,4,PageIndex),
                    SqlHelper.MakeInParam("@PageSize",DbType.Int32,4,PageSize),
                    SqlHelper.MakeInParam("@FieldShow",DbType.String,255,(Column==""?"*":Column)),
                    SqlHelper.MakeInParam("@FieldOrder",DbType.String,255,(FieldOrder==""?"Addtime desc":FieldOrder)),
                    SqlHelper.MakeInParam("@Where",DbType.String,0,strWhere),
                    SqlHelper.MakeOutParam("@RecordCount",DbType.Int32,4),
                    SqlHelper.MakeOutParam("@PageCount",DbType.Int32,4)
									 };
      ds = SqlHelper.ExecuteDataset(CommandType.StoredProcedure, "Aspx_PageList", p);
      return ds;
    }

    public DataSet FindByPage<T>(string FieldKey, int PageSize, int PageIndex, Expression<Func<T, bool>> where, string Column, string FieldOrder) where T : class, new()
    {
      //string tableName = getTableName<T>(); 
      string strWhere = (new T()).Where(where);
      System.Web.HttpContext.Current.Response.Write(strWhere);
      DataSet ds = new DataSet();
      DbParameter[] p ={
                    SqlHelper.MakeInParam("@tbname",DbType.String,255,TablePrefix + tableName),
                    SqlHelper.MakeInParam("@FieldKey",DbType.String,255,(FieldKey==""?"Code":FieldKey)),
                    SqlHelper.MakeInParam("@PageCurrent",DbType.Int32,4,PageIndex),
                    SqlHelper.MakeInParam("@PageSize",DbType.Int32,4,PageSize),
                    SqlHelper.MakeInParam("@FieldShow",DbType.String,255,(Column==""?"*":Column)),
                    SqlHelper.MakeInParam("@FieldOrder",DbType.String,255,(FieldOrder==""?"Addtime desc":FieldOrder)),
                    SqlHelper.MakeInParam("@Where",DbType.String,0,strWhere),
                    SqlHelper.MakeOutParam("@RecordCount",DbType.Int32,4),
                    SqlHelper.MakeOutParam("@PageCount",DbType.Int32,4)
									 };
      ds = SqlHelper.ExecuteDataset(CommandType.StoredProcedure, "Aspx_PageList", p);
      return ds;
    }

    public DataSet FindByPage<T>(string FieldKey, int PageSize, int PageIndex, string strWhere, string Column, string FieldOrder, ref int RecordCount) where T : class, new()
    {
      //string tableName = getTableName<T>(); 
      DataSet ds = new DataSet();
      DbParameter[] p ={
                    SqlHelper.MakeInParam("@tbname",DbType.String,255,TablePrefix + tableName),
                    SqlHelper.MakeInParam("@FieldKey",DbType.String,255,(FieldKey==""?"Code":FieldKey)),
                    SqlHelper.MakeInParam("@PageCurrent",DbType.Int32,4,PageIndex),
                    SqlHelper.MakeInParam("@PageSize",DbType.Int32,4,PageSize),
                    SqlHelper.MakeInParam("@FieldShow",DbType.String,255,(Column==""?"*":Column)),
                    SqlHelper.MakeInParam("@FieldOrder",DbType.String,255,(FieldOrder==""?"Addtime desc":FieldOrder)),
                    SqlHelper.MakeInParam("@Where",DbType.String,0,strWhere),
                    SqlHelper.MakeOutParam("@RecordCount",DbType.Int32,4),
                    SqlHelper.MakeOutParam("@PageCount",DbType.Int32,4)
									 };
      ds = SqlHelper.ExecuteDataset(CommandType.StoredProcedure, "Aspx_PageList", p);
      RecordCount = int.Parse(p[7].Value.ToString());
      return ds;
    }

    public T TryFind<T>(string column, string where) where T : class, new()
    {
      //string tableName = getTableName<T>(); 
      StringBuilder strSql = new StringBuilder();
      strSql.Append("select top 1 " + (string.IsNullOrEmpty(column) ? "*" : column) + " from " + TablePrefix + tableName);
      if (where.Trim() != "")
      {
        strSql.Append(" where " + where);
      }
      DataTable dt = SqlHelper.ExecuteDataset(strSql.ToString()).Tables[0];

      if (dt.Rows.Count == 0)
      {
        T _t = default(T);
        return _t;
      }
      else
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
              if (dt.Rows[0][j] != DBNull.Value)
              {
                info.SetValue(_t, dt.Rows[0][j], null);
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

    public T TryFind<T>(string where) where T : class, new()
    {
      //string tableName = getTableName<T>(); 
      StringBuilder strSql = new StringBuilder();
      strSql.Append("select top 1 * from " + TablePrefix + tableName);
      if (where.Trim() != "")
      {
        strSql.Append(" where " + where);
      }
      DataTable dt = SqlHelper.ExecuteDataset(strSql.ToString()).Tables[0];

      if (dt.Rows.Count == 0)
      {
        T _t = default(T);
        return _t;
      }
      else
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
              if (dt.Rows[0][j] != DBNull.Value)
              {
                info.SetValue(_t, dt.Rows[0][j], null);
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

    public T TryFind<T>(string column, string where, Hashtable table) where T : class, new()
    {
      //string tableName = getTableName<T>(); 
      List<DbParameter> p = new Core().getDbParameter<T>(table, Prefix);
      if (p.Count == 0)
        return TryFind<T>(where);

      StringBuilder strSql = new StringBuilder();
      strSql.Append("select top 1 " + (string.IsNullOrEmpty(column) ? "*" : column) + " from " + TablePrefix + tableName);
      if (where.Trim() != "")
      {
        strSql.Append(" where " + where);
      }
      DataTable dt = SqlHelper.ExecuteDataset(strSql.ToString(), p.ToArray()).Tables[0];

      if (dt.Rows.Count == 0)
      {
        T _t = default(T);
        return _t;
      }
      else
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
              if (dt.Rows[0][j] != DBNull.Value)
              {
                info.SetValue(_t, dt.Rows[0][j], null);
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

    public T TryFind<T>(string where, Hashtable table) where T : class, new()
    {
      //string tableName = getTableName<T>(); 
      List<DbParameter> p = new Core().getDbParameter<T>(table, Prefix);
      if (p.Count == 0)
        return TryFind<T>(where);

      StringBuilder strSql = new StringBuilder();
      strSql.Append("select top 1 * from " + TablePrefix + tableName);
      if (where.Trim() != "")
      {
        strSql.Append(" where " + where);
      }
      DataTable dt = SqlHelper.ExecuteDataset(strSql.ToString(), p.ToArray()).Tables[0];

      if (dt.Rows.Count == 0)
      {
        T _t = default(T);
        return _t;
      }
      else
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
              if (dt.Rows[0][j] != DBNull.Value)
              {
                info.SetValue(_t, dt.Rows[0][j], null);
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

    public List<T> ToList<T>(DataSet table) where T : class, new()
    {
      //string tableName = getTableName<T>(); 
      DataTable dt = table.Tables[0];
      if (dt.Rows.Count == 0)
      {
        List<T> _t = default(List<T>);
        return _t;
      }
      else
      {
        List<T> list = new List<T>();

        for (int i = 0; i < dt.Rows.Count; i++)
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
          list.Add(_t);
        }
        return list;
      }
    }

    public object FindMaxColumn<T>(string Column, string where) where T : class, new()
    {
      //string tableName = getTableName<T>(); 
      StringBuilder strSql = new StringBuilder();
      strSql.Append("select isnull(max([" + Column + "]),0) from " + TablePrefix + tableName);
      if (where.Trim() != "")
      {
        strSql.Append(" where " + where);
      }
      return SqlHelper.ExecuteScalar(strSql.ToString());
    }

    public int Procedure(string ProcName, DbParameter[] p)
    {
      return SqlHelper.ExecuteNonQuery(CommandType.StoredProcedure, ProcName, p);
    }

    public int ProcedureByErr(string ProcName, DbParameter[] p, ref string ErrString)
    {
      return SqlHelper.ExecuteNonQueryByErr(CommandType.StoredProcedure, ProcName, p, ref ErrString);
    }

    public DataSet ProDataSet(string ProcName, DbParameter[] p)
    {
      return SqlHelper.ExecuteDataset(CommandType.StoredProcedure, ProcName, p);
    }

    public DataSet Query(string sql)
    {
      return SqlHelper.ExecuteDataset(sql);
    }

    public DataSet Query(string sql, DbParameter[] p)
    {
      return SqlHelper.ExecuteDataset(sql, p);
    }

    public int ExecuteNonQuery(string sql, DbParameter[] p)
    {
      return SqlHelper.ExecuteNonQuery(sql, p);
    }

    public int ExecuteNonQuery<T>(string sql, Hashtable table) where T : class, new()
    {
      if (table != null && table.Count > 0)
      {
        DbParameter[] p = new Core().getDbParameter<T>(table, Prefix).ToArray();
        return SqlHelper.ExecuteNonQuery(sql, p);
      }
      return SqlHelper.ExecuteNonQuery(sql);
    }

    public DataSet FindSetBySql(string tableName, string ColName, string OrderString, string strWhere, DbParameter[] para, int page, int PageSize)
    {
      if (string.IsNullOrEmpty(tableName))
      {
        tableName = TablePrefix + this.tableName;
      }
      return SqlHelper.FindSetBySql(tableName, ColName, OrderString, strWhere, para, page, PageSize);
    }

    public DataSet FindSetBySql<T>(string tableName, string ColName, string OrderString, string strWhere, Hashtable table, int page, int PageSize) where T : class, new()
    {
      if (string.IsNullOrEmpty(tableName))
      {
        tableName = TablePrefix + this.tableName;
      }
      DbParameter[] p = null;
      if (table != null && table.Count != 0)
        p = new Core().getDbParameter<T>(table, Prefix).ToArray();
      return SqlHelper.FindSetBySql(tableName, ColName, OrderString, strWhere, p, page, PageSize);
    }

    /// <summary>
    /// 生成database
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void SetTable<T>() where T : class, new()
    {

      DataTable table = new AttributeHelper<T>().getEntityTable();
      StringBuilder sqlBuilder = new StringBuilder();
      sqlBuilder.Append("if object_id(N'" + TablePrefix + tableName + "',N'U') is not null");
      sqlBuilder.Append(" begin");
      sqlBuilder.Append("CREATE TABLE [dbo].[" + TablePrefix + tableName + "] (");
      string alterString = "";
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
        //var type = typeof(decimal);
        string type = "nvachar";
        if ((Usage == EnumFieldUsage.PrimaryKey) || (Usage == EnumFieldUsage.IncPK))
        {
          alterString = "ALTER TABLE [dbo].[" + TablePrefix + tableName + "] ADD PRIMARY KEY ([" + Name + "])";
        }
        if (Type == typeof(int))
          type = "int";
        else if (Type == typeof(decimal))
          type = "decimal(" + Length + ",2)";
        else if (Type == typeof(Int64))
          type = "bigint";
        else if (Type == typeof(DateTime))
          type = "datetime";
        else if (Type == typeof(float))
          type = "float(53)";
        else
          type = "nvarchar(" + Length + ")";

        sqlBuilder.Append("[" + Name + "] " + type + " " + (Null ? "NULL" : "NOT NULL") + " " + (((Usage == EnumFieldUsage.IncKey) || (Usage==EnumFieldUsage.IncPK)) ? "IDENTITY(1,1)" : "") + ",");
      }
      sqlBuilder.Append(" end");

    }

    private string Prefix
    {
      get
      {
        return "@";
      }
    }

    //private string getTableName<T>() where T : class,new()
    //{
    //  string tableName = new T().GetType().Name;
    //  string tmpTable = TableAttributeHelper<T>.TableName();
    //  if (!string.IsNullOrEmpty(tmpTable))
    //    tableName = tmpTable;
    //  return tableName;
    //}

    public List<DbParameter> DbParameter(Dictionary<string, object> dict)
    {
      List<DbParameter> p = new List<DbParameter>();
      foreach (KeyValuePair<string, object> kv in dict)
      {
        DbParameter d = SqlHelper.MakeObjectParam(kv.Key, kv.Value);
        p.Add(d);
      }
      return p;
    }


  }
}
