using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Apache.Ignite.Core;
using Apache.Ignite.Linq;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Newtonsoft.Json;
using Apache.Ignite.Core.Cache.Query;
using Newtonsoft.Json.Converters;
using IgniteTest.Library;
namespace IgniteTest
{
    public partial class Form1 : Form
    {
        //not referred in code. 
        public static string configURL = @"D:\GitRepository\Ignite\IgniteTest\IgniteTest\ignite-config.xml";

        public Form1()
        {
            InitializeComponent();
            LoadInitialData();
        }

        #region Test connection
        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                Ignition.ClientMode = true;
                using (var ignite = Ignition.StartFromApplicationConfiguration("igniteConfiguration"))
                {
                    txtMsg.Text = "Connection successful.......:)";
                    ICache<int, string> cache = ignite.GetOrCreateCache<int, string>("test");
                    cache.Put(1, "Hello, World!");
                }
                
            }
            catch (Exception ex)
            {
                txtMsg.Text = ex.ToString();
            }
        }
        #endregion

        #region binary form data
        //set binary data into cache
        private void btnInsert_Click(object sender, EventArgs e)
        {
            try
            {
                List<Person> lstData = GeneratePersonData(5);
                var ccfg = new CacheConfiguration
                {
                    Name = "BINARY_DATA",
                    CacheMode = CacheMode.Replicated
                };

                var cfg = new IgniteConfiguration
                {
                    SpringConfigUrl = configURL,
                    BinaryConfiguration = new BinaryConfiguration(typeof(Person), typeof(PersonFilter))
                };
                Ignition.ClientMode = true;
                using (var ignite = Ignition.Start(cfg))
                {
                    ICache<int, IBinaryObject> persons = ignite.GetOrCreateCache<int, object>(ccfg).WithKeepBinary<int, IBinaryObject>();

                    int i = 1;
                    foreach(Person p in lstData)
                    {
                        IBinaryObject person = ignite.GetBinary()
                        .GetBuilder("Person")
                        .SetIntField("Power", p.Id)
                        .SetStringField("Name", p.Name)
                        .SetIntField("Age", p.Age)
                        .SetStringField("Designation", p.Designation)
                        .SetIntField("Salary", p.Salary)
                        .Build();

                        persons.Put(i++, person);
                    }
                }

                txtMsg.Text = "Binary data pushed into cache 'BINARY_DATA' heap.........";
            }
            catch (Exception ex)
            {
                txtMsg.Text = ex.Message;
            }
        }

        //get all binary data
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                List<Person> lstData = new List<Person>();
                var ccfg = new CacheConfiguration
                {
                    Name = "BINARY_DATA",
                    CacheMode = CacheMode.Replicated
                };

                var cfg = new IgniteConfiguration
                {
                    SpringConfigUrl = configURL,
                    BinaryConfiguration = new BinaryConfiguration(typeof(Person),typeof(PersonFilter))
                };
                Ignition.ClientMode = true;
                using (var ignite = Ignition.Start(cfg))
                {
                    ICache<int, IBinaryObject> persons = ignite.GetOrCreateCache<int, Person>(ccfg).WithKeepBinary<int, IBinaryObject>();
                    //var cache = ignite.GetOrCreateCache<int, Person>(ccfg);

                    //getting single object with key
                    IBinaryObject p = persons.Get(1);

                    //casting issue in iteration.. 
                    //foreach (IBinaryObject item in persons)
                    //{
                    //    int i = 0;
                    //}
                    //further coding
                }

                txtMsg.Text = "Binary data pushed into cache 'BINARY_DATA' heap.........";
            }
            catch (Exception ex)
            {
                txtMsg.Text = ex.Message;
            }

        }



        #endregion

        #region Scan query
        //Insert Data for Query
        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                List<Person> lstData = GeneratePersonData(5);

                var ccfg = new CacheConfiguration
                {
                    Name = "SCAN_DATA",
                    CacheMode= CacheMode.Replicated
                };

                var cfg = new IgniteConfiguration
                {
                    BinaryConfiguration = new BinaryConfiguration(typeof(Person), typeof(PersonFilter)),
                };
                Ignition.ClientMode = true;
                using (var ignite = Ignition.StartFromApplicationConfiguration("igniteConfiguration"))
                {
                    ICache<int, Person> persons = ignite.GetOrCreateCache<int, Person>(ccfg);

                    int i = 1;
                    foreach (Person p in lstData)
                    {
                        persons.Put(i++, p);
                    }
                }

                txtMsg.Text = "Data pushed into cache 'SCAN_DATA' heap.........";
                dgCacheData.DataSource = lstData;
            }
            catch (Exception ex)
            {
                txtMsg.Text = ex.ToString();
            }
        }

        //load from cache
        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                List<Person> lstData = new List<Person>();
                var ccfg = new CacheConfiguration
                {
                    Name = "SCAN_DATA",
                    CacheMode = CacheMode.Replicated
                };

                var cfg = new IgniteConfiguration
                {
                    BinaryConfiguration = new BinaryConfiguration(typeof(Person), typeof(PersonFilter)),
                };

                Ignition.ClientMode = true;
                using (var ignite = Ignition.StartFromApplicationConfiguration("igniteConfiguration"))
                {
                    ICache<int, Person> persons = ignite.GetOrCreateCache<int, Person>(ccfg);

                    //getting single record-- working
                    //lstData.Add(persons.Get(1));

                    //all record coming - working
                    List<ICacheEntry<int, Person>> lst1 = persons.ToList<ICacheEntry<int, Person>>();
                    foreach (ICacheEntry<int, Person> p in lst1)
                    {
                        lstData.Add(new Person() { Age = p.Value.Age, Designation = p.Value.Designation, Id = p.Value.Id, Name = p.Value.Name, Salary = p.Value.Salary });
                    }

                    dgCacheData.DataSource = lstData;
                    lstData.Clear();


                    //filter records
                    var scanQuery = new ScanQuery<int, Person>(new PersonFilter(Convert.ToInt32(txtAge.Text)));
                    IQueryCursor<ICacheEntry<int, Person>> queryCursor = persons.Query(scanQuery);

                    int j = 0;
                    foreach (ICacheEntry<int, Person> p in queryCursor)
                    {
                        j++;
                        lstData.Add(new Person() { Age=p.Value.Age, Designation=p.Value.Designation, Id=p.Value.Id, Name=p.Value.Name, Salary=p.Value.Salary });
                    }
                    txtMsg.Text = j.ToString() + " records found";
                }
                
                dgFilter.DataSource = lstData;

            }
            catch (Exception ex)
            {
                txtMsg.Text = ex.ToString();
            }
        }

        #endregion

        #region common & data generation
        private void LoadInitialData()
        {
            //dgInitialData.DataSource = GetPersonList();
            
        }
        private List<Person> GetPersonList()
        {
            List<Person> persons = new List<Person>();

            persons.Add(new Person() { Id = 12345, Name = "Afzal", Designation = "Developer" });
            persons.Add(new Person() { Id = 12346, Name = "Mohan", Designation = "Developer" });
            persons.Add(new Person() { Id = 12347, Name = "Subhash", Designation = "Developer" });
            persons.Add(new Person() { Id = 12348, Name = "Manoj", Designation = "Developer" });
            persons.Add(new Person() { Id = 12349, Name = "Mariena", Designation = "Developer" });

            return persons;
        }

        private List<Person> GeneratePersonData(int count)
        {
            List<Person> persons = new List<Person>();
            Random rand = new Random();
            for (int i = 1; i <= count; i++)
            {
                persons.Add(new Person() { Id = i * 100, Name = "Person_" + i.ToString(), Designation = "Developer", Age = rand.Next(25, 60), Salary = rand.Next(5000, 25000) });
            }

            return persons;
        }

        #endregion

        #region Dynamic Class column data
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {

                var ccfg = new CacheConfiguration
                {
                    Name = "Persons"//,
                };

                var cfg = new IgniteConfiguration
                {
                    BinaryConfiguration = new Apache.Ignite.Core.Binary.BinaryConfiguration(typeof(Person)),
                    SpringConfigUrl = @"D:\GitRepository\IgniteTest\IgniteTest\ignite-config.xml"
                };

                //Build Dynamic Class based on data
                List<dynamic> data = GetData();

                //Ignition.ClientMode = true;
                //using (var ignite = Ignition.Start(cfg))
                //{
                //    ICache<int, IBinaryObject> cars = ignite.GetOrCreateCache<int, object>(ccfg).WithKeepBinary<int, IBinaryObject>();
                //    for (int i = 1; i <= 10; i++)
                //    {
                //        IBinaryObject car = ignite.GetBinary()
                //        .GetBuilder("Car")
                //        .SetStringField("Name", "Honda NSX_" + i.ToString())
                //        .SetIntField("Power", 600)
                //        .Build();

                //        // Put an entry to Ignite cache, this causes AdoNetCacheStore.Write call.
                //        cars.Put(i, car);
                //    }
                //}
                txtMsg.Text = "coding not completed";

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private List<dynamic> GetData()
        {
            List<dynamic> dataList = new List<dynamic>();

            //generate object
            dynamic dataObj = new ExpandoObject();
            for (int i = 1; i <=3; i++)
            {
                AddProperty(dataObj, "prop_" + i.ToString(), string.Empty);
                
            }

            //add data to object
            List<Person> persons = GetPersonList();
            //assign values of person class to dynamic object

            DataTable dt = new DataTable();
            dt.Columns.Add("UserId", typeof(Int32));
            dt.Columns.Add("UserName", typeof(string));
            dt.Columns.Add("Education", typeof(string));
            dt.Columns.Add("Location", typeof(string));
            dt.Rows.Add(1, "Satinder Singh", "Bsc Com Sci", "Mumbai");
            dt.Rows.Add(2, "Amit Sarna", "Mstr Com Sci", "Mumbai");
            dt.Rows.Add(3, "Andrea Ely", "Bsc Bio-Chemistry", "Queensland");
            dt.Rows.Add(4, "Leslie Mac", "MSC", "Town-ville");
            dt.Rows.Add(5, "Vaibhav Adhyapak", "MBA", "New Delhi");

            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            //return JSONString;

            return dataList;
        }
        

        private void AddProperty(ExpandoObject obj, string propName, string propValue)
        {
            var newProp = obj as IDictionary<string, object>;
            newProp.Add(propName, propValue);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            txtMsg.Text = "coding not completed";
        }
        #endregion

        #region JSON Data to cache
        private void button1_Click(object sender, EventArgs e)
        {
            List<Person> lstData;
            try
            {
                lstData = GeneratePersonData(10);
                var json = JsonConvert.SerializeObject(lstData);

                var cfg = new IgniteConfiguration
                {
                    SpringConfigUrl = @"D:\GitRepository\IgniteTest\IgniteTest\ignite-config.xml"

                };
                Ignition.ClientMode = true;
                using (var ignite = Ignition.Start(cfg))
                {
                    ICache<int, string> cache = ignite.GetOrCreateCache<int, string>("JSON_DATA");
                    cache.Put(1, json);
                }

                txtMsg.Text = "JSON data pushed into cache 'JSON_DATA' heap........";
                dgCacheData.DataSource = lstData;
            }
            catch (Exception ex)
            {
                txtMsg.Text = ex.ToString();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            List<Person> lstData;
            string strJSON;
            try
            {
                var cfg = new IgniteConfiguration
                {
                    SpringConfigUrl = @"D:\GitRepository\IgniteTest\IgniteTest\ignite-config.xml"

                };
                Ignition.ClientMode = true;
                using (var ignite = Ignition.Start(cfg))
                {
                    ICache<int, string> cache = ignite.GetOrCreateCache<int, string>("JSON_DATA");
                    strJSON = cache.Get(1);
                }

                lstData = (List<Person>) JsonConvert.DeserializeObject(strJSON, typeof(List<Person>));

                dgCacheData.DataSource = lstData;
                
            }
            catch (Exception ex)
            {
                txtMsg.Text = ex.Message;
            }
        }



        #endregion

        #region SQL Query
        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                List<Person> lstData = GeneratePersonData(5);
                var cfg = new IgniteConfiguration
                {
                    BinaryConfiguration = new BinaryConfiguration(typeof(Person), typeof(PersonFilter)),
                };
                Ignition.ClientMode = true;
                using (var ignite = Ignition.StartFromApplicationConfiguration("igniteConfiguration"))
                {
                    ICache<int, Person> persons = ignite.GetOrCreateCache<int, Person>(new CacheConfiguration("SQL_DATA", typeof(Person),typeof(PersonFilter)));

                    int i = 1;
                    foreach (Person p in lstData)
                    {
                        persons.Put(i++, p);
                    }
                }

                txtMsg.Text = "Data pushed into cache 'SQL_DATA' heap.........";
                dgCacheData.DataSource = lstData;
            }
            catch (Exception ex)
            {
                txtMsg.Text = ex.ToString();
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                List<Person> lstData = new List<Person>();
                var cfg = new IgniteConfiguration
                {
                    BinaryConfiguration = new BinaryConfiguration(typeof(Person), typeof(PersonFilter)),
                };
                Ignition.ClientMode = true;
                using (var ignite = Ignition.StartFromApplicationConfiguration("igniteConfiguration"))
                {
                    ICache<int, Person> personCache = ignite.GetOrCreateCache<int, Person>(new CacheConfiguration("SQL_DATA", typeof(Person), typeof(PersonFilter)));

                    //all record - working
                    List<ICacheEntry<int, Person>> lst1 = personCache.ToList<ICacheEntry<int, Person>>();
                    foreach (ICacheEntry<int, Person> p in lst1)
                    {
                        lstData.Add(new Person() { Age = p.Value.Age, Designation = p.Value.Designation, Id = p.Value.Id, Name = p.Value.Name, Salary = p.Value.Salary });
                    }

                    dgCacheData.DataSource = lstData;
                    lstData.Clear();

                    ///SQL query
                    var sqlQuery = new SqlQuery(typeof(Person), "where age >= ?", txtAge.Text);
                    IQueryCursor<ICacheEntry<int, Person>> queryCursor = personCache.Query(sqlQuery);

                    int j = 0;
                    foreach (ICacheEntry<int, Person> p in queryCursor)
                    {
                        j++;
                        lstData.Add(new Person() { Age = p.Value.Age, Designation = p.Value.Designation, Id = p.Value.Id, Name = p.Value.Name, Salary = p.Value.Salary });
                    }
                    txtMsg.Text = j.ToString() + " records found using sql query";

                    //LINQ - normal .. help URL :  https://apacheignite-sql.readme.io/docs/linq
                    IQueryable<ICacheEntry<int, Person>> persons = personCache.AsCacheQueryable();
                    IQueryable<ICacheEntry<int, Person>> qry = persons.Where(d => d.Value.Age > Convert.ToInt32(txtAge.Text));
                    List<ICacheEntry<int, Person>> lst2 = qry.ToList<ICacheEntry<int, Person>>();
                    var pers = qry.ToArray();

                    //LINQ - compiled
                    var compiledQuery = CompiledQuery.Compile((int age) => persons.Where(emp => emp.Value.Age > age));
                    IQueryCursor<ICacheEntry<int, Person>> cursor = compiledQuery(Convert.ToInt32(txtAge.Text));
                    var result = cursor.ToArray();

                    txtMsg.Text = txtMsg.Text + '\n' + "  and " + lst2.Count + " found using LINQ";
                }
                dgFilter.DataSource = lstData;
            }
            catch (Exception ex)
            {
                txtMsg.Text = ex.ToString();
            }
        }
        #endregion  
    }

    
}
