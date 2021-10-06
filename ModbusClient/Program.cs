using System;

namespace ModbusClient
{
    internal class Program
    {       
        static void Main(string[] args)
        {
            var mb = new ModbusClient("127.0.0.1", 502);
            mb.Connect();
            try
            {
                bool showMenu = true;
                while (showMenu)
                {
                    showMenu = MainMenu(mb);
                }
            }
            finally
            {
                mb.Disconnect();
            }
            //var mb = new ModbusClient("127.0.0.1", 502);
            //mb.Connect();
            //Console.WriteLine("Starting Address:");            
            //var address = Console.ReadKey();
            //Console.WriteLine();
            //Console.WriteLine("Number of Values:" );            
            //var values = Console.ReadKey();
            //Console.WriteLine();
            //var result = mb.ReadCoils(int.Parse(address.KeyChar.ToString()), int.Parse(values.KeyChar.ToString()));
            //Console.WriteLine("Result:");
            //Console.WriteLine();
            //foreach (var r in result)
            //{
            //    Console.WriteLine(r);
            //}
            //Console.ReadKey();
        }


        private static bool MainMenu(ModbusClient _mb)
        {
            Console.Clear();
            Console.WriteLine("Choose an option:");
            Console.WriteLine("1) ReadCoil");
            Console.WriteLine("2) WriteCoil");
            Console.WriteLine("3) ReadRegister");
            Console.WriteLine("4) WriteRegister");
            Console.WriteLine("5) Exit");
            Console.Write("\r\nSelect an option: ");

            switch (Console.ReadLine())
            {
                case "1":
                    Console.WriteLine("\r\nStarting Address:");
                    var address1 = Console.ReadKey();             
                    Console.WriteLine("\r\nNumber of Values:");
                    var values1 = Console.ReadKey();
                    Console.WriteLine();
                    var result = _mb.ReadCoils(int.Parse(address1.KeyChar.ToString())-1, int.Parse(values1.KeyChar.ToString()));
                    Console.WriteLine("\r\nResult:");              
                    foreach (var r in result)
                    {
                        Console.WriteLine(r);
                    }
                    Console.WriteLine("\r\nPress any key...");
                    Console.ReadKey();
                    return true;

                case "2":
                    Console.WriteLine("\r\nStarting Address:");
                    var address2 = Console.ReadKey();
                    Console.WriteLine("\r\nValue:");
                    var values2 = Console.ReadKey();
                    if (!values2.KeyChar.ToString().Equals("0") && !values2.KeyChar.ToString().Equals("1"))
                    {
                        Console.WriteLine("\r\nNot bool value");
                        return true;
                    }
                    Console.WriteLine();
                    _mb.WriteCoil(int.Parse(address2.KeyChar.ToString()) - 1, values2.KeyChar.ToString().Equals("0") ? false : true );                                    
                    Console.WriteLine("\r\nPress any key...");
                    Console.ReadKey();
                    return true;

                case "3":
                    Console.WriteLine("\r\nStarting Address:");
                    var address3 = Console.ReadKey();
                    Console.WriteLine("\r\nNumber of Values:");
                    var values3 = Console.ReadKey();
                    Console.WriteLine();
                    var result3 = _mb.ReadHoldingRegisters(int.Parse(address3.KeyChar.ToString()) - 1, int.Parse(values3.KeyChar.ToString()));
                    Console.WriteLine("\r\nResult:");
                    foreach (var r in result3)
                    {
                        Console.WriteLine(r);
                    }
                    Console.WriteLine("\r\nPress any key...");
                    Console.ReadKey();
                    return true;

                case "4":
                    Console.WriteLine("\r\nStarting Address:");
                    var address4 = Console.ReadKey();
                    Console.WriteLine("\r\nValue:");
                    var values4 = Console.ReadKey();                   
                    Console.WriteLine();
                    _mb.WriteSingleRegister(int.Parse(address4.KeyChar.ToString()) - 1, int.Parse(values4.KeyChar.ToString()));
                    Console.WriteLine("\r\nPress any key...");
                    Console.ReadKey();
                    return true;

                case "5":
                    return false;

                default:
                    return true;
            }
        }
    }
}