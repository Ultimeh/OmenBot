using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace baseBot
{
	public class AppData
	{
		//public string aide = "";
		public Dictionary<string, int> UserList { get; set; } = new Dictionary<string, int>();
		//private List<data> _data = new List<data>();
		//public List<data> data { get { return _data; } set { _data = value; } }
	}

	//public class data
	//{
	//	private string _name = "";



	//	public string name { get { return _name; } set { _name = value; } }
	//}
}
