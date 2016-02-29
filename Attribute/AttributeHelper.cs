using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Reflection;

namespace Hyc.DbDriver
{
	public class AttributeHelper<Entity> where Entity:class,new()
	{
    public DataTable getEntityTable(){
      PropertyInfo[] Properties = new Entity().GetType().GetProperties();//类型属性集
      DataTable myTable = new DataTable(new Entity().GetType().Name);
      myTable.Columns.Add("Name", System.Type.GetType("System.String"));
      myTable.Columns.Add("ClassName", System.Type.GetType("System.String"));
      myTable.Columns.Add("Explain", System.Type.GetType("System.String"));
      myTable.Columns.Add("Type", System.Type.GetType("System.Type"));
      myTable.Columns.Add("Length", System.Type.GetType("System.Int32"));
      myTable.Columns.Add("Usage", System.Type.GetType("System.Int32"));
      myTable.Columns.Add("DefaultValue", System.Type.GetType("System.Object"));
      myTable.Columns.Add("Null", System.Type.GetType("System.Boolean"));
      myTable.Columns.Add("UrlEncode", System.Type.GetType("System.Boolean"));

      
      foreach(PropertyInfo p in Properties){
        DataRow dr = myTable.NewRow();
        string pn = p.Name.ToLower();
        object defaultValue = "";
        int Usage = 0;
        int Length = 0;
        string Explain = "";
        bool Null = true;
        bool UrlEncode = true;
        object[] attrs = p.GetCustomAttributes(typeof(FieldAttribute), true);
        if (attrs.Length == 1)
        {
          FieldAttribute attr = (FieldAttribute)attrs[0];
          if(!string.IsNullOrEmpty(attr.FieldName.Trim())){
            pn = attr.FieldName.Trim().ToLower();
          }
          else
          {
            pn = p.Name;
          }
          if(attr.DefaultValue != null){
            defaultValue = attr.DefaultValue;
          }
          if(attr.Usage != null){
            Usage = (int)attr.Usage;
          }
          if(attr.Length != 0){
            Length = attr.Length;
          }
          if(!string.IsNullOrEmpty(attr.Description)){
            Explain = attr.Description.Trim();
          }
          if (attr.Null != null)
          {
            Null = attr.Null;
          }
          if (attr.UrlEncode != null)
          {
            UrlEncode = attr.UrlEncode;
          }          
        }
        dr["Name"] = pn;
        dr["ClassName"] = p.Name.ToLower();
        dr["Explain"] = Explain;
        dr["Type"] = p.PropertyType;
        dr["Length"] = Length;
        dr["Usage"] = Usage;
        dr["DefaultValue"] = defaultValue;
        dr["Null"] = Null;
        dr["UrlEncode"] = UrlEncode;
        myTable.Rows.Add(dr);
      }
      return myTable;
    }
	}
}
