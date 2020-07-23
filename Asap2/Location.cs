﻿using QUT.Gppg;

namespace Asap2
{
    /// <summary>
    ///     Class for handling location information.
    /// </summary>
    public class Location : IMerge<Location>
    {
        private readonly int endColumn; // end column
        private readonly int endLine; // end line
        private readonly string fileName; // current filename.
        private readonly int startColumn; // start column
        private readonly int startLine; // start line

        /// <summary>
        ///     Default no-arg constructor.
        /// </summary>
        public Location() : this(0, 0, 0, 0, "")
        {
        }

        /// <summary>
        ///     Constructor with only filename parameter.
        /// </summary>
        /// <param name="fn">file name</param>
        public Location(string fn) : this(0, 0, 0, 0, fn)
        {
        }

        /// <summary>
        ///     Constructor for text-span with given start and end.
        /// </summary>
        /// <param name="sl">start line</param>
        /// <param name="sc">start column</param>
        /// <param name="el">end line </param>
        /// <param name="ec">end column</param>
        /// <param name="fn">file name</param>
        public Location(int sl, int sc, int el, int ec, string fn)
        {
            startLine = sl;
            startColumn = sc;
            endLine = el;
            endColumn = ec;
            fileName = fn;
        }

        /// <summary>
        ///     The line at which the text span starts.
        /// </summary>
        public int StartLine
        {
            get { return startLine; }
        }

        /// <summary>
        ///     The column at which the text span starts.
        /// </summary>
        public int StartColumn
        {
            get { return startColumn; }
        }

        /// <summary>
        ///     The line on which the text span ends.
        /// </summary>
        public int EndLine
        {
            get { return endLine; }
        }

        /// <summary>
        ///     The column of the first character
        ///     beyond the end of the text span.
        /// </summary>
        public int EndColumn
        {
            get { return endColumn; }
        }

        /// <summary>
        ///     The column at which the text span starts.
        /// </summary>
        public string FileName
        {
            get { return fileName; }
        }

        /// <summary>
        ///     Create a text location which spans from the
        ///     start of "this" to the end of the argument "last"
        /// </summary>
        /// <param name="last">The last location in the result span</param>
        /// <returns>The merged span</returns>
        public Location Merge(Location last)
        {
            return new Location(startLine, startColumn, last.endLine, last.endColumn, fileName);
        }
    }
}