using System.IO;
using System.Linq;

namespace WISRAT.CLI
{
    /// <summary>
    /// Makes BanLists easier to use
    /// </summary>
    public static class BanList
    {
        /// <summary>
        /// Does banlist contains a user?
        /// </summary>
        /// <param name="file">Filename</param>
        /// <param name="name">Username</param>
        /// <returns>Answer</returns>
        public static bool Contains(string file, string name) 
            => File.ReadAllText(file).Split('|').Contains(name);
        
        /// <summary>
        /// Add user to banlist
        /// </summary>
        /// <param name="file">Filename</param>
        /// <param name="name">Username</param>
        public static void Add(string file, string name)
        {
            StreamWriter stream = File.AppendText(file);
            stream.Write($"|{name}");
            stream.Flush();
            stream.Dispose();
        }
    }
}