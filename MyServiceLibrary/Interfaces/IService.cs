using System;
using System.Collections.Generic;

namespace ServiceLibrary.Interfaces
{
    /// <summary>
    ///     Represents user service
    /// </summary>
    public interface IUserService
    {
        IEnumerable<User> GetUsers();

        /// <summary>
        ///     Add new user to list
        /// </summary>
        /// <param name="user">User object to add</param>
        void Add(User user);

        void Add(params User[] users);

        /// <summary>
        ///     Removes user from list
        /// </summary>
        /// <param name="id">User's id</param>
        /// <returns>True, if user successfully removed, otherwise false</returns>
        void Remove(User param);

        /// <summary>
        ///     Searches for user by specified predicate
        /// </summary>
        /// <param name="param">Search predicate</param>
        /// <returns>User object if user was found, otherwise null</returns>
        User SearchById(int id);

        void Save();
    }
}