using System;
using System.Collections.Generic;
using System.IO;

namespace System.IO
{
    /// <summary>
    /// Streams class for Extension Methods
    /// </summary>
    public static class StreamsHelpers
    {
        /// <summary>
        /// Read all text in stream
        /// </summary>
        /// <param name="value">Stream to read</param>
        /// <param name="closeStream">close stream at end</param>
        /// <returns>All text in stream</returns>
        public static String ReadToEnd(this StreamReader value, bool closeStream)
        {
            String ret = value.ReadToEnd();
            
            if(closeStream)
                value.Close();

            return ret;
        }

        /// <summary>
        /// Read all text lines in stream
        /// </summary>
        /// <param name="value">Stream to read</param>
        /// <param name="closeStream">close stream at end</param>
        /// <returns>All text lines in stream</returns>
        public static IList<String> ReadLines(this StreamReader value, bool closeStream)
        {

            IList<String> ret = new List<String>();

            String line = value.ReadLine();
            while (line != null)
            {
                ret.Add(line);
                line = value.ReadLine();
            }
            
            if(closeStream)
                value.Close();

            return ret;
        }

    }
}