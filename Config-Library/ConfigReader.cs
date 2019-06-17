using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using MongoDB;
using MongoDB.Bson;
using MongoDB.Driver; 



namespace Config_Library
{
    public class ConfigReader
    {
        // DB 
        private MongoClient client;
        private IMongoDatabase database;
        private string connStr;
        private string applicationName;

        // keeping all configurationsin dictionary data structure so that updates are more inconsistent.
        private Dictionary<string, ArrayList> items = new Dictionary<string, ArrayList>();

        // timer for configuration updates
        private static Timer aTimer;

        public ConfigReader(string connStr, string applicationName, int retrievalFrequencyInMs)
        {
            this.connStr = connStr;
            this.applicationName = applicationName;

            SetTimer(retrievalFrequencyInMs);

        }

        public bool connect(string dbName)
        {
            try
            {
                client = new MongoClient(connStr);
                database = client.GetDatabase(dbName);
                return true;
            } catch(Exception ex)
            {
                throw ex;
            }
        }

        private void SetTimer(int interval)
        {
            aTimer = new System.Timers.Timer(interval);
            // updates data 
            aTimer.Elapsed += loader;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }


        private void loader(Object source, ElapsedEventArgs e)
        {
            loadConfigurationsAsync();
        }

        public async System.Threading.Tasks.Task loadConfigurationsAsync()
        {
            try
            {
                string filter = "{ isActive: {'$eq': 1}, applicationName: {'$eq': '" + applicationName + "'} }";
                var collection = database.GetCollection<BsonDocument>("configs");

                IFindFluent<BsonDocument, BsonDocument> list = collection.Find(filter);
                await list.ForEachAsync(document =>
                {
                    if ((string)document.GetValue("type") == "String")
                    {
                        string cValue = (string)document.GetValue("value");
                        ConfigLibrary.ConfigItem<string> item = new ConfigLibrary.ConfigItem<string>(cValue);
                        ArrayList x = new ArrayList();
                        x.Add(item);
                        items[(string)document.GetValue("name")] = x;
                    } else if ((string)document.GetValue("type") == "Int")
                    {
                        int cValue = document["value"].ToInt32();
                        ConfigLibrary.ConfigItem<int> item = new ConfigLibrary.ConfigItem<int>(cValue);
                        ArrayList x = new ArrayList();
                        x.Add(item);
                        items[(string)document.GetValue("name")] = x;
                    }
                    else if ((string)document.GetValue("type") == "Double")
                    {
                        double cValue = document["value"].ToDouble();
                        ConfigLibrary.ConfigItem<double> item = new ConfigLibrary.ConfigItem<double>(cValue);
                        ArrayList x = new ArrayList();
                        x.Add(item);
                        items[(string)document.GetValue("name")] = x;
                    }
                    else if ((string)document.GetValue("type") == "Boolean")
                    {
                        bool cValue = document["value"].ToBoolean();
                        ConfigLibrary.ConfigItem<bool> item = new ConfigLibrary.ConfigItem<bool>(cValue);
                        ArrayList x = new ArrayList();
                        x.Add(item);
                        items[(string)document.GetValue("name")] = x;
                    }

                });
                return;
            } catch(Exception ex )
            {
                return;
            }
        }

        public T GetValue<T>(string name)
        {
            ArrayList al = this.items[name];
            ConfigLibrary.ConfigItem<T> item = (ConfigLibrary.ConfigItem<T>)al[0];
            return item.GetValue();
        }
       

    }
}
