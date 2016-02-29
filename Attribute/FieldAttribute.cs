using System;
using System.Collections.Generic;
using System.Text;

namespace Hyc.DbDriver
{
    /// <summary>
    /// 数据库字段的用途。
    /// </summary>
    public enum EnumFieldUsage
    {
      /// <summary>
      /// 未定义。
      /// </summary>
      None = 0x00,

      /// <summary>
      /// 主键。
      /// </summary>
      PrimaryKey = 0x01,

      /// <summary>
      /// 唯一键。
      /// </summary>
      UniqueKey = 0x02,
      
      /// <summary>
      /// 自增键。
      /// </summary>
      IncKey = 0x03,

      /// <summary>
      /// 由系统控制该字段的值。
      /// </summary>
      BySystem = 0x04,

      /// <summary>
      /// 自增主键。
      /// </summary>
      IncPK = 0x05
    }

   /* 
      DalObj dalObj = new DalObj("users");
      StringBuilder sb = new StringBuilder();
      foreach (PropertyInfo proInfo in dalObj.GetType().GetProperties())
      {
        object[] attrs = proInfo.GetCustomAttributes(typeof(FieldAttribute), true);
        if (attrs.Length == 1)
        {
          FieldAttribute attr = (FieldAttribute)attrs[0];
          sb.Append(attr.FieldName + ":" + (attr.DefaultValue == null ? "null" : attr.DefaultValue.ToString()) + "\r\n");
        }
      }
      MessageBox.Show(sb.ToString());
   */
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false,Inherited = false )]   
  public class FieldAttribute : Attribute   
  {
    EnumFieldUsage m_usage;
    string m_strFieldName;
    string m_strDescription;
    object m_defaultValue;

    public FieldAttribute(string strFieldName, object defaultValue, EnumFieldUsage usage, string strDescription, int length, bool Null, bool UrlEncode)
    {
      m_strFieldName = strFieldName;
      m_defaultValue = defaultValue;
      m_usage = usage;
      m_strDescription = strDescription;
      Length = length;
      this.Null = Null;
      this.UrlEncode = UrlEncode;
    }

    public FieldAttribute(string fieldName) : this(fieldName,null, EnumFieldUsage.None,null,0,true,true)
    { }

    public FieldAttribute(string fieldName, EnumFieldUsage usage)
      : this(fieldName, null, usage, null, 0, true, true)
    { }

    public FieldAttribute(string fieldName, EnumFieldUsage usage, int length)
      : this(fieldName, null, usage, null, length, true, true)
    { }
    
    // 获取该成员映射的数据库字段名称。
    public string FieldName
    {
      get
      {
        return m_strFieldName;
      }
      set
      {
        m_strFieldName = value;
      }
    }

    /// <summary>
    /// 字段长度
    /// </summary>
    public int Length{get;set;}

    /// <summary>
    /// 是否允许为空，默认为（true允许）
    /// </summary>
    public bool Null{get;set;}

    /// <summary>
    /// 是否自动转换(为true是超过200个字符的自动转换html编码)
    /// </summary>
    public bool UrlEncode { get; set; }

    /// <summary>
    /// 说明
    /// </summary>
    public string Description
    {
      get
      {
        return m_strDescription;
      }
      set
      {
        m_strDescription = value;
      }
    }
    
    // 获取该字段的默认值
    public object DefaultValue
    {
      get
      {
        return m_defaultValue;
      }
      set 
      {
        m_defaultValue = value;
      }
    }

    // 获取该字段的数据库用途
    public EnumFieldUsage Usage
    {
      get
      {
        return m_usage;
      }
      set 
      {
        m_usage = value;
      }
    }


   
  }
}