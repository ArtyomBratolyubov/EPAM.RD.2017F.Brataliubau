using System;

namespace ServiceLibrary
{
    /// <summary>
    ///     Represents user
    /// </summary>
    [Serializable]
    public class User
    {
        #region public properties

        /// <summary>
        ///     User's Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     User's first name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        ///     User's first last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        ///     User's first date Of birth
        /// </summary>
        public int Age { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            var p = obj as User;

            if (p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return this.FirstName == p.FirstName && this.LastName == p.LastName && this.Age == p.Age && this.Id == p.Id;
        }

        public override int GetHashCode()
        {
            return this.FirstName.GetHashCode() + this.LastName.GetHashCode() - this.Age.GetHashCode() - this.Id.GetHashCode();
        }

        #endregion
    }
}