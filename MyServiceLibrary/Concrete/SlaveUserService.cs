using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using NLog;

namespace ServiceLibrary.Concrete
{
    [Serializable]
    public class SlaveUserService : Service
    {
        #region private fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private TcpListener listener;

        private TcpClient client;

        private Task task;

        private Task taskCheck;

        private IPEndPoint addr;

        private bool logging;

        #endregion

        #region constructors

        /// <summary>
        /// Constructor whith two parameters
        /// </summary>
        /// <param name="addr">Slave's address</param>
        /// <param name="logging">Determines if logging is on</param>
        public SlaveUserService(IPEndPoint addr, bool logging)
        {
            if (addr == null)
            {
                throw new NullReferenceException(nameof(addr));
            }

            this.logging = logging;

            this.addr = addr;
            Log.Trace("Slave " + addr + " started");

            this.InitSlave();
            this.taskCheck = new Task(this.CheckConnection);
            this.taskCheck.Start();
        }

        #endregion

        #region public methods

        public override void Add(params User[] users)
        {
            throw new InvalidOperationException("Application works in \"SLAVE\" mode ");
        }

        public override void Add(User user)
        {
            throw new InvalidOperationException("Application works in \"SLAVE\" mode ");
        }

        public override void Remove(User user)
        {
            throw new InvalidOperationException("Application works in \"SLAVE\" mode ");
        }

        public override void Save()
        {
            throw new InvalidOperationException("Application works in \"SLAVE\" mode ");
        }

        /// <summary>
        ///     Searches for user by specified predicate
        /// </summary>
        /// <param name="param">Search predicate</param>
        /// <returns>User object if user was found, otherwise null</returns>
        public override User SearchById(int id)
        {
            lock (this.Data)
            {
                foreach (var i in this.Data)
                {
                    if (i.Id == id)
                    {
                        return i;
                    }
                }
            }

            return null;
        }

        #endregion

        #region private methods

        private object Deserialize()
        {
            var data = new BinaryFormatter().Deserialize(this.client.GetStream());

            return data;
        }

        private Actions ReadAction()
        {
            var buf = new byte[1];
            this.client.GetStream().Read(buf, 0, 1);

            return (Actions)buf[0];
        }

        private void ManageConnection()
        {
            while (true)
            {
                var act = this.ReadAction();

                switch (act)
                {
                    case Actions.Add:
                        lock (this.Data)
                        {
                            this.Data.AddRange(this.Deserialize() as User[]);
                        }

                        break;
                    case Actions.Delete:

                        var u = this.Deserialize() as User;

                        lock (this.Data)
                        {
                            this.Data.Remove(u);
                        }

                        break;
                }
            }
        }

        private void InitSlave()
        {
            this.Data = new List<User>();

            this.listener = new TcpListener(this.addr);

            this.listener.Start();

            this.client = this.listener.AcceptTcpClient();

            this.Data.Clear();
            this.Data.AddRange(this.Deserialize() as User[]);

            this.task = new Task(this.ManageConnection);
            this.task.Start();

            Log.Trace("Slave initialized");
        }

        private void CheckConnection()
        {
            while (true)
            {
                var ipProperties = IPGlobalProperties.GetIPGlobalProperties();
                var tcpConnections = ipProperties.GetActiveTcpConnections()
                    .Where(
                        x =>
                            x.LocalEndPoint.Equals(this.client.Client.LocalEndPoint) &&
                            x.RemoteEndPoint.Equals(this.client.Client.RemoteEndPoint)).ToArray();

                if (tcpConnections != null && tcpConnections.Length > 0)
                {
                    var stateOfConnection = tcpConnections.First().State;
                    if (stateOfConnection == TcpState.Established)
                    {
                        // Connection is OK
                    }
                    else
                    {
                        this.client.GetStream().Dispose();
                        this.client.Client.Close();
                        this.listener.Server.Close();

                        this.InitSlave();
                    }
                }
                else
                {
                    this.listener.Server.Close();
                    this.client.Client.Close();

                    this.InitSlave();
                }
            }
        }

        #endregion
    }
}