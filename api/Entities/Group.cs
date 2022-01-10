using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dating_WebAPI.Entities
{
	public class Group
	{
        // for entity framework constructor, when create a table, it need it
        public Group() { }


        // Connections has default value, wont need to consturct
        public Group(string name)
        {
            Name = name;
        }

        [Key]
        public string Name { get; set; }

        // 給予起始值讓我們可以直接加入connection
        public ICollection<Connection> Connections { get; set; } = new List<Connection>();
    }
}

