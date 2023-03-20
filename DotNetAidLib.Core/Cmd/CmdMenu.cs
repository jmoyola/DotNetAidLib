using System;
using System.Collections.Generic;

namespace DotNetAidLib.Core.Cmd
{
	public class CmdMenu
	{
		private IList<CmdMenu> _Items = new List<CmdMenu>();

		public CmdMenu()
		{}

		public CmdMenu(String name)
		{
			this.Name = name;
		}

		public String Name { get; set; }

		public Action Action { get; set; }

		public CmdMenu ParentMenu { get; set; }

		public CmdMenu AddItem(CmdMenu menuItem) {
			_Items.Add(menuItem);
			menuItem.ParentMenu = this;
			return menuItem;
		}

		public CmdMenu AddItem(String menuName)
		{
			CmdMenu menuItem = new CmdMenu() { Name = menuName};
			return this.AddItem(menuItem);
		}

		public CmdMenu AddActionItem(String menuName, Action menuAction)
		{
			CmdMenu menuItem = new CmdMenu() { Name = menuName, Action = menuAction };
			_Items.Add(menuItem);
			menuItem.ParentMenu = this;
			return this;
		}

		public void Show() {
			String aux;
			int n;
            bool ok;

            do
            {
				Console.Clear();
				Console.WriteLine("Menu " + this.Name);
				Console.WriteLine();
				for (int i = 0; i < _Items.Count; i++) {
					CmdMenu subMenu = _Items[i];
					Console.WriteLine(i+1 + ".- " + subMenu.Name);
				}
				Console.WriteLine();
				Console.WriteLine("0 or ESC for exit.");

			    ok = ConsoleHelper.ReadLineESC(out aux);
                if (!ok)
                    break;
				if (Int32.TryParse(aux, out n) && n>0 && n<=_Items.Count){
					CmdMenu subMenu = _Items[n - 1];
					if (subMenu.Action != null)
						subMenu.Action.Invoke();
					else
						subMenu.Show();
				}
				
			} while (aux != "0");
		}

	}
}
