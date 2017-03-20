using System;
using System.Collections;
using System.Net.Sockets;
using System.Runtime.Serialization;
using ServiceLibrary;
using ServiceLibrary.Concrete;
using ServiceLibrary.Interfaces;

namespace ServiceApplication
{

    public class Program
    {
        public static void Main(string[] args)
        {
            int a = -1;
            IUserService service = new UserServiceBuilder().UserService;

            while (true)
            {
                try
                {
                    Console.WriteLine("1. Add");
                    Console.WriteLine("2. Remove");
                    Console.WriteLine("3. Search");
                    Console.WriteLine("4. Show all");
                    Console.WriteLine("5. Save");
                    Console.WriteLine("0. Exit");

                    a = int.Parse(Console.ReadLine());

                    Console.Clear();

                    if (a == 1)
                    {
                        User u = new User();
                        Console.Write("Name: ");
                        u.FirstName = Console.ReadLine();

                        Console.Write("Surname: ");
                        u.LastName = Console.ReadLine();

                        Console.Write("Age: ");
                        u.Age = int.Parse(Console.ReadLine());

                        service.Add(u);
                    }
                    if (a == 2)
                    {
                        Console.WriteLine("ID: ");
                        int id = int.Parse(Console.ReadLine());

                        service.Remove(service.SearchById(id));
                    }
                    if (a == 3)
                    {
                        Console.WriteLine("ID: ");
                        int id = int.Parse(Console.ReadLine());
                        User user = service.SearchById(id);

                        Console.WriteLine("ID: " + user.Id);
                        Console.WriteLine("Name: " + user.FirstName);
                        Console.WriteLine("Surname: " + user.LastName);
                        Console.WriteLine("Age: " + user.Age);
                        Console.WriteLine();
                    }
                    if (a == 4)
                    {
                        foreach (var user in service.GetUsers())
                        {
                            Console.WriteLine("ID: " + user.Id);
                            Console.WriteLine("Name: " + user.FirstName);
                            Console.WriteLine("Surname: " + user.LastName);
                            Console.WriteLine("Age: " + user.Age);
                            Console.WriteLine();
                        }
                    }
                    if (a == 5)
                    {
                        service.Save();
                        Console.WriteLine("Saved!");
                    }
                    if (a == 0)
                    {
                        break;
                    }

                    Console.ReadKey();
                    Console.Clear();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.ReadKey();
                    Console.Clear();
                }
            }


        }

    }
}
