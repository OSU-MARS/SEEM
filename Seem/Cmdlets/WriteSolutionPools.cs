using Mars.Seem.Optimization;
using Mars.Seem.Output;
using Mars.Seem.Silviculture;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;

namespace Mars.Seem.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "SolutionPools")]
    public class WriteSolutionPools : WriteSilviculturalTrajectoriesCmdlet
    {
        protected override void ProcessRecord()
        {
            this.ValidateParameters();

            using StreamWriter writer = this.CreateCsvWriter();

            int maxPoolCapacity = 0;
            for (int trajectoryIndex = 0; trajectoryIndex < this.Trajectories.Count; ++trajectoryIndex)
            {
                SilviculturalSpace silviculturalSpace = this.Trajectories[trajectoryIndex];
                if (silviculturalSpace.CoordinatesEvaluated.Count > 0)
                {
                    SilviculturalCoordinate coordinate = silviculturalSpace.CoordinatesEvaluated[0];
                    maxPoolCapacity = Int32.Max(maxPoolCapacity, silviculturalSpace[coordinate].Pool.PoolCapacity);
                }
            }

            // header
            if (this.ShouldWriteCsvHeader())
            {
                string?[] financialHeader = new string?[maxPoolCapacity];
                string?[] distanceHeader = new string?[maxPoolCapacity];
                for (int solutionIndex = 0; solutionIndex < maxPoolCapacity; ++solutionIndex)
                {
                    financialHeader[solutionIndex] = "financial" + solutionIndex.ToString(CultureInfo.InvariantCulture);
                    distanceHeader[solutionIndex] = "distance" + solutionIndex.ToString(CultureInfo.InvariantCulture);
                }

                writer.WriteLine(this.GetCsvHeaderForSilviculturalCoordinate() + ",pooled,accepted,rejected," + 
                                 String.Join(',', financialHeader) + "," +
                                 String.Join(',', distanceHeader));
            }

            // pool contents
            long estimatedBytesSinceLastFileLength = 0;
            long knownFileSizeInBytes = 0;
            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            WriteSilviculturalCoordinateContext writeContext = new(this.HeuristicParameters);
            for (int trajectoryIndex = 0; trajectoryIndex < this.Trajectories.Count; ++trajectoryIndex)
            {
                SilviculturalSpace silviculturalSpace = this.Trajectories[trajectoryIndex];
                writeContext.SetSilviculturalSpace(silviculturalSpace);

                int poolCapacity = 0;
                if (silviculturalSpace.CoordinatesEvaluated.Count > 0)
                {
                    SilviculturalCoordinate coordinate = silviculturalSpace.CoordinatesEvaluated[0];
                    poolCapacity = silviculturalSpace[coordinate].Pool.PoolCapacity;
                }
                if (poolCapacity != maxPoolCapacity)
                {
                    throw new NotSupportedException("Pool capacity " + poolCapacity + " for trajectory " + trajectoryIndex + " does not match the maximum capacity of " + maxPoolCapacity + ". Padding to align columns to the maximum capacity hasn't yet been implemented.");
                }

                string?[] distances = new string?[poolCapacity];
                string?[] financialValues = new string?[poolCapacity];
                for (int resultIndex = 0; resultIndex < silviculturalSpace.CoordinatesEvaluated.Count; ++resultIndex)
                {
                    SilviculturalCoordinate coordinate = silviculturalSpace.CoordinatesEvaluated[resultIndex];
                    SilviculturalPrescriptionPool prescriptions = silviculturalSpace[coordinate].Pool;
                    if (prescriptions.PoolCapacity != poolCapacity)
                    {
                        throw new NotSupportedException("Solution pool capacity changed from " + poolCapacity + " to " + prescriptions.PoolCapacity + ".");
                    }

                    writeContext.SetSilviculturalCoordinate(coordinate);
                    string linePrefx = writeContext.GetCsvPrefixForSilviculturalCoordinate();
                    for (int solutionIndex = 0; solutionIndex < prescriptions.SolutionsInPool; ++solutionIndex)
                    {
                        //TreeSelectionBySpecies? eliteTreeSelection = prescriptions.EliteTreeSelections[solutionIndex];
                        //Debug.Assert(eliteTreeSelection != null);
                        float highestFinancialValue = prescriptions.EliteFinancialValues[solutionIndex];
                        int nearestNeighborDistance = SolutionPool.UnknownDistance;
                        int nearestNeighborIndex = prescriptions.NearestNeighborIndex[solutionIndex];
                        if (nearestNeighborIndex != SolutionPool.UnknownNeighbor)
                        {
                            nearestNeighborDistance = prescriptions.DistanceMatrix[nearestNeighborIndex, solutionIndex];
                            Debug.Assert((nearestNeighborIndex != solutionIndex) && (nearestNeighborDistance == prescriptions.DistanceMatrix[solutionIndex, nearestNeighborIndex]));
                        }

                        financialValues[solutionIndex] = highestFinancialValue.ToString(CultureInfo.InvariantCulture);
                        if (nearestNeighborDistance != SolutionPool.UnknownDistance)
                        {
                            distances[solutionIndex] = nearestNeighborDistance.ToString(CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            distances[solutionIndex] = null;
                        }
                    }
                    for (int solutionIndex = prescriptions.SolutionsInPool; solutionIndex < financialValues.Length; ++solutionIndex)
                    {
                        financialValues[solutionIndex] = null;
                        distances[solutionIndex] = null;
                    }

                    string line = linePrefx + "," +
                        prescriptions.SolutionsInPool.ToString(CultureInfo.InvariantCulture) + "," +
                        prescriptions.SolutionsAccepted.ToString(CultureInfo.InvariantCulture) + "," +
                        prescriptions.SolutionsRejected.ToString(CultureInfo.InvariantCulture) + "," +
                        String.Join(',', financialValues) + "," +
                        String.Join(',', distances);
                    writer.WriteLine(line);
                    estimatedBytesSinceLastFileLength += line.Length + Environment.NewLine.Length;

                    if (estimatedBytesSinceLastFileLength > WriteCmdlet.StreamLengthSynchronizationInterval)
                    {
                        // see remarks on WriteCmdlet.StreamLengthSynchronizationInterval
                        knownFileSizeInBytes = writer.BaseStream.Length;
                        estimatedBytesSinceLastFileLength = 0;
                    }
                    if (knownFileSizeInBytes + estimatedBytesSinceLastFileLength > maxFileSizeInBytes)
                    {
                        this.WriteWarning("Write-SolutionPool: File size limit of " + this.LimitGB.ToString(Constant.Default.FileSizeLimitFormat) + " GB exceeded.");
                        break;
                    }
                }
            }
        }
    }
}
