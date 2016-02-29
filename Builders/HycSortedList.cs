using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Hyc.DbDriver.Builders
{
  
  public class HycComparer : IComparer
  {
    public int Compare(object x, object y)
    {
      return -1;
    }
  }

	public class HycSortedList:SortedList
	{
    public HycSortedList():base(new HycComparer()){}
  }
}
