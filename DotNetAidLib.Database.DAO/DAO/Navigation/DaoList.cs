using System;
using System.Collections.Generic;
using System.Reflection;

namespace DotNetAidLib.Database.DAO.Navigation
{
	public class DaoList<T> : List<T>
	{

		private DaoList()
		{
		}

		public DaoList<T> Retrieve () {
			return this;
		}
	}
}
