﻿using System;
using System.Collections.Generic;

namespace Osu.Cof.Ferm.Organon
{
    public class OrganonTreatments
    {
        // basal area before most recent thinning (Hann RC 40, equations 10 and 18)
        public float BasalAreaBeforeMostRecentHarvest { get; private set; }
        // basal area removed by thinning in ft²/ac in the years specified
        public List<float> BasalAreaRemovedByHarvest { get; private set; }

        public int FertilizationsPerformed { get; set; }

        public List<IHarvest> Harvests { get; private set; }
        public int HarvestsPerformed { get; set; }

        // N applied in lb/ac (Hann RC 40)
        public List<float> PoundsOfNitrogenPerAcre { get; private set; }

        // number of simulation time steps since fertlization at index was performed
        public List<int> TimeStepsSinceFertilization { get; private set; }
        // number of simulation time steps since thin at index was performed
        public List<int> TimeStepsSinceHarvest { get; private set; }

        public OrganonTreatments()
        {
            this.BasalAreaBeforeMostRecentHarvest = -1.0F;
            this.BasalAreaRemovedByHarvest = new List<float>();
            this.FertilizationsPerformed = 0;
            this.Harvests = new List<IHarvest>();
            this.HarvestsPerformed = 0;
            this.PoundsOfNitrogenPerAcre = new List<float>();
            this.TimeStepsSinceHarvest = new List<int>();
            this.TimeStepsSinceFertilization = new List<int>();
        }

        public OrganonTreatments(OrganonTreatments other)
        {
            this.BasalAreaBeforeMostRecentHarvest = other.BasalAreaBeforeMostRecentHarvest;
            this.BasalAreaRemovedByHarvest = new List<float>(other.BasalAreaRemovedByHarvest);
            this.FertilizationsPerformed = other.FertilizationsPerformed;
            this.Harvests = new List<IHarvest>(other.Harvests);
            this.HarvestsPerformed = other.HarvestsPerformed;
            this.PoundsOfNitrogenPerAcre = new List<float>(other.PoundsOfNitrogenPerAcre);
            this.TimeStepsSinceHarvest = new List<int>(other.TimeStepsSinceHarvest);
            this.TimeStepsSinceFertilization = new List<int>(other.TimeStepsSinceFertilization);
        }

        // TODO: move this to simulation state
        public void CompleteTimeStep()
        {
            for (int fertilizationIndex = 0; fertilizationIndex < this.FertilizationsPerformed; ++fertilizationIndex)
            {
                ++this.TimeStepsSinceFertilization[fertilizationIndex];
            }

            for (int thinIndex = 0; thinIndex < this.HarvestsPerformed; ++thinIndex)
            {
                ++this.TimeStepsSinceHarvest[thinIndex];
            }
        }
        
        public float EvaluateTriggers(int periodJustBeginning, OrganonStandTrajectory trajectory)
        {
            if (periodJustBeginning < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(periodJustBeginning));
            }

            float basalAreaRemovedInPeriod = 0.0F;
            foreach (IHarvest harvest in this.Harvests)
            {
                if (harvest.Period == periodJustBeginning)
                {
                    float basalAreaRemovedByHarvest = harvest.EvaluateTreeSelection(trajectory);
                    if (basalAreaRemovedByHarvest > 0.0F)
                    {
                        this.BasalAreaBeforeMostRecentHarvest = trajectory.DensityByPeriod[periodJustBeginning - 1].BasalAreaPerAcre;
                        this.BasalAreaRemovedByHarvest.Insert(0, basalAreaRemovedByHarvest);
                        this.TimeStepsSinceHarvest.Insert(0, 0);
                        
                        ++this.HarvestsPerformed;
                        basalAreaRemovedInPeriod += basalAreaRemovedByHarvest;
                    }
                }
            }
            return basalAreaRemovedInPeriod;
        }

        public bool IsTriggerInPeriod(int periodJustBeginning)
        {
            foreach (IHarvest harvest in this.Harvests)
            {
                if (harvest.Period == periodJustBeginning)
                {
                    return true;
                }
            }

            return false;
        }

        public void Reset()
        {
            this.BasalAreaBeforeMostRecentHarvest = -1.0F;
            this.BasalAreaRemovedByHarvest.Clear();
            this.FertilizationsPerformed = 0;
            // for now, no state to clear in this.Harvests
            this.HarvestsPerformed = 0;
            this.PoundsOfNitrogenPerAcre.Clear();
            this.TimeStepsSinceHarvest.Clear();
            this.TimeStepsSinceFertilization.Clear();
        }
    }
}