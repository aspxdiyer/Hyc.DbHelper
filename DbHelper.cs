﻿using System;
using System.Collections.Generic;
using System.Text;
using AspxFrameWork.DataHelper;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace Hyc.DbDriver
{
  public class DbHelper<Entity> where Entity : class,new()
  {
    /// <summary>
    /// 数据库连接字符串
    /// </summary>
    //public String DBConnectString{get;set;}

    /// <summary>
    /// 数据库类型
    /// </summary>
    //public String DbProvider{get;set;}

    /// <summary>
    /// 表前缀
    /// </summary>
    public String TablePrefix { get; set; }

    private string tableName = "";

    public DbHelper(string DBConnectString, string DbProvider, string TablePrefix)
    {
      this.TablePrefix = TablePrefix;
      tableName = new Entity().GetType().Name;
      string tmpTable = TableAttributeHelper<Entity>.TableName();
      if (!string.IsNullOrEmpty(tmpTable))
        tableName = tmpTable;

      SqlHelper.AspxConnection = DBConnectString;
      SqlHelper.AspxProvider = DbProvider;
    }

    public object CreateRecord(Hashtable myhash)
    {
      DataTable table = new AttributeHelper<Entity>().getEntityTable();
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

    public object UpdateRecord(Hashtable myhash)
    {
      DataTable table = new AttributeHelper<Entity>().getEntityTable();
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

    /// <summary>
    /// 根据主键删除记录
    /// </summary>
    /// <param name="Code"></param>
    public void DeleteRecord(object Code)
    {
      DataTable table = new AttributeHelper<Entity>().getEntityTable();
      Dictionary<Type, DbType> TypeToDb = getDbType();
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
          if (Usage == EnumFieldUsage.IncKey || Usage == EnumFieldUsage.IncPK)
          {
            PkValue = Code;
            PkName = Name;
            PkType = Type;
            PkLength = Length;

            DbParameter d = SqlHelper.MakeInParam(Prefix + PkName, TypeToDb[PkType], PkLength, PkValue);
            if (Type == typeof(System.String) && Length <= 200 && Length != 0)
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


    /// <summary>
    /// 删除记录
    /// </summary>
    /// <param name="Code"></param>
    public void DeleteRecord(string strwhere)
    {
      if (!string.IsNullOrEmpty(strwhere))
      {
        StringBuilder strSql = new StringBuilder();
        strSql.Append("delete from " + TablePrefix + tableName + " ");
        strSql.Append(" where " + strwhere);
        SqlHelper.ExecuteNonQuery(strSql.ToString());
      }
    }

    public bool Exists(string strwhere)
    {
      try
      {
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


    public bool Exists(string strwhere, Hashtable myhash)
    {
      try
      {
        string mysql = "select count(1) from {0}";
        if (strwhere.Trim() != "")
          mysql += " where " + strwhere;
        string sqlString = string.Format(mysql, TablePrefix + tableName);
        List<DbParameter> p = getDbParameter(myhash);
        if (p.Count == 0)
          return Exists(strwhere);
        object total = SqlHelper.ExecuteScalar(sqlString, p.ToArray());
        return (Convert.ToInt32(total) != 0);
      }
      catch
      {
        return false;
      }
    }

    /// <summary>
    /// 获得一个值
    /// </summary>
    public object GetColObject(string Col, string strWhere)
    {
      try
      {
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

    /// <summary>
    /// 统计数量
    /// </summary>
    /// <param name="strwhere"></param>
    /// <returns></returns>
    public Int32 GetTableTotal(string strwhere)
    {
      try
      {
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

    /// <summary>
    /// 获得一个值
    /// </summary>
    public Int32 GetTableSum(string strWhere)
    {
      try
      {
        string mysql = "select isnull(sum(1),0) from " + TablePrefix + tableName + "";
        if (strWhere != "")
          mysql += " where " + strWhere;
        return Convert.ToInt32(SqlHelper.ExecuteScalar(mysql));
      }
      catch
      {
        return 0;
      }
    }

    /// <summary>
    /// 获得一个值
    /// </summary>
    public Int32 GetTableSum(string col,string strWhere)
    {
      try
      {
        string mysql = "select isnull(sum("+col+"),0) from " + TablePrefix + tableName + "";
        if (strWhere != "")
          mysql += " where " + strWhere;
        return Convert.ToInt32(SqlHelper.ExecuteScalar(mysql));
      }
      catch
      {
        return 0;
      }
    }


    public void UpdateByCol(string Col, string Value, string CodeCol, int code)
    {
      StringBuilder strSql = new StringBuilder();
      strSql.Append("update " + TablePrefix + tableName + " set ");
      strSql.Append("[" + Col + "]='" + Value + "'");
      strSql.Append(" where [" + CodeCol + "]=" + code);
      SqlHelper.ExecuteNonQuery(strSql.ToString());
    }

    public void UpdateByCol(string[] Col, string[] Value, string CodeCol, int code)
    {
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

    public void UpdateByCol(string Col, string Value, string CodeCol, string code)
    {
      StringBuilder strSql = new StringBuilder();
      strSql.Append("update " + TablePrefix + tableName + " set ");
      strSql.Append("[" + Col + "]='" + Value + "'");
      strSql.Append(" where [" + CodeCol + "] in(" + code + ")");
      SqlHelper.ExecuteNonQuery(strSql.ToString());
    }

    public void UpdateByCol(string[] Col, string[] Value, string CodeCol, string code)
    {
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

    /// <summary>
    /// 自增
    /// </summary>
    /// <param name="ColName">自增字段</param>
    /// <param name="IncValue">自增基数</param>
    /// <param name="CodeCol"></param>
    /// <param name="code"></param>
    public void Inc(string ColName, int IncValue, string CodeCol, string code)
    {
      StringBuilder strSql = new StringBuilder();
      strSql.Append("update " + TablePrefix + tableName + " set ");
      strSql.Append("[" + ColName + "]=isnull([" + ColName + "],0)+" + IncValue);
      strSql.Append(" where [" + CodeCol + "] in(" + code + ")");
      SqlHelper.ExecuteNonQuery(strSql.ToString());
    }

    /// <summary>
    /// 自增
    /// </summary>
    /// <param name="ColName">自增字段</param>
    /// <param name="IncValue">自增基数</param>
    /// <param name="strwhere"></param>
    public void Inc(string ColName, int IncValue, string strwhere)
    {
      StringBuilder strSql = new StringBuilder();
      strSql.Append("update " + TablePrefix + tableName + " set ");
      strSql.Append("[" + ColName + "]=isnull([" + ColName + "],0)+" + IncValue);
      if (string.IsNullOrEmpty(strwhere))
        strSql.Append(" where " + strwhere);
      SqlHelper.ExecuteNonQuery(strSql.ToString());
    }

    /// <summary>
    /// 获得数据列表
    /// </summary>
    public DataSet FindList(string strWhere, string order)
    {
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


    /// <summary>
    /// 获得数据列表
    /// </summary>
    public DataSet FindList(string Column, string strWhere, string order)
    {
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

    /// <summary>
    /// 分页显示数据
    /// </summary>
    public DataSet FindByPage(string FieldKey, int PageSize, int PageIndex, string strWhere, string Column, string FieldOrder)
    {
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

    /// <summary>
    /// 分页显示数据
    /// </summary>
    public DataSet FindByPage(string FieldKey, int PageSize, int PageIndex, string strWhere, string Column, string FieldOrder, ref int RecordCount)
    {
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

    public T TryFind<T>(string where)
    {
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

    public Entity TryFind(string column, string where)
    {
      StringBuilder strSql = new StringBuilder();
      strSql.Append("select top 1 " + (string.IsNullOrEmpty(column) ? "*" : column) + " from " + TablePrefix + tableName);
      if (where.Trim() != "")
      {
        strSql.Append(" where " + where);
      }
      DataTable dt = SqlHelper.ExecuteDataset(strSql.ToString()).Tables[0];

      if (dt.Rows.Count == 0)
      {
        Entity _t = default(Entity);
        return _t;
      }
      else
      {
        Entity _t = Activator.CreateInstance<Entity>();

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

    public Entity TryFind(string where)
    {
      StringBuilder strSql = new StringBuilder();
      strSql.Append("select top 1 * from " + TablePrefix + tableName);
      if (where.Trim() != "")
      {
        strSql.Append(" where " + where);
      }
      DataTable dt = SqlHelper.ExecuteDataset(strSql.ToString()).Tables[0];

      if (dt.Rows.Count == 0)
      {
        Entity _t = default(Entity);
        return _t;
      }
      else
      {
        Entity _t = Activator.CreateInstance<Entity>();

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

    public Entity TryFind(string column, string where, Hashtable table)
    {
      List<DbParameter> p = getDbParameter(table);
      if (p.Count == 0)
        return TryFind(where);

      StringBuilder strSql = new StringBuilder();
      strSql.Append("select top 1 " + (string.IsNullOrEmpty(column) ? "*" : column) + " from " + TablePrefix + tableName);
      if (where.Trim() != "")
      {
        strSql.Append(" where " + where);
      }
      DataTable dt = SqlHelper.ExecuteDataset(strSql.ToString(), p.ToArray()).Tables[0];

      if (dt.Rows.Count == 0)
      {
        Entity _t = default(Entity);
        return _t;
      }
      else
      {
        Entity _t = Activator.CreateInstance<Entity>();

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

    public Entity TryFind(string where, Hashtable table)
    {
      List<DbParameter> p = getDbParameter(table);
      if (p.Count == 0)
        return TryFind(where);

      StringBuilder strSql = new StringBuilder();
      strSql.Append("select top 1 * from " + TablePrefix + tableName);
      if (where.Trim() != "")
      {
        strSql.Append(" where " + where);
      }
      DataTable dt = SqlHelper.ExecuteDataset(strSql.ToString(), p.ToArray()).Tables[0];

      if (dt.Rows.Count == 0)
      {
        Entity _t = default(Entity);
        return _t;
      }
      else
      {
        Entity _t = Activator.CreateInstance<Entity>();

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

    public List<Entity> ToList(DataSet table)
    {
      DataTable dt = table.Tables[0];
      if (dt.Rows.Count == 0)
      {
        List<Entity> _t = default(List<Entity>);
        return _t;
      }
      else
      {
        List<Entity> list = new List<Entity>();

        for (int i = 0; i < dt.Rows.Count; i++)
        {
          Entity _t = Activator.CreateInstance<Entity>();
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

    /// <summary>
    /// 获取最大字段
    /// </summary>
    /// <param name="Column"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    public object FindMaxColumn(string Column, string where)
    {
      StringBuilder strSql = new StringBuilder();
      strSql.Append("select isnull(max([" + Column + "]),0) from " + TablePrefix + tableName);
      if (where.Trim() != "")
      {
        strSql.Append(" where " + where);
      }
      return SqlHelper.ExecuteScalar(strSql.ToString());
    }

    /// <summary>
    /// 执行存储过程
    /// </summary>
    /// <param name="ProcName"></param>
    /// <param name="p"></param>
    public int Procedure(string ProcName, DbParameter[] p)
    {
      return SqlHelper.ExecuteNonQuery(CommandType.StoredProcedure, ProcName, p);
    }


    /// <summary>
    /// 执行存储过程
    /// </summary>
    /// <param name="ProcName"></param>
    /// <param name="p"></param>
    public int ProcedureByErr(string ProcName, DbParameter[] p, ref String ErrString)
    {
      return SqlHelper.ExecuteNonQueryByErr(CommandType.StoredProcedure, ProcName, p, ref ErrString);
    }


    /// <summary>
    /// 执行存储过程
    /// </summary>
    /// <param name="ProcName"></param>
    /// <param name="p"></param>
    public DataSet ProDataSet(string ProcName, DbParameter[] p)
    {
      return SqlHelper.ExecuteDataset(CommandType.StoredProcedure, ProcName, p);
    }

    /// <summary>
    /// 执行sql语句
    /// </summary>
    /// <param name="sql"></param>
    /// <returns></returns>
    public DataSet Query(string sql)
    {
      return SqlHelper.ExecuteDataset(sql);
    }


    public DataSet Query(string sql, DbParameter[] p)
    {
      return SqlHelper.ExecuteDataset(sql, p);
    }

    public Int32 ExecuteNonQuery(string sql, DbParameter[] p)
    {
      return SqlHelper.ExecuteNonQuery(sql, p);
    }


    public Int32 ExecuteNonQuery(string sql, Hashtable table)
    {
      if (table != null && table.Count > 0)
      {
        DbParameter[] p = getDbParameter(table).ToArray();
        return SqlHelper.ExecuteNonQuery(sql, p);
      }
      return SqlHelper.ExecuteNonQuery(sql);
    }

    /// <summary>
    /// 生成database
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void SetTable()
    {
      DataTable table = new AttributeHelper<Entity>().getEntityTable();
      StringBuilder sqlBuilder = new StringBuilder();
      sqlBuilder.Append("if object_id(N'" + TablePrefix + tableName + "',N'U') is null");
      sqlBuilder.Append(" begin");
      sqlBuilder.Append(" CREATE TABLE [dbo].[" + TablePrefix + tableName + "] (");
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
          Null = false;
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

        string _default="";
        if (!string.IsNullOrEmpty(DefaultValue.ToString()))
        {
          if (Type == typeof(int))
            _default = "DEFAULT ((" + DefaultValue + "))";
          else if (Type == typeof(DateTime) && DefaultValue.ToString().ToLower() == "now")
            _default = "DEFAULT (getdate())";
          else
            _default = "DEFAULT ('" + DefaultValue + "')";
        }

        sqlBuilder.Append("[" + Name + "] " + type + " " + (Null ? "NULL" : "NOT NULL") + " " + (((Usage == EnumFieldUsage.IncKey) || (Usage == EnumFieldUsage.IncPK)) ? "IDENTITY(1,1)" : "") + " " + _default + ",");
      }
      sqlBuilder = sqlBuilder.Remove(sqlBuilder.Length - 1, 1);
      sqlBuilder.Append(" );");
      sqlBuilder.Append(alterString);
      sqlBuilder.Append(" end");
      SqlHelper.ExecuteNonQuery(sqlBuilder.ToString());
      //return sqlBuilder.ToString();
    }

    public string TableName
    {
      get
      {
        return tableName;
      }
    }

    /// <summary>
    /// 支持sql2005以上分页
    /// </summary>
    /// <param name="Table">表，支持多表，使用sql语句</param>
    /// <param name="ColName">查询的字段</param>
    /// <param name="OrderString">排序(必填)</param>
    /// <param name="strWhere">条件</param>
    /// <param name="para">参数</param>
    /// <param name="page">页码</param>
    /// <param name="PageSize">每页显示数</param>
    /// <returns></returns>
    public DataSet FindSetBySql(string tableName, string ColName, string OrderString, string strWhere, DbParameter[] para, int page, int PageSize)
    {
      if (string.IsNullOrEmpty(tableName))
      {
        this.TablePrefix = TablePrefix;
        tableName = new Entity().GetType().Name;
        string tmpTable = TableAttributeHelper<Entity>.TableName();
        if (!string.IsNullOrEmpty(tmpTable))
          tableName = tmpTable;
        tableName = TablePrefix + tableName;
      }
      return SqlHelper.FindSetBySql(tableName, ColName, OrderString, strWhere, para, page, PageSize);
    }

    public DataSet FindSetBySql(string tableName, string ColName, string OrderString, string strWhere, Hashtable table, int page, int PageSize)
    {
      if (string.IsNullOrEmpty(tableName))
      {
        this.TablePrefix = TablePrefix;
        tableName = new Entity().GetType().Name;
        string tmpTable = TableAttributeHelper<Entity>.TableName();
        if (!string.IsNullOrEmpty(tmpTable))
          tableName = tmpTable;
        tableName = TablePrefix + tableName;
      }
      DbParameter[] p = null;
      if (table != null && table.Count != 0)
        p = getDbParameter(table).ToArray();
      return SqlHelper.FindSetBySql(tableName, ColName, OrderString, strWhere, p, page, PageSize);
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

    private List<DbParameter> getDbParameter(Hashtable myhash)
    {
      DataTable table = new AttributeHelper<Entity>().getEntityTable();
      List<DbParameter> p = new List<DbParameter>();
      Dictionary<Type, DbType> TypeToDb = getDbType();
      if (table.Rows.Count > 0)
      {
        foreach (DataRow dr in table.Rows)
        {
          String Name = dr["Name"].ToString();
          String ClassName = dr["ClassName"].ToString();
          Type Type = (Type)dr["Type"];
          Int32 Length = Convert.ToInt32(dr["Length"]);
          EnumFieldUsage Usage = (EnumFieldUsage)Convert.ToInt32(dr["Usage"]);
          bool Null = Convert.ToBoolean(dr["Null"]);

          object colValue = null;
          bool NameExists = myhash.Contains(Name.ToLower());
          bool ClassNameExists = myhash.Contains(ClassName.ToLower());

          if (NameExists || ClassNameExists)
          {
            String ValueName = NameExists ? Name : ClassName;
            colValue = Convert.ChangeType(myhash[ValueName.ToLower()], Type);
            DbParameter d = SqlHelper.MakeInParam(Prefix + Name, TypeToDb[Type], Length, colValue);
            if (Type == typeof(System.String) && Length <= 200 && Length != 0)
            {
              d = SqlHelper.MakeInParam(Prefix + Name, TypeToDb[Type], Length, colValue.ToString().Replace("&", "&#38;").Replace("<", "&#60;").Replace(">", "&#62;"));
            }
            p.Add(d);
          }
        }
      }
      return p;
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
