/*
Copyright (c) 2013, robert.r.h.vella@gmail.com
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met: 

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer. 
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies, 
either expressed or implied, of the FreeBSD Project.
*/

﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithms
{
    public class Species<GType, PType>
    {
        private ISpeciatedGA parent;

        internal double PreviousScore { get; set; }

        internal bool CanBreed { get; set; }

        internal int TotalIterationsWithNoInnovation { get; set; }

        private double averageFitness;
        private bool averageFitnessCacheInvalidated;

        public double AverageFitness
        {
            get
            {
                if (averageFitnessCacheInvalidated)
                {
                    parent.UpdateGenomes();

                    averageFitness = members.Select(member => member.AdjustedScore).Average();
                    averageFitnessCacheInvalidated = false;
                }

                return averageFitness;
            }
        }

        public SpeciatedGenome<GType, PType> Best
        {
            get
            {
                return Members.First();
            }
        }

        private SpeciatedGenome<GType, PType> representative;

        private List<SpeciatedGenome<GType, PType>> members;
        private bool listCacheInvalidated;

        public IEnumerable<SpeciatedGenome<GType, PType>> Members
        {
            get
            {
                if (listCacheInvalidated)
                {
                    parent.UpdateGenomes();

                    members.Sort(new Comparison<SpeciatedGenome<GType, PType>>(
                                        (member1, member2) => member2.Score.CompareTo(member1.Score)));
                    listCacheInvalidated = false;
                }

                return members.AsReadOnly();
            }
        }

        public Species(ISpeciatedGA parent, SpeciatedGenome<GType, PType> representative)
        {
            this.parent = parent;
            this.representative = representative;

            this.members = new List<SpeciatedGenome<GType, PType>>() { representative };
            this.CanBreed = true;

            this.PreviousScore = Best.Score;

            Update();
        }

        public bool BelongsTo(SpeciatedGenome<GType, PType> genome)
        {
            return representative.CompatibilityDistance(genome) <= parent.CompatibilityDistanceThreshold;
        }

        public void Add(SpeciatedGenome<GType, PType> genome)
        {
            members.Add(genome);

            representative = genome;

            Update();
        }

        public void Update()
        {
            listCacheInvalidated = true;
            averageFitnessCacheInvalidated = true;
        }

        public void Remove(SpeciatedGenome<GType, PType> genome)
        {
            members.Remove(genome);
            Update();
        }

        public int Count
        {
            get
            {
                return members.Count;
            }
        }

        public void Clear()
        {
            members.Clear();
            Update();
        }
    }
}