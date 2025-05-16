using FullPortManagementSystem.Models;

namespace FullPortManagementSystem.Data
{
    public class DataSeeder
    {
        public static void Seed(PortDbContext context)
        {
            if (context.VesselEvents.Any()) return;

            var vessel_types = new[] { "Tanker", "RoRo", "Container", "Bulk" };
            var berth_type  = new[] { "Tanker", "RoRo", "Container", "Bulk" };
            var random = new Random();

            for (int i = 0; i < 100; i++)
            {
                var type = vessel_types[random.Next(vessel_types.Length)];
                var possibleSizes = new[] { 50, 75, 100, 150 };
                var size = possibleSizes[random.Next(possibleSizes.Length)];
                var eta = random.Next(0, 20);
                var duration = random.Next(4, 6);
                var departure = Math.Min(eta + duration, 23);

                // map berth_id based on vessel_type
                string berth_id = type switch
                {
                    "Tanker" => "A",
                    "Container" => "B",
                    "Bulk" => "C",
                    "RoRo" => "D",
                    _ => "X"
                };

                context.VesselEvents.Add(new VesselEvent
                {
                    vessel_type = type,
                    vessel_size = size,
                    eta_hour = eta,
                    planned_departure_hour = departure,
                    berth_id = berth_id,
                    berth_type = type,
                    weather_score = random.Next(0, 5)
                });
            }

            context.SaveChanges();
        }
    }
}
