using AspxFrameWork.DataHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;

namespace Hyc.DbDriver
{
  public class LinqHelper
  {
    private Dictionary<string, object> Argument;
    public string SqlWhere;
    public DbParameter[] Paras;

    private DataTable entityTable;
    private string DbProvider;

    private Dictionary<string, string> tableDict;

    /// <summary>
    /// 解析lamdba，生成Sql查询条件
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    public void ResolveExpression(Expression expression, DataTable entityTable, string DbProvider)
    {
      this.Argument = new Dictionary<string, object>();
      this.entityTable = entityTable;
      this.DbProvider = DbProvider;

      tableDict = new Dictionary<string, string>();
      foreach (DataRow dr in entityTable.Rows)
      {
        tableDict.Add(dr["ClassName"].ToString(), dr["Name"].ToString());
      }

      this.SqlWhere = Resolve(expression);

      this.Paras = Argument.Select(x =>
      {
        return SqlHelper.MakeObjectParam(x.Key, x.Value);
        //return new SqlParameter(x.Key, x.Value); 
      }).ToArray();
    }

    private string Resolve(Expression expression)
    {
      if (expression is LambdaExpression)
      {
        LambdaExpression lambda = expression as LambdaExpression;
        expression = lambda.Body;
        //return Resolve(expression);
      }
      if (expression is BinaryExpression)//二元运算符
      {
        BinaryExpression binary = expression as BinaryExpression;
        if (binary.Left is MemberExpression && binary.Right is ConstantExpression)//解析x=>x.Name=="123" x.Age==123这类
          return ResolveFunc(binary.Left, binary.Right, binary.NodeType);
        if (binary.Left is MethodCallExpression && binary.Right is ConstantExpression)//解析x=>x.Name.Contains("xxx")==false这类的
        {
          object value = (binary.Right as ConstantExpression).Value;
          return ResolveLinqToObject(binary.Left, value, binary.NodeType);
        }
        if (binary.Left is MemberExpression && (binary.Right is MemberExpression || binary.Right is MethodCallExpression))//解析x=>x.Date==DateTime.Now这种
        {
          LambdaExpression lambda = Expression.Lambda(binary.Right);
          Delegate fn = lambda.Compile();
          ConstantExpression value = Expression.Constant(fn.DynamicInvoke(null), binary.Right.Type);
          return ResolveFunc(binary.Left, value, binary.NodeType);
        }
      }
      if (expression is UnaryExpression)//一元运算符
      {
        UnaryExpression unary = expression as UnaryExpression;
        if (unary.Operand is MethodCallExpression)//解析!x=>x.Name.Contains("xxx")或!array.Contains(x.Name)这类
          return ResolveLinqToObject(unary.Operand, false);
        //if (unary.Operand is MemberExpression && unary.NodeType == ExpressionType.Not)//解析x=>!x.isDeletion这样的 
        //{
        //  ConstantExpression constant = Expression.Constant(false);
        //  return ResolveFunc(unary.Operand, constant, ExpressionType.Equal);
        //}
        if (unary.Operand is MemberExpression)//解析x=>!x.isDeletion这样的 
        {
          bool value = (unary.NodeType == ExpressionType.Not ? false : true);
          ConstantExpression constant = Expression.Constant(value);
          return ResolveFunc(unary.Operand, constant, ExpressionType.Equal);
        }
      }
      if (expression is MemberExpression && expression.NodeType == ExpressionType.MemberAccess)//解析x=>x.isDeletion这样的 
      {
        MemberExpression member = expression as MemberExpression;
        ConstantExpression constant = Expression.Constant(true);
        return ResolveFunc(member, constant, ExpressionType.Equal);
      }
      if (expression is MethodCallExpression)//x=>x.Name.Contains("xxx")或array.Contains(x.Name)这类
      {
        MethodCallExpression methodcall = expression as MethodCallExpression;

        return ResolveLinqToObject(methodcall, true);
      }
      var body = expression as BinaryExpression;
      if (body == null)
        throw new Exception("无法解析" + expression);
      var Operator = GetOperator(body.NodeType);
      var Left = Resolve(body.Left);
      var Right = Resolve(body.Right);
      string Result = string.Format("({0} {1} {2})", getFieldName(Left), Operator, Right);
      return Result;
    }

