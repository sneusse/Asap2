using System;
using System.Text;

namespace Asap2
{
    /// <summary>
    ///     Class for holding multi line comments. Use <see cref="Environment.NewLine" /> when adding a new line to the
    ///     comment.
    ///     Start ('/* ') and end (' */') of the comment block is added by the class.
    /// </summary>
    public class FileComment : Asap2Base
    {
        /// <summary>
        ///     Default constructor.
        /// </summary>
        /// <param name="comment">First comment line.</param>
        /// <param name="startNewLineWithStar">Indicates if each comment line shall start with a *.</param>
        public FileComment(string comment = null, bool startNewLineWithStar = false) : base(new Location())
        {
            if (comment != null)
            {
                Comment = new StringBuilder();
                Comment.Append(comment);
                StartNewLineWithStar = startNewLineWithStar;
            }
        }

        /// <summary>
        ///     Default constructor.
        /// </summary>
        /// <param name="comment">First comment line.</param>
        /// <param name="startNewLineWithStar">Indicates if each comment line shall start with a *.</param>
        public FileComment(Location location, string comment = null, bool startNewLineWithStar = false) : base(location)
        {
            if (comment != null)
            {
                Comment = new StringBuilder();
                Comment.Append(comment);
                StartNewLineWithStar = startNewLineWithStar;
            }
        }

        public StringBuilder Comment { get; set; }

        public bool StartNewLineWithStar { get; private set; }

        public void Append(object value)
        {
            Comment.Append(value);
        }

        public override string ToString()
        {
            Comment.Insert(0, "/* ");
            if (StartNewLineWithStar)
            {
                Comment.Replace(Environment.NewLine, Environment.NewLine + " * ");
                Comment[Comment.Length - 1] = '/';
            }
            else
            {
                Comment.Append(" */");
            }

            return Comment.ToString();
        }
    }
}