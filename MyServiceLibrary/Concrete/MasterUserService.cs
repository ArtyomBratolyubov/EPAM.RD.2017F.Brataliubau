using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using NLog;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Interfaces;

namespace ServiceLibrary.Concrete
{
    /// <summary>
    ///     Represents service, that stores data in memory
    /// </summary>
    [Serializable]
    public class MasterUserService : Service, IXmlSerializable
    {
        #region private fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private IIdAlgorithm idAlgorithm;

        private string savePath;

        private List<IPEndPoint> slaves;

        private List<TcpClient> clients;

        private bool logging;

        #endregion

        #region constructors
        /// <summary>
        /// Constructor whith one parameter
        /// </summary>
        public MasterUserService()
        {
            this.Data = new List<User>();
        }

        /// <summary>
        /// Constructor whith four parameters
        /// </summary>
        /// <param name="savePath">Path to save data</param>
        /// <param name="slaves">Slaves' addresses</param>
        /// <param name="idAlgorithm">Alghorithm to generate user ids</param>
        /// <param name="logging">Determines if logging is on</param>
        public MasterUserService(string savePath, List<IPEndPoint> slaves, IIdAlgorithm idAlgorithm, bool logging)
        {
            if (idAlgorithm == null)
            {
                this.idAlgorithm = new IncrementAlgorithm();
            }
            else
            {
                this.idAlgorithm = idAlgorithm as IncrementAlgorithm;
            }

            if (savePath == null)
            {
                throw new NullReferenceException(nameof(savePath));
            }

            if (slaves == null)
            {
                throw new NullReferenceException(nameof(slaves));
            }

            this.logging = logging;

            this.Data = new List<User>();

            this.savePath = savePath;
            this.slaves = slaves;

            this.clients = new List<TcpClient>();

            this.Load();
            this.ConfigureSlaves();

            if (this.logging)
            {
                Log.Trace("Master app started");
            }
        }

        #endregion

        #region public methods

        /// <summary>
        ///     Add new user to list
        /// </summary>
        /// <param name="user">User object to add</param>
        public override void Add(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.Id = this.idAlgorithm.NextId();

            this.Data.Add(user);

            lock (this.clients)
            {
                foreach (var client in this.clients)
                {
                    this.WriteAction(client, Actions.Add);

                    Serialize(client.GetStream(), new[] { user });
                }
            }

            if (this.logging)
            {
                Log.Trace("User id=" + user.Id + " added");
            }
        }

        /// <summary>
        ///     Removes user from list
        /// </summary>
        /// <param name="id">User's id</param>
        /// <returns>True, if user successfully removed, otherwise false</returns>
        public override void Remove(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (!this.Data.Remove(user))
            {
                throw new UserNotFoundException();
            }

            lock (this.clients)
            {
                foreach (var client in this.clients)
                {
                    this.WriteAction(client, Actions.Delete);

                    Serialize(client.GetStream(), user);
                }
            }

            if (this.logging)
            {
                Log.Trace("User id=" + user.Id + " removed");
            }
        }

        /// <summary>
        ///     Add new multiple users to list
        /// </summary>
        /// <param name="users">Users objects to add</param>
        public override void Add(params User[] users)
        {
            if (users == null)
            {
                throw new ArgumentNullException(nameof(users));
            }

            foreach (var user in users)
            {
                user.Id = this.idAlgorithm.NextId();

                if (this.logging)
                {
                    Log.Trace("User id=" + user.Id + " added");
                }
            }

            this.Data.AddRange(users);
            lock (this.clients)
            {
                foreach (var client in this.clients)
                {
                    this.WriteAction(client, Actions.Add);

                    Serialize(client.GetStream(), users);
                }
            }
        }

        /// <summary>
        /// Save current data
        /// </summary>
        public override void Save()
        {
            var formatter = new XmlSerializer(typeof(MasterUserService));

            using (var fs = new FileStream(this.savePath, FileMode.Create))
            {
                formatter.Serialize(fs, this);
            }

            if (this.logging)
            {
                Log.Trace("Master saved");
            }
        }

