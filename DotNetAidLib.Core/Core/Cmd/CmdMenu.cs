using System;
using System.Collections.Generic;

namespace DotNetAidLib.Core.Cmd
{
    public class CmdMenu
    {
        private readonly IList<CmdMenu> _Items = new List<CmdMenu>();

        public CmdMenu()
        {
        }

        public CmdMenu(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public Action Action { get; set; }

        public CmdMenu ParentMenu { get; set; }

        public CmdMenu AddItem(CmdMenu menuItem)
        {
            _Items.Add(menuItem);
            menuItem.ParentMenu = this;
            return menuItem;
        }

        public CmdMenu AddItem(string menuName)
        {
            var menuItem = new CmdMenu {Name = menuName};
            return AddItem(menuItem);
        }

        public CmdMenu AddActionItem(string menuName, Action menuAction)
        {
            var menuItem = new CmdMenu {Name = menuName, Action = menuAction};
            _Items.Add(menuItem);
            menuItem.ParentMenu = this;
            return this;
        }

        public void Show()
        {
            string aux;
            int n;
            bool ok;

            do
            {
                Console.Clear();
                Console.WriteLine("Menu " + Name);
                Console.WriteLine();
                for (var i = 0; i < _Items.Count; i++)
                {
                    var subMenu = _Items[i];
                    Console.WriteLine(i + 1 + ".- " + subMenu.Name);
                }

                Console.WriteLine();
                Console.WriteLine("0 or ESC for exit.");

                ok = ConsoleHelper.ReadLineESC(out aux);
                if (!ok)
                    break;
                if (int.TryParse(aux, out n) && n > 0 && n <= _Items.Count)
                {
                    var subMenu = _Items[n - 1];
                    if (subMenu.Action != null)
                        subMenu.Action.Invoke();
                    else
                        subMenu.Show();
                }
            } while (aux != "0");
        }
    }
}