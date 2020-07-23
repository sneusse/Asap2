﻿namespace Asap2
{
    /// <summary>
    ///     Interface for error and warning reports.
    /// </summary>
    public interface IErrorReporter
    {
        void reportWarning(string message);
        void reportError(string message);
        void reportInformation(string message);
    }
}