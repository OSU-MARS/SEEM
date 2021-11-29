using Mars.Seem.Optimization;
using Mars.Seem.Silviculture;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;

namespace Mars.Seem.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "SolutionPools")]
    public class WriteSolutionPools : WriteTrajectoriesCmdlet
    {
        protected override void ProcessRecord()
        {
            this.ValidateParameters();
            Debug.Assert(this.Trajectories != null);

            int poolCapacity = 0;
            if (this.Trajectories.CoordinatesEvaluated.Count > 0)
            {
                StandTrajectoryCoordinate coordinate = this.Trajectories.CoordinatesEvaluated[0];
                poolCapacity = this.Trajectories[coordinate].Pool.PoolCapacity;
            }

            using StreamWriter writer = this.GetWriter();

            // header
            bool resultsSpecified = this.Trajectories != null;
            if (this.ShouldWriteHeader())
            {
                string?[] financialHeader = new string?[poolCapacity];
                string?[] distanceHeader = new string?[poolCapacity];
                for (int solutionIndex = 0; solutionIndex < poolCapacity; ++solutionIndex)
                {
                    financialHeader[solutionIndex] = "financial" + solutionIndex.ToString(CultureInfo.InvariantCulture);
                    distanceHeader[solutionIndex] = "distance" + solutionIndex.ToString(CultureInfo.InvariantCulture);
                }

                writer.WriteLine(WriteTrajectoriesCmdlet.GetHeuristicAndPositionCsvHeader(this.Trajectories!) + ",pooled,accepted,rejected," + 
                                 String.Join(',', financialHeader) + "," +
                                 String.Join(',', distanceHeader));
            }

            // pool contents
            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            string?[] financialValues = new string?[poolCapacity];
            string?[] distances = new string?[poolCapacity];
            for (int resultIndex = 0; resultIndex < this.Trajectories!.CoordinatesEvaluated.Count; ++resultIndex)
            {
                StandTrajectoryCoordinate coordinate = this.Trajectories.CoordinatesEvaluated[resultIndex];
                SilviculturalPrescriptionPool prescriptions = this.Trajectories[coordinate].Pool;
                if (prescriptions.PoolCapacity != poolCapacity)
                {
                    throw new NotSupportedException("Solution pool capacity changed from " + poolCapacity + " to " + prescriptions.PoolCapacity + ".");
                }

                string linePrefx = this.GetPositionPrefix(coordinate);
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

                writer.WriteLine(linePrefx + "," +
                                 prescriptions.SolutionsInPool.ToString(CultureInfo.InvariantCulture) + "," +
                                 prescriptions.SolutionsAccepted.ToString(CultureInfo.InvariantCulture) + "," +
                                 prescriptions.SolutionsRejected.ToString(CultureInfo.InvariantCulture) + "," +
                                 String.Join(',', financialValues) + "," + 
                                 String.Join(',', distances));

                if (writer.BaseStream.Length > maxFileSizeInBytes)
                {
                    this.WriteWarning("Write-SolutionPool: File size limit of " + this.LimitGB.ToString("0.00") + " GB exceeded.");
                    break;
                }
            }
        }
    }
}
