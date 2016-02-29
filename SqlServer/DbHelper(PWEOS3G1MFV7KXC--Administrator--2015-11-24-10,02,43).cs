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

  interface IMonth<T>
  {
    void Test();
  }

  class SampleClass1<T> where T: class,IMonth<T>,new()
  {

    public void Test()
    {
      string tableName = (new T()).GetType().Name;
      throw new NotImplementedException();
    }
  }


  public class DbHelper<T1> : IDbHelper<T1>
  {

    public object CreateRecord(Hashtable myhash)
    {
      throw new NotImplementedException();
    }

    public object UpdateRecord(Hashtable myhash)
    {
      throw new NotImplementedException();
    }

    public void DeleteRecord(object Code)
    {
      throw new NotImplementedException();
    }

    public void DeleteRecord(string strwhere)
    {
      throw new NotImplementedException();
    }

    public bool Exists(string strwhere)
    {
      throw new NotImplementedException();
    }

    public bool Exists(string strwhere, Hashtable myhash)
    {
      throw new NotImplementedException();
    }

    public object GetColObject(string Col, string strWhere)
    {
      throw new NotImplementedException();
    }

    public int GetTableTotal(string strwhere)
    {
      throw new NotImplementedException();
    }

    public int GetTableSum(string strWhere)
    {
      throw new NotImplementedException();
    }

    public void UpdateByCol(string Col, string Value, string CodeCol, int code)
    {
      throw new NotImplementedException();
    }

    public void UpdateByCol(string[] Col, string[] Value, string CodeCol, int code)
    {
      throw new NotImplementedException();
    }

    public void UpdateByCol(string Col, string Value, string CodeCol, string code)
    {
      throw new NotImplementedException();
    }

    public void UpdateByCol(string[] Col, string[] Value, string CodeCol, string code)
    {
      throw new NotImplementedException();
    }

    public void Inc(string ColName, int IncValue, string CodeCol, string code)
    {
      throw new NotImplementedException();
    }

    public void Inc(string ColName, int IncValue, string strwhere)
    {
      throw new NotImplementedException();
    }

    public DataSet FindList(string strWhere, string order)
    {
      throw new NotImplementedException();
    }

    public DataSet FindList(string Column, string strWhere, string order)
    {
      throw new NotImplementedException();
    }

    public DataSet FindByPage(string FieldKey, int PageSize, int PageIndex, string strWhere, string Column, string FieldOrder)
    {
      throw new NotImplementedException();
    }

    public DataSet FindByPage(string FieldKey, int PageSize, int PageIndex, string strWhere, string Column, string FieldOrder, ref int RecordCount)
    {
      throw new NotImplementedException();
    }

    public T TryFind<T>(string where)
    {
      throw new NotImplementedException();
    }

    public T1 TryFind(string column, string where)
    {
      throw new NotImplementedException();
    }

    public T1 TryFind(string where)
    {
      throw new NotImplementedException();
    }

    public T1 TryFind(string column, string where, Hashtable table)
    {
      throw new NotImplementedException();
    }

    public T1 TryFind(string where, Hashtable table)
    {
      throw new NotImplementedException();
    }

    public List<T1> ToList(DataSet table)
    {
      throw new NotImplementedException();
    }

    public object FindMaxColumn(string Column, string where)
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
  }
}