        /// <summary>
        /// Writes data to the xml file
        /// </summary>
        /// <param name="writer">Instance of the XmlWriter</param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("master");

            writer.WriteElementString("idAlgorithm", this.idAlgorithm.GetType().ToString());

            writer.WriteElementString("id", this.idAlgorithm.GetCurrentId().ToString());

            writer.WriteStartElement("data");
            writer.WriteAttributeString("count", this.Data.Count.ToString());

            var otherSer = new XmlSerializer(typeof(User));

            foreach (var other in this.Data)
            {
                otherSer.Serialize(writer, other);
            }

            writer.WriteEndElement();

            writer.WriteEndElement();
        }

        /// <summary>
        /// Reads data from the xml file
        /// </summary>
        /// <param name="reader">Instance of the XmlReader</param>
        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement(nameof(MasterUserService));

            reader.ReadStartElement("master");

            reader.ReadStartElement("idAlgorithm");
            var idAlgorithm = reader.ReadContentAsString();

            var type = Type.GetType(idAlgorithm);
            var ctor = type.GetConstructor(new[] { typeof(int) });

            reader.ReadEndElement();

            reader.ReadStartElement("id");
            var id = reader.ReadContentAsString();

            this.idAlgorithm = ctor.Invoke(new object[] { int.Parse(id) }) as IIdAlgorithm;

            reader.ReadEndElement();
            reader.MoveToAttribute("count");
            var count = int.Parse(reader.Value);
            reader.ReadStartElement("data");

            var otherSer = new XmlSerializer(typeof(User));
            for (var i = 0; i < count; i++)
            {
                var other = (User)otherSer.Deserialize(reader);
                this.Data.Add(other);
            }

            reader.ReadEndElement();
            reader.ReadEndElement();
        }

        /// <summary>
        /// Retunr xml schema
        /// </summary>
        /// <returns>XmlSchema object</returns>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        ///     Searches for user by specified predicate
        /// </summary>
        /// <param name="param">Search predicate</param>
        /// <returns>User object if user was found, otherwise null</returns>
        public override User SearchById(int id)
        {
            return this.SearchHelper(user => user.Id == id);
        }

        #endregion

        #region private methods

        private static void Serialize(NetworkStream stream, object data)
        {
            new BinaryFormatter().Serialize(stream, data);
        }

        private void Load()
        {
            var formatter = new XmlSerializer(typeof(MasterUserService));

            try
            {
                using (var fs = new FileStream(this.savePath, FileMode.Open))
                {
                    var ob = formatter.Deserialize(fs) as MasterUserService;

                    this.Data = ob.Data;

                    this.idAlgorithm = ob.idAlgorithm;
                }
            }
            catch (FileNotFoundException ex)
            {
            }

            if (this.logging)
            {
                Log.Trace("Master loaded");
            }
        }

        private void ConfigureSlaves()
        {
            foreach (var slave in this.slaves)
            {
                var task = new Task(() => this.ConfigureSlaveThread(slave));

                task.Start();
            }
        }

        private void ConfigureSlaveThread(IPEndPoint slave)
        {
            TcpClient client;

            while (true)
            {
                while (true)
                {
                    try
                    {
                        client = new TcpClient(slave.Address.ToString(), slave.Port);
                    }
                    catch (SocketException ex)
                    {
                        continue;
                    }

                    break;
                }

                lock (this.clients)
                {
                    this.clients.Add(client);
                }

                Serialize(client.GetStream(), this.Data.ToArray());

                while (true)
                {
                    if (client.Client.Poll(0, SelectMode.SelectRead))
                    {
                        lock (this.clients)
                        {
                            this.clients.Remove(client);
                            break;
                        }
                    }
                }
            }
        }

        private void WriteAction(TcpClient client, Actions action)
        {
            byte[] buf = { (byte)action };

            client.GetStream().Write(buf, 0, 1);
        }

        #endregion
    }
}