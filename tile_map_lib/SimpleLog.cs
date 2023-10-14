using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace tile_map_lib
{
    public class SimpleLog
    {
        #region Static Variables

        static string DateStampFormat { get; } = "yyyy-MM-dd-HH-mm-ss-ff";

        #endregion Static Variables




        #region Static Properties

        /// <summary>
        /// Print a datestamp recording the date, time and milliseconds of the logging call.
        /// </summary>
        public static bool PrintDateStamp { get; set; } = true;

        /// <summary>
        /// Print the calling source file, method and line number.
        /// </summary>
        public static bool PrintCallerInfo { get; set; } = true;

        #endregion Static Properties




        #region Static Functionality

        static string FormatLogMessage(string message, bool mute_caller_info = false, bool mute_datestamp = false, [CallerFilePath] string caller_file = "", [CallerMemberName] string caller_member = "", [CallerLineNumber] int caller_line = 0)
        {
            string formatted_message = string.Empty;
            string datestamp = PrintDateStamp && !mute_datestamp ? $"{DateTime.Now.ToString(DateStampFormat)} " : "";
            string caller_info = "";

            if (PrintCallerInfo && !mute_caller_info)
            {
                string caller_filename = string.Empty;
                if (caller_file != null && !string.IsNullOrEmpty(caller_file))
                {
                    caller_filename = Path.GetFileNameWithoutExtension(caller_file);
                }
                caller_info = $"[{caller_filename}.{caller_member}.{caller_line}] ";
            }

            string[] lines = message.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

            foreach (string line in lines)
            {
                string formatted_line = $"{datestamp}{caller_info}{line}";
                if (string.IsNullOrEmpty(formatted_message)) formatted_message = formatted_line;
                else formatted_message = $"{formatted_message}\r\n{formatted_line}";
            }

            return formatted_message;
        }

        #endregion Static Functionality




        #region Static Methods

        public static void ToConsole(string message, bool mute_caller_info = false, bool mute_datestamp = false, [CallerFilePath] string caller_file = "", [CallerMemberName] string caller_member = "", [CallerLineNumber] int caller_line = 0)
        {
            Console.WriteLine(FormatLogMessage(message, mute_caller_info, mute_datestamp, caller_file, caller_member, caller_line));
        }

        #endregion Static Methods

    }
}