    /// <summary>
    /// 根据条件生成对应的sql查询操作符
    /// </summary>
    /// <param name="expressiontype"></param>
    /// <returns></returns>
    private string GetOperator(ExpressionType expressiontype)
    {
      switch (expressiontype)
      {
        case ExpressionType.And:
          return "and";
        case ExpressionType.AndAlso:
          return "and";
        case ExpressionType.Or:
          return "or";
        case ExpressionType.OrElse:
          return "or";
        case ExpressionType.Equal:
          return "=";
        case ExpressionType.NotEqual:
          return "<>";
        case ExpressionType.LessThan:
          return "<";
        case ExpressionType.LessThanOrEqual:
          return "<=";
        case ExpressionType.GreaterThan:
          return ">";
        case ExpressionType.GreaterThanOrEqual:
          return ">=";
        default:
          throw new Exception(string.Format("不支持{0}此种运算符查找！" + expressiontype));
      }
    }

    private string ResolveFunc(Expression left, Expression right, ExpressionType expressiontype)
    {
      var Name = (left as MemberExpression).Member.Name;
      var Value = (right as ConstantExpression).Value;
      var Operator = GetOperator(expressiontype);
      string CompName = SetArgument(Name, Value.ToString());
      string Result = string.Format("({0} {1} {2})", getFieldName(Name), Operator, CompName);
      return Result;
    }

    private string ResolveLinqToObject(Expression expression, object value, ExpressionType? expressiontype = null)
    {
      var MethodCall = expression as MethodCallExpression;
      var MethodName = MethodCall.Method.Name;
      switch (MethodName)//这里其实还可以改成反射调用，不用写switch
      {
        case "Contains":
          if (MethodCall.Object != null)
            return Contains(MethodCall, 0);
          return In(MethodCall, value);
        case "Count":
          return Len(MethodCall, value, expressiontype.Value);
        case "LongCount":
          return Len(MethodCall, value, expressiontype.Value);
        case "StartsWith":
          return Contains(MethodCall, 1);
        case "EndsWith":
          return Contains(MethodCall, 2);
        case "Like":
          return Like(MethodCall, -1);
        default:
          return Call(MethodCall);
        //throw new Exception(string.Format("不支持{0}方法的查找！", MethodName));
      }
    }


    private string In(MethodCallExpression expression, object isTrue)
    {
      var arg1 = expression.Arguments[0] as MemberExpression;
      var Argument1 = arg1.Expression as ConstantExpression;
      var Names = arg1.Member.Name;

      var Argument2 = expression.Arguments[1] as MemberExpression;

      var Field_Array = Argument1.Value.GetType().GetField(Names);
      var Field_Value = Field_Array.GetValue(Argument1.Value);

      string Name = Argument2.Member.Name;
      List<string> SetInPara = new List<string>();
      List<object> list = new List<object>();
      if (Field_Array.FieldType == typeof(int[]))
      {
        int[] Array = (int[])Field_Value;
        foreach (int v in Array)
        {
          list.Add(v);
        }
      }
      else if (Field_Array.FieldType == typeof(DateTime[]))
      {
        DateTime[] Array = (DateTime[])Field_Value;

        foreach (DateTime v in Array)
        {
          list.Add(v);
        }
      }
      else
      {
        object[] Array = (object[])Field_Value;

        foreach (object v in Array)
        {
          list.Add(v);
        }
      }
      for (int i = 0; i < list.Count; i++)
      {
        string Name_para = Name + "_" + Names + "_" + i;
        string Value = list[i].ToString();
        string Key = SetArgument(Name_para, Value);
        SetInPara.Add(Key);
      }
      string Operator = Convert.ToBoolean(isTrue) ? "in" : " not in";
      string CompName = string.Join(",", SetInPara.ToArray());
      string Result = string.Format("{0} {1} ({2})", getFieldName(Name), Operator, CompName);
      return Result;
    }

