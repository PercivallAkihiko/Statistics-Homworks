using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Wireshark_Statistic
{
    internal class Address
    {
        public string Id { get; set; }
        public Dictionary<string, (int, int )> Connected { get; set; }
        public int Index { get; set; }

        public Address(string id, int index)
        {
            Id = id;
            Connected = new Dictionary<string, (int, int)>();
            Index = index;
        }

        public void AddNew(string id, int index)
        {
            Connected.Add(id, (index, 0));
        }

        public void Increase(string id)
        {
            int index = Connected[id].Item1;
            int value = Connected[id].Item2;
            Connected[id] = (index, value + 1);
        }

        public bool Check(string id)
        {
            if(Connected.ContainsKey(id)) return true;
            return false;
        }

    }
}
