using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Runtime.Remoting;
using System.Security.Policy;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Interfaces;

namespace ServiceLibrary.Concrete
{
    public class UserServiceBuilder : IUserServiceBuilder
    {
        private IUserService service;

        public IUserService UserService
        {
            get
            {
                if (this.service != null)
                {
                    return this.service;
                }

                var mode = ConfigurationManager.AppSettings["UserServiceMode"];

                var logging = bool.Parse(ConfigurationManager.AppSettings["Logging"]);

                object obj = null;

                AppDomain domain = AppDomain.CreateDomain("ServiceDomain");

                if (mode == "master")
                {
                    var savePath = ConfigurationManager.AppSettings["DataPath"];

                    var slaveData = ConfigurationManager.AppSettings["Slaves"];

                    var slaves = new List<IPEndPoint>();

                    var slaveAddr = slaveData.Split(';');

                    foreach (var s in slaveAddr)
                    {
                        var slave = s.Split(':');

                        slaves.Add(new IPEndPoint(IPAddress.Parse(slave[0]), int.Parse(slave[1])));
                    }

                    object[] pars =
                    {
                        savePath,
                        slaves,
                        new IncrementAlgorithm(),
                        logging
                    };

                    Type t = typeof(MasterUserService);
                    obj = domain.CreateInstanceAndUnwrap(
                        "ServiceLibrary",
                        t.FullName,
                        true,
                        BindingFlags.Default,
                        null,
                        pars,
                        CultureInfo.CurrentCulture,
                        null);

                    // obj = new MasterUserService(savePath, slaves, new IncrementAlgorithm(),logging);
                }
                else if (mode == "slave")
                {
                    object[] pars =
                    {
                        new IPEndPoint(
                            IPAddress.Parse(ConfigurationManager.AppSettings["IP"]),
                            int.Parse(ConfigurationManager.AppSettings["Port"])),
                        logging
                    };

                    obj = domain.CreateInstanceAndUnwrap(
                        "ServiceLibrary",
                        "ServiceLibrary.Concrete.SlaveUserService",
                        true,
                        BindingFlags.Default,
                        null,
                        pars,
                        CultureInfo.CurrentCulture,
                        null);

                    // obj = new SlaveUserService(new IPEndPoint(
                    //    IPAddress.Parse(ConfigurationManager.AppSettings["IP"]),
                    //    Int32.Parse(ConfigurationManager.AppSettings["Port"])),
                    //    logging);
                }
                else
                {
                    throw new WrongUserServiceModeException();
                }

                this.service = obj as IUserService;

                return obj as IUserService;
            }
        }
    }
}