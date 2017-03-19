using System;
using System.Collections.Generic;
using ServiceLibrary.Interfaces;

namespace ServiceLibrary.Concrete
{

    public abstract class Service : IUserService
    {
        #region protected fields

        protected List<User> Data { get; set; }

        #endregion

        #region abstract methods

        public abstract void Add(params User[] users);

        public abstract void Add(User user);

        public abstract void Remove(User user);

        public abstract void Save();

        public abstract User Search(Predicate<User> param);

        public IEnumerable<User> GetUsers()
        {
            return this.Data;
        }

        #endregion

        #region protected methods

        protected User SearchHelper(Predicate<User> param)
        {
            foreach (var i in this.Data)
            {
                if (param(i))
                {
                    return i;
                }
            }

            return null;
        }

        #endregion
    }
}