using System;
using ServiceLibrary.Interfaces;

namespace ServiceLibrary
{
    /// <summary>
    ///     Represents algorithm of generating ids by incrementing
    /// </summary>
    [Serializable]
    public class IncrementAlgorithm : IIdAlgorithm
    {
        #region private fields

        private int startId;

        private int currentId;

        #endregion

        #region constructors

        public IncrementAlgorithm()
        {
        }

        /// <summary>
        ///     Constructor whith one parameter
        /// </summary>
        /// <param name="startId">Number to start generating</param>
        public IncrementAlgorithm(int startId = 0)
        {
            this.startId = startId;

            this.currentId = this.startId;
        }

        #endregion

        #region public methods

        /// <summary>
        ///     Generates new id
        /// </summary>
        /// <returns>New id</returns>
        public int NextId()
        {
            return this.currentId++;
        }

        /// <summary>
        ///     Checks if id is valid
        /// </summary>
        /// <param name="id">Id to check</param>
        /// <returns>True if id is valid, otherwise false</returns>
        public bool CheckId(int id)
        {
            return !(id < this.startId || id > this.currentId - 1);
        }

        public int GetCurrentId()
        {
            return this.currentId;
        }

        public void SetStartId(int id)
        {
            this.startId = id;

            this.currentId = this.startId;
        }

        #endregion
    }
}