    private string Contains(MethodCallExpression expression, int format = 0)
    {
      var arg = expression.Arguments[0];
      object Temp_Vale = null;
      string member_Name = "";
      if (arg.ToString().StartsWith("value("))
      {
        Temp_Vale = Expression.Lambda(arg).Compile().DynamicInvoke();
      }
      else
      {
        if (expression.Arguments.Count > 1)
        {
          member_Name = ((MemberExpression)expression.Arguments[0]).Member.Name;
          Temp_Vale = (expression.Arguments[1] as ConstantExpression).Value;
        }
        else
        {
          Temp_Vale = (expression.Arguments[0] as ConstantExpression).Value;
        }
      }

      string formatString = "%{0}%";
      string Name = "";
      if (string.IsNullOrEmpty(member_Name))
      {
        Name = (expression.Object as MemberExpression).Member.Name;
        if (format == 1)//startswith
        {
          formatString = "{0}%";
        }
        else if (format == 2)
        {
          formatString = "%{0}";
        }
      }
      else
      {
        Name = member_Name;
        formatString = "{0}";
      }
      string Value = string.Format(formatString, Temp_Vale);

      string CompName = SetArgument(Name, Value);
      string Result = string.Format("{0} like {1}", getFieldName(Name), CompName);
      return Result;
    }

    private string Like(MethodCallExpression expression, int format = 0)
    {
      var arg = expression.Arguments[0];
      object Temp_Vale = null;
      string member_Name = "";
      if (expression.Arguments.Count > 1)
      {
        member_Name = ((MemberExpression)expression.Arguments[0]).Member.Name;
        if (expression.Arguments[1].ToString().IndexOf("value(") > 0)
        {
          try
          {
            var result = Expression.Lambda(expression.Arguments[1]).Compile().DynamicInvoke();
            if (result is string || result is char || result is DateTime)
              Temp_Vale = result.ToString();
            else
              Temp_Vale = result.ToString();
          }
          catch { }
        }
      }
      else
      {
        Temp_Vale = (expression.Arguments[0] as ConstantExpression).Value;
      }

      string formatString = "%{0}%";
      string Name = "";
      if (string.IsNullOrEmpty(member_Name))
      {
        Name = (expression.Object as MemberExpression).Member.Name;
        if (format == 1)//startswith
        {
          formatString = "{0}%";
        }
        else if (format == 2)
        {
          formatString = "%{0}";
        }
      }
      else
      {
        Name = member_Name;
        formatString = "{0}";
      }
      string Value = string.Format(formatString, Temp_Vale);

      string CompName = SetArgument(Name, Value);
      string Result = string.Format("{0} like {1}", getFieldName(Name), CompName);
      return Result;
    }

    private string Len(MethodCallExpression expression, object value, ExpressionType expressiontype)
    {
      object Name = (expression.Arguments[0] as MemberExpression).Member.Name;
      string Operator = GetOperator(expressiontype);
      string CompName = SetArgument(Name.ToString(), value.ToString());
      string Result = string.Format("len({0}){1}{2}", getFieldName(Name), Operator, CompName);
      return Result;
    }

    private string Call(MethodCallExpression expression)
    {
      object value = null;
      var result = Expression.Lambda(expression).Compile().DynamicInvoke();
      if (result == null)
        return null;
      else if (result is string || result is char || result is DateTime)
        value = result;
      else if (result is ValueType)
        value = result.ToString();

      return value.ToString();
      object Name = (expression.Arguments[0] as MemberExpression).Member.Name;
      string Operator = "=";// GetOperator(expressiontype);
      string CompName = SetArgument(Name.ToString(), value.ToString());
      string Result = string.Format("len({0}){1}{2}", getFieldName(Name), Operator, CompName);
      return Result;
    }

    /// <summary>
    /// 获取字段名称
    /// </summary>
    /// <param name="Name"></param>
    /// <returns></returns>
    private string getFieldName(object Name)
    {
      string fieldName = Name.ToString();
      if (tableDict.ContainsKey(Name.ToString().ToLower()))
      {
        fieldName = tableDict[Name.ToString().ToLower()];
      }
      switch (DbProvider.ToLower())
      {
        case "mysql":
          fieldName = "`" + fieldName + "`";
          break;
        default:
          fieldName = "[" + fieldName + "]";
          break;
      }
      return fieldName;
    }

    /// <summary>
    /// 配置
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    private string SetArgument(string name, string value)
    {
      string Prefix = "@";
      switch (DbProvider.ToLower())
      {
        case "mysql":
          Prefix = "?";
          break;
        default:
          Prefix = "@";
          break;
      }
      name = Prefix + name;
      string temp = name;
      while (Argument.ContainsKey(temp))
      {
        int code = Guid.NewGuid().GetHashCode();
        if (code < 0)
          code *= -1;
        temp = name + code;
      }
      Argument[temp] = value;
      return temp;
    }

  }
}
