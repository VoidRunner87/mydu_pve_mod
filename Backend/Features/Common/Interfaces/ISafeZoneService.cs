using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NQ;

namespace Mod.DynamicEncounters.Features.Common.Interfaces;

public interface ISafeZoneService
{
    public Task<IEnumerable<SafeZoneSphere>> GetSafeZones();
    
    public struct SafeZoneSphere
    {
        public Vec3 Position { get; set; }
        public double Radius { get; set; }

        public bool IsPointInside(Vec3 point)
        {
            // Compute the Euclidean distance between the point and the center of the sphere
            var distance = Math.Sqrt(Math.Pow(Position.x - point.x, 2)
                                     + Math.Pow(Position.y - point.y, 2)
                                     + Math.Pow(Position.z - point.z, 2));
            // The point is inside the sphere if the computed distance is less than the radius of the sphere
            return distance <= Radius;
        }
    }
}