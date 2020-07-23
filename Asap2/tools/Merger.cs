using System;
using System.Collections.Generic;
using System.Linq;

namespace Asap2.tools
{
    public class Merger
    {
        private readonly Options options;

        public Merger(Options options)
        {
            this.options = options;
        }

        public void MergeModules(ref MODULE destination, MODULE source)
        {
            #region OTHER_ELEMENTS

            foreach (var obj in source.elements)
                try
                {
                    if (obj.GetType() == typeof(A2ML) &&
                        !options.ElementToIgnoreWhenMerging.HasFlag(Options.ElementTypes.A2ML))
                    {
                        var list = destination.elements.FindAll(x => x.GetType() == typeof(A2ML));
                        if (list != null && list.Count > 1)
                        {
                            if (options.MergeConflict == Options.MergeConflictType.UseFromFirstModuleAndWarn)
                                Console.Error.WriteLine(
                                    "Warning: A2ML found '{0}' and in {1}. Ignoring the version from {1}.",
                                    destination.location.FileName, source.location.FileName);

                            if (options.MergeConflict == Options.MergeConflictType.AbortWithError)
                                throw new ErrorException(ErrorException.ErrorCodes.MergeError,
                                    string.Format("Error: A2ML found '{0}' and in {1}.", destination.location.FileName,
                                        source.location.FileName));
                        }
                        else
                        {
                            destination.elements.Add(obj);
                        }
                    }
                    else if (obj.GetType() == typeof(IF_DATA) &&
                             !options.ElementToIgnoreWhenMerging.HasFlag(Options.ElementTypes.IF_DATA))
                    {
                        destination.elements.Add(obj);
                    }
                    else if (obj.GetType() == typeof(MOD_COMMON) &&
                             !options.ElementToIgnoreWhenMerging.HasFlag(Options.ElementTypes.MOD_COMMON))
                    {
                        var list = destination.elements.FindAll(x => x.GetType() == typeof(MOD_COMMON));
                        if (list != null && list.Count > 1)
                        {
                            if (options.MergeConflict == Options.MergeConflictType.UseFromFirstModuleAndWarn)
                                Console.Error.WriteLine(
                                    "Warning: MOD_COMMON found '{0}' and in {1}. Ignoring the version from {1}.",
                                    destination.location.FileName, source.location.FileName);

                            if (options.MergeConflict == Options.MergeConflictType.AbortWithError)
                                throw new ErrorException(ErrorException.ErrorCodes.MergeError,
                                    string.Format("Error: MOD_COMMON found '{0}' and in {1}.",
                                        destination.location.FileName, source.location.FileName));
                        }
                        else
                        {
                            destination.elements.Add(obj);
                        }
                    }
                    else if (obj.GetType() == typeof(MOD_PAR) &&
                             !options.ElementToIgnoreWhenMerging.HasFlag(Options.ElementTypes.MOD_PAR))
                    {
                        var list = destination.elements.FindAll(x => x.GetType() == typeof(MOD_PAR));
                        if (list != null && list.Count > 1)
                        {
                            if (options.MergeConflict == Options.MergeConflictType.UseFromFirstModuleAndWarn)
                                Console.Error.WriteLine(
                                    "Warning: MOD_PAR found '{0}' and in {1}. Ignoring the version from {1}.",
                                    destination.location.FileName, source.location.FileName);

                            if (options.MergeConflict == Options.MergeConflictType.AbortWithError)
                                throw new ErrorException(ErrorException.ErrorCodes.MergeError,
                                    string.Format("Error: MOD_PAR found '{0}' and in {1}.",
                                        destination.location.FileName, source.location.FileName));
                        }
                        else
                        {
                            destination.elements.Add(obj);
                        }
                    }
                    else if (obj.GetType() == typeof(VARIANT_CODING) &&
                             !options.ElementToIgnoreWhenMerging.HasFlag(Options.ElementTypes.VARIANT_CODING))
                    {
                        var list = destination.elements.FindAll(x => x.GetType() == typeof(VARIANT_CODING));
                        if (list != null && list.Count > 1)
                        {
                            if (options.MergeConflict == Options.MergeConflictType.UseFromFirstModuleAndWarn)
                                Console.Error.WriteLine(
                                    "Warning: VARIANT_CODING found '{0}' and in {1}. Ignoring the version from {1}.",
                                    destination.location.FileName, source.location.FileName);

                            if (options.MergeConflict == Options.MergeConflictType.AbortWithError)
                                throw new ErrorException(ErrorException.ErrorCodes.MergeError,
                                    string.Format("Error: VARIANT_CODING found '{0}' and in {1}.",
                                        destination.location.FileName, source.location.FileName));
                        }
                        else
                        {
                            destination.elements.Add(obj);
                        }
                    }
                    else
                    {
                        Console.Error.WriteLine("Warning: Unhandled element type found '{0}' in {1}.", obj.GetType(),
                            source.location.FileName);
                        destination.elements.Add(obj);
                    }
                }
                catch (ValidationErrorException e)
                {
                    if (options.MergeConflict == Options.MergeConflictType.UseFromFirstModuleAndWarn)
                        Console.Error.WriteLine(e.Message);

                    if (options.MergeConflict == Options.MergeConflictType.AbortWithError)
                        throw new ErrorException(ErrorException.ErrorCodes.MergeError, e.Message);
                }

            #endregion

            #region AXIS_PTS_MEASUREMENT_CHARACTERISTIC

            foreach (var obj in source.AxisPtsCharacteristicMeasurement.Values)
                try
                {
                    if (obj.GetType() == typeof(AXIS_PTS) &&
                        !options.ElementToIgnoreWhenMerging.HasFlag(Options.ElementTypes.AXIS_PTS))
                        destination.AddElement(obj as AXIS_PTS);
                    if (obj.GetType() == typeof(MEASUREMENT) &&
                        !options.ElementToIgnoreWhenMerging.HasFlag(Options.ElementTypes.MEASUREMENT))
                        destination.AddElement(obj as MEASUREMENT);
                    if (obj.GetType() == typeof(CHARACTERISTIC) &&
                        !options.ElementToIgnoreWhenMerging.HasFlag(Options.ElementTypes.CHARACTERISTIC))
                        destination.AddElement(obj as CHARACTERISTIC);
                }
                catch (ValidationErrorException e)
                {
                    if (options.MergeConflict == Options.MergeConflictType.UseFromFirstModuleAndWarn)
                        Console.Error.WriteLine(e.Message);

                    if (options.MergeConflict == Options.MergeConflictType.AbortWithError)
                        throw new ErrorException(ErrorException.ErrorCodes.MergeError, e.Message);
                }

            #endregion

            #region COMPU_TAB_COMPU_VTAB_COMPU_VTAB_RANGE

            foreach (var obj in source.CompuTabCompuVtabCompuVtabRanges.Values)
                try
                {
                    if (obj.GetType() == typeof(COMPU_TAB) &&
                        !options.ElementToIgnoreWhenMerging.HasFlag(Options.ElementTypes.COMPU_TAB))
                        destination.AddElement(obj as COMPU_TAB);
                    if (obj.GetType() == typeof(COMPU_VTAB) &&
                        !options.ElementToIgnoreWhenMerging.HasFlag(Options.ElementTypes.COMPU_VTAB))
                        destination.AddElement(obj as COMPU_VTAB);
                    if (obj.GetType() == typeof(COMPU_VTAB_RANGE) &&
                        !options.ElementToIgnoreWhenMerging.HasFlag(Options.ElementTypes.COMPU_VTAB_RANGE))
                        destination.AddElement(obj as COMPU_VTAB_RANGE);
                }
                catch (ValidationErrorException e)
                {
                    if (options.MergeConflict == Options.MergeConflictType.UseFromFirstModuleAndWarn)
                        Console.Error.WriteLine(e.Message);

                    if (options.MergeConflict == Options.MergeConflictType.AbortWithError)
                        throw new ErrorException(ErrorException.ErrorCodes.MergeError, e.Message);
                }

            #endregion

            #region COMPU_METHOD

            if (!options.ElementToIgnoreWhenMerging.HasFlag(Options.ElementTypes.COMPU_METHOD))
                foreach (var obj in source.CompuMethods.Values)
                    try
                    {
                        destination.AddElement(obj);
                    }
                    catch (ValidationErrorException e)
                    {
                        if (options.MergeConflict == Options.MergeConflictType.UseFromFirstModuleAndWarn)
                            Console.Error.WriteLine(e.Message);

                        if (options.MergeConflict == Options.MergeConflictType.AbortWithError)
                            throw new ErrorException(ErrorException.ErrorCodes.MergeError, e.Message);
                    }

            #endregion

            #region FRAME

            if (!options.ElementToIgnoreWhenMerging.HasFlag(Options.ElementTypes.FRAME))
                foreach (var obj in source.Frames.Values)
                    try
                    {
                        destination.AddElement(obj);
                    }
                    catch (ValidationErrorException e)
                    {
                        if (options.MergeConflict == Options.MergeConflictType.UseFromFirstModuleAndWarn)
                            Console.Error.WriteLine(e.Message);

                        if (options.MergeConflict == Options.MergeConflictType.AbortWithError)
                            throw new ErrorException(ErrorException.ErrorCodes.MergeError, e.Message);
                    }

            #endregion

            #region FUNCTION

            if (!options.ElementToIgnoreWhenMerging.HasFlag(Options.ElementTypes.FUNCTION))
                foreach (var obj in source.Functions.Values)
                    try
                    {
                        destination.AddElement(obj);
                    }
                    catch (ValidationErrorException e)
                    {
                        if (options.MergeConflict == Options.MergeConflictType.UseFromFirstModuleAndWarn)
                            Console.Error.WriteLine(e.Message);

                        if (options.MergeConflict == Options.MergeConflictType.AbortWithError)
                            throw new ErrorException(ErrorException.ErrorCodes.MergeError, e.Message);
                    }

            #endregion

            #region GROUP

            if (!options.ElementToIgnoreWhenMerging.HasFlag(Options.ElementTypes.GROUP))
                foreach (var obj in source.Groups.Values)
                    try
                    {
                        destination.AddElement(obj);
                    }
                    catch (ValidationErrorException e)
                    {
                        if (options.MergeConflict == Options.MergeConflictType.UseFromFirstModuleAndWarn)
                            Console.Error.WriteLine(e.Message);

                        if (options.MergeConflict == Options.MergeConflictType.AbortWithError)
                            throw new ErrorException(ErrorException.ErrorCodes.MergeError, e.Message);
                    }

            #endregion

            #region RECORD_LAYOUT

            if (!options.ElementToIgnoreWhenMerging.HasFlag(Options.ElementTypes.RECORD_LAYOUT))
                foreach (var obj in source.Record_layouts.Values)
                    try
                    {
                        destination.AddElement(obj);
                    }
                    catch (ValidationErrorException e)
                    {
                        if (options.MergeConflict == Options.MergeConflictType.UseFromFirstModuleAndWarn)
                            Console.Error.WriteLine(e.Message);

                        if (options.MergeConflict == Options.MergeConflictType.AbortWithError)
                            throw new ErrorException(ErrorException.ErrorCodes.MergeError, e.Message);
                    }

            #endregion

            #region UNIT

            if (!options.ElementToIgnoreWhenMerging.HasFlag(Options.ElementTypes.UNIT))
                foreach (var obj in source.Units.Values)
                    try
                    {
                        destination.AddElement(obj);
                    }
                    catch (ValidationErrorException e)
                    {
                        if (options.MergeConflict == Options.MergeConflictType.UseFromFirstModuleAndWarn)
                            Console.Error.WriteLine(e.Message);

                        if (options.MergeConflict == Options.MergeConflictType.AbortWithError)
                            throw new ErrorException(ErrorException.ErrorCodes.MergeError, e.Message);
                    }

            #endregion

            #region USER_RIGHTS

            if (!options.ElementToIgnoreWhenMerging.HasFlag(Options.ElementTypes.USER_RIGHTS))
                foreach (var obj in source.User_rights.Values)
                    try
                    {
                        destination.AddElement(obj);
                    }
                    catch (ValidationErrorException e)
                    {
                        if (options.MergeConflict == Options.MergeConflictType.UseFromFirstModuleAndWarn)
                            Console.Error.WriteLine(e.Message);

                        if (options.MergeConflict == Options.MergeConflictType.AbortWithError)
                            throw new ErrorException(ErrorException.ErrorCodes.MergeError, e.Message);
                    }

            #endregion
        }

