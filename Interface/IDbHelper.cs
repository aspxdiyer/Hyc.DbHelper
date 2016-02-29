using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Hyc.DbDriver.Interface
{
  public interface IDbHelper
  {

    /// <summary>
    /// 创建记录
    /// </summary>
    /// <param name="myhash"></param>
    /// <returns></returns>
    object CreateRecord<T>(T model) where T : class, new();
    /// <summary>
    /// 创建记录
    /// </summary>
    /// <param name="myhash"></param>
    /// <returns></returns>
    object CreateRecord<T>(Hashtable myhash) where T : class, new();

    /// <summary>
    /// 更新记录
    /// </summary>
    /// <param name="myhash"></param>
    /// <returns></returns>
    object UpdateRecord<T>(Hashtable myhash) where T : class, new();

    /// <summary>
    /// 更新数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="myhash"></param>
    /// <param name="update_hash"></param>
    void UpdateRecord<T>(Hashtable myhash, Hashtable update_hash) where T : class,new();

    /// <summary>
    /// 删除记录
    /// </summary>
    /// <param name="Code"></param>
    void DeleteRecord<T>(object Code) where T : class, new();

    /// <summary>
    /// 删除记录
    /// </summary>
    /// <param name="strwhere"></param>
    void DeleteRecord<T>(string strwhere) where T : class, new();


    /// <summary>
    /// 删除记录
    /// </summary>
    /// <param name="strwhere"></param>
    void DeleteRecord<T>(Expression<Func<T, bool>> where) where T : class, new();

    /// <summary>
    /// 删除记录
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="strwhere"></param>
    /// <param name="myhash"></param>
    void DeleteRecord<T>(string strwhere, Hashtable myhash) where T : class, new();

    /// <summary>
    /// 判断记录是否存在
    /// </summary>
    /// <param name="strwhere"></param>
    /// <returns></returns>
    bool Exists<T>(string strwhere) where T : class, new();

    /// <summary>
    /// 判断记录是否存在
    /// </summary>
    /// <param name="strwhere"></param>
    /// <param name="myhash"></param>
    /// <returns></returns>
    bool Exists<T>(string strwhere, Hashtable myhash) where T : class, new();

    /// <summary>
    /// 获取一个记录值
    /// </summary>
    /// <param name="Col"></param>
    /// <param name="strWhere"></param>
    /// <returns></returns>
    object GetColObject<T>(string Col, string strWhere) where T : class, new();

    /// <summary>
    /// 表单记录数
    /// </summary>
    /// <param name="strwhere"></param>
    /// <returns></returns>
    Int32 GetTableTotal<T>(string strwhere) where T : class, new();

    /// <summary>
    /// 表单求和
    /// </summary>
    /// <param name="strWhere"></param>
    /// <returns></returns>
    Int32 GetTableSum<T>(string col,string strWhere) where T : class, new();

    /// <summary>
    /// 更新数据
    /// </summary>
    /// <param name="Col"></param>
    /// <param name="Value"></param>
    /// <param name="CodeCol"></param>
    /// <param name="code"></param>
    void UpdateByCol<T>(string Col, string Value, string CodeCol, int code) where T : class, new();

    /// <summary>
    /// 更新数据
    /// </summary>
    /// <param name="Col"></param>
    /// <param name="Value"></param>
    /// <param name="CodeCol"></param>
    /// <param name="code"></param>
    void UpdateByCol<T>(string[] Col, string[] Value, string CodeCol, int code) where T : class, new();

    /// <summary>
    /// 更新数据
    /// </summary>
    /// <param name="Col"></param>
    /// <param name="Value"></param>
    /// <param name="CodeCol"></param>
    /// <param name="code"></param>
    void UpdateByCol<T>(string Col, string Value, string CodeCol, string code) where T : class, new();

    /// <summary>
    /// 更新数据
    /// </summary>
    /// <param name="Col"></param>
    /// <param name="Value"></param>
    /// <param name="CodeCol"></param>
    /// <param name="code"></param>
    void UpdateByCol<T>(string[] Col, string[] Value, string CodeCol, string code) where T : class, new();

    /// <summary>
    /// inc
    /// </summary>
    /// <param name="ColName"></param>
    /// <param name="IncValue"></param>
    /// <param name="CodeCol"></param>
    /// <param name="code"></param>
    void Inc<T>(string ColName, int IncValue, string CodeCol, string code) where T : class, new();

    /// <summary>
    /// inc
    /// </summary>
    /// <param name="ColName"></param>
    /// <param name="IncValue"></param>
    /// <param name="strwhere"></param>
    void Inc<T>(string ColName, int IncValue, string strwhere) where T : class, new();

    /// <summary>
    /// 列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="strWhere"></param>
    /// <param name="order"></param>
    /// <returns></returns>
    DataSet FindList<T>(string strWhere, string order) where T : class, new();

    /// <summary>
    /// 列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="Column"></param>
    /// <param name="strWhere"></param>
    /// <param name="order"></param>
    /// <returns></returns>
    DataSet FindList<T>(string Column, string strWhere, string order) where T : class, new();

    /// <summary>
    /// 分页列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="FieldKey"></param>
    /// <param name="PageSize"></param>
    /// <param name="PageIndex"></param>
    /// <param name="strWhere"></param>
    /// <param name="Column"></param>
    /// <param name="FieldOrder"></param>
    /// <returns></returns>
    DataSet FindByPage<T>(string FieldKey, int PageSize, int PageIndex, string strWhere, string Column, string FieldOrder) where T : class, new();

    /// <summary>
    /// 分页列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="FieldKey"></param>
    /// <param name="PageSize"></param>
    /// <param name="PageIndex"></param>
    /// <param name="where"></param>
    /// <param name="Column"></param>
    /// <param name="FieldOrder"></param>
    /// <returns></returns>
    DataSet FindByPage<T>(string FieldKey, int PageSize, int PageIndex, Expression<Func<T, bool>> where, string Column, string FieldOrder) where T : class, new();

    /// <summary>
    /// 分页列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="FieldKey"></param>
    /// <param name="PageSize"></param>
    /// <param name="PageIndex"></param>
    /// <param name="strWhere"></param>
    /// <param name="Column"></param>
    /// <param name="FieldOrder"></param>
    /// <param name="RecordCount"></param>
    /// <returns></returns>
    DataSet FindByPage<T>(string FieldKey, int PageSize, int PageIndex, string strWhere, string Column, string FieldOrder, ref int RecordCount) where T : class, new();

    /// <summary>
    /// 实体查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="column"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    T TryFind<T>(string column, string where) where T : class, new();

    /// <summary>
    /// 实体查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="where"></param>
    /// <returns></returns>
    T TryFind<T>(string where) where T : class, new();

    /// <summary>
    /// 实体查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="column"></param>
    /// <param name="where"></param>
    /// <param name="table"></param>
    /// <returns></returns>
    T TryFind<T>(string column, string where, Hashtable table) where T : class, new();

    /// <summary>
    /// 实体查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="where"></param>
    /// <param name="table"></param>
    /// <returns></returns>
    T TryFind<T>(string where, Hashtable table) where T : class, new();

    List<T> ToList<T>(DataSet table) where T : class, new();

    /// <summary>
    /// 获取字段最大值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="Column"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    object FindMaxColumn<T>(string Column, string where) where T : class, new();

    /// <summary>
    /// 存储过程
    /// </summary>
    /// <param name="ProcName"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    int Procedure(string ProcName, DbParameter[] p);

    /// <summary>
    /// 存储过程
    /// </summary>
    /// <param name="ProcName"></param>
    /// <param name="p"></param>
    /// <param name="ErrString"></param>
    /// <returns></returns>
    int ProcedureByErr(string ProcName, DbParameter[] p, ref String ErrString);

    /// <summary>
    /// 存储过程返回dataset
    /// </summary>
    /// <param name="ProcName"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    DataSet ProDataSet(string ProcName, DbParameter[] p);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sql"></param>
    /// <returns></returns>
    DataSet Query(string sql);

    DataSet Query(string sql, DbParameter[] p);

    Int32 ExecuteNonQuery(string sql, DbParameter[] p);

    Int32 ExecuteNonQuery<T>(string sql, Hashtable table) where T:class,new();

    /// <summary>
    /// 返回列表
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="ColName"></param>
    /// <param name="OrderString"></param>
    /// <param name="strWhere"></param>
    /// <param name="para"></param>
    /// <param name="page"></param>
    /// <param name="PageSize"></param>
    /// <returns></returns>
    DataSet FindSetBySql(string tableName, string ColName, string OrderString, string strWhere, DbParameter[] para, int page, int PageSize);

    /// <summary>
    /// 返回列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tableName"></param>
    /// <param name="ColName"></param>
    /// <param name="OrderString"></param>
    /// <param name="strWhere"></param>
    /// <param name="table"></param>
    /// <param name="page"></param>
    /// <param name="PageSize"></param>
    /// <returns></returns>
    DataSet FindSetBySql<T>(string tableName, string ColName, string OrderString, string strWhere, Hashtable table, int page, int PageSize) where T:class,new();

    List<DbParameter> DbParameter(Dictionary<string, object> dict);

    /// <summary>
    /// 建表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    void SetTable<T>() where T : class, new();
  }
}
