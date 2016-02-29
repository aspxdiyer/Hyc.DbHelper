using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Linq;
using System.Data;
using System.Reflection;
using System.Linq.Expressions;

namespace Hyc.DbDriver
{
  /// <summary>
  /// 参考：http://www.cnblogs.com/Airfeeling/archive/2011/09/14/1320397.html
  /// http://www.cnblogs.com/irenebbkiss/p/4157364.html
  /// </summary>
  public static class HycExFunc
  {
    private static Dictionary<string, string> tableDict;
    public static bool In<T>(this T obj, T[] array)
    {
      return true;
    }
    public static bool NotIn<T>(this T obj, T[] array)
    {
      return true;
    }
    public static bool Like(this string str, string likeStr)
    {
      return true;
    }
    public static bool NotLike(this string str, string likeStr)
    {
      return true;
    }
    public static string Where<T>(this T entity, Expression<Func<T, bool>> func, DataTable table = null) where T : class,new()
    {
      if (table == null)
        table = new AttributeHelper<T>().getEntityTable();
      tableDict = new Dictionary<string, string>();
      foreach (DataRow dr in table.Rows)
      {
        tableDict.Add(dr["ClassName"].ToString(), dr["Name"].ToString());
      }

      string where = "";
      if (func.Body is BinaryExpression)
      {
        BinaryExpression be = ((BinaryExpression)func.Body);
        where = BinarExpressionProvider(be.Left, be.Right, be.NodeType);
      }
      else if (func.Body is MethodCallExpression)
      {
        MethodCallExpression mce = (MethodCallExpression)func.Body;
        where = ExpressionMethodCall(mce);
      }
      else
        where = string.Empty;
      return where;
    }

    static string BinarExpressionProvider(Expression left, Expression right, ExpressionType type)
    {
      string sb = "(";
      //先处理左边
      sb += ExpressionRouter(left);
      sb += ExpressionTypeCast(type);

      //再处理右边
      string tmpStr = ExpressionRouter(right);
      //System.Web.HttpContext.Current.Response.Write(ExpressionRouter(right) + "<br/>");
      if (tmpStr == "null")
      {
        string fieldName = sb.Substring(0, sb.Length - 2);//tableDict[sb.Substring(0, sb.Length - 2).ToLower()];
        if (sb.EndsWith(" ="))
          sb = fieldName + " is null";
        else if (sb.EndsWith("<>"))
          sb = fieldName + " is not null";
      }
      else
        sb += tmpStr;
      sb += ")";
      return sb;
    }

    static string ExpressionRouter(Expression exp)
    {
      string sb = string.Empty;
      if (exp is BinaryExpression)
      {
        BinaryExpression be = ((BinaryExpression)exp);
        return BinarExpressionProvider(be.Left, be.Right, be.NodeType);
      }
      else if (exp is MemberExpression)
      {
        if (!exp.ToString().StartsWith("value("))
        {
          if (exp.Type == typeof(string))
          {
            MemberExpression me = ((MemberExpression)exp);
            string fieldName = tableDict.ContainsKey(me.Member.Name.ToLower()) ? tableDict[me.Member.Name.ToLower()] : me.Member.Name;

            return fieldName;
          }
          else
          {
            MemberExpression me = ((MemberExpression)exp);
            try
            {
              if (!exp.ToString().StartsWith("DateTime.") && !exp.ToString().StartsWith("int."))
              {
                string fieldName = tableDict.ContainsKey(me.Member.Name.ToLower()) ? tableDict[me.Member.Name.ToLower()] : me.Member.Name;
                return fieldName;
              }
              var result = Expression.Lambda(exp).Compile().DynamicInvoke();
              if (result == null)
              {
                string fieldName = tableDict.ContainsKey(me.Member.Name.ToLower()) ? tableDict[me.Member.Name.ToLower()] : me.Member.Name;
                return fieldName;
              }
              else if (result is string || result is char || result is DateTime)
                return string.Format("'{0}'", result.ToString());
              else if (result is ValueType)
                return result.ToString();
              else
                return result.ToString();
              //
            }
            catch
            {
              string fieldName = tableDict.ContainsKey(me.Member.Name.ToLower()) ? tableDict[me.Member.Name.ToLower()] : me.Member.Name;
              return fieldName;
            }
          }
        }
        else
        {
          var result = Expression.Lambda(exp).Compile().DynamicInvoke();
          if (result == null)
            return "null";
          else if (result is string || result is char || result is DateTime)
            return string.Format("'{0}'", result.ToString());
          else if (result is ValueType)
            return result.ToString();
        }
      }
      else if (exp is NewArrayExpression)
      {
        NewArrayExpression ae = ((NewArrayExpression)exp);
        StringBuilder tmpstr = new StringBuilder();
        foreach (Expression ex in ae.Expressions)
        {
          tmpstr.Append(ExpressionRouter(ex));
          tmpstr.Append(",");
        }
        return tmpstr.ToString(0, tmpstr.Length - 1);
      }
      else if (exp is MethodCallExpression)
      {
        MethodCallExpression mce = (MethodCallExpression)exp;
        return ExpressionMethodCall(mce);
      }
      else if (exp is ConstantExpression)
      {
        ConstantExpression ce = ((ConstantExpression)exp);
        if (ce.Value == null)
          return "null";
        else if (ce.Value is ValueType)
          return ce.Value.ToString();
        else if (ce.Value is string || ce.Value is DateTime || ce.Value is char)
          return string.Format("'{0}'", ce.Value.ToString());
      }
      else if (exp is UnaryExpression)
      {
        UnaryExpression ue = ((UnaryExpression)exp);
        return ExpressionRouter(ue.Operand);
      }

      try
      {
        var result = Expression.Lambda(exp).Compile().DynamicInvoke();
        if (result == null)
          return "null";
        else if (result is string || result is char || result is DateTime)
          return string.Format("'{0}'", result.ToString());
        else if (result is ValueType)
          return result.ToString();
      }
      catch
      {

      }
      return null;
    }

