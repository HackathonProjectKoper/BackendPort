using FullPortManagementSystem.Models;

namespace FullPortManagementSystem.Data
{
    public class DataSeeder
    {
        public static void Seed(PortDbContext context)
        {
            context.VesselEvents.RemoveRange(context.VesselEvents);
            context.SaveChanges();

            if (context.VesselEvents.Any()) return;

            var vesselTypes = new[] { "Tanker", "RoRo", "Container", "Bulk" };
            var berthMap = new Dictionary<string, string>
            {
                { "Tanker", "A" },
                { "Container", "B" },
                { "Bulk", "C" },
                { "RoRo", "D" }
            };

            var possibleSizes = new[] { 50, 75, 100, 150 };
            var possibleMinutes = new[] { 0, 15, 30, 45 };
            var containerSubtypes = new[] { "ShortExpiryDateFood", "LongExpiryDateFood", "Others" };

            var random = new Random();

            // Store berth schedules in total minutes
            var berthSchedules = new Dictionary<string, List<(int start, int end)>>()
            {
                { "A", new List<(int, int)>() },
                { "B", new List<(int, int)>() },
                { "C", new List<(int, int)>() },
                { "D", new List<(int, int)>() }
            };

            for (int i = 0; i < 100; i++)
            {
                var type = vesselTypes[random.Next(vesselTypes.Length)];
                var size = possibleSizes[random.Next(possibleSizes.Length)];
                var berthId = berthMap[type];
                var schedule = berthSchedules[berthId];

                string? containerSubtype = null;
                if (type.Equals("Container", StringComparison.OrdinalIgnoreCase))
                {
                    containerSubtype = containerSubtypes[random.Next(containerSubtypes.Length)];
                    Console.WriteLine($"[DEBUG] Assigned subtype: {containerSubtype} for Container {i}");
                }


                // Pick a random starting time in 15-minute intervals
                int etaHour = random.Next(0, 24); // hour from 0 to 23
                int etaMinute = possibleMinutes[random.Next(possibleMinutes.Length)];
                int etaTotalMinutes = etaHour * 60 + etaMinute;

                // Determine unload time in minutes
                int unloadMinutes = size switch
                {
                    50 => 60,
                    75 => 90,
                    100 => 120,
                    150 => 180,
                    _ => 60
                };

                // Make sure we don’t allow scheduling beyond the latest valid start time
                int latestStart = (23 * 60 + 45) - unloadMinutes;
                if (etaTotalMinutes > latestStart) continue; // skip vessel

                int actualStart = etaTotalMinutes;
                int actualEnd = actualStart + unloadMinutes;

                // Try to find next available slot (in 15-minute intervals)
                while (IsOverlapping(schedule, actualStart, actualEnd))
                {
                    actualStart += 15;
                    actualEnd = actualStart + unloadMinutes;

                    if (actualStart > latestStart)
                        goto SkipVessel; // can't fit today, skip
                }

                // Reserve time slot
                schedule.Add((actualStart, actualEnd));

                // Convert to TimeSpan
                TimeSpan actualEta = TimeSpan.FromMinutes(actualStart);
                TimeSpan plannedDeparture = TimeSpan.FromMinutes(actualEnd);

                int weatherScore = actualEta.Hours / 4; // 0-3 -> 0, 4-7 -> 1, 8-11 -> 2, 12-15 -> 3, 16-19 -> 4, 20-23 -> 5

                context.VesselEvents.Add(new VesselEvent
                {
                    vessel_type = type,
                    vessel_size = size,
                    eta_hour = actualEta,
                    planned_departure_hour = plannedDeparture,
                    berth_id = berthId,
                    berth_type = type,
                    weather_score = weatherScore,
                    container_subtype = containerSubtype
                });

            SkipVessel:;
            }

            context.SaveChanges();
        }

        private static bool IsOverlapping(List<(int start, int end)> schedule, int newStart, int newEnd)
        {
            foreach (var (start, end) in schedule)
            {
                if (newStart < end && newEnd > start)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
