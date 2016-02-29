using System;
using System.Collections.Generic;
using System.Text;

namespace Hyc.DbDriver
{
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public class TableAttribute : Attribute
  {
    private string _tableName = "";
    public TableAttribute(string tableName)
    {
      _tableName = tableName;
    }


    // 获取该成员映射的数据库字段名称。
    public string TableName
    {
      get
      {
        return _tableName;
      }
      set
      {
        _tableName = value;
      }
    }

  }

  public class TableAttributeHelper<Entity> where Entity : class,new()
  {
    public static string TableName()
    {
      var attrs = new Entity().GetType().GetCustomAttributes(true);
      if (attrs.Length > 0)
      {

        if (attrs[0].GetType() == typeof(Hyc.DbDriver.TableAttribute))
        {
          TableAttribute attr = (TableAttribute)attrs[0];
          if (!string.IsNullOrEmpty(attr.TableName.Trim()))
          {
            return attr.TableName.Trim();
          }
        }
      }
      return "";
    }
  }

  public class TableAttributeHelper<Entity>
  {
    public static string TableName()
    {
      var attrs = default(Entity).GetType().GetCustomAttributes(true);
      if (attrs.Length > 0)
      {

        if (attrs[0].GetType() == typeof(Hyc.DbDriver.TableAttribute))
        {
          TableAttribute attr = (TableAttribute)attrs[0];
          if (!string.IsNullOrEmpty(attr.TableName.Trim()))
          {
            return attr.TableName.Trim();
          }
        }
      }
      return "";
    }
  }

}