        public MODULE MergeModulesList(Dictionary<string, MODULE> modules)
        {
            MODULE baseModule = null;
            foreach (var module in modules.Values)
                if (baseModule == null)
                    baseModule = module;
                else
                    MergeModules(ref baseModule, module);
            return baseModule;
        }

        public void Merge(ref Asap2File destination, Asap2File source)
        {
            var destinationProject = destination.elements.First(x => x is PROJECT) as PROJECT;
            var sourceProject = source.elements.First(x => x is PROJECT) as PROJECT;
            if (options.ModuleMerge == Options.ModuleMergeType.Multiple)
                foreach (var module in sourceProject.modules.Values)
                    try
                    {
                        destinationProject.modules.Add(module.Name, module);
                    }
                    catch (ArgumentException)
                    {
                        if (options.MergeConflict == Options.MergeConflictType.UseFromFirstModuleAndWarn)
                            Console.Error.WriteLine("Warning: Duplicate MODULE with name '{0}' found in {1}",
                                module.Name, source.baseFilename);

                        if (options.MergeConflict == Options.MergeConflictType.AbortWithError)
                            throw new ErrorException(ErrorException.ErrorCodes.MergeError,
                                string.Format("Error: Duplicate MODULE with name '{0}' found in {1}", module.Name,
                                    source.baseFilename));
                    }

            if (options.ModuleMerge == Options.ModuleMergeType.One)
            {
                var destinationModule = destinationProject.modules.First().Value;
                var sourceModule = MergeModulesList(sourceProject.modules);

                MergeModules(ref destinationModule, sourceModule);
            }
        }
    }
}