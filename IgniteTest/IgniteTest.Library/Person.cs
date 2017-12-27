using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;

namespace IgniteTest.Library
{
    public class Person
    {
        [QuerySqlField(IsIndexed = true)]
        public int Id { get; set; }
        [QuerySqlField]
        public string Name { get; set; }
        [QuerySqlField]
        public string Designation { get; set; }
        [QuerySqlField]
        public int Salary { get; set; }
        [QuerySqlField]
        public int Age { get; set; }

    }
    public class PersonFilter : ICacheEntryFilter<int, Person>
    {
        public int _Age { get; set; }
        public PersonFilter(int age) { _Age = age; }

        public bool Invoke(ICacheEntry<int, Person> entry)
        {
            return entry.Value.Age >= _Age;
        }
    }
}
