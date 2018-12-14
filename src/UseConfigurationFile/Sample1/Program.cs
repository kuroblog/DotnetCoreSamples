using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Sample1
{
    class Program
    {
        private static void exe(Action action)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception error)
            {
                var errJson = JsonConvert.SerializeObject(error, Newtonsoft.Json.Formatting.Indented);
                Console.WriteLine(errJson);
            }
        }

        static void Main(string[] args) => exe(() =>
        {
            Console.WriteLine("Hello World!");

            // project path
            Console.WriteLine(Directory.GetCurrentDirectory());
            // bin path
            Console.WriteLine(AppDomain.CurrentDomain.BaseDirectory);

            // method 1
            //var builder = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddJsonFile("sample1.json");
            // or
            // method 2
            var builder =
                new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile(cfg =>
                {
                    cfg.Path = "sample1.json";
                    cfg.ReloadOnChange = true;
                    cfg.Optional = false;
                });

            var config = builder.Build();

            Console.WriteLine(config["Version"]);
            Console.WriteLine(config["Host:Key1"]);
            Console.WriteLine(config["Host:Key2:Key21"]);
            Console.WriteLine(config["Host:Key2:Key2s"]);

            // print all
            foreach (var kv in config.AsEnumerable())
            {
                Console.WriteLine($"{kv.Key} - {kv.Value}");
            }

            // use binder
            var k2 = new Key2Model();
            config.Bind("Host:Key2", k2);
            Console.WriteLine(JsonConvert.SerializeObject(k2));
        });
    }

    public class Key2Model
    {
        public string Key21 { get; set; }

        public int[] Key2s { get; set; }
    }
}
