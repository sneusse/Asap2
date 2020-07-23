﻿using System;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Asap2
{
    /// <summary>
    ///     Base class for the Asap2Tree.
    /// </summary>
    public abstract class Asap2Base
    {
        private static ulong orderId;

        // Static constructor to initialize the static member, orderId. This
        // constructor is called one time, automatically, before any instance
        // of Asap2Base is created, or currentID is referenced.
        static Asap2Base()
        {
            orderId = 0;
        }

        public Asap2Base(Location location)
        {
            this.location = location;
            OrderID = GetOrderID();
        }

        public ulong OrderID { get; protected set; }

        public Location location { get; set; }

        protected ulong GetOrderID()
        {
            // currentID is a static field. It is incremented each time a new
            // instance of Asap2Base is created.
            return ++orderId;
        }

        public void reportErrorOrWarning(string message, bool isError, IErrorReporter errorReporter,
            bool isInfo = false)
        {
            if (isError)
            {
                var msg = string.Format("{0} : Line: {1} : Row: {2} : ValidationError : {3}", location.FileName,
                    location.StartLine, location.StartColumn, message);
                errorReporter.reportError(msg);
                throw new ValidationErrorException(msg);
            }

            if (isInfo)
                errorReporter.reportInformation(string.Format(
                    "{0} : Line: {1} : Row: {2} : ValidationInformation : {3}", location.FileName, location.StartLine,
                    location.StartColumn, message));
            else
                errorReporter.reportWarning(string.Format("{0} : Line: {1} : Row: {2} : ValidationWarning : {3}",
                    location.FileName, location.StartLine, location.StartColumn, message));
        }

        #region ValidateIdentifier

        private static readonly string IdentifierPattern = @"[A-Za-z_][A-Za-z0-9_]*(\[[A-Za-z0-9_]*\])*";

        private static readonly Regex IdentifierRgx =
            new Regex(IdentifierPattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        ///     Validates the provided identifier according to the rules.
        /// </summary>
        /// <param name="Identifier">Identifier to validate.</param>
        /// <param name="errorReporter">Error reporter to use.</param>
        protected void ValidateIdentifier(string Identifier, IErrorReporter errorReporter)
        {
            var splitted = Identifier.Split('.');

            if (splitted.Length < 1)
            {
                var tmp = string.Format("Identifier '{0}' is not a valid identifier", Identifier);
                errorReporter.reportError(tmp);
                throw new ValidationErrorException(tmp);
            }

            if (splitted.Length == 1 && splitted[0].Length > 128)
            {
                var tmp = string.Format(
                    "Identifier '{0}' is not a valid identifier, is longer than 128  (MAX_PARTIAL_IDENT).", Identifier);
                errorReporter.reportWarning(tmp);
            }
            else if (splitted.Length > 1 && Identifier.Length > 1024)
            {
                var tmp = string.Format("Identifier '{0}' is not a valid identifier, is longer than 1024 (MAX_IDENT).",
                    Identifier);
                errorReporter.reportWarning(tmp);
            }

            foreach (var line in splitted)
            {
                if (line.Length > 128)
                {
                    var tmp = string.Format(
                        "Part '{0}' of Identifier '{1}' is not a valid identifier, the part is longer than 128 (MAX_PARTIAL_IDENT)",
                        line, Identifier);
                    errorReporter.reportWarning(tmp);
                }

                var matches = IdentifierRgx.Matches(line);

                if (matches.Count == 0)
                {
                    var tmp = string.Format("Part '{0}' of Identifier '{1}' is not a valid identifier", line,
                        Identifier);
                    errorReporter.reportError(tmp);
                    throw new ValidationErrorException(tmp);
                }
            }
        }

        #endregion
    }

    /// <summary>
    ///     Data types defined by the standard.
    /// </summary>
    public enum DataType
    {
        UBYTE,
        SBYTE,
        UWORD,
        SWORD,
        ULONG,
        SLONG,
        A_UINT64,
        A_INT64,
        FLOAT32_IEEE,
        FLOAT64_IEEE
    }

    /// <summary>
    ///     Data sizes defined by the standard.
    /// </summary>
    public enum DataSize
    {
        BYTE,
        WORD,
        LONG
    }

    public enum AddrType
    {
        PBYTE,
        PWORD,
        PLONG,
        DIRECT
    }

    public enum IndexOrder
    {
        /// <summary>
        ///     Increasing index with increasing address.
        /// </summary>
        INDEX_INCR,

        /// <summary>
        ///     Decreasing index with increasing address.
        /// </summary>
        INDEX_DECR
    }

    /// <summary>
    ///     Conversion types used by CHARACTERISTICs and MEASUREMENT.
    /// </summary>
    public enum ConversionType
    {
        IDENTICAL,
        FORM,
        LINEAR,
        RAT_FUNC,
        TAB_INTP,
        TAB_NOINTP,
        TAB_VERB
    }

    [Serializable]
    public class ValidationErrorException : Exception
    {
        public ValidationErrorException()
        {
        }

        public ValidationErrorException(string message) : base(message)
        {
        }

        public ValidationErrorException(string message, Exception inner) : base(message, inner)
        {
        }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client. 
        protected ValidationErrorException(SerializationInfo info,
            StreamingContext context)
        {
        }

        public override string ToString()
        {
            return base.Message;
        }
    }

    internal interface IValidator
    {
        /// <summary>
        ///     Validates the class contents according to the requirement rules.
        /// </summary>
        /// <exception cref="ValidationErrorException">Fatal validation error.</exception>
        void Validate(IErrorReporter errorReporter);
    }

    public interface IAxisPtsCharacteristicMeasurement
    {
        string GetName();
        ulong GetEcuAddress();
        void SetEcuAddress(ulong address);
        ulong orderID();
    }
}