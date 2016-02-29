using Hyc.DbDriver.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Hyc.DbDriver
{
  public class Helper<Entity> where Entity : class,new()
  {
    /// <summary>
    /// 数据库类型
    /// </summary>
    public String DbProvider { get; set; }

    /// <summary>
    /// 表前缀
    /// </summary>
    public String TablePrefix { get; set; }

    private string tableName = "";

    IDbHelper DbHelper = null;

    public Helper(string DBConnectString, string DbProvider, string TablePrefix)
    {
      this.TablePrefix = TablePrefix;
      tableName = new Entity().GetType().Name;
      string tmpTable = TableAttributeHelper<Entity>.TableName();
      if (!string.IsNullOrEmpty(tmpTable))
        tableName = tmpTable;
      //System.Web.HttpContext.Current.Response.Write(tableName);
      switch (DbProvider.ToLower())
      {
        case "mssql":
          DbHelper = new SqlServer.DbHelper(DBConnectString, tableName, TablePrefix);
          break;
        case "mysql":
          DbHelper = new MySql.DbHelper(DBConnectString, tableName, TablePrefix);
          break;
        default:
          DbHelper = new SqlServer.DbHelper(DBConnectString, tableName, TablePrefix);
          break;
      }
    }


    public object CreateRecord(Entity model)
    {
      return DbHelper.CreateRecord<Entity>(model);
    }

    public object CreateRecord(Hashtable myhash)
    {
      return  DbHelper.CreateRecord<Entity>(myhash);
    }

    public object UpdateRecord(Hashtable myhash)
    {
      return DbHelper.UpdateRecord<Entity>(myhash);
    }

    public void UpdateRecord(Hashtable myhash, Hashtable update_hash)
    {
      DbHelper.UpdateRecord<Entity>(myhash, update_hash);
    }

    /// <summary>
    /// 根据主键删除记录
    /// </summary>
    /// <param name="Code"></param>
    public void DeleteRecord(object Code)
    {
      DbHelper.DeleteRecord<Entity>(Code);
    }


    /// <summary>
    /// 删除记录
    /// </summary>
    /// <param name="Code"></param>
    public void DeleteRecord(string strwhere)
    {
      DbHelper.DeleteRecord<Entity>(strwhere);
    }


    public void DeleteRecord(Expression<Func<Entity, bool>> where)
    {
      DbHelper.DeleteRecord<Entity>(where);
    }


    public bool Exists(string strwhere)
    {
      return DbHelper.Exists<Entity>(strwhere);
    }


    public bool Exists(string strwhere, Hashtable myhash)
    {
      return DbHelper.Exists<Entity>(strwhere,myhash);
    }

    /// <summary>
    /// 获得一个值
    /// </summary>
    public object GetColObject(string Col, string strWhere)
    {
      return DbHelper.GetColObject<Entity>(Col, strWhere);
    }

    /// <summary>
    /// 统计数量
    /// </summary>
    /// <param name="strwhere"></param>
    /// <returns></returns>
    public Int32 GetTableTotal(string strwhere)
    {
      return DbHelper.GetTableTotal<Entity>(strwhere);
    }

    /// <summary>
    /// 获得一个值
    /// </summary>
    public Int32 GetTableSum(string col,string strWhere)
    {
      return DbHelper.GetTableSum<Entity>(col,strWhere);
    }


    public void UpdateByCol(string Col, string Value, string CodeCol, int code)
    {
      DbHelper.UpdateByCol<Entity>(Col,Value,CodeCol,code);
    }

    public void UpdateByCol(string[] Col, string[] Value, string CodeCol, int code)
    {
      DbHelper.UpdateByCol<Entity>(Col, Value, CodeCol, code);
    }

    public void UpdateByCol(string Col, string Value, string CodeCol, string code)
    {
      DbHelper.UpdateByCol<Entity>(Col, Value, CodeCol, code);
    }

    public void UpdateByCol(string[] Col, string[] Value, string CodeCol, string code)
    {
      DbHelper.UpdateByCol<Entity>(Col, Value, CodeCol, code);
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
      DbHelper.Inc<Entity>(ColName, IncValue, CodeCol, code);
    }

    /// <summary>
    /// 自增
    /// </summary>
    /// <param name="ColName">自增字段</param>
    /// <param name="IncValue">自增基数</param>
    /// <param name="strwhere"></param>
    public void Inc(string ColName, int IncValue, string strwhere)
    {
      DbHelper.Inc<Entity>(ColName, IncValue, strwhere);
    }

    /// <summary>
    /// 获得数据列表
    /// </summary>
    public DataSet FindList(string strWhere, string order)
    {
      return DbHelper.FindList<Entity>(strWhere, order);
    }


    /// <summary>
    /// 获得数据列表
    /// </summary>
    public DataSet FindList(string Column, string strWhere, string order)
    {
      return DbHelper.FindList<Entity>(Column,strWhere, order);
    }

    /// <summary>
    /// 分页显示数据
    /// </summary>
    public DataSet FindByPage(string FieldKey, int PageSize, int PageIndex, string strWhere, string Column, string FieldOrder)
    {
      return DbHelper.FindByPage<Entity>(FieldKey, PageSize, PageIndex, strWhere, Column, FieldOrder);

    }
    /// <summary>
    /// 分页显示数据
    /// </summary>
    public DataSet FindByPage(string FieldKey, int PageSize, int PageIndex, Expression<Func<Entity, bool>> where, string Column, string FieldOrder)
    {
      return DbHelper.FindByPage<Entity>(FieldKey, PageSize, PageIndex, where, Column, FieldOrder);

    }

    /// <summary>
    /// 分页显示数据
    /// </summary>
    public DataSet FindByPage(string FieldKey, int PageSize, int PageIndex, string strWhere, string Column, string FieldOrder, ref int RecordCount)
    {
      return DbHelper.FindByPage<Entity>(FieldKey, PageSize, PageIndex, strWhere, Column, FieldOrder, ref RecordCount);
    }

    public Entity TryFind(string column, string where)
    {
      return DbHelper.TryFind<Entity>(column,where);
    }

    public Entity TryFind(string where)
    {
      return DbHelper.TryFind<Entity>(where);
    }

    public Entity TryFind(string column, string where, Hashtable table)
    {
      return DbHelper.TryFind<Entity>(column,where,table);
    }

    public Entity TryFind(string where, Hashtable table)
    {
      return DbHelper.TryFind<Entity>(where, table);
    }

    public List<Entity> ToList(DataSet table)
    {
      return DbHelper.ToList<Entity>(table);
    }

    /// <summary>
    /// 获取最大字段
    /// </summary>
    /// <param name="Column"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    public object FindMaxColumn(string Column, string where)
    {
      return DbHelper.FindMaxColumn<Entity>(Column,where);
    }

    /// <summary>
    /// 执行存储过程
    /// </summary>
    /// <param name="ProcName"></param>
    /// <param name="p"></param>
    public int Procedure(string ProcName, DbParameter[] p)
    {
      return DbHelper.Procedure(ProcName, p);
    }


    /// <summary>
    /// 执行存储过程
    /// </summary>
    /// <param name="ProcName"></param>
    /// <param name="p"></param>
    public int ProcedureByErr(string ProcName, DbParameter[] p, ref String ErrString)
    {
      return DbHelper.ProcedureByErr(ProcName, p, ref ErrString);
    }


    /// <summary>
    /// 执行存储过程
    /// </summary>
    /// <param name="ProcName"></param>
    /// <param name="p"></param>
    public DataSet ProDataSet(string ProcName, DbParameter[] p)
    {
      return DbHelper.ProDataSet(ProcName, p);
    }

    /// <summary>
    /// 执行sql语句
    /// </summary>
    /// <param name="sql"></param>
    /// <returns></returns>
    public DataSet Query(string sql)
    {
      return DbHelper.Query(sql);
    }


    public DataSet Query(string sql, DbParameter[] p)
    {
      return DbHelper.Query(sql,p);
    }

    public Int32 ExecuteNonQuery(string sql, DbParameter[] p)
    {
      return DbHelper.ExecuteNonQuery(sql, p);
    }


    public Int32 ExecuteNonQuery(string sql, Hashtable table)
    {
      return DbHelper.ExecuteNonQuery<Entity>(sql, table);
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
      return DbHelper.FindSetBySql(tableName, ColName, OrderString, strWhere,  para, page, PageSize);
    }

    public DataSet FindSetBySql(string tableName, string ColName, string OrderString, string strWhere, Hashtable table, int page, int PageSize)
    {
      return DbHelper.FindSetBySql<Entity>(tableName, ColName, OrderString, strWhere, table, page, PageSize);

    }

    public List<DbParameter> DbParameter(Dictionary<string, object> dict)
    {
      return DbHelper.DbParameter(dict);
    }

  }
}
