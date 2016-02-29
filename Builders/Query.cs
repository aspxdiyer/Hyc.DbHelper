using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Hyc.DbDriver.Builders
{
  /// <summary>
  /// 结构
  /// </summary>
  public struct HycValue
  {
    private HycSortedList value;

    public HycValue(HycSortedList v){
      value = v;
    }

    public bool Exists(String Keys){
      return value.ContainsKey(Keys);
    }

    public HycSortedList ToValue(){
      return value;
    }

    public String ToJson(){
      StringBuilder result = new StringBuilder();
      result.Append("[");
      int Count = 0;
      foreach (DictionaryEntry de in value){
        result.Append("{\""+de.Key+"\":"+de.Value+"},");
        Count ++;
      }
      if(Count>0)
        result = result.Remove(result.Length-1,1);
      result.Append("]");
      return result.ToString();
    }
  }

	public class Query
	{

    public static HycValue And(params HycValue[] Doc){
      HycSortedList result = new HycSortedList();
      foreach(HycValue d in Doc){
        HycSortedList _dic = new HycSortedList();
        _dic = d.ToValue();

        foreach (DictionaryEntry de in _dic)
        {
          result.Add(de.Key,de.Value);
        }
      }
      return new HycValue(result);
    }

    public static HycValue Or(params HycValue[] Doc){
      HycValue tmp = And(Doc);
      string value = tmp.ToJson();
      HycSortedList result = new HycSortedList();
      result.Add("$or",value);
      return new HycValue(result);
    }

    public static HycValue Operate(String Name,Object Value,string Operators){
      if (Name == null)
      {
        throw new ArgumentNullException("name");
      }
      if (Value == null)
      {
        throw new ArgumentNullException("value");
      }
      HycSortedList SortedList = new HycSortedList();
      /*
      switch(Operators.ToLower())
      {
        case "eq":
          SortedList.Add(Name,Value);
          break;
        case "ne":
        case "le":
        case "lt":
        case "ge":
        case "gt":
        case "in":
        case "like":
        case "start":
        case "end":
          SortedList.Add(Name,"{ \"$"+Operators.ToLower()+"\" : \""+Value+"\" }");
          break;
      }
      */
      SortedList.Add(Name,"{ \"$"+Operators.ToLower()+"\" : \""+String2Json(Value.ToString())+"\" }");
      return new HycValue(SortedList);
    }
    /// <summary>
    /// 等于
    /// </summary>
    /// <param name="name">The name of the element to test.</param>
    /// <param name="value">The value to compare to.</param>
    /// <returns></returns>
    public static HycValue EQ(string name, Object value)
    {
      return Operate(name,value,"eq");
    }

    /// <summary>
    /// 不等于
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static HycValue NE(string name, Object value)
    {
      return Operate(name,value,"ne");
    }

    /// <summary>
    /// 小于等于号(<=)
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static HycValue LE(string name, Object value)
    {
      return Operate(name,value,"le");
    }

    /// <summary>
    /// LT是小于号(<)
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static HycValue LT(string name, Object value)
    {
      return Operate(name,value,"lt");
    }

    /// <summary>
    /// GE是大于等于号（>=）
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static HycValue GE(string name, Object value)
    {
      return Operate(name,value,"ge");
    }

    /// <summary>
    /// GT是大于号(>)
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static HycValue GT(string name, Object value)
    {
      return Operate(name,value,"gt");
    }

    /// <summary>
    /// in
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static HycValue IN(string name, Object value)
    {
      return Operate(name,value,"in");
    }
    
    /// <summary>
    /// LIKE
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static HycValue LIKE(string name, Object value)
    {
      return Operate(name,value,"like");
    }
    
    /// <summary>
    /// Start
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static HycValue Start(string name, Object value)
    {
      return Operate(name,value,"start");
    }

    
    /// <summary>
    /// Start
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static HycValue End(string name, Object value)
    {
      return Operate(name,value,"end");
    }

    #region 方法
    static private string String2Json(String s)
    {
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < s.Length; i++)
      {
        char c = s.ToCharArray()[i];
        switch (c)
        {
          case '\"':
            sb.Append("\\\""); break;
          case '\\':
            sb.Append("\\\\"); break;
          case '\'':
            sb.Append("\\'"); break;
          case '/':
            sb.Append("\\/"); break;
          case '\b':
            sb.Append("\\b"); break;
          case '\f':
            sb.Append("\\f"); break;
          case '\n':
            sb.Append("\\n"); break;
          case '\r':
            sb.Append("\\r"); break;
          case '\t':
            sb.Append("\\t"); break;
          default:
            sb.Append(c); break;
        }
      }
      return sb.ToString();
    }
    #endregion

	}
}
