namespace ServiceLibrary
{
    /// <summary>
    ///     Exposes the algorithm, which supports genreating new id
    /// </summary>
    public interface IIdAlgorithm
    {
        #region methods

        /// <summary>
        ///     Generates new id
        /// </summary>
        /// <returns>New id</returns>
        int NextId();

        /// <summary>
        ///     Checks if id is valid
        /// </summary>
        /// <param name="id">Id to check</param>
        /// <returns>True if id is valid, otherwise false</returns>
        bool CheckId(int id);

        int GetCurrentId();

        void SetStartId(int id);

        #endregion
    }
}