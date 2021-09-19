using System;
using System.IO;
using System.Reflection;
using System.Text;
using Serilog;
using Serilog.Core;

namespace WISRAT.CLI
{
    /// <summary>
    /// Patch client assembly
    /// </summary>
    public class PatchClient
    {
        private static void ProcessAssembly(Logger logger, string file)
        {
            Assembly assembly = null;
            try { assembly = Assembly.LoadFrom(file); }
            catch {
                logger.Error("Invalid assembly!");
                Environment.Exit(1);
            }

            if (assembly.GetName().FullName.Split(',')[0] != "WISRAT.Client")
            {
                logger.Error($"Assembly is not WISRAT.Client!");
                Environment.Exit(1);
            }

            string version = assembly.GetName().Version?.ToString();
            if (version != Program.Information.RequiredClient)
            {
                logger.Error($"WISRat CLI requires Client {Program.Information.RequiredClient}!");
                Environment.Exit(1);
            }
        }
        
        /// <summary>
        /// Patch file using Fetcher
        /// </summary>
        /// <param name="logger">Serilog Logger</param>
        /// <param name="file">Filename</param>
        /// <param name="user">Username</param>
        /// <param name="password">Password</param>
        /// <param name="url">URL to fetch</param>
        public static void Patch(Logger logger, string file, string user, string password, string url)
        {
            ProcessAssembly(logger, file);
            if (File.ReadAllText(file).IndexOf("%$_WISRAT_CONFIG_START_$%", StringComparison.Ordinal) != -1)
            {
                logger.Error($"Already patched!");
                Environment.Exit(1);
            }
            var builder = new StringBuilder();
            builder.Append("%$_WISRAT_CONFIG_START_$%");
            builder.Append(user); builder.Append('|');
            builder.Append(password); builder.Append('|');
            builder.Append("fetcher|");
            builder.Append(url);
            builder.Append("%$_WISRAT_CONFIG_END_$%");
            StreamWriter stream = File.AppendText(file);
            stream.Write(builder.ToString());
            stream.Flush();
            stream.Dispose();
        }
        
        /// <summary>
        /// Patch file using Direct
        /// </summary>
        /// /// <param name="logger">Serilog Logger</param>
        /// <param name="file">Filename</param>
        /// <param name="user">Username</param>
        /// <param name="password">Password</param>
        /// <param name="ip">IP</param>
        /// <param name="port">Port</param>
        public static void Patch(Logger logger, string file, string user, string password, string ip, int port)
        {
            ProcessAssembly(logger, file);
            if (File.ReadAllText(file).IndexOf("%$_WISRAT_CONFIG_START_$%", StringComparison.Ordinal) != -1)
            {
                logger.Error($"Already patched!");
                Environment.Exit(1);
            }
            var builder = new StringBuilder();
            builder.Append("%$_WISRAT_CONFIG_START_$%");
            builder.Append(user); builder.Append('|');
            builder.Append(password); builder.Append('|');
            builder.Append("direct|");
            builder.Append(ip); builder.Append('|');
            builder.Append(port);
            builder.Append("%$_WISRAT_CONFIG_END_$%");
            StreamWriter stream = File.AppendText(file);
            stream.Write(builder.ToString());
            stream.Flush();
            stream.Dispose();
        }
    }
}