﻿using System.Collections.Generic;
using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Suffixed.Part;

namespace kOS.Suffixed
{
    public class AggregateResourceValue : Structure
    {
        private readonly string name;
        private readonly SharedObjects shared;
        private double amount;
        private double capacity;
        private readonly ListValue<PartValue> parts;
        private readonly float density;

        public AggregateResourceValue(PartResourceDefinition definition, SharedObjects shared)
        {
            name = definition.name;
            density = definition.density;
            this.shared = shared;
            amount = 0;
            capacity = 0;
            parts = new ListValue<PartValue>();
            InitializeAggregateResourceSuffixes();
        }

        private void InitializeAggregateResourceSuffixes()
        {
            AddSuffix("NAME", new Suffix<string>(() => name, "The name of the resource (eg LiguidFuel, ElectricCharge)"));
            AddSuffix("DENSITY", new Suffix<float>(() => density, "The density of the resource"));
            AddSuffix("AMOUNT", new Suffix<double>(() => amount, "The resources currently available"));
            AddSuffix("CAPACITY", new Suffix<double>(() => capacity, "The total storage capacity currently available"));
            AddSuffix("PARTS", new Suffix<ListValue<PartValue>>(() => parts, "The containers for this resource"));
        }

        public void AddResource(PartResource resource)
        {
            amount += resource.amount;
            capacity += resource.maxAmount;
            parts.Add(new PartValue(resource.part, shared));
        }

        public override string ToString()
        {
            return string.Format("SHIPRESOURCE({0},{1},{2})", name, amount, capacity);
        }

        private static Dictionary<string, AggregateResourceValue> ProspectResources(IEnumerable<global::Part> parts, SharedObjects shared)
        {
            var resources = new Dictionary<string, AggregateResourceValue>();
            foreach (var part in parts)
            {
                foreach (PartResource resource in part.Resources)
                {
                    AggregateResourceValue aggregateResourceAmount;
                    if (!resources.TryGetValue(resource.resourceName, out aggregateResourceAmount))
                    {
                        aggregateResourceAmount = new AggregateResourceValue(resource.info, shared);
                    }
                    aggregateResourceAmount.AddResource(resource);
                    resources[resource.resourceName] = aggregateResourceAmount;
                }
            }
            return resources;
        }

        public static ListValue PartsToList(IEnumerable<global::Part> parts, SharedObjects shared)
        {
            var list = new ListValue();
            var resources = ProspectResources(parts, shared);
            foreach (var resource in resources)
            {
                list.Add(resource.Value);
            }
            return list;
        }

        public static ListValue<AggregateResourceValue> FromVessel(Vessel vessel, SharedObjects shared)
        {
            var resources = ProspectResources(vessel.parts, shared);
            return ListValue<AggregateResourceValue>.CreateList(resources.Values);
        }
    }
}