﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Vts.Api.Models;
using Vts.Extensions;

namespace Vts.Api.Tools
{
    public class ParameterTools : IParameterTools
    {
        /// <summary>
        /// Returns a list of optical properties
        /// </summary>
        /// <param name="opticalProperties">Single set of Optical Properties</param>
        /// <param name="wavelengthOpticalProperties">List of Optical Properties when adding wavelength</param>
        /// <returns>A list of optical properties</returns>
        public IEnumerable<OpticalProperties> GetOpticalPropertiesObject(OpticalProperties opticalProperties, IEnumerable<OpticalProperties> wavelengthOpticalProperties)
        {
            if (wavelengthOpticalProperties != null)
            {
                return wavelengthOpticalProperties;
            }

            if (opticalProperties != null)
            {
                return new[] { opticalProperties };
            }
            return new List<OpticalProperties>();
        }

        /// <summary>
        /// This method gets the parameters in the correct order to then send to ComputeReflectance
        /// </summary>
        /// <param name="opticalProperties">list of optical properties</param>
        /// <param name="solutionDomain">the solution domain enum</param>
        /// <param name="xAxis">the x-axis object</param>
        /// <param name="independentAxis">the independent axis, if needed</param>
        /// <param name="secondIndependentAxis">the second independent axis, if needed</param>
        /// <returns></returns>
        public IDictionary<IndependentVariableAxis, object> GetParametersInOrder(
            object opticalProperties, SolutionDomainType solutionDomain, IndependentAxis xAxis, IndependentAxis independentAxis, IndependentAxis secondIndependentAxis)
        {
            // make list of independent vars with independent first then constant
            var listIndependentVariableAxes = new List<IndependentVariableAxis>();
            switch (solutionDomain)
            {
                case SolutionDomainType.ROfRho:
                    listIndependentVariableAxes.Add(IndependentVariableAxis.Rho);
                    break;
                case SolutionDomainType.ROfRhoAndTime:
                    listIndependentVariableAxes.Add(IndependentVariableAxis.Rho);
                    listIndependentVariableAxes.Add(IndependentVariableAxis.Time);
                    break;
                case SolutionDomainType.ROfRhoAndFt:
                    listIndependentVariableAxes.Add(IndependentVariableAxis.Ft);
                    listIndependentVariableAxes.Add(IndependentVariableAxis.Rho);
                    break;
                case SolutionDomainType.ROfFx:
                    listIndependentVariableAxes.Add(IndependentVariableAxis.Fx);
                    break;
                case SolutionDomainType.ROfFxAndTime:
                    listIndependentVariableAxes.Add(IndependentVariableAxis.Time);
                    listIndependentVariableAxes.Add(IndependentVariableAxis.Fx);
                    break;
                case SolutionDomainType.ROfFxAndFt:
                    listIndependentVariableAxes.Add(IndependentVariableAxis.Ft);
                    listIndependentVariableAxes.Add(IndependentVariableAxis.Fx);
                    break;
                default:
                    throw new InvalidEnumArgumentException("There is no Enum of this type");
            }

            // get all parameters in order
            var allParameters =
                from independentVariableAxis in listIndependentVariableAxes
                orderby GetParameterOrder(independentVariableAxis)
                select new KeyValuePair<IndependentVariableAxis, object>(independentVariableAxis,
                    GetParameterValues(independentVariableAxis, xAxis, independentAxis, secondIndependentAxis));
            // Optical Properties are always first in the list
            return
                new KeyValuePair<IndependentVariableAxis, object>(
                        IndependentVariableAxis.Wavelength,
                        opticalProperties)
                    .AsEnumerable()
                    .Concat(allParameters).ToDictionary();
        }

        /// <summary>
        /// Based on the axis, it returns the sort index
        /// </summary>
        /// <param name="axis">The independent variable axis</param>
        /// <returns>Returns an integer</returns>
        internal int GetParameterOrder(IndependentVariableAxis axis)
        {
            switch (axis)
            {
                case IndependentVariableAxis.Wavelength:
                    return 0;
                case IndependentVariableAxis.Rho:
                    return 1;
                case IndependentVariableAxis.Fx:
                    return 1;
                case IndependentVariableAxis.Time:
                    return 2;
                case IndependentVariableAxis.Ft:
                    return 2;
                case IndependentVariableAxis.Z:
                    return 3;
                default:
                    throw new InvalidEnumArgumentException("There is no Enum of this type");
            }
        }

        /// <summary>
        /// This method will get the correct parameter value based on whether it is fixed or a range
        /// </summary>
        /// <param name="currentAxis">The current Axis to return values</param>
        /// <param name="xAxis">The x-axis</param>
        /// <param name="independentAxis">The first independent axis</param>
        /// <param name="secondIndependentAxis">The second independent axis</param>
        /// <returns>a double array with the value(s)</returns>
        internal double[] GetParameterValues(IndependentVariableAxis currentAxis, IndependentAxis xAxis, IndependentAxis independentAxis, IndependentAxis secondIndependentAxis)
        {
            if (independentAxis != null && currentAxis == independentAxis.Axis)
            {
                return independentAxis.AxisRange == null ? new[] { independentAxis.AxisValue } : independentAxis.AxisRange.AsEnumerable().ToArray();
            }
            if (secondIndependentAxis != null && currentAxis == secondIndependentAxis.Axis)
            {
                return secondIndependentAxis.AxisRange == null ? new[] { secondIndependentAxis.AxisValue } : secondIndependentAxis.AxisRange.AsEnumerable().ToArray();
            }
            return xAxis.AxisRange.AsEnumerable().ToArray();
        }
    }
}