    static string ExpressionTypeCast(ExpressionType type)
    {
      switch (type)
      {
        case ExpressionType.And:
        case ExpressionType.AndAlso:
          return " and ";
        case ExpressionType.Equal:
          return " =";
        case ExpressionType.GreaterThan:
          return " >";
        case ExpressionType.GreaterThanOrEqual:
          return ">=";
        case ExpressionType.LessThan:
          return "<";
        case ExpressionType.LessThanOrEqual:
          return "<=";
        case ExpressionType.NotEqual:
          return "<>";
        case ExpressionType.Or:
        case ExpressionType.OrElse:
          return " or ";
        case ExpressionType.Add:
        case ExpressionType.AddChecked:
          return "+";
        case ExpressionType.Subtract:
        case ExpressionType.SubtractChecked:
          return "-";
        case ExpressionType.Divide:
          return "/";
        case ExpressionType.Multiply:
        case ExpressionType.MultiplyChecked:
          return "*";
        default:
          return null;
      }
    }

    static string ExpressionMethodCall(MethodCallExpression mce)
    {
      string fieldName = ExpressionRouter(mce.Arguments[0]);
      //System.Web.HttpContext.Current.Response.Write(mce.Arguments.Count);
      string arg = "";
      if (mce.Arguments.Count > 1)
      {
        arg = ExpressionRouter(mce.Arguments[1]);
        //System.Web.HttpContext.Current.Response.Write(mce.Arguments[1]);
        if (mce.Arguments[1].ToString().IndexOf("value(") > 0)
        {
          try
          {
            var result = Expression.Lambda(mce.Arguments[1]).Compile().DynamicInvoke();
            if (result is string || result is char || result is DateTime)
              arg = string.Format("'{0}'", result.ToString());
            else
              arg = result.ToString();
          }
          catch { }
        }
      }
      ////System.Web.HttpContext.Current.Response.Write(mce.Arguments[1] + "<br/>");

      if (mce.Method.Name == "Like")
        return string.Format("({0} like {1})", fieldName, arg);
      else if (mce.Method.Name == "NotLike")
        return string.Format("({0} not like {1})", fieldName, arg);
      else if (mce.Method.Name == "In")
        return string.Format("{0} in ({1})", fieldName, arg);
      else if (mce.Method.Name == "NotIn")
        return string.Format("{0} not in ({1})", fieldName, arg);
      else
        return string.Empty;

    }
  }


  public static class DbLinq
  {
    //static public string Test(List<T> list)
    //{
    //  string tableName = list.GetType().Name;
    //  foreach (T t in list)
    //  {

    //  }
    //  return tableName;
    //}

    static public string Create<T>(this T model) where T : class,new()
    {
      T _t = Activator.CreateInstance<T>();

      //获取对象所有属性
      PropertyInfo[] propertyInfo = _t.GetType().GetProperties();
      foreach (PropertyInfo propinfo in propertyInfo)
      {
        //System.Web.HttpContext.Current.Response.Write(propinfo.Name + "=" + propinfo.GetValue(model, null));
        System.Web.HttpContext.Current.Response.Write(propinfo.Name + "=" + GetObjectProperty(model, propinfo.Name));
      }
      //for (int j = 0; j < dt.Columns.Count; j++)
      //{
      //  foreach (PropertyInfo info in propertyInfo)
      //  {
      //  }
      //}
      return "";
    }

    static public string Find<T>(this T model, Func<T, bool> func) where T : class,new()
    {
      System.Web.HttpContext.Current.Response.Write("<br/>" + func(model) + "<br/>");
      return "111";
    }

    //public static void SetWhereFunc<T>(this T entity, Expression<Func<T, bool>> func) where T : baseEntity
    //{
    //  //来源：下次继续：http://www.cnblogs.com/Airfeeling/archive/2011/09/14/1320397.html
    //  entity.whereFunc = func;
    //}

    public static IEnumerable<TSource> MyWhere<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
      foreach (TSource s in source)
      {
        if (predicate(s))
        {
          yield return s;
        }
      }
    }


    private static string GetObjectProperty(object obj, string propertyName)
    {
      try
      {
        if (obj == null)
          return null;
        else
        {
          PropertyInfo p = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
          if (p == null)
            return null;
          else
            return Convert.ToString(p.GetValue(obj, null));
        }
      }
      catch
      {
        return null;
      }
    }
  }

  //class baseEntity
  //{
  //  internal Expression whereFunc;
  //}
}